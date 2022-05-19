using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class DocumentType
{
    private DocumentType()
    {
        Label = null!;
        Documents = new HashSet<Document>();
    }

    public DocumentType(DocumentTypeId documentTypeId) : this()
    {
        DocumentTypeId = documentTypeId;
        Label = documentTypeId.ToString();
    }
    
    [Key]
    public DocumentTypeId DocumentTypeId { get; private set; }

    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<Document> Documents { get; private set; }
}
