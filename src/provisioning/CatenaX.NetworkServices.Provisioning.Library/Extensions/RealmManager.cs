/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
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

using CatenaX.NetworkServices.Keycloak.ErrorHandling;
using CatenaX.NetworkServices.Keycloak.Library;
using CatenaX.NetworkServices.Keycloak.Library.Models.RealmsAdmin;
using System.Text.Json;

namespace CatenaX.NetworkServices.Provisioning.Library;

public partial class ProvisioningManager
{
    private async ValueTask CreateSharedRealmAsync(KeycloakClient keycloak, string realm, string name)
    {
        var newRealm = CloneRealm(_Settings.SharedRealm);
        newRealm.Id = realm;
        newRealm._Realm = realm;
        newRealm.DisplayName = name;
        if (!await keycloak.ImportRealmAsync(realm, newRealm).ConfigureAwait(false))
        {
            throw new KeycloakNoSuccessException($"failed to create shared realm {realm} for {name}");
        }
    }

    private static async ValueTask UpdateSharedRealmAsync(KeycloakClient keycloak, string alias, string displayName)
    {
        var realm = await keycloak.GetRealmAsync(alias).ConfigureAwait(false);
        realm.DisplayName = displayName;
        if (!await keycloak.UpdateRealmAsync(alias, realm).ConfigureAwait(false))
        {
            throw new KeycloakNoSuccessException($"failed to update shared realm {alias}");
        }
    }

    private static async ValueTask SetSharedRealmStatusAsync(KeycloakClient keycloak, string alias, bool enabled)
    {
        var realm = await keycloak.GetRealmAsync(alias).ConfigureAwait(false);
        realm.Enabled = enabled;
        if (!await keycloak.UpdateRealmAsync(alias, realm).ConfigureAwait(false))
        {
            throw new KeycloakNoSuccessException($"failed to update shared realm {alias}");
        }
    }

    private static Realm CloneRealm(Realm realm) =>
        JsonSerializer.Deserialize<Realm>(JsonSerializer.Serialize(realm))!;
}
