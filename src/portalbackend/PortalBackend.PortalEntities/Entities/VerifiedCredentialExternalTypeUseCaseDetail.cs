using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class VerifiedCredentialExternalTypeUseCaseDetail : IBaseEntity
{
    public VerifiedCredentialExternalTypeUseCaseDetail()
    {
        Version = null!;
        Template = null!;
        CompanySsiDetails = new HashSet<CompanySsiDetail>();
    }

    public Guid Id { get; set; }
    public VerifiedCredentialExternalTypeId VerifiedCredentialExternalTypeId { get; set; }
    public string Version { get; set; }
    public string Template { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? Expiry { get; set; }

    public virtual VerifiedCredentialExternalType? VerifiedCredentialExternalType { get; private set; }
    public virtual ICollection<CompanySsiDetail>? CompanySsiDetails { get; private set; }
}
