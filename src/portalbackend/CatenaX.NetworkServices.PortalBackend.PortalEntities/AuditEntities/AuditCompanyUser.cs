using CatenaX.NetworkServices.PortalBackend.PortalEntities.Auditing;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.AuditEntities;

/// <summary>
/// Audit entity for <see cref="CompanyUser"/> only needed for configuration purposes
/// </summary>
public class AuditCompanyUser : CompanyUser, IAuditEntity
{
    /// <inheritdoc />
    public Guid AuditId { get; set; }

    /// <inheritdoc />
    public AuditOperationId AuditOperationId { get; set; }
    
    /// <inheritdoc />
    public new DateTimeOffset DateLastChanged { get; set; }
}