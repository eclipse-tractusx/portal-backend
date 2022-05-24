using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyServiceAccountData
{
    public CompanyServiceAccountData(Guid serviceAccountId, string clientId, string name)
    {
        ServiceAccountId = serviceAccountId;
        ClientId = clientId;
        Name = name;
    }

    [JsonPropertyName("serviceAccountId")]
    public Guid ServiceAccountId { get; set; }

    [JsonPropertyName("clientId")]
    public string ClientId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
