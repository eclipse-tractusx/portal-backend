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

file="src/framework/Framework.Async/Directory.Build.props"
# Get the version prefix
version_prefix=$(grep -oP '<VersionPrefix>\K[^<]+' $file)

# Get the version suffix
version_suffix=$(grep -oP '<VersionSuffix>\K[^<]+' $file)

# Combine the prefix and suffix if the suffix is not empty
if [ -n "$version_suffix" ]; then
  version="$version_prefix-$version_suffix-framework"
else
  version="$version_prefix-framework"
fi

echo "Version: $version"