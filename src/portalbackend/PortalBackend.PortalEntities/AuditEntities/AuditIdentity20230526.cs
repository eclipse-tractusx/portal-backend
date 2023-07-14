using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class AuditIdentity20230526 : IAuditEntityV1
{
    /// <inheritdoc />
    [Key]
    public Guid AuditV1Id { get; set; }

    public Guid Id { get; set; }

    public DateTimeOffset DateCreated { get; set; }

    public Guid CompanyId { get; set; }

    [JsonPropertyName("user_status_id")]
    public UserStatusId UserStatusId { get; set; }

    [StringLength(36)]
    public string? UserEntityId { get; set; }

    public IdentityTypeId IdentityTypeId { get; set; }

    public DateTimeOffset? DateLastChanged { get; set; }

    public Guid? LastEditorId { get; set; }

    /// <inheritdoc />
    public Guid? AuditV1LastEditorId { get; set; }

    /// <inheritdoc />
    public AuditOperationId AuditV1OperationId { get; set; }

    /// <inheritdoc />
    public DateTimeOffset AuditV1DateLastChanged { get; set; }
}
