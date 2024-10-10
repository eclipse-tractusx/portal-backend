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

using Flurl.Http.Testing;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.FlurlSetup;
using Config = Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders.Config;
using IdentityProvider = Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders.IdentityProvider;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests;

public class IdentityProviderManagerTests
{
    private const string CentralRealm = "test";
    private const string CentralUrl = "https://central.de";
    private readonly IProvisioningManager _sut;

    public IdentityProviderManagerTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var keycloakFactory = A.Fake<IKeycloakFactory>();
        var provisioningDbAccess = A.Fake<IProvisioningDBAccess>();
        A.CallTo(() => keycloakFactory.CreateKeycloakClient("central"))
            .Returns(new KeycloakClient(CentralUrl, "test", "test", "test", false));
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

        _sut = new ProvisioningManager(keycloakFactory, provisioningDbAccess, Options.Create(settings));
    }

    [Fact]
    public async Task GetIdentityProviderMappers_WithFilledDictionary_ReturnsExpected()
    {
        // Arrange
        using var httpTest = new HttpTest();
        var idpMapper = new IdentityProviderMapper
        {
            Id = "test123",
            Name = "test",
            _IdentityProviderMapper = "oidc-username-idp-mapper",
            Config = new Dictionary<string, string>
            {
                { "Test", "testValue1" },
                { "NewerTest", "testValue2" },
                { "lowerTest", "newValue" }
            }
        };
        httpTest
            .WithAuthorization()
            .WithIdentityProviderMappersAsync(Enumerable.Repeat(idpMapper, 1));

        // Act
        var result = await _sut.GetIdentityProviderMappers("test").ToListAsync();

        // Assert
        result.Should().ContainSingle().And.Satisfy(x =>
            x.Id == "test123" &&
            x.Name == "test" &&
            x.Type == IdentityProviderMapperType.OIDC_USERNAME &&
            x.Config.Keys.Count == 3 && x.Config.ContainsKey("Test") && x.Config.ContainsKey("NewerTest") && x.Config.ContainsKey("lowerTest"));
    }
}
