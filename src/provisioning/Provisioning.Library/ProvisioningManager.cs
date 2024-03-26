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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
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

    public async ValueTask DeleteSharedIdpRealmAsync(string alias)
    {
        var deleteSharedKeycloak = await GetSharedKeycloakClient(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        await deleteSharedKeycloak.DeleteRealmAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        var sharedKeycloak = _Factory.CreateKeycloakClient("shared");
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
        var user = await _CentralIdp.GetUserAsync(_Settings.CentralRealm, centralUserId).ConfigureAwait(ConfigureAwaitOptions.None);
        user.Attributes ??= new Dictionary<string, IEnumerable<string>>();
        user.Attributes[_Settings.MappedBpnAttribute] = user.Attributes.TryGetValue(_Settings.MappedBpnAttribute, out var existingBpns)
            ? existingBpns.Concat(bpns).Distinct()
            : bpns;
        await _CentralIdp.UpdateUserAsync(_Settings.CentralRealm, centralUserId.ToString(), user).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public Task AddProtocolMapperAsync(string clientId) =>
        _CentralIdp.CreateClientProtocolMapperAsync(
            _Settings.CentralRealm,
            clientId,
            Clone(_Settings.ClientProtocolMapper));

    public async Task DeleteCentralUserBusinessPartnerNumberAsync(string centralUserId, string businessPartnerNumber)
    {
        var user = await _CentralIdp.GetUserAsync(_Settings.CentralRealm, centralUserId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (user.Attributes == null || !user.Attributes.TryGetValue(_Settings.MappedBpnAttribute, out var existingBpns))
        {
            throw new KeycloakEntityNotFoundException($"attribute {_Settings.MappedBpnAttribute} not found in the mappers of user {centralUserId}");
        }

        user.Attributes[_Settings.MappedBpnAttribute] = existingBpns.Where(bpn => bpn != businessPartnerNumber);

        await _CentralIdp.UpdateUserAsync(_Settings.CentralRealm, centralUserId, user).ConfigureAwait(ConfigureAwaitOptions.None);
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
        return (await _CentralIdp.GetClientRoleMappingsForUserAsync(_Settings.CentralRealm, userId, idOfClient).ConfigureAwait(ConfigureAwaitOptions.None))?
            .Where(r => r.Composite.HasValue && r.Composite.Value).Select(x => x.Name ?? throw new KeycloakInvalidResponseException("name of role is null")) ?? throw new KeycloakInvalidResponseException();
    }

    public async ValueTask<bool> IsCentralIdentityProviderEnabled(string alias)
    {
        return (await GetCentralIdentityProviderAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None)).Enabled ?? false;
    }

    private static readonly string NullDisplayNameMessage = "display_name of identityProvider is null";
    private static readonly string NullConfigMessage = "config of identityProvider is null";

    public async Task<string> GetCentralIdentityProviderDisplayName(string alias) =>
        (await GetCentralIdentityProviderAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None)).DisplayName ?? throw new KeycloakInvalidResponseException(NullDisplayNameMessage);

    public async ValueTask<IdentityProviderConfigOidc> GetCentralIdentityProviderDataOIDCAsync(string alias)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        var redirectUri = await GetCentralBrokerEndpointOIDCAsync(alias).ConfigureAwait(false);
        return new IdentityProviderConfigOidc(
            identityProvider.DisplayName ?? throw new KeycloakInvalidResponseException(NullDisplayNameMessage),
            redirectUri,
            (identityProvider.Config ?? throw new KeycloakInvalidResponseException(NullConfigMessage)).TokenUrl ?? throw new KeycloakInvalidResponseException("token_url of identityProvider is null"),
            identityProvider.Config.LogoutUrl,
            identityProvider.Config.ClientId ?? throw new KeycloakInvalidResponseException("client_id of identityProvider is null"),
            identityProvider.Config.ClientSecret,
            identityProvider.Enabled ?? false,
            identityProvider.Config.AuthorizationUrl ?? throw new KeycloakInvalidResponseException("authorization_url of identityProvider is null"),
            IdentityProviderClientAuthTypeToIamClientAuthMethod(identityProvider.Config.ClientAuthMethod ?? throw new KeycloakInvalidResponseException("client_auth_method of identityProvider is null")),
            identityProvider.Config.ClientAssertionSigningAlg == null ? null : Enum.Parse<IamIdentityProviderSignatureAlgorithm>(identityProvider.Config.ClientAssertionSigningAlg));
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
        await UpdateSharedRealmAsync(sharedKeycloak, alias, identityProvider.DisplayName ?? throw new KeycloakInvalidResponseException(NullDisplayNameMessage), loginTheme).ConfigureAwait(false);
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
        var redirectUri = await GetCentralBrokerEndpointSAMLAsync(alias).ConfigureAwait(false);
        return new IdentityProviderConfigSaml(
            identityProvider.DisplayName ?? throw new KeycloakInvalidResponseException(NullDisplayNameMessage),
            redirectUri ?? throw new KeycloakInvalidResponseException("same endpoint of identityProvider is null"),
            (identityProvider.Config ?? throw new KeycloakInvalidResponseException(NullConfigMessage)).ClientId ?? throw new KeycloakInvalidResponseException("client_id of identityProvider is null"),
            identityProvider.Enabled ?? false,
            identityProvider.Config.EntityId ?? throw new KeycloakInvalidResponseException("entity_id of identityProvider is null"),
            identityProvider.Config.SingleSignOnServiceUrl ?? throw new KeycloakInvalidResponseException("single_sign_on_service_url of identityProvider is null"));
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
        return _Factory.CreateKeycloakClient("shared", clientId, secret);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> TriggerDeleteSharedRealmAsync(string alias)
    {
        try
        {
            var deleteSharedKeycloak = await GetSharedKeycloakClient(alias).ConfigureAwait(false);
            await deleteSharedKeycloak.DeleteRealmAsync(alias).ConfigureAwait(false);
            return (new[] { ProcessStepTypeId.TRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT }, ProcessStepStatusId.DONE, false, null);
        }
        catch (KeycloakEntityNotFoundException)
        {
            return (new[] { ProcessStepTypeId.TRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT }, ProcessStepStatusId.DONE, false, "Entity not found");
        }
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> TriggerDeleteIdpSharedServiceAccount(string alias)
    {
        try
        {
            var sharedKeycloak = _Factory.CreateKeycloakClient("shared");
            await DeleteSharedIdpServiceAccountAsync(sharedKeycloak, alias);
            return (new[] { ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_LINKED_USERS }, ProcessStepStatusId.DONE, false, null);
        }
        catch (KeycloakEntityNotFoundException)
        {
            return (new[] { ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_LINKED_USERS }, ProcessStepStatusId.DONE, false, "Entity not found");
        }
    }

    private static T Clone<T>(T cloneObject)
        where T : class =>
        JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(cloneObject))!;
}
