using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess
{
    public interface IPortalBackendDBAccess
    {
        Task<string?> GetSharedIdentityProviderIamAliasUntrackedAsync(string iamUserId);
        IAsyncEnumerable<CompanyUser> GetCompanyUserRolesIamUsersAsync(IEnumerable<Guid> companyUserIds, string iamUser);
        CompanyUserAssignedRole RemoveCompanyUserAssignedRole(CompanyUserAssignedRole companyUserAssignedRole);
        IamUser RemoveIamUser(IamUser iamUser);
        Task<IdpUser?> GetIdpCategoryIdByUserIdAsync(Guid companyUserId, string adminUserId);
        Task<int> SaveAsync();
    }
}
