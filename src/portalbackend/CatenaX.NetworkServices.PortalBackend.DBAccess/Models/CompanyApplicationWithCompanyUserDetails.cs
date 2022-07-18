using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyApplicationWithCompanyUserDetails
{
    public CompanyApplicationWithCompanyUserDetails(CompanyApplicationStatusId companyApplicationStatusId, DateTimeOffset dateCreated, string companyName)
    {
        CompanyApplicationStatusId = companyApplicationStatusId;
        DateCreated = dateCreated;
        CompanyName = companyName;
    }
    
    [JsonPropertyName("applicationStatus")]
    public CompanyApplicationStatusId CompanyApplicationStatusId { get; set; }
   
    [JsonPropertyName("dateCreated")]
    public DateTimeOffset DateCreated { get; set; }
    
    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; }
    
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }
}
