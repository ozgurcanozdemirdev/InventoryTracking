param([string]$SqlServer='.',[ValidateSet('Debug','Release')][string]$Configuration='Debug',[string]$MSBuildPath,[string]$ScreenshotDirectory)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$project=Join-Path $root 'InventoryTracking'
if(-not $MSBuildPath) {
  $MSBuildPath=(Get-Command msbuild.exe -ErrorAction SilentlyContinue).Source
  if(-not $MSBuildPath) {
    $vswhere=Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if(Test-Path -LiteralPath $vswhere){$MSBuildPath=& $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe'|Select-Object -First 1}
  }
}
if(-not $MSBuildPath){throw 'Use Developer PowerShell or pass -MSBuildPath.'}
if(-not (Test-Path -LiteralPath (Join-Path $project 'App.config'))){Copy-Item -LiteralPath (Join-Path $project 'App.config.example') -Destination (Join-Path $project 'App.config')}
& $MSBuildPath (Join-Path $project 'InventoryTracking.csproj') /t:Build "/p:Configuration=$Configuration" /p:UseSharedCompilation=false /nr:false /nologo /v:minimal
if($LASTEXITCODE -ne 0){throw 'Build failed.'}
$compiler=Join-Path (Split-Path $MSBuildPath -Parent) 'Roslyn\csc.exe'
$target=Join-Path $project "bin\$Configuration\SalesReturnCheck.exe"
& $compiler /nologo /target:exe /r:System.dll /r:System.Core.dll /r:System.Data.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll ('/r:'+(Join-Path $project "bin\$Configuration\InventoryTracking.exe")) ('/out:'+$target) (Join-Path $PSScriptRoot 'SalesReturnCheck\SalesReturnCheck.cs')
if($LASTEXITCODE -ne 0){throw 'Test compilation failed.'}
$preferences=Join-Path ([IO.Path]::GetTempPath()) ('InventoryTracking_SalePreference_'+[guid]::NewGuid().ToString('N'))
$arguments=@($SqlServer,(Join-Path $project 'Notes\DatabaseSetup.sql'),(Join-Path $project 'Notes\2026-10-10_SalesReturnsAndAuditLog.sql'),$preferences)
if($ScreenshotDirectory){New-Item -ItemType Directory -Path $ScreenshotDirectory -Force|Out-Null;$arguments+=$ScreenshotDirectory}
try { & $target @arguments; if($LASTEXITCODE -ne 0){throw 'Sale/return checks failed.'} }
finally {
  $file=Join-Path $preferences 'language.txt'
  if(Test-Path -LiteralPath $file){Remove-Item -LiteralPath $file}
  if((Test-Path -LiteralPath $preferences) -and @(Get-ChildItem -LiteralPath $preferences -Force).Count -eq 0){Remove-Item -LiteralPath $preferences}
}
