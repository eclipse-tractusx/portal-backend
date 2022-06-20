using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess;

public class PortalRepositories : IPortalRepositories
{
    private readonly PortalDbContext _dbContext;

    public PortalRepositories(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public RepositoryType GetInstance<RepositoryType>()
    {
        var repositoryType = typeof(RepositoryType);

        if (repositoryType == typeof(IApplicationRepository))
        {
            return To<RepositoryType>(new ApplicationRepository(_dbContext));
        }
        else if (repositoryType == typeof(IAppUserRepository))
        {
            return To<RepositoryType>(new AppUserRepository(_dbContext));
        }
        else if (repositoryType == typeof(IConnectorsRepository))
        {
            return To<RepositoryType>(new ConnectorsRepository(_dbContext));
        }
        else if (repositoryType == typeof(IServiceAccountsRepository))
        {
            return To<RepositoryType>(new ServiceAccountRepository(_dbContext));
        }
        else if (repositoryType == typeof(IUserBusinessPartnerRepository))
        {
            return To<RepositoryType>(new UserBusinessPartnerRepository(_dbContext));
        }
        else if (repositoryType == typeof(IUserRepository))
        {
            return To<RepositoryType>(new UserRepository(_dbContext));
        }
        else if (repositoryType == typeof(IUserRolesRepository))
        {
            return To<RepositoryType>(new UserRolesRepository(_dbContext));
        }
        else
        {
            throw new ArgumentException($"unexpected type {typeof(RepositoryType).Name}",nameof(RepositoryType));
        }
    }

    public Task<int> SaveAsync() => _dbContext.SaveChangesAsync();

    private static T To<T>(dynamic value) => (T) value;
}
