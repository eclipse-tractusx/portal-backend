using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class VerifiedCredentialExternalType
{
    public VerifiedCredentialExternalType(VerifiedCredentialExternalTypeId id)
    {
        Id = id;
        Label = id.ToString();

        VerifiedCredentialTypeAssignedExternalTypes = new HashSet<VerifiedCredentialTypeAssignedExternalType>();
        VerifiedCredentialExternalTypeUseCaseDetailVersions = new HashSet<VerifiedCredentialExternalTypeUseCaseDetailVersion>();
    }

    public VerifiedCredentialExternalTypeId Id { get; private set; }

    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<VerifiedCredentialTypeAssignedExternalType> VerifiedCredentialTypeAssignedExternalTypes { get; private set; }

    public virtual ICollection<VerifiedCredentialExternalTypeUseCaseDetailVersion> VerifiedCredentialExternalTypeUseCaseDetailVersions { get; private set; }
}
