/********************************************************************************
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

using Flurl;
using Flurl.Http;
using Flurl.Http.Testing;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.OpenIDConfiguration;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;
using System.Net;
using IdentityProvider = Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders.IdentityProvider;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests.FlurlSetup;

public static class FlurlSetupExtensions
{
    public static HttpTest WithAuthorization(this HttpTest testClient)
    {
        testClient.ForCallsTo("*/auth/realms/*/protocol/openid-connect/token")
            .RespondWithJson(new { access_token = "123" });
        return testClient;
    }

    public static HttpTest WithCreateClient(this HttpTest testClient, string newClientId)
    {
        testClient.ForCallsTo("*/admin/realms/*/clients")
            .WithVerb(HttpMethod.Post)
            .RespondWith("Ok", 200,
                new[] { new ValueTuple<string, object>("Location", new Uri($"https://www.test.de/{newClientId}")) });
        return testClient;
    }

    public static HttpTest WithGetUserForServiceAccount(this HttpTest testClient, string clientId, User user)
    {
        testClient.ForCallsTo($"*/admin/realms/*/clients/{clientId}/service-account-user")
            .WithVerb(HttpMethod.Get)
            .RespondWithJson(user);
        return testClient;
    }

    public static HttpTest WithGetRoleByNameAsync(this HttpTest testClient, string clientId, string roleName, Role role)
    {
        testClient.ForCallsTo($"*/admin/realms/*/clients/{clientId}/roles/{roleName}")
            .WithVerb(HttpMethod.Get)
            .RespondWithJson(role);
        return testClient;
    }

    public static HttpTest WithGetClientSecretAsync(this HttpTest testClient, string clientId, Credentials credentials)
    {
        testClient.ForCallsTo($"*/admin/realms/*/clients/{clientId}/client-secret")
            .WithVerb(HttpMethod.Get)
            .RespondWithJson(credentials);
        return testClient;
    }

    public static HttpTest WithGetOpenIdConfigurationAsync(this HttpTest testClient, OpenIDConfiguration config)
    {
        testClient.ForCallsTo($"*/realms/*/.well-known/openid-configuration")
            .WithVerb(HttpMethod.Get)
            .RespondWithJson(config);
        return testClient;
    }

    public static HttpTest WithGetIdentityProviderAsync(this HttpTest testClient, string alias, IdentityProvider idp)
    {
        testClient.ForCallsTo($"*/admin/realms/*/identity-provider/instances/{alias}")
            .WithVerb(HttpMethod.Get)
            .RespondWithJson(idp);
        return testClient;
    }

    public static HttpTest WithGetClientsAsync(this HttpTest testClient, string alias, IEnumerable<Client> clients)
    {
        testClient.ForCallsTo($"*/admin/realms/{alias}/clients")
            .WithVerb(HttpMethod.Get)
            .RespondWithJson(clients);
        return testClient;
    }

    public static HttpTest WithGetClientAsync(this HttpTest testClient, string alias, string clientId, Client client)
    {
        testClient.ForCallsTo($"*/admin/realms/{alias}/clients/{clientId}")
            .WithVerb(HttpMethod.Get)
            .RespondWithJson(client);
        return testClient;
    }

    public static HttpTest With(this HttpTest testClient, string alias, string clientId, Client client)
    {
        testClient.ForCallsTo($"*/admin/realms/{alias}/clients/{clientId}")
            .WithVerb(HttpMethod.Get)
            .RespondWithJson(client);
        return testClient;
    }

    public static HttpTest WithGetRealmAsync(this HttpTest testClient, string alias, Realm realm)
    {
        testClient.ForCallsTo($"*/admin/realms/{alias}")
            .WithVerb(HttpMethod.Get)
            .RespondWithJson(realm);
        return testClient;
    }

    public static HttpTest WithGetUsersAsync(this HttpTest testClient, IEnumerable<User> users)
    {
        testClient.ForCallsTo("*/admin/realms/*/users")
            .WithVerb(HttpMethod.Get)
            .RespondWithJson(users);
        return testClient;
    }

    public static HttpTest WithGetUsersFailing(this HttpTest testClient, HttpStatusCode statusCode)
    {
        testClient.ForCallsTo("*/admin/realms/*/users")
            .WithVerb(HttpMethod.Get)
            .SimulateException(new FlurlHttpException(new FlurlCall
            {
                HttpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "/test"),
                Request = new FlurlRequest(new Uri("https://test.de")),
                HttpResponseMessage = new HttpResponseMessage(statusCode) { ReasonPhrase = "test" },
                Response = new FlurlResponse(new HttpResponseMessage(statusCode))
            }));
        return testClient;
    }
}
