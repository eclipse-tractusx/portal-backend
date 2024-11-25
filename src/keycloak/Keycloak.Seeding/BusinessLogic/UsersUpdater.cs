/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class UsersUpdater(IKeycloakFactory keycloakFactory, ISeedDataHandler seedDataHandler)
    : IUsersUpdater
{
    public async Task UpdateUsers(string keycloakInstanceName, CancellationToken cancellationToken)
    {
        var realm = seedDataHandler.Realm;
        var keycloak = keycloakFactory.CreateKeycloakClient(keycloakInstanceName);
        var clientsDictionary = seedDataHandler.ClientsDictionary;
        var seederConfig = seedDataHandler.GetSpecificConfiguration(ConfigurationKey.Users);

        foreach (var seedUser in seedDataHandler.Users)
        {
            if (seedUser.Username == null)
                throw new ConflictException($"username must not be null {seedUser.Id}");

            var createAllowed = seederConfig.ModificationAllowed(ModificationType.Create, seedUser.Username);
            var updateAllowed = seederConfig.ModificationAllowed(ModificationType.Update, seedUser.Username);
            if (!createAllowed && !updateAllowed)
            {
                continue;
            }

            var user = (await keycloak.GetUsersAsync(realm, username: seedUser.Username, cancellationToken: cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)).SingleOrDefault(x => x.UserName == seedUser.Username);

            if (user == null && createAllowed)
            {
                var result = await keycloak.RealmPartialImportAsync(realm, CreatePartialImportUser(seedUser), cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
                if (result.Overwritten != 0 || result.Added != 1 || result.Skipped != 0)
                {
                    throw new ConflictException($"PartialImport failed to add user id: {seedUser.Id}, userName: {seedUser.Username}");
                }
            }
            else if (user != null && updateAllowed)
            {
                await UpdateUser(
                    keycloak,
                    realm,
                    user,
                    seedUser,
                    clientsDictionary,
                    seederConfig,
                    cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            }
        }
    }

    private static async Task UpdateUser(KeycloakClient keycloak, string realm, User user, UserModel seedUser, IReadOnlyDictionary<string, string> clientsDictionary, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        if (user.Id == null)
            throw new ConflictException($"user.Id must not be null: userName {seedUser.Username}");

        if (user.UserName == null)
            throw new ConflictException($"user.UserName must not be null: userName {seedUser.Username}");

        if (!CompareUser(user, seedUser) && seederConfig.ModificationAllowed(ModificationType.Update, user.UserName))
        {
            await keycloak.UpdateUserAsync(
                realm,
                user.Id,
                CreateUpdateUser(user, seedUser),
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        await UpdateClientAndRealmRoles(
            keycloak,
            realm,
            user.Id,
            seedUser,
            clientsDictionary,
            cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        await UpdateFederatedIdentities(
            keycloak,
            realm,
            user.UserName,
            user.Id,
            seedUser.FederatedIdentities ?? Enumerable.Empty<FederatedIdentityModel>(),
            seederConfig,
            cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
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
                add => keycloak.AddClientRoleMappingsToUserAsync(realm, userId, id, add, cancellationToken)).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        await UpdateUserRoles(
            () => keycloak.GetRealmRoleMappingsForUserAsync(realm, userId, cancellationToken),
            () => seedUser.RealmRoles ?? Enumerable.Empty<string>(),
            () => keycloak.GetRolesAsync(realm, cancellationToken: cancellationToken),
            delete => keycloak.DeleteRealmRoleMappingsFromUserAsync(realm, userId, delete, cancellationToken),
            add => keycloak.AddRealmRoleMappingsToUserAsync(realm, userId, add, cancellationToken)).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private static async Task UpdateUserRoles(Func<Task<IEnumerable<Role>>> getUserRoles, Func<IEnumerable<string>> getSeedRoles, Func<Task<IEnumerable<Role>>> getAllRoles, Func<IEnumerable<Role>, Task> deleteRoles, Func<IEnumerable<Role>, Task> addRoles)
    {
        var userRoles = await getUserRoles().ConfigureAwait(ConfigureAwaitOptions.None);
        var seedRoles = getSeedRoles();

        await userRoles.ExceptBy(seedRoles, x => x.Name).IfAnyAwait(
            delete => deleteRoles(delete)).ConfigureAwait(false);

        await seedRoles.IfAnyAwait(
            async seed =>
            {
                var allRoles = await getAllRoles().ConfigureAwait(ConfigureAwaitOptions.None);
                seed.Except(allRoles.Select(x => x.Name)).IfAny(nonexisting => throw new ConflictException($"roles {string.Join(",", nonexisting)} does not exist"));
                await seed.Except(userRoles.Select(x => x.Name)).IfAnyAwait(
                    add => addRoles(allRoles.IntersectBy(add, x => x.Name))).ConfigureAwait(false);
            }).ConfigureAwait(false);
    }

    private static User CreateUpdateUser(User? user, UserModel update) => new()
    {
        // Roles, ClientConsents, Credentials, FederatedIdentities are not in scope
        Id = user?.Id,
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
        Attributes = update.Attributes?.FilterNotNullValues().ToDictionary(),
        Groups = update.Groups,
        ServiceAccountClientId = update.ServiceAccountClientId,
        Access = CreateUpdateUserAccess(update.Access),
        FederationLink = update.FederationLink,
        Origin = update.Origin,
        Self = update.Self
    };

    private static bool CompareUser(User user, UserModel update) =>
        // Roles, ClientConsents, Credentials, FederatedIdentities, are not in scope
        user.CreatedTimestamp == update.CreatedTimestamp &&
        user.UserName == update.Username &&
        user.Enabled == update.Enabled &&
        user.Totp == update.Totp &&
        user.EmailVerified == update.EmailVerified &&
        user.FirstName == update.FirstName &&
        user.LastName == update.LastName &&
        user.Email == update.Email &&
        user.DisableableCredentialTypes.NullOrContentEqual(update.DisableableCredentialTypes) &&
        user.RequiredActions.NullOrContentEqual(update.RequiredActions) &&
        user.NotBefore == update.NotBefore &&
        user.Attributes.NullOrContentEqual(update.Attributes?.FilterNotNullValues()) &&
        user.Groups.NullOrContentEqual(update.Groups) &&
        user.ServiceAccountClientId == update.ServiceAccountClientId &&
        CompareUserAccess(user.Access, update.Access) &&
        user.FederationLink == update.FederationLink &&
        user.Origin == update.Origin &&
        user.Self == update.Self;

    private static UserAccess? CreateUpdateUserAccess(UserAccessModel? update) =>
        update == null ? null : new()
        {
            ManageGroupMembership = update.ManageGroupMembership,
            View = update.View,
            MapRoles = update.MapRoles,
            Impersonate = update.Impersonate,
            Manage = update.Manage
        };

    private static bool CompareUserAccess(UserAccess? userAccess, UserAccessModel? update) =>
        userAccess == null && update == null ||
        userAccess != null && update != null &&
        userAccess.ManageGroupMembership == update.ManageGroupMembership &&
        userAccess.View == update.View &&
        userAccess.MapRoles == update.MapRoles &&
        userAccess.Impersonate == update.Impersonate &&
        userAccess.Manage == update.Manage;

    private static bool CompareFederatedIdentity(FederatedIdentity identity, FederatedIdentityModel update) =>
        identity.IdentityProvider == update.IdentityProvider &&
        identity.UserId == update.UserId &&
        identity.UserName == update.UserName;

    private static async Task UpdateFederatedIdentities(KeycloakClient keycloak, string realm, string username, string userId, IEnumerable<FederatedIdentityModel> updates, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        var identities = await keycloak.GetUserSocialLoginsAsync(realm, userId).ConfigureAwait(ConfigureAwaitOptions.None);
        await DeleteObsoleteFederatedIdentities(keycloak, realm, username, userId, identities, updates, seederConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await CreateMissingFederatedIdentities(keycloak, realm, username, userId, identities, updates, seederConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await UpdateExistingFederatedIdentities(keycloak, realm, username, userId, identities, updates, seederConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private static async Task DeleteObsoleteFederatedIdentities(KeycloakClient keycloak, string realm, string username, string userId, IEnumerable<FederatedIdentity> identities, IEnumerable<FederatedIdentityModel> updates, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        foreach (var identity in identities
                     .Where(x => seederConfig.ModificationAllowed(username, ConfigurationKey.FederatedIdentities, ModificationType.Delete, x.IdentityProvider))
                     .ExceptBy(updates.Select(x => x.IdentityProvider), x => x.IdentityProvider))
        {
            await keycloak.RemoveUserSocialLoginProviderAsync(
                realm,
                userId,
                identity.IdentityProvider ?? throw new ConflictException($"federatedIdentity.IdentityProvider is null {userId}"),
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task CreateMissingFederatedIdentities(KeycloakClient keycloak, string realm, string username, string userId, IEnumerable<FederatedIdentity> identities, IEnumerable<FederatedIdentityModel> updates, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        foreach (var update in updates
                     .Where(x => seederConfig.ModificationAllowed(username, ConfigurationKey.FederatedIdentities, ModificationType.Create, x.IdentityProvider))
                     .ExceptBy(identities.Select(x => x.IdentityProvider), x => x.IdentityProvider))
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
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task UpdateExistingFederatedIdentities(KeycloakClient keycloak, string realm, string username, string userId, IEnumerable<FederatedIdentity> identities, IEnumerable<FederatedIdentityModel> updates, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        foreach (var (identity, update) in identities
            .Where(x => seederConfig.ModificationAllowed(username, ConfigurationKey.FederatedIdentities, ModificationType.Update, x.IdentityProvider))
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
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

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
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static Credentials CreateUpdateCredentials(CredentialsModel update) =>
        new()
        {
            Algorithm = update.Algorithm,
            Config = update.Config?.FilterNotNullValues().ToDictionary(),
            Counter = update.Counter,
            CreatedDate = update.CreatedDate,
            Device = update.Device,
            Digits = update.Digits,
            HashIterations = update.HashIterations,
            Period = update.Period,
            Salt = update.Salt,
            Temporary = update.Temporary,
            Type = update.Type,
            Value = update.Value,
        };

    private static FederatedIdentity CreateUpdateFederatedIdentity(FederatedIdentityModel update) =>
        new()
        {
            IdentityProvider = update.IdentityProvider,
            UserId = update.UserId,
            UserName = update.UserName
        };

    private static PartialImport CreatePartialImportUser(UserModel update) =>
        new()
        {
            IfResourceExists = "FAIL",
            Users = [
                new()
                {
                    // ClientConsents are not in scope
                    Id = update.Id,
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
                    Access = CreateUpdateUserAccess(update.Access),
                    Attributes = update.Attributes?.FilterNotNullValues().ToDictionary(),
                    ClientRoles = update.ClientRoles?.FilterNotNullValues().ToDictionary(),
                    Credentials = update.Credentials?.Select(CreateUpdateCredentials),
                    FederatedIdentities = update.FederatedIdentities?.Select(CreateUpdateFederatedIdentity),
                    FederationLink = update.FederationLink,
                    Groups = update.Groups,
                    Origin = update.Origin,
                    RealmRoles = update.RealmRoles,
                    Self = update.Self,
                    ServiceAccountClientId = update.ServiceAccountClientId
                }]
        };
}
