using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public class UserRolesRepository : IUserRolesRepository
{
    private readonly PortalDbContext _dbContext;

    public UserRolesRepository(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public CompanyUserAssignedRole CreateCompanyUserAssignedRole(Guid companyUserId, Guid userRoleId) =>
        _dbContext.CompanyUserAssignedRoles.Add(
            new CompanyUserAssignedRole(
                companyUserId,
                userRoleId
            )).Entity;

    public IAsyncEnumerable<UserRoleData> GetUserRoleDataUntrackedAsync(IEnumerable<Guid> userRoleIds) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRoleIds.Contains(userRole.Id))
            .Select(userRole => new UserRoleData(
                userRole.Id,
                userRole.IamClient!.ClientClientId,
                userRole.UserRoleText))
            .ToAsyncEnumerable();

    public async IAsyncEnumerable<Guid> GetUserRoleIdsUntrackedAsync(IDictionary<string, IEnumerable<string>> clientRoles)
    {
        foreach (var clientRole in clientRoles)
        {
            await foreach (var userRoleId in _dbContext.UserRoles
                .AsNoTracking()
                .Where(userRole => userRole.IamClient!.ClientClientId == clientRole.Key && clientRole.Value.Contains(userRole.UserRoleText))
                .AsQueryable()
                .Select(userRole => userRole.Id)
                .AsAsyncEnumerable().ConfigureAwait(false))
            {
                yield return userRoleId;
            }
        }
    }

    public IAsyncEnumerable<UserRoleWithId> GetUserRoleWithIdsUntrackedAsync(string clientClientId, IEnumerable<string> userRoles) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.IamClient!.ClientClientId == clientClientId && userRoles.Contains(userRole.UserRoleText))
            .AsQueryable()
            .Select(userRole => new UserRoleWithId(
                userRole.UserRoleText,
                userRole.Id
            ))
            .AsAsyncEnumerable();

    public async IAsyncEnumerable<UserRoleWithId> GetUserRoleUntrackedAsync(IDictionary<string, IEnumerable<string>> clientRoles)
    {
        foreach (var clientRole in clientRoles)
        {
            await foreach (var userRoleId in _dbContext.UserRoles
                .AsNoTracking()
                .Where(userRole => userRole.IamClient!.ClientClientId == clientRole.Key && clientRole.Value.Contains(userRole.UserRoleText))
                .AsQueryable()
                .Select(userRole => new 
                {
                   Name = userRole.UserRoleText,
                   Id = userRole.Id
                })
                .AsAsyncEnumerable().ConfigureAwait(false))
            {
                yield return new UserRoleWithId
                (
                    userRoleId.Name,
                    userRoleId.Id
                );
            }
        }
    }
}
