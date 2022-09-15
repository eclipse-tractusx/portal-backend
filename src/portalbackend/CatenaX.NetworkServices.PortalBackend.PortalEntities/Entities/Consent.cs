using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class Consent
{
    private Consent() 
    {
        ConsentAssignedOffers = new HashSet<ConsentAssignedOffer>();
    }

    /// <summary>
    /// Please only use when attaching the Consent to the database
    /// </summary>
    /// <param name="id"></param>
    public Consent(Guid id): this()
    {
        Id = id;
    }

    public Consent(Guid id, Guid agreementId, Guid companyId, Guid companyUserId, ConsentStatusId consentStatusId, DateTimeOffset dateCreated): this()
    {
        Id = id;
        AgreementId = agreementId;
        CompanyId = companyId;
        CompanyUserId = companyUserId;
        ConsentStatusId = consentStatusId;
        DateCreated = dateCreated;
    }

    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    [MaxLength(255)]
    public string? Comment { get; set; }

    public ConsentStatusId ConsentStatusId { get; set; }

    [MaxLength(255)]
    public string? Target { get; set; }

    public Guid AgreementId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid? DocumentId { get; set; }
    public Guid CompanyUserId { get; private set; }

    // Navigation properties
    public virtual Agreement? Agreement { get; private set; }
    public virtual Company? Company { get; private set; }
    public virtual CompanyUser? CompanyUser { get; private set; }
    public virtual ConsentStatus? ConsentStatus { get; set; }
    public virtual Document? Document { get; set; }
    public virtual ICollection<ConsentAssignedOffer> ConsentAssignedOffers { get; private set; }
}
