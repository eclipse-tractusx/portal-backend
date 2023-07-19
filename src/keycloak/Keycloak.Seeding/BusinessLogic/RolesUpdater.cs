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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;
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

            foreach (var newRole in updateRoles.ExceptBy(roles.Select(role => role.Name), roleModel => roleModel.Name))
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

        foreach (var newRole in updateRealmRoles.ExceptBy(roles.Select(role => role.Name), roleModel => roleModel.Name))
        {
            await keycloak.CreateRoleAsync(realm, CreateRole(newRole));
        }
        await UpdateAndDeleteRoles(keycloak, realm, roles, updateRealmRoles).ConfigureAwait(false);
    }

    private static async Task UpdateAndDeleteRoles(KeycloakClient keycloak, string realm, IEnumerable<Role> roles, IEnumerable<RoleModel> updateRoles)
    {
        foreach (var (role, update) in
            roles.Join(
                updateRoles,
                role => role.Name,
                roleModel => roleModel.Name,
                (role, roleModel) => (Role: role, Update: roleModel)))
        {
            if (!Compare(role, update))
            {
                if (role.Id == null)
                    throw new ConflictException($"role id must not be null: {role.Name}");
                if (role.ContainerId == null)
                    throw new ConflictException($"role containerId must not be null: {role.Name}");
                await keycloak.UpdateRoleByIdAsync(realm, role.Id, CreateUpdateRole(role.Id, role.ContainerId, update)).ConfigureAwait(false);
            }
        }

        foreach (var deleteRole in
            roles.ExceptBy(updateRoles.Select(roleModel => roleModel.Name), role => role.Name))
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
                foreach (var (clientId, updateRoles) in clientRoles)
                {
                    var id = _seedData.GetIdOfClient(clientId);

                    await UpdateCompositeRolesInner(
                        () => keycloak.GetRolesAsync(realm, id),
                        updateRoles,
                        (name, roles) => keycloak.RemoveCompositesFromRoleAsync(realm, id, name, roles),
                        (name, roles) => keycloak.AddCompositesToRoleAsync(realm, id, name, roles)).ConfigureAwait(false);
                }
            },
            out var updateCompositeClientRolesTask))
        {
            await updateCompositeClientRolesTask!.ConfigureAwait(false);
        }

        if (_seedData.RealmRoles.IfAny(realmRoles =>
            UpdateCompositeRolesInner(
                () => keycloak.GetRolesAsync(realm),
                realmRoles,
                (name, roles) => keycloak.RemoveCompositesFromRoleAsync(realm, name, roles),
                (name, roles) => keycloak.AddCompositesToRoleAsync(realm, name, roles)),
            out var updateCompositeRealmRolesTask))
        {
            await updateCompositeRealmRolesTask!.ConfigureAwait(false);
        }

        async Task UpdateCompositeRolesInner(
            Func<Task<IEnumerable<Role>>> getRoles,
            IEnumerable<RoleModel> updateRoles,
            Func<string, IEnumerable<Role>, Task> removeCompositeRoles,
            Func<string, IEnumerable<Role>, Task> addCompositeRoles)
        {
            var roles = await getRoles().ConfigureAwait(false);

            await RemoveAddCompositeRolesInner<(string ContainerId, string Name)>(
                roleModel => roleModel.Composites?.Client?.Any() ?? false,
                role => role.Composites?.Client?.Any() ?? false,
                role => role.ClientRole ?? false,
                roleModel => roleModel.Composites?.Client?
                        .Select(x => (
                            Id: _seedData.GetIdOfClient(x.Key),
                            Names: x.Value))
                        .SelectMany(x => x.Names.Select(name => (x.Id, name))) ?? throw new ConflictException($"roleModel.Composites.Client is null: {roleModel.Id} {roleModel.Name}"),
                role => (
                    role.ContainerId ?? throw new ConflictException($"role.ContainerId is null: {role.Id} {role.Name}"),
                    role.Name ?? throw new ConflictException($"role.Name is null: {role.Id}")),
                x => keycloak.GetRoleByNameAsync(realm, x.ContainerId, x.Name)
            ).ConfigureAwait(false);

            await RemoveAddCompositeRolesInner(
                roleModel => roleModel.Composites?.Realm?.Any() ?? false,
                role => role.Composites?.Realm?.Any() ?? false,
                role => !(role.ClientRole ?? false),
                roleModel => roleModel.Composites?.Realm ?? throw new ConflictException($"roleModel.Composites.Realm is null: {roleModel.Id} {roleModel.Name}"),
                role => role.Name ?? throw new ConflictException($"role.Name is null: {role.Id}"),
                name => keycloak.GetRoleByNameAsync(realm, name)
            ).ConfigureAwait(false);

            async Task RemoveAddCompositeRolesInner<T>(
                Func<RoleModel, bool> compositeRolesUpdatePredicate,
                Func<Role, bool> compositeRolesPredicate,
                Func<Role, bool> rolePredicate,
                Func<RoleModel, IEnumerable<T>> joinUpdateSelector,
                Func<Role, T> joinUpdateKey,
                Func<T, Task<Role>> getRoleByName)
            {
                var updateComposites = updateRoles.Where(x => compositeRolesUpdatePredicate(x));
                var removeComposites = roles.Where(x => compositeRolesPredicate(x)).ExceptBy(updateComposites.Select(roleModel => roleModel.Name), role => role.Name);

                foreach (var remove in removeComposites)
                {
                    if (remove.Id == null || remove.Name == null)
                        throw new ConflictException($"role.id or role.name must not be null {remove.Id} {remove.Name}");

                    var composites = (await keycloak.GetRoleChildrenAsync(realm, remove.Id).ConfigureAwait(false)).Where(role => rolePredicate(role));
                    await removeCompositeRoles(remove.Name, composites).ConfigureAwait(false);
                }

                var joinedComposites = roles.Join(
                    updateComposites,
                    role => role.Name,
                    roleModel => roleModel.Name,
                    (role, roleModel) => (
                        Role: role,
                        Update: joinUpdateSelector(roleModel)));

                foreach (var (role, updates) in joinedComposites)
                {
                    if (role.Id == null || role.Name == null)
                        throw new ConflictException($"role.id or role.name must not be null {role.Id} {role.Name}");
                    var composites = (await keycloak.GetRoleChildrenAsync(realm, role.Id).ConfigureAwait(false)).Where(role => rolePredicate(role));
                    composites.Where(role => role.ContainerId == null || role.Name == null).IfAny(
                        invalid => throw new ConflictException($"composites roles containerId or name must not be null: {string.Join(" ", invalid.Select(x => $"[{string.Join(",", x.Id, x.Name, x.Description, x.ContainerId)}]"))}"));

                    var remove = composites.ExceptBy(updates, role => joinUpdateKey(role));
                    await removeCompositeRoles(role.Name, remove).ConfigureAwait(false);

                    var add = await updates.Except(composites.Select(role => joinUpdateKey(role)))
                        .Select(x => getRoleByName(x))
                        .ToEnumerableTask()
                        .ConfigureAwait(false);
                    await addCompositeRoles(role.Name, add).ConfigureAwait(false);
                }
            }
        }
    }

    private static bool Compare(Role role, RoleModel update) =>
        role.Name == update.Name &&
        role.Description == update.Description &&
        role.Attributes.NullOrContentEqual(update.Attributes);

    private static Role CreateRole(RoleModel update) =>
        new Role
        {
            Name = update.Name,
            Description = update.Description,
            Composite = update.Composite,
            ClientRole = update.ClientRole,
            Attributes = update.Attributes?.ToDictionary(x => x.Key, x => x.Value)
        };

    private static Role CreateUpdateRole(string id, string containerId, RoleModel update)
    {
        var role = CreateRole(update);
        role.Id = id;
        role.ContainerId = containerId;
        return role;
    }
}
