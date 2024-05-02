/********************************************************************************
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
    private readonly KeycloakClient _centralIdp;
    private readonly IKeycloakFactory _factory;
    private readonly IProvisioningDBAccess? _provisioningDBAccess;
    private readonly ProvisioningSettings _settings;

    public ProvisioningManager(IKeycloakFactory keycloakFactory, IProvisioningDBAccess? provisioningDBAccess, IOptions<ProvisioningSettings> options)
    {
        _centralIdp = keycloakFactory.CreateKeycloakClient("central");
        _factory = keycloakFactory;
        _settings = options.Value;
        _provisioningDBAccess = provisioningDBAccess;
    }

    public ProvisioningManager(IKeycloakFactory keycloakFactory, IOptions<ProvisioningSettings> options)
        : this(keycloakFactory, null, options)
    {
    }

    public async ValueTask DeleteSharedIdpRealmAsync(string alias)
    {
        var deleteSharedKeycloak = await GetSharedKeycloakClient(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        await deleteSharedKeycloak.DeleteRealmAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        var sharedKeycloak = _factory.CreateKeycloakClient("shared");
        await DeleteSharedIdpServiceAccountAsync(sharedKeycloak, alias);
    }

    public async Task<string> CreateOwnIdpAsync(string displayName, string organisationName, IamIdentityProviderProtocol providerProtocol)
    {
        var idpName = await GetNextCentralIdentityProviderNameAsync().ConfigureAwait(false);

        await CreateCentralIdentityProviderAsyncInternal(idpName, displayName, GetIdentityProviderTemplate(providerProtocol)).ConfigureAwait(ConfigureAwaitOptions.None);

        await CreateCentralIdentityProviderOrganisationMapperAsync(idpName, organisationName).ConfigureAwait(ConfigureAwaitOptions.None);

        return idpName;
    }

    public IEnumerable<(string AttributeName, IEnumerable<string> AttributeValues)> GetStandardAttributes(string? organisationName = null, string? businessPartnerNumber = null)
    {
        var attributes = new List<(string, IEnumerable<string>)>();
        if (organisationName != null)
        {
            attributes.Add(new(_settings.MappedCompanyAttribute, Enumerable.Repeat(organisationName, 1)));
        }
        if (businessPartnerNumber != null)
        {
            attributes.Add(new(_settings.MappedBpnAttribute, Enumerable.Repeat(businessPartnerNumber, 1)));
        }
        return attributes;
    }

    async Task<string> IProvisioningManager.SetupClientAsync(string redirectUrl, string? baseUrl, IEnumerable<string>? optionalRoleNames, bool enabled)
    {
        var clientId = await GetNextClientIdAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        var internalId = await CreateCentralOIDCClientAsync(clientId, redirectUrl, baseUrl, enabled).ConfigureAwait(ConfigureAwaitOptions.None);
        await CreateCentralOIDCClientAudienceMapperAsync(internalId, clientId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (optionalRoleNames != null && optionalRoleNames.Any())
        {
            await AssignClientRolesToClient(internalId, optionalRoleNames).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        return clientId;
    }

    public async Task AddBpnAttributetoUserAsync(string centralUserId, IEnumerable<string> bpns)
    {
        var user = await _centralIdp.GetUserAsync(_settings.CentralRealm, centralUserId).ConfigureAwait(ConfigureAwaitOptions.None);
        user.Attributes ??= new Dictionary<string, IEnumerable<string>>();
        user.Attributes[_settings.MappedBpnAttribute] = user.Attributes.TryGetValue(_settings.MappedBpnAttribute, out var existingBpns)
            ? existingBpns.Concat(bpns).Distinct()
            : bpns;
        await _centralIdp.UpdateUserAsync(_settings.CentralRealm, centralUserId.ToString(), user).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public Task AddProtocolMapperAsync(string clientId) =>
        _centralIdp.CreateClientProtocolMapperAsync(
            _settings.CentralRealm,
            clientId,
            Clone(_settings.ClientProtocolMapper));

    public async Task DeleteCentralUserBusinessPartnerNumberAsync(string centralUserId, string businessPartnerNumber)
    {
        var user = await _centralIdp.GetUserAsync(_settings.CentralRealm, centralUserId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (user.Attributes == null || !user.Attributes.TryGetValue(_settings.MappedBpnAttribute, out var existingBpns))
        {
            throw new KeycloakEntityNotFoundException($"attribute {_settings.MappedBpnAttribute} not found in the mappers of user {centralUserId}");
        }

        user.Attributes[_settings.MappedBpnAttribute] = existingBpns.Where(bpn => bpn != businessPartnerNumber);

        await _centralIdp.UpdateUserAsync(_settings.CentralRealm, centralUserId, user).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task ResetSharedUserPasswordAsync(string realm, string userId)
    {
        var providerUserId = await GetProviderUserIdForCentralUserIdAsync(realm, userId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (providerUserId == null)
        {
            throw new KeycloakEntityNotFoundException($"userId {userId} is not linked to shared realm {realm}");
        }
        var sharedKeycloak = await GetSharedKeycloakClient(realm).ConfigureAwait(ConfigureAwaitOptions.None);
        await sharedKeycloak.SendUserUpdateAccountEmailAsync(realm, providerUserId, Enumerable.Repeat("UPDATE_PASSWORD", 1)).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<IEnumerable<string>> GetClientRoleMappingsForUserAsync(string userId, string clientId)
    {
        var idOfClient = await GetIdOfCentralClientAsync(clientId).ConfigureAwait(ConfigureAwaitOptions.None);
        return (await _centralIdp.GetClientRoleMappingsForUserAsync(_settings.CentralRealm, userId, idOfClient).ConfigureAwait(ConfigureAwaitOptions.None))?
            .Where(r => r.Composite.HasValue && r.Composite.Value).Select(x => x.Name ?? throw new KeycloakInvalidResponseException("name of role is null")) ?? throw new KeycloakInvalidResponseException();
    }

    public async ValueTask<bool> IsCentralIdentityProviderEnabled(string alias)
    {
        return (await GetCentralIdentityProviderAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None)).Enabled ?? false;
    }

    private static readonly string NullConfigMessage = "config of identityProvider is null";

    public async Task<string?> GetCentralIdentityProviderDisplayName(string alias) =>
        (await GetCentralIdentityProviderAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None)).DisplayName;

    public async ValueTask<IdentityProviderConfigOidc> GetCentralIdentityProviderDataOIDCAsync(string alias)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        return new IdentityProviderConfigOidc(
            identityProvider.DisplayName,
            await GetCentralBrokerEndpointOIDCAsync(alias).ConfigureAwait(false),
            identityProvider.Config?.TokenUrl,
            identityProvider.Config?.LogoutUrl,
            identityProvider.Config?.ClientId,
            identityProvider.Config?.ClientSecret,
            identityProvider.Enabled,
            identityProvider.Config?.AuthorizationUrl,
            identityProvider.Config?.ClientAuthMethod == null
                ? null
                : IdentityProviderClientAuthTypeToIamClientAuthMethod(identityProvider.Config.ClientAuthMethod),
            identityProvider.Config?.ClientAssertionSigningAlg == null
                ? null
                : Enum.Parse<IamIdentityProviderSignatureAlgorithm>(identityProvider.Config.ClientAssertionSigningAlg)
        );
    }

    public async ValueTask UpdateSharedIdentityProviderAsync(string alias, string displayName)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        identityProvider.DisplayName = displayName;
        var sharedKeycloak = await GetSharedKeycloakClient(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        await UpdateSharedRealmAsync(sharedKeycloak, alias, displayName, null).ConfigureAwait(false);
        await UpdateCentralIdentityProviderAsync(alias, identityProvider).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async ValueTask UpdateSharedRealmTheme(string alias, string loginTheme)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        var sharedKeycloak = await GetSharedKeycloakClient(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        await UpdateSharedRealmAsync(sharedKeycloak, alias, identityProvider.DisplayName, loginTheme).ConfigureAwait(false);
    }

    public async ValueTask SetSharedIdentityProviderStatusAsync(string alias, bool enabled)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        identityProvider.Enabled = enabled;
        (identityProvider.Config ?? throw new KeycloakInvalidResponseException(NullConfigMessage)).HideOnLoginPage = enabled ? "false" : "true";
        var sharedKeycloak = await GetSharedKeycloakClient(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        await SetSharedRealmStatusAsync(sharedKeycloak, alias, enabled).ConfigureAwait(false);
        await UpdateCentralIdentityProviderAsync(alias, identityProvider).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async ValueTask SetCentralIdentityProviderStatusAsync(string alias, bool enabled)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        identityProvider.Enabled = enabled;
        (identityProvider.Config ?? throw new KeycloakInvalidResponseException(NullConfigMessage)).HideOnLoginPage = enabled ? "false" : "true";
        await UpdateCentralIdentityProviderAsync(alias, identityProvider).ConfigureAwait(ConfigureAwaitOptions.None);
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
                    throw new ArgumentException($"secret must not be null for clientAuthMethod {identityProviderConfigOidc.ClientAuthMethod}");
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
                    throw new ArgumentException($"signatureAlgorithm must not be null for clientAuthMethod {identityProviderConfigOidc.ClientAuthMethod}");
                default:
                    break;
            }
        }
        return UpdateCentralIdentityProviderDataOIDCInternalAsync(identityProviderConfigOidc, cancellationToken);
    }

    private async ValueTask UpdateCentralIdentityProviderDataOIDCInternalAsync(IdentityProviderEditableConfigOidc identityProviderConfigOidc, CancellationToken cancellationToken)
    {
        var (alias, displayName, metadataUrl, clientAuthMethod, clientId, secret, signatureAlgorithm) = identityProviderConfigOidc;
        var identityProvider = await SetIdentityProviderMetadataFromUrlAsync(await GetCentralIdentityProviderAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None), metadataUrl, cancellationToken).ConfigureAwait(false);
        identityProvider.DisplayName = displayName;
        (identityProvider.Config ?? throw new KeycloakInvalidResponseException(NullConfigMessage)).ClientAuthMethod = IamIdentityProviderClientAuthMethodToInternal(clientAuthMethod);
        identityProvider.Config.ClientId = clientId;
        identityProvider.Config.ClientSecret = secret;
        identityProvider.Config.ClientAssertionSigningAlg = signatureAlgorithm?.ToString();
        await UpdateCentralIdentityProviderAsync(alias, identityProvider).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async ValueTask<IdentityProviderConfigSaml> GetCentralIdentityProviderDataSAMLAsync(string alias)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        return new IdentityProviderConfigSaml(
            identityProvider.DisplayName,
            await GetCentralBrokerEndpointSAMLAsync(alias).ConfigureAwait(false),
            identityProvider.Config?.ClientId,
            identityProvider.Enabled,
            identityProvider.Config?.EntityId,
            identityProvider.Config?.SingleSignOnServiceUrl);
    }

    public async ValueTask UpdateCentralIdentityProviderDataSAMLAsync(IdentityProviderEditableConfigSaml identityProviderEditableConfigSaml)
    {
        var (alias, displayName, entityId, singleSignOnServiceUrl) = identityProviderEditableConfigSaml;
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        identityProvider.DisplayName = displayName;
        (identityProvider.Config ?? throw new KeycloakInvalidResponseException(NullConfigMessage)).EntityId = entityId;
        identityProvider.Config.SingleSignOnServiceUrl = singleSignOnServiceUrl;
        await UpdateCentralIdentityProviderAsync(alias, identityProvider).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task<KeycloakClient> GetSharedKeycloakClient(string realm)
    {
        var (clientId, secret) = await GetSharedIdpServiceAccountSecretAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None);
        return _factory.CreateKeycloakClient("shared", clientId, secret);
    }

    private static T Clone<T>(T cloneObject)
        where T : class =>
        JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(cloneObject))!;
}
