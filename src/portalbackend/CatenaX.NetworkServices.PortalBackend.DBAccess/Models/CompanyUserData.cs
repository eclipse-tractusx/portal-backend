using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyUserData
{
    public CompanyUserData(Guid companyUserId, CompanyUserStatusId companyUserStatusId)
    {
        CompanyUserId = companyUserId;
        CompanyUserStatusId = companyUserStatusId;
    }

    [JsonPropertyName("companyUserId")]
    public Guid CompanyUserId { get; set; }

    [JsonPropertyName("status")]
    public CompanyUserStatusId CompanyUserStatusId { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}
