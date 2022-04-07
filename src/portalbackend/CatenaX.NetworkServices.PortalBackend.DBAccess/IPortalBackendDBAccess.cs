using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess
{
    public interface IPortalBackendDBAccess
    {
        Task<IEnumerable<string>> GetBpnForUserAsync(Guid userId, string bpn = null);
        Task<string> GetIdpAliasForCompanyIdAsync(Guid companyId, string idPAlias = null);

        Company CreateCompany(string companyName);
        CompanyApplication CreateCompanyApplication(Company company);
        CompanyUser CreateCompanyUser(string firstName, string lastName, string email, Company company);
        Invitation CreateInvitation(CompanyApplication application, CompanyUser user);
        IdentityProvider CreateSharedIdentityProvider(Company company);
        IamIdentityProvider CreateIamIdentityProvider(IdentityProvider identityProvider, string idpAlias);
        IamUser CreateIamUser(CompanyUser companyUser, string iamUserId);
        public Task<CompanyWithAddress> GetCompanyWithAdressUntrackedAsync(Guid companyApplicationId);
        public Task SetCompanyWithAdressAsync(Guid companyApplicationId, CompanyWithAddress companyWithAddress);
        Task<int> SaveAsync();
    }
}
