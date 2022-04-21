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

        public CompanyApplication(Guid id, Guid companyId, CompanyApplicationStatusId applicationStatusId, DateTime dateCreated) : this()
        {
            Id = id;
            CompanyId = companyId;
            ApplicationStatusId = applicationStatusId;
            DateCreated = dateCreated;
        }

        [Key]
        public Guid Id { get; private set; }

        public DateTime DateCreated { get; private set; }

        public DateTime? DateLastChanged { get; set; }

        public CompanyApplicationStatusId ApplicationStatusId { get; set; }
        public Guid CompanyId { get; private set; }

        public virtual CompanyApplicationStatus? ApplicationStatus { get; set; }
        public virtual Company? Company { get; private set; }
        public virtual ICollection<Invitation> Invitations { get; private set; }
    }
}
