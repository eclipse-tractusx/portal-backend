using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;
using CatenaX.NetworkServices.Framework.Models;

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
                userRole.Offer!.AppInstances.First().IamClient!.ClientClientId,
                userRole.UserRoleText))
            .ToAsyncEnumerable();

    public async IAsyncEnumerable<Guid> GetUserRoleIdsUntrackedAsync(IDictionary<string, IEnumerable<string>> clientRoles)
    {
        foreach (var clientRole in clientRoles)
        {
            await foreach (var userRoleId in _dbContext.UserRoles
                .AsNoTracking()
                .Where(userRole => userRole.Offer!.AppInstances.Any(x => x.IamClient!.ClientClientId == clientRole.Key) && clientRole.Value.Contains(userRole.UserRoleText))
                .Select(userRole => userRole.Id)
                .AsAsyncEnumerable()
                .ConfigureAwait(false))
            {
                yield return userRoleId;
            }
        }
    }

    public IAsyncEnumerable<UserRoleWithId> GetUserRoleWithIdsUntrackedAsync(string clientClientId, IEnumerable<string> userRoles) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.Offer!.AppInstances.Any(x => x.IamClient!.ClientClientId == clientClientId) && userRoles.Contains(userRole.UserRoleText))
            .Select(userRole => new UserRoleWithId(
                userRole.UserRoleText,
                userRole.Id
            ))
            .AsAsyncEnumerable();

    public async IAsyncEnumerable<UserRoleData> GetUserRoleDataUntrackedAsync(IDictionary<string, IEnumerable<string>> clientRoles)
    {
        foreach (var clientRole in clientRoles)
        {
            await foreach (var userRoleData in _dbContext.UserRoles
                .AsNoTracking()
                .Where(userRole => userRole.Offer!.AppInstances.Any(ai => ai.IamClient!.ClientClientId == clientRole.Key) && clientRole.Value.Contains(userRole.UserRoleText))
                .Select(userRole => new UserRoleData(
                    userRole.Id,
                    userRole.Offer!.AppInstances.Single(ai => ai.IamClient!.ClientClientId == clientRole.Key).IamClient!.ClientClientId,
                    userRole.UserRoleText
                ))
                .AsAsyncEnumerable())
            {
                yield return userRoleData;
            }
        }
    }

     public IAsyncEnumerable<string> GetClientRolesCompositeAsync(string keyCloakClientId) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.Offer!.AppInstances.Any(x => x.IamClient!.ClientClientId == keyCloakClientId))
            .Select(userRole => userRole.UserRoleText)
            .AsAsyncEnumerable();

    public IAsyncEnumerable<UserRoleWithDescription> GetServiceAccountRolesAsync(string clientId, string? languageShortName = null) =>
       _dbContext.UserRoles
           .AsNoTracking()
           .Where(userRole => userRole.Offer!.AppInstances.Any(x => x.IamClient!.ClientClientId == clientId))
           .Select(userRole => new UserRoleWithDescription(
                   userRole.Id,
                   userRole.UserRoleText,
                   userRole.UserRoleDescriptions.SingleOrDefault(desc =>
                   desc.LanguageShortName == (languageShortName ?? Constants.DefaultLanguage))!.Description))
           .AsAsyncEnumerable();
}
