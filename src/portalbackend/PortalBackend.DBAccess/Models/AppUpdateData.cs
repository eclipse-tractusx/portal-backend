using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace PortalBackend.DBAccess.Models;

public record AppUpdateData
(OfferStatusId OfferState,
    bool IsUserOfProvider,
    IEnumerable<(string, string, string)> OfferDescriptions,
    IEnumerable<(string Shortname, bool IsMatch)> Languages,
    IEnumerable<Guid> MatchingUseCases, 
    ValueTuple<Guid, string, bool> OfferLicense);