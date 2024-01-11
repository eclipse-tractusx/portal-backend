/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.ExternalSystems.Provisioning.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.OpenIDConfiguration;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.FlurlSetup;
using Config = Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders.Config;
using Idp = Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders;

namespace Org.Eclipse.TractusX.Portal.Backend.ExternalSystems.Provisioning.Library.Tests;

public class IdpManagementTests
{
    private const string CentralRealm = "test";
    private const string CentralUrl = "https://central.de";
    private const string SharedUrl = "https://shared.de";
    private readonly IProvisioningDBAccess _provisioningDbAccess;
    private readonly IdpManagement _sut;

    public IdpManagementTests()
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
        var settings = new IdpManagementSettings
        {
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
            IdpPrefix = "cl",
            MappedCompanyAttribute = "orgName",
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

        _sut = new IdpManagement(keycloakFactory, _provisioningDbAccess, Options.Create(settings));
    }

    #region GetNextCentralIdentityProviderNameAsync

    [Fact]
    public async Task GetNextCentralIdentityProviderNameAsync_CallsExpected()
    {
        // Arrange
        A.CallTo(() => _provisioningDbAccess.GetNextIdentityProviderSequenceAsync()).Returns(1);

        // Act
        var idpName = await _sut.GetNextCentralIdentityProviderNameAsync().ConfigureAwait(false);

        // Assert
        idpName.Should().Be("cl1");
    }

    #endregion

    #region CreateCentralIdentityProviderAsync

    [Fact]
    public async Task CreateCentralIdentityProviderAsync_CallsExpected()
    {
        // Arrange
        using var httpTest = new HttpTest();
        httpTest.WithAuthorization();

        // Act
        await _sut.CreateCentralIdentityProviderAsync("idp1", "testCorp").ConfigureAwait(false);

        // Assert
        httpTest.ShouldHaveCalled($"{CentralUrl}/admin/realms/test/identity-provider/instances")
            .WithVerb(HttpMethod.Post)
            .Times(1);
    }

    #endregion

    #region CreateSharedIdpServiceAccountAsync

    [Fact]
    public async Task CreateSharedIdpServiceAccountAsync_CallsExpected()
    {
        // Arrange
        const string newClientId = "saidp1";
        using var httpTest = new HttpTest();
        var userId = Guid.NewGuid().ToString();
        httpTest.WithAuthorization()
            .WithCreateClient(newClientId)
            .WithGetUserForServiceAccount(newClientId, new User { Id = userId })
            .WithGetRoleByNameAsync(newClientId, "create-realm", new Role { Id = Guid.NewGuid().ToString() })
            .WithGetClientSecretAsync(newClientId, new Credentials { Value = "super-secret" });

        // Act
        var result = await _sut.CreateSharedIdpServiceAccountAsync("idp1").ConfigureAwait(false);

        // Assert
        result.ClientId.Should().Be(newClientId);
        result.Secret.Should().Be("super-secret");
        httpTest.ShouldHaveCalled($"{SharedUrl}/admin/realms/master/clients")
            .WithVerb(HttpMethod.Post)
            .Times(1);
    }

    #endregion

    #region AddRealmRoleMappingsToUserAsync

    [Fact]
    public async Task AddRealmRoleMappingsToUserAsync_CallsExpected()
    {
        // Arrange
        const string newClientId = "saidp1";
        using var httpTest = new HttpTest();
        var userId = Guid.NewGuid().ToString();
        httpTest.WithAuthorization()
            .WithCreateClient(newClientId)
            .WithGetUserForServiceAccount(newClientId, new User { Id = userId })
            .WithGetRoleByNameAsync(newClientId, "create-realm", new Role { Id = Guid.NewGuid().ToString() })
            .WithGetClientSecretAsync(newClientId, new Credentials { Value = "super-secret" });

        // Act
        await _sut.AddRealmRoleMappingsToUserAsync(userId).ConfigureAwait(false);

        // Assert
        httpTest.ShouldHaveCalled($"{SharedUrl}/admin/realms/master/users/{userId}/role-mappings/realm")
            .WithVerb(HttpMethod.Post)
            .Times(1);
    }

    #endregion

    #region UpdateCentralIdentityProviderUrlsAsync

    [Fact]
    public async Task UpdateCentralIdentityProviderUrlsAsync_CallsExpected()
    {
        // Arrange
        using var httpTest = new HttpTest();
        httpTest.WithAuthorization()
            .WithGetOpenIdConfigurationAsync(new OpenIDConfiguration
            {
                AuthorizationEndpoint = new Uri("https://example.org/auth"),
                TokenEndpoint = new Uri("https://example.org/tkn"),
                EndSessionEndpoint = new Uri("https://example.org/session"),
                JwksUri = new Uri("https://example.org/jwks")
            })
            .WithGetIdentityProviderAsync("idp1", new PortalBackend.PortalEntities.Entities.IdentityProvider(Guid.NewGuid(), IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.OWN, Guid.NewGuid(), DateTimeOffset.UtcNow));

        // Act
        await _sut.UpdateCentralIdentityProviderUrlsAsync("idp1", "testCorp", "theme1", "cl1", "safePw").ConfigureAwait(false);

        // Assert
        httpTest.ShouldHaveCalled($"{CentralUrl}/admin/realms/{CentralRealm}/identity-provider/instances/idp1")
            .WithVerb(HttpMethod.Put)
            .Times(1);
    }

    #endregion

    #region CreateCentralIdentityProviderOrganisationMapperAsync

    [Fact]
    public async Task CreateCentralIdentityProviderOrganisationMapperAsync_CallsExpected()
    {
        // Arrange
        using var httpTest = new HttpTest();
        httpTest.WithAuthorization();

        // Act
        await _sut.CreateCentralIdentityProviderOrganisationMapperAsync("idp1", "testCorp").ConfigureAwait(false);

        // Assert
        httpTest.ShouldHaveCalled($"{CentralUrl}/admin/realms/{CentralRealm}/identity-provider/instances/idp1/mappers")
            .WithVerb(HttpMethod.Post)
            .Times(1);
    }

    #endregion

    #region CreateSharedRealmIdpClientAsync

    [Fact]
    public async Task CreateSharedRealmIdpClientAsync_CallsExpected()
    {
        // Arrange
        using var httpTest = new HttpTest();
        httpTest.WithAuthorization()
            .WithGetOpenIdConfigurationAsync(new OpenIDConfiguration
            {
                JwksUri = new Uri("https://test.org/jwks"),
                Issuer = new Uri("https://example.org/issuer")
            });

        // Act
        await _sut.CreateSharedRealmIdpClientAsync("idp1", "testCorp", "theme1", "cl1", "safePw").ConfigureAwait(false);

        // Assert
        httpTest.ShouldHaveCalled($"{SharedUrl}/admin/realms")
            .WithVerb(HttpMethod.Post)
            .Times(1);
    }

    #endregion

    #region CreateSharedClientAsync

    [Fact]
    public async Task CreateSharedClientAsync_CallsExpected()
    {
        // Arrange
        using var httpTest = new HttpTest();
        httpTest.WithAuthorization()
            .WithGetOpenIdConfigurationAsync(new OpenIDConfiguration
            {
                JwksUri = new Uri("https://test.org/jwks"),
                Issuer = new Uri("https://example.org/issuer")
            });

        // Act
        await _sut.CreateSharedClientAsync("idp1", "cl1", "safePw").ConfigureAwait(false);

        // Assert
        httpTest.ShouldHaveCalled($"{SharedUrl}/admin/realms/idp1/clients")
            .WithVerb(HttpMethod.Post)
            .Times(1);
    }

    #endregion

    #region EnableCentralIdentityProviderAsync

    [Fact]
    public async Task EnableCentralIdentityProviderAsync_CallsExpected()
    {
        // Arrange
        using var httpTest = new HttpTest();
        httpTest.WithAuthorization()
            .WithGetIdentityProviderAsync("idp1", new PortalBackend.PortalEntities.Entities.IdentityProvider(Guid.NewGuid(), IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.OWN, Guid.NewGuid(), DateTimeOffset.UtcNow));

        // Act
        await _sut.EnableCentralIdentityProviderAsync("idp1").ConfigureAwait(false);

        // Assert
        httpTest.ShouldHaveCalled($"{CentralUrl}/admin/realms/{CentralRealm}/identity-provider/instances/idp1")
            .WithVerb(HttpMethod.Put)
            .Times(1);
    }

    #endregion
}
