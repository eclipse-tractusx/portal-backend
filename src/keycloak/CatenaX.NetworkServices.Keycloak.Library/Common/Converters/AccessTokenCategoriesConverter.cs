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

using CatenaX.NetworkServices.Keycloak.Library.Models.Clients;

namespace CatenaX.NetworkServices.Keycloak.Library.Common.Converters;

public class AccessTokenCategoriesConverter : JsonEnumConverter<AccessTokenCategories>
{
    private static readonly Dictionary<AccessTokenCategories, string> s_pairs = new Dictionary<AccessTokenCategories, string>
    {
        [AccessTokenCategories.Internal] = "INTERNAL", 
        [AccessTokenCategories.Access] = "ACCESS", 
        [AccessTokenCategories.Id] = "ID", 
        [AccessTokenCategories.Admin] = "ADMIN", 
        [AccessTokenCategories.Userinfo] = "USERINFO"
    };

    protected override string EntityString { get; } = "access token category";

    protected override string ConvertToString(AccessTokenCategories value) => s_pairs[value];

    protected override AccessTokenCategories ConvertFromString(string s)
    {
        var pair = s_pairs.FirstOrDefault(kvp => kvp.Value.Equals(s, StringComparison.OrdinalIgnoreCase));
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (EqualityComparer<KeyValuePair<AccessTokenCategories, string>>.Default.Equals(pair))
        {
            throw new ArgumentException($"Unknown {EntityString}: {s}");
        }

        return pair.Key;
    }
}
