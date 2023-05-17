using System.ComponentModel.DataAnnotations;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.AuditEntities;

public class AuditConnector20230503 : IAuditEntityV1
{
    public AuditConnector20230503()
    {
        Name = null!;
        ConnectorUrl = null!;
        LocationId = null!;
    }
    
    /// <inheritdoc />
    [Key]
    public Guid AuditV1Id { get; set; }

    public Guid Id { get; private set; }

    [MaxLength(255)]
    public string Name { get; set; }

    [MaxLength(255)]
    public string ConnectorUrl { get; set; }

    public ConnectorTypeId TypeId { get; set; }

    public ConnectorStatusId StatusId { get; set; }

    public Guid ProviderId { get; set; }

    public Guid? HostId { get; set; }

    /// <summary>
    /// Link to the self description document
    /// </summary>
    public Guid? SelfDescriptionDocumentId { get; set; }

    [StringLength(2, MinimumLength = 2)]
    public string LocationId { get; set; }

    public bool? DapsRegistrationSuccessful { get; set; }

    public string? SelfDescriptionMessage { get; set; }

    public DateTimeOffset? DateLastChanged { get; private set; }

    public Guid? CompanyServiceAccountId { get; set; }

    [AuditLastEditorV1]
    public Guid? LastEditorId { get; set; }

    /// <inheritdoc />
    public DateTimeOffset AuditV1DateLastChanged { get; set; }

    /// <inheritdoc />
    public Guid? AuditV1LastEditorId { get; set; }

    /// <inheritdoc />
    public AuditOperationId AuditV1OperationId { get; set; }
}
