namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

/// <summary>
/// Possible status for documents
/// </summary>
public enum DocumentStatusId
{
    /// <summary>
    /// The document is pending
    /// </summary>
    PENDING = 1,
    
    /// <summary>
    /// The document is locked for changes
    /// </summary>
    LOCKED = 2,
    
    /// <summary>
    /// The document was deleted by the user
    /// </summary>
    INACTIVE = 3,
}
