using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public interface IUserBusinessPartnerRepository
{
    CompanyUserAssignedBusinessPartner CreateCompanyUserAssignedBusinessPartner(Guid companyUserId, string businessPartnerNumber);
    CompanyUserAssignedBusinessPartner RemoveCompanyUserAssignedBusinessPartner(CompanyUserAssignedBusinessPartner companyUserAssignedBusinessPartner);
    CompanyUserAssignedBusinessPartner RemoveCompanyUserAssignedBusinessPartner(Guid companyUserId, string businessPartnerNumber);
    Task<CompanyUserAssignedBusinessPartner?> GetOwnCompanyUserWithAssignedBusinessPartnerNumbersAsync(Guid companyUserId);
    Task<bool> IsAdminUserAndCompanyUserHavingSameCompanyId(Guid companyUserId, string adminUserId);
}
