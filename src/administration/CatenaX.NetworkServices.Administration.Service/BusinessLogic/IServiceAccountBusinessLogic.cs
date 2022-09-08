using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public interface IServiceAccountBusinessLogic
{
    Task<ServiceAccountDetails> CreateOwnCompanyServiceAccountAsync(ServiceAccountCreationInfo serviceAccountCreationInfos, string iamAdminId);
    Task<int> DeleteOwnCompanyServiceAccountAsync(Guid serviceAccountId, string iamAdminId);
    Task<ServiceAccountDetails> GetOwnCompanyServiceAccountDetailsAsync(Guid serviceAccountId, string iamAdminId);
    Task<ServiceAccountDetails> UpdateOwnCompanyServiceAccountDetailsAsync(Guid serviceAccountId, ServiceAccountEditableDetails serviceAccountDetails, string iamAdminId);
    Task<ServiceAccountDetails> ResetOwnCompanyServiceAccountSecretAsync(Guid serviceAccountId, string iamAdminId);
    Task<Pagination.Response<CompanyServiceAccountData>> GetOwnCompanyServiceAccountsDataAsync(int page, int size, string iamAdminId);
    IAsyncEnumerable<UserRoleWithDescription> GetServiceAccountRolesAsync(string? languageShortName = null);
}
