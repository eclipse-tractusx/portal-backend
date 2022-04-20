using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class Consent
    {
        public Consent() {}

        public Consent(DateTime dateCreated, ConsentStatusId consentStatusId, Guid agreementId, Guid companyId, Guid companyUserId)
        {
            DateCreated = dateCreated;
            ConsentStatusId = consentStatusId;
            AgreementId = agreementId;
            CompanyId = companyId;
            CompanyUserId = companyUserId;
        }

        [Key]
        public Guid Id { get; set; }

        public DateTime DateCreated { get; set; }

        [MaxLength(255)]
        public string? Comment { get; set; }

        public ConsentStatusId ConsentStatusId { get; set; }

        [MaxLength(255)]
        public string? Target { get; set; }

        public Guid AgreementId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid? DocumentsId { get; set; }
        public Guid CompanyUserId { get; set; }

        public virtual Agreement? Agreement { get; set; }
        public virtual Company? Company { get; set; }
        public virtual CompanyUser? CompanyUser { get; set; }
        public virtual ConsentStatus? ConsentStatus { get; set; }
        public virtual Document? Documents { get; set; }
    }
}
