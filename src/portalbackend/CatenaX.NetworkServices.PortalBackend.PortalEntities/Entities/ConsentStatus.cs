using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class ConsentStatus
{
    private ConsentStatus()
    {
        Label = null!;
        Consents = new HashSet<Consent>();
    }

    public ConsentStatus(ConsentStatusId consentStatusId) : this()
    {
        Id = consentStatusId;
        Label = consentStatusId.ToString();
    }

    public ConsentStatusId Id { get; private set; }

    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<Consent> Consents { get; private set; }
}
