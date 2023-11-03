#!/bin/bash

# Check if the correct number of arguments are provided
if [ "$#" -ne 1 ] && [ "$#" -ne 2 ]; then
  echo "Usage: $0 <nugetSource> <OPTIONAL: nugetApiKey>"
  exit 1
fi

# Assign the arguments to variables
nugetSource="$1"
NUGET_API_KEY="$2"

folderPath="./packages"

# Initialize a global arrays to store data
packages=()

# get all nuget packages that have changed
get_packages() {
  IFS=$'\n' read -d '' -ra changedPackages < <(git diff HEAD~1 --name-only | xargs dirname | sort | uniq | grep '^src/framework')
  for dir in "${changedPackages[@]}"; do
    package="$(basename "$dir")"
    packages+=("$package")
  done
}

# Call the iterate_directories function to start the script
get_packages

for proj in "${packages[@]}"; do
  echo "PACK PROJECT: $proj"
  dotnet pack src/framework/$proj/$proj.csproj -c Release -o "$folderPath"
done

case "$nugetSource" in
  local)
    dotnet nuget push "$folderPath/*" --source "local"
    ;;
  nuget)
    dotnet nuget push "$folderPath/*" --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json
    ;;
  *)
    echo "Invalid nuget source argument. Valid options: local, nuget"
    ;;
esac

rm -r "$folderPath"
