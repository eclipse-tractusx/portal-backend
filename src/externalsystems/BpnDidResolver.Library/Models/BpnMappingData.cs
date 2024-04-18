using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Models;

public record BpnMappingData(
    [property: JsonPropertyName("bpn")] string Bpn,
    [property: JsonPropertyName("did")] string Did
);
