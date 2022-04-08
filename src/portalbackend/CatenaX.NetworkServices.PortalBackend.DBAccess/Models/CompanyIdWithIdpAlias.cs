using System;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class CompanyNameIdWithIdpAlias
    {
        public string CompanyName { get; set; }
        public Guid CompanyId { get; set; }
        public string IdpAlias { get; set; }
    }
}
