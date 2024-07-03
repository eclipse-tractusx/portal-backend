using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

public record OwnServiceAccountData(
    IEnumerable<Guid> UserRoleIds,
    CompanyServiceAccount ServiceAccount,
    Guid? ConnectorId,
    string? ClientClientId,
    ConnectorStatusId? StatusId,
    OfferSubscriptionStatusId? OfferStatusId,
    bool IsDimServiceAccount,
    Guid? ProcessId
);
