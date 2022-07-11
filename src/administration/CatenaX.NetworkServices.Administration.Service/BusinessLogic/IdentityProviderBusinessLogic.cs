using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.Provisioning.Library;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class IdentityProviderBusinessLogic : IIdentityProviderBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;

    public IdentityProviderBusinessLogic(IPortalRepositories portalRepositories, IProvisioningManager provisioningManager)
    {
        _portalRepositories = portalRepositories;
        _provisioningManager = provisioningManager;
    }

    public async IAsyncEnumerable<IdentityProviderDetails> GetOwnCompanyIdentityProviders(string iamUserId)
    {
        await foreach ( var identityProviderData in _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderDataUntracked(iamUserId).ConfigureAwait(false))
        {
            var identityProvider = await _provisioningManager.GetCentralIdentityProviderDataAsync(identityProviderData.Alias).ConfigureAwait(false);
            yield return new IdentityProviderDetails(
                identityProviderData.Id,
                identityProviderData.CategoryId,
                identityProvider.RedirectUrl,
                identityProvider.DisplayName,
                identityProvider.AuthorizationUrl,
                identityProvider.ClientAuthMethod,
                identityProvider.ClientId,
                identityProvider.Enabled
            );
        }
    }
}
