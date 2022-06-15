using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess
{
    public interface IPortalBackendDBAccess
    {
        Company CreateCompany(string companyName);
        CompanyApplication CreateCompanyApplication(Company company, CompanyApplicationStatusId companyApplicationStatusId);
        CompanyUser CreateCompanyUser(string? firstName, string? lastName, string email, Guid companyId, CompanyUserStatusId companyUserStatusId);
        Invitation CreateInvitation(Guid applicationId, CompanyUser user);
        [Obsolete("use IUserRolesRepository instead")]
        CompanyUserAssignedRole CreateCompanyUserAssignedRole(Guid companyUserId, Guid companyUserRoleId);
        IdentityProvider CreateSharedIdentityProvider(Company company);
        IamIdentityProvider CreateIamIdentityProvider(IdentityProvider identityProvider, string idpAlias);
        IamUser CreateIamUser(CompanyUser companyUser, string iamUserId);
        Address CreateAddress(string city, string streetname, string countryAlpha2Code);
        Consent CreateConsent(Guid agreementId, Guid companyId, Guid companyUserId, ConsentStatusId consentStatusId, string? Comment = null, string? Target = null, Guid? DocumentId = null);
        CompanyAssignedRole CreateCompanyAssignedRole(Guid companyId, CompanyRoleId companyRoleId);
        Document CreateDocument(Guid applicationId, Guid companyUserId, string documentName, string documentContent, string hash, uint documentOId, DocumentTypeId documentTypeId);
        IAsyncEnumerable<CompanyApplicationWithStatus> GetApplicationsWithStatusUntrackedAsync(string iamUserId);
        [Obsolete("use IApplicationRepository instead")]
        Task<CompanyWithAddress?> GetCompanyWithAdressUntrackedAsync(Guid companyApplicationId);
        Task<Company?> GetCompanyWithAdressAsync(Guid companyApplicationId, Guid companyId);
        Task<CompanyNameIdIdpAlias?> GetCompanyNameIdWithSharedIdpAliasUntrackedAsync(Guid companyApplicationId, string iamUserId);
        Task<CompanyNameBpnIdpAlias?> GetCompanyNameIdpAliasUntrackedAsync(string iamUserId);
        Task<string?> GetSharedIdentityProviderIamAliasUntrackedAsync(string iamUserId);
        IAsyncEnumerable<CompanyUser> GetCompanyUserRolesIamUsersAsync(IEnumerable<Guid> companyUserIds, string iamUser);
        IAsyncEnumerable<CompanyUserData> GetCompanyUserDetailsUntrackedAsync(string adminUserId, Guid? companyUserId = null, string? userEntityId = null, string? firstName = null, string? lastName = null, string? email = null, CompanyUserStatusId? companyUserStatusId = null);
        Task<CompanyApplication?> GetCompanyApplicationAsync(Guid applicationId);
        Task<Guid> GetCompanyUserIdForUserApplicationUntrackedAsync(Guid applicationId, string iamUserId);
        Task<CompanyApplicationStatusId> GetApplicationStatusUntrackedAsync(Guid applicationId);
        IAsyncEnumerable<AgreementsAssignedCompanyRoleData> GetAgreementAssignedCompanyRolesUntrackedAsync(IEnumerable<CompanyRoleId> companyRoleIds);
        Task<CompanyRoleAgreementConsentData?> GetCompanyRoleAgreementConsentDataAsync(Guid applicationId, string iamUserId);
        Task<CompanyRoleAgreementConsents?> GetCompanyRoleAgreementConsentStatusUntrackedAsync(Guid applicationId, string iamUserId);
        CompanyAssignedRole RemoveCompanyAssignedRole(CompanyAssignedRole companyAssignedRole);
        CompanyUserAssignedRole RemoveCompanyUserAssignedRole(CompanyUserAssignedRole companyUserAssignedRole);
        IamUser RemoveIamUser(IamUser iamUser);
        IAsyncEnumerable<CompanyRoleData> GetCompanyRoleAgreementsUntrackedAsync();
        IAsyncEnumerable<AgreementData> GetAgreementsUntrackedAsync();
        [Obsolete("use IUserRolesRepository instead")]
        IAsyncEnumerable<Guid> GetUserRoleIdsUntrackedAsync(IDictionary<string, IEnumerable<string>> clientRoles);
        IAsyncEnumerable<UserRoleWithId> GetUserRoleWithIdsUntrackedAsync(string clientClientId, IEnumerable<string> companyUserRoles);
        IAsyncEnumerable<InvitedUserDetail> GetInvitedUserDetailsUntrackedAsync(Guid applicationId);
        Task<IdpUser?> GetIdpCategoryIdByUserIdAsync(Guid companyUserId, string adminUserId);
        IAsyncEnumerable<UploadDocuments> GetUploadedDocumentsAsync(Guid applicationId, DocumentTypeId documentTypeId, string iamUserId);
        Task<Invitation?> GetInvitationStatusAsync(string iamUserId);
        Task<RegistrationData?> GetRegistrationDataUntrackedAsync(Guid applicationId, string iamUserId);
        Task<bool> IsUserExisting(string iamUserId);
        IAsyncEnumerable<ClientRoles> GetClientRolesAsync(Guid appId, string? languageShortName = null);
        Task<string?> GetLanguageAsync(string LanguageShortName);
        Task<Guid> GetAppAssignedClientsAsync(Guid appId);
        Task<int> SaveAsync();
    }
}
