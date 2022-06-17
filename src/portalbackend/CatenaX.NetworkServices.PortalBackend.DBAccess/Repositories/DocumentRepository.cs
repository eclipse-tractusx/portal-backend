using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// Implementation of <see cref="IDocumentRepository"/> accessing database with EF Core.
public class DocumentRepository : IDocumentRepository
{
    private readonly PortalDbContext _dbContext;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dbContext">PortalDb context.</param>
    public DocumentRepository(PortalDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<Document> CreateDocumentAsync(Guid companyUserId, string documentName, string documentContent, string hash, uint documentOId, DocumentTypeId documentTypeId)
    {
        var result = await _dbContext.Documents.AddAsync(
            new Document(
                Guid.NewGuid(),
                documentContent,
                hash,
                documentName,
                DateTimeOffset.UtcNow)
            {
                DocumentOid = documentOId,
                DocumentTypeId = documentTypeId,
                CompanyUserId = companyUserId
            });
        return result.Entity;
    }
}
