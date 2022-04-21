namespace CatenaX.NetworkServices.Provisioning.Library.Models
{
    public class IdentityProviderClientConfig
    {
        public IdentityProviderClientConfig(string RedirectUri, string JwksUrl)
        {
            this.RedirectUri = RedirectUri;
            this.JwksUrl = JwksUrl;
        }
        public string RedirectUri;
        public string JwksUrl;
    }
}
