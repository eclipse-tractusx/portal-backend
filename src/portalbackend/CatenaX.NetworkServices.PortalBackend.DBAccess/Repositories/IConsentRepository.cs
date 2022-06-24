using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for accessing consents on the persistence layer.
/// </summary>
public interface IConsentRepository
{
    /// <summary>
    /// Gets the consent ids for the given documentId
    /// </summary>
    /// <param name="documentId">The id of the document</param>
    /// <returns>All consents for the given documentId</returns>
    IAsyncEnumerable<Guid> GetConsentIdsForDocumentId(Guid documentId);

    /// <summary>
    /// Attaches the consents to the database
    /// </summary>
    /// <param name="consents">The consents that should be attached to the database.</param>
    void AttachToDatabase(params Consent[] consents);
}