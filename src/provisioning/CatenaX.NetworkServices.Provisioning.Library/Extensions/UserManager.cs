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

using CatenaX.NetworkServices.Provisioning.Library.Models;
using Keycloak.Net.Models.Users;
using System.Text.Json;

namespace CatenaX.NetworkServices.Provisioning.Library;

public partial class ProvisioningManager
{
    public async Task<bool> UpdateSharedRealmUserAsync(string realm, string userId, string firstName, string lastName, string email)
    {
        var user = await _SharedIdp.GetUserAsync(realm, userId).ConfigureAwait(false);
        user.FirstName = firstName;
        user.LastName = lastName;
        user.Email = email;
        return await _SharedIdp.UpdateUserAsync(realm, userId, user).ConfigureAwait(false);
    }

    public Task<bool> DeleteSharedRealmUserAsync(string realm, string userId) =>
        _SharedIdp.DeleteUserAsync(realm, userId);

    public Task<bool> DeleteCentralRealmUserAsync(string userId) =>
        _CentralIdp.DeleteUserAsync(_Settings.CentralRealm, userId);

    public async Task<string?> GetProviderUserIdForCentralUserIdAsync(string identityProvider, string userId) =>
        (await _CentralIdp.GetUserSocialLoginsAsync(_Settings.CentralRealm, userId).ConfigureAwait(false))
            .Where(federatedIdentity => federatedIdentity.IdentityProvider == identityProvider)
            .SingleOrDefault()?.UserId;

    public async Task<IEnumerable<(string Alias, string UserId, string UserName)>> GetProviderUserLinkDataForCentralUserIdAsync(IEnumerable<string> identityProviders, string userId) =>
        (await _CentralIdp.GetUserSocialLoginsAsync(_Settings.CentralRealm, userId).ConfigureAwait(false))
            .Where(federatedIdentity => identityProviders.Any(identityProvider => federatedIdentity.IdentityProvider == identityProvider))
            .Select(federatedIdentity =>
                ((string Alias, string UserId, string UserName)) new (
                    federatedIdentity.IdentityProvider,
                    federatedIdentity.UserId,
                    federatedIdentity.UserName));

    public Task AddProviderUserLinkToCentralUserAsync(string userId, string alias, string providerUserId, string providerUserName) =>
        _CentralIdp.AddUserSocialLoginProviderAsync(
            _Settings.CentralRealm,
            userId,
            alias,
            new FederatedIdentity()
            {
                IdentityProvider = alias,
                UserId = providerUserId,
                UserName = providerUserName
            });

    public Task DeleteProviderUserLinkToCentralUserAsync(string userId, string alias) =>
        _CentralIdp.RemoveUserSocialLoginProviderAsync(
            _Settings.CentralRealm,
            userId,
            alias);

    private async Task<string> CreateSharedRealmUserAsync(string realm, UserProfile profile)
    {
        var newUser = CloneUser(_Settings.SharedUser);
        newUser.UserName = profile.UserName;
        newUser.FirstName = profile.FirstName;
        newUser.LastName = profile.LastName;
        newUser.Email = profile.Email;
        newUser.Credentials ??= profile.Password == null ? null : Enumerable.Repeat( new Credentials { Type = "Password", Value = profile.Password }, 1);
        var newUserId = await _SharedIdp.CreateAndRetrieveUserIdAsync(realm, newUser).ConfigureAwait(false);
        if (newUserId == null)
        {
            throw new Exception($"failed to created shared user {profile.UserName} in realm {realm}");
        }
        return newUserId;
    }

    private async Task<string> CreateCentralUserAsync(string alias, UserProfile profile)
    {
        var newUser = CloneUser(_Settings.CentralUser);
        newUser.UserName = profile.UserName;
        newUser.FirstName = profile.FirstName;
        newUser.LastName = profile.LastName;
        newUser.Email = profile.Email;
        newUser.Attributes ??= new Dictionary<string,IEnumerable<string>>();
        newUser.Attributes[_Settings.MappedIdpAttribute] = Enumerable.Repeat<string>(alias,1);
        newUser.Attributes[_Settings.MappedCompanyAttribute] = Enumerable.Repeat<string>(profile.OrganisationName,1);
        if (!String.IsNullOrWhiteSpace(profile.BusinessPartnerNumber))
        {
            newUser.Attributes[_Settings.MappedBpnAttribute] = Enumerable.Repeat<string>(profile.BusinessPartnerNumber,1);
        }
        var newUserId = await _CentralIdp.CreateAndRetrieveUserIdAsync(_Settings.CentralRealm, newUser).ConfigureAwait(false);
        if (newUserId == null)
        {
            throw new Exception($"failed to created central user {profile.UserName} for identityprovider {alias}, organisation {profile.OrganisationName}");
        }
        return newUserId;
    }

    private async Task LinkCentralSharedRealmUserAsync(string alias, string centralUserId, string sharedUserId, string sharedUserName)
    {
        if (! await _CentralIdp.AddUserSocialLoginProviderAsync(_Settings.CentralRealm, centralUserId, alias, new FederatedIdentity {
            IdentityProvider = alias,
            UserId = sharedUserId,
            UserName = sharedUserName
        }).ConfigureAwait(false))
        {
            throw new Exception($"failed to create link in between central user {centralUserId} and shared realm {alias} user {sharedUserId}");
        }
    }

    private User CloneUser(User user) =>
        JsonSerializer.Deserialize<User>(JsonSerializer.Serialize(user))!;
}
