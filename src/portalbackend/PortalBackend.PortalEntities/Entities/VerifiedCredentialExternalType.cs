using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class VerifiedCredentialExternalType
{
    private VerifiedCredentialExternalType()
    {
        Label = null!;
        VerifiedCredentialTypeId = default!;
    }

    public VerifiedCredentialExternalType(VerifiedCredentialExternalTypeId id, VerifiedCredentialTypeId verifiedCredentialTypeId) : this()
    {
        Id = id;
        Label = id.GetEnumValue();
        VerifiedCredentialTypeId = verifiedCredentialTypeId;
    }

    public VerifiedCredentialExternalTypeId Id { get; private set; }

    public string Label { get; private set; }

    public VerifiedCredentialTypeId VerifiedCredentialTypeId { get; set; }

    // Navigation properties
    public virtual VerifiedCredentialType? VerifiedCredentialType { get; private set; }
    public virtual ICollection<VerifiedCredentialExternalTypeUseCaseDetail> VerifiedCredentialExternalTypeUseCaseDetails { get; private set; }
}
