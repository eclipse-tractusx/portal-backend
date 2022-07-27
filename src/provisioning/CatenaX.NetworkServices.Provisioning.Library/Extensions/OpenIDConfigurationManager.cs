using Keycloak.Net.Models.OpenIDConfiguration;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningManager
    {
        private async Task<OpenIDConfiguration> GetSharedRealmOpenIDConfigAsync(string realm)
        {
            var openidconfig = await _SharedIdp.GetOpenIDConfigurationAsync(realm).ConfigureAwait(false);
            if (openidconfig == null)
            {
                throw new System.Exception($"failed to retrieve openidconfiguration for shared realm {realm}");
            }
            return openidconfig;
        }

        private async Task<string> GetCentralRealmJwksUrlAsync()
        {
            var openidconfig = await _CentralIdp.GetOpenIDConfigurationAsync(_Settings.CentralRealm).ConfigureAwait(false);
            if (openidconfig == null)
            {
                throw new System.Exception($"failed to retrieve openidconfiguration for central realm {_Settings.CentralRealm}");
            }
            return openidconfig.JwksUri.ToString();
        }
    }
}
