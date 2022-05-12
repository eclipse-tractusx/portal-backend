using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess
{
    public interface IPortalBackendDBAccess
    {
        IAsyncEnumerable<UserBpn> GetBpnForUsersUntrackedAsync(IEnumerable<string> userIds);
        IAsyncEnumerable<string> GetIdpAliaseForCompanyIdUntrackedAsync(Guid companyId);
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
        IAsyncEnumerable<CompanyApplicationWithStatus> GetApplicationsWithStatusUntrackedAsync(string iamUserId);
        Task<CompanyWithAddress> GetCompanyWithAdressUntrackedAsync(Guid companyApplicationId);
        Task<Company> GetCompanyWithAdressAsync(Guid companyApplicationId, Guid companyId);
        Task<CompanyNameIdBpnIdpAlias> GetCompanyNameIdWithSharedIdpAliasUntrackedAsync(Guid companyApplicationId, string iamUserId);
        Task<CompanyNameBpnIdpAlias> GetCompanyNameIdpAliasUntrackedAsync(string iamUserId);
        Task<CompanyApplication> GetCompanyApplicationAsync(Guid applicationId);
        Task<CompanyIdWithUserId> GetCompanyWithUserIdForUserApplicationUntrackedAsync(Guid applicationId, string iamUserId);
        Task<Guid> GetCompanyUserIdForUserApplicationUntrackedAsync(Guid applicationId, string iamUserId);
        Task<CompanyApplicationStatusId> GetApplicationStatusUntrackedAsync(Guid applicationId);
        IAsyncEnumerable<AgreementsAssignedCompanyRoleData> GetAgreementAssignedCompanyRolesUntrackedAsync(IEnumerable<CompanyRoleId> companyRoleIds);
        Task<CompanyRoleAgreementConsentData> GetCompanyRoleAgreementConsentDataAsync(Guid applicationId, string iamUserId);
        Task<CompanyRoleAgreementConsents> GetCompanyRoleAgreementConsentStatusUntrackedAsync(Guid applicationId, string iamUserId);
        CompanyAssignedRole RemoveCompanyAssignedRole(CompanyAssignedRole companyAssignedRole);
        IAsyncEnumerable<CompanyRoleData> GetCompanyRoleAgreementsUntrackedAsync();
        IAsyncEnumerable<AgreementData> GetAgreementsUntrackedAsync();
        IAsyncEnumerable<Guid> GetUserRoleIdsUntrackedAsync(IDictionary<string,IEnumerable<string>> clientRoles);
        IAsyncEnumerable<UserRoleWithId> GetUserRoleWithIdsUntrackedAsync(string clientClientId, IEnumerable<string> companyUserRoles);
        IAsyncEnumerable<InvitedUserDetail> GetInvitedUserDetailsUntrackedAsync(Guid applicationId);
        Task<IdpUser> GetIdpCategoryIdByUserId(Guid companyUserId, string adminUserId);
        Task<int> SaveAsync();
    }
}
