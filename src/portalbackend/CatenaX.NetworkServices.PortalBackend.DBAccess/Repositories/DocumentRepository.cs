using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
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
    public Document CreateDocument(Guid companyUserId, string documentName, byte[] documentContent, byte[] hash, uint documentOId, DocumentTypeId documentTypeId) =>
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

    public IAsyncEnumerable<UploadDocuments> GetUploadedDocumentsAsync(Guid applicationId, DocumentTypeId documentTypeId, string iamUserId) =>
        _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser =>
                iamUser.UserEntityId == iamUserId
                && iamUser.CompanyUser!.Company!.CompanyApplications.Any(application => application.Id == applicationId))
            .SelectMany(iamUser => iamUser.CompanyUser!.Documents.Where(docu => docu.DocumentTypeId == documentTypeId))
            .Select(document =>
                new UploadDocuments(
                    document!.Id,
                    document!.DocumentName))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public async Task<Document?> GetDocumentByIdAsync(Guid documentId) =>
        await this._dbContext.Documents.SingleOrDefaultAsync(x => x.Id == documentId);
}
