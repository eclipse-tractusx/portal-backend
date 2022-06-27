namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

/// <summary>
/// Repository for writing documents on persistence layer.
/// </summary>
public interface IDocumentsBusinessLogic
{
    Task<(string fileName, byte[] content)> GetDocumentAsync(Guid documentId, string iamUserId);
    
    /// <summary>
    /// Deletes the document and the corresponding consent from the persistence layer.
    /// </summary>
    /// <param name="documentId">Id of the document that should be deleted</param>
    /// <param name="iamUserId"></param>
    /// <returns>Returns <c>true</c> if the document and corresponding consent were deleted successfully. Otherwise a specific error is thrown.</returns>
    Task<bool> DeleteDocumentAsync(Guid documentId, string iamUserId);
}
