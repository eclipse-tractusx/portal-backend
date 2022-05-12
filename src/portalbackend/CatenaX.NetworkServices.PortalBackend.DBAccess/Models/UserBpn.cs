namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class UserBpn
    {
        public UserBpn(string userId, string bpn)
        {
            this.UserId = userId;
            this.BusinessPartnerNumber = bpn;
        }
        public string UserId { get; set; }
        public string BusinessPartnerNumber { get; set; }
    }
}
