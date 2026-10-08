param([string]$PackageFolder)

$ErrorActionPreference = 'Stop'
$workspace = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
if (-not $PackageFolder) {
  $PackageFolder = (Get-ChildItem -LiteralPath (Join-Path $workspace 'dist') -Directory |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1).FullName
}
$sqlFile = Join-Path $PackageFolder 'SQL\SqlDataBaseScriptV1_Customer.sql'
if (-not (Test-Path -LiteralPath $sqlFile)) { throw 'Customer SQL script was not found.' }
[xml]$config = Get-Content (Join-Path (Split-Path $PSScriptRoot -Parent) 'App.config')
$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($config.configuration.connectionStrings.add.connectionString)
$databaseName = 'InventoryTracking_CustomerQA_' + [guid]::NewGuid().ToString('N').Substring(0, 8)
$builder['Initial Catalog'] = 'master'
$admin = New-Object System.Data.SqlClient.SqlConnection($builder.ConnectionString)

function Scalar($query) {
  $command = $admin.CreateCommand()
  try { $command.CommandText = $query; $command.CommandTimeout = 60; return $command.ExecuteScalar() }
  finally { $command.Dispose() }
}

try {
  $admin.Open()
  $script = [IO.File]::ReadAllText($sqlFile).Replace('InventoryTracking', $databaseName)
  foreach ($batch in [regex]::Split($script, '(?im)^\s*GO\s*$')) {
    if (-not [string]::IsNullOrWhiteSpace($batch)) { $null = Scalar $batch }
  }
  $counts = Scalar "SELECT CONCAT((SELECT COUNT(*) FROM Urun), '|', (SELECT COUNT(*) FROM Stok), '|', (SELECT COUNT(*) FROM MalzemeKatalog), '|', (SELECT COUNT(*) FROM Depo), '|', (SELECT COUNT(*) FROM Birim), '|', (SELECT COUNT(*) FROM Kategori))"
  if ($counts -ne '0|0|0|1|4|1') { throw "Unexpected customer seed counts: $counts" }
  Write-Output 'PASS: Customer database has zero products, stock, and catalog rows; one depot, four units, one category.'
}
finally {
  if ($admin.State -ne 'Open') { $admin.Open() }
  $admin.ChangeDatabase('master')
  $null = Scalar "IF DB_ID('$databaseName') IS NOT NULL BEGIN ALTER DATABASE [$databaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$databaseName]; END"
  $admin.Dispose()
}
