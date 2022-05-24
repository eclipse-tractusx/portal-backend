using CatenaX.NetworkServices.Provisioning.Library.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Administration.Service.Models;

public class ServiceAccountEditableDetails
{
    public ServiceAccountEditableDetails(Guid serviceAccountId, string name, string description, IamClientAuthMethod iamClientAuthMethod)
    {
        ServiceAccountId = serviceAccountId;
        Name = name;
        Description = description;
        IamClientAuthMethod = iamClientAuthMethod;
    }

    [JsonPropertyName("serviceAccountId")]
    public Guid ServiceAccountId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("authenticationType")]
    public IamClientAuthMethod IamClientAuthMethod { get; set; }
}
