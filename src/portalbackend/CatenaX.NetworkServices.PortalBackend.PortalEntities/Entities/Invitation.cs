using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class Invitation
    {
        private Invitation() {}

        public Invitation(Guid id, Guid companyApplicationId, Guid companyUserId, InvitationStatusId invitationStatusId, DateTimeOffset dateCreated)
        {
            Id = id;
            DateCreated = dateCreated;
            CompanyApplicationId = companyApplicationId;
            CompanyUserId = companyUserId;
            InvitationStatusId = invitationStatusId;
        }

        [Key]
        public Guid Id { get; private set; }

        public DateTimeOffset DateCreated { get; private set; }

        public InvitationStatusId InvitationStatusId { get; set; }
        public Guid CompanyApplicationId { get; private set; }
        public Guid CompanyUserId { get; private set; }

        // Navigation properties
        public virtual CompanyApplication? CompanyApplication { get; private set; }
        public virtual CompanyUser? CompanyUser { get; private set; }
        public virtual InvitationStatus? InvitationStatus { get; set; }
    }
}
