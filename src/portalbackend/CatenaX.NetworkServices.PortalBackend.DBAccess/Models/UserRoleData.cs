using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class UserRoleData
{
    public UserRoleData(Guid userRoleId, string clientClientId, string userRoleName)
    {
        UserRoleId = userRoleId;
        ClientClientId = clientClientId;
        UserRoleText = userRoleName;
    }

    [JsonPropertyName("roleId")]
    public Guid UserRoleId { get; set; }

    [JsonPropertyName("clientId")]
    public string ClientClientId { get; set; }

    [JsonPropertyName("roleName")]
    public string UserRoleText { get; set; }
}
