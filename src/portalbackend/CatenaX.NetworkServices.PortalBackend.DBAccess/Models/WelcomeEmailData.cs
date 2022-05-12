using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class WelcomeEmailData
    {
        public WelcomeEmailData(string userName, string emailId, string companyName)
        {
            UserName = userName;
            EmailId = emailId;
            CompanyName= companyName;
        }

        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string CompanyName { get; set; }
    }
}
