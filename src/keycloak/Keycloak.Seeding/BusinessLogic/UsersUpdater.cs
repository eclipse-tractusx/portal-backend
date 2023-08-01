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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class UsersUpdater : IUsersUpdater
{
    private readonly IKeycloakFactory _keycloakFactory;
    private readonly ISeedDataHandler _seedData;

    public UsersUpdater(IKeycloakFactory keycloakFactory, ISeedDataHandler seedDataHandler)
    {
        _keycloakFactory = keycloakFactory;
        _seedData = seedDataHandler;
    }

    public async Task UpdateUsers(string keycloakInstanceName, CancellationToken cancellationToken)
    {
        var realm = _seedData.Realm;
        var keycloak = _keycloakFactory.CreateKeycloakClient(keycloakInstanceName);
        var clientsDictionary = _seedData.ClientsDictionary;

        foreach (var seedUser in _seedData.Users)
        {
            if (seedUser.Username == null)
                throw new ConflictException($"username must not be null {seedUser.Id}");

            var userId = await CreateOrUpdateUserReturningId(
                keycloak,
                realm,
                seedUser,
                cancellationToken).ConfigureAwait(false);

            await UpdateClientAndRealmRoles(
                keycloak,
                realm,
                userId,
                seedUser,
                clientsDictionary,
                cancellationToken).ConfigureAwait(false);

            await UpdateFederatedIdentities(
                keycloak,
                realm,
                userId,
                seedUser.FederatedIdentities ?? Enumerable.Empty<FederatedIdentityModel>(),
                cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<string> CreateOrUpdateUserReturningId(KeycloakClient keycloak, string realm, UserModel seedUser, CancellationToken cancellationToken)
    {
        var user = (await keycloak.GetUsersAsync(realm, username: seedUser.Username, cancellationToken: cancellationToken).ConfigureAwait(false)).SingleOrDefault(x => x.UserName == seedUser.Username);

        if (user == null)
        {
            return await keycloak.CreateAndRetrieveUserIdAsync(
                realm,
                CreateUpdateUser(null, seedUser),
                cancellationToken).ConfigureAwait(false) ?? throw new KeycloakNoSuccessException($"failed to retrieve id of newly created user {seedUser.Username}");
        }
        else
        {
            if (user.Id == null)
                throw new ConflictException($"user.Id must not be null: userName {seedUser.Username}");
            if (!CompareUser(user, seedUser))
            {
                await keycloak.UpdateUserAsync(
                    realm,
                    user.Id,
                    CreateUpdateUser(user.Id, seedUser),
                    cancellationToken).ConfigureAwait(false);
            }
            return user.Id;
        }
    }

    private static async Task UpdateClientAndRealmRoles(KeycloakClient keycloak, string realm, string userId, UserModel seedUser, IReadOnlyDictionary<string, string> clientsDictionary, CancellationToken cancellationToken)
    {
        foreach (var (clientId, id) in clientsDictionary)
        {
            await UpdateUserRoles(
                () => keycloak.GetClientRoleMappingsForUserAsync(realm, userId, id, cancellationToken),
                () => seedUser.ClientRoles?.GetValueOrDefault(clientId) ?? Enumerable.Empty<string>(),
                () => keycloak.GetRolesAsync(realm, id, cancellationToken: cancellationToken),
                delete => keycloak.DeleteClientRoleMappingsFromUserAsync(realm, userId, id, delete, cancellationToken),
                add => keycloak.AddClientRoleMappingsToUserAsync(realm, userId, id, add, cancellationToken)).ConfigureAwait(false);
        }

        await UpdateUserRoles(
            () => keycloak.GetRealmRoleMappingsForUserAsync(realm, userId, cancellationToken),
            () => seedUser.RealmRoles ?? Enumerable.Empty<string>(),
            () => keycloak.GetRolesAsync(realm, cancellationToken: cancellationToken),
            delete => keycloak.DeleteRealmRoleMappingsFromUserAsync(realm, userId, delete, cancellationToken),
            add => keycloak.AddRealmRoleMappingsToUserAsync(realm, userId, add, cancellationToken)).ConfigureAwait(false);
    }

    private static async Task UpdateUserRoles(Func<Task<IEnumerable<Role>>> getUserRoles, Func<IEnumerable<string>> getSeedRoles, Func<Task<IEnumerable<Role>>> getAllRoles, Func<IEnumerable<Role>, Task> deleteRoles, Func<IEnumerable<Role>, Task> addRoles)
    {
        var userRoles = await getUserRoles().ConfigureAwait(false);
        var seedRoles = getSeedRoles();

        if (userRoles.ExceptBy(seedRoles, x => x.Name).IfAny(
            delete => deleteRoles(delete),
            out var deleteRolesTask))
        {
            await deleteRolesTask!.ConfigureAwait(false);
        }

        if (seedRoles.IfAny(
            async seed =>
            {
                var allRoles = await getAllRoles().ConfigureAwait(false);
                seed.Except(allRoles.Select(x => x.Name)).IfAny(nonexisting => throw new ConflictException($"roles {string.Join(",", nonexisting)} does not exist"));
                if (seed.Except(userRoles.Select(x => x.Name)).IfAny(
                    add => addRoles(allRoles.IntersectBy(add, x => x.Name)),
                    out var addRolesTask))
                {
                    await addRolesTask!.ConfigureAwait(false);
                }
            },
            out var updateRolesTask))
        {
            await updateRolesTask!.ConfigureAwait(false);
        }
    }

    private static User CreateUpdateUser(string? id, UserModel update) => new User
    {
        Id = id,
        CreatedTimestamp = update.CreatedTimestamp,
        UserName = update.Username,
        Enabled = update.Enabled,
        Totp = update.Totp,
        EmailVerified = update.EmailVerified,
        FirstName = update.FirstName,
        LastName = update.LastName,
        Email = update.Email,
        DisableableCredentialTypes = update.DisableableCredentialTypes,
        RequiredActions = update.RequiredActions,
        NotBefore = update.NotBefore,
        // Access = update.Access,
        Attributes = update.Attributes?.ToDictionary(x => x.Key, x => x.Value),
        // ClientConsents = update.ClientConsents,
        // Credentials = update.Credentials,
        // FederatedIdentities: doesn't update
        // FederationLink = update.FederationLink,
        Groups = update.Groups,
        // Origin = update.Origin,
        // Self = update.Self,
        ServiceAccountClientId = update.ServiceAccountClientId
    };

    private static bool CompareUser(User user, UserModel update) =>
        user.CreatedTimestamp == update.CreatedTimestamp &&
        user.UserName == update.Username &&
        user.Enabled == update.Enabled &&
        user.Totp == update.Totp &&
        user.EmailVerified == update.EmailVerified &&
        user.FirstName == update.FirstName &&
        user.LastName == update.LastName &&
        user.Email == update.Email &&
        user.DisableableCredentialTypes == update.DisableableCredentialTypes &&
        user.RequiredActions == update.RequiredActions &&
        user.NotBefore == update.NotBefore &&
        // Access == update.Access &&
        user.Attributes.NullOrContentEqual(update.Attributes) &&
        // ClientConsents == update.ClientConsents &&
        // Credentials == update.Credentials &&
        // CompareFederatedIdentities(user.FederatedIdentities, update.FederatedIdentities) && // doesn't update
        // FederationLink == update.FederationLink &&
        user.Groups.NullOrContentEqual(update.Groups) &&
        // Origin == update.Origin &&
        // Self == update.Self &&
        user.ServiceAccountClientId == update.ServiceAccountClientId;

    private static bool CompareFederatedIdentity(FederatedIdentity identity, FederatedIdentityModel update) =>
        identity.IdentityProvider == update.IdentityProvider &&
        identity.UserId == update.UserId &&
        identity.UserName == update.UserName;

    private static async Task UpdateFederatedIdentities(KeycloakClient keycloak, string realm, string userId, IEnumerable<FederatedIdentityModel> updates, CancellationToken cancellationToken)
    {
        var identities = await keycloak.GetUserSocialLoginsAsync(realm, userId).ConfigureAwait(false);
        await DeleteObsoleteFederatedIdentities(keycloak, realm, userId, identities, updates, cancellationToken).ConfigureAwait(false);
        await CreateMissingFederatedIdentities(keycloak, realm, userId, identities, updates, cancellationToken).ConfigureAwait(false);
        await UpdateExistingFederatedIdentities(keycloak, realm, userId, identities, updates, cancellationToken).ConfigureAwait(false);
    }

    private static async Task DeleteObsoleteFederatedIdentities(KeycloakClient keycloak, string realm, string userId, IEnumerable<FederatedIdentity> identities, IEnumerable<FederatedIdentityModel> updates, CancellationToken cancellationToken)
    {
        foreach (var identity in identities.ExceptBy(updates.Select(x => x.IdentityProvider), x => x.IdentityProvider))
        {
            await keycloak.RemoveUserSocialLoginProviderAsync(
                realm,
                userId,
                identity.IdentityProvider ?? throw new ConflictException($"federatedIdentity.IdentityProvider is null {userId}"),
                cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task CreateMissingFederatedIdentities(KeycloakClient keycloak, string realm, string userId, IEnumerable<FederatedIdentity> identities, IEnumerable<FederatedIdentityModel> updates, CancellationToken cancellationToken)
    {
        foreach (var update in updates.ExceptBy(identities.Select(x => x.IdentityProvider), x => x.IdentityProvider))
        {
            await keycloak.AddUserSocialLoginProviderAsync(
                realm,
                userId,
                update.IdentityProvider ?? throw new ConflictException($"federatedIdentity.IdentityProvider is null {userId}"),
                new()
                {
                    IdentityProvider = update.IdentityProvider,
                    UserId = update.UserId ?? throw new ConflictException($"federatedIdentity.UserId is null {userId}, {update.IdentityProvider}"),
                    UserName = update.UserName ?? throw new ConflictException($"federatedIdentity.UserName is null {userId}, {update.IdentityProvider}")
                },
                cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task UpdateExistingFederatedIdentities(KeycloakClient keycloak, string realm, string userId, IEnumerable<FederatedIdentity> identities, IEnumerable<FederatedIdentityModel> updates, CancellationToken cancellationToken)
    {
        foreach (var (identity, update) in identities
            .Join(
                updates,
                x => x.IdentityProvider,
                x => x.IdentityProvider,
                (identity, update) => (Identity: identity, Update: update))
            .Where(x => !CompareFederatedIdentity(x.Identity, x.Update)))
        {
            await keycloak.RemoveUserSocialLoginProviderAsync(
                realm,
                userId,
                identity.IdentityProvider ?? throw new ConflictException($"federatedIdentity.IdentityProvider is null {userId}"),
                cancellationToken).ConfigureAwait(false);

            await keycloak.AddUserSocialLoginProviderAsync(
                realm,
                userId,
                update.IdentityProvider ?? throw new ConflictException($"federatedIdentity.IdentityProvider is null {userId}"),
                new()
                {
                    IdentityProvider = update.IdentityProvider,
                    UserId = update.UserId ?? throw new ConflictException($"federatedIdentity.UserId is null {userId}, {update.IdentityProvider}"),
                    UserName = update.UserName ?? throw new ConflictException($"federatedIdentity.UserName is null {userId}, {update.IdentityProvider}")
                },
                cancellationToken).ConfigureAwait(false);
        }
    }
}
