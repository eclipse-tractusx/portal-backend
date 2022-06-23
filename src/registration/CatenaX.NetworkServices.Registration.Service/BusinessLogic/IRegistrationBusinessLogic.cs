using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Registration.Service.BPN.Model;
using CatenaX.NetworkServices.Registration.Service.Model;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Registration.Service.BusinessLogic
{
    public interface IRegistrationBusinessLogic
    {
        Task<List<FetchBusinessPartnerDto>> GetCompanyByIdentifierAsync(string companyIdentifier, string token);
        Task<IEnumerable<string>> GetClientRolesCompositeAsync();
        Task<int> UploadDocumentAsync(Guid applicationId, IFormFile document, DocumentTypeId documentTypeId, string iamUserId);
        Task SetIdpAsync(SetIdp idpToSet);
        IAsyncEnumerable<CompanyApplicationData> GetAllApplicationsForUserWithStatus(string userId);
        Task<CompanyWithAddress> GetCompanyWithAddressAsync(Guid applicationId);
        Task SetCompanyWithAddressAsync(Guid applicationId, CompanyWithAddress companyWithAddress, string iamUserId);
        Task<int> InviteNewUserAsync(Guid applicationId, UserCreationInfo userCreationInfo, string createdById);
        Task<int> SetApplicationStatusAsync(Guid applicationId, CompanyApplicationStatusId status);
        Task<CompanyApplicationStatusId> GetApplicationStatusAsync(Guid applicationId);
        Task<int> SubmitRoleConsentAsync(Guid applicationId, CompanyRoleAgreementConsents roleAgreementConsentStatuses, string iamUserId);
        Task<CompanyRoleAgreementConsents> GetRoleAgreementConsentsAsync(Guid applicationId, string iamUserId);
        Task<CompanyRoleAgreementData> GetCompanyRoleAgreementDataAsync();
        Task<bool> SubmitRegistrationAsync(Guid applicationId, string iamUserId);
        IAsyncEnumerable<InvitedUser> GetInvitedUsersAsync(Guid applicationId);
        IAsyncEnumerable<UploadDocuments> GetUploadedDocumentsAsync(Guid applicationId,DocumentTypeId documentTypeId,string iamUserId);
        Task<int> SetInvitationStatusAsync(string iamUserId);
        Task<RegistrationData> GetRegistrationDataAsync(Guid applicationId, string iamUserId);
    }
}
