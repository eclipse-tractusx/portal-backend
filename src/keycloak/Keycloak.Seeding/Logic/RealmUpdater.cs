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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Logic;

public class RealmUpdater
{
    private readonly Library.KeycloakClient _keycloak;
    private readonly string _realm;
    private readonly Library.Models.RealmsAdmin.Realm _keycloakRealm;
    private readonly KeycloakRealm _updateRealm;

    public RealmUpdater(Library.KeycloakClient keycloak, Library.Models.RealmsAdmin.Realm keycloakRealm, KeycloakRealm updateRealm)
    {
        if (keycloakRealm._Realm == null)
            throw new ArgumentException($"realm loaded from keycloak must not have _Realm set to null", nameof(keycloakRealm));
        if (updateRealm.Realm == null)
            throw new ArgumentException($"realm loaded from json must not have Realm set to null", nameof(updateRealm));
        if (keycloakRealm._Realm != updateRealm.Realm)
            throw new ArgumentException($"realm loaded from json must be the same as loaded from keycloak");
        _keycloak = keycloak;
        _realm = keycloakRealm._Realm;
        _keycloakRealm = keycloakRealm;
        _updateRealm = updateRealm;
    }

    public async Task Update()
    {
        UpdateRealm(_keycloakRealm, _updateRealm);
        await _keycloak.UpdateRealmAsync(_realm, _keycloakRealm).ConfigureAwait(false);

        await UpdateRealmRoles(_updateRealm.Roles?.Realm).ConfigureAwait(false);

        if (_updateRealm.Clients != null)
        {
            var clients = await UpdateClients(_updateRealm.Clients).ConfigureAwait(false);
            if (_updateRealm.Roles?.Client != null)
            {
                await UpdateClientRoles(clients, _updateRealm.Roles.Client.Select(x => (x.Key, x.Value))).ConfigureAwait(false);
            }
            if (_updateRealm.Roles != null)
            {
                await UpdateCompositeRoles(clients, _updateRealm.Roles).ConfigureAwait(false);
            }
        }
        if (_updateRealm.IdentityProviders != null)
        {
            await UpdateIdentityProviders(_updateRealm.IdentityProviders, _updateRealm.IdentityProviderMappers).ConfigureAwait(false);
        }
    }

    public async Task<IEnumerable<Library.Models.Clients.Client>> UpdateClients(IEnumerable<ClientModel> updateClients)
    {
        var clients = await _keycloak.GetClientsAsync(_realm).ConfigureAwait(false);

        Library.Models.Clients.Client CreateUpdateClient(string? id, ClientModel update) => new Library.Models.Clients.Client
        {
            Id = id,
            ClientId = update.ClientId,
            RootUrl = update.RootUrl,
            Name = update.Name,
            BaseUrl = update.BaseUrl,
            SurrogateAuthRequired = update.SurrogateAuthRequired,
            Enabled = update.Enabled,
            AlwaysDisplayInConsole = update.AlwaysDisplayInConsole,
            ClientAuthenticatorType = update.ClientAuthenticatorType,
            RedirectUris = update.RedirectUris,
            WebOrigins = update.WebOrigins,
            NotBefore = update.NotBefore,
            BearerOnly = update.BearerOnly,
            ConsentRequired = update.ConsentRequired,
            StandardFlowEnabled = update.StandardFlowEnabled,
            ImplicitFlowEnabled = update.ImplicitFlowEnabled,
            DirectAccessGrantsEnabled = update.DirectAccessGrantsEnabled,
            ServiceAccountsEnabled = update.ServiceAccountsEnabled,
            PublicClient = update.PublicClient,
            FrontChannelLogout = update.FrontchannelLogout,
            Protocol = update.Protocol,
            Attributes = update.Attributes?.ToDictionary(x => x.Key, x => x.Value),
            AuthenticationFlowBindingOverrides = update.AuthenticationFlowBindingOverrides?.ToDictionary(x => x.Key, x => x.Value),
            FullScopeAllowed = update.FullScopeAllowed,
            NodeReregistrationTimeout = update.NodeReRegistrationTimeout,
            // ProtocolMappers = update.ProtocolMappers?.Select(x => new Library.Models.Clients.ClientProtocolMapper
            // {
            // }),
            DefaultClientScopes = update.DefaultClientScopes,
            OptionalClientScopes = update.OptionalClientScopes,
            Access = update.Access == null
                ? null
                : new Library.Models.Clients.ClientAccess
                {
                    View = update.Access.View,
                    Configure = update.Access.Configure,
                    Manage = update.Access.Manage
                },
            Secret = update.Secret,
            AuthorizationServicesEnabled = update.AuthorizationServicesEnabled
        };

        var updates = clients.Join(
            updateClients,
            x => x.ClientId,
            x => x.ClientId,
            (client, update) => CreateUpdateClient(client.Id, update)).ToList();

        foreach (var update in updates)
        {
            if (update.Id == null)
                throw new ConflictException($"Id must not be null: clientId {update.ClientId}");
            await _keycloak.UpdateClientAsync(_realm, update.Id, update).ConfigureAwait(false);
        }

        var creates = updateClients.ExceptBy(clients.Select(x => x.ClientId), x => x.ClientId).Select(update => CreateUpdateClient(null, update)).ToList();
        foreach (var create in creates)
        {
            create.Id = await _keycloak.CreateClientAndRetrieveClientIdAsync(_realm, create).ConfigureAwait(false);
        }
        return updates.Concat(creates);
    }

    private async Task UpdateClientRoles(IEnumerable<Library.Models.Clients.Client> clients, IEnumerable<(string ClientId, IReadOnlyList<RoleModel> Roles)> updateClientRoles)
    {
        foreach (var (clientId, updateRoles) in updateClientRoles)
        {
            var id = clients.SingleOrDefault(x => x.ClientId == clientId)?.Id;
            if (id == null)
                throw new ConflictException($"unknown clientId {clientId}");

            var roles = await _keycloak.GetRolesAsync(_realm, id).ConfigureAwait(false);

            foreach (var newRole in updateRoles.ExceptBy(roles.Select(x => x.Name), x => x.Name))
            {
                await _keycloak.CreateRoleAsync(_realm, id, CreateRole(newRole));
            }
            await UpdateAndDeleteRoles(roles, updateRoles).ConfigureAwait(false);
        }
    }

    private async Task UpdateRealmRoles(IEnumerable<RoleModel>? updateRealmRoles)
    {
        var roles = await _keycloak.GetRolesAsync(_realm).ConfigureAwait(false);

        if (updateRealmRoles == null)
        {
            foreach (var role in roles)
            {
                if (role.Id == null)
                    throw new ConflictException($"role id must not be null: {role.Name}");
                await _keycloak.DeleteRoleByIdAsync(_realm, role.Id).ConfigureAwait(false);
            }
            return;
        }
        foreach (var newRole in updateRealmRoles.ExceptBy(roles.Select(x => x.Name), x => x.Name))
        {
            await _keycloak.CreateRoleAsync(_realm, CreateRole(newRole));
        }
        await UpdateAndDeleteRoles(roles, updateRealmRoles).ConfigureAwait(false);
    }

    private async Task UpdateAndDeleteRoles(IEnumerable<Library.Models.Roles.Role> roles, IEnumerable<RoleModel> updateRoles)
    {
        foreach (var joinedRole in roles.Join(
            updateRoles,
            x => x.Name,
            x => x.Name,
            (role, updateRole) => (Role: role, Update: updateRole)))
        {
            if (joinedRole.Role.Id == null)
                throw new ConflictException($"role id must not be null: {joinedRole.Role.Name}");
            if (joinedRole.Role.ContainerId == null)
                throw new ConflictException($"role containerId must not be null: {joinedRole.Role.Name}");
            await _keycloak.UpdateRoleByIdAsync(_realm, joinedRole.Role.Id, CreateUpdateRole(joinedRole.Role.Id, joinedRole.Role.ContainerId, joinedRole.Update)).ConfigureAwait(false);
        }
        foreach (var deleteRole in roles.ExceptBy(updateRoles.Select(x => x.Name), x => x.Name))
        {
            if (deleteRole.Id == null)
                throw new ConflictException($"role id must not be null: {deleteRole.Name}");
            await _keycloak.DeleteRoleByIdAsync(_realm, deleteRole.Id).ConfigureAwait(false);
        }
    }

    private async Task UpdateCompositeRoles(IEnumerable<Library.Models.Clients.Client> clients, RolesModel updateRolesModel)
    {
        if (updateRolesModel.Client != null)
        {
            await Task.WhenAll(updateRolesModel.Client.Select(updateClientRoles =>
            {
                var (clientId, updateRoles) = updateClientRoles;
                var id = clients.SingleOrDefault(x => x.ClientId == clientId)?.Id ?? throw new ConflictException($"unknown clientId {clientId}");

                return UpdateCompositeRolesInternal(
                    clients,
                    () => _keycloak.GetRolesAsync(_realm, id),
                    updateRoles,
                    (string name, IEnumerable<Library.Models.Roles.Role> roles) => _keycloak.RemoveCompositesFromRoleAsync(_realm, id, name, roles),
                    (string name, IEnumerable<Library.Models.Roles.Role> roles) => _keycloak.AddCompositesToRoleAsync(_realm, id, name, roles));
            })).ConfigureAwait(false);
        }

        if (updateRolesModel.Realm != null)
        {
            await UpdateCompositeRolesInternal(
                clients,
                () => _keycloak.GetRolesAsync(_realm),
                updateRolesModel.Realm,
                (string name, IEnumerable<Library.Models.Roles.Role> roles) => _keycloak.RemoveCompositesFromRoleAsync(_realm, name, roles),
                (string name, IEnumerable<Library.Models.Roles.Role> roles) => _keycloak.AddCompositesToRoleAsync(_realm, name, roles)).ConfigureAwait(false);
        }
    }

    private async Task UpdateCompositeRolesInternal(IEnumerable<Library.Models.Clients.Client> clients, Func<Task<IEnumerable<Library.Models.Roles.Role>>> getRoles, IReadOnlyList<RoleModel> updateRoles, Func<string, IEnumerable<Library.Models.Roles.Role>, Task> removeCompositeRoles, Func<string, IEnumerable<Library.Models.Roles.Role>, Task> addCompositeRoles)
    {
        var roles = await getRoles().ConfigureAwait(false);

        var updateClientComposites = updateRoles.Where(x => x.Composites?.Client?.Any() ?? false);

        var removeClientComposites = roles.Where(x => x.Composites?.Client?.Any() ?? false).ExceptBy(updateClientComposites.Select(x => x.Name), x => x.Name);

        foreach (var remove in removeClientComposites)
        {
            if (remove.Id == null || remove.Name == null)
                throw new ConflictException($"role.id or role.name must not be null {remove.Id} {remove.Name}");

            var clientComposites = (await _keycloak.GetRoleChildrenAsync(_realm, remove.Id).ConfigureAwait(false)).Where(x => x.ClientRole ?? false);
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
                        Id: clients.SingleOrDefault(c => c.ClientId == x.Key)?.Id ?? throw new ConflictException($"unknown clientId {x.Key}"),
                        Names: x.Value))
                    .SelectMany(x => x.Names.Select(name => (x.Id, Name: name)))));

        foreach (var (role, update) in joinedClientComposites)
        {
            if (role.Id == null || role.Name == null)
                throw new ConflictException($"role.id or role.name must not be null {role.Id} {role.Name}");
            var clientComposites = (await _keycloak.GetRoleChildrenAsync(_realm, role.Id).ConfigureAwait(false)).Where(x => x.ClientRole ?? false);
            clientComposites.Where(x => x.ContainerId == null || x.Name == null).IfAny(invalid => throw new ConflictException($"composites roles containerId or name must not be null: {string.Join(" ", invalid.Select(x => $"[{string.Join(",", x.Id, x.Name, x.Description, x.ContainerId)}]"))}"));

            var remove = clientComposites.ExceptBy(update, x => (x.ContainerId!, x.Name!));
            await removeCompositeRoles(role.Name, remove).ConfigureAwait(false);

            var add = await Task.WhenAll(update.Except(clientComposites.Select(x => (x.ContainerId!, x.Name!)))
                .Select(x => _keycloak.GetRoleByNameAsync(_realm, x.Item1, x.Item2))).ConfigureAwait(false);
            await addCompositeRoles(role.Name, add).ConfigureAwait(false);
        }

        var updateRealmComposites = updateRoles.Where(x => x.Composites?.Realm?.Any() ?? false); // imported roles this client with realm composite roles

        var removeRealmComposites = roles.Where(x => x.Composites?.Realm?.Any() ?? false).ExceptBy(updateRealmComposites.Select(x => x.Name), x => x.Name);

        foreach (var remove in removeRealmComposites)
        {
            if (remove.Id == null || remove.Name == null)
                throw new ConflictException($"role.id or role.name must not be null {remove.Id} {remove.Name}");

            var realmComposites = (await _keycloak.GetRoleChildrenAsync(_realm, remove.Id).ConfigureAwait(false)).Where(x => !(x.ClientRole ?? false));
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
            var realmComposites = (await _keycloak.GetRoleChildrenAsync(_realm, role.Id).ConfigureAwait(false)).Where(x => !(x.ClientRole ?? false));
            realmComposites.Where(x => x.Name == null).IfAny(invalid => throw new ConflictException($"composites roles name must not be null: {string.Join(" ", invalid.Select(x => $"[{string.Join(",", x.Id, x.Name, x.Description, x.ContainerId)}]"))}"));

            var remove = realmComposites.ExceptBy(update, x => x.Name);
            await removeCompositeRoles(role.Name, remove).ConfigureAwait(false);

            var add = await Task.WhenAll(update.Except(realmComposites.Select(x => x.Name!))
                .Select(x => _keycloak.GetRoleByNameAsync(_realm, x))).ConfigureAwait(false);
            await addCompositeRoles(role.Name, add);
        }
    }

    private static Library.Models.Roles.Role CreateRole(RoleModel updateRole) =>
        new Library.Models.Roles.Role
        {
            Name = updateRole.Name,
            Description = updateRole.Description,
            Composite = updateRole.Composite,
            // Composites = updateRole.Composites == null
            //     ? null
            //     : new Library.Models.Roles.RoleComposite
            //     {
            //         Realm = updateRole.Composites.Realm,
            //         Client = updateRole.Composites.Client?.ToDictionary(x => x.Key, x => x.Value.AsEnumerable())
            //     },
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

    public static void UpdateRealm(Library.Models.RealmsAdmin.Realm realm, KeycloakRealm jsonRealm)
    {
        realm._Realm = jsonRealm.Realm;
        realm.DisplayName = jsonRealm.DisplayName;
        realm.NotBefore = jsonRealm.NotBefore;
        realm.RevokeRefreshToken = jsonRealm.RevokeRefreshToken;
        realm.RefreshTokenMaxReuse = jsonRealm.RefreshTokenMaxReuse;
        realm.AccessTokenLifespan = jsonRealm.AccessTokenLifespan;
        realm.AccessTokenLifespanForImplicitFlow = jsonRealm.AccessTokenLifespanForImplicitFlow;
        realm.SsoSessionIdleTimeout = jsonRealm.SsoSessionIdleTimeout;
        realm.SsoSessionMaxLifespan = jsonRealm.SsoSessionMaxLifespan;
        realm.SsoSessionIdleTimeoutRememberMe = jsonRealm.SsoSessionIdleTimeoutRememberMe;
        realm.SsoSessionMaxLifespanRememberMe = jsonRealm.SsoSessionMaxLifespanRememberMe;
        realm.OfflineSessionIdleTimeout = jsonRealm.OfflineSessionIdleTimeout;
        realm.OfflineSessionMaxLifespanEnabled = jsonRealm.OfflineSessionMaxLifespanEnabled;
        realm.OfflineSessionMaxLifespan = jsonRealm.OfflineSessionMaxLifespan;
        realm.AccessCodeLifespan = jsonRealm.AccessCodeLifespan;
        realm.AccessCodeLifespanUserAction = jsonRealm.AccessCodeLifespanUserAction;
        realm.AccessCodeLifespanLogin = jsonRealm.AccessCodeLifespanLogin;
        realm.ActionTokenGeneratedByAdminLifespan = jsonRealm.ActionTokenGeneratedByAdminLifespan;
        realm.ActionTokenGeneratedByUserLifespan = jsonRealm.ActionTokenGeneratedByUserLifespan;
        realm.Enabled = jsonRealm.Enabled;
        realm.SslRequired = jsonRealm.SslRequired;
        realm.RegistrationAllowed = jsonRealm.RegistrationAllowed;
        realm.RegistrationEmailAsUsername = jsonRealm.RegistrationEmailAsUsername;
        realm.RememberMe = jsonRealm.RememberMe;
        realm.VerifyEmail = jsonRealm.VerifyEmail;
        realm.LoginWithEmailAllowed = jsonRealm.LoginWithEmailAllowed;
        realm.DuplicateEmailsAllowed = jsonRealm.DuplicateEmailsAllowed;
        realm.ResetPasswordAllowed = jsonRealm.ResetPasswordAllowed;
        realm.EditUsernameAllowed = jsonRealm.EditUsernameAllowed;
        realm.BruteForceProtected = jsonRealm.BruteForceProtected;
        realm.PermanentLockout = jsonRealm.PermanentLockout;
        realm.MaxFailureWaitSeconds = jsonRealm.MaxFailureWaitSeconds;
        realm.MinimumQuickLoginWaitSeconds = jsonRealm.MinimumQuickLoginWaitSeconds;
        realm.WaitIncrementSeconds = jsonRealm.WaitIncrementSeconds;
        realm.QuickLoginCheckMilliSeconds = jsonRealm.QuickLoginCheckMilliSeconds;
        realm.MaxDeltaTimeSeconds = jsonRealm.MaxDeltaTimeSeconds;
        realm.FailureFactor = jsonRealm.FailureFactor;
        // realm.DefaultRole = UpdateDefaultRole(realm.DefaultRole, jsonRealm.DefaultRole);
        realm.RequiredCredentials = jsonRealm.RequiredCredentials;
        realm.OtpPolicyType = jsonRealm.OtpPolicyType;
        realm.OtpPolicyAlgorithm = jsonRealm.OtpPolicyAlgorithm;
        realm.OtpPolicyInitialCounter = jsonRealm.OtpPolicyInitialCounter;
        realm.OtpPolicyDigits = jsonRealm.OtpPolicyDigits;
        realm.OtpPolicyLookAheadWindow = jsonRealm.OtpPolicyLookAheadWindow;
        realm.OtpPolicyPeriod = jsonRealm.OtpPolicyPeriod;
        realm.OtpSupportedApplications = jsonRealm.OtpSupportedApplications;
        realm.BrowserSecurityHeaders = UpdateBrowserSecurityHeaders(jsonRealm.BrowserSecurityHeaders);
        realm.SmtpServer = UpdateSmtpServer(jsonRealm.SmtpServer);
        realm.EventsEnabled = jsonRealm.EventsEnabled;
        realm.EventsListeners = jsonRealm.EventsListeners;
        realm.EnabledEventTypes = jsonRealm.EnabledEventTypes;
        realm.AdminEventsEnabled = jsonRealm.AdminEventsEnabled;
        realm.AdminEventsDetailsEnabled = jsonRealm.AdminEventsDetailsEnabled;
        // realm.IdentityProviders = UpdateIdentityProviders(realm.IdentityProviders, jsonRealm.IdentityProviders);
        realm.InternationalizationEnabled = jsonRealm.InternationalizationEnabled;
        realm.SupportedLocales = jsonRealm.SupportedLocales;
        realm.BrowserFlow = jsonRealm.BrowserFlow;
        realm.RegistrationFlow = jsonRealm.RegistrationFlow;
        realm.DirectGrantFlow = jsonRealm.DirectGrantFlow;
        realm.ResetCredentialsFlow = jsonRealm.ResetCredentialsFlow;
        realm.ClientAuthenticationFlow = jsonRealm.ClientAuthenticationFlow;
        realm.DockerAuthenticationFlow = jsonRealm.DockerAuthenticationFlow;
        realm.Attributes = jsonRealm.Attributes?.ToDictionary(x => x.Key, x => x.Value);
        realm.UserManagedAccessAllowed = jsonRealm.UserManagedAccessAllowed;
        //realm.PasswordPolicy = jsonRealm.PasswordPolicy;
        realm.LoginTheme = jsonRealm.LoginTheme;
    }

    private static Library.Models.Roles.Role? UpdateDefaultRole(Library.Models.Roles.Role? role, RoleModel? updateRole) =>
        updateRole == null
            ? null
            : new Library.Models.Roles.Role
            {
                Id = role == null
                    ? updateRole.Id
                    : role.Id,
                Name = updateRole.Name,
                Description = updateRole.Description,
                Composite = updateRole.Composite,
                ClientRole = updateRole.ClientRole,
                ContainerId = role == null
                    ? updateRole.ContainerId
                    : role.ContainerId,
                Attributes = updateRole.Attributes?.ToDictionary(x => x.Key, x => x.Value.AsEnumerable())
            };

    private static Library.Models.RealmsAdmin.BrowserSecurityHeaders? UpdateBrowserSecurityHeaders(BrowserSecurityHeadersModel? updateSecurityHeaders) =>
        updateSecurityHeaders == null
            ? null
            : new Library.Models.RealmsAdmin.BrowserSecurityHeaders
            {
                ContentSecurityPolicyReportOnly = updateSecurityHeaders.ContentSecurityPolicyReportOnly,
                XContentTypeOptions = updateSecurityHeaders.XContentTypeOptions,
                XRobotsTag = updateSecurityHeaders.XRobotsTag,
                XFrameOptions = updateSecurityHeaders.XFrameOptions,
                XXssProtection = updateSecurityHeaders.XXSSProtection,
                ContentSecurityPolicy = updateSecurityHeaders.ContentSecurityPolicy,
                StrictTransportSecurity = updateSecurityHeaders.StrictTransportSecurity
            };

    private static Library.Models.RealmsAdmin.SmtpServer? UpdateSmtpServer(SmtpServerModel? updateSmtpServer) =>
        updateSmtpServer == null
            ? null
            : new Library.Models.RealmsAdmin.SmtpServer
            {
                Host = updateSmtpServer.Host,
                Ssl = updateSmtpServer.Ssl,
                StartTls = updateSmtpServer.Starttls,
                User = updateSmtpServer.User,
                Password = updateSmtpServer.Password,
                Auth = updateSmtpServer.Auth,
                From = updateSmtpServer.From,
                FromDisplayName = updateSmtpServer.FromDisplayName,
                ReplyTo = updateSmtpServer.ReplyTo,
                ReplyToDisplayName = updateSmtpServer.ReplyToDisplayName,
                EnvelopeFrom = updateSmtpServer.EnvelopeFrom,
                Port = updateSmtpServer.Port
            };

    private async Task UpdateIdentityProviders(IEnumerable<IdentityProviderModel> updateIdentityProviders, IEnumerable<IdentityProviderMapperModel>? updateIdentityProviderMappers)
    {
        foreach (var updateIdentityProvider in updateIdentityProviders)
        {
            if (updateIdentityProvider.Alias == null)
                throw new ConflictException($"identityProvider alias must not be null: {updateIdentityProvider.InternalId} {updateIdentityProvider.DisplayName}");


            var updateMappers = updateIdentityProviderMappers?.Where(x => x.IdentityProviderAlias == updateIdentityProvider.Alias) ?? Enumerable.Empty<IdentityProviderMapperModel>();
            IEnumerable<IdentityProviderMapperModel> createMappers;
            try
            {
                var identityProvider = await _keycloak.GetIdentityProviderAsync(_realm, updateIdentityProvider.Alias).ConfigureAwait(false);
                UpdateIdentityProvider(identityProvider, updateIdentityProvider);
                await _keycloak.UpdateIdentityProviderAsync(_realm, updateIdentityProvider.Alias, identityProvider).ConfigureAwait(false);

                var mappers = await _keycloak.GetIdentityProviderMappersAsync(_realm, updateIdentityProvider.Alias).ConfigureAwait(false);

                createMappers = updateMappers.ExceptBy(mappers.Select(x => x.Name), x => x.Name);

                await Task.WhenAll(
                    mappers.Join(
                        updateMappers,
                        x => x.Name,
                        x => x.Name,
                        (mapper, update) =>
                            _keycloak.UpdateIdentityProviderMapperAsync(
                                _realm,
                                updateIdentityProvider.Alias,
                                mapper.Id ?? throw new ConflictException($"identityProviderMapper.id must never be null {mapper.Name} {mapper.IdentityProviderAlias}"),
                                UpdateIdentityProviderMapper(mapper, update))))
                    .ConfigureAwait(false);

                await Task.WhenAll(
                    mappers
                        .ExceptBy(updateMappers.Select(x => x.Name), x => x.Name)
                        .Select(mapper =>
                            _keycloak.DeleteIdentityProviderMapperAsync(
                                _realm,
                                updateIdentityProvider.Alias,
                                mapper.Id ?? throw new ConflictException($"identityProviderMapper.id must never be null {mapper.Name} {mapper.IdentityProviderAlias}"))))
                    .ConfigureAwait(false);
            }
            catch(KeycloakEntityNotFoundException)
            {
                var identityProvider = new Library.Models.IdentityProviders.IdentityProvider();
                UpdateIdentityProvider(identityProvider, updateIdentityProvider);
                await _keycloak.CreateIdentityProviderAsync(_realm, identityProvider).ConfigureAwait(false);
                createMappers = updateMappers;
            }

            await Task.WhenAll(
                createMappers
                    .Select(mapper =>
                        _keycloak.AddIdentityProviderMapperAsync(
                            _realm,
                            updateIdentityProvider.Alias,
                            UpdateIdentityProviderMapper(new Library.Models.IdentityProviders.IdentityProviderMapper
                                {
                                    Name = mapper.Name,
                                    IdentityProviderAlias = mapper.IdentityProviderAlias
                                },
                                mapper))))
                .ConfigureAwait(false);
        }
    }

    private static Library.Models.IdentityProviders.IdentityProvider UpdateIdentityProvider(Library.Models.IdentityProviders.IdentityProvider provider, IdentityProviderModel update)
    {
        provider.Alias = update.Alias;
        provider.DisplayName = update.DisplayName;
        provider.ProviderId = update.ProviderId;
        provider.Enabled = update.Enabled;
        provider.UpdateProfileFirstLoginMode = update.UpdateProfileFirstLoginMode;
        provider.TrustEmail = update.TrustEmail;
        provider.StoreToken = update.StoreToken;
        provider.AddReadTokenRoleOnCreate = update.AddReadTokenRoleOnCreate;
        provider.AuthenticateByDefault = update.AuthenticateByDefault;
        provider.LinkOnly = update.LinkOnly;
        provider.FirstBrokerLoginFlowAlias = update.FirstBrokerLoginFlowAlias;
        provider.Config = update.Config == null
            ? null
            : new Library.Models.IdentityProviders.Config
            {
                HideOnLoginPage = update.Config.HideOnLoginPage,
                //ClientSecret = update.Config.ClientSecret,
                DisableUserInfo = update.Config.DisableUserInfo,
                ValidateSignature = update.Config.ValidateSignature,
                ClientId = update.Config.ClientId,
                TokenUrl = update.Config.TokenUrl,
                AuthorizationUrl = update.Config.AuthorizationUrl,
                ClientAuthMethod = update.Config.ClientAuthMethod,
                JwksUrl = update.Config.JwksUrl,
                LogoutUrl = update.Config.LogoutUrl,
                ClientAssertionSigningAlg = update.Config.ClientAssertionSigningAlg,
                SyncMode = update.Config.SyncMode,
                UseJwksUrl = update.Config.UseJwksUrl,
                UserInfoUrl = update.Config.UserInfoUrl,
                Issuer = update.Config.Issuer,
                // for Saml:
                NameIDPolicyFormat = update.Config.NameIDPolicyFormat,
                PrincipalType = update.Config.PrincipalType,
                SignatureAlgorithm = update.Config.SignatureAlgorithm,
                XmlSigKeyInfoKeyNameTransformer = update.Config.XmlSigKeyInfoKeyNameTransformer,
                AllowCreate = update.Config.AllowCreate,
                EntityId = update.Config.EntityId,
                AuthnContextComparisonType = update.Config.AuthnContextComparisonType,
                BackchannelSupported = update.Config.BackchannelSupported,
                PostBindingResponse = update.Config.PostBindingResponse,
                PostBindingAuthnRequest = update.Config.PostBindingAuthnRequest,
                PostBindingLogout = update.Config.PostBindingLogout,
                WantAuthnRequestsSigned = update.Config.WantAuthnRequestsSigned,
                WantAssertionsSigned = update.Config.WantAssertionsSigned,
                WantAssertionsEncrypted = update.Config.WantAssertionsEncrypted,
                ForceAuthn = update.Config.ForceAuthn,
                SignSpMetadata = update.Config.SignSpMetadata,
                LoginHint = update.Config.LoginHint,
                SingleSignOnServiceUrl = update.Config.SingleSignOnServiceUrl,
                AllowedClockSkew = update.Config.AllowedClockSkew,
                AttributeConsumingServiceIndex = update.Config.AttributeConsumingServiceIndex
        };
        return provider;
    }

    public static Library.Models.IdentityProviders.IdentityProviderMapper UpdateIdentityProviderMapper(Library.Models.IdentityProviders.IdentityProviderMapper mapper, IdentityProviderMapperModel updateMapper)
    {
        mapper._IdentityProviderMapper = updateMapper.IdentityProviderMapper;
        mapper.Config = updateMapper.Config?.ToDictionary(x => x.Key, x => x.Value);
        return mapper;
    }
}
