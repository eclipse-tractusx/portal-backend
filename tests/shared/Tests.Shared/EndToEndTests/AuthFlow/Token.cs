using Newtonsoft.Json;

namespace Tests.Shared.RestAssured.AuthFlow;

public record Token(
    [JsonProperty(PropertyName = "access_token")]
    string AccessToken
);