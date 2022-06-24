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
        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
        var companyUserId = await userRepository.GetCompanyUserIdForIamUserUntrackedAsync(iamUserId).ConfigureAwait(false);
        var details = await documentRepository.GetDetailsForIdAsync(documentId).ConfigureAwait(false);

        if (details is null)
        {
            throw new NotFoundException("Document is not existing");
        }

        if (details.Item1.CompanyUserId != companyUserId)
        {
            throw new ForbiddenException("User is not allowed to delete this document");
        }

        if (details.Item1.DocumentStatusId != DocumentStatusId.PENDING)
        {
            throw new ArgumentException("Incorrect document status");
        }

        var document = new Document(details.Item1.DocumentId);
        documentRepository.AttachToDatabase(document);
        document.DocumentStatusId = DocumentStatusId.INACTIVE;

        var consentRepository = _portalRepositories.GetInstance<IConsentRepository>();
        var consentIds = consentRepository.GetConsentIdsForDocumentId(document.Id);
        var consents = await consentIds.Select(x => new Consent(x)).ToArrayAsync();
        consentRepository.AttachToDatabase(consents);
        foreach (var consent in consents)
        {
            consent.ConsentStatusId = ConsentStatusId.INACTIVE;
        }

        await this._portalRepositories.SaveAsync().ConfigureAwait(false);
        return true;
    }
}