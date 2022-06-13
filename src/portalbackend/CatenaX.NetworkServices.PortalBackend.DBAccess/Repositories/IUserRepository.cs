using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for User Management on persistence layer.
/// </summary>
public interface IUserRepository
{
    Task<Guid> GetCompanyIdForIamUserUntrackedAsync(string iamUserId);

    /// <summary>
    /// Get the IdpName ,UserId and Role Ids by CompanyUser and AdminUser Id
    /// </summary>
    /// <param name="companyUserId"></param>
    /// <param name="adminUserId"></param>
    /// <returns>Company and IamUser</returns>
    Task<CompanyIamUser> GetIdpUserByIdAsync(Guid companyUserId, string adminUserId);

    /// <summary>
    /// Get Client Name by App Id
    /// </summary>
    /// <param name="appId"></param>
    /// <returns>Client Name</returns>
    Task<string> GetAppAssignedRolesClientIdAsync(Guid appId);
}
