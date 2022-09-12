/********************************************************************************
 * Copyright (c) 2021,2022 Contributors to https://github.com/lvermeulen/Keycloak.Net.git and BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

﻿using System;
using System.Collections.Generic;
using System.Linq;
using Keycloak.Net.Models.Root;

namespace Keycloak.Net.Common.Converters
{
    public class ConfigTypeConverter : JsonEnumConverter<ConfigType>
    {
        private static readonly Dictionary<ConfigType, string> s_pairs = new Dictionary<ConfigType, string>
        {
            [ConfigType.Int] = "int",
            [ConfigType.String] = "string"
        };

        protected override string EntityString { get; } = nameof(ConfigType).ToLower();

        protected override string ConvertToString(ConfigType value) => s_pairs[value];

        protected override ConfigType ConvertFromString(string s)
        {
            var pair = s_pairs.FirstOrDefault(kvp => kvp.Value.Equals(s, StringComparison.OrdinalIgnoreCase));
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (EqualityComparer<KeyValuePair<ConfigType, string>>.Default.Equals(pair))
            {
                throw new ArgumentException($"Unknown {EntityString}: {s}");
            }

            return pair.Key;
        }
    }
}
