using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess
{
    public interface IPortalBackendDBAccess
    {
        Task<string> GetBpnForUserUntrackedAsync(string userId);
        IAsyncEnumerable<UserBpn> GetBpnForUsersUntrackedAsync(IEnumerable<string> userIds);
        IAsyncEnumerable<string> GetIdpAliaseForCompanyIdUntrackedAsync(Guid companyId);
        Company CreateCompany(string companyName);
        CompanyApplication CreateCompanyApplication(Company company);
        CompanyUser CreateCompanyUser(string firstName, string lastName, string email, Guid companyId);
        Invitation CreateInvitation(Guid applicationId, CompanyUser user);
        IdentityProvider CreateSharedIdentityProvider(Company company);
        IamIdentityProvider CreateIamIdentityProvider(IdentityProvider identityProvider, string idpAlias);
        IamUser CreateIamUser(CompanyUser companyUser, string iamUserId);
        IAsyncEnumerable<CompanyApplicationWithStatus> GetApplicationsWithStatusUntrackedAsync(string iamUserId);        
        Task<CompanyWithAddress> GetCompanyWithAdressUntrackedAsync(Guid companyApplicationId);
        Task SetCompanyWithAdressAsync(Guid companyApplicationId, CompanyWithAddress companyWithAddress);
        Task<CompanyNameIdWithIdpAlias> GetCompanyNameIdWithSharedIdpAliasUntrackedAsync(Guid companyApplicationId);
        Task <int> UpdateApplicationStatusAsync(Guid applicationId, CompanyApplicationStatusId applicationStatus);
        Task<CompanyApplicationStatusId?> GetApplicationStatusAsync(Guid applicationId);
        Task<int> SaveAsync();
    }
}
