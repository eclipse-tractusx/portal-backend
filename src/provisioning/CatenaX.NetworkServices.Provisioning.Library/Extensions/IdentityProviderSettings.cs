using Keycloak.Net.Models.IdentityProviders;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningSettings
    {
        public IdentityProvider CentralIdentityProvider { get; set; }
        public IdentityProvider SamlIdentityProvider { get; set; }
        public IdentityProvider OidcIdentityProvider { get; set; }
    }
}
