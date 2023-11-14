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

using Flurl.Http.Testing;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.OpenIDConfiguration;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests.FlurlSetup;
using Config = Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders.Config;
using IdentityProvider = Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;
using Idp = Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests;

public class ProvisioningManagerTests
{
    private const string ValidClientName = "valid";
    private const string CentralRealm = "test";
    private const string OrgName = "Stark Industries";
    private const string LoginTheme = "colorful-theme";
    private const string CentralUrl = "https://central.de";
    private const string SharedUrl = "https://shared.de";
    private readonly IProvisioningManager _sut;
    private readonly IProvisioningDBAccess _provisioningDbAccess;

    public ProvisioningManagerTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var keycloakFactory = A.Fake<IKeycloakFactory>();
        _provisioningDbAccess = A.Fake<IProvisioningDBAccess>();
        A.CallTo(() => keycloakFactory.CreateKeycloakClient("central"))
            .Returns(new KeycloakClient(CentralUrl, "test", "test", "test", false));
        A.CallTo(() => keycloakFactory.CreateKeycloakClient("shared"))
            .Returns(new KeycloakClient(SharedUrl, "test", "test", "test", false));
        A.CallTo(() => keycloakFactory.CreateKeycloakClient("shared", A<string>._, A<string>._))
            .Returns(new KeycloakClient(SharedUrl, "test", "test", "test", false));
        var settings = new ProvisioningSettings
        {
            ClientPrefix = "cl",
            CentralOIDCClient = new Client
            {
                RedirectUris = new List<string>()
            },
            CentralRealm = CentralRealm,
            CentralIdentityProvider = new Idp.IdentityProvider
            {
                ProviderId = "keycloak-oidc",
                FirstBrokerLoginFlowAlias = "first broker login",
                Config = new Config
                {
                    ClientAuthMethod = "private_key_jwt",
                    HideOnLoginPage = "true",
                    SyncMode = "true"
                }
            },
            ServiceAccountClientPrefix = "sa",
            ServiceAccountClient = new Client
            {
                Protocol = "openid-connect"
            },
            SharedRealm = new Realm
            {
                Enabled = true,
            },
            SharedRealmClient = new Client
            {
                Protocol = "openid-connect",
                ClientAuthenticatorType = "client-jwt",
                Enabled = true,
                Attributes = new Dictionary<string, string>()
            },
        };

        _sut = new ProvisioningManager(keycloakFactory, _provisioningDbAccess, Options.Create(settings));
    }

    [Fact]
    public async Task SetupClientAsync_CallsExpected()
    {
        // Arrange
        const string url = "https://newurl.com";
        const string newClientId = "cl1";
        using var httpTest = new HttpTest();
        A.CallTo(() => _provisioningDbAccess.GetNextClientSequenceAsync()).ReturnsLazily(() => 1);
        httpTest.WithAuthorization()
            .WithCreateClient(newClientId)
            .WithGetClientSecretAsync(newClientId, new Credentials { Value = "super-secret" });

        // Act
        await _sut.SetupClientAsync($"{url}/*", url, new[] { "adminRole" }).ConfigureAwait(false);

        // Assert
        httpTest.ShouldHaveCalled($"{CentralUrl}/admin/realms/test/clients/{newClientId}/protocol-mappers/models")
            .WithVerb(HttpMethod.Post)
            .Times(1);
        httpTest.ShouldHaveCalled($"{CentralUrl}/admin/realms/test/clients/{newClientId}/roles")
            .WithVerb(HttpMethod.Post)
            .Times(1);
    }

    [Fact]
    public async Task SetupSharedIdpAsync_CallsExpected()
    {
        // Arrange
        const string userId = "userid";
        const string newClientId = "client1";
        using var httpTest = new HttpTest();
        httpTest.WithAuthorization()
            .WithCreateClient(newClientId)
            .WithGetUserForServiceAccount(newClientId, new User { Id = userId })
            .WithGetRoleByNameAsync(newClientId, "create-realm", new Role())
            .WithGetClientSecretAsync(newClientId, new Credentials { Value = "super-secret" })
            .WithGetOpenIdConfigurationAsync(new OpenIDConfiguration
            {
                AuthorizationEndpoint = new Uri("https://test.com/auth"),
                TokenEndpoint = new Uri("https://test.com/token"),
                EndSessionEndpoint = new Uri("https://test.com/end-session"),
                JwksUri = new Uri("https://test.com/jwksUri"),
                Issuer = new Uri("https://test.com/issuer")
            })
            .WithGetIdentityProviderAsync(ValidClientName, new IdentityProvider.IdentityProvider { Config = new IdentityProvider.Config() });

        // Act
        await _sut.SetupSharedIdpAsync(ValidClientName, OrgName, LoginTheme).ConfigureAwait(false);

        // Assert
        httpTest.ShouldHaveCalled($"{CentralUrl}/admin/realms/test/identity-provider/instances")
            .WithVerb(HttpMethod.Post)
            .Times(1);
        httpTest.ShouldHaveCalled($"{SharedUrl}/admin/realms/master/clients")
            .WithVerb(HttpMethod.Post)
            .Times(1);
        httpTest.ShouldHaveCalled($"{SharedUrl}/admin/realms/master/users/{userId}/role-mappings/realm")
            .WithVerb(HttpMethod.Post)
            .Times(1);
        httpTest.ShouldHaveCalled($"{SharedUrl}/admin/realms")
            .WithVerb(HttpMethod.Post)
            .Times(1);
        httpTest.ShouldHaveCalled($"{CentralUrl}/admin/realms/test/identity-provider/instances/{ValidClientName}")
            .WithVerb(HttpMethod.Put)
            .Times(2);
        httpTest.ShouldHaveCalled($"{CentralUrl}/admin/realms/test/identity-provider/instances/{ValidClientName}/mappers")
            .WithVerb(HttpMethod.Post)
            .Times(1);
    }

    [Fact]
    public async Task UpdateSharedIdentityProviderAsync_CallsExpected()
    {
        // Arrange
        const string id = "123";
        using var httpTest = new HttpTest();
        httpTest.WithAuthorization()
            .WithGetIdentityProviderAsync(ValidClientName, new IdentityProvider.IdentityProvider { DisplayName = "test", Config = new IdentityProvider.Config() })
            .WithGetClientsAsync("master", new[] { new Client { Id = id, ClientId = "savalid" } })
            .WithGetClientSecretAsync(id, new Credentials { Value = "super-secret" })
            .WithGetRealmAsync(ValidClientName, new Realm { DisplayName = "test", LoginTheme = "test" });

        // Act
        await _sut.UpdateSharedIdentityProviderAsync(ValidClientName, "displayName").ConfigureAwait(false);

        // Arrange
        httpTest.ShouldHaveCalled($"{SharedUrl}/admin/realms/{ValidClientName}")
            .WithVerb(HttpMethod.Put)
            .Times(1);
        httpTest.ShouldHaveCalled($"{CentralUrl}/admin/realms/test/identity-provider/instances/{ValidClientName}")
            .WithVerb(HttpMethod.Put)
            .Times(1);
    }

    [Fact]
    public async Task UpdateSharedRealmTheme_CallsExpected()
    {
        // Arrange
        const string id = "123";
        using var httpTest = new HttpTest();
        httpTest.WithAuthorization()
            .WithGetIdentityProviderAsync(ValidClientName, new IdentityProvider.IdentityProvider { DisplayName = "test", Config = new IdentityProvider.Config() })
            .WithGetClientsAsync("master", new[] { new Client { Id = id, ClientId = "savalid" } })
            .WithGetClientSecretAsync(id, new Credentials { Value = "super-secret" })
            .WithGetRealmAsync(ValidClientName, new Realm { DisplayName = "test", LoginTheme = "test" });

        // Act
        await _sut.UpdateSharedRealmTheme(ValidClientName, "new-theme").ConfigureAwait(false);

        // Arrange
        httpTest.ShouldHaveCalled($"{SharedUrl}/admin/realms/{ValidClientName}")
            .WithVerb(HttpMethod.Put)
            .Times(1);
    }

    [Fact]
    public async Task GetIdentityProviderDisplayName_CallsExpected()
    {
        // Arrange
        const string alias = "idp123";
        using var httpTest = new HttpTest();
        httpTest.WithAuthorization()
            .WithGetIdentityProviderAsync(alias, new IdentityProvider.IdentityProvider { DisplayName = "test", Config = new IdentityProvider.Config() });

        // Act
        var displayName = await _sut.GetIdentityProviderDisplayName(alias).ConfigureAwait(false);

        // Arrange
        displayName.Should().NotBeNullOrWhiteSpace();
        displayName.Should().Be("test");
    }
}
