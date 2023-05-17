using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;

public record OfferDeclineRequest(
    [property: JsonPropertyName("message")] string Message);
