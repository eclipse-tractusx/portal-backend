using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for persistence layer access relating <see cref="AppAssignedUseCase"/> entities.
/// </summary>
public interface IAppAssignedUseCasesRepository
{
    /// <summary>
    /// Adds <see cref="AppAssignedUseCase"/>s to the database
    /// </summary>
    /// <param name="useCases">The use cases that should be added to the database</param>
    void AddUseCases(IEnumerable<AppAssignedUseCase> useCases);
}
