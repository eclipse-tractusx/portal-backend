using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;

public record SsiCertificateData
(
    VerifiedCredentialTypeId CredentialType,
    IEnumerable<CompanySsiDetailData> SsiDetailData
);
