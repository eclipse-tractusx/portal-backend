using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class Document
    {
        private Document()
        {
            Documenthash = null!;
            Documentname = null!;
            Consents = new HashSet<Consent>();
        }

        public Document(Guid id, string documentHash, string documentname, DateTime dateCreated) : this()
        {
            Id = id;
            Documenthash = documentHash;
            Documentname = documentname;
            DateCreated = dateCreated;
        }

        [Key]
        public Guid Id { get; private set; }

        public DateTime DateCreated { get; private set; }

        public uint DocumentOid { get; set; } // FIXME: What is this good for?

        [MaxLength(255)]
        public string Documenthash { get; set; }

        [MaxLength(255)]
        public string Documentname { get; set; }

        public DocumentTypeId? DocumentTypeId { get; set; }

        public Guid? CompanyUserId { get; set; }

        public virtual CompanyUser? CompanyUser { get; set; }
        public virtual DocumentType? DocumentType { get; set; }
        public virtual ICollection<Consent> Consents { get; private set; }

    }
}
