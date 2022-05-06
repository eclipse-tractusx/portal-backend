using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Collections.Generic;

namespace CatenaX.NetworkServices.Registration.Service.Model
{
    public class InvitedUser
    {
        public InvitedUser(InvitationStatusId invitationStatus, string? emailId, IEnumerable<string> invitedUserRoles)
        {
            InvitationStatus = invitationStatus;
            EmailId = emailId;
            InvitedUserRoles = invitedUserRoles;
        }

        public InvitationStatusId InvitationStatus { get; set; }
        public string? EmailId { get; set; }
        public IEnumerable<string> InvitedUserRoles { get; set; }
    }
}
