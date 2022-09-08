using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public interface IUserRolesRepository
{
    CompanyUserAssignedRole CreateCompanyUserAssignedRole(Guid companyUserId, Guid companyUserRoleId);
    IAsyncEnumerable<UserRoleData> GetUserRoleDataUntrackedAsync(IEnumerable<Guid> userRoleIds);
    IAsyncEnumerable<Guid> GetUserRoleIdsUntrackedAsync(IDictionary<string, IEnumerable<string>> clientRoles);
    IAsyncEnumerable<UserRoleWithId> GetUserRoleWithIdsUntrackedAsync(string clientClientId, IEnumerable<string> userRoles);
    IAsyncEnumerable<UserRoleData> GetUserRoleDataUntrackedAsync(IDictionary<string, IEnumerable<string>> clientRoles);
    IAsyncEnumerable<string> GetClientRolesCompositeAsync(string keyCloakClientId);
    IAsyncEnumerable<UserRoleWithDescription> GetServiceAccountRolesAsync(string clientId,string? languageShortName = null);
    
    /// <summary>
    /// Gets all user role ids for the given offerId
    /// </summary>
    /// <param name="offerId">Id of the offer the roles are assigned to.</param>
    /// <returns>Returns a list of user role ids</returns>
    Task<List<string>> GetUserRolesForOfferIdAsync(Guid offerId);
}
