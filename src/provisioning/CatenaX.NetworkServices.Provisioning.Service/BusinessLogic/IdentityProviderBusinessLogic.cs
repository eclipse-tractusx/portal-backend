using System.Threading.Tasks;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Service.Models;

namespace CatenaX.NetworkServices.Provisioning.Service.BusinessLogic
{
    public class IdentityProviderBusinessLogic : IIdentityProviderBusinessLogic
    {
        private readonly IProvisioningManager _provisioningManager;

        public IdentityProviderBusinessLogic(IProvisioningManager provisioningManager)
        {
            _provisioningManager = provisioningManager;
        }

        public async Task<string> CreateIdentityProvider(IdentityProviderSetupData identityProviderData)
        {
            return await _provisioningManager.SetupOwnIdpAsync(
                identityProviderData.OrganisationName,
                identityProviderData.ClientId,
                identityProviderData.MetadataUrl,
                identityProviderData.ClientAuthMethod,
                identityProviderData.ClientSecret
            ).ConfigureAwait(false);
        }
    }
}
