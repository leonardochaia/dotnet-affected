#!/bin/bash
set -e

# Change version in dotnet-affected.csproj first
# Run from repo root, i.e ./eng/deploy-packages.sh -k <NUGET_API_KEY>

source $(dirname "$0")/activate.sh

PATH_TO_SOURCE=$(dirname "$0")/../src/dotnet-affected

CI_BUILD=true
dotnet pack $PATH_TO_SOURCE/dotnet-affected.csproj

dotnet nuget push $PATH_TO_SOURCE/bin/Debug/*.nupkg -s nuget.org "${@:1}"
