using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for User Management on persistence layer.
/// </summary>
public interface IUserRepository
{
    CompanyUser CreateCompanyUser(string? firstName, string? lastName, string email, Guid companyId, CompanyUserStatusId companyUserStatusId);
    IamUser CreateIamUser(CompanyUser companyUser, string iamUserId);
    IQueryable<CompanyUser> GetOwnCompanyUserQuery(string adminUserId, Guid? companyUserId = null, string? userEntityId = null, string? firstName = null, string? lastName = null, string? email = null, CompanyUserStatusId? status = null);
    Task<bool> IsOwnCompanyUserWithEmailExisting(string email, string adminUserId);
    Task<CompanyUserDetails?> GetOwnCompanyUserDetailsUntrackedAsync(Guid companyUserId, string iamUserId);
    Task<CompanyUserBusinessPartners?> GetOwnCompanyUserWithAssignedBusinessPartnerNumbersUntrackedAsync(Guid companyUserId, string adminUserId);
    Task<Guid> GetCompanyIdForIamUserUntrackedAsync(string iamUserId);

    /// <summary>
    /// Gets the CompanyUser Id for the given IamUser Id
    /// </summary>
    /// <param name="userId">the iam userid the company user should be searched for.</param>
    /// <returns>Returns the id of the CompanyUser</returns>
    Task<Guid> GetCompanyUserIdForIamUserUntrackedAsync(string userId);
    
    /// <summary>
    /// Get the IdpName ,UserId and Role Ids by CompanyUser and AdminUser Id
    /// </summary>
    /// <param name="companyUserId"></param>
    /// <param name="adminUserId"></param>
    /// <returns>Company and IamUser</returns>
    Task<CompanyIamUser?> GetIdpUserByIdUntrackedAsync(Guid companyUserId, string adminUserId);

    Task<CompanyUserDetails?> GetUserDetailsUntrackedAsync(string iamUserId);
    Task<CompanyUserWithIdpBusinessPartnerData?> GetUserWithCompanyIdpAsync(string iamUserId);
    Task<CompanyUserWithIdpData?> GetUserWithIdpAsync(string iamUserId);
    Task<Guid> GetCompanyUserIdForUserApplicationUntrackedAsync(Guid applicationId, string iamUserId);
    
    /// <summary>
    /// GGets all apps for the give user from the persistence layer.
    /// </summary>
    /// <param name="userId">Id of the user which apps should be selected.</param>
    /// <returns>Returns an IAsyncEnumerable of GUIDs</returns>
    IAsyncEnumerable<Guid> GetAllFavouriteAppsForUserUntrackedAsync(string userId);

    /// <summary>
    /// Gets all business app data for the given userId 
    /// </summary>
    /// <param name="userId">Id of the user to get the app data for.</param>
    /// <returns>Returns an IAsyncEnumerable of <see cref="BusinessAppData"/></returns>
    IAsyncEnumerable<BusinessAppData> GetAllBusinessAppDataForUserIdAsync(string userId);
}
