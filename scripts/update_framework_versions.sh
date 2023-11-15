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
  echo "Usage: $0 <version>"
  exit 1
fi

# Assign the arguments to variables
version="$1"

# Define the version update functions
update_major() {
  local version="$1"
  local updated_version="$(echo "$version" | awk -F. '{$1+=1; $2=0; $3=0; print}' | tr ' ' '.')"
  echo "$updated_version"
}

update_minor() {
  local version="$1"
  local updated_version="$(echo "$version" | awk -F. '{$2+=1; $3=0; print}' | tr ' ' '.')"
  echo "$updated_version"
}

update_patch() {
  local version="$1"
  local updated_version="$(echo "$version" | awk -F. '{$3+=1; print}' | tr ' ' '.')"
  echo "$updated_version"
}

update_pre() {
  local version="$1"
  local current_suffix=$(grep '<VersionSuffix>' "$props_file" | sed -n 's/.*<VersionSuffix>\(.*\)<\/VersionSuffix>.*/\1/p' | sed 's/^[[:space:]]*//;s/[[:space:]]*$//' | tr -d '\n')
  local current_suffix_version="${current_suffix%%"."*}"
  if [ "$current_suffix_version" != "$version" ]; then
    updated_suffix="$version"
  else
    if [[ "$current_suffix" == "alpha" || "$current_suffix" == "beta" ]]; then
      updated_suffix="${current_suffix}.1"
    else
      numeric_part=$(echo "$current_suffix" | sed 's/[^0-9]//g')
      new_numeric_part=$((numeric_part + 1))
      updated_suffix="${version}.${new_numeric_part}"
    fi
  fi
  echo "$updated_suffix"
}

update_version(){
  local directory="$1"
  
  local props_file=$directory"Directory.Build.props"
  # Check if the Directory.Builds.props file exists
  if [ -f "$props_file" ]; then
    # Extract the current version from the XML file
    current_version=$(awk -F'[<>]' '/<VersionPrefix>/{print $3}' "$props_file")
    current_suffix=$(awk -F'[<>]' '/<VersionSuffix>/{print $3}' "$props_file")

    case "$version" in
      major)
        updated_version=$(update_major "$current_version")
        updated_suffix=""
        ;;
      minor)
        updated_version=$(update_minor "$current_version")
        updated_suffix=""
        ;;
      patch)
        updated_version=$(update_patch "$current_version")
        updated_suffix=""
        ;;
      alpha|beta|pre|rc)
        updated_version="$current_version"
        updated_suffix=$(update_pre "$version")
        ;;
      *)
        echo "Invalid version argument. Valid options: major, minor, patch, alpha, beta, pre, rc"
        exit 1
        ;;
    esac

    # Update the VersionPrefix and VersionSuffix in the file
    awk -v new_version="$updated_version" -v new_suffix="$updated_suffix" '/<VersionPrefix>/{gsub(/<VersionPrefix>[^<]+<\/VersionPrefix>/, "<VersionPrefix>" new_version "</VersionPrefix>")}/<VersionSuffix>/{gsub(/<VersionSuffix>[^<]+<\/VersionSuffix>/, "<VersionSuffix>" new_suffix "</VersionSuffix>")}1' "$props_file" > temp && mv temp "$props_file"
    echo "Updated version in $props_file to $updated_version $updated_suffix"
  else
    echo "Directory.Builds.props file not found in $directory"
  fi
}

# Function to iterate over directories in the Framework directory and update the project version
iterate_directories() {
  for dir in ./src/framework/*/; do
    if [ -d "$dir" ]; then
      update_version "$dir"
    fi
  done
}

# Call the iterate_directories function to start the script
iterate_directories
