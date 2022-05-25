using CatenaX.NetworkServices.Provisioning.Library.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Provisioning.Library.ViewModels;

public class ServiceAccountCreationInfo
{
    public ServiceAccountCreationInfo(string name, string description, IamClientAuthMethod iamClientAuthMethod)
    {
        Name = name;
        Description = description;
        IamClientAuthMethod = iamClientAuthMethod;
    }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("authenticationType")]
    public IamClientAuthMethod IamClientAuthMethod { get; set; }
}
