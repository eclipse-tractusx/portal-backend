using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class Invitation
    {
        public Invitation() {}

        public Invitation(InvitationStatusId invitationStatusId, Guid companyApplicationId, Guid companyUserId)
        {
            InvitationStatusId = invitationStatusId;
            CompanyApplicationId = companyApplicationId;
            CompanyUserId = companyUserId;
        }

        [Key]
        public Guid Id { get; set; }

        public DateTime? DateCreated { get; set; }

        public InvitationStatusId InvitationStatusId { get; set; }
        public Guid CompanyApplicationId { get; set; }
        public Guid CompanyUserId { get; set; }

        public virtual CompanyApplication? CompanyApplication { get; set; }
        public virtual CompanyUser? CompanyUser { get; set; }
        public virtual InvitationStatus? InvitationStatus { get; set; }
    }
}
