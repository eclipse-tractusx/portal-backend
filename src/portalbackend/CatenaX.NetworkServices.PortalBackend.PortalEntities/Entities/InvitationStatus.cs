using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class InvitationStatus
    {
        public InvitationStatus()
        {
            Invitations = new HashSet<Invitation>();
        }

        [Key]
        public InvitationStatusId InvitationStatusId { get; set; }

        [MaxLength(255)]
        public string? Label { get; set; }

        public virtual ICollection<Invitation> Invitations { get; set; }
    }
}
