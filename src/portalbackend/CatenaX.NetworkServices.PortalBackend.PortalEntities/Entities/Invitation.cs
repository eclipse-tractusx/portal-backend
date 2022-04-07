using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class Invitation : BaseEntity
    {
        public InvitationStatusId InvitationStatusId { get; set; }
        public Guid? CompanyApplicationId { get; set; }
        public Guid CompanyUserId { get; set; }

        public virtual CompanyApplication? CompanyApplication { get; set; }
        public virtual CompanyUser? CompanyUser { get; set; }
        public virtual InvitationStatus? InvitationStatus { get; set; }
    }
}
