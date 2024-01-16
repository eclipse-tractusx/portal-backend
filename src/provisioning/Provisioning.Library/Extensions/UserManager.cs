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

using Flurl.Http;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

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

    public async Task<string?> GetUserByUserName(string userName)
    {
        try
        {
            return (await _CentralIdp.GetUsersAsync(_Settings.CentralRealm, username: userName).ConfigureAwait(false)).SingleOrDefault(user => user.UserName == userName)?.Id;
        }
        catch (FlurlHttpException ex)
        {
            if (ex.StatusCode == 404)
            {
                return null;
            }

            throw;
        }
        catch (InvalidOperationException)
        {
            throw new UnexpectedConditionException($"there should never be multiple users in keycloak having the same username '{userName}'");
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

    public async Task<string> CreateSharedRealmUserAsync(string realm, UserProfile profile)
    {
        var sharedKeycloak = await GetSharedKeycloakClient(realm).ConfigureAwait(false);
        var newUser = CloneUser(_Settings.SharedUser);
        newUser.UserName = profile.UserName;
        newUser.FirstName = profile.FirstName;
        newUser.LastName = profile.LastName;
        newUser.Email = profile.Email;
        newUser.Credentials ??= profile.Password == null ? null : Enumerable.Repeat(new Credentials { Type = "Password", Value = profile.Password }, 1);
        return await CreateAndRetrieveUserIdMappingError(sharedKeycloak, realm, newUser).ConfigureAwait(false);
    }

    public Task<string> CreateCentralUserAsync(UserProfile profile, IEnumerable<(string Name, IEnumerable<string> Values)> attributes)
    {
        var newUser = CloneUser(_Settings.CentralUser);
        newUser.UserName = profile.UserName;
        newUser.FirstName = profile.FirstName;
        newUser.LastName = profile.LastName;
        newUser.Email = profile.Email;
        newUser.Enabled = profile.Enabled;
        if (attributes.Any())
        {
            newUser.Attributes = attributes.Where(a => a.Values.Any()).ToDictionary(a => a.Name, a => a.Values);
        }
        return CreateAndRetrieveUserIdMappingError(_CentralIdp, _Settings.CentralRealm, newUser);
    }

    private static async Task<string> CreateAndRetrieveUserIdMappingError(Keycloak.Library.KeycloakClient keycloak, string realm, User newUser)
    {
        if (newUser.UserName == null)
            throw ControllerArgumentException.Create(ProvisioningServiceErrors.USER_CREATION_USERNAME_NULL, new ErrorParameter[] { new("userName", "null"), new("realm", realm) });

        string? newUserId;
        try
        {
            newUserId = await keycloak.CreateAndRetrieveUserIdAsync(realm, newUser).ConfigureAwait(false);
        }
        catch (Exception error)
        {
            throw error switch
            {
                KeycloakEntityConflictException => ConflictException.Create(ProvisioningServiceErrors.USER_CREATION_CONFLICT, new ErrorParameter[] { new("userName", newUser.UserName), new("realm", realm) }, error),
                KeycloakEntityNotFoundException => NotFoundException.Create(ProvisioningServiceErrors.USER_CREATION_NOTFOUND, new ErrorParameter[] { new("userName", newUser.UserName), new("realm", realm) }, error),
                ArgumentException => ServiceException.Create(ProvisioningServiceErrors.USER_CREATION_ARGUMENT, new ErrorParameter[] { new("userName", newUser.UserName), new("realm", realm) }, error),
                ServiceException serviceException => ServiceException.Create(ProvisioningServiceErrors.USER_CREATION_FAILURE, new ErrorParameter[] { new("userName", newUser.UserName), new("realm", realm) }, serviceException.StatusCode, serviceException.IsRecoverable, error),
                _ => ServiceException.Create(ProvisioningServiceErrors.USER_CREATION_FAILURE, new ErrorParameter[] { new("userName", newUser.UserName), new("realm", realm) }, error)
            };
        }
        if (newUserId == null)
        {
            throw ServiceException.Create(ProvisioningServiceErrors.USER_CREATION_RETURNS_NULL, new ErrorParameter[] { new("userName", newUser.UserName), new("realm", realm) });
        }
        return newUserId;
    }

    private static User CloneUser(User user) =>
        JsonSerializer.Deserialize<User>(JsonSerializer.Serialize(user))!;
}
