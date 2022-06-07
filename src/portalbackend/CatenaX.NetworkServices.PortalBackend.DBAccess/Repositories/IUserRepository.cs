namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public interface IUserRepository
{
    Task<Guid> GetCompanyIdForIamUserUntrackedAsync(string iamUserId);
}
