using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyUserDetails
{
    public CompanyUserDetails(Guid companyUserId, DateTimeOffset createdAt, IEnumerable<string> businessPartnerNumbers, string companyName, CompanyUserStatusId companyUserStatusId,IEnumerable<CompanyUserAssignedRoleDetails> assignedRoles)
    {
        CompanyUserId = companyUserId;
        CreatedAt = createdAt;
        BusinessPartnerNumbers = businessPartnerNumbers;
        CompanyName = companyName;
        CompanyUserStatusId = companyUserStatusId;
        AssignedRoles = assignedRoles;
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
    public IEnumerable<string> BusinessPartnerNumbers { get; set; }

    [JsonPropertyName("created")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("company")]
    public string CompanyName { get; set; }

    [JsonPropertyName("status")]
    public CompanyUserStatusId CompanyUserStatusId { get; set; }

    [JsonPropertyName("assignedRoles")]
    public IEnumerable<CompanyUserAssignedRoleDetails> AssignedRoles { get; set; }
}

public class CompanyUserAssignedRoleDetails
{
    public CompanyUserAssignedRoleDetails(Guid appId, IEnumerable<string> roles)
    {
        AppId = appId;
        UserRoles = roles;
    }

    [JsonPropertyName("appId")]
    public Guid AppId { get; set; }

    [JsonPropertyName("roles")]
    public IEnumerable<string> UserRoles { get; set; }

}
