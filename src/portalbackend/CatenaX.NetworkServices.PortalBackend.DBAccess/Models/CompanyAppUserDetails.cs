using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyAppUserDetails
{
    public CompanyAppUserDetails(Guid companyUserId, CompanyUserStatusId companyUserStatusId,IEnumerable<string> roles)
    {
        CompanyUserId = companyUserId;
        CompanyUserStatusId = companyUserStatusId;
        Roles = roles;
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
    public IEnumerable<string> Roles { get; set; }
}
