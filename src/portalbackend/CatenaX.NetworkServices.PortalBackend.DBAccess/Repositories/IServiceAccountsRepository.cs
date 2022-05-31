using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public interface IServiceAccountsRepository
{
    CompanyServiceAccount CreateCompanyServiceAccount(Guid companyId, CompanyServiceAccountStatusId companyServiceAccountStatusId, string name, string description);
    IamServiceAccount CreateIamServiceAccount(string clientId, string clientClientId, string userEntityId, Guid companyServiceAccountId);
    CompanyServiceAccountAssignedRole CreateCompanyServiceAccountAssignedRole(Guid companyServiceAccountId, Guid userRoleId);
    IamServiceAccount RemoveIamServiceAccount(IamServiceAccount iamServiceAccount);
    CompanyServiceAccountAssignedRole RemoveCompanyServiceAccountAssignedRole(CompanyServiceAccountAssignedRole companyServiceAccountAssignedRole);
    Task<CompanyServiceAccountWithRoleDataClientId?> GetOwnCompanyServiceAccountWithIamClientIdAsync(Guid serviceAccountId, string adminUserId);
    Task<CompanyServiceAccount?> GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(Guid serviceAccountId, string adminUserId);
    Task<CompanyServiceAccountDetailedData?> GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(Guid serviceAccountId, string iamAdminId);
    IQueryable<CompanyServiceAccount> GetOwnCompanyServiceAccountsUntracked(string adminUserId);
}
