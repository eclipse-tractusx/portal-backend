namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class AgreementAssignedDocumentTemplate
{
    private AgreementAssignedDocumentTemplate() {}

    public AgreementAssignedDocumentTemplate(Guid agreementId, Guid documentTemplateId)
    {
        AgreementId = agreementId;
        DocumentTemplateId = documentTemplateId;
    }

    public Guid AgreementId { get; private set; }
    public Guid DocumentTemplateId { get; private set; }

    // Navigation properties
    public virtual Agreement? Agreement { get; private set; }
    public virtual DocumentTemplate? DocumentTemplate { get; private set; }
}
