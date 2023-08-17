using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public record Token(
    [property: JsonPropertyName("access_token")]
    string AccessToken
);
