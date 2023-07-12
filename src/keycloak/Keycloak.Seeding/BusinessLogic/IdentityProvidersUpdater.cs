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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class IdentityProvidersUpdater : IIdentityProvidersUpdater
{
    private readonly IKeycloakFactory _keycloakFactory;
    private readonly ISeedDataHandler _seedData;

    public IdentityProvidersUpdater(IKeycloakFactory keycloakFactory, ISeedDataHandler seedDataHandler)
    {
        _keycloakFactory = keycloakFactory;
        _seedData = seedDataHandler;
    }

    public async Task UpdateIdentityProviders(string keycloakInstanceName)
    {
        var keycloak = _keycloakFactory.CreateKeycloakClient(keycloakInstanceName);
        var realm = _seedData.Realm;

        foreach (var updateIdentityProvider in _seedData.IdentityProviders)
        {
            if (updateIdentityProvider.Alias == null)
                throw new ConflictException($"identityProvider alias must not be null: {updateIdentityProvider.InternalId} {updateIdentityProvider.DisplayName}");

            try
            {
                var identityProvider = await keycloak.GetIdentityProviderAsync(realm, updateIdentityProvider.Alias).ConfigureAwait(false);
                UpdateIdentityProvider(identityProvider, updateIdentityProvider);
                await keycloak.UpdateIdentityProviderAsync(realm, updateIdentityProvider.Alias, identityProvider).ConfigureAwait(false);
            }
            catch (KeycloakEntityNotFoundException)
            {
                var identityProvider = new Library.Models.IdentityProviders.IdentityProvider();
                UpdateIdentityProvider(identityProvider, updateIdentityProvider);
                await keycloak.CreateIdentityProviderAsync(realm, identityProvider).ConfigureAwait(false);
            }

            var updateMappers = _seedData.IdentityProviderMappers.Where(x => x.IdentityProviderAlias == updateIdentityProvider.Alias);
            var mappers = await keycloak.GetIdentityProviderMappersAsync(realm, updateIdentityProvider.Alias).ConfigureAwait(false);

            // create missing mappers
            foreach (var mapper in updateMappers.ExceptBy(mappers.Select(x => x.Name), x => x.Name))
            {
                await keycloak.AddIdentityProviderMapperAsync(
                    realm,
                    updateIdentityProvider.Alias,
                    UpdateIdentityProviderMapper(
                        new Library.Models.IdentityProviders.IdentityProviderMapper
                        {
                            Name = mapper.Name,
                            IdentityProviderAlias = mapper.IdentityProviderAlias
                        },
                        mapper)).ConfigureAwait(false);
            }

            // update existing mappers
            foreach (var x in mappers.Join(
                updateMappers,
                x => x.Name,
                x => x.Name,
                (mapper, update) => (Mapper: mapper, Update: update)))
            {
                var (mapper, update) = x;
                await keycloak.UpdateIdentityProviderMapperAsync(
                    realm,
                    updateIdentityProvider.Alias,
                    mapper.Id ?? throw new ConflictException($"identityProviderMapper.id must never be null {mapper.Name} {mapper.IdentityProviderAlias}"),
                    UpdateIdentityProviderMapper(mapper, update)).ConfigureAwait(false);
            }

            // delete redundant mappers
            foreach (var mapper in mappers.ExceptBy(updateMappers.Select(x => x.Name), x => x.Name))
            {
                await keycloak.DeleteIdentityProviderMapperAsync(
                    realm,
                    updateIdentityProvider.Alias,
                    mapper.Id ?? throw new ConflictException($"identityProviderMapper.id must never be null {mapper.Name} {mapper.IdentityProviderAlias}")).ConfigureAwait(false);
            }
        }
    }

    private static Library.Models.IdentityProviders.IdentityProvider UpdateIdentityProvider(Library.Models.IdentityProviders.IdentityProvider provider, IdentityProviderModel update)
    {
        provider.Alias = update.Alias;
        provider.DisplayName = update.DisplayName;
        provider.ProviderId = update.ProviderId;
        provider.Enabled = update.Enabled;
        provider.UpdateProfileFirstLoginMode = update.UpdateProfileFirstLoginMode;
        provider.TrustEmail = update.TrustEmail;
        provider.StoreToken = update.StoreToken;
        provider.AddReadTokenRoleOnCreate = update.AddReadTokenRoleOnCreate;
        provider.AuthenticateByDefault = update.AuthenticateByDefault;
        provider.LinkOnly = update.LinkOnly;
        provider.FirstBrokerLoginFlowAlias = update.FirstBrokerLoginFlowAlias;
        provider.Config = update.Config == null
            ? null
            : new Library.Models.IdentityProviders.Config
            {
                HideOnLoginPage = update.Config.HideOnLoginPage,
                //ClientSecret = update.Config.ClientSecret,
                DisableUserInfo = update.Config.DisableUserInfo,
                ValidateSignature = update.Config.ValidateSignature,
                ClientId = update.Config.ClientId,
                TokenUrl = update.Config.TokenUrl,
                AuthorizationUrl = update.Config.AuthorizationUrl,
                ClientAuthMethod = update.Config.ClientAuthMethod,
                JwksUrl = update.Config.JwksUrl,
                LogoutUrl = update.Config.LogoutUrl,
                ClientAssertionSigningAlg = update.Config.ClientAssertionSigningAlg,
                SyncMode = update.Config.SyncMode,
                UseJwksUrl = update.Config.UseJwksUrl,
                UserInfoUrl = update.Config.UserInfoUrl,
                Issuer = update.Config.Issuer,
                // for Saml:
                NameIDPolicyFormat = update.Config.NameIDPolicyFormat,
                PrincipalType = update.Config.PrincipalType,
                SignatureAlgorithm = update.Config.SignatureAlgorithm,
                XmlSigKeyInfoKeyNameTransformer = update.Config.XmlSigKeyInfoKeyNameTransformer,
                AllowCreate = update.Config.AllowCreate,
                EntityId = update.Config.EntityId,
                AuthnContextComparisonType = update.Config.AuthnContextComparisonType,
                BackchannelSupported = update.Config.BackchannelSupported,
                PostBindingResponse = update.Config.PostBindingResponse,
                PostBindingAuthnRequest = update.Config.PostBindingAuthnRequest,
                PostBindingLogout = update.Config.PostBindingLogout,
                WantAuthnRequestsSigned = update.Config.WantAuthnRequestsSigned,
                WantAssertionsSigned = update.Config.WantAssertionsSigned,
                WantAssertionsEncrypted = update.Config.WantAssertionsEncrypted,
                ForceAuthn = update.Config.ForceAuthn,
                SignSpMetadata = update.Config.SignSpMetadata,
                LoginHint = update.Config.LoginHint,
                SingleSignOnServiceUrl = update.Config.SingleSignOnServiceUrl,
                AllowedClockSkew = update.Config.AllowedClockSkew,
                AttributeConsumingServiceIndex = update.Config.AttributeConsumingServiceIndex
            };
        return provider;
    }

    private static Library.Models.IdentityProviders.IdentityProviderMapper UpdateIdentityProviderMapper(Library.Models.IdentityProviders.IdentityProviderMapper mapper, IdentityProviderMapperModel updateMapper)
    {
        mapper._IdentityProviderMapper = updateMapper.IdentityProviderMapper;
        mapper.Config = updateMapper.Config?.ToDictionary(x => x.Key, x => x.Value);
        return mapper;
    }
}
