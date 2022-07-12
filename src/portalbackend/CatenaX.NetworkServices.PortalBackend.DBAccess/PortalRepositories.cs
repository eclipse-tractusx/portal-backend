using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

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

        if (repositoryType == typeof(IAppRepository))
        {
            return To<RepositoryType>(new AppRepository(_dbContext));
        }
        else if (repositoryType == typeof(IApplicationRepository))
        {
            return To<RepositoryType>(new ApplicationRepository(_dbContext));
        }
        else if (repositoryType == typeof(IAppUserRepository))
        {
            return To<RepositoryType>(new AppUserRepository(_dbContext));
        }
          else if (repositoryType == typeof(ICompanyAssignedAppsRepository))
        {
            return To<RepositoryType>(new CompanyAssignedAppsRepository(_dbContext));
        }
        else if (repositoryType == typeof(ICompanyRepository))
        {
            return To<RepositoryType>(new CompanyRepository(_dbContext));
        }
        else if (repositoryType == typeof(ICompanyRolesRepository))
        {
            return To<RepositoryType>(new CompanyRolesRepository(_dbContext));
        }
        else if (repositoryType == typeof(IConnectorsRepository))
        {
            return To<RepositoryType>(new ConnectorsRepository(_dbContext));
        }
        else if (repositoryType == typeof(IConsentRepository))
        {
            return To<RepositoryType>(new ConsentRepository(_dbContext));
        }
        else if (repositoryType == typeof(ICountryRepository))
        {
            return To<RepositoryType>(new CountryRepository(_dbContext));
        }
        else if (repositoryType == typeof(IDocumentRepository))
        {
            return To<RepositoryType>(new DocumentRepository(_dbContext));
        }
        else if (repositoryType == typeof(IIdentityProviderRepository))
        {
            return To<RepositoryType>(new IdentityProviderRepository(_dbContext));
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

    public Task<IDbContextTransaction> BeginTransactionAsync() => _dbContext.Database.BeginTransactionAsync();

    private static T To<T>(dynamic value) => (T) value;
}
