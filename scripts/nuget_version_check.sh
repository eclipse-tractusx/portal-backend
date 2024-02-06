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
  echo "Usage: $0 <baseBranch|branchRange>"
  exit 1
fi

# Assign the arguments to variables
branchRange="$1"

# Initialize a global arrays to store data
version_update_needed=()

check_version_update(){
  local project="$1"
  local props_file=$project"Directory.Build.props"

  # check if the code (.cs) unchanged
  if ! git diff --name-only $branchRange -- "$project" | grep -qE '\.cs$' ||
    # check if build.props file has been deleted
    ! [ -z $(git diff --name-only --diff-filter=D $branchRange -- "$props_file") ]; then
    return
  fi

  # check if build.props file is unchanged
  if [ -z $(git diff --name-only $branchRange -- "$props_file") ]; then
    version_update_needed+=($project)
    return
  fi

  suffix_before=$(git diff $branchRange -- "$props_file" | grep -E '^-.*<VersionSuffix>' | sed -E 's/^-.*<VersionSuffix>([^<]*)<\/VersionSuffix>/\1/')
  suffix_after=$(git diff $branchRange -- "$props_file" | grep -E '^\+.*<VersionSuffix>' | sed -E 's/^\+.*<VersionSuffix>([^<]*)<\/VersionSuffix>/\1/')

  version_before=$(git diff $branchRange -- "$props_file" | grep -E '^-.*<VersionPrefix>' | sed -E 's/^-.*<VersionPrefix>([^<]*)<\/VersionPrefix>/\1/')
  version_after=$(git diff $branchRange -- "$props_file" | grep -E '^\+.*<VersionPrefix>' | sed -E 's/^\+.*<VersionPrefix>([^<]*)<\/VersionPrefix>/\1/')

  if [ -z $version_after ] &&
     [ -z $suffix_after ] && 
     [ -z $suffix_before ]; then
    version_update_needed+=($project)
    return
  fi

  if [ -z $version_before ]; then
    version_before="0.0.0"
  fi

  if [ -z $version_after ]; then
    version_after=$version_before
  fi

  IFS='.' read -r major_before minor_before patch_before <<< "$version_before"
  IFS='.' read -r major_after minor_after patch_after <<< "$version_after"

  if [ -n "$major_before" ] && [ -n "$major_after" ] &&
    [ -n "$minor_before" ] && [ -n "$minor_after" ] &&
    [ -n "$patch_before" ] && [ -n "$patch_after" ]; then

    # example
    # 1.0.0.rc1 -> 1.0.0.rc1 OK
    # 1.0.0.rc1 -> 1.0.0.rc2 OK
    # 1.0.0.rc1 -> 1.0.0 OK
    if [ -n "$suffix_before" ] &&
      [ "$major_after" -eq "$major_before" -a "$minor_after" -eq "$minor_before" -a "$patch_after" -eq "$patch_before" ]; then
      return
    fi

    # example
    # 1.0.0 -> 1.1.0.rc1 OK
    # 1.0.0.rc1 -> 1.1.0.rc2 OK
    # 1.0.0 -> 2.0.0 OK
    # 1.0.0 -> 1.1.0 OK
    # 1.0.0 -> 1.0.1 OK
    # 1.0.0 -> 1.0.0.rc1 NOT OK
    # 1.0.0 -> 0.9.0 NOT OK
    if [ "$major_after" -gt "$major_before" ] ||
        [ "$major_after" -eq "$major_before" -a "$minor_after" -gt "$minor_before" ] ||
        [ "$major_after" -eq "$major_before" -a "$minor_after" -eq "$minor_before" -a "$patch_after" -gt "$patch_before" ]; then
      return;
    fi

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
