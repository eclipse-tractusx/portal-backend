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
    /// <param name="documentOId"></param>
    /// <param name="documentTypeId">Type of the document</param>
    /// <returns>Returns the created document</returns>
    Task<Document> CreateDocumentAsync(Guid companyUserId, string documentName, string documentContent, string hash, uint documentOId, DocumentTypeId documentTypeId);

    /// <summary>
    /// Gets the document with the given id from the persistence layer.
    /// </summary>
    /// <param name="documentId">Id of the document</param>
    /// <returns>Returns the document</returns>
    Task<Document?> GetDocumentByIdAsync(Guid documentId);
}
