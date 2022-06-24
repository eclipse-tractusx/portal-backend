using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

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

    /// <inheritdoc />
    public Document CreateDocument(Guid companyUserId, string documentName, byte[] documentContent, byte[] hash, DocumentTypeId documentTypeId) =>
        _dbContext.Documents.Add(
            new Document(
                Guid.NewGuid(),
                documentContent,
                hash,
                documentName,
                DateTimeOffset.UtcNow,
                DocumentStatusId.PENDING)
            {
                DocumentTypeId = documentTypeId,
                CompanyUserId = companyUserId
            }).Entity;

    /// <inheritdoc />
    public Task<Document?> GetDocumentByIdAsync(Guid documentId) => 
        _dbContext.Documents.SingleOrDefaultAsync(x => x.Id == documentId);

    /// <inheritdoc />
    public Task<Tuple<(Guid DocumentId, DocumentStatusId DocumentStatusId, Guid? CompanyUserId)>?>
        GetDetailsForIdAsync(Guid documentId) =>
        _dbContext.Documents
            .Where(x => x.Id == documentId)
            .Select(x => new Tuple<(Guid DocumentId, DocumentStatusId DocumentStatusId, Guid? CompanyUserId)>(new (x.Id, x.DocumentStatusId, x.CompanyUserId)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void AttachToDatabase(Document document)
    {
        _dbContext.Documents.Attach(document);
    }
}
