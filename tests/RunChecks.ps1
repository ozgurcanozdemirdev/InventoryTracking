param(
  [string]$SqlServer,
  [string]$ScreenshotDirectory,
  [string]$MSBuildPath,
  [ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$project = Join-Path $root 'InventoryTracking'
if (-not $MSBuildPath) {
  $MSBuildPath = (Get-Command msbuild.exe -ErrorAction SilentlyContinue).Source
  if (-not $MSBuildPath) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path -LiteralPath $vswhere) {
      $MSBuildPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    }
  }
}
if (-not $MSBuildPath -or -not (Test-Path -LiteralPath $MSBuildPath)) {
  throw 'Run from Developer PowerShell for Visual Studio or pass -MSBuildPath.'
}

# İlk klonda yerel ayarlar yoktur; örnek yapılandırmada parola bulunmaz.
$config = Join-Path $project 'App.config'
if (-not (Test-Path -LiteralPath $config)) {
  Copy-Item -LiteralPath (Join-Path $project 'App.config.example') -Destination $config
}
& $MSBuildPath (Join-Path $PSScriptRoot 'LocalizationCheck\LocalizationCheck.csproj') /t:Build "/p:Configuration=$Configuration" /p:UseSharedCompilation=false /nr:false /nologo /v:minimal
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }

$testExe = Join-Path $PSScriptRoot "LocalizationCheck\bin\$Configuration\LocalizationCheck.exe"
$preferenceFolder = Join-Path ([IO.Path]::GetTempPath()) ('InventoryTrackingLocalization_' + [guid]::NewGuid().ToString('N'))
try {
  if ($SqlServer) {
    $testArgs = @($preferenceFolder, $SqlServer, (Join-Path $project 'Notes\DatabaseSetup.sql'))
    if ($ScreenshotDirectory) { $testArgs += $ScreenshotDirectory }
    & $testExe @testArgs
  }
  else { & $testExe $preferenceFolder }
  if ($LASTEXITCODE -ne 0) { throw 'Localization checks failed.' }
}
finally {
  # Yalnızca bu çalışmanın dil tercihi temizlenir; kullanıcının tercihi korunur.
  $preferenceFile = Join-Path $preferenceFolder 'language.txt'
  if (Test-Path -LiteralPath $preferenceFile) { Remove-Item -LiteralPath $preferenceFile }
  $blockedFolder = Join-Path $preferenceFolder 'blocked-language-path'
  if (Test-Path -LiteralPath $blockedFolder) { Remove-Item -LiteralPath $blockedFolder }
  if ((Test-Path -LiteralPath $preferenceFolder) -and
      @(Get-ChildItem -LiteralPath $preferenceFolder -Force).Count -eq 0) {
    Remove-Item -LiteralPath $preferenceFolder
  }
}
