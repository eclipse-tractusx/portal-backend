using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class IamUser : BaseEntity
    {
        public Guid CompanyUserId { get; set; }

        public virtual CompanyUser? CompanyUser { get; set; }
    }
}
