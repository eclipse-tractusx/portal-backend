using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public interface IUserRolesRepository
{
    CompanyUserAssignedRole CreateCompanyUserAssignedRole(Guid companyUserId, Guid companyUserRoleId);
    IAsyncEnumerable<UserRoleData> GetUserRoleDataUntrackedAsync(IEnumerable<Guid> userRoleIds);
    IAsyncEnumerable<Guid> GetUserRoleIdsUntrackedAsync(IDictionary<string, IEnumerable<string>> clientRoles);
    IAsyncEnumerable<UserRoleWithId> GetUserRoleWithIdsUntrackedAsync(string clientClientId, IEnumerable<string> userRoles);
    IAsyncEnumerable<UserRoleData> GetUserRoleUntrackedAsync(IDictionary<string, IEnumerable<string>> clientRoles);
}
