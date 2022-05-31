namespace CatenaX.NetworkServices.Administration.Service.Custodian
{
    public class CustodianSettings
    {
        public CustodianSettings()
        {
            Username = null!;
            Password = null!;
            ClientId = null!;
            GrantType = null!;
            ClientSecret = null!;
            Scope = null!;
            KeyCloakTokenAdress = null!;
            BaseAdress = null!;
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }
        public string GrantType { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
        public string KeyCloakTokenAdress { get; set; }
        public string BaseAdress { get; set; }
    }
}


