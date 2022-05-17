using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyUserDetails
{
    public CompanyUserDetails(string iamUserId, Guid companyUserId, CompanyUserStatusId companyUserStatusId)
    {
        IamUserId = iamUserId;
        CompanyUserId = companyUserId;
        CompanyUserStatusId = companyUserStatusId;
    }

    [JsonPropertyName("userEntityId")]
    public string IamUserId { get; set; }

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
