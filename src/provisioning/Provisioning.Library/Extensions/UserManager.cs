/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
        var sharedKeycloak = await GetSharedKeycloakClient(realm).ConfigureAwait(ConfigureAwaitOptions.None);
        var user = await sharedKeycloak.GetUserAsync(realm, userId).ConfigureAwait(ConfigureAwaitOptions.None);
        user.FirstName = firstName;
        user.LastName = lastName;
        user.Email = email;
        await sharedKeycloak.UpdateUserAsync(realm, userId, user).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task UpdateCentralUserAsync(string userId, string firstName, string lastName, string email)
    {
        var user = await _centralIdp.GetUserAsync(_settings.CentralRealm, userId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (user.FirstName != firstName || user.LastName != lastName || user.Email != email)
        {
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;
            await _centralIdp.UpdateUserAsync(_settings.CentralRealm, userId, user).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    public async Task<string?> GetUserByUserName(string userName)
    {
        try
        {
            return (await _centralIdp.GetUsersAsync(_settings.CentralRealm, username: userName).ConfigureAwait(ConfigureAwaitOptions.None)).SingleOrDefault(user => user.UserName == userName)?.Id;
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
        var sharedKeycloak = await GetSharedKeycloakClient(realm).ConfigureAwait(ConfigureAwaitOptions.None);
        await sharedKeycloak.DeleteUserAsync(realm, userId).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public Task DeleteCentralRealmUserAsync(string userId) =>
        _centralIdp.DeleteUserAsync(_settings.CentralRealm, userId);

    public async Task<string?> GetProviderUserIdForCentralUserIdAsync(string identityProvider, string userId) =>
        (await _centralIdp.GetUserSocialLoginsAsync(_settings.CentralRealm, userId).ConfigureAwait(ConfigureAwaitOptions.None))
            .SingleOrDefault(federatedIdentity => federatedIdentity.IdentityProvider == identityProvider)
            ?.UserId;

    public async IAsyncEnumerable<IdentityProviderLink> GetProviderUserLinkDataForCentralUserIdAsync(string userId)
    {
        foreach (var federatedIdentity in await _centralIdp.GetUserSocialLoginsAsync(_settings.CentralRealm, userId).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            yield return new IdentityProviderLink(
                federatedIdentity.IdentityProvider ?? throw new KeycloakInvalidResponseException("identity_provider of ferderated_identity is null"),
                federatedIdentity.UserId ?? throw new KeycloakInvalidResponseException("user_id of ferderated_identity is null"),
                federatedIdentity.UserName ?? throw new KeycloakInvalidResponseException("user_name of ferderated_identity is null"));
        }
    }

    public Task AddProviderUserLinkToCentralUserAsync(string userId, IdentityProviderLink identityProviderLink) =>
        _centralIdp.AddUserSocialLoginProviderAsync(
            _settings.CentralRealm,
            userId,
            identityProviderLink.Alias,
            new FederatedIdentity()
            {
                IdentityProvider = identityProviderLink.Alias,
                UserId = identityProviderLink.UserId,
                UserName = identityProviderLink.UserName
            });

    public Task DeleteProviderUserLinkToCentralUserAsync(string userId, string alias) =>
        _centralIdp.RemoveUserSocialLoginProviderAsync(
            _settings.CentralRealm,
            userId,
            alias);

    public async Task<string> CreateSharedRealmUserAsync(string realm, UserProfile profile)
    {
        var sharedKeycloak = await GetSharedKeycloakClient(realm).ConfigureAwait(ConfigureAwaitOptions.None);
        var newUser = CloneUser(_settings.SharedUser);
        newUser.UserName = profile.UserName;
        newUser.FirstName = profile.FirstName;
        newUser.LastName = profile.LastName;
        newUser.Email = profile.Email;
        newUser.Credentials ??= profile.Password == null ? null : Enumerable.Repeat(new Credentials { Type = "Password", Value = profile.Password }, 1);
        return await CreateAndRetrieveUserIdMappingError(sharedKeycloak, realm, newUser).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public Task<string> CreateCentralUserAsync(UserProfile profile, IEnumerable<(string Name, IEnumerable<string> Values)> attributes)
    {
        var newUser = CloneUser(_settings.CentralUser);
        newUser.UserName = profile.UserName;
        newUser.FirstName = profile.FirstName;
        newUser.LastName = profile.LastName;
        newUser.Email = profile.Email;
        newUser.Enabled = profile.Enabled;
        if (attributes.Any())
        {
            newUser.Attributes = attributes.Where(a => a.Values.Any()).ToDictionary(a => a.Name, a => a.Values);
        }
        return CreateAndRetrieveUserIdMappingError(_centralIdp, _settings.CentralRealm, newUser);
    }

    private static readonly string ParamUserName = "userName";
    private static readonly string ParamRealm = "realm";
    private static async Task<string> CreateAndRetrieveUserIdMappingError(Keycloak.Library.KeycloakClient keycloak, string realm, User newUser)
    {
        if (newUser.UserName == null)
            throw ControllerArgumentException.Create(ProvisioningServiceErrors.USER_CREATION_USERNAME_NULL, new ErrorParameter[] { new(ParamUserName, "null"), new(ParamRealm, realm) });

        string? newUserId;
        try
        {
            newUserId = await keycloak.CreateAndRetrieveUserIdAsync(realm, newUser).ConfigureAwait(ConfigureAwaitOptions.None);
        }
        catch (Exception error)
        {
            throw error switch
            {
                KeycloakEntityConflictException => ConflictException.Create(ProvisioningServiceErrors.USER_CREATION_CONFLICT, new ErrorParameter[] { new(ParamUserName, newUser.UserName), new(ParamRealm, realm) }, error),
                KeycloakEntityNotFoundException => NotFoundException.Create(ProvisioningServiceErrors.USER_CREATION_NOTFOUND, new ErrorParameter[] { new(ParamUserName, newUser.UserName), new(ParamRealm, realm) }, error),
                ArgumentException => ServiceException.Create(ProvisioningServiceErrors.USER_CREATION_ARGUMENT, new ErrorParameter[] { new(ParamUserName, newUser.UserName), new(ParamRealm, realm) }, error),
                ServiceException serviceException => ServiceException.Create(ProvisioningServiceErrors.USER_CREATION_FAILURE, new ErrorParameter[] { new(ParamUserName, newUser.UserName), new(ParamRealm, realm) }, serviceException.StatusCode, serviceException.IsRecoverable, error),
                _ => ServiceException.Create(ProvisioningServiceErrors.USER_CREATION_FAILURE, new ErrorParameter[] { new(ParamUserName, newUser.UserName), new(ParamRealm, realm) }, error)
            };
        }
        if (newUserId == null)
        {
            throw ServiceException.Create(ProvisioningServiceErrors.USER_CREATION_RETURNS_NULL, new ErrorParameter[] { new(ParamUserName, newUser.UserName), new(ParamRealm, realm) });
        }
        return newUserId;
    }

    private static User CloneUser(User user) =>
        JsonSerializer.Deserialize<User>(JsonSerializer.Serialize(user))!;
}
