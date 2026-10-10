param(
  [string]$SqlServer='.',
  [ValidateSet('Debug','Release')][string]$Configuration='Debug',
  [string]$MSBuildPath,
  [string]$OutputDirectory
)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$project=Join-Path $root 'InventoryTracking'
if(-not $MSBuildPath) {
  $MSBuildPath=(Get-Command msbuild.exe -ErrorAction SilentlyContinue).Source
  if(-not $MSBuildPath) {
    $vswhere=Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if(Test-Path -LiteralPath $vswhere) {$MSBuildPath=& $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe'|Select-Object -First 1}
  }
}
if(-not $MSBuildPath){throw 'Use Developer PowerShell or pass -MSBuildPath.'}
if(-not (Test-Path -LiteralPath (Join-Path $project 'App.config'))) {
  Copy-Item -LiteralPath (Join-Path $project 'App.config.example') -Destination (Join-Path $project 'App.config')
}
& $MSBuildPath (Join-Path $project 'InventoryTracking.csproj') /t:Build "/p:Configuration=$Configuration" /p:UseSharedCompilation=false /nr:false /nologo /v:minimal
if($LASTEXITCODE -ne 0){throw 'Build failed.'}
$compiler=Join-Path (Split-Path $MSBuildPath -Parent) 'Roslyn\csc.exe'
$target=Join-Path $project "bin\$Configuration\FinancialReportCheck.exe"
& $compiler /nologo /target:exe /r:System.dll /r:System.Core.dll /r:System.Data.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Xml.dll /r:System.Xml.Linq.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll ('/r:'+(Join-Path $project "bin\$Configuration\InventoryTracking.exe")) ('/out:'+$target) (Join-Path $PSScriptRoot 'FinancialReportCheck\FinancialReportCheck.cs')
if($LASTEXITCODE -ne 0){throw 'Test compilation failed.'}
if(-not $OutputDirectory) {$OutputDirectory=Join-Path $root ('dist\financial-checks-'+[guid]::NewGuid().ToString('N'))}
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
# Test, yalnızca benzersiz bir geçici veritabanında kayıt oluşturur.
& $target $SqlServer (Join-Path $project 'Notes\DatabaseSetup.sql') $OutputDirectory
if($LASTEXITCODE -ne 0){throw 'Financial report checks failed.'}
Write-Output ('Test exports: '+$OutputDirectory)
