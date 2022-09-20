using CatenaX.NetworkServices.Keycloak.Library.Models.Users;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningSettings
    {
        public User SharedUser { get; set; }
        
        public User CentralUser { get; set; }
    }
}
