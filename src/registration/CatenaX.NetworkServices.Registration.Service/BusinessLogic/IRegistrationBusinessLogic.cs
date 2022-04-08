using System;
using CatenaX.NetworkServices.Cosent.Library.Data;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Registration.Service.BPN.Model;
using CatenaX.NetworkServices.Registration.Service.Model;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

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
        Task<CompanyWithAddress> GetCompanyWithAddressAsync(Guid applicationId);
        Task SetCompanyWithAddressAsync(Guid applicationId, CompanyWithAddress companyWithAddress);
        Task<int> InviteNewUserAsync(Guid applicationId, UserInvitationData userInvitationData);
        Task<bool> SubmitRegistrationAsync(string userEmail);
    }
}
