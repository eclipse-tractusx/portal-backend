using CatenaX.NetworkServices.Provisioning.Library.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Provisioning.Library.ViewModels;

public class ServiceAccountCreationInfo
{
    public ServiceAccountCreationInfo(string name, string description, IamClientAuthMethod iamClientAuthMethod, IEnumerable<Guid> userRoleIds)
    {
        Name = name;
        Description = description;
        IamClientAuthMethod = iamClientAuthMethod;
        UserRoleIds = userRoleIds;
    }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("authenticationType")]
    public IamClientAuthMethod IamClientAuthMethod { get; set; }

    [JsonPropertyName("roleIds")]
    public IEnumerable<Guid> UserRoleIds { get; set; }
}
