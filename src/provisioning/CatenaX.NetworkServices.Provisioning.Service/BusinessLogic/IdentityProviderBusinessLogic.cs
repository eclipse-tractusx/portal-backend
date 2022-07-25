using System;
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

        public async Task<string> CreateIdentityProvider(IdentityProviderSetupData? identityProviderData)
        {
            if (identityProviderData == null)
            {
                throw new ArgumentException("identityProviderData must not be null");
            }
            if (String.IsNullOrWhiteSpace(identityProviderData.OrganisationName))
            {
                throw new ArgumentException("OrganisationName must not be empty");
            }
            if (String.IsNullOrWhiteSpace(identityProviderData.ClientAuthMethod))
            {
                throw new ArgumentException("ClientAuthMethod must not be empty");
            }
            if (String.IsNullOrWhiteSpace(identityProviderData.ClientId))
            {
                throw new ArgumentException("ClientId must not be empty");
            }
            if (String.IsNullOrWhiteSpace(identityProviderData.MetadataUrl))
            {
                throw new ArgumentException("MetadataUrl must not be empty");
            }

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
