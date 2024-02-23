/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

public partial class ProvisioningManager : IProvisioningManager
{
    private readonly KeycloakClient _CentralIdp;
    private readonly IKeycloakFactory _Factory;
    private readonly IProvisioningDBAccess? _ProvisioningDBAccess;
    private readonly ProvisioningSettings _Settings;

    public ProvisioningManager(IKeycloakFactory keycloakFactory, IProvisioningDBAccess? provisioningDBAccess, IOptions<ProvisioningSettings> options)
    {
        _CentralIdp = keycloakFactory.CreateKeycloakClient("central");
        _Factory = keycloakFactory;
        _Settings = options.Value;
        _ProvisioningDBAccess = provisioningDBAccess;
    }

    public ProvisioningManager(IKeycloakFactory keycloakFactory, IOptions<ProvisioningSettings> options)
        : this(keycloakFactory, null, options)
    {
    }

    public async Task SetupSharedIdpAsync(string idpName, string organisationName, string? loginTheme)
    {
        await CreateCentralIdentityProviderAsync(idpName, organisationName, _Settings.CentralIdentityProvider).ConfigureAwait(false);

        var (clientId, secret) = await CreateSharedIdpServiceAccountAsync(idpName).ConfigureAwait(false);
        var sharedKeycloak = _Factory.CreateKeycloakClient("shared", clientId, secret);

        await CreateSharedRealmAsync(sharedKeycloak, idpName, organisationName, loginTheme).ConfigureAwait(false);

        await UpdateCentralIdentityProviderUrlsAsync(idpName, await sharedKeycloak.GetOpenIDConfigurationAsync(idpName).ConfigureAwait(false)).ConfigureAwait(false);

        await CreateCentralIdentityProviderOrganisationMapperAsync(idpName, organisationName).ConfigureAwait(false);

        await CreateSharedRealmIdentityProviderClientAsync(sharedKeycloak, idpName, new IdentityProviderClientConfig(
            await GetCentralBrokerEndpointOIDCAsync(idpName).ConfigureAwait(false) + "/*",
            await GetCentralRealmJwksUrlAsync().ConfigureAwait(false)
        )).ConfigureAwait(false);

        await EnableCentralIdentityProviderAsync(idpName).ConfigureAwait(false);
    }

    public async ValueTask DeleteSharedIdpRealmAsync(string alias)
    {
        var deleteSharedKeycloak = await GetSharedKeycloakClient(alias).ConfigureAwait(false);
        await deleteSharedKeycloak.DeleteRealmAsync(alias).ConfigureAwait(false);
        var sharedKeycloak = _Factory.CreateKeycloakClient("shared");
        await DeleteSharedIdpServiceAccountAsync(sharedKeycloak, alias);
    }

    public async Task<string> CreateOwnIdpAsync(string displayName, string organisationName, IamIdentityProviderProtocol providerProtocol)
    {
        var idpName = await GetNextCentralIdentityProviderNameAsync().ConfigureAwait(false);

        await CreateCentralIdentityProviderAsync(idpName, displayName, GetIdentityProviderTemplate(providerProtocol)).ConfigureAwait(false);

        await CreateCentralIdentityProviderOrganisationMapperAsync(idpName, organisationName).ConfigureAwait(false);

        return idpName;
    }

    public IEnumerable<(string AttributeName, IEnumerable<string> AttributeValues)> GetStandardAttributes(string? organisationName = null, string? businessPartnerNumber = null)
    {
        var attributes = new List<(string, IEnumerable<string>)>();
        if (organisationName != null)
        {
            attributes.Add(new(_Settings.MappedCompanyAttribute, Enumerable.Repeat(organisationName, 1)));
        }
        if (businessPartnerNumber != null)
        {
            attributes.Add(new(_Settings.MappedBpnAttribute, Enumerable.Repeat(businessPartnerNumber, 1)));
        }
        return attributes;
    }

    async Task<string> IProvisioningManager.SetupClientAsync(string redirectUrl, string? baseUrl, IEnumerable<string>? optionalRoleNames, bool enabled)
    {
        var clientId = await GetNextClientIdAsync().ConfigureAwait(false);
        var internalId = await CreateCentralOIDCClientAsync(clientId, redirectUrl, baseUrl, enabled).ConfigureAwait(false);
        await CreateCentralOIDCClientAudienceMapperAsync(internalId, clientId).ConfigureAwait(false);
        if (optionalRoleNames != null && optionalRoleNames.Any())
        {
            await this.AssignClientRolesToClient(internalId, optionalRoleNames).ConfigureAwait(false);
        }

        return clientId;
    }

    public async Task AddBpnAttributetoUserAsync(string userId, IEnumerable<string> bpns)
    {
        var user = await _CentralIdp.GetUserAsync(_Settings.CentralRealm, userId).ConfigureAwait(false);
        user.Attributes ??= new Dictionary<string, IEnumerable<string>>();
        user.Attributes[_Settings.MappedBpnAttribute] = (user.Attributes.TryGetValue(_Settings.MappedBpnAttribute, out var existingBpns))
            ? existingBpns.Concat(bpns).Distinct()
            : bpns;
        await _CentralIdp.UpdateUserAsync(_Settings.CentralRealm, userId.ToString(), user).ConfigureAwait(false);
    }

    public Task AddProtocolMapperAsync(string clientId) =>
        _CentralIdp.CreateClientProtocolMapperAsync(
            _Settings.CentralRealm,
            clientId,
            Clone(_Settings.ClientProtocolMapper));

    public async Task DeleteCentralUserBusinessPartnerNumberAsync(string userId, string businessPartnerNumber)
    {
        var user = await _CentralIdp.GetUserAsync(_Settings.CentralRealm, userId).ConfigureAwait(false);

        if (user.Attributes == null || !user.Attributes.TryGetValue(_Settings.MappedBpnAttribute, out var existingBpns))
        {
            throw new KeycloakEntityNotFoundException($"attribute {_Settings.MappedBpnAttribute} not found in the mappers of user {userId}");
        }

        user.Attributes[_Settings.MappedBpnAttribute] = existingBpns.Where(bpn => bpn != businessPartnerNumber);

        await _CentralIdp.UpdateUserAsync(_Settings.CentralRealm, userId, user).ConfigureAwait(false);
    }

    public async Task ResetSharedUserPasswordAsync(string realm, string userId)
    {
        var providerUserId = await GetProviderUserIdForCentralUserIdAsync(realm, userId).ConfigureAwait(false);
        if (providerUserId == null)
        {
            throw new KeycloakEntityNotFoundException($"userId {userId} is not linked to shared realm {realm}");
        }
        var sharedKeycloak = await GetSharedKeycloakClient(realm).ConfigureAwait(false);
        await sharedKeycloak.SendUserUpdateAccountEmailAsync(realm, providerUserId, Enumerable.Repeat("UPDATE_PASSWORD", 1)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<string>> GetClientRoleMappingsForUserAsync(string userId, string clientId)
    {
        var idOfClient = await GetIdOfCentralClientAsync(clientId).ConfigureAwait(false);
        return (await _CentralIdp.GetClientRoleMappingsForUserAsync(_Settings.CentralRealm, userId, idOfClient).ConfigureAwait(false))
            .Where(r => r.Composite == true).Select(x => x.Name);
    }

    public async ValueTask<bool> IsCentralIdentityProviderEnabled(string alias)
    {
        return (await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false)).Enabled ?? false;
    }

    public async Task<string> GetCentralIdentityProviderDisplayName(string alias) =>
        (await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false)).DisplayName;

    public async ValueTask<IdentityProviderConfigOidc> GetCentralIdentityProviderDataOIDCAsync(string alias)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false);
        var redirectUri = await GetCentralBrokerEndpointOIDCAsync(alias).ConfigureAwait(false);
        return new IdentityProviderConfigOidc(
            identityProvider.DisplayName,
            redirectUri,
            identityProvider.Config.TokenUrl,
            identityProvider.Config.LogoutUrl,
            identityProvider.Config.ClientId,
            identityProvider.Config.ClientSecret,
            identityProvider.Enabled ?? false,
            identityProvider.Config.AuthorizationUrl,
            IdentityProviderClientAuthTypeToIamClientAuthMethod(identityProvider.Config.ClientAuthMethod),
            identityProvider.Config.ClientAssertionSigningAlg == null ? null : Enum.Parse<IamIdentityProviderSignatureAlgorithm>(identityProvider.Config.ClientAssertionSigningAlg));
    }

    public async ValueTask UpdateSharedIdentityProviderAsync(string alias, string displayName)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false);
        identityProvider.DisplayName = displayName;
        var sharedKeycloak = await GetSharedKeycloakClient(alias).ConfigureAwait(false);
        await UpdateSharedRealmAsync(sharedKeycloak, alias, displayName, null).ConfigureAwait(false);
        await UpdateCentralIdentityProviderAsync(alias, identityProvider).ConfigureAwait(false);
    }

    public async ValueTask UpdateSharedRealmTheme(string alias, string loginTheme)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false);
        var sharedKeycloak = await GetSharedKeycloakClient(alias).ConfigureAwait(false);
        await UpdateSharedRealmAsync(sharedKeycloak, alias, identityProvider.DisplayName, loginTheme).ConfigureAwait(false);
    }

    public async ValueTask SetSharedIdentityProviderStatusAsync(string alias, bool enabled)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false);
        identityProvider.Enabled = enabled;
        identityProvider.Config.HideOnLoginPage = enabled ? "false" : "true";
        var sharedKeycloak = await GetSharedKeycloakClient(alias).ConfigureAwait(false);
        await SetSharedRealmStatusAsync(sharedKeycloak, alias, enabled).ConfigureAwait(false);
        await UpdateCentralIdentityProviderAsync(alias, identityProvider).ConfigureAwait(false);
    }

    public async ValueTask SetCentralIdentityProviderStatusAsync(string alias, bool enabled)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false);
        identityProvider.Enabled = enabled;
        identityProvider.Config.HideOnLoginPage = enabled ? "false" : "true";
        await UpdateCentralIdentityProviderAsync(alias, identityProvider).ConfigureAwait(false);
    }

    public ValueTask UpdateCentralIdentityProviderDataOIDCAsync(IdentityProviderEditableConfigOidc identityProviderConfigOidc, CancellationToken cancellationToken)
    {
        if (identityProviderConfigOidc.Secret == null)
        {
            switch (identityProviderConfigOidc.ClientAuthMethod)
            {
                case IamIdentityProviderClientAuthMethod.SECRET_BASIC:
                case IamIdentityProviderClientAuthMethod.SECRET_POST:
                case IamIdentityProviderClientAuthMethod.SECRET_JWT:
                    throw new ArgumentException($"secret must not be null for clientAuthMethod {identityProviderConfigOidc.ClientAuthMethod.ToString()}");
                default:
                    break;
            }
        }
        if (!identityProviderConfigOidc.SignatureAlgorithm.HasValue)
        {
            switch (identityProviderConfigOidc.ClientAuthMethod)
            {
                case IamIdentityProviderClientAuthMethod.SECRET_JWT:
                case IamIdentityProviderClientAuthMethod.JWT:
                    throw new ArgumentException($"signatureAlgorithm must not be null for clientAuthMethod {identityProviderConfigOidc.ClientAuthMethod.ToString()}");
                default:
                    break;
            }
        }
        return UpdateCentralIdentityProviderDataOIDCInternalAsync(identityProviderConfigOidc, cancellationToken);
    }

    private async ValueTask UpdateCentralIdentityProviderDataOIDCInternalAsync(IdentityProviderEditableConfigOidc identityProviderConfigOidc, CancellationToken cancellationToken)
    {
        var (alias, displayName, metadataUrl, clientAuthMethod, clientId, secret, signatureAlgorithm) = identityProviderConfigOidc;
        var identityProvider = await SetIdentityProviderMetadataFromUrlAsync(await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false), metadataUrl, cancellationToken).ConfigureAwait(false);
        identityProvider.DisplayName = displayName;
        identityProvider.Config.ClientAuthMethod = IamIdentityProviderClientAuthMethodToInternal(clientAuthMethod);
        identityProvider.Config.ClientId = clientId;
        identityProvider.Config.ClientSecret = secret;
        identityProvider.Config.ClientAssertionSigningAlg = signatureAlgorithm?.ToString();
        await UpdateCentralIdentityProviderAsync(alias, identityProvider).ConfigureAwait(false);
    }

    public async ValueTask<IdentityProviderConfigSaml> GetCentralIdentityProviderDataSAMLAsync(string alias)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false);
        var redirectUri = await GetCentralBrokerEndpointSAMLAsync(alias).ConfigureAwait(false);
        return new IdentityProviderConfigSaml(
            identityProvider.DisplayName,
            redirectUri,
            identityProvider.Config.ClientId,
            identityProvider.Enabled ?? false,
            identityProvider.Config.EntityId,
            identityProvider.Config.SingleSignOnServiceUrl);
    }

    public async ValueTask UpdateCentralIdentityProviderDataSAMLAsync(IdentityProviderEditableConfigSaml identityProviderEditableConfigSaml)
    {
        var (alias, displayName, entityId, singleSignOnServiceUrl) = identityProviderEditableConfigSaml;
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false);
        identityProvider.DisplayName = displayName;
        identityProvider.Config.EntityId = entityId;
        identityProvider.Config.SingleSignOnServiceUrl = singleSignOnServiceUrl;
        await UpdateCentralIdentityProviderAsync(alias, identityProvider).ConfigureAwait(false);
    }

    private async Task<KeycloakClient> GetSharedKeycloakClient(string realm)
    {
        var (clientId, secret) = await GetSharedIdpServiceAccountSecretAsync(realm).ConfigureAwait(false);
        return _Factory.CreateKeycloakClient("shared", clientId, secret);
    }

    private static T Clone<T>(T cloneObject)
        where T : class =>
        JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(cloneObject))!;
}
