using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

public record OfferSubscriptionClientCreationData(
    Guid OfferId,
    OfferTypeId OfferType,
    string OfferUrl,
    bool IsTechnicalUserNeeded
);

public record OfferSubscriptionTechnicalUserCreationData(
    bool IsTechnicalUserNeeded,
    string? ClientId,
    string? OfferName,
    string CompanyName,
    Guid CompanyId,
    string? Bpn,
    OfferTypeId OfferTypeId
);
