using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <inheritdoc />
public class AppAssignedUseCasesRepository : IAppAssignedUseCasesRepository
{
    private readonly PortalDbContext _dbContext;

    /// <summary>
    /// Creates a new instance of <see cref="AppAssignedUseCasesRepository"/>
    /// </summary>
    /// <param name="dbContext">Access to the database</param>
    public AppAssignedUseCasesRepository(PortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public void AddUseCases(IEnumerable<AppAssignedUseCase> useCases) => 
        _dbContext.AppAssignedUseCases.AddRange(useCases);
}
