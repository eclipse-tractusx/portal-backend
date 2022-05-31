using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Administration.Service.Models;

public class WelcomeData
{
    public WelcomeData(string userName, string email, string companyName)
    {
        UserName = userName;
        Email = email;
        CompanyName = companyName;
    }

    [JsonPropertyName("userName")]
    public string UserName { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; }
}
