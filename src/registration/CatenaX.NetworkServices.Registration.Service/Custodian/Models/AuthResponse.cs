namespace CatenaX.NetworkServices.Registration.Service.Custodian.Models
{
    public class AuthResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public int refresh_expires_in { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }
        public string id_token { get; set; }
        public int notbeforepolicy { get; set; }
        public string session_state { get; set; }
        public string scope { get; set; }
    }
}
