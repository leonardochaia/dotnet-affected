# Change version in dotnet-affected.csproj first
# Change version in README.md
# Run from repo root, i.e ./eng/deploy-packages.sh -k <NUGET_API_KEY>

$activateScript = "$PSScriptRoot\activate.ps1"
. $activateScript

$env:PATH_TO_SOURCE = "$PSScriptRoot\..\src\dotnet-affected"

# Clean so that we don't try to push old packages!
Remove-Item -Path "$env:PATH_TO_SOURCE\bin" -Recurse -Force -ErrorAction SilentlyContinue

$env:CI_BUILD = "true"
dotnet pack "$env:PATH_TO_SOURCE\dotnet-affected.csproj"

Write-Host "Pushing. This takes A LONG TIME"
dotnet nuget push "$env:PATH_TO_SOURCE\bin\Debug\*.nupkg" -s nuget.org -t 3600 $args

Write-Host "Finish. Remember to deploy docker image!"
