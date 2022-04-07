using System.Threading.Tasks;
using Keycloak.Net.Models.OpenIDConfiguration;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningManager
    {
        private Task<OpenIDConfiguration> GetSharedRealmOpenIDConfigAsync(string realm) =>
            _SharedIdp.GetOpenIDConfigurationAsync(realm);

        private async Task<string> GetCentralRealmJwksUrlAsync() =>
            (await _CentralIdp.GetOpenIDConfigurationAsync(_Settings.CentralRealm).ConfigureAwait(false)).JwksUri.ToString();
    }
}
