using System;
using CatenaX.NetworkServices.Consent.Library.Data;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Registration.Service.BPN.Model;
using CatenaX.NetworkServices.Registration.Service.Model;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

using Microsoft.AspNetCore.Http;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Registration.Service.BusinessLogic
{
    public interface IRegistrationBusinessLogic
    {
        Task<List<FetchBusinessPartnerDto>> GetCompanyByIdentifierAsync(string companyIdentifier, string token);
        Task<IEnumerable<string>> GetClientRolesCompositeAsync();
        Task<IEnumerable<CompanyRole>> GetCompanyRolesAsync();
        Task<IEnumerable<string>> CreateUsersAsync(List<UserCreationInfo> userList, string tenant, string createdByName);
        Task SetCompanyRolesAsync(CompanyToRoles rolesToSet);
        Task CreateDocument(IFormFile document, string userName);
        Task<IEnumerable<ConsentForCompanyRole>> GetConsentForCompanyRoleAsync(int roleId);
        Task SignConsentAsync(SignConsentRequest signedConsent);
        Task<IEnumerable<SignedConsent>> SignedConsentsByCompanyIdAsync(string companyId);
        Task SetIdpAsync(SetIdp idpToSet);
        Task CreateCustodianWalletAsync(WalletInformation information);
        IAsyncEnumerable<CompanyApplication> GetAllApplicationsForUserWithStatus(string? userId);
        Task<CompanyWithAddress> GetCompanyWithAddressAsync(Guid? applicationId);
        Task SetCompanyWithAddressAsync(Guid? applicationId, CompanyWithAddress? companyWithAddress);
        Task<int> InviteNewUserAsync(Guid? applicationId, UserInvitationData? userInvitationData);
        Task<int> SetApplicationStatusAsync(Guid? applicationId, CompanyApplicationStatusId? status);
        Task<CompanyApplicationStatusId> GetApplicationStatusAsync(Guid? applicationId);
        Task<bool> SubmitRegistrationAsync(string userEmail);
    }
}
