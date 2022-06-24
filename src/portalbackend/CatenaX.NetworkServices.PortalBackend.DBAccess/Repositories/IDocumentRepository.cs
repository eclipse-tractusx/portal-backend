using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
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
    Document CreateDocument(Guid companyUserId, string documentName, byte[] documentContent, byte[] hash, uint documentOId, DocumentTypeId documentTypeId);

    /// <summary>
    /// Gets all documents for the given applicationId, documentId and userId
    /// </summary>
    /// <param name="applicationId">Id of the application</param>
    /// <param name="documentTypeId">Id of the document type</param>
    /// <param name="iamUserId">Id of the user</param>
    /// <returns>A collection of documents</returns>
    IAsyncEnumerable<UploadDocuments> GetUploadedDocumentsAsync(Guid applicationId, DocumentTypeId documentTypeId, string iamUserId);
    
    /// <summary>
    /// Gets the document with the given id from the persistence layer.
    /// </summary>
    /// <param name="documentId">Id of the document</param>
    /// <returns>Returns the document</returns>
    Task<Document?> GetDocumentByIdAsync(Guid documentId);

    /// <summary>
    /// Gets the documents userid by the document id
    /// </summary>
    /// <param name="documentId">id of the document the user id should be selected for</param>
    /// <returns>Returns the user id if a document is found for the given id, otherwise null</returns>
    Task<Guid?> GetDocumentsCompanyUserByDocumentIdAsync(Guid documentId);
}
