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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class ClientsUpdater : IClientsUpdater
{
    private readonly IKeycloakFactory _keycloakFactory;
    private readonly ISeedDataHandler _seedData;

    public ClientsUpdater(IKeycloakFactory keycloakFactory, ISeedDataHandler seedDataHandler)
    {
        _keycloakFactory = keycloakFactory;
        _seedData = seedDataHandler;
    }

    public async Task UpdateClients(string keycloakInstanceName)
    {
        var realm = _seedData.Realm;
        var keycloak = _keycloakFactory.CreateKeycloakClient(keycloakInstanceName);

        var clients = await keycloak.GetClientsAsync(realm).ConfigureAwait(false);

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

        var updateClients = _seedData.Clients;
        var updates = clients.Join(
            updateClients,
            x => x.ClientId,
            x => x.ClientId,
            (client, update) => CreateUpdateClient(client.Id, update)).ToList();

        await Task.WhenAll(
            updates.Select(update =>
                keycloak.UpdateClientAsync(
                    realm,
                    update.Id ?? throw new ConflictException($"Id must not be null: clientId {update.ClientId}"),
                    update)))
            .ConfigureAwait(false);

        var creates = updateClients.ExceptBy(clients.Select(x => x.ClientId), x => x.ClientId).Select(update => CreateUpdateClient(null, update)).ToList();
        foreach (var create in creates)
        {
            create.Id = await keycloak.CreateClientAndRetrieveClientIdAsync(realm, create).ConfigureAwait(false);
        }
        _seedData.ClientsDictionary = updates.Concat(creates).ToDictionary(x => x.ClientId!, x => x.Id!);
    }
}
