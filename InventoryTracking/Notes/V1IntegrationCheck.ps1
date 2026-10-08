param([switch]$KeepDatabase, [string]$AssemblyPath, [string]$ReportScreenshotFolder)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data
$root = Split-Path $PSScriptRoot -Parent
[xml]$config = Get-Content (Join-Path $root 'App.config')
$source = $config.configuration.connectionStrings.add.connectionString
$base = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($source)
$databaseName = 'InventoryTracking_QA_' + [guid]::NewGuid().ToString('N').Substring(0, 8)
$base['Initial Catalog'] = 'master'
$admin = New-Object System.Data.SqlClient.SqlConnection($base.ConnectionString)
$previousAppConnection = $env:INVENTORYTRACKING_CONNECTION_STRING

function Run-Sql($connection, $query) {
  $command = $connection.CreateCommand()
  try {
    $command.CommandTimeout = 60
    $command.CommandText = $query
    return $command.ExecuteScalar()
  }
  finally { $command.Dispose() }
}

function Check($condition, $label) {
  if (-not $condition) { throw "FAILED: $label" }
  Write-Output "PASS: $label"
}

try {
  $admin.Open()
  $script = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'SqlDataBaseScriptV1.txt'))
  $script = $script.Replace('InventoryTracking', $databaseName)
  foreach ($batch in [regex]::Split($script, '(?im)^\s*GO\s*$')) {
    if (-not [string]::IsNullOrWhiteSpace($batch)) { $null = Run-Sql $admin $batch }
  }
  $null = Run-Sql $admin @'
ALTER TABLE dbo.Urun DROP CONSTRAINT FK_Urun_MalzemeBag;
ALTER TABLE dbo.Urun WITH CHECK ADD CONSTRAINT FK_Urun_MalzemeBag
  FOREIGN KEY (MalzemeId, Barkod, BirimId)
  REFERENCES dbo.MalzemeKatalog(MalzemeId, Barkod, BirimId);
'@
  $migration = [IO.File]::ReadAllText((Join-Path $PSScriptRoot '2026-09-26_V1_MalzemeBirimGuncelleme.sql'))
  $null = Run-Sql $admin $migration
  $null = Run-Sql $admin $migration
  Check ((Run-Sql $admin "SELECT update_referential_action_desc FROM sys.foreign_keys WHERE name='FK_Urun_MalzemeBag'") -eq 'CASCADE') 'idempotent schema migration'
  $null = Run-Sql $admin @'
ALTER TABLE dbo.StokHareketi DROP CONSTRAINT FK_StokHareketi_Transfer;
DROP INDEX UX_StokHareketi_TransferYonu ON dbo.StokHareketi;
ALTER TABLE dbo.StokHareketi DROP COLUMN TransferId;
DROP TABLE dbo.StokTransferi;
'@
  $transferMigration = [IO.File]::ReadAllText((Join-Path $PSScriptRoot '2026-09-26_V1_StokTransferi.sql'))
  $null = Run-Sql $admin $transferMigration
  $null = Run-Sql $admin $transferMigration
  Check ((Run-Sql $admin "SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID('StokHareketi') AND name='TransferId'") -eq 1) 'idempotent transfer migration'
  $permissions = [IO.File]::ReadAllText((Join-Path $PSScriptRoot '2026-09-26_V1_UygulamaYetkileri.sql'))
  $null = Run-Sql $admin $permissions
  Check ((Run-Sql $admin "SELECT COUNT(*) FROM sys.database_principals WHERE name='InventoryTrackingApp'") -eq 1) 'limited application role created'
  $base['Initial Catalog'] = $databaseName
  $env:INVENTORYTRACKING_CONNECTION_STRING = $base.ConnectionString
  if ([string]::IsNullOrWhiteSpace($AssemblyPath)) {
    $AssemblyPath = Join-Path $root 'bin\Debug\InventoryTracking.exe'
  }
  [void][Reflection.Assembly]::LoadFrom($AssemblyPath)
  Check ($null -eq [InventoryTracking.Connection.ConnectionSettingsStore]::Test($base.ConnectionString)) 'customer connection and schema test'
  $savedPath = Join-Path $env:TEMP ('InventoryTrackingConnectionQA_' + [guid]::NewGuid().ToString('N') + '.dat')
  try {
    [InventoryTracking.Connection.ConnectionSettingsStore]::Save($base.ConnectionString, $savedPath)
    Check ([InventoryTracking.Connection.ConnectionSettingsStore]::Load($savedPath) -eq $base.ConnectionString) 'encrypted customer connection roundtrip'
    $rawSaved = [Text.Encoding]::UTF8.GetString([IO.File]::ReadAllBytes($savedPath))
    Check (-not $rawSaved.Contains($databaseName)) 'customer connection is not stored in plaintext'
  } finally { if (Test-Path $savedPath) { Remove-Item -LiteralPath $savedPath } }

  $category = New-Object InventoryTracking.Business.Services+KategoriService
  Check ($category.AddKategori('QA Category', '').Success) 'category add'
  Check (-not $category.AddKategori('qa category', '').Success) 'category case-insensitive duplicate'
  $categoryId = ($category.GetAllKategoriler() | Where-Object KategoriAdi -eq 'QA Category').KategoriId
  Check ($category.UpdateKategori($categoryId, 'QA Category Updated', 'Test').Success) 'category update'
  Check ($category.DeleteKategori($categoryId).Success) 'unused category delete'
  Check (-not $category.DeleteKategori(1).Success) 'used category protected'

  $depot = New-Object InventoryTracking.Business.Services+DepoService
  Check ($depot.AddDepo('QA Depot', 'City', '').Success) 'depot add'
  Check (-not $depot.AddDepo('qa depot', 'City', '').Success) 'depot case-insensitive duplicate'
  $depotId = ($depot.GetAllDepolar() | Where-Object DepoAdi -eq 'QA Depot').DepoId
  Check ($depot.UpdateDepo($depotId, 'QA Depot Updated', 'City', 'Test').Success) 'depot update'
  Check (-not $depot.SetDepoActive(1, $false).Success) 'depot with stock protected'
  Check ($depot.SetDepoActive($depotId, $false).Success) 'empty depot deactivation'
  Check ($depot.SetDepoActive($depotId, $true).Success) 'depot reactivation'

  $material = New-Object InventoryTracking.Business.Services+MalzemeService
  $item = $material.GetByBarkod('8690000123456')
  Check ($null -ne $item) 'material lookup'
  $item.Aciklama = 'QA Updated Product'
  $item.BirimId = 2
  Check ($material.Update($item).Success) 'material name and unit update'
  $product = New-Object InventoryTracking.Business.Services+UrunService
  $found = $product.SearchByBarkod('8690000123456')
  Check (($found.UrunAdi -eq 'QA Updated Product') -and ($found.BirimId -eq 2)) 'product follows material update'
  Check ($product.DeleteProduct($found.UrunId).Success) 'product soft delete'
  Check ($null -eq $product.SearchByBarkod('8690000123456')) 'soft deleted product hidden'
  Check ($null -ne $product.SearchInactiveByBarkod('8690000123456')) 'inactive product lookup'
  Check ($product.ReactivateProduct($found.UrunId).Success) 'product reactivation'
  Check ($null -ne $product.SearchByBarkod('8690000123456')) 'reactivated product visible'

  $newMaterial = New-Object 'InventoryTracking.Models.Entities+MalzemeKatalog'
  $newMaterial.MalzemeNo = 'QA-MAT-1'
  $newMaterial.Barkod = 'QA-BAR-1'
  $newMaterial.Aciklama = 'QA New Product'
  $newMaterial.BirimId = 1
  Check ($material.Add($newMaterial).Success) 'new material registration'
  $newProduct = New-Object 'InventoryTracking.Models.Entities+UrunViewModel'
  $newProduct.Barkod = 'QA-BAR-1'
  $newProduct.UrunAdi = 'QA New Product'
  $newProduct.BirimId = 1
  $newProduct.KategoriId = 1
  $newProduct.DepoId = $depotId
  $newProduct.Miktar = 5
  $newProduct.MinimumStok = 1
  $newProduct.MaksimumStok = 20
  $newProduct.Fiyat = 12
  Check ($product.AddProduct($newProduct).Success) 'product and initial stock registration'
  $newMaterial2 = New-Object 'InventoryTracking.Models.Entities+MalzemeKatalog'
  $newMaterial2.MalzemeNo = 'QA-MAT-2'
  $newMaterial2.Barkod = 'QA-BAR-2'
  $newMaterial2.Aciklama = 'qa new product'
  $newMaterial2.BirimId = 1
  Check ($material.Add($newMaterial2).Success) 'second material registration'
  $newProduct.Barkod = 'QA-BAR-2'
  $newProduct.UrunAdi = 'qa new product'
  Check (-not $product.AddProduct($newProduct).Success) 'product name case-insensitive duplicate'
  $newProduct.Barkod = 'QA-MISSING'
  Check (-not $product.AddProduct($newProduct).Success) 'missing material blocks product'

  $stock = New-Object InventoryTracking.Business.Services+StokService
  $newId = $product.SearchByBarkod('QA-BAR-1').UrunId
  Check ($stock.UpdateStok($newId, $depotId, -2, 'Exit', 'QA').Success) 'stock exit'
  Check (-not $stock.UpdateStok($newId, $depotId, -100, 'Exit', 'QA').Success) 'negative stock blocked'
  $newProduct.Barkod = 'QA-BAR-1'
  $newProduct.UrunAdi = 'QA New Product'
  $newProduct.UrunId = $newId
  $newProduct.Miktar = 7
  $newProduct.AktifMi = $true
  Check (-not $product.UpdateProduct($newProduct).Success) 'stock correction requires reason'
  Check ($product.UpdateProduct($newProduct, 'QA inventory count').Success) 'existing stock update'
  Check (-not $stock.TransferStock($newId, $depotId, $depotId, 1, 'QA transfer').Success) 'same depot transfer blocked'
  Check (-not $stock.TransferStock($newId, $depotId, 1, 100, 'QA transfer').Success) 'insufficient transfer rolled back'
  Check ((Run-Sql $admin "SELECT COUNT(*) FROM Depo WHERE DepoId IN ($depotId,1) AND AktifMi=1") -eq 2) 'both transfer depots active'
  Check ((Run-Sql $admin "SELECT COUNT(*) FROM Urun WHERE UrunId=$newId AND AktifMi=1") -eq 1) 'transfer product active'
  $transferResult = $stock.TransferStock($newId, $depotId, 1, 3, 'QA transfer')
  Check ($transferResult.Success) "two-depot transfer: $($transferResult.Message)"

  $qa = New-Object System.Data.SqlClient.SqlConnection($base.ConnectionString)
  $qa.Open()
  try {
    $count = Run-Sql $qa 'SELECT COUNT(*) FROM StokHareketi WHERE CikisMi NOT IN (0,1)'
    Check ($count -eq 0) 'movement direction bit valid'
    $count = Run-Sql $qa "SELECT COUNT(*) FROM StokHareketi WHERE UrunId=$newId AND CikisMi=0"
    Check ($count -eq 3) 'stock entries recorded as zero'
    $count = Run-Sql $qa "SELECT COUNT(*) FROM StokHareketi WHERE UrunId=$newId AND CikisMi=1"
    Check ($count -eq 2) 'stock exits recorded as one'
    $count = Run-Sql $qa "SELECT Miktar FROM Stok WHERE UrunId=$newId AND DepoId=$depotId"
    Check ($count -eq 4) 'source stock decreased after transfer'
    $count = Run-Sql $qa "SELECT Miktar FROM Stok WHERE UrunId=$newId AND DepoId=1"
    Check ($count -eq 3) 'target stock increased after transfer'
    $count = Run-Sql $qa "SELECT COUNT(*) FROM StokTransferi WHERE UrunId=$newId"
    Check ($count -eq 1) 'failed transfer left no transfer record'
    $count = Run-Sql $qa "SELECT COUNT(*) FROM StokHareketi WHERE TransferId IS NOT NULL AND UrunId=$newId"
    Check ($count -eq 2) 'transfer has exactly two linked movements'
    $count = Run-Sql $qa "SELECT COUNT(*) FROM StokHareketi WHERE UrunId=$newId AND Aciklama='QA inventory count'"
    Check ($count -eq 1) 'stock correction reason persisted'
    $report = New-Object InventoryTracking.Business.Services+ZReportService
    $periods = @(
      ('G' + [char]0x00FC + 'nl' + [char]0x00FC + 'k'),
      ('Ayl' + [char]0x0131 + 'k'),
      ('Y' + [char]0x0131 + 'll' + [char]0x0131 + 'k')
    )
    $filter = New-Object 'InventoryTracking.Models.Entities+HareketRaporFiltresi'
    $filter.Baslangic = (Get-Date).Date
    $filter.Bitis = (Get-Date).Date
    $filter.UrunArama = 'QA-BAR-1'
    foreach ($period in $periods) {
      $rows = @($report.GetHareketOzeti($filter, $period))
      Check ($rows.Count -gt 0) "movement report loads: $period"
    }
    $summary = @($report.GetHareketOzeti($filter, $periods[0]))
    Check (($summary.Count -eq 1) -and ($summary[0].NormalGiris -eq 5) -and
      ($summary[0].NormalCikis -eq 2) -and ($summary[0].DuzeltmeGiris -eq 4) -and
      ($summary[0].TransferGiris -eq 3) -and ($summary[0].TransferCikis -eq 3) -and
      ($summary[0].NetDegisim -eq 7) -and ($summary[0].HareketSayisi -eq 5)) 'report separates normal, correction and transfer totals'
    $details = @($report.GetHareketDetayi($filter))
    Check (($details.Count -eq 5) -and
      (($details | Where-Object { $_.TransferId -ne $null -and $_.Aciklama -eq 'QA transfer' }).Count -eq 2)) 'report detail links transfer description'
    $filter.DepoId = $depotId
    $depotSummary = @($report.GetHareketOzeti($filter, $periods[0]))
    Check (($depotSummary[0].TransferGiris -eq 0) -and ($depotSummary[0].TransferCikis -eq 3) -and
      ($depotSummary[0].NetDegisim -eq 4)) 'report depot filter keeps transfer direction'
    $filter.DepoId = $null
    $filter.HareketGrubu = 2
    Check (@($report.GetHareketDetayi($filter)).Count -eq 2) 'report transfer-type filter'
    $filter.HareketGrubu = 3
    $countSummary = @($report.GetHareketOzeti($filter, $periods[0]))
    Check (($countSummary[0].DuzeltmeGiris -eq 4) -and ($countSummary[0].NormalGiris -eq 0) -and
      (@($report.GetHareketDetayi($filter)).Count -eq 1)) 'report correction-type filter'
    $filter.HareketGrubu = 1
    Check (@($report.GetHareketDetayi($filter)).Count -eq 2) 'report normal movement filter'
    $filter.HareketGrubu = 0
    $filter.CikisMi = $true
    Check (@($report.GetHareketDetayi($filter)).Count -eq 2) 'report exit-direction filter'
    $filter.CikisMi = $null
    $filter.UrunArama = 'QA-NOT-FOUND'
    Check (@($report.GetHareketDetayi($filter)).Count -eq 0) 'report product filter excludes unrelated movements'
  }
  finally { $qa.Dispose() }

  Check ($product.DeleteProduct($newId).Success) 'product can be deactivated after movement'
  Check (-not $stock.UpdateStok($newId, $depotId, 1, 'Giriş', 'QA inactive product').Success) 'inactive product blocks stock movement'
  Check ($product.ReactivateProduct($newId).Success) 'product reactivated after inactive stock check'
  Check ($depot.AddDepo('QA Closed Depot', 'City', '').Success) 'empty depot created for inactive stock check'
  $closedDepotId = ($depot.GetAllDepolar() | Where-Object DepoAdi -eq 'QA Closed Depot').DepoId
  Check ($depot.SetDepoActive($closedDepotId, $false).Success) 'empty depot deactivated for stock check'
  Check (-not $stock.UpdateStok($newId, $closedDepotId, 1, 'Giriş', 'QA inactive depot').Success) 'inactive depot blocks stock movement'

  Add-Type -AssemblyName System.Windows.Forms
  foreach ($formName in @('KategoriYonetimForm', 'DepoYonetimForm')) {
    $form = New-Object "InventoryTracking.UI.$formName"
    try {
      $form.Show()
      [System.Windows.Forms.Application]::DoEvents()
      $grid = $form.Controls | Where-Object { $_ -is [System.Windows.Forms.DataGridView] } | Select-Object -First 1
      Check (($null -ne $grid) -and ($grid.Rows.Count -gt 0)) "$formName opens and loads"
    }
    finally { $form.Close(); $form.Dispose() }
  }
  $editMaterial = New-Object InventoryTracking.UI.MalzemeKayitForm -ArgumentList (,$item)
  try {
    $editMaterial.Show()
    [System.Windows.Forms.Application]::DoEvents()
    Check ($editMaterial.Text -like 'Malzeme D*zenle') 'material edit screen opens'
  }
  finally { $editMaterial.Close(); $editMaterial.Dispose() }
  $editProduct = New-Object InventoryTracking.UI.AddEditProductForm -ArgumentList $found, 1
  try {
    $editProduct.Show()
    [System.Windows.Forms.Application]::DoEvents()
    $editButton = $editProduct.Controls.Find('pnlButtons', $true) | Select-Object -First 1
    Check (($null -ne $editButton) -and
      ($null -ne ($editButton.Controls | Where-Object Text -like 'Malzeme D*zenle' | Select-Object -First 1))) 'product edit screen links material editor'
  }
  finally { $editProduct.Close(); $editProduct.Dispose() }
  $newProductForm = New-Object InventoryTracking.UI.AddEditProductForm -ArgumentList $depotId
  try {
    $newProductForm.GelenBarkod = 'QA-BAR-2'
    $newProductForm.Show()
    [System.Windows.Forms.Application]::DoEvents()
    $productName = $newProductForm.Controls.Find('txtUrunAdi', $true) | Select-Object -First 1
    Check (($newProductForm.Text -like 'Yeni *') -and ($productName.Text -eq 'qa new product')) 'new product screen loads scanned catalog item'
  }
  finally { $newProductForm.Close(); $newProductForm.Dispose() }
  $transferForm = New-Object InventoryTracking.UI.StokTransferForm -ArgumentList 1, $newId
  try {
    $transferForm.Show()
    [System.Windows.Forms.Application]::DoEvents()
    Check ($transferForm.Visible) 'transfer screen opens'
  }
  finally { $transferForm.Close(); $transferForm.Dispose() }
  $reportForm = New-Object InventoryTracking.UI.ZReportForm
  try {
    $reportForm.Show(); [System.Windows.Forms.Application]::DoEvents()
    Check ($reportForm.Visible) 'movement report screen opens'
    $getButton = $reportForm.Controls.Find('btnGetir', $true) | Select-Object -First 1
    $getButton.PerformClick(); [System.Windows.Forms.Application]::DoEvents()
    $summaryGrid = $reportForm.Controls.Find('dgvRapor', $true) | Select-Object -First 1
    $detailGrid = $reportForm.Controls.Find('dgvHareketDetay', $true) | Select-Object -First 1
    Check (($summaryGrid.Rows.Count -gt 0) -and ($detailGrid.Rows.Count -gt 0)) 'report button loads summary and detail tabs'
    $tabs = $reportForm.Controls.Find('tabHareketRaporu', $true) | Select-Object -First 1
    if ($ReportScreenshotFolder) {
      [IO.Directory]::CreateDirectory($ReportScreenshotFolder) | Out-Null
      foreach ($tabIndex in @(0, 1)) {
        $tabs.SelectedIndex = $tabIndex
        [System.Windows.Forms.Application]::DoEvents()
        $shot = New-Object Drawing.Bitmap -ArgumentList $reportForm.Width, $reportForm.Height
        try {
          $reportForm.DrawToBitmap($shot, (New-Object Drawing.Rectangle -ArgumentList 0, 0, $reportForm.Width, $reportForm.Height))
          $shot.Save((Join-Path $ReportScreenshotFolder ('report-tab-' + $tabIndex + '.png')))
        }
        finally { $shot.Dispose() }
      }
    }
    $depotCombo = $reportForm.Controls.Find('cmbRaporDepo', $true) | Select-Object -First 1
    for ($i = 0; $i -lt $depotCombo.Items.Count; $i++) {
      if ($depotCombo.Items[$i].Id -eq $depotId) { $depotCombo.SelectedIndex = $i; break }
    }
    ($reportForm.Controls.Find('txtRaporUrun', $true) | Select-Object -First 1).Text = 'QA-BAR-1'
    ($reportForm.Controls.Find('cmbRaporHareket', $true) | Select-Object -First 1).SelectedIndex = 2
    ($reportForm.Controls.Find('cmbRaporYon', $true) | Select-Object -First 1).SelectedIndex = 2
    $getButton.PerformClick(); [System.Windows.Forms.Application]::DoEvents()
    Check (($detailGrid.Rows.Count -eq 1) -and ($detailGrid.Rows[0].Cells['DegisenMiktar'].Value -eq -3)) 'report screen applies depot, product, transfer and direction filters'
    $writeCsv = $reportForm.GetType().GetMethod('WriteCsv', [Reflection.BindingFlags]'Static,NonPublic')
    $csvPath = Join-Path $env:TEMP ('InventoryTrackingReportQA_' + [guid]::NewGuid().ToString('N') + '.csv')
    try {
      $null = $writeCsv.Invoke($null, [object[]]@([string]$csvPath, [System.Windows.Forms.DataGridView]$detailGrid))
      $csv = [IO.File]::ReadAllText($csvPath, [Text.Encoding]::UTF8)
      Check ($csv.Contains('QA transfer') -and $csv.Contains('Transfer No')) 'filtered detail CSV contains movement fields'
    }
    finally { if (Test-Path $csvPath) { Remove-Item -LiteralPath $csvPath } }
    $csvValue = $reportForm.GetType().GetMethod('CsvValue', [Reflection.BindingFlags]'Static,NonPublic')
    Check ($csvValue.Invoke($null, @('=1+1')).StartsWith('"''')) 'CSV text formula is escaped'
    $printPage = $reportForm.GetType().GetMethod('Pd_PrintPage', [Reflection.BindingFlags]'Instance,NonPublic')
    $bitmap = New-Object System.Drawing.Bitmap -ArgumentList 1200, 900
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    try {
      $bounds = New-Object Drawing.Rectangle -ArgumentList 50, 50, 1100, 800
      $pageSettings = New-Object Drawing.Printing.PageSettings
      foreach ($tabIndex in @(0, 1)) {
        $tabs.SelectedIndex = $tabIndex
        $pageArgs = New-Object Drawing.Printing.PrintPageEventArgs -ArgumentList $graphics, $bounds, $bounds, $pageSettings
        $null = $printPage.Invoke($reportForm, [object[]]@($null, [System.Drawing.Printing.PrintPageEventArgs]$pageArgs))
      }
      Check $true 'summary and detail PDF pages render without printer'
    }
    finally { $graphics.Dispose(); $bitmap.Dispose() }
  }
  finally { $reportForm.Close(); $reportForm.Dispose() }
  $reasonForm = New-Object InventoryTracking.UI.StokDuzeltmeNedeniForm
  try { $reasonForm.Show(); [System.Windows.Forms.Application]::DoEvents(); Check ($reasonForm.Visible) 'correction reason screen opens' }
  finally { $reasonForm.Close(); $reasonForm.Dispose() }
  $setupForm = New-Object InventoryTracking.UI.ConnectionSetupForm
  try { $setupForm.Show(); [System.Windows.Forms.Application]::DoEvents(); Check ($setupForm.Visible) 'customer connection screen opens' }
  finally { $setupForm.Close(); $setupForm.Dispose() }
  $main = New-Object InventoryTracking.UI.MainForm
  try {
    $initialize = $main.GetType().GetMethod('MainForm_Load',
      [Reflection.BindingFlags]'Instance,NonPublic')
    $null = $initialize.Invoke($main, @($null, [EventArgs]::Empty))
    $mainGrid = $main.Controls.Find('dgvStoklar', $true) | Select-Object -First 1
    $mainDepots = $main.Controls.Find('cmbDepo', $true) | Select-Object -First 1
    Check (($mainGrid.Rows.Count -gt 0) -and ($mainDepots.Items.Count -gt 0)) 'main screen loads depot and stock rows'
    $toolbar = $main.Controls.Find('pnlButtons', $true) | Select-Object -First 1
    $categoryButton = $toolbar.Controls | Where-Object { $_.Text -like '*Kategoriler' } | Select-Object -First 1
    $depotButton = $toolbar.Controls | Where-Object { $_.Text -like '*Depolar' } | Select-Object -First 1
    $transferButton = $toolbar.Controls | Where-Object { $_.Text -like '*Transfer' } | Select-Object -First 1
    $oldAdd = $toolbar.Controls.Find('btnYeniUrun', $false) | Select-Object -First 1
    $oldExit = $toolbar.Controls.Find('btnKapat', $false) | Select-Object -First 1
    Check (($categoryButton.Left -eq 661) -and ($depotButton.Left -eq 791) -and ($transferButton.Left -eq 921) -and
      ($oldAdd.Left -eq 15) -and ($oldExit.Left -eq 1065)) 'main toolbar button positions preserved'
  }
  finally { $main.Dispose() }
}
finally {
  $env:INVENTORYTRACKING_CONNECTION_STRING = $previousAppConnection
  if ('InventoryTracking.Connection.SqlDBConnection' -as [type]) {
    [InventoryTracking.Connection.SqlDBConnection]::CloseConnection()
  }
  [System.Data.SqlClient.SqlConnection]::ClearAllPools()
  if ($admin.State -ne 'Open') { $admin.Open() }
  $admin.ChangeDatabase('master')
  if ($KeepDatabase) { Write-Output "Kept QA database: $databaseName" }
  else {
    $null = Run-Sql $admin "IF DB_ID('$databaseName') IS NOT NULL BEGIN ALTER DATABASE [$databaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$databaseName]; END"
  }
  $admin.Dispose()
}
