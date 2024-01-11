/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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

using Flurl;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.ExternalSystems.Provisioning.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.ExternalSystems.Provisioning.Library;

public class IdpManagement : IIdpManagement
{
    private static readonly IReadOnlyDictionary<IamClientAuthMethod, string> IamClientAuthMethodsInternalDictionary = new Dictionary<IamClientAuthMethod, string>
    {
        { IamClientAuthMethod.JWT, "client-jwt" },
        { IamClientAuthMethod.SECRET, "client-secret" },
        { IamClientAuthMethod.X509, "client-x509" },
        { IamClientAuthMethod.SECRET_JWT, "client-secret-jwt" }
    };

    private readonly IKeycloakFactory _factory;
    private readonly KeycloakClient _centralIdp;
    private readonly IdpManagementSettings _settings;
    private readonly IProvisioningDBAccess? _provisioningDbAccess;

    public IdpManagement(IKeycloakFactory keycloakFactory, IProvisioningDBAccess provisioningDBAccess, IOptions<IdpManagementSettings> options)
    {
        _factory = keycloakFactory;
        _centralIdp = keycloakFactory.CreateKeycloakClient("central");
        _provisioningDbAccess = provisioningDBAccess;
        _settings = options.Value;
    }

    public async ValueTask<string> GetNextCentralIdentityProviderNameAsync() =>
        $"{_settings.IdpPrefix}{await _provisioningDbAccess!.GetNextIdentityProviderSequenceAsync().ConfigureAwait(false)}";

    public Task CreateCentralIdentityProviderAsync(string alias, string displayName)
    {
        var newIdp = _settings.CentralIdentityProvider.Clone();
        newIdp.Alias = alias;
        newIdp.DisplayName = displayName;
        return _centralIdp.CreateIdentityProviderAsync(_settings.CentralRealm, newIdp);
    }

    public async Task<(string ClientId, string Secret, string ServiceAccountUserId)> CreateSharedIdpServiceAccountAsync(string realm)
    {
        var sharedIdp = _factory.CreateKeycloakClient("shared");
        var clientId = GetServiceAccountClientId(realm);
        var internalClientId = await CreateServiceAccountClient(sharedIdp, "master", clientId, clientId, IamClientAuthMethod.SECRET, true);
        var serviceAccountUser = await sharedIdp.GetUserForServiceAccountAsync("master", internalClientId).ConfigureAwait(false);
        if (serviceAccountUser == null || string.IsNullOrWhiteSpace(serviceAccountUser.Id))
        {
            throw new NotFoundException("ServiceAccount could not be found for client id: {internalClientId}");
        }

        var credentials = await sharedIdp.GetClientSecretAsync("master", internalClientId).ConfigureAwait(false);
        return new(clientId, credentials.Value, serviceAccountUser.Id);
    }

    public async Task AddRealmRoleMappingsToUserAsync(string serviceAccountUserId)
    {
        var sharedIdp = _factory.CreateKeycloakClient("shared");
        var roleCreateRealm = await sharedIdp.GetRoleByNameAsync("master", "create-realm").ConfigureAwait(false);
        await sharedIdp.AddRealmRoleMappingsToUserAsync("master", serviceAccountUserId, Enumerable.Repeat(roleCreateRealm, 1)).ConfigureAwait(false);
    }

    private async Task<string> CreateServiceAccountClient(KeycloakClient keycloak, string realm, string clientId, string name, IamClientAuthMethod iamClientAuthMethod, bool enabled)
    {
        var newClient = _settings.ServiceAccountClient.Clone();
        newClient.ClientId = clientId;
        newClient.Name = name;
        newClient.ClientAuthenticatorType = IamClientAuthMethodToInternal(iamClientAuthMethod);
        newClient.Enabled = enabled;
        var newClientId = await keycloak.CreateClientAndRetrieveClientIdAsync(realm, newClient).ConfigureAwait(false);
        if (newClientId == null)
        {
            throw new KeycloakNoSuccessException($"failed to create new client {clientId} in central realm");
        }

        return newClientId;
    }

    private static string IamClientAuthMethodToInternal(IamClientAuthMethod iamClientAuthMethod)
    {
        if (!IamClientAuthMethodsInternalDictionary.TryGetValue(iamClientAuthMethod, out var clientAuthMethod))
        {
            throw new ArgumentException($"unexpected value of IamClientAuthMethod: {iamClientAuthMethod}", nameof(iamClientAuthMethod));
        }

        return clientAuthMethod;
    }

    public async ValueTask UpdateCentralIdentityProviderUrlsAsync(string alias, string organisationName, string loginTheme, string clientId, string secret)
    {
        var sharedKeycloak = await CreateSharedRealmAsync(alias, organisationName, loginTheme, clientId, secret);
        var config = await sharedKeycloak.GetOpenIDConfigurationAsync(alias).ConfigureAwait(false);
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false);
        identityProvider.Config!.AuthorizationUrl = config.AuthorizationEndpoint.ToString();
        identityProvider.Config.TokenUrl = config.TokenEndpoint.ToString();
        identityProvider.Config.LogoutUrl = config.EndSessionEndpoint.ToString();
        identityProvider.Config.JwksUrl = config.JwksUri.ToString();
        await _centralIdp.UpdateIdentityProviderAsync(_settings.CentralRealm, alias, identityProvider).ConfigureAwait(false);
    }

    public Task CreateCentralIdentityProviderOrganisationMapperAsync(string alias, string organisationName) =>
        _centralIdp.AddIdentityProviderMapperAsync(
            _settings.CentralRealm,
            alias,
            new IdentityProviderMapper
            {
                Name = _settings.MappedCompanyAttribute + "-mapper",
                _IdentityProviderMapper = "hardcoded-attribute-idp-mapper",
                IdentityProviderAlias = alias,
                Config = new Dictionary<string, string>
                {
                    ["syncMode"] = "INHERIT",
                    ["attribute"] = _settings.MappedCompanyAttribute,
                    ["attribute.value"] = organisationName
                }
            });

    public async Task CreateSharedRealmIdpClientAsync(string realm, string loginTheme, string organisationName, string clientId, string secret)
    {
        await CreateSharedRealmAsync(realm, organisationName, loginTheme, clientId, secret).ConfigureAwait(false);
    }

    public async Task CreateSharedClientAsync(string realm, string clientId, string secret)
    {
        var redirectUrl = await GetCentralBrokerEndpointOIDCAsync(realm).ConfigureAwait(false);
        var jwksUrl = await GetCentralRealmJwksUrlAsync().ConfigureAwait(false);
        var config = new IdentityProviderClientConfig(
            $"{redirectUrl}/*",
            jwksUrl);
        var sharedKeycloak = _factory.CreateKeycloakClient("shared", clientId, secret);
        var newClient = _settings.SharedRealmClient.Clone();
        newClient.RedirectUris = Enumerable.Repeat(config.RedirectUri, 1);
        newClient.Attributes ??= new Dictionary<string, string>();
        newClient.Attributes["jwks.url"] = config.JwksUrl;
        await sharedKeycloak.CreateClientAsync(realm, newClient).ConfigureAwait(false);
    }

    private async ValueTask<string> GetCentralBrokerEndpointOIDCAsync(string alias)
    {
        var openidconfig = await _centralIdp.GetOpenIDConfigurationAsync(_settings.CentralRealm).ConfigureAwait(false);
        return new Url(openidconfig.Issuer)
            .AppendPathSegment("/broker/")
            .AppendPathSegment(alias, true)
            .AppendPathSegment("/endpoint")
            .ToString();
    }

    private async Task<string> GetCentralRealmJwksUrlAsync()
    {
        var config = await _centralIdp.GetOpenIDConfigurationAsync(_settings.CentralRealm).ConfigureAwait(false);
        return config.JwksUri.ToString();
    }

    public async ValueTask EnableCentralIdentityProviderAsync(string alias)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false);
        identityProvider.Enabled = true;
        identityProvider.Config!.HideOnLoginPage = "false";
        await _centralIdp.UpdateIdentityProviderAsync(_settings.CentralRealm, alias, identityProvider).ConfigureAwait(false);
    }

    private async Task<KeycloakClient> CreateSharedRealmAsync(string idpName, string organisationName, string? loginTheme, string clientId, string secret)
    {
        var sharedKeycloak = _factory.CreateKeycloakClient("shared", clientId, secret);

        await CreateSharedRealmAsyncInternal(sharedKeycloak, idpName, organisationName, loginTheme).ConfigureAwait(false);
        return sharedKeycloak;
    }

    private Task CreateSharedRealmAsyncInternal(KeycloakClient keycloak, string realm, string displayName, string? loginTheme)
    {
        var newRealm = _settings.SharedRealm.Clone();
        newRealm.Id = realm;
        newRealm._Realm = realm;
        newRealm.DisplayName = displayName;
        newRealm.LoginTheme = loginTheme;
        return keycloak.ImportRealmAsync(realm, newRealm);
    }

    private Task<IdentityProvider> GetCentralIdentityProviderAsync(string alias) =>
        _centralIdp.GetIdentityProviderAsync(_settings.CentralRealm, alias);

    private string GetServiceAccountClientId(string realm) =>
        _settings.ServiceAccountClientPrefix + realm;
}
