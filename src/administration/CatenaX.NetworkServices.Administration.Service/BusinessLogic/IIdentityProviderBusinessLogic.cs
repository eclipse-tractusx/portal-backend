using CatenaX.NetworkServices.Administration.Service.Models;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public interface IIdentityProviderBusinessLogic
{
    IAsyncEnumerable<IdentityProviderDetails> GetOwnCompanyIdentityProviders(string iamUserId);
}
