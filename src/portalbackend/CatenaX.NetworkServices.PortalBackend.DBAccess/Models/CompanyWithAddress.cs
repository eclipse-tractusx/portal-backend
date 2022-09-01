using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public record CompanyWithAddress(
    [property: JsonPropertyName("companyId")] Guid companyId,
    [property: JsonPropertyName("name")] string name,
    [property: JsonPropertyName("city")] string city,
    [property: JsonPropertyName("streetName")] string streetName,
    [property: JsonPropertyName("countryAlpha2Code")] string countryAlpha2Code)
{

    [JsonPropertyName("bpn")]
    public string? BusinessPartnerNumber { get; set; }

    [JsonPropertyName("shortName")]
    public string? Shortname { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("streetAdditional")]
    public string? Streetadditional { get; set; }

    [JsonPropertyName("streetNumber")]
    public string? Streetnumber { get; set; }

    [JsonPropertyName("zipCode")]
    public string? Zipcode { get; set; }

    [JsonPropertyName("countryDe")]
    public string? CountryDe { get; set; }

    [JsonPropertyName("taxId")]
    public string? TaxId { get; set; }
}



