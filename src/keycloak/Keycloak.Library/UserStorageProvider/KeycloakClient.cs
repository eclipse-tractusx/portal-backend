/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 ********************************************************************************/

using Flurl.Http;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.UserStorageProvider;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
    [Obsolete("Not working yet")]
    public async Task RemoveImportedUsersAsync(string realm, string storageProviderId)
    {
        using var stringContent = new StringContent("");
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/user-storage/")
            .AppendPathSegment(storageProviderId, true)
            .AppendPathSegment("/remove-imported-users")
            .PostAsync(stringContent)
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    [Obsolete("Not working yet")]
    public async Task<SynchronizationResult> TriggerUserSynchronizationAsync(string realm, string storageProviderId, UserSyncActions action)
    {
        using var stringContent = new StringContent("");
        return await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/user-storage/")
            .AppendPathSegment(storageProviderId, true)
            .AppendPathSegment("/sync")
            .SetQueryParam(nameof(action), action == UserSyncActions.Full ? "triggerFullSync" : "triggerChangedUsersSync")
            .PostAsync(stringContent)
            .ReceiveJson<SynchronizationResult>()
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    [Obsolete("Not working yet")]
    public async Task UnlinkImportedUsersAsync(string realm, string storageProviderId)
    {
        using var stringContent = new StringContent("");
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/user-storage/")
            .AppendPathSegment(storageProviderId, true)
            .AppendPathSegment("/unlink-users")
            .PostAsync(stringContent)
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    [Obsolete("Not working yet")]
    public async Task<SynchronizationResult> TriggerLdapMapperSynchronizationAsync(string realm, string storageProviderId, string mapperId, LdapMapperSyncActions direction)
    {
        using var stringContent = new StringContent("");
        return await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/user-storage/")
            .AppendPathSegment(storageProviderId, true)
            .AppendPathSegment("/mappers/")
            .AppendPathSegment(mapperId, true)
            .AppendPathSegment("/sync")
            .SetQueryParam(nameof(direction), direction == LdapMapperSyncActions.FedToKeycloak ? "fedToKeycloak" : "keycloakToFed")
            .PostAsync(stringContent)
            .ReceiveJson<SynchronizationResult>()
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }
}
