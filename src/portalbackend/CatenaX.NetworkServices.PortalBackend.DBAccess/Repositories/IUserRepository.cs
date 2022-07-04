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
    /// Gets the company user id for the given iam user id
    /// </summary>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <returns>The Guid of the company user</returns>
    Task<Guid> GetCompanyUserIdForIamUserIdUntrackedAsync(string iamUserId);

    /// <summary>
    /// Checks whether a user with the given company User Id exists.
    /// </summary>
    /// <param name="companyUserId">The id of the company user to check in the persistence layer.</param>
    /// <returns><c>true</c> if the user exists, otherwise <c>false</c></returns>
    Task<bool> IsUserWithIdExisting(Guid companyUserId);
}
