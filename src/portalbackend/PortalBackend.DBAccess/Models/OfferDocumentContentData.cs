using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

public record OfferDocumentContentData(
    bool IsValidDocumentType,
    bool IsDocumentLinkedToOffer,
    bool IsValidOfferType,
    bool IsInactive,
    byte[]? Content,
    string FileName,
    MediaTypeId MediaTypeId
);
