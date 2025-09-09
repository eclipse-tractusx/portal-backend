/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests;

public class SharedMultiKeycloakResolverTests
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityProviderRepository _idpRepository;
    private readonly IKeycloakFactory _keycloakFactory;
    private readonly byte[] _encryptionKey;
    private readonly IFixture _fixture;
    private readonly IOptions<MultiKeycloakSettings> _options;
    private readonly ISharedMultiKeycloakResolver _sut;

    public SharedMultiKeycloakResolverTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _portalRepositories = A.Fake<IPortalRepositories>();
        _idpRepository = A.Fake<IIdentityProviderRepository>();
        _keycloakFactory = A.Fake<IKeycloakFactory>();

        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>())
            .Returns(_idpRepository);

        _encryptionKey = _fixture.CreateMany<byte>(32).ToArray();
        _options = Options.Create(new MultiKeycloakSettings
        {
            IsSharedIdpMultiInstancesEnabled = true,
            EncryptionConfigs =
            [
                new EncryptionModeConfig
                {
                    Index = 1,
                    CipherMode = CipherMode.CBC,
                    PaddingMode = PaddingMode.PKCS7,
                    EncryptionKey = Convert.ToHexString(_encryptionKey)
                }
            ]
        });
        _sut = new SharedMultiKeycloakResolver(_portalRepositories, _keycloakFactory, _options);
    }

    [Fact]
    public async Task GetKeycloakClient_DisabledMultiShared_CallsFactory()
    {
        var settings = new MultiKeycloakSettings { IsSharedIdpMultiInstancesEnabled = false };
        var sut = new SharedMultiKeycloakResolver(_portalRepositories, _keycloakFactory, Options.Create(settings));
        var fakeClient = new KeycloakClient("http://fake", "u", "p", "r", false);

        A.CallTo(() => _keycloakFactory.CreateKeycloakClient("shared"))
            .Returns(fakeClient);

        var result = await sut.GetKeycloakClient("anyRealm");

        result.Should().Be(fakeClient);
        A.CallTo(() => _keycloakFactory.CreateKeycloakClient("shared"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetKeycloakClient_EnabledMultiShared_ReturnsDecryptedClient()
    {
        var cryptoConfig = _options.Value.EncryptionConfigs.First();
        var (secret, vector) = CryptoHelper.Encrypt("test123", Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);

        A.CallTo(() => _idpRepository.GetSharedIdpInstanceByRealm("realm1"))
            .Returns((
                SharedIdpUrl: "https://shared.example.org",
                ClientId: "clientX",
                ClientSecret: secret,
                InitializationVector: vector,
                EncryptionMode: 1,
                authRealm: "authRealm",
                useAuthTrail: true
            ));

        var result = await _sut.GetKeycloakClient("realm1");

        result.Should().NotBeNull();
        result.Should().BeOfType<KeycloakClient>();
    }

    [Fact]
    public async Task ResolveAndAssignKeycloak_EnabledMultiShared_AssignsRealm()
    {
        var cryptoConfig = _options.Value.EncryptionConfigs.First();
        var (secret, vector) = CryptoHelper.Encrypt("test123", Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);
        var instance = new SharedIdpInstanceDetail(
            Guid.NewGuid(),
            "https://shared.example.org",
            "clientY",
            secret,
            vector,
            1,
            DateTimeOffset.UtcNow
        )
        {
            AuthRealm = "authRealm",
            UseAuthTrail = false,
            RealmUsed = 0,
            MaxRealmCount = 10
        };

        A.CallTo(() => _idpRepository.GetAllSharedIdpInstanceDetails())
            .Returns([instance]);

        A.CallTo(() => _idpRepository.CreateSharedIdpRealmMapping(instance.Id, "realmX"))
            .Returns(new SharedIdpRealmMapping(instance.Id, "realmX"));

        var result = await _sut.ResolveAndAssignKeycloak("realmX");

        result.Should().NotBeNull();
        result.Should().BeOfType<KeycloakClient>();
        instance.RealmUsed.Should().Be(1);
    }

    [Fact]
    public async Task ResolveAndAssignKeycloak_EnabledMultiShared_NoAvailableInstance_Throws()
    {

        A.CallTo(() => _idpRepository.GetAllSharedIdpInstanceDetails())
            .Returns([]);

        var act = async () => await _sut.ResolveAndAssignKeycloak("realmX");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No available shared IDP instance found under realm threshold.");
    }

    [Fact]
    public async Task GetKeycloakClient_EnabledMultiShared_InvalidEncryptionMode_Throws()
    {
        A.CallTo(() => _idpRepository.GetSharedIdpInstanceByRealm("realm1"))
            .Returns((
                "https://shared.example.org",
                "clientX",
                new byte[] { 1, 2, 3 },
                new byte[16],
                99, // invalid
                "authRealm",
                true
            ));

        var act = async () => await _sut.GetKeycloakClient("realm1");

        await act.Should().ThrowAsync<ConfigurationException>()
            .WithMessage("EncryptionModeIndex 99 is not configured");
    }
}
