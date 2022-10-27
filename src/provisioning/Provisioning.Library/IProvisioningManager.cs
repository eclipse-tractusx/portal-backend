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

using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Enums;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Models;

namespace Org.CatenaX.Ng.Portal.Backend.Provisioning.Library;

public interface IProvisioningManager
{
    ValueTask<string> GetNextCentralIdentityProviderNameAsync();
    Task<string> GetNextServiceAccountClientIdAsync();
    Task SetupSharedIdpAsync(string idpName, string organisationName);
    Task<string> CreateSharedUserLinkedToCentralAsync(string idpName, UserProfile userProfile, IEnumerable<(string Name, IEnumerable<string> Values)> attributes);
    Task<string> CreateSharedRealmUserAsync(string realm, UserProfile profile);
    Task<string> CreateCentralUserAsync(UserProfile profile, IEnumerable<(string Name, IEnumerable<string> Values)> attributes);
    IAsyncEnumerable<(string Client, IEnumerable<string> Roles)> AssignClientRolesToCentralUserAsync(string centralUserId, IDictionary<string, IEnumerable<string>> clientRoleNames);
    Task<string> CreateOwnIdpAsync(string organisationName, IamIdentityProviderProtocol providerProtocol);
    Task<string?> GetProviderUserIdForCentralUserIdAsync(string identityProvider, string userId);
    IAsyncEnumerable<IdentityProviderLink> GetProviderUserLinkDataForCentralUserIdAsync(string userId);
    Task AddProviderUserLinkToCentralUserAsync(string userId, IdentityProviderLink identityProviderLink);
    Task DeleteProviderUserLinkToCentralUserAsync(string userId, string alias);
    Task UpdateSharedRealmUserAsync(string realm, string userId, string firstName, string lastName, string email);
    Task UpdateCentralUserAsync(string userId, string firstName, string lastName, string email);
    Task DeleteSharedRealmUserAsync(string realm, string userId);
    Task DeleteCentralRealmUserAsync(string userId);
    Task<string> SetupClientAsync(string redirectUrl, IEnumerable<string>? optionalRoleNames = null);
    Task<ServiceAccountData> SetupCentralServiceAccountClientAsync(string clientId, ClientConfigRolesData config);
    Task UpdateCentralClientAsync(string internalClientId, ClientConfigData config);
    Task DeleteCentralClientAsync(string internalClientId);
    Task<ClientAuthData> GetCentralClientAuthDataAsync(string internalClientId);
    Task<ClientAuthData> ResetCentralClientAuthDataAsync(string internalClientId);
    Task AddBpnAttributetoUserAsync(string centralUserId, IEnumerable<string> bpns);
    Task AddProtocolMapperAsync(string clientId);
    Task DeleteCentralUserBusinessPartnerNumberAsync(string centralUserId,string businessPartnerNumber);
    Task ResetSharedUserPasswordAsync(string realm, string userId);
    Task<IEnumerable<string>> GetClientRoleMappingsForUserAsync(string userId, string clientId);
    ValueTask<bool> IsCentralIdentityProviderEnabled(string alias);
    ValueTask<IdentityProviderConfigOidc> GetCentralIdentityProviderDataOIDCAsync(string alias);
    ValueTask SetSharedIdentityProviderStatusAsync(string alias, bool enabled);
    ValueTask SetCentralIdentityProviderStatusAsync(string alias, bool enabled);
    ValueTask UpdateSharedIdentityProviderAsync(string alias, string displayName);
    ValueTask UpdateCentralIdentityProviderDataOIDCAsync(IdentityProviderEditableConfigOidc identityProviderConfigOidc);
    ValueTask<IdentityProviderConfigSaml> GetCentralIdentityProviderDataSAMLAsync(string alias);
    ValueTask UpdateCentralIdentityProviderDataSAMLAsync(IdentityProviderEditableConfigSaml identityProviderEditableConfigSaml);
    Task DeleteCentralIdentityProviderAsync(string alias);
    IAsyncEnumerable<IdentityProviderMapperModel> GetIdentityProviderMappers(string alias);
    ValueTask DeleteSharedIdpRealmAsync(string alias);
    IEnumerable<(string AttributeName,IEnumerable<string> AttributeValues)> GetStandardAttributes(string? alias = null, string? organisationName = null, string? businessPartnerNumber = null);
    Task DeleteClientRolesFromCentralUserAsync(string centralUserId, IDictionary<string, IEnumerable<string>> clientRoleNames);
}
