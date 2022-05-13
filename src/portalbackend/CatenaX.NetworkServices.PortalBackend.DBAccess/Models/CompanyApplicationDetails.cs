using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyApplicationDetails
{
    public CompanyApplicationDetails(Guid applicationId, CompanyApplicationStatusId companyApplicationStatusId, DateTimeOffset dateCreated, string companyName, IEnumerable<DocumentDetails> documents)
    {
        ApplicationId = applicationId;
        CompanyApplicationStatusId = companyApplicationStatusId;
        DateCreated = dateCreated;
        CompanyName = companyName;
        Documents = documents;
    }

    [JsonPropertyName("applicationId")]
    public Guid ApplicationId { get; set; }
    [JsonPropertyName("applicationStatus")]
    public CompanyApplicationStatusId CompanyApplicationStatusId { get; set; }
    [JsonPropertyName("dateCreated")]
    public DateTimeOffset DateCreated { get; set; }
    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; }
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    [JsonPropertyName("bpn")]
    public string? BusinessPartnerNumber { get; set; }
    [JsonPropertyName("documents")]
    public IEnumerable<DocumentDetails> Documents { get; set; }
}

public class DocumentDetails
{
    public DocumentDetails(string documentHash)
    {
        DocumentHash = documentHash;
    }

    [JsonPropertyName("documentType")]
    public DocumentTypeId? DocumentTypeId { get; set; }
    [JsonPropertyName("documentHash")]
    public string DocumentHash { get; set; }
}
