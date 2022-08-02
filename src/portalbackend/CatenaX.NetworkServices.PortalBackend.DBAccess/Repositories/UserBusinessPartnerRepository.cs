using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public class UserBusinessPartnerRepository : IUserBusinessPartnerRepository
{
    private readonly PortalDbContext _dbContext;

    public UserBusinessPartnerRepository(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public CompanyUserAssignedBusinessPartner CreateCompanyUserAssignedBusinessPartner(Guid companyUserId, string businessPartnerNumber)
    {
        if (businessPartnerNumber.Length > 20)
        {
            throw new ArgumentException($"{nameof(businessPartnerNumber)} {businessPartnerNumber} exceeds maximum length of 20 characters", nameof(businessPartnerNumber));
        }
        return _dbContext.CompanyUserAssignedBusinessPartners.Add(
            new CompanyUserAssignedBusinessPartner(
                companyUserId,
                businessPartnerNumber
            )).Entity;
    }

    public CompanyUserAssignedBusinessPartner RemoveCompanyUserAssignedBusinessPartner(CompanyUserAssignedBusinessPartner companyUserAssignedBusinessPartner) =>
        _dbContext.Remove(companyUserAssignedBusinessPartner).Entity;

    public Task<(string? UserEntityId, CompanyUserAssignedBusinessPartner? AssignedBusinessPartner, bool IsValidUser)> GetOwnCompanyUserWithAssignedBusinessPartnerNumbersAsync(Guid companyUserId,string adminUserId, string businessPartnerNumber) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => ((string? UserEntityId, CompanyUserAssignedBusinessPartner? AssignedBusinessPartner, bool IsValidUser)) new (
                companyUser.IamUser!.UserEntityId,
                companyUser.CompanyUserAssignedBusinessPartners!.SingleOrDefault(assignedPartner => assignedPartner.BusinessPartnerNumber == businessPartnerNumber),
                companyUser.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId)
            ))
            .SingleOrDefaultAsync();
}
