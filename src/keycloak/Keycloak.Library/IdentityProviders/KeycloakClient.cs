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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Common;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task<IDictionary<string, object>> ImportIdentityProviderAsync(string realm, string input) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/import-config")
            .PostMultipartAsync(content => content.AddFile(Path.GetFileName(input), Path.GetDirectoryName(input)))
            .ReceiveJson<IDictionary<string, object>>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IDictionary<string, object>> ImportIdentityProviderFromUrlAsync(string realm, string url, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/import-config")
            .PostJsonAsync(new Dictionary<string, string>
            {
                ["fromUrl"] = url,
                ["providerId"] = "oidc"
            }, cancellationToken)
            .ReceiveJson<IDictionary<string, object>>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task CreateIdentityProviderAsync(string realm, IdentityProvider identityProvider, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances")
            .PostJsonAsync(identityProvider, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IEnumerable<IdentityProvider>> GetIdentityProviderInstancesAsync(string realm) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances")
            .GetJsonAsync<IEnumerable<IdentityProvider>>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IdentityProvider> GetIdentityProviderAsync(string realm, string identityProviderAlias, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .GetJsonAsync<IdentityProvider>(cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    /// <summary>
    /// <see cref="https://github.com/keycloak/keycloak-documentation/blob/master/server_development/topics/identity-brokering/tokens.adoc"/>
    /// </summary>
    /// <param name="realm"></param>
    /// <param name="identityProviderAlias"></param>
    /// <returns></returns>
    public async Task<IdentityProviderToken> GetIdentityProviderTokenAsync(string realm, string identityProviderAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/broker/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/token")
            .GetJsonAsync<IdentityProviderToken>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task UpdateIdentityProviderAsync(string realm, string identityProviderAlias, IdentityProvider identityProvider, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .PutJsonAsync(identityProvider, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task DeleteIdentityProviderAsync(string realm, string identityProviderAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .DeleteAsync()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task ExportIdentityProviderPublicBrokerConfigurationAsync(string realm, string identityProviderAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/export")
            .GetAsync()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<ManagementPermission> GetIdentityProviderAuthorizationPermissionsInitializedAsync(string realm, string identityProviderAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/management/permissions")
            .GetJsonAsync<ManagementPermission>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<ManagementPermission> SetIdentityProviderAuthorizationPermissionsInitializedAsync(string realm, string identityProviderAlias, ManagementPermission managementPermission) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/management/permissions")
            .PutJsonAsync(managementPermission)
            .ReceiveJson<ManagementPermission>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IDictionary<string, object>> GetIdentityProviderMapperTypesAsync(string realm, string identityProviderAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/mapper-types")
            .GetJsonAsync<IDictionary<string, object>>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task AddIdentityProviderMapperAsync(string realm, string identityProviderAlias, IdentityProviderMapper identityProviderMapper, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/mappers")
            .PostJsonAsync(identityProviderMapper, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IEnumerable<IdentityProviderMapper>> GetIdentityProviderMappersAsync(string realm, string identityProviderAlias, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/mappers")
            .GetJsonAsync<IEnumerable<IdentityProviderMapper>>(cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IdentityProviderMapper> GetIdentityProviderMapperByIdAsync(string realm, string identityProviderAlias, string mapperId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/mappers/")
            .AppendPathSegment(mapperId, true)
            .GetJsonAsync<IdentityProviderMapper>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task UpdateIdentityProviderMapperAsync(string realm, string identityProviderAlias, string mapperId, IdentityProviderMapper identityProviderMapper, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/mappers/")
            .AppendPathSegment(mapperId, true)
            .PutJsonAsync(identityProviderMapper, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task DeleteIdentityProviderMapperAsync(string realm, string identityProviderAlias, string mapperId, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/mappers/")
            .AppendPathSegment(mapperId, true)
            .DeleteAsync(cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IdentityProviderInfo> GetIdentityProviderByProviderIdAsync(string realm, string providerId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/providers/")
            .AppendPathSegment(providerId, true)
            .GetJsonAsync<IdentityProviderInfo>()
            .ConfigureAwait(ConfigureAwaitOptions.None);
}
