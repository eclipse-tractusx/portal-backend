using Newtonsoft.Json;

namespace CatenaX.NetworkServices.Provisioning.Library.Models
{
    public class UserInfo
    {
        [JsonProperty("id")]
        public string userId { get; set; }
        [JsonProperty("username")]
        public string userName { get; set; }
        [JsonProperty("enabled")]
        public bool? enabled { get; set; }
        [JsonProperty("firstName")]
        public string firstName { get; set; }
        [JsonProperty("lastName")]
        public string lastName { get; set; }
        [JsonProperty("email")]
        public string eMail { get; set; }
    }
}