using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.UserAdministration.Service.Models
{
    public class RegistrationData
    {
        [JsonPropertyName("userName")]
        public string userName { get; set; }
        [JsonPropertyName("email")]
        public string email { get; set; }
        [JsonPropertyName("organisationName")]
        public string organisationName { get; set; }
    }
}
