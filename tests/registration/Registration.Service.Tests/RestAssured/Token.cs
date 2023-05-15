using Newtonsoft.Json;

namespace Registration.Service.Tests.RestAssured;

public record Token(
    [JsonProperty(PropertyName = "access_token")]
    string AccessToken
);