using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

/// <summary>
/// Business logic for document handling
/// </summary>
public class DocumentsBusinessLogic : IDocumentsBusinessLogic
{
    private readonly IDocumentRepository _documentRepository;

    /// <summary>
    /// Creates a new instance <see cref="DocumentsBusinessLogic"/>
    /// </summary>
    public DocumentsBusinessLogic(IPortalRepositories portalRepositories)
    {
        _documentRepository = portalRepositories.GetInstance<IDocumentRepository>();
    }

    /// <inheritdoc />
    public async Task<(string fileName, byte[] content)> GetDocumentAsync(Guid documentId, string iamUserId)
    {
        var document = await _documentRepository.GetDocumentByIdAsync(documentId).ConfigureAwait(false);
        if (document is null)
        {
            throw new NotFoundException("No document with the given id was found.");
        }

        return (document.DocumentName, document.DocumentContent);
    }
}
