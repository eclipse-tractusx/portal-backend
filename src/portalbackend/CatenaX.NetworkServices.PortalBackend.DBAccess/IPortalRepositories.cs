namespace CatenaX.NetworkServices.PortalBackend.DBAccess;

public interface IPortalRepositories
{
    public T GetInstance<T>();

    public Task<int> SaveAsync();
}
