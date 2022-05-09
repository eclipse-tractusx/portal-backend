using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Administration.Service.Models
{
    public class CompanyInvitationData
    {
        public CompanyInvitationData(string userName, string firstName, string lastName, string email, string organisationName)
        {
            this.userName = userName;
            this.firstName = firstName;
            this.lastName = lastName;
            this.email = email;
            this.organisationName = organisationName;
        }

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
