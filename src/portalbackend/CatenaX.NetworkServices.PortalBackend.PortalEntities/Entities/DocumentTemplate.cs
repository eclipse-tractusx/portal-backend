using System;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class DocumentTemplate
    {
        public DocumentTemplate() {}
        public DocumentTemplate(string documenttemplatename, string documenttemplateversion)
        {
            Documenttemplatename = documenttemplatename;
            Documenttemplateversion = documenttemplateversion;
        }

        [Key]
        public Guid Id { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateLastChanged { get; set; }

        [MaxLength(255)]
        public string Documenttemplatename { get; set; }

        [MaxLength(255)]
        public string Documenttemplateversion { get; set; }

        public virtual AgreementAssignedDocumentTemplate? AgreementAssignedDocumentTemplate { get; set; }
    }
}
