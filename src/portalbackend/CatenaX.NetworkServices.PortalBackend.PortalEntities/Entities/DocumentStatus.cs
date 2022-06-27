using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class DocumentStatus
{
    private DocumentStatus()
    {
        Label = null!;
        Documents = new HashSet<Document>();
    }

    public DocumentStatus(DocumentStatusId documentStatusId) : this()
    {
        Id = documentStatusId;
        Label = documentStatusId.ToString();
    }
    
    public DocumentStatusId Id { get; private set; }

    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<Document> Documents { get; private set; }
}
