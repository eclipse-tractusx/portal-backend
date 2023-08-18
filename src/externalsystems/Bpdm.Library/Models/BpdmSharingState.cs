namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;

public record BpdmPaginationSharingStateOutput(
    IEnumerable<BpdmSharingState>? Content
);

public record BpdmSharingState(
    BpdmSharingStateBusinessPartnerType BusinessPartnerType,
    Guid ExternalId,
    BpdmSharingStateType SharingStateType,
    string? SharingErrorCode,
    string? SharingErrorMessage,
    string? Bpn,
    DateTimeOffset SharingProcessStarted
);

public enum BpdmSharingStateType
{
    Pending = 1,
    Success = 2,
    Error = 3,
    Initial = 4
}

public enum BpdmSharingStateBusinessPartnerType
{
    LEGAL_ENTITY = 1,
    SITE = 2,
    ADDRESS = 3
}
