using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class OfferLicense
{
    private OfferLicense()
    {
        Licensetext = null!;
        Apps = new HashSet<Offer>();
    }

    public OfferLicense(Guid id, string licensetext) : this()
    {
        Id = id;
        Licensetext = licensetext;
    }

    public Guid Id { get; private set; }

    [MaxLength(255)]
    public string Licensetext { get; set; }

    // Navigation properties
    public virtual ICollection<Offer> Apps { get; private set; }
}
