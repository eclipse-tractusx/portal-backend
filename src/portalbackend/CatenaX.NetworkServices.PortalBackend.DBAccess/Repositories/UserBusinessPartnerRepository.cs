using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
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

    public CompanyUserAssignedBusinessPartner RemoveCompanyUserAssignedBusinessPartner(Guid companyUserId, string businessPartnerNumber) =>
        _dbContext.Remove(CreateCompanyUserAssignedBusinessPartner(companyUserId, businessPartnerNumber)).Entity;

    public Task<CompanyUserBusinessPartnerNumbersDetails?> GetOwnCompanyUserWithAssignedBusinessPartnerNumbersAsync(Guid companyUserId,string adminUserId, string businessPartnerNumber) =>
      _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser => iamUser.UserEntityId == adminUserId)
            .SelectMany(iamUser => iamUser.CompanyUser!.Company!.CompanyUsers)
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new CompanyUserBusinessPartnerNumbersDetails(
                companyUser.IamUser!.UserEntityId,
                companyUser.CompanyUserAssignedBusinessPartners.Where(assignedPartner => assignedPartner.BusinessPartnerNumber == businessPartnerNumber).SingleOrDefault()
            ))
            .SingleOrDefaultAsync();

    public Task<bool> IsAdminUserAndCompanyUserHavingSameCompanyId(Guid companyUserId, string adminUserId) =>
    _dbContext.IamUsers
        .Where(iamUser => iamUser.UserEntityId == adminUserId)
        .SelectMany(iamUser => iamUser.CompanyUser!.Company!.CompanyUsers)
        .AnyAsync(user => user.Id == companyUserId);
}
