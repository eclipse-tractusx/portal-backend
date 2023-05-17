
using Flurl.Http.Testing;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests.FlurlSetup;
using Config = Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders.Config;
using IdentityProvider = Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders.IdentityProvider;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests.Extensions;

public class ClientManagerTests
{
    private const string CentralRealm = "test";
    private const string CentralUrl = "https://central.de";
    private const string SharedUrl = "https://shared.de";
    private readonly IProvisioningManager _sut;
    private readonly IProvisioningDBAccess _provisioningDbAccess;

    public ClientManagerTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var keycloakFactory = A.Fake<IKeycloakFactory>();
        _provisioningDbAccess = A.Fake<IProvisioningDBAccess>();
        A.CallTo(() => keycloakFactory.CreateKeycloakClient("central"))
            .Returns(new KeycloakClient(CentralUrl, "test", "test", "test"));
        A.CallTo(() => keycloakFactory.CreateKeycloakClient("shared"))
            .Returns(new KeycloakClient(SharedUrl, "test", "test", "test"));
        A.CallTo(() => keycloakFactory.CreateKeycloakClient("shared", A<string>._, A<string>._))
            .Returns(new KeycloakClient(SharedUrl, "test", "test", "test"));
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
    public async Task UpdateClient_CallsExpected()
    {
        // Arrange
        const string url = "https://newurl.com";
        const string clientId = "cl1";
        var clientClientId = Guid.NewGuid().ToString();
        var client = new Client { Id = clientClientId, ClientId = clientId };
        using var httpTest = new HttpTest();
        A.CallTo(() => _provisioningDbAccess.GetNextClientSequenceAsync()).Returns(1);
        httpTest.WithAuthorization()
            .WithGetClientsAsync("test", Enumerable.Repeat(client, 1))
            .WithGetClientAsync("test", clientClientId, client)
            .WithGetClientSecretAsync(clientId, new Credentials { Value = "super-secret" });

        // Act
        await _sut.UpdateClient(clientId, $"{url}/*", url).ConfigureAwait(false);

        // Assert
        httpTest.ShouldHaveCalled($"{CentralUrl}/auth/admin/realms/test/clients/{clientClientId}")
            .WithVerb(HttpMethod.Put)
            .Times(1);
    }

    [Fact]
    public async Task UpdateClient_WithClientNull_ThrowsException()
    {
        // Arrange
        const string url = "https://newurl.com";
        const string clientId = "cl1";
        using var httpTest = new HttpTest();
        A.CallTo(() => _provisioningDbAccess.GetNextClientSequenceAsync()).Returns(1);
        httpTest.WithAuthorization()
            .WithGetClientsAsync("test", Enumerable.Empty<Client>());

        // Act
        async Task Act() => await _sut.UpdateClient(clientId, $"{url}/*", url).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<KeycloakEntityNotFoundException>(Act);
        ex.Message.Should().Be($"clientId {clientId} not found in central keycloak");
    }

    [Fact]
    public async Task EnableClient_CallsExpected()
    {
        // Arrange
        const string clientId = "cl1";
        using var httpTest = new HttpTest();
        A.CallTo(() => _provisioningDbAccess.GetNextClientSequenceAsync()).Returns(1);
        httpTest.WithAuthorization()
            .WithGetClientAsync("test", clientId, new Client { Id = Guid.NewGuid().ToString(), ClientId = clientId })
            .WithGetClientSecretAsync(clientId, new Credentials { Value = "super-secret" });

        // Act
        await _sut.EnableClient(clientId).ConfigureAwait(false);

        // Assert
        httpTest.ShouldHaveCalled($"{CentralUrl}/auth/admin/realms/test/clients/{clientId}")
            .WithVerb(HttpMethod.Put)
            .Times(1);
    }
}
