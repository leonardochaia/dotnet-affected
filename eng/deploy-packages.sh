#!/bin/bash
set -e

# Change version in dotnet-affected.csproj first
# Change version in README.md
# Run from repo root, i.e ./eng/deploy-packages.sh -k <NUGET_API_KEY>

source $(dirname "$0")/activate.sh

PATH_TO_SOURCE=$(dirname "$0")/../src/dotnet-affected

# Clean so that we don't try to push old packages!
rm -rf $PATH_TO_SOURCE/bin/

CI_BUILD=true
dotnet pack $PATH_TO_SOURCE/dotnet-affected.csproj

echo "Pushing. This takes A LONG TIME"
dotnet nuget push $PATH_TO_SOURCE/bin/Debug/*.nupkg -s nuget.org -t 3600 "${@:1}"

echo "Finish. Remember to deploy docker image!"
