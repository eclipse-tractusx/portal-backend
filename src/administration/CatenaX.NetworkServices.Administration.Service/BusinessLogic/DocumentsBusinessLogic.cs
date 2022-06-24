using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

/// <summary>
/// Repository for writing documents on persistence layer.
/// </summary>
public class DocumentsBusinessLogic : IDocumentsBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    /// Creates a new instance of <see cref="DocumentsBusinessLogic"/>
    /// </summary>
    /// <param name="portalRepositories">Portal repositories</param>
    public DocumentsBusinessLogic(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
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
