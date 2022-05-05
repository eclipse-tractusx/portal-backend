using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Collections.Generic;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class InvitedUser
    {
        public InvitedUser(string userId, InvitationStatusId invitationStatus)
        {
            UserId = userId;
            InvitationStatus = invitationStatus;
        }

        public string UserId { get; set; }
        public InvitationStatusId InvitationStatus { get; set; }
        public string? EmailId { get; set; }
        public IEnumerable<string>? InvitedUserRoles { get; set; }
    }
}
