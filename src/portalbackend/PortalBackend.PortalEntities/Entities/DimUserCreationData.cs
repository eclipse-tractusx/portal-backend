namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class DimUserCreationData(Guid id, Guid serviceAccountId, Guid processId)
{
    public Guid Id { get; set; } = id;
    public Guid ServiceAccountId { get; set; } = serviceAccountId;
    public Guid ProcessId { get; set; } = processId;
    public CompanyServiceAccount? ServiceAccount { get; set; }
    public Process? Process { get; set; }
}
