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

using CatenaX.NetworkServices.Keycloak.ErrorHandling;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Keycloak.Library;
using CatenaX.NetworkServices.Keycloak.Library.Models.Users;
using System.Text.Json;

namespace CatenaX.NetworkServices.Provisioning.Library;

public partial class ProvisioningManager
{
    public async Task UpdateSharedRealmUserAsync(string realm, string userId, string firstName, string lastName, string email)
    {
        var sharedKeycloak = await GetSharedKeycloakClient(realm).ConfigureAwait(false);
        var user = await sharedKeycloak.GetUserAsync(realm, userId).ConfigureAwait(false);
        user.FirstName = firstName;
        user.LastName = lastName;
        user.Email = email;
        await sharedKeycloak.UpdateUserAsync(realm, userId, user).ConfigureAwait(false);
    }

    public async Task UpdateCentralUserAsync(string userId, string firstName, string lastName, string email)
    {
        var user = await _CentralIdp.GetUserAsync(_Settings.CentralRealm, userId).ConfigureAwait(false);
        if (user.FirstName != firstName || user.LastName != lastName || user.Email != email)
        {
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;
            await _CentralIdp.UpdateUserAsync(_Settings.CentralRealm, userId, user).ConfigureAwait(false);
        }
    }

    public async Task DeleteSharedRealmUserAsync(string realm, string userId)
    {
        var sharedKeycloak = await GetSharedKeycloakClient(realm).ConfigureAwait(false);
        await sharedKeycloak.DeleteUserAsync(realm, userId).ConfigureAwait(false);
    }

    public Task DeleteCentralRealmUserAsync(string userId) =>
        _CentralIdp.DeleteUserAsync(_Settings.CentralRealm, userId);

    public async Task<string?> GetProviderUserIdForCentralUserIdAsync(string identityProvider, string userId) =>
        (await _CentralIdp.GetUserSocialLoginsAsync(_Settings.CentralRealm, userId).ConfigureAwait(false))
            .SingleOrDefault(federatedIdentity => federatedIdentity.IdentityProvider == identityProvider)
            ?.UserId;

    public async IAsyncEnumerable<IdentityProviderLink> GetProviderUserLinkDataForCentralUserIdAsync(string userId)
    {
        foreach (var federatedIdentity in await _CentralIdp.GetUserSocialLoginsAsync(_Settings.CentralRealm, userId).ConfigureAwait(false))
        {
            yield return new IdentityProviderLink(
                federatedIdentity.IdentityProvider,
                federatedIdentity.UserId,
                federatedIdentity.UserName);
        }
    }

    public Task AddProviderUserLinkToCentralUserAsync(string userId, IdentityProviderLink identityProviderLink) =>
        _CentralIdp.AddUserSocialLoginProviderAsync(
            _Settings.CentralRealm,
            userId,
            identityProviderLink.Alias,
            new FederatedIdentity()
            {
                IdentityProvider = identityProviderLink.Alias,
                UserId = identityProviderLink.UserId,
                UserName = identityProviderLink.UserName
            });

    public Task DeleteProviderUserLinkToCentralUserAsync(string userId, string alias) =>
        _CentralIdp.RemoveUserSocialLoginProviderAsync(
            _Settings.CentralRealm,
            userId,
            alias);

    private async Task<string> CreateSharedRealmUserAsync(KeycloakClient keycloak, string realm, UserProfile profile)
    {
        var newUser = CloneUser(_Settings.SharedUser);
        newUser.UserName = profile.UserName;
        newUser.FirstName = profile.FirstName;
        newUser.LastName = profile.LastName;
        newUser.Email = profile.Email;
        newUser.Credentials ??= profile.Password == null ? null : Enumerable.Repeat( new Credentials { Type = "Password", Value = profile.Password }, 1);
        var newUserId = await keycloak.CreateAndRetrieveUserIdAsync(realm, newUser).ConfigureAwait(false);
        if (newUserId == null)
        {
            throw new KeycloakNoSuccessException($"failed to created shared user {profile.UserName} in realm {realm}");
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
        if (!string.IsNullOrWhiteSpace(profile.BusinessPartnerNumber))
        {
            newUser.Attributes[_Settings.MappedBpnAttribute] = Enumerable.Repeat<string>(profile.BusinessPartnerNumber,1);
        }
        var newUserId = await _CentralIdp.CreateAndRetrieveUserIdAsync(_Settings.CentralRealm, newUser).ConfigureAwait(false);
        if (newUserId == null)
        {
            throw new KeycloakNoSuccessException($"failed to created central user {profile.UserName} for identityprovider {alias}, organisation {profile.OrganisationName}");
        }
        return newUserId;
    }

    private Task LinkCentralSharedRealmUserAsync(string alias, string centralUserId, string sharedUserId, string sharedUserName) =>
        _CentralIdp.AddUserSocialLoginProviderAsync(_Settings.CentralRealm, centralUserId, alias, new FederatedIdentity {
            IdentityProvider = alias,
            UserId = sharedUserId,
            UserName = sharedUserName
        });

    private User CloneUser(User user) =>
        JsonSerializer.Deserialize<User>(JsonSerializer.Serialize(user))!;
}
