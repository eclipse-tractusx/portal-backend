###############################################################
# Copyright (c) 2024 Contributors to the Eclipse Foundation
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

# Get branch names
if [ "$#" -ne 1 ]; then
  echo "Usage: $0 <branchRange>"
  exit 1
fi

# Assign the arguments to variables
branchRange="$1"

# Initialize a global arrays to store data
version_update_needed=()
first_version=""
unmatching_package=()

check_version_update(){
  local project="$1"
  local props_file=$project"Directory.Build.props"

  if ! git diff --name-only $branchRange -- "$project" | grep -qE '\.cs$' ||
    ! [ -z $(git diff --name-only --diff-filter=D $branchRange -- "$props_file") ]; then
    return
  fi

  if [ -z $(git diff --name-only $branchRange -- "$props_file") ]; then
    version_update_needed+=($project)
    return
  fi

  # version prefix change is mandatory
  if ! git diff $branchRange -- "$props_file" | grep -qE '^\+[[:space:]]*<VersionPrefix>[0-9]+\.[0-9]+\.[0-9]+</VersionPrefix>' ||
    # version suffix update not permitted
    git diff $branchRange -- "$props_file" | grep -qE '^\+[[:space:]]*<VersionSuffix>[^<]*</VersionSuffix>'; then
      version_update_needed+=($project)
      return
  fi

  version_before=$(git diff $branchRange -- "$props_file" | grep -E '^-.*<VersionPrefix>' | sed -E 's/^-.*<VersionPrefix>([^<]+)<\/VersionPrefix>/\1/')
  version_after=$(git diff $branchRange -- "$props_file" | grep -E '^\+.*<VersionPrefix>' | sed -E 's/^\+.*<VersionPrefix>([^<]+)<\/VersionPrefix>/\1/')
  
  IFS='.' read -r major_before minor_before patch_before <<< "$version_before"
  IFS='.' read -r major_after minor_after patch_after <<< "$version_after"

  if [ -n "$major_before" ] && [ -n "$major_after" ] &&
    [ -n "$minor_before" ] && [ -n "$minor_after" ] &&
    [ -n "$patch_before" ] && [ -n "$patch_after" ] &&
    ( [ "$major_after" -gt "$major_before" ] ||
      [ "$major_before" -eq "$major_after" -a "$minor_after" -gt "$minor_before" ] ||
      [ "$major_before" -eq "$major_after" -a "$minor_before" -eq "$minor_after" -a "$patch_after" -gt "$patch_before" ] ); then
    return;
  fi

  version_update_needed+=($project)
}

# check version update was made for all framework packages which includes changes
for dir in ./src/framework/*/; do
  if [ -d "$dir" ]; then
    check_version_update $dir
  fi
done

# return all packages that still need a version update
for dir in "${version_update_needed[@]}"; do
  echo "$dir"
done
