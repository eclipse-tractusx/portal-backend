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
    Task<CompanyUserDetails?> GetOwnCompanyUserDetailsUntrackedAsync(Guid companyUserId, string iamUserId);
    Task<CompanyUserBusinessPartners?> GetOwnCompanyUserWithAssignedBusinessPartnerNumbersUntrackedAsync(Guid companyUserId, string adminUserId);
    Task<Guid> GetCompanyIdForIamUserUntrackedAsync(string iamUserId);

    /// <summary>
    /// Gets the company user id for the given iam user id
    /// </summary>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <returns>Returns the id of the company user if one is found.</returns>
    Task<Guid?> GetCompanyUserIdForIamUserUntrackedAsync(string iamUserId);

    /// <summary>
    /// Get the IdpName ,UserId and Role Ids by CompanyUser and AdminUser Id
    /// </summary>
    /// <param name="companyUserId"></param>
    /// <param name="adminUserId"></param>
    /// <returns>Company and IamUser</returns>
    Task<CompanyIamUser?> GetIdpUserByIdUntrackedAsync(Guid companyUserId, string adminUserId);

    public Task<CompanyUserDetails?> GetUserDetailsUntrackedAsync(string iamUserId);
    Task<CompanyUserWithIdpBusinessPartnerData?> GetUserWithCompanyIdpAsync(string iamUserId);
    Task<CompanyUserWithIdpData?> GetUserWithIdpAsync(string iamUserId);
    
    Task<Guid> GetCompanyUserIdForUserApplicationUntrackedAsync(Guid applicationId, string iamUserId);
}
