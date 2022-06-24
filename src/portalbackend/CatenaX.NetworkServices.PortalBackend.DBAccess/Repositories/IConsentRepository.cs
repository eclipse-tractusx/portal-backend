using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for accessing consents on the persistence layer.
/// </summary>
public interface IConsentRepository
{
    /// <summary>
    /// Attaches the consents to the database
    /// </summary>
    /// <param name="consents">The consents that should be attached to the database.</param>
    void AttachToDatabase(IEnumerable<Consent> consents);
}