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

        if (repositoryType == typeof(IAppRepository))
        {
            return To<RepositoryType>(new AppRepository(_dbContext));
        }
        if (repositoryType == typeof(IAppAssignedLicensesRepository))
        {
            return To<RepositoryType>(new AppAssignedLicensesRepository(_dbContext));
        }
        if (repositoryType == typeof(IAppAssignedUseCasesRepository))
        {
            return To<RepositoryType>(new AppAssignedUseCasesRepository(_dbContext));
        }
        if (repositoryType == typeof(IAppDescriptionsRepository))
        {
            return To<RepositoryType>(new AppDescriptionsRepository(_dbContext));
        }
        if (repositoryType == typeof(IAppLanguagesRepository))
        {
            return To<RepositoryType>(new AppLanguagesRepository(_dbContext));
        }
        if (repositoryType == typeof(IAppLicensesRepository))
        {
            return To<RepositoryType>(new AppLicensesRepository(_dbContext));
        }
        if (repositoryType == typeof(IApplicationRepository))
        {
            return To<RepositoryType>(new ApplicationRepository(_dbContext));
        }
        if (repositoryType == typeof(IAppUserRepository))
        {
            return To<RepositoryType>(new AppUserRepository(_dbContext));
        }
        if (repositoryType == typeof(ICompanyAssignedAppsRepository))
        {
            return To<RepositoryType>(new CompanyAssignedAppsRepository(_dbContext));
        }
        if (repositoryType == typeof(ICompanyRepository))
        {
            return To<RepositoryType>(new CompanyRepository(_dbContext));
        }
        if (repositoryType == typeof(ICompanyRolesRepository))
        {
            return To<RepositoryType>(new CompanyRolesRepository(_dbContext));
        }
        if (repositoryType == typeof(ICompanyUserAssignedAppFavouritesRepository))
        {
            return To<RepositoryType>(new CompanyUserAssignedAppFavouritesRepository(_dbContext));
        }
        if (repositoryType == typeof(IConnectorsRepository))
        {
            return To<RepositoryType>(new ConnectorsRepository(_dbContext));
        }
        if (repositoryType == typeof(IConsentRepository))
        {
            return To<RepositoryType>(new ConsentRepository(_dbContext));
        }
        if (repositoryType == typeof(IDocumentRepository))
        {
            return To<RepositoryType>(new DocumentRepository(_dbContext));
        }
        if (repositoryType == typeof(IIdentityProviderRepository))
        {
            return To<RepositoryType>(new IdentityProviderRepository(_dbContext));
        }
        if (repositoryType == typeof(IServiceAccountsRepository))
        {
            return To<RepositoryType>(new ServiceAccountRepository(_dbContext));
        }
        if (repositoryType == typeof(IUserBusinessPartnerRepository))
        {
            return To<RepositoryType>(new UserBusinessPartnerRepository(_dbContext));
        }
        if (repositoryType == typeof(IUserRepository))
        {
            return To<RepositoryType>(new UserRepository(_dbContext));
        }
        if (repositoryType == typeof(IUserRolesRepository))
        {
            return To<RepositoryType>(new UserRolesRepository(_dbContext));
        }
        throw new ArgumentException($"unexpected type {typeof(RepositoryType).Name}",nameof(RepositoryType));
    }

    public Task<int> SaveAsync() => _dbContext.SaveChangesAsync();

    private static T To<T>(dynamic value) => (T) value;
}
