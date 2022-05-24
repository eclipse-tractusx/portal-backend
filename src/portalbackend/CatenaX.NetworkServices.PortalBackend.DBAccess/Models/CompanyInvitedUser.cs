using System;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class CompanyInvitedUser
    {

        public Guid CompanyId { get; set; }
        public Guid CompanyUserId { get; set; }
        public string UserEntityId { get; set; }
    }
}
