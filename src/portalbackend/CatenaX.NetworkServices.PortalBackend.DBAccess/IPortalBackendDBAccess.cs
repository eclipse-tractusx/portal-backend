using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess
{
    public interface IPortalBackendDBAccess
    {
        Task<string?> GetBpnUntrackedAsync(Guid companyId);
        IAsyncEnumerable<string> GetIamUsersUntrackedAsync(Guid companyId);
        Company CreateCompany(string companyName);
        CompanyApplication CreateCompanyApplication(Company company, CompanyApplicationStatusId companyApplicationStatusId);
        CompanyUser CreateCompanyUser(string? firstName, string? lastName, string email, Guid companyId, CompanyUserStatusId companyUserStatusId);
        Invitation CreateInvitation(Guid applicationId, CompanyUser user);
        CompanyUserAssignedRole CreateCompanyUserAssignedRole(Guid companyUserId, Guid companyUserRoleId);
        IdentityProvider CreateSharedIdentityProvider(Company company);
        IamIdentityProvider CreateIamIdentityProvider(IdentityProvider identityProvider, string idpAlias);
        IamUser CreateIamUser(CompanyUser companyUser, string iamUserId);
        Address CreateAddress(string city, string streetname, decimal zipcode, string countryAlpha2Code);
        Consent CreateConsent(Guid agreementId, Guid companyId, Guid companyUserId, ConsentStatusId consentStatusId, string? Comment = null, string? Target = null, Guid? DocumentId = null);
        CompanyAssignedRole CreateCompanyAssignedRole(Guid companyId, CompanyRoleId companyRoleId);
        Document CreateDocument(Guid applicationId, Guid companyUserId, string documentName, string documentContent, string hash, uint documentOId, DocumentTypeId documentTypeId);
        CompanyServiceAccount CreateCompanyServiceAccount(Guid companyId, CompanyServiceAccountStatusId companyServiceAccountStatusId, string name, string description);
        IamServiceAccount CreateIamServiceAccount(string clientId, string clientClientId, string userEntityId, Guid companyServiceAccountId);        
        IAsyncEnumerable<CompanyApplicationWithStatus> GetApplicationsWithStatusUntrackedAsync(string iamUserId);
        Pagination.AsyncSource<CompanyApplicationDetails> GetCompanyApplicationDetailsUntrackedAsync(int skip, int take);
        Task<CompanyWithAddress?> GetCompanyWithAdressUntrackedAsync(Guid companyApplicationId);
        Task<Company?> GetCompanyWithAdressAsync(Guid companyApplicationId, Guid companyId);
        Task<CompanyNameIdBpnIdpAlias?> GetCompanyNameIdWithSharedIdpAliasUntrackedAsync(Guid companyApplicationId, string iamUserId);
        Task<CompanyNameBpnIdpAlias?> GetCompanyNameIdpAliasUntrackedAsync(string iamUserId);
        Task<string?> GetSharedIdentityProviderIamAliasUntrackedAsync(string iamUserId);
        Task<CompanyUserWithIdpData?> GetCompanyUserWithIdpAsync(string iamUserId);
        Task<OwnCompanyUserDetails?> GetOwnCompanyUserDetailsUntrackedAsync(string iamUserId);
        Task<CompanyUserWithIdpData?> GetCompanyUserWithCompanyIdpAsync(string iamUserId);
        IAsyncEnumerable<CompanyUser> GetCompanyUserRolesIamUsersAsync(IEnumerable<Guid> companyUserIds, string iamUser);
        IAsyncEnumerable<CompanyUserDetails> GetCompanyUserDetailsUntrackedAsync(string adminUserId, Guid? companyUserId = null, string? userEntityId = null, string? firstName = null, string? lastName = null, string? email = null, CompanyUserStatusId? companyUserStatusId = null);
        Task<Guid> GetCompanyIdForIamUserUntrackedAsync(string iamUserId);
        Task<CompanyApplication?> GetCompanyApplicationAsync(Guid applicationId);
        Task<Guid> GetCompanyUserIdForUserApplicationUntrackedAsync(Guid applicationId, string iamUserId);
        Task<CompanyApplicationStatusId> GetApplicationStatusUntrackedAsync(Guid applicationId);
        IAsyncEnumerable<AgreementsAssignedCompanyRoleData> GetAgreementAssignedCompanyRolesUntrackedAsync(IEnumerable<CompanyRoleId> companyRoleIds);
        Task<CompanyRoleAgreementConsentData?> GetCompanyRoleAgreementConsentDataAsync(Guid applicationId, string iamUserId);
        Task<CompanyRoleAgreementConsents?> GetCompanyRoleAgreementConsentStatusUntrackedAsync(Guid applicationId, string iamUserId);
        CompanyAssignedRole RemoveCompanyAssignedRole(CompanyAssignedRole companyAssignedRole);
        CompanyUserAssignedRole RemoveCompanyUserAssignedRole(CompanyUserAssignedRole companyUserAssignedRole);
        IamUser RemoveIamUser(IamUser iamUser);
        IamServiceAccount RemoveIamServiceAccount(IamServiceAccount iamServiceAccount);
        IAsyncEnumerable<CompanyRoleData> GetCompanyRoleAgreementsUntrackedAsync();
        IAsyncEnumerable<AgreementData> GetAgreementsUntrackedAsync();
        IAsyncEnumerable<Guid> GetUserRoleIdsUntrackedAsync(IDictionary<string,IEnumerable<string>> clientRoles);
        IAsyncEnumerable<UserRoleWithId> GetUserRoleWithIdsUntrackedAsync(string clientClientId, IEnumerable<string> companyUserRoles);
        IAsyncEnumerable<InvitedUserDetail> GetInvitedUserDetailsUntrackedAsync(Guid applicationId);
        IAsyncEnumerable<WelcomeEmailData> GetWelcomeEmailDataUntrackedAsync(Guid applicationId);
        Task<IdpUser?> GetIdpCategoryIdByUserId(Guid companyUserId, string adminUserId);
        IAsyncEnumerable<UploadDocuments> GetUploadedDocumentsAsync(Guid applicationId,DocumentTypeId documentTypeId,string iamUserId);
        Task<CompanyServiceAccountWithClientId?> GetOwnCompanyServiceAccountWithIamClientIdAsync(Guid serviceAccountId, string adminUserId);
        Task<CompanyServiceAccount?> GetOwnCompanyServiceAccountWithIamServiceAccountAsync(Guid serviceAccountId, string adminUserId);
        Task<CompanyServiceAccountDetailedData?> GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(Guid serviceAccountId, string iamAdminId);
        Task<Pagination.Source<CompanyServiceAccountData>?> GetOwnCompanyServiceAccountDetailsUntracked(int skip, int take, string adminUserId);
        Task<RegistrationData?> GetRegistrationDataAsync(Guid applicationId, string iamUserId);
        Task<int> SaveAsync();
    }
}
