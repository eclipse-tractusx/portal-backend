using Flurl.Http.Testing;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.OpenIDConfiguration;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;
using IdentityProvider = Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders.IdentityProvider;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests.FlurlSetup;

public static class FlurlSetupExtensions
{
    public static HttpTest WithAuthorization(this HttpTest testClient, string realm)
    {
        testClient.ForCallsTo("*/auth/realms/*/protocol/openid-connect/token")
            .RespondWithJson(new {access_token = "123"});
        return testClient;
    }

    public static HttpTest WithCreateClient(this HttpTest testClient, string newClientId)
    {
        testClient.ForCallsTo("*/admin/realms/*/clients")
            .WithVerb(HttpMethod.Post)
            .RespondWith("Ok", 200,
                new[] {new ValueTuple<string, object>("Location", new Uri($"https://www.test.de/{newClientId}"))});
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

    public static HttpTest WithGetRealmAsync(this HttpTest testClient, string alias, Realm realm)
    {
        testClient.ForCallsTo($"*/admin/realms/{alias}")
            .WithVerb(HttpMethod.Get)
            .RespondWithJson(realm);
        return testClient;
    }
}
