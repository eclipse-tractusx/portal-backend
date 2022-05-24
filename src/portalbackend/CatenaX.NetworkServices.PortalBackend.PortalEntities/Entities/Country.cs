using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class Country
{
    private Country()
    {
        Alpha2Code = null!;
        CountryNameDe = null!;
        CountryNameEn = null!;
        Addresses = new HashSet<Address>();
        Connectors = new HashSet<Connector>();
    }

    public Country(string alpha2Code, string countryNameDe, string countryNameEn) : this()
    {
        Alpha2Code = alpha2Code;
        CountryNameDe = countryNameDe;
        CountryNameEn = countryNameEn;
    }

    [Key]
    [StringLength(2,MinimumLength = 2)]
    public string Alpha2Code { get; private set; }

    [StringLength(3, MinimumLength = 3)]
    public string? Alpha3Code { get; set; }

    [MaxLength(255)]
    public string CountryNameDe { get; set; }

    [MaxLength(255)]
    public string CountryNameEn { get; set; }

    // Navigation properties
    public virtual ICollection<Address> Addresses { get; private set; }
    public virtual ICollection<Connector> Connectors { get; private set; }
}
