using CatenaX.NetworkServices.Provisioning.Library.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Administration.Service.Models;

public class ServiceAccountDetails
{
    public ServiceAccountDetails(Guid serviceAccountId, string clientId, string name, string description, IamClientAuthMethod iamClientAuthMethod)
    {
        ServiceAccountId = serviceAccountId;
        ClientId = clientId;
        Name = name;
        Description = description;
        IamClientAuthMethod = iamClientAuthMethod;
    }

    [JsonPropertyName("serviceAccountId")]
    public Guid ServiceAccountId { get; set; }

    [JsonPropertyName("clientId")]
    public string ClientId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("authenticationType")]
    public IamClientAuthMethod IamClientAuthMethod { get; set; }

    [JsonPropertyName("secret")]
    public string? Secret { get; set; }
}
