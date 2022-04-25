using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class InvitationStatus
    {
        private InvitationStatus()
        {
            Label = null!;
            Invitations = new HashSet<Invitation>();
        }

        public InvitationStatus(InvitationStatusId invitationStatusId) : this()
        {
            InvitationStatusId = invitationStatusId;
            Label = invitationStatusId.ToString();
        }

        [Key]
        public InvitationStatusId InvitationStatusId { get; private set; }

        [MaxLength(255)]
        public string Label { get; private set; }

        // Navigation properties
        public virtual ICollection<Invitation> Invitations { get; private set; }
    }
}
