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

using Flurl.Http;
using Flurl.Http.Testing;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests.FlurlSetup;
using System.Net;
using Config = Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders.Config;
using IdentityProvider = Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders.IdentityProvider;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests;

public class UserManagerTests
{
    private readonly IFixture _fixture;
    private const string CentralRealm = "test";
    private const string CentralUrl = "https://central.de";
    private const string SharedUrl = "https://shared.de";
    private readonly IProvisioningManager _sut;
    private readonly IProvisioningDBAccess _provisioningDbAccess;

    public UserManagerTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

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
            CentralIdentityProvider = new IdentityProvider
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
    public async Task GetUserByUserName_WithoutMatchingUser_ReturnsNull()
    {
        // Arrange
        using var httpTest = new HttpTest();
        httpTest.WithAuthorization()
            .WithGetUsersAsync(Enumerable.Empty<User>());

        // Act
        var result = await _sut.GetUserByUserName("test").ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByUserName_WithMultipleUsers_ReturnsNull()
    {
        // Arrange
        using var httpTest = new HttpTest();
        httpTest.WithAuthorization()
            .WithGetUsersAsync(_fixture.CreateMany<User>(2));

        // Act
        var result = await _sut.GetUserByUserName("test").ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByUserName_WithOneUser_ReturnsId()
    {
        // Arrange
        using var httpTest = new HttpTest();
        var user = new User { Id = "test123", UserName = "test" };
        httpTest.WithAuthorization()
            .WithGetUsersAsync(Enumerable.Repeat(user, 1));

        // Act
        var result = await _sut.GetUserByUserName("test").ConfigureAwait(false);

        // Assert
        result.Should().Be("test123");
    }

    [Fact]
    public async Task GetUserByUserName_WithDuplicateUsers_Throws()
    {
        // Arrange
        using var httpTest = new HttpTest();
        var user = new User { Id = "test123", UserName = "test" };
        httpTest.WithAuthorization()
            .WithGetUsersAsync(Enumerable.Repeat(user, 2));

        // Act
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(() => _sut.GetUserByUserName("test")).ConfigureAwait(false);

        // Assert
        result.Message.Should().Be("there should never be multiple users in keycloak having the same username 'test'");
    }

    [Fact]
    public async Task GetUserByUserName_With404Error_ReturnsNull()
    {
        // Arrange
        using var httpTest = new HttpTest();
        httpTest.WithAuthorization()
            .WithGetUsersFailing(HttpStatusCode.NotFound);

        // Act
        var result = await _sut.GetUserByUserName("test").ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByUserName_WithError_ThrowsException()
    {
        // Arrange
        using var httpTest = new HttpTest();
        httpTest.WithAuthorization()
            .WithGetUsersFailing(HttpStatusCode.InternalServerError);

        // Act
        async Task Act() => await _sut.GetUserByUserName("test").ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<FlurlHttpException>(Act);
        ex.Message.Should().Be("Call failed with status code 500 (test): GET https://test.de/");
    }
}
