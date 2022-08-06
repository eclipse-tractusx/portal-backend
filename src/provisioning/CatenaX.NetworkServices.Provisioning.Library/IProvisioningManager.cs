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

using CatenaX.NetworkServices.Provisioning.Library.Enums;
using CatenaX.NetworkServices.Provisioning.Library.Models;

namespace CatenaX.NetworkServices.Provisioning.Library;

public interface IProvisioningManager
{
    Task<string> GetNextCentralIdentityProviderNameAsync();
    Task<string> GetNextServiceAccountClientIdAsync();
    Task SetupSharedIdpAsync(string idpName, string organisationName);
    Task<string> CreateSharedUserLinkedToCentralAsync(string idpName, UserProfile userProfile);
    Task<IDictionary<string, IEnumerable<string>>> AssignClientRolesToCentralUserAsync(string centralUserId, IDictionary<string,IEnumerable<string>> clientRoleNames);
    Task<IEnumerable<string>> GetClientRolesAsync(string clientId);
    Task<IEnumerable<string>> GetClientRolesCompositeAsync(string clientId);
    Task<string> SetupOwnIdpAsync(string organisationName, string clientId, string metadataUrl, string clientAuthMethod, string? clientSecret);
    Task<string> CreateOwnIdpAsync(string organisationName, IamIdentityProviderProtocol providerProtocol);
    Task<string?> GetProviderUserIdForCentralUserIdAsync(string identityProvider, string userId);
    Task<IEnumerable<IdentityProviderLink>> GetProviderUserLinkDataForCentralUserIdAsync(IEnumerable<string> identityProviders, string userId);
    Task AddProviderUserLinkToCentralUserAsync(string userId, string alias, string providerUserId, string providerUserName);
    Task DeleteProviderUserLinkToCentralUserAsync(string userId, string alias);
    Task<bool> UpdateSharedRealmUserAsync(string realm, string userId, string firstName, string lastName, string email);
    Task<bool> UpdateCentralUserAsync(string userId, string firstName, string lastName, string email);
    Task<bool> DeleteSharedRealmUserAsync(string idpName, string userIdShared);
    Task<bool> DeleteCentralRealmUserAsync(string userIdCentral);
    Task<string> SetupClientAsync(string redirectUrl);
    Task<ServiceAccountData> SetupCentralServiceAccountClientAsync(string clientId, ClientConfigRolesData config);
    Task UpdateCentralClientAsync(string internalClientId, ClientConfigData config);
    Task DeleteCentralClientAsync(string internalClientId);
    Task<ClientAuthData> GetCentralClientAuthDataAsync(string internalClientId);
    Task<ClientAuthData> ResetCentralClientAuthDataAsync(string internalClientId);
    Task AddBpnAttributetoUserAsync(string centralUserId, IEnumerable<string> bpns);
    Task DeleteCentralUserBusinessPartnerNumberAsync(string centralUserId,string businessPartnerNumber);
    Task<bool> ResetSharedUserPasswordAsync(string realm, string userId);
    Task<IEnumerable<string>> GetClientRoleMappingsForUserAsync(string userId, string clientId);
    Task<IdentityProviderConfigOidc> GetCentralIdentityProviderDataOIDCAsync(string alias);
    Task UpdateCentralIdentityProviderDataOIDCAsync(IdentityProviderEditableConfigOidc identityProviderConfigOidc);
    Task<IdentityProviderConfigSaml> GetCentralIdentityProviderDataSAMLAsync(string alias);
    Task UpdateCentralIdentityProviderDataSAMLAsync(IdentityProviderEditableConfigSaml identityProviderEditableConfigSaml);
    Task DeleteCentralIdentityProviderAsync(string alias);
}
