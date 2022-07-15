using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyUserData
{
    public CompanyUserData(string userEntityId, Guid companyUserId, CompanyUserStatusId companyUserStatusId)
    {
        UserEntityId = userEntityId;
        CompanyUserId = companyUserId;
        CompanyUserStatusId = companyUserStatusId;
    }
    [JsonPropertyName("userEntityId")]
    public string UserEntityId { get; set; }

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

    [JsonPropertyName("roles")]
    public IEnumerable<string> Roles { get; set; }
}
