using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Registration.Service.Model
{
    public class UserInvitationData
    {
        private UserInvitationData()
        {
            userName = null!;
            firstName = null!;
            lastName = null!;
            email = null!;
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
