using System.ComponentModel.DataAnnotations;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Auditing;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class AuditOperation
{
    private AuditOperation()
    {
        Label = null!;
    }

    public AuditOperation(AuditOperationId auditOperationId) : this()
    {
        Id = auditOperationId;
        Label = auditOperationId.ToString();
    }

    public AuditOperationId Id { get; private set; }

    [MaxLength(255)]
    public string Label { get; private set; }
}
