using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class InvitedUserDetail
    {
        public InvitedUserDetail(string userId, InvitationStatusId invitationStatus, string? emailId)
        {
            UserId = userId;
            InvitationStatus = invitationStatus;
            EmailId = emailId;
        }

        public string UserId { get; set; }
        public InvitationStatusId InvitationStatus { get; set; }
        public string? EmailId { get; set; }
    }
}
