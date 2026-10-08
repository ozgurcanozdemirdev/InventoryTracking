param(
  [string]$Server = '.',
  [string]$Database = 'InventoryTracking',
  [string]$BackupDirectory,
  [string]$RestoreDirectory
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data
$raw = $env:INVENTORYTRACKING_MAINTENANCE_CONNECTION_STRING
if ([string]::IsNullOrWhiteSpace($raw)) {
  $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
  $builder['Data Source'] = $Server
  $builder['Initial Catalog'] = $Database
  $builder['Integrated Security'] = $true
  $builder['TrustServerCertificate'] = $true
} else {
  $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($raw)
  if (-not $PSBoundParameters.ContainsKey('Database')) { $Database = $builder['Initial Catalog'] }
}
if (-not [regex]::IsMatch($Database, '^[A-Za-z][A-Za-z0-9_]{0,120}$',
    [Text.RegularExpressions.RegexOptions]::CultureInvariant)) {
  throw 'Database name must contain only letters, numbers, and underscore.'
}
$builder['Initial Catalog'] = 'master'
$connection = New-Object System.Data.SqlClient.SqlConnection($builder.ConnectionString)
$restoreName = 'InventoryTracking_RestoreQA_' + [guid]::NewGuid().ToString('N').Substring(0, 8)
$createdRestore = $false

function Scalar($sql, $disk = $null) {
  $command = $connection.CreateCommand()
  try {
    $command.CommandTimeout = 3600
    $command.CommandText = $sql
    if ($null -ne $disk) { $null = $command.Parameters.Add('@Disk', [System.Data.SqlDbType]::NVarChar, 4000); $command.Parameters['@Disk'].Value = $disk }
    return $command.ExecuteScalar()
  } finally { $command.Dispose() }
}

function Execute($sql, $disk = $null) {
  $command = $connection.CreateCommand()
  try {
    $command.CommandTimeout = 3600
    $command.CommandText = $sql
    if ($null -ne $disk) { $null = $command.Parameters.Add('@Disk', [System.Data.SqlDbType]::NVarChar, 4000); $command.Parameters['@Disk'].Value = $disk }
    $null = $command.ExecuteNonQuery()
  } finally { $command.Dispose() }
}

try {
  $connection.Open()
  if ($null -eq (Scalar "SELECT DB_ID(N'$Database')")) { throw 'Database not found.' }
  if (-not $BackupDirectory) { $BackupDirectory = Scalar "SELECT CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultBackupPath'))" }
  if (-not $RestoreDirectory) { $RestoreDirectory = Scalar "SELECT CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultDataPath'))" }
  if (-not $BackupDirectory -or -not $RestoreDirectory) { throw 'Specify BackupDirectory and RestoreDirectory paths visible to SQL Server.' }
  $backupPath = [IO.Path]::Combine($BackupDirectory, ($Database + '_' + (Get-Date -Format yyyyMMdd_HHmmss) + '_' + [guid]::NewGuid().ToString('N').Substring(0, 6) + '.bak'))
  Execute "BACKUP DATABASE [$Database] TO DISK = @Disk WITH INIT, CHECKSUM" $backupPath
  Execute 'RESTORE VERIFYONLY FROM DISK = @Disk WITH CHECKSUM' $backupPath

  $files = @()
  $command = $connection.CreateCommand()
  try {
    $command.CommandTimeout = 3600
    $command.CommandText = 'RESTORE FILELISTONLY FROM DISK = @Disk'
    $null = $command.Parameters.Add('@Disk', [System.Data.SqlDbType]::NVarChar, 4000)
    $command.Parameters['@Disk'].Value = $backupPath
    $reader = $command.ExecuteReader()
    try {
      while ($reader.Read()) {
        $files += [pscustomobject]@{ Logical = $reader['LogicalName'].ToString(); Type = $reader['Type'].ToString() }
      }
    } finally { $reader.Dispose() }
  } finally { $command.Dispose() }
  if ($files.Count -eq 0) { throw 'Backup contains no database files.' }
  $move = @()
  for ($i = 0; $i -lt $files.Count; $i++) {
    $name = $files[$i].Logical.Replace("'", "''")
    $extension = if ($files[$i].Type -eq 'L') { '.ldf' } else { '.mdf' }
    $path = [IO.Path]::Combine($RestoreDirectory, ($restoreName + '_' + $i + $extension)).Replace("'", "''")
    $move += "MOVE N'$name' TO N'$path'"
  }
  Execute "RESTORE DATABASE [$restoreName] FROM DISK = @Disk WITH $($move -join ', '), RECOVERY, CHECKSUM" $backupPath
  $createdRestore = $true
  Execute "DBCC CHECKDB ([$restoreName]) WITH NO_INFOMSGS, ALL_ERRORMSGS"
  $count = Scalar "SELECT COUNT(*) FROM [$restoreName].dbo.Urun"
  Write-Output "Backup verified by restore and DBCC CHECKDB. Products in restored copy: $count"
  Write-Output "Backup file on SQL Server: $backupPath"
}
finally {
  if ($connection.State -eq 'Open') {
    try {
      if ($createdRestore -or $null -ne (Scalar "SELECT DB_ID(N'$restoreName')")) {
        Execute "ALTER DATABASE [$restoreName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$restoreName]"
      }
    } finally { $connection.Dispose() }
  } else { $connection.Dispose() }
}
