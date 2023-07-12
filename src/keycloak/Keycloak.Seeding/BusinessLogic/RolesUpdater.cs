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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class RolesUpdater : IRolesUpdater
{
    private readonly IKeycloakFactory _keycloakFactory;
    private readonly ISeedDataHandler _seedData;

    public RolesUpdater(IKeycloakFactory keycloakFactory, ISeedDataHandler seedDataHandler)
    {
        _keycloakFactory = keycloakFactory;
        _seedData = seedDataHandler;
    }

    public async Task UpdateClientRoles(string keycloakInstanceName)
    {
        var keycloak = _keycloakFactory.CreateKeycloakClient(keycloakInstanceName);
        var realm = _seedData.Realm;

        foreach (var (clientId, updateRoles) in _seedData.ClientRoles)
        {
            var id = _seedData.GetIdOfClient(clientId);
            var roles = await keycloak.GetRolesAsync(realm, id).ConfigureAwait(false);

            foreach (var newRole in updateRoles.ExceptBy(roles.Select(x => x.Name), x => x.Name))
            {
                await keycloak.CreateRoleAsync(realm, id, CreateRole(newRole)).ConfigureAwait(false);
            }

            await UpdateAndDeleteRoles(keycloak, realm, roles, updateRoles).ConfigureAwait(false);
        }
    }

    public async Task UpdateRealmRoles(string keycloakInstanceName)
    {
        var keycloak = _keycloakFactory.CreateKeycloakClient(keycloakInstanceName);
        var realm = _seedData.Realm;
        var roles = await keycloak.GetRolesAsync(realm).ConfigureAwait(false);
        var updateRealmRoles = _seedData.RealmRoles;

        if (!updateRealmRoles.Any())
        {
            foreach (var role in roles)
            {
                if (role.Id == null)
                    throw new ConflictException($"role id must not be null: {role.Name}");
                await keycloak.DeleteRoleByIdAsync(realm, role.Id).ConfigureAwait(false);
            }
            return;
        }
        foreach (var newRole in updateRealmRoles.ExceptBy(roles.Select(x => x.Name), x => x.Name))
        {
            await keycloak.CreateRoleAsync(realm, CreateRole(newRole));
        }
        await UpdateAndDeleteRoles(keycloak, realm, roles, updateRealmRoles).ConfigureAwait(false);
    }

    private static async Task UpdateAndDeleteRoles(Library.KeycloakClient keycloak, string realm, IEnumerable<Library.Models.Roles.Role> roles, IEnumerable<RoleModel> updateRoles)
    {
        foreach (var joinedRole in 
            roles.Join(
                    updateRoles,
                    x => x.Name,
                    x => x.Name,
                    (role, updateRole) => (Role: role, Update: updateRole)))
        {
            if (joinedRole.Role.Id == null)
                throw new ConflictException($"role id must not be null: {joinedRole.Role.Name}");
            if (joinedRole.Role.ContainerId == null)
                throw new ConflictException($"role containerId must not be null: {joinedRole.Role.Name}");
            await keycloak.UpdateRoleByIdAsync(realm, joinedRole.Role.Id, CreateUpdateRole(joinedRole.Role.Id, joinedRole.Role.ContainerId, joinedRole.Update)).ConfigureAwait(false);
        }

        foreach (var deleteRole in
            roles.ExceptBy(updateRoles.Select(x => x.Name), x => x.Name))
        {
            if (deleteRole.Id == null)
                throw new ConflictException($"role id must not be null: {deleteRole.Name}");
            await keycloak.DeleteRoleByIdAsync(realm, deleteRole.Id).ConfigureAwait(false);
        }
    }

    public async Task UpdateCompositeRoles(string keycloakInstanceName)
    {
        var keycloak = _keycloakFactory.CreateKeycloakClient(keycloakInstanceName);
        var realm = _seedData.Realm;

        if (_seedData.ClientRoles.IfAny(async clientRoles =>
            {
                foreach (var updateClientRoles in clientRoles)
                {
                    var (clientId, updateRoles) = updateClientRoles;
                    var id = _seedData.GetIdOfClient(clientId);

                    await UpdateCompositeRolesInternal(
                        keycloak,
                        realm,
                        () => keycloak.GetRolesAsync(realm, id),
                        updateRoles,
                        (string name, IEnumerable<Library.Models.Roles.Role> roles) => keycloak.RemoveCompositesFromRoleAsync(realm, id, name, roles),
                        (string name, IEnumerable<Library.Models.Roles.Role> roles) => keycloak.AddCompositesToRoleAsync(realm, id, name, roles)).ConfigureAwait(false);
                }
            },
            out var updateCompositeClientRolesTask))
        {
            await updateCompositeClientRolesTask!.ConfigureAwait(false);
        }

        if (_seedData.RealmRoles.IfAny(realmRoles =>
            UpdateCompositeRolesInternal(
                keycloak,
                realm,
                () => keycloak.GetRolesAsync(realm),
                realmRoles,
                (string name, IEnumerable<Library.Models.Roles.Role> roles) => keycloak.RemoveCompositesFromRoleAsync(realm, name, roles),
                (string name, IEnumerable<Library.Models.Roles.Role> roles) => keycloak.AddCompositesToRoleAsync(realm, name, roles)),
            out var updateCompositeRealmRolesTask))
        {
            await updateCompositeRealmRolesTask!.ConfigureAwait(false);
        }
    }

    private async Task UpdateCompositeRolesInternal(KeycloakClient keycloak, string realm, Func<Task<IEnumerable<Library.Models.Roles.Role>>> getRoles, IEnumerable<RoleModel> updateRoles, Func<string, IEnumerable<Library.Models.Roles.Role>, Task> removeCompositeRoles, Func<string, IEnumerable<Library.Models.Roles.Role>, Task> addCompositeRoles)
    {
        var roles = await getRoles().ConfigureAwait(false);
        var updateClientComposites = updateRoles.Where(x => x.Composites?.Client?.Any() ?? false);
        var removeClientComposites = roles.Where(x => x.Composites?.Client?.Any() ?? false).ExceptBy(updateClientComposites.Select(x => x.Name), x => x.Name);

        foreach (var remove in removeClientComposites)
        {
            if (remove.Id == null || remove.Name == null)
                throw new ConflictException($"role.id or role.name must not be null {remove.Id} {remove.Name}");

            var clientComposites = (await keycloak.GetRoleChildrenAsync(realm, remove.Id).ConfigureAwait(false)).Where(x => x.ClientRole ?? false);
            await removeCompositeRoles(remove.Name, clientComposites).ConfigureAwait(false);
        }

        var joinedClientComposites = roles.Join(
            updateClientComposites,
            x => x.Name,
            x => x.Name,
            (role, update) => (
                Role: role,
                Update: update.Composites!.Client!
                    .Select(x => (
                        Id: _seedData.GetIdOfClient(x.Key),
                        Names: x.Value))
                    .SelectMany(x => x.Names.Select(name => (x.Id, Name: name)))));

        foreach (var (role, update) in joinedClientComposites)
        {
            if (role.Id == null || role.Name == null)
                throw new ConflictException($"role.id or role.name must not be null {role.Id} {role.Name}");
            var clientComposites = (await keycloak.GetRoleChildrenAsync(realm, role.Id).ConfigureAwait(false)).Where(x => x.ClientRole ?? false);
            clientComposites.Where(x => x.ContainerId == null || x.Name == null).IfAny(invalid => throw new ConflictException($"composites roles containerId or name must not be null: {string.Join(" ", invalid.Select(x => $"[{string.Join(",", x.Id, x.Name, x.Description, x.ContainerId)}]"))}"));

            var remove = clientComposites.ExceptBy(update, x => (x.ContainerId!, x.Name!));
            await removeCompositeRoles(role.Name, remove).ConfigureAwait(false);

            var add = await update.Except(clientComposites.Select(x => (x.ContainerId!, x.Name!)))
                .Select(x => keycloak.GetRoleByNameAsync(realm, x.Item1, x.Item2))
                .ToEnumerableTask()
                .ConfigureAwait(false);
            await addCompositeRoles(role.Name, add).ConfigureAwait(false);
        }

        var updateRealmComposites = updateRoles.Where(x => x.Composites?.Realm?.Any() ?? false); // imported roles this client with realm composite roles

        var removeRealmComposites = roles.Where(x => x.Composites?.Realm?.Any() ?? false).ExceptBy(updateRealmComposites.Select(x => x.Name), x => x.Name);

        foreach (var remove in removeRealmComposites)
        {
            if (remove.Id == null || remove.Name == null)
                throw new ConflictException($"role.id or role.name must not be null {remove.Id} {remove.Name}");

            var realmComposites = (await keycloak.GetRoleChildrenAsync(realm, remove.Id).ConfigureAwait(false)).Where(x => !(x.ClientRole ?? false));
            await removeCompositeRoles(remove.Name, realmComposites);
        }

        var joinedRealmComposites = roles.Join(
            updateRealmComposites,
            x => x.Name,
            x => x.Name,
            (role, update) => (
                Role: role,
                Update: update.Composites!.Realm!));

        foreach (var (role, update) in joinedRealmComposites)
        {
            if (role.Id == null || role.Name == null)
                throw new ConflictException($"role.id or role.name must not be null {role.Id} {role.Name}");
            var realmComposites = (await keycloak.GetRoleChildrenAsync(realm, role.Id).ConfigureAwait(false)).Where(x => !(x.ClientRole ?? false));
            realmComposites.Where(x => x.Name == null).IfAny(invalid => throw new ConflictException($"composites roles name must not be null: {string.Join(" ", invalid.Select(x => $"[{string.Join(",", x.Id, x.Name, x.Description, x.ContainerId)}]"))}"));

            var remove = realmComposites.ExceptBy(update, x => x.Name);
            await removeCompositeRoles(role.Name, remove).ConfigureAwait(false);

            var add = await update.Except(realmComposites.Select(x => x.Name!))
                .Select(x => keycloak.GetRoleByNameAsync(realm, x))
                .ToEnumerableTask()
                .ConfigureAwait(false);
            await addCompositeRoles(role.Name, add);
        }
    }

    private static Library.Models.Roles.Role CreateRole(RoleModel updateRole) =>
        new Library.Models.Roles.Role
        {
            Name = updateRole.Name,
            Description = updateRole.Description,
            Composite = updateRole.Composite,
            ClientRole = updateRole.ClientRole,
            Attributes = updateRole.Attributes?.ToDictionary(x => x.Key, x => x.Value.AsEnumerable())
        };

    private static Library.Models.Roles.Role CreateUpdateRole(string id, string containerId, RoleModel updateRole)
    {
        var role = CreateRole(updateRole);
        role.Id = id;
        role.ContainerId = containerId;
        return role;
    }
}
