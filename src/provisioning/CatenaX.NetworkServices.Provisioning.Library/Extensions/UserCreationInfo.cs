using System.Text.Json.Serialization;
namespace CatenaX.NetworkServices.Provisioning.Library;
public class UserCreationInfo
{
    public UserCreationInfo(string? userName, string email, string? firstName, string? lastName, string? role, string? message)
    {
        this.userName = userName;
        this.eMail = email;
        this.firstName = firstName;
        this.lastName = lastName;
        this.Role = role;
        this.Message = message;
    }

    [JsonPropertyName("userName")]
    public string? userName { get; set; }

    [JsonPropertyName("email")]
    public string eMail { get; set; }

    [JsonPropertyName("firstName")]
    public string? firstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? lastName { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
