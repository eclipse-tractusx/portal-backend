using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class DocumentTemplate : BaseEntity
    {
        public DocumentTemplate() {}
        public DocumentTemplate(string documenttemplatename, string documenttemplateversion)
        {
            Documenttemplatename = documenttemplatename;
            Documenttemplateversion = documenttemplateversion;
        }

        [MaxLength(255)]
        public string Documenttemplatename { get; set; }

        [MaxLength(255)]
        public string Documenttemplateversion { get; set; }

        public virtual AgreementAssignedDocumentTemplate? AgreementAssignedDocumentTemplate { get; set; }
    }
}
