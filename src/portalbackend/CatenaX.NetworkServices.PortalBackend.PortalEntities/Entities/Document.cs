using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class Document
    {
        public Document()
        {
            Consents = new HashSet<Consent>();
        }

        public Document(string documentHash, string documentname) : this()
        {
            Documenthash = documentHash;
            Documentname = documentname;
        }

        [Key]
        public Guid Id { get; set; }

        public DateTime? DateCreated { get; set; }

        public uint DocumentOid { get; set; } // FIXME: What is this good for?

        [MaxLength(255)]
        public string Documenthash { get; set; }

        [MaxLength(255)]
        public string Documentname { get; set; }

        public DocumentTypeId? DocumentTypeId { get; set; }

        public Guid? CompanyUserId { get; set; }

        public virtual CompanyUser? CompanyUser { get; set; }
        public virtual DocumentType? DocumentType { get; set; }
        public virtual ICollection<Consent> Consents { get; set; }

    }
}
