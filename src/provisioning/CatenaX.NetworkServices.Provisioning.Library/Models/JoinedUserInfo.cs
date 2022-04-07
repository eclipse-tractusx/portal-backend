namespace CatenaX.NetworkServices.Provisioning.Library.Models

{
    public class JoinedUserInfo
    {
        public string userId { get; set; }
        public string providerUserId { get; set; }
        public bool enabled { get; set; }
        public string userName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
    }
}
