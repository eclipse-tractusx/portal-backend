###############################################################
# Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
#
# See the NOTICE file(s) distributed with this work for additional
# information regarding copyright ownership.
#
# This program and the accompanying materials are made available under the
# terms of the Apache License, Version 2.0 which is available at
# https://www.apache.org/licenses/LICENSE-2.0.
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
# WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
# License for the specific language governing permissions and limitations
# under the License.
#
# SPDX-License-Identifier: Apache-2.0
###############################################################

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
  echo "Build project: $proj"
  dotnet build src/framework/$proj/$proj.csproj -c Release
  echo "Pack project: $proj"
  dotnet pack --no-build --no-restore src/framework/$proj/$proj.csproj -c Release -o "$folderPath"
done

case "$nugetSource" in
  local)
    for packageFile in "$folderPath"/*.nupkg; do
      dotnet nuget push "$packageFile" --source "local"
    done
    ;;
  nuget)
    for packageFile in "$folderPath"/*.nupkg; do
      dotnet nuget push "$packageFile" --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json --skip-duplicate
    done
    ;;
  *)
    echo "Invalid nuget source argument. Valid options: local, nuget"
    ;;
esac

rm -r "$folderPath"
