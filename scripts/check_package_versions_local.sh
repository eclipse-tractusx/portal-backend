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

# Initialize a global arrays to store data
version_update_needed=()

# get the directory.build files to check the updated versions
changed_versions=($(git diff --name-only HEAD~1 | grep 'Directory.Build.props'))

check_version_update(){
  local project="$1"
  local props_file="src/framework/"$project"/Directory.Build.props"
  if [[ " ${changed_versions[@]} " =~ " $props_file " ]]; then
    if ! git diff HEAD~1 -- "$props_file" | grep -qE '^\+[[:space:]]*<VersionPrefix>[0-9]+\.[0-9]+\.[0-9]+</VersionPrefix>' ||
      (! git diff HEAD~1 -- "$props_file" | grep -qE '^\+[[:space:]]*<VersionSuffix></VersionSuffix>' && git diff HEAD~1 -- "$props_file" | grep -qE '^\+[[:space:]]*<VersionSuffix>[^<]*</VersionSuffix>'); then
        version_update_needed+=($project)
    fi
  else
    version_update_needed+=($project)
  fi
}

for dir in ./src/framework/*/; do
  if [ -d "$dir" ]; then
    proj="$(basename "$dir")"
    check_version_update $proj
  fi
done

# return all packages that still need a version update
for dir in "${version_update_needed[@]}"; do
  echo "$dir"
done