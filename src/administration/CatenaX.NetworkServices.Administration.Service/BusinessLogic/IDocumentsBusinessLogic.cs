namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public interface IDocumentsBusinessLogic
{
    Task<(string fileName, byte[] content)> GetDocumentAsync(Guid documentId, string iamUserId);
}
