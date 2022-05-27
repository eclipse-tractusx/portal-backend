using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Administration.Service.Models;

public class UserUpdateBpn
{
    public UserUpdateBpn(string userId, IEnumerable<string> bpns)
    {
        UserId = userId;
        BusinessPartnerNumbers = bpns;
    }

    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    [JsonPropertyName("bpns")]
    public IEnumerable<string> BusinessPartnerNumbers { get; set; }
}
