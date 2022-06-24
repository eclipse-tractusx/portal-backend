using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <inheritdoc />
public class ConsentRepository : IConsentRepository
{
    private readonly PortalDbContext _portalDbContext;

    /// <summary>
    /// Creates an instance of <see cref="ConsentRepository"/>
    /// </summary>
    /// <param name="portalDbContext">The database</param>
    public ConsentRepository(PortalDbContext portalDbContext)
    {
        _portalDbContext = portalDbContext;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetConsentIdsForDocumentId(Guid documentId) =>
        _portalDbContext.Consents
            .Where(x => x.DocumentId == documentId)
            .Select(x => x.Id)
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public void AttachToDatabase(params Consent[] consents) => _portalDbContext.AttachRange(consents);
}