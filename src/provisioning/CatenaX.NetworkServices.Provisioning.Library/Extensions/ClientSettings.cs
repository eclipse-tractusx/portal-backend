using Keycloak.Net.Models.Clients;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningSettings
    {
        public Client SharedRealmClient { get; set; }
        public Client CentralOIDCClient { get; set; }
    }
}
