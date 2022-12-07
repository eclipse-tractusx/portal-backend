using Newtonsoft.Json;

namespace Org.CatenaX.Ng.Portal.Backend.Offers.Library.Models;

public record OfferDeclineRequest(
    [property: JsonProperty("message")]string Message);