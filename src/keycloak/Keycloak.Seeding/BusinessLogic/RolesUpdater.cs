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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class RolesUpdater(IKeycloakFactory keycloakFactory, ISeedDataHandler seedDataHandler)
    : IRolesUpdater
{
    public async Task UpdateClientRoles(string keycloakInstanceName, CancellationToken cancellationToken)
    {
        var keycloak = keycloakFactory.CreateKeycloakClient(keycloakInstanceName);
        var realm = seedDataHandler.Realm;
        var seederConfig = seedDataHandler.GetSpecificConfiguration(ConfigurationKey.ClientRoles);

        foreach (var (clientId, updateRoles) in seedDataHandler.ClientRoles)
        {
            var id = seedDataHandler.GetIdOfClient(clientId);
            var roles = await keycloak.GetRolesAsync(realm, id, cancellationToken: cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

            foreach (var newRole in updateRoles
                         .Where(x => seederConfig.ModificationAllowed(ModificationType.Create, x.Name))
                         .ExceptBy(roles.Select(role => role.Name), roleModel => roleModel.Name))
            {
                await keycloak.CreateRoleAsync(realm, id, CreateRole(newRole), cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            }

            await UpdateAndDeleteRoles(keycloak, realm, roles, updateRoles, seederConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    public async Task UpdateRealmRoles(string keycloakInstanceName, CancellationToken cancellationToken)
    {
        var keycloak = keycloakFactory.CreateKeycloakClient(keycloakInstanceName);
        var realm = seedDataHandler.Realm;
        var seederConfig = seedDataHandler.GetSpecificConfiguration(ConfigurationKey.Roles);
        var roles = await keycloak.GetRolesAsync(realm, cancellationToken: cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var updateRealmRoles = seedDataHandler.RealmRoles;

        foreach (var newRole in updateRealmRoles
                     .Where(x => seederConfig.ModificationAllowed(ModificationType.Create, x.Name))
                     .ExceptBy(roles.Select(role => role.Name), roleModel => roleModel.Name))
        {
            await keycloak.CreateRoleAsync(realm, CreateRole(newRole), cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        await UpdateAndDeleteRoles(keycloak, realm, roles, updateRealmRoles, seederConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private static async Task UpdateAndDeleteRoles(KeycloakClient keycloak, string realm, IEnumerable<Role> roles, IEnumerable<RoleModel> updateRoles, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        foreach (var (role, update) in
            roles
                .Where(x => seederConfig.ModificationAllowed(ModificationType.Update, x.Name))
                .Join(
                    updateRoles,
                    role => role.Name,
                    roleModel => roleModel.Name,
                    (role, roleModel) => (Role: role, Update: roleModel)))
        {
            if (!CompareRole(role, update))
            {
                if (role.Id == null)
                    throw new ConflictException($"role id must not be null: {role.Name}");
                if (role.ContainerId == null)
                    throw new ConflictException($"role containerId must not be null: {role.Name}");

                await keycloak.UpdateRoleByIdAsync(realm, role.Id, CreateUpdateRole(role.Id, role.ContainerId, update), cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            }
        }

        foreach (var deleteRole in roles
                .Where(x => seederConfig.ModificationAllowed(ModificationType.Delete, x.Name))
                .ExceptBy(updateRoles.Select(roleModel => roleModel.Name), role => role.Name))
        {
            if (deleteRole.Id == null)
                throw new ConflictException($"role id must not be null: {deleteRole.Name}");

            await keycloak.DeleteRoleByIdAsync(realm, deleteRole.Id, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    public async Task UpdateCompositeRoles(string keycloakInstanceName, CancellationToken cancellationToken)
    {
        var keycloak = keycloakFactory.CreateKeycloakClient(keycloakInstanceName);
        var realm = seedDataHandler.Realm;
        var seederConfig = seedDataHandler.GetSpecificConfiguration(ConfigurationKey.Roles);

        foreach (var (clientId, updateRoles) in seedDataHandler.ClientRoles)
        {
            var id = seedDataHandler.GetIdOfClient(clientId);

            await UpdateCompositeRolesInner(
                keycloak,
                realm,
                seederConfig,
                updateRoles,
                () => keycloak.GetRolesAsync(realm, id, cancellationToken: cancellationToken),
                (name, roles) => keycloak.RemoveCompositesFromRoleAsync(realm, id, name, roles, cancellationToken),
                (name, roles) => keycloak.AddCompositesToRoleAsync(realm, id, name, roles, cancellationToken),
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        await UpdateCompositeRolesInner(
            keycloak,
            realm,
            seederConfig,
            seedDataHandler.RealmRoles,
            () => keycloak.GetRolesAsync(realm, cancellationToken: cancellationToken),
            (name, roles) => keycloak.RemoveCompositesFromRoleAsync(realm, name, roles, cancellationToken),
            (name, roles) => keycloak.AddCompositesToRoleAsync(realm, name, roles, cancellationToken),
            cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task UpdateCompositeRolesInner(
        KeycloakClient keycloak,
        string realm,
        KeycloakSeederConfigModel seederConfig,
        IEnumerable<RoleModel> updateRoles,
        Func<Task<IEnumerable<Role>>> getRoles,
        Func<string, IEnumerable<Role>, Task> removeCompositeRoles,
        Func<string, IEnumerable<Role>, Task> addCompositeRoles,
        CancellationToken cancellationToken)
    {
        var roles = await getRoles().ConfigureAwait(ConfigureAwaitOptions.None);

        await RemoveAddCompositeRolesInner<(string ContainerId, string Name)>(
            keycloak,
            realm,
            seederConfig,
            updateRoles,
            roles,
            removeCompositeRoles,
            addCompositeRoles,
            roleModel => roleModel.Composites?.Client?.Any() ?? false,
            role => role.Composites?.Client?.Any() ?? false,
            role => role.ClientRole ?? false,
            roleModel => roleModel.Composites?.Client?
                    .FilterNotNullValues()
                    .Select(x => (
                        Id: seedDataHandler.GetIdOfClient(x.Key),
                        Names: x.Value))
                    .SelectMany(x => x.Names.Select(name => (x.Id, name))) ?? throw new ConflictException($"roleModel.Composites.Client is null: {roleModel.Id} {roleModel.Name}"),
            role => (
                role.ContainerId ?? throw new ConflictException($"role.ContainerId is null: {role.Id} {role.Name}"),
                role.Name ?? throw new ConflictException($"role.Name is null: {role.Id}")),
            async x => await keycloak.GetRoleByNameAsync(realm, x.ContainerId, x.Name, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None),
            cancellationToken
        ).ConfigureAwait(ConfigureAwaitOptions.None);

        await RemoveAddCompositeRolesInner(
            keycloak,
            realm,
            seederConfig,
            updateRoles,
            roles,
            removeCompositeRoles,
            addCompositeRoles,
            roleModel => roleModel.Composites?.Realm?.Any() ?? false,
            role => role.Composites?.Realm?.Any() ?? false,
            role => !(role.ClientRole ?? false),
            roleModel => roleModel.Composites?.Realm ?? throw new ConflictException($"roleModel.Composites.Realm is null: {roleModel.Id} {roleModel.Name}"),
            role => role.Name ?? throw new ConflictException($"role.Name is null: {role.Id}"),
            async name => await keycloak.GetRoleByNameAsync(realm, name, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None),
            cancellationToken
        ).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private static async Task RemoveAddCompositeRolesInner<T>(
        KeycloakClient keycloak,
        string realm,
        KeycloakSeederConfigModel seederConfig,
        IEnumerable<RoleModel> updateRoles,
        IEnumerable<Role> roles,
        Func<string, IEnumerable<Role>, Task> removeCompositeRoles,
        Func<string, IEnumerable<Role>, Task> addCompositeRoles,
        Func<RoleModel, bool> compositeRolesUpdatePredicate,
        Func<Role, bool> compositeRolesPredicate,
        Func<Role, bool> rolePredicate,
        Func<RoleModel, IEnumerable<T>> joinUpdateSelector,
        Func<Role, T> joinUpdateKey,
        Func<T, ValueTask<Role>> getRoleByName,
        CancellationToken cancellationToken)
    {
        var updateComposites = updateRoles.Where(x => compositeRolesUpdatePredicate(x));
        var removeComposites = roles.Where(x => compositeRolesPredicate(x)).ExceptBy(updateComposites.Select(roleModel => roleModel.Name), role => role.Name);

        await RemoveRoles(keycloak, realm, removeCompositeRoles, rolePredicate, removeComposites, cancellationToken);

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
            var composites = (await keycloak.GetRoleChildrenAsync(realm, role.Id, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)).Where(role => rolePredicate(role));
            composites.Where(role => role.ContainerId == null || role.Name == null).IfAny(
                invalid => throw new ConflictException($"composites roles containerId or name must not be null: {string.Join(" ", invalid.Select(x => $"[{string.Join(",", x.Id, x.Name, x.Description, x.ContainerId)}]"))}"));

            var remove = composites.ExceptBy(updates, role => joinUpdateKey(role)).Where(x => seederConfig.ModificationAllowed(ModificationType.Delete, role.Name) || seederConfig.ModificationAllowed(ModificationType.Delete, x.Name));
            await removeCompositeRoles(role.Name, remove).ConfigureAwait(ConfigureAwaitOptions.None);

            var add = await updates.Except(composites.Select(role => joinUpdateKey(role)))
                .ToAsyncEnumerable()
                .SelectAwait(x => getRoleByName(x))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            await addCompositeRoles(role.Name, add.Where(x => seederConfig.ModificationAllowed(ModificationType.Create, role.Name) || seederConfig.ModificationAllowed(ModificationType.Create, x.Name))).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task RemoveRoles(
        KeycloakClient keycloak,
        string realm,
        Func<string, IEnumerable<Role>, Task> removeCompositeRoles,
        Func<Role, bool> rolePredicate,
        IEnumerable<Role> removeComposites,
        CancellationToken cancellationToken)
    {
        foreach (var remove in removeComposites)
        {
            if (remove.Id == null || remove.Name == null)
                throw new ConflictException($"role.id or role.name must not be null {remove.Id} {remove.Name}");

            var composites = (await keycloak.GetRoleChildrenAsync(realm, remove.Id, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)).Where(role => rolePredicate(role));
            await removeCompositeRoles(remove.Name, composites).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static bool CompareRole(Role role, RoleModel update) =>
        role.Name == update.Name &&
        role.Description == update.Description &&
        role.Attributes.NullOrContentEqual(update.Attributes?.FilterNotNullValues());

    private static Role CreateRole(RoleModel update) =>
        new Role
        {
            Name = update.Name,
            Description = update.Description,
            Composite = update.Composite,
            ClientRole = update.ClientRole,
            Attributes = update.Attributes?.FilterNotNullValues().ToDictionary()
        };

    private static Role CreateUpdateRole(string id, string containerId, RoleModel update)
    {
        var role = CreateRole(update);
        role.Id = id;
        role.ContainerId = containerId;
        return role;
    }
}
