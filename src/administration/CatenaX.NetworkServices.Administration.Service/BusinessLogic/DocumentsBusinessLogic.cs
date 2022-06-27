using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

/// <summary>
/// Business logic for document handling
/// </summary>
public class DocumentsBusinessLogic : IDocumentsBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    /// Creates a new instance <see cref="DocumentsBusinessLogic"/>
    /// </summary>
    public DocumentsBusinessLogic(IPortalRepositories portalRepositories)
    {
    
        _portalRepositories = portalRepositories;
    }

    /// <inheritdoc />
    public async Task<(string fileName, byte[] content)> GetDocumentAsync(Guid documentId, string iamUserId)
    {
        var document = await this._portalRepositories.GetInstance<IDocumentRepository>().GetDocumentByIdAsync(documentId).ConfigureAwait(false);
        if (document is null)
        {
            throw new NotFoundException("No document with the given id was found.");
        }

        return (document.DocumentName, document.DocumentContent);
    }
    
    /// <inheritdoc />
    public async Task<bool> DeleteDocumentAsync(Guid documentId, string iamUserId)
    {
        var documentRepository = _portalRepositories.GetInstance<IDocumentRepository>();
        var details = await documentRepository.GetDocumentDetailsForIdUntrackedAsync(documentId, iamUserId).ConfigureAwait(false);

        if (details.DocumentId == default)
        {
            throw new NotFoundException("Document is not existing");
        }

        if (!details.IsSameUser)
        {
            throw new ForbiddenException("User is not allowed to delete this document");
        }

        if (details.DocumentStatusId != DocumentStatusId.PENDING)
        {
            throw new ArgumentException("Incorrect document status");
        }

        var document = new Document(details.DocumentId);
        documentRepository.AttachToDatabase(document);
        document.DocumentStatusId = DocumentStatusId.INACTIVE;

        var consents = details.ConsentIds.Select(x => new Consent(x));
        _portalRepositories.GetInstance<IConsentRepository>().AttachToDatabase(consents);

        foreach (var consent in consents)
        {
            consent.ConsentStatusId = ConsentStatusId.INACTIVE;
        }

        await this._portalRepositories.SaveAsync().ConfigureAwait(false);
        return true;
    }
}
