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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;
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

    public Task UpdateClients(string keycloakInstanceName)
    {
        var realm = _seedData.Realm;
        var keycloak = _keycloakFactory.CreateKeycloakClient(keycloakInstanceName);
        return _seedData.SetClientInternalIds(UpdateClientsInternal(keycloak, realm));
    }

    private async IAsyncEnumerable<(string ClientId, string Id)> UpdateClientsInternal(KeycloakClient keycloak, string realm)
    {
        foreach (var update in _seedData.Clients)
        {
            if (update.ClientId == null)
                throw new ConflictException($"clientId must not be null {update.Id}");
            var client = (await keycloak.GetClientsAsync(realm, clientId: update.ClientId).ConfigureAwait(false)).SingleOrDefault();
            if (client == null)
            {
                var id = await keycloak.CreateClientAndRetrieveClientIdAsync(realm, CreateUpdateClient(null, update)).ConfigureAwait(false);
                if (id == null)
                    throw new KeycloakNoSuccessException($"creation of client {update.ClientId} did not return the expected result");
                yield return (update.ClientId, id);
            }
            else if (!Compare(client, update))
            {
                if (client.Id == null)
                    throw new ConflictException($"client.Id must not be null: clientId {update.ClientId}");
                var updateClient = CreateUpdateClient(client.Id, update);
                await keycloak.UpdateClientAsync(
                    realm,
                    client.Id,
                    updateClient).ConfigureAwait(false);
                yield return (update.ClientId, client.Id);
            }
        }
    }

    private static Client CreateUpdateClient(string? id, ClientModel update) => new Client
    {
        Id = id,
        ClientId = update.ClientId,
        RootUrl = update.RootUrl,
        Name = update.Name,
        Description = update.Description,
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
        // ProtocolMappers = update.ProtocolMappers?.Select(x => new ClientProtocolMapper
        // {
        // }),
        DefaultClientScopes = update.DefaultClientScopes,
        OptionalClientScopes = update.OptionalClientScopes,
        Access = update.Access == null
            ? null
            : new ClientAccess
            {
                View = update.Access.View,
                Configure = update.Access.Configure,
                Manage = update.Access.Manage
            },
        // Secret = update.Secret,
        AuthorizationServicesEnabled = update.AuthorizationServicesEnabled
    };

    private static bool Compare(Client client, ClientModel update) =>
        client.ClientId == update.ClientId &&
        client.RootUrl == update.RootUrl &&
        client.Name == update.Name &&
        client.Description == update.Description &&
        client.BaseUrl == update.BaseUrl &&
        client.SurrogateAuthRequired == update.SurrogateAuthRequired &&
        client.Enabled == update.Enabled &&
        client.AlwaysDisplayInConsole == update.AlwaysDisplayInConsole &&
        client.ClientAuthenticatorType == update.ClientAuthenticatorType &&
        client.RedirectUris.NullOrContentEqual(update.RedirectUris) &&
        client.WebOrigins.NullOrContentEqual(update.WebOrigins) &&
        client.NotBefore == update.NotBefore &&
        client.BearerOnly == update.BearerOnly &&
        client.ConsentRequired == update.ConsentRequired &&
        client.StandardFlowEnabled == update.StandardFlowEnabled &&
        client.ImplicitFlowEnabled == update.ImplicitFlowEnabled &&
        client.DirectAccessGrantsEnabled == update.DirectAccessGrantsEnabled &&
        client.ServiceAccountsEnabled == update.ServiceAccountsEnabled &&
        client.PublicClient == update.PublicClient &&
        client.FrontChannelLogout == update.FrontchannelLogout &&
        client.Protocol == update.Protocol &&
        client.Attributes.NullOrContentEqual(update.Attributes) &&
        client.AuthenticationFlowBindingOverrides.NullOrContentEqual(update.AuthenticationFlowBindingOverrides) &&
        client.FullScopeAllowed == update.FullScopeAllowed &&
        client.NodeReregistrationTimeout == update.NodeReRegistrationTimeout &&
        client.DefaultClientScopes.NullOrContentEqual(update.DefaultClientScopes) &&
        client.OptionalClientScopes.NullOrContentEqual(update.OptionalClientScopes) &&
        Compare(client.Access, update.Access) &&
        // client.Secret == update.Secret &&
        client.AuthorizationServicesEnabled == update.AuthorizationServicesEnabled;

    private static bool Compare(ClientAccess? access, ClientAccessModel? updateAccess) =>
        access == null && updateAccess == null ||
        access != null && updateAccess != null &&
        access.Configure == updateAccess.Configure &&
        access.Manage == updateAccess.Manage &&
        access.View == updateAccess.View;
}
