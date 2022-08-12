namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

/// <summary>
/// Possible operations for the audit table
/// </summary>
public enum AuditOperationId
{
    /// <summary>
    /// The entity has been inserted
    /// </summary>
    INSERT = 1,
    
    /// <summary>
    /// The entity has been updated
    /// </summary>
    UPDATE = 2,
    
    /// <summary>
    /// The entity has been deleted
    /// </summary>
    DELETE = 3,
}