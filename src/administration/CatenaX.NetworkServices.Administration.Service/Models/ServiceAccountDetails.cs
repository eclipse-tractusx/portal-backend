using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.Provisioning.Library.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Administration.Service.Models;

public class ServiceAccountDetails
{
    public ServiceAccountDetails(Guid serviceAccountId, string clientId, string name, string description, IamClientAuthMethod iamClientAuthMethod, IEnumerable<UserRoleData> userRoleDatas)
    {
        ServiceAccountId = serviceAccountId;
        ClientId = clientId;
        Name = name;
        Description = description;
        IamClientAuthMethod = iamClientAuthMethod;
        UserRoleDatas = userRoleDatas;
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

    [JsonPropertyName("roles")]
    public IEnumerable<UserRoleData> UserRoleDatas { get; set; }
}
