using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public record NotificationContent(
    [property: JsonPropertyName("offerId")] string OfferId,
    [property: JsonPropertyName("coreOfferName")] string CoreOfferName,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("removedRoles")] string RemovedRoles,
    [property: JsonPropertyName("addedRoles")] string AddedRoles
);
