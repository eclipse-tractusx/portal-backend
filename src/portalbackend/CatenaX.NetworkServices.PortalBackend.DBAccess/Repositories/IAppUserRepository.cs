using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;


public interface IAppUserRepository
{
   IQueryable<CompanyUser> GetCompanyAppUsersUntrackedAsync( Guid appId,string iamUserId);
}
