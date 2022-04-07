using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class AgreementAssignedDocumentTemplate
    {
        public AgreementAssignedDocumentTemplate() {}
        public AgreementAssignedDocumentTemplate(Agreement agreement, DocumentTemplate documentTemplate)
        {
            Agreement = agreement;
            DocumentTemplate = documentTemplate;
        }

        public Guid AgreementId { get; set; }
        public Guid DocumentTemplateId { get; set; }

        public virtual Agreement Agreement { get; set; }
        public virtual DocumentTemplate DocumentTemplate { get; set; }
    }
}
