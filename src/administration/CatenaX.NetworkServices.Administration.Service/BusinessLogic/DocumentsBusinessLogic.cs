using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
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
        var companyUserId = await userRepository.GetCompanyUserIdForIamUserUntrackedAsync(iamUserId);
        var document = await documentRepository.GetDocumentByIdAsync(documentId);

        if (document is null)
        {
            throw new NotFoundException("Document is not existing");
        }

        if (document.CompanyUserId != companyUserId)
        {
            throw new ForbiddenException("User is not allowed to delete this document");
        }

        if (document.DocumentStatusId != DocumentStatusId.PENDING)
        {
            throw new ArgumentException("Incorrect document status");
        }

        await documentRepository.DeleteDocumentAsync(document);
        await this._portalRepositories.SaveAsync().ConfigureAwait(false);
        return true;
    }

}