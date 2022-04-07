using System;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class Consent : BaseEntity
    {
        public Consent() {}
        public Consent(byte[] timestamp)
        {
            Timestamp = timestamp;
        }

        [MaxLength(255)]
        public string? Comment { get; set; }

        public int ConsentStatusId { get; set; }

        [MaxLength(255)]
        public string? Target { get; set; }

        public byte[] Timestamp { get; set; }

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
