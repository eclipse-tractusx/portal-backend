/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Common;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders;
using Flurl.Http;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task<IDictionary<string, object>> ImportIdentityProviderAsync(string realm, string input) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/import-config")
            .PostMultipartAsync(content => content.AddFile(Path.GetFileName(input), Path.GetDirectoryName(input)))
            .ReceiveJson<IDictionary<string, object>>()
            .ConfigureAwait(false);

    public async Task<IDictionary<string, object>> ImportIdentityProviderFromUrlAsync(string realm, string url) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/import-config")
            .PostJsonAsync(new Dictionary<string,string> {
                ["fromUrl"] = url,
                ["providerId"] = "oidc"
            })
            .ReceiveJson<IDictionary<string, object>>()
            .ConfigureAwait(false);

    public async Task CreateIdentityProviderAsync(string realm, IdentityProvider identityProvider) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances")
            .PostJsonAsync(identityProvider)
            .ConfigureAwait(false);

    public async Task<IEnumerable<IdentityProvider>> GetIdentityProviderInstancesAsync(string realm) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances")
            .GetJsonAsync<IEnumerable<IdentityProvider>>()
            .ConfigureAwait(false);

    public async Task<IdentityProvider> GetIdentityProviderAsync(string realm, string identityProviderAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .GetJsonAsync<IdentityProvider>()
            .ConfigureAwait(false);

    /// <summary>
    /// <see cref="https://github.com/keycloak/keycloak-documentation/blob/master/server_development/topics/identity-brokering/tokens.adoc"/>
    /// </summary>
    /// <param name="realm"></param>
    /// <param name="identityProviderAlias"></param>
    /// <returns></returns>
    public async Task<IdentityProviderToken> GetIdentityProviderTokenAsync(string realm, string identityProviderAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/broker/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/token")
            .GetJsonAsync<IdentityProviderToken>()
            .ConfigureAwait(false);

    public async Task UpdateIdentityProviderAsync(string realm, string identityProviderAlias, IdentityProvider identityProvider) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .PutJsonAsync(identityProvider)
            .ConfigureAwait(false);

    public async Task DeleteIdentityProviderAsync(string realm, string identityProviderAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .DeleteAsync()
            .ConfigureAwait(false);

    public async Task ExportIdentityProviderPublicBrokerConfigurationAsync(string realm, string identityProviderAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/export")
            .GetAsync()
            .ConfigureAwait(false);
    
    public async Task<ManagementPermission> GetIdentityProviderAuthorizationPermissionsInitializedAsync(string realm, string identityProviderAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/management/permissions")
            .GetJsonAsync<ManagementPermission>()
            .ConfigureAwait(false);

    public async Task<ManagementPermission> SetIdentityProviderAuthorizationPermissionsInitializedAsync(string realm, string identityProviderAlias, ManagementPermission managementPermission) => 
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/management/permissions")
            .PutJsonAsync(managementPermission)
            .ReceiveJson<ManagementPermission>()
            .ConfigureAwait(false);
    
    public async Task<IDictionary<string, object>> GetIdentityProviderMapperTypesAsync(string realm, string identityProviderAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/mapper-types")
            .GetJsonAsync<IDictionary<string, object>>()
            .ConfigureAwait(false);

    public async Task AddIdentityProviderMapperAsync(string realm, string identityProviderAlias, IdentityProviderMapper identityProviderMapper) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/mappers")
            .PostJsonAsync(identityProviderMapper)
            .ConfigureAwait(false);
    
    public async Task<IEnumerable<IdentityProviderMapper>> GetIdentityProviderMappersAsync(string realm, string identityProviderAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/mappers")
            .GetJsonAsync<IEnumerable<IdentityProviderMapper>>()
            .ConfigureAwait(false);
    
    public async Task<IdentityProviderMapper> GetIdentityProviderMapperByIdAsync(string realm, string identityProviderAlias, string mapperId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/mappers/")
            .AppendPathSegment(mapperId, true)
            .GetJsonAsync<IdentityProviderMapper>()
            .ConfigureAwait(false);

    public async Task UpdateIdentityProviderMapperAsync(string realm, string identityProviderAlias, string mapperId, IdentityProviderMapper identityProviderMapper) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/mappers/")
            .AppendPathSegment(mapperId, true)
            .PutJsonAsync(identityProviderMapper)
            .ConfigureAwait(false);

    public async Task DeleteIdentityProviderMapperAsync(string realm, string identityProviderAlias, string mapperId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/instances/")
            .AppendPathSegment(identityProviderAlias, true)
            .AppendPathSegment("/mappers/")
            .AppendPathSegment(mapperId, true)
            .DeleteAsync()
            .ConfigureAwait(false);

    public async Task<IdentityProviderInfo> GetIdentityProviderByProviderIdAsync(string realm, string providerId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/identity-provider/providers/")
            .AppendPathSegment(providerId, true)
            .GetJsonAsync<IdentityProviderInfo>()
            .ConfigureAwait(false);
}
