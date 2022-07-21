using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public interface IUserBusinessPartnerRepository
{
    CompanyUserAssignedBusinessPartner CreateCompanyUserAssignedBusinessPartner(Guid companyUserId, string businessPartnerNumber);
    CompanyUserAssignedBusinessPartner RemoveCompanyUserAssignedBusinessPartner(CompanyUserAssignedBusinessPartner companyUserAssignedBusinessPartner);
    CompanyUserAssignedBusinessPartner RemoveCompanyUserAssignedBusinessPartner(Guid companyUserId, string businessPartnerNumber);
    Task<(string? UserEntityId, CompanyUserAssignedBusinessPartner? AssignedBusinessPartner, bool IsValidUser)> GetOwnCompanyUserWithAssignedBusinessPartnerNumbersAsync(Guid companyUserId,string adminUserId, string businessPartnerNumber);
}
