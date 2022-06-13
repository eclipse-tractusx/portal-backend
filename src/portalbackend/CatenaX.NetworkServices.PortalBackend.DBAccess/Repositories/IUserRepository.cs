using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public interface IUserRepository
{
    Task<Guid> GetCompanyIdForIamUserUntrackedAsync(string iamUserId);
    Task<CompanyIamUser> GetIdpUserById(Guid companyUserId, string adminUserId);
    Task<string> GetAppAssignedRolesClientId(Guid appId);
}
