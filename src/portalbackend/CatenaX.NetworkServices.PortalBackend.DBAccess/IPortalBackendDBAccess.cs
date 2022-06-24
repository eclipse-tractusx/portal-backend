using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess
{
    public interface IPortalBackendDBAccess
    {
        [Obsolete("use IUserRepository instead")]
        CompanyUser CreateCompanyUser(string? firstName, string? lastName, string email, Guid companyId, CompanyUserStatusId companyUserStatusId);
        [Obsolete("use IUserRolesRepository instead")]
        CompanyUserAssignedRole CreateCompanyUserAssignedRole(Guid companyUserId, Guid companyUserRoleId);
        [Obsolete("use IUserRepository instead")]
        IamUser CreateIamUser(CompanyUser companyUser, string iamUserId);
        IAsyncEnumerable<CompanyApplicationWithStatus> GetApplicationsWithStatusUntrackedAsync(string iamUserId);
        Task<CompanyNameBpnIdpAlias?> GetCompanyNameIdpAliasUntrackedAsync(string iamUserId);
        Task<string?> GetSharedIdentityProviderIamAliasUntrackedAsync(string iamUserId);
        IAsyncEnumerable<CompanyUser> GetCompanyUserRolesIamUsersAsync(IEnumerable<Guid> companyUserIds, string iamUser);
        Task<CompanyApplication?> GetCompanyApplicationAsync(Guid applicationId);
        Task<CompanyApplicationStatusId> GetApplicationStatusUntrackedAsync(Guid applicationId);
        CompanyUserAssignedRole RemoveCompanyUserAssignedRole(CompanyUserAssignedRole companyUserAssignedRole);
        IamUser RemoveIamUser(IamUser iamUser);
        [Obsolete("user IUserRolesRepository instead")]
        IAsyncEnumerable<UserRoleWithId> GetUserRoleWithIdsUntrackedAsync(string clientClientId, IEnumerable<string> companyUserRoles);
        IAsyncEnumerable<InvitedUserDetail> GetInvitedUserDetailsUntrackedAsync(Guid applicationId);
        Task<IdpUser?> GetIdpCategoryIdByUserIdAsync(Guid companyUserId, string adminUserId);
        IAsyncEnumerable<UploadDocuments> GetUploadedDocumentsAsync(Guid applicationId, DocumentTypeId documentTypeId, string iamUserId);
        Task<Invitation?> GetInvitationStatusAsync(string iamUserId);
        Task<RegistrationData?> GetRegistrationDataUntrackedAsync(Guid applicationId, string iamUserId);
        IAsyncEnumerable<ClientRoles> GetClientRolesAsync(Guid appId, string? languageShortName = null);
        Task<string?> GetLanguageAsync(string LanguageShortName);
        Task<Guid> GetAppAssignedClientsAsync(Guid appId);
        Task<int> SaveAsync();
    }
}
