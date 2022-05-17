using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class DocumentTemplate
{
    private DocumentTemplate()
    {
        Documenttemplatename = null!;
        Documenttemplateversion = null!;
    }

    public DocumentTemplate(Guid id, string documenttemplatename, string documenttemplateversion, DateTimeOffset dateCreated)
    {
        Id = id;
        Documenttemplatename = documenttemplatename;
        Documenttemplateversion = documenttemplateversion;
        DateCreated = dateCreated;
    }

    [Key]
    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    public DateTimeOffset? DateLastChanged { get; set; }

    [MaxLength(255)]
    public string Documenttemplatename { get; set; }

    [MaxLength(255)]
    public string Documenttemplateversion { get; set; }

    // Navigation properties
    public virtual AgreementAssignedDocumentTemplate? AgreementAssignedDocumentTemplate { get; set; }
}
