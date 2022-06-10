using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyUserDetails
{
    public CompanyUserDetails(Guid companyUserId, DateTimeOffset createdAt, string companyName, CompanyUserStatusId companyUserStatusId)
    {
        CompanyUserId = companyUserId;
        CreatedAt = createdAt;
        CompanyName = companyName;
        CompanyUserStatusId = companyUserStatusId;
    }

    [JsonPropertyName("companyUserId")]
    public Guid CompanyUserId { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("bpn")]
    public string? BusinessPartnerNumber { get; set; }

    [JsonPropertyName("created")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("company")]
    public string CompanyName { get; set; }

    [JsonPropertyName("status")]
    public CompanyUserStatusId CompanyUserStatusId { get; set; }
}
