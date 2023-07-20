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

        foreach (var seedUser in _seedData.Users)
        {
            if (seedUser.Username == null)
                throw new ConflictException($"username must not be null {seedUser.Id}");

            var user = (await keycloak.GetUsersAsync(realm, username: seedUser.Username, cancellationToken: cancellationToken).ConfigureAwait(false)).SingleOrDefault(x => x.UserName == seedUser.Username);
            if (user == null)
            {
                user = CreateUpdateUser(null, seedUser);
                user.Id = await keycloak.CreateAndRetrieveUserIdAsync(realm, user, cancellationToken).ConfigureAwait(false);
                if (user.Id == null)
                    throw new KeycloakNoSuccessException($"failed to retrieve id of newly created user {seedUser.Username}");
            }
            else
            {
                if (user.Id == null)
                    throw new ConflictException($"user.Id must not be null: userName {seedUser.Username}");
                if (!Compare(user, seedUser))
                {
                    var updateUser = CreateUpdateUser(user.Id, seedUser);
                    await keycloak.UpdateUserAsync(
                        realm,
                        user.Id,
                        updateUser,
                        cancellationToken).ConfigureAwait(false);
                }
            }

            foreach (var (clientId, id) in _seedData.ClientsDictionary)
            {
                await UpdateUserRoles(
                    () => keycloak.GetClientRoleMappingsForUserAsync(realm, user.Id, id, cancellationToken),
                    () => (seedUser.ClientRoles?.TryGetValue(clientId, out var seedRoles) ?? false) ? seedRoles : Enumerable.Empty<string>(),
                    () => keycloak.GetRolesAsync(realm, id, cancellationToken: cancellationToken),
                    delete => keycloak.DeleteClientRoleMappingsFromUserAsync(realm, user.Id, id, delete, cancellationToken),
                    add => keycloak.AddClientRoleMappingsToUserAsync(realm, user.Id, id, add, cancellationToken)).ConfigureAwait(false);
            }

            await UpdateUserRoles(
                () => keycloak.GetRealmRoleMappingsForUserAsync(realm, user.Id, cancellationToken),
                () => seedUser.RealmRoles ?? Enumerable.Empty<string>(),
                () => keycloak.GetRolesAsync(realm, cancellationToken: cancellationToken),
                delete => keycloak.DeleteRealmRoleMappingsFromUserAsync(realm, user.Id, delete, cancellationToken),
                add => keycloak.AddRealmRoleMappingsToUserAsync(realm, user.Id, add, cancellationToken)).ConfigureAwait(false);
        }
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
        FederatedIdentities = update.FederatedIdentities?.Select(x => new FederatedIdentity
        { // TODO: this works only on usercreation, it does not update existing identities
            IdentityProvider = x.IdentityProvider,
            UserId = x.UserId,
            UserName = x.UserName
        }),
        // FederationLink = update.FederationLink,
        Groups = update.Groups,
        // Origin = update.Origin,
        // Self = update.Self,
        ServiceAccountClientId = update.ServiceAccountClientId
    };

    private static bool Compare(User user, UserModel update) =>
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
        Compare(user.FederatedIdentities, update.FederatedIdentities) && // doesn't update
                                                                         // FederationLink == update.FederationLink &&
        user.Groups.NullOrContentEqual(update.Groups) &&
        // Origin == update.Origin &&
        // Self == update.Self &&
        user.ServiceAccountClientId == update.ServiceAccountClientId;

    private static bool Compare(IEnumerable<FederatedIdentity>? identities, IEnumerable<FederatedIdentityModel>? updates) =>
        identities == null && updates == null ||
        identities != null && updates != null &&
        identities.Select(x => x.IdentityProvider ?? throw new ConflictException("keycloak federated identity identityProvider must not be null")).NullOrContentEqual(updates.Select(x => x.IdentityProvider ?? throw new ConflictException("seeding federated identity identityProvider must not be null"))) &&
        identities.Select(x => x.UserId ?? throw new ConflictException("keycloak federated identity identityProvider must not be null")).NullOrContentEqual(updates.Select(x => x.UserId ?? throw new ConflictException("seeding federated identity identityProvider must not be null"))) &&
        identities.Select(x => x.UserName ?? throw new ConflictException("keycloak federated identity identityProvider must not be null")).NullOrContentEqual(updates.Select(x => x.UserName ?? throw new ConflictException("seeding federated identity identityProvider must not be null")));
}
