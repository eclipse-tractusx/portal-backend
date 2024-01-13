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
if [ "$#" -ne 1 ]; then
  echo "Usage: $0 <nugetApiKey>"
  exit 1
fi

# Assign the arguments to variables
NUGET_API_KEY="$1"
folderPath="./packages"

# Function to iterate over directories in the Framework directory and create a nuget package
iterate_directories() {
  for dir in ./src/framework/*/; do
    if [ -d "$dir" ]; then
      proj="$(basename "$dir")"
      echo "Pack project: $proj"
      dotnet pack src/framework/$proj/$proj.csproj -c Release -o "$folderPath"
    fi
  done
}

iterate_directories

for packageFile in "$folderPath"/*.nupkg; do
  dotnet nuget push "$packageFile" --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json --skip-duplicate
done

rm -r "$folderPath"
