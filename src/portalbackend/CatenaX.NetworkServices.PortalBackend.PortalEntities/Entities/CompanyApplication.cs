using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;
using System.Collections.Generic;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyApplication : BaseEntity
    {
        public CompanyApplication()
        {
            Invitations = new HashSet<Invitation>();
        }

        public CompanyApplicationStatusId? ApplicationStatusId { get; set; }
        public Guid CompanyId { get; set; }

        public virtual CompanyApplicationStatus? ApplicationStatus { get; set; }
        public virtual Company? Company { get; set; }
        public virtual ICollection<Invitation> Invitations { get; set; }
    }
}
