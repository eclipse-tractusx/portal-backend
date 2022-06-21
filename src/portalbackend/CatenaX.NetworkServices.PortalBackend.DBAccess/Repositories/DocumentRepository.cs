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
    public Document CreateDocument(Guid companyUserId, string documentName, string documentContent, string hash, uint documentOId, DocumentTypeId documentTypeId) =>
        _dbContext.Documents.Add(
            new Document(
                Guid.NewGuid(),
                documentContent,
                hash,
                documentName,
                DateTimeOffset.UtcNow,
                DocumentStatusId.PENDING)
            {
                DocumentOid = documentOId,
                DocumentTypeId = documentTypeId,
                CompanyUserId = companyUserId
            }).Entity;

    /// <inheritdoc />
    public async Task<Document?> GetDocumentByIdAsync(Guid documentId) => 
        await _dbContext.Documents.SingleOrDefaultAsync(x => x.Id == documentId);

    /// <inheritdoc />
    public async Task DeleteDocumentAsync(Document document)
    {
        var consents = _dbContext.Consents.Where(x => x.DocumentId == document.Id);
        document.DocumentStatusId = DocumentStatusId.INACTIVE;
        _dbContext.Consents.RemoveRange(consents);
        await _dbContext.SaveChangesAsync();
    }
}
