using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

public record CredentialDetailData
(
    Guid CredentialDetailId,
    Guid CompanyId,
    VerifiedCredentialTypeId CredentialType,
    string? UseCase,
    CompanySsiDetailStatusId ParticipantStatus,
    DateTimeOffset? ExpiryDate,
    DocumentData Document,
    ExternalTypeDetailData? ExternalTypeDetail
);
