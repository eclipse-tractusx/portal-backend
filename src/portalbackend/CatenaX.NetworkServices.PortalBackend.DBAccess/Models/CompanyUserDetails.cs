using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public record CompanyUserDetails(
    [property: JsonPropertyName("companyUserId")] Guid companyUserId,
    [property: JsonPropertyName("created")] DateTimeOffset createdAt,
    [property: JsonPropertyName("bpn")] IEnumerable<string> businessPartnerNumbers,
    [property: JsonPropertyName("company")] string companyName,
    [property: JsonPropertyName("status")] CompanyUserStatusId companyUserStatusId,
    [property: JsonPropertyName("assignedRoles")] IEnumerable<CompanyUserAssignedRoleDetails> assignedRoles)
{
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

public record CompanyUserAssignedRoleDetails(
    [property: JsonPropertyName("appId")] Guid appId,
    [property: JsonPropertyName("roles")] IEnumerable<string> userRoles);
