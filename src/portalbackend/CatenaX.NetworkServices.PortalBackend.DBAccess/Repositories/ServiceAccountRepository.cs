using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public class ServiceAccountRepository : IServiceAccountRepository
{
    private readonly PortalDbContext _dbContext;

    private ServiceAccountRepository(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public CompanyServiceAccount CreateCompanyServiceAccount(Guid companyId, CompanyServiceAccountStatusId companyServiceAccountStatusId, string name, string description) =>
        _dbContext.CompanyServiceAccounts.Add(
            new CompanyServiceAccount(
                Guid.NewGuid(),
                companyId,
                companyServiceAccountStatusId,
                name,
                description,
                DateTimeOffset.UtcNow)).Entity;

    public IamServiceAccount CreateIamServiceAccount(string clientId, string clientClientId, string userEntityId, Guid companyServiceAccountId) =>
        _dbContext.IamServiceAccounts.Add(
            new IamServiceAccount(
                clientId,
                clientClientId,
                userEntityId,
                companyServiceAccountId)).Entity;

    public CompanyServiceAccountAssignedRole CreateCompanyServiceAccountAssignedRole(Guid companyServiceAccountId, Guid userRoleId) =>
        _dbContext.CompanyServiceAccountAssignedRoles.Add(
            new CompanyServiceAccountAssignedRole(
                companyServiceAccountId,
                userRoleId)).Entity;

    public IamServiceAccount RemoveIamServiceAccount(IamServiceAccount iamServiceAccount) =>
        _dbContext.Remove(iamServiceAccount).Entity;

    public CompanyServiceAccountAssignedRole RemoveCompanyServiceAccountAssignedRole(CompanyServiceAccountAssignedRole companyServiceAccountAssignedRole) =>
        _dbContext.Remove(companyServiceAccountAssignedRole).Entity;

    public Task<CompanyServiceAccountWithRoleDataClientId?> GetOwnCompanyServiceAccountWithIamClientIdAsync(Guid serviceAccountId, string adminUserId) =>
        _dbContext.CompanyServiceAccounts
            .Where(serviceAccount =>
                serviceAccount.Id == serviceAccountId
                && serviceAccount.CompanyServiceAccountStatusId == CompanyServiceAccountStatusId.ACTIVE
                && serviceAccount.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId))
            .Select(serviceAccount =>
                new CompanyServiceAccountWithRoleDataClientId(
                    serviceAccount,
                    serviceAccount.IamServiceAccount!.ClientId,
                    serviceAccount.IamServiceAccount.ClientClientId,
                    serviceAccount.CompanyServiceAccountAssignedRoles
                        .Select(assignedRole => assignedRole.UserRole)
                        .Select(userRole => new UserRoleData(
                            userRole!.Id,
                            userRole.IamClient!.ClientClientId,
                            userRole.UserRoleText))))
            .SingleOrDefaultAsync();

    public Task<CompanyServiceAccount?> GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(Guid serviceAccountId, string adminUserId) =>
        _dbContext.CompanyServiceAccounts
            .Where(serviceAccount =>
                serviceAccount.Id == serviceAccountId
                && serviceAccount.CompanyServiceAccountStatusId == CompanyServiceAccountStatusId.ACTIVE
                && serviceAccount.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId))
            .Include(serviceAccount => serviceAccount.IamServiceAccount)
            .Include(serviceAccount => serviceAccount.CompanyServiceAccountAssignedRoles)
            .SingleOrDefaultAsync();

    public Task<CompanyServiceAccountDetailedData?> GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(Guid serviceAccountId, string adminUserId) =>
        _dbContext.CompanyServiceAccounts
            .AsNoTracking()
            .Where(serviceAccount =>
                serviceAccount.Id == serviceAccountId
                && serviceAccount.CompanyServiceAccountStatusId == CompanyServiceAccountStatusId.ACTIVE
                && serviceAccount.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId))
            .Select(serviceAccount => new CompanyServiceAccountDetailedData(
                    serviceAccount.Id,
                    serviceAccount.IamServiceAccount!.ClientId,
                    serviceAccount.IamServiceAccount.ClientClientId,
                    serviceAccount.IamServiceAccount.UserEntityId,
                    serviceAccount.Name,
                    serviceAccount.Description,
                    serviceAccount.CompanyServiceAccountAssignedRoles
                        .Select(assignedRole => assignedRole.UserRole)
                        .Select(userRole => new UserRoleData(
                            userRole!.Id,
                            userRole.IamClient!.ClientClientId,
                            userRole.UserRoleText))))
            .SingleOrDefaultAsync();

    public IQueryable<CompanyServiceAccount> GetOwnCompanyServiceAccountsUntracked(string adminUserId) =>
        _dbContext.Companies
            .AsNoTracking()
            .Where(company =>
                company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId))
            .SelectMany(company => company.CompanyServiceAccounts)
            .Where(serviceAccount => serviceAccount.CompanyServiceAccountStatusId == CompanyServiceAccountStatusId.ACTIVE);
}
