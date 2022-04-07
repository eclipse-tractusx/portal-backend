using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class Document : BaseEntity
    {
        public Document() {}
        public Document(string documentHash, string documentname, byte[] documentuploaddate, string documentversion)
        {
            Consents = new HashSet<Consent>();
            Documenthash = documentHash;
            Documentname = documentname;
            Documentuploaddate = documentuploaddate;
            Documentversion = documentversion;
        }

        public uint DocumentOid { get; set; } // FIXME: What is this good for?

        [MaxLength(255)]
        public string Documenthash { get; set; }

        [MaxLength(255)]
        public string Documentname { get; set; }

        public byte[] Documentuploaddate { get; set; }

        [MaxLength(255)]
        public string Documentversion { get; set; }

        public Guid? CompanyUserId { get; set; }

        public virtual CompanyUser? CompanyUser { get; set; }
        public virtual ICollection<Consent> Consents { get; set; }

    }
}
