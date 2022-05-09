using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Registration.Service.Model
{
    public class UserInvitationData
    {
        public UserInvitationData(string userName, string firstName, string lastName, string email)
        {
            this.userName = userName;
            this.firstName = firstName;
            this.lastName = lastName;
            this.email = email;
        }

        [JsonPropertyName("userName")]
        public string userName { get; set; }
        [JsonPropertyName("firstName")]
        public string firstName { get; set; }
        [JsonPropertyName("lastName")]
        public string lastName { get; set; }
        [JsonPropertyName("email")]
        public string email { get; set; }
    }
}
