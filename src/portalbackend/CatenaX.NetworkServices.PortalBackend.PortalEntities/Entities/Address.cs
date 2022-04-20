using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class Address
    {
        public Address()
        {
            Companies = new HashSet<Company>();
        }

        public Address(string city, string streetname, decimal zipcode, string countryAlpha2Code) : this()
        {
            City = city;
            Streetname = streetname;
            Zipcode = zipcode;
            CountryAlpha2Code = countryAlpha2Code;
        }

        [Key]
        public Guid Id { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateLastChanged { get; set; }

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

        public decimal Zipcode { get; set; }

        [StringLength(2, MinimumLength = 2)]
        public string CountryAlpha2Code { get; set; }

        public virtual Country? Country { get; set; }
        public virtual ICollection<Company> Companies { get; set; }
    }
}
