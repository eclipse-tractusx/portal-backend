using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public record CompanyApplicationWithCompanyUserDetails(
    [property: JsonPropertyName("applicationId")] Guid ApplicationId,
    [property: JsonPropertyName("applicationStatus")] CompanyApplicationStatusId CompanyApplicationStatusId,
    [property: JsonPropertyName("dateCreated")] DateTimeOffset DateCreated,
    [property: JsonPropertyName("companyName")] string CompanyName)
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }
}
