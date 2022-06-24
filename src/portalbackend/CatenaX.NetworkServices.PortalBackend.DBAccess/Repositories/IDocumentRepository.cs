using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for writing documents on persistence layer.
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Creates a document in the persistence layer.
    /// </summary>
    /// <param name="companyUserId">Id of the company</param>
    /// <param name="documentName">The documents name</param>
    /// <param name="documentContent">The document itself</param>
    /// <param name="hash">Hash of the document</param>
    /// <param name="documentTypeId">Type of the document</param>
    /// <returns>Returns the created document</returns>
    Document CreateDocument(Guid companyUserId, string documentName, byte[] documentContent, byte[] hash, DocumentTypeId documentTypeId);

    /// <summary>
    /// Gets the document by the given id from the persistence layer.
    /// </summary>
    /// <param name="documentId">Id of the document</param>
    /// <returns>The document or null if no document is found</returns>
    Task<Document?> GetDocumentByIdAsync(Guid documentId);

    Task<Tuple<(Guid DocumentId, DocumentStatusId DocumentStatusId, Guid? CompanyUserId)>?> GetDetailsForIdAsync(
        Guid documentId);

    /// <summary>
    /// Attaches the document to the database
    /// </summary>
    /// <param name="document">the document that should be attached to the database</param>
    void AttachToDatabase(Document document);
}
