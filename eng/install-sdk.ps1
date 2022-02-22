# Downloads the dotnet-install.ps1 script and executes it against the global.json file.

$installScriptUrl = "https://dot.net/v1/dotnet-install.ps1"
$installScript = "$PSScriptRoot\dotnet-install.ps1"

Write-Host "Downloading '$installScriptUrl'"
Invoke-WebRequest -Uri $installScriptUrl -OutFile $installScript -MaximumRetryCount 10 -RetryIntervalSec 3

$globalJsonFile = "$PSScriptRoot\..\global.json"
$dotnetInstallDir = "$PSScriptRoot\.dotnet"

. $installScript  -InstallDir $dotnetInstallDir -JSonFile $globalJsonFile
. $installScript  -InstallDir $dotnetInstallDir -Channel 5.0
. $installScript  -InstallDir $dotnetInstallDir -Channel 3.1
