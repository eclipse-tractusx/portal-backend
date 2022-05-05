using System;
using System.Collections.Generic;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class InvitedUser
    {
        public string EmailId { get; set; }
        public string InvitationStatus { get; set; }
        public string UserId { get; set; }
        public IEnumerable<string> InvitedUserRoles { get; set; }
    }
}