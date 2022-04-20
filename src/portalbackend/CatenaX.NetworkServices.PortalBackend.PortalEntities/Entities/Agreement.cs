using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class Agreement
    {
        public Agreement()
        {
            Consents = new HashSet<Consent>();
            AgreementAssignedCompanyRoles = new HashSet<AgreementAssignedCompanyRole>();
            AgreementAssignedDocumentTemplates = new HashSet<AgreementAssignedDocumentTemplate>();
        }

        public Agreement(string name) : this()
        {
            Name = name;
        }

        public int AgreementCategoryId { get; set; }

        [Key]
        public Guid Id { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateLastChanged { get; set; }

        [MaxLength(255)]
        public string? AgreementType { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public Guid? AppId { get; set; }

        public Guid IssuerCompanyId { get; set; }

        public Guid? UseCaseId { get; set; }

        public virtual AgreementCategory? AgreementCategory { get; set; }
        public virtual App? App { get; set; }
        public virtual Company? IssuerCompany { get; set; }
        public virtual UseCase? UseCase { get; set; }
        public virtual ICollection<Consent> Consents { get; set; }
        public virtual ICollection<AgreementAssignedCompanyRole> AgreementAssignedCompanyRoles { get; set; }
        public virtual ICollection<AgreementAssignedDocumentTemplate> AgreementAssignedDocumentTemplates { get; set; }
    }
}
