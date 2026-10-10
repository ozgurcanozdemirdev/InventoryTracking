param([switch]$SkipBuild, [string]$OutputRoot)

$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
$workspace = Split-Path $project -Parent
if (-not $SkipBuild) {
  $msbuild = (Get-Command msbuild.exe -ErrorAction SilentlyContinue).Source
  if (-not $msbuild) {
    $msbuild = 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe'
  }
  if (-not (Test-Path -LiteralPath $msbuild)) { throw 'MSBuild was not found. Build Release in Visual Studio first and run with -SkipBuild.' }
  & $msbuild (Join-Path $project 'InventoryTracking.csproj') /t:Build /p:Configuration=Release /p:UseSharedCompilation=false /nologo /v:minimal
  if ($LASTEXITCODE -ne 0) { throw 'Release build failed.' }
}

$source = Join-Path $project 'bin\Release'
$exe = Join-Path $source 'InventoryTracking.exe'
$config = Join-Path $source 'InventoryTracking.exe.config'
if (-not (Test-Path -LiteralPath $exe) -or -not (Test-Path -LiteralPath $config)) { throw 'Release files are missing.' }
[xml]$xml = Get-Content -LiteralPath $config
$connectionString = $xml.configuration.connectionStrings.add.connectionString
$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($connectionString)
if (-not [string]::IsNullOrWhiteSpace($builder.Password) -or -not [string]::IsNullOrWhiteSpace($builder.UserID)) {
  throw 'Release configuration contains SQL credentials; package was not created.'
}
$demoMarker = '-- 14. DEMO VERISI EKLE - KATEGORILER'
$fullSchema = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'DatabaseSetup.sql'))
$demoStart = $fullSchema.IndexOf($demoMarker, [StringComparison]::Ordinal)
if ($demoStart -lt 0) { throw 'Demo-data boundary was not found in the SQL schema; package was not created.' }
$customerSchema = $fullSchema.Substring(0, $demoStart) + @'
-- Müşteri başlangıç kayıtları: örnek ürün ve stok içermez.
INSERT INTO Kategori (KategoriAdi, Aciklama) VALUES (N'Genel', N'Genel kategori');
INSERT INTO Birim (BirimAdi, Kisaltma) VALUES (N'Adet', N'Ad');
INSERT INTO Birim (BirimAdi, Kisaltma) VALUES (N'Kilogram', N'Kg');
INSERT INTO Birim (BirimAdi, Kisaltma) VALUES (N'Litre', N'Lt');
INSERT INTO Birim (BirimAdi, Kisaltma) VALUES (N'Paket', N'Pk');
INSERT INTO Depo (DepoAdi, Lokasyon, Aciklama, AktifMi)
VALUES (N'Ana Depo', N'Merkez', N'Başlangıç deposu', 1);
PRINT 'Müşteri veritabanı kuruldu; ürün ve stok kayıtları boş.';
GO
'@

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
  $OutputRoot = Join-Path $workspace 'dist'
}
$version = [Reflection.AssemblyName]::GetAssemblyName($exe).Version.ToString(3)
$satellite = Join-Path $source 'en\InventoryTracking.resources.dll'
if (-not (Test-Path -LiteralPath $satellite)) { throw 'English resources are missing. Rebuild Release before packaging.' }
$package = Join-Path $OutputRoot ('InventoryTracking-v' + $version + '-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
$sqlFolder = Join-Path $package 'SQL'
New-Item -ItemType Directory -Path $sqlFolder -Force | Out-Null
Copy-Item -LiteralPath $exe, $config -Destination $package
# İngilizce kaynak DLL'i uygulamanın yanındaki en klasöründe bulunmalıdır.
Copy-Item -LiteralPath (Join-Path $source 'en') -Destination $package -Recurse
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CustomerInstallation.md') -Destination $package
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'BackupAndVerify.ps1') -Destination $package
foreach ($name in @(
  '2026-09-26_UniqueProductName.sql',
  '2026-09-26_MaterialCatalogAndMovementDirection.sql',
  '2026-09-26_V1_MaterialUnitUpdate.sql',
  '2026-09-26_V1_StockTransfer.sql',
  '2026-10-06_MaterialNumberSimplification.sql',
  '2026-10-10_ProductCodesAndCategoryNames.sql',
  '2026-10-10_SalesReturnsAndAuditLog.sql',
  'DemoData.sql',
  '2026-09-26_V1_ApplicationPermissions.sql')) {
  Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $sqlFolder
}
[IO.File]::WriteAllText((Join-Path $sqlFolder 'DatabaseSetup.Customer.sql'),
  $customerSchema, (New-Object Text.UTF8Encoding($true)))
$zip = $package + '.zip'
Compress-Archive -LiteralPath $package -DestinationPath $zip
Write-Output "Customer package: $zip"
