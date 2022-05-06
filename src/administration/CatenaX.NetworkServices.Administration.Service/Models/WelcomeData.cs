using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Administration.Service.Models
{
    public class WelcomeData
    {
        [JsonPropertyName("userName")]
        public string userName { get; set; }
        [JsonPropertyName("email")]
        public string email { get; set; }
        [JsonPropertyName("companyName")]
        public string companyName { get; set; }
    }
}
