using Keycloak.Net;

namespace CatenaX.NetworkServices.Keycloak.Factory
{
    public interface IKeycloakFactory
    {
        KeycloakClient CreateKeycloakClient(string instance);
    }
}
