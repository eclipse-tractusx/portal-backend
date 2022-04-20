using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyApplication
    {
        public CompanyApplication()
        {
            Invitations = new HashSet<Invitation>();
        }

        public CompanyApplication(CompanyApplicationStatusId applicationStatusId, Guid companyId) : this()
        {
            ApplicationStatusId = applicationStatusId;
            CompanyId = companyId;
        }

        [Key]
        public Guid Id { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateLastChanged { get; set; }

        public CompanyApplicationStatusId ApplicationStatusId { get; set; }
        public Guid CompanyId { get; set; }

        public virtual CompanyApplicationStatus? ApplicationStatus { get; set; }
        public virtual Company? Company { get; set; }
        public virtual ICollection<Invitation> Invitations { get; set; }
    }
}
