using Keycloak.Net.Models.Clients;

namespace CatenaX.NetworkServices.Provisioning.Library;

public partial class ProvisioningSettings
{
    public Client ServiceAccountClient { get; set; }
    public string ServiceAccountClientPrefix { get; set; }
}
