using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class Document
{
    private Document()
    {
        DocumentHash = null!;
        DocumentName = null!;
        DocumentContent = null!;
        Consents = new HashSet<Consent>();
    }
    
    /// <summary>
    /// Please only use when attaching the Document to the database
    /// </summary>
    /// <param name="id"></param>
    public Document(Guid id) : this()
    {
        Id = id;
    }

    public Document(Guid id, byte[] documentContent, byte[] documentHash, string documentName, DateTimeOffset dateCreated, DocumentStatusId documentStatusId) : this()
    {
        Id = id;
        DocumentContent = documentContent;
        DocumentHash = documentHash;
        DocumentName = documentName;
        DateCreated = dateCreated;
        DocumentStatusId = documentStatusId;
    }

    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    public byte[] DocumentHash { get; set; }

    public byte[] DocumentContent { get; set; }

    [MaxLength(255)]
    public string DocumentName { get; set; }

    public DocumentTypeId? DocumentTypeId { get; set; }

    public DocumentStatusId DocumentStatusId { get; set; }

    public Guid? CompanyUserId { get; set; }

    // Navigation properties
    public virtual CompanyUser? CompanyUser { get; set; }
    public virtual DocumentType? DocumentType { get; set; }
    public virtual DocumentStatus? DocumentStatus { get; set; }
    public virtual ICollection<Consent> Consents { get; private set; }
}
