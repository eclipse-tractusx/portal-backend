using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Administration.Service.Models;

/// <summary>
/// Model used to request connector registration at sd factory.
/// </summary>
public class ConnectorSdFactoryRequestModel
{
    public ConnectorSdFactoryRequestModel(string companyNumber, string headquarterCountry, string legalCountry, string serviceProvider, string sdType, string bpn, string holder, string issuer)
    {
        CompanyNumber = companyNumber;
        HeadquarterCountry = headquarterCountry;
        LegalCountry = legalCountry;
        ServiceProvider = serviceProvider;
        SdType = sdType;
        Bpn = bpn;
        Holder = holder;
        Issuer = issuer;
    }

    [JsonPropertyName("company_number")]
    public string CompanyNumber { get; set; }

    [JsonPropertyName("headquarter_country")]
    public string HeadquarterCountry { get; set; }

    [JsonPropertyName("legal_country")]
    public string LegalCountry { get; set; }

    [JsonPropertyName("service_provider")]
    public string ServiceProvider { get; set; }

    [JsonPropertyName("sd_type")]
    public string SdType { get; set; }

    [JsonPropertyName("bpn")]
    public string Bpn { get; set; }

    [JsonPropertyName("holder")]
    public string Holder { get; set; }

    [JsonPropertyName("issuer")]
    public string Issuer { get; set; }
}
