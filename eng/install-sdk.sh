#!/bin/bash

# Downloads the dotnet-install.sh script and executes it against the global.json file.

install_script_url="https://dot.net/v1/dotnet-install.sh"
install_script="$(dirname "$0")/dotnet-install.sh"

echo "Downloading '$install_script_url'"
curl "$install_script_url" -sSL --retry 10 --create-dirs -o "$install_script"

chmod +x "$install_script"

global_json_file="$(dirname "$0")/../global.json"
dotnet_install_dir="$(dirname "$0")/.dotnet"

"$install_script" --install-dir "$dotnet_install_dir" --jsonfile "$global_json_file"
"$install_script" --install-dir "$dotnet_install_dir" --channel 3.1
