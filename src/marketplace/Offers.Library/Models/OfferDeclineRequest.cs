using Newtonsoft.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;

public record OfferDeclineRequest(
    [property: JsonProperty("message")]string Message);