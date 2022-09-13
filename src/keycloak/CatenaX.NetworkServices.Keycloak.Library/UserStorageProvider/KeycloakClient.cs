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

using CatenaX.NetworkServices.Keycloak.Library.Models.UserStorageProvider;
using Flurl.Http;

namespace CatenaX.NetworkServices.Keycloak.Library;

public partial class KeycloakClient
{
    [Obsolete("Not working yet")]
    public async Task RemoveImportedUsersAsync(string realm, string storageProviderId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/user-storage/")
            .AppendPathSegment(storageProviderId, true)
            .AppendPathSegment("/remove-imported-users")
            .PostAsync(new StringContent(""))
            .ConfigureAwait(false);

    [Obsolete("Not working yet")]
    public async Task<SynchronizationResult> TriggerUserSynchronizationAsync(string realm, string storageProviderId, UserSyncActions action) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/user-storage/")
            .AppendPathSegment(storageProviderId, true)
            .AppendPathSegment("/sync")
            .SetQueryParam(nameof(action), action == UserSyncActions.Full ? "triggerFullSync" : "triggerChangedUsersSync")
            .PostAsync(new StringContent(""))
            .ReceiveJson<SynchronizationResult>()
            .ConfigureAwait(false);

    [Obsolete("Not working yet")]
    public async Task UnlinkImportedUsersAsync(string realm, string storageProviderId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/user-storage/")
            .AppendPathSegment(storageProviderId, true)
            .AppendPathSegment("/unlink-users")
            .PostAsync(new StringContent(""))
            .ConfigureAwait(false);

    [Obsolete("Not working yet")]
    public async Task<SynchronizationResult> TriggerLdapMapperSynchronizationAsync(string realm, string storageProviderId, string mapperId, LdapMapperSyncActions direction) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/user-storage/")
            .AppendPathSegment(storageProviderId, true)
            .AppendPathSegment("/mappers/")
            .AppendPathSegment(mapperId, true)
            .AppendPathSegment("/sync")
            .SetQueryParam(nameof(direction), direction == LdapMapperSyncActions.FedToKeycloak ? "fedToKeycloak" : "keycloakToFed")
            .PostAsync(new StringContent(""))
            .ReceiveJson<SynchronizationResult>()
            .ConfigureAwait(false);
}
