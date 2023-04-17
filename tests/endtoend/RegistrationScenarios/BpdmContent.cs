using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public record BpdmContent(
    [property: JsonPropertyName("score")] float Score,
    [property: JsonPropertyName("legalEntity")] BpdmLegalEntityDto LegalEntity
);
