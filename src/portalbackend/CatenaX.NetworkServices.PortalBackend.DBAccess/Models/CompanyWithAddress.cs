using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyWithAddress
{
    private CompanyWithAddress()
    {
        Name = null!;
        City = null!;
        Streetname = null!;
        CountryAlpha2Code = null!;
    }

    public CompanyWithAddress(Guid companyId, string name, string city, string streetName, string countryAlpha2Code)
    {
        CompanyId = companyId;
        Name = name;
        City = city;
        Streetname = streetName;
        CountryAlpha2Code = countryAlpha2Code;
    }

    [JsonPropertyName("companyId")]
    public Guid CompanyId { get; set; }

    [JsonPropertyName("bpn")]
    public string? Bpn { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("shortName")]
    public string? Shortname { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("streetAdditional")]
    public string? Streetadditional { get; set; }

    [JsonPropertyName("streetName")]
    public string Streetname { get; set; }

    [JsonPropertyName("streetNumber")]
    public string? Streetnumber { get; set; }

    [JsonPropertyName("zipCode")]
    public decimal? Zipcode { get; set; }

    [JsonPropertyName("countryAlpha2Code")]
    public string CountryAlpha2Code { get; set; }

    [JsonPropertyName("countryDe")]
    public string? CountryDe { get; set; }

    [JsonPropertyName("taxId")]
    public string? TaxId { get; set; }
}
