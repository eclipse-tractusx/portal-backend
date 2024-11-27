/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Extensions;

public static class KeycloakRealmSettingsExtensions
{
    public static Dictionary<string, (bool Create, bool Update, bool Delete)> GetFlatDictionary(this KeycloakRealmSettings realmSettings)
    {
        var result = new Dictionary<string, (bool Create, bool Update, bool Delete, int Depth)>();

        GetFlatDictionary(realmSettings.SeederConfigurations, 0, result);

        return result.Select(x => new KeyValuePair<string, ValueTuple<bool, bool, bool>>(x.Key.ToLower(), new ValueTuple<bool, bool, bool>(x.Value.Create, x.Value.Update, x.Value.Delete))).ToDictionary();
    }

    private static (bool Create, bool Update, bool Delete) GetFlatDictionary(
        IEnumerable<SeederConfiguration>? configurations,
        int depth,
        IDictionary<string, (bool Create, bool Update, bool Delete, int Depth)> result)
    {
        if (configurations == null)
        {
            return (false, false, false);
        }

        var parentCreate = false;
        var parentUpdate = false;
        var parentDelete = false;

        foreach (var config in configurations)
        {
            // Process child configurations first
            var childPermissions = GetFlatDictionary(config.SeederConfigurations, depth + 1, result);

            // Combine child permissions with current configuration's permissions
            var create = config.Create || childPermissions.Create;
            var update = config.Update || childPermissions.Update;
            var delete = config.Delete || childPermissions.Delete;

            // If the key doesn't exist or the current depth is deeper, update the result
            if (!result.TryGetValue(config.Key.ToLower(), out var value) || depth > value.Depth)
            {
                result[config.Key.ToLower()] = (create, update, delete, depth);
            }

            // Aggregate permissions for the parent
            parentCreate |= create;
            parentUpdate |= update;
            parentDelete |= delete;
        }

        return (parentCreate, parentUpdate, parentDelete);
    }

    public static SeederConfigurationModel GetConfigurationDictionaries(this KeycloakRealmSettings realmSettings) =>
        new(
            realmSettings.Create,
            realmSettings.Update,
            realmSettings.Delete,
            realmSettings.SeederConfigurations?.ToDictionary(sc =>
                sc.Key.ToLower(),
                ConvertSeederConfigToSeederConfigurationModel) ?? new Dictionary<string, SeederConfigurationModel>());

    private static SeederConfigurationModel ConvertSeederConfigToSeederConfigurationModel(this SeederConfiguration seederConfig) =>
        new(
            seederConfig.Create,
            seederConfig.Update,
            seederConfig.Delete,
            seederConfig.SeederConfigurations?.ToDictionary(sc =>
                sc.Key.ToLower(),
                ConvertSeederConfigToSeederConfigurationModel) ?? new Dictionary<string, SeederConfigurationModel>());
}
