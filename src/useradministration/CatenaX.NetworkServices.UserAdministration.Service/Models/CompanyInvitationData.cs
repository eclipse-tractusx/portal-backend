using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.UserAdministration.Service.Models
{
    public class CompanyInvitationData
    {
        [JsonPropertyName("userName")]
        public string userName { get; set; }
        [JsonPropertyName("firstName")]
        public string firstName { get; set; }
        [JsonPropertyName("lastName")]
        public string lastName { get; set; }
        [JsonPropertyName("email")]
        public string email { get; set; }
        [JsonPropertyName("organisationName")]
        public string organisationName { get; set; }
    }
}
