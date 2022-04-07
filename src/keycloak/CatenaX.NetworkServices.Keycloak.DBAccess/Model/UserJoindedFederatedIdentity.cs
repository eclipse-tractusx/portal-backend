namespace CatenaX.NetworkServices.Keycloak.DBAccess.Model

{
    public class UserJoinedFederatedIdentity
    {
        public string id { get; set; } 
        public string email { get; set; }
        public bool enabled { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string federated_user_id { get; set; }
        public string federated_username { get; set; }
    }
}
