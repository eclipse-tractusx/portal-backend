using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class Address
{
    private Address()
    {
        City = null!;
        Streetname = null!;
        CountryAlpha2Code = null!;
        Companies = new HashSet<Company>();
    }

    public Address(Guid id, string city, string streetname, decimal zipcode, string countryAlpha2Code, DateTimeOffset dateCreated) : this()
    {
        Id = id;
        DateCreated = dateCreated;
        City = city;
        Streetname = streetname;
        Zipcode = zipcode;
        CountryAlpha2Code = countryAlpha2Code;
    }

    [Key]
    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    public DateTimeOffset? DateLastChanged { get; set; }

    [MaxLength(255)]
    public string City { get; set; }

    [MaxLength(255)]
    public string? Region { get; set; }

    [MaxLength(255)]
    public string? Streetadditional { get; set; }

    [MaxLength(255)]
    public string Streetname { get; set; }

    [MaxLength(255)]
    public string? Streetnumber { get; set; }

    [Precision(19,2)]
    public decimal Zipcode { get; set; }

    [StringLength(2, MinimumLength = 2)]
    public string CountryAlpha2Code { get; set; }

    // Navigation properties
    public virtual Country? Country { get; set; }
    public virtual ICollection<Company> Companies { get; private set; }
}
