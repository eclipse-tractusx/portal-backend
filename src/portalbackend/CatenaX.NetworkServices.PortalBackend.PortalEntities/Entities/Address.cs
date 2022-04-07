using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class Address : BaseEntity
    {
        public Address()
        {
            Companies = new HashSet<Company>();
        }

        [MaxLength(255)]
        public string? City { get; set; }

        [MaxLength(255)]
        public string? Region { get; set; }

        [MaxLength(255)]
        public string? Streetadditional { get; set; }

        [MaxLength(255)]
        public string? Streetname { get; set; }

        [MaxLength(255)]
        public string? Streetnumber { get; set; }

        public decimal? Zipcode { get; set; }

        [StringLength(2, MinimumLength = 2)]
        public string? CountryAlpha2Code { get; set; }

        public virtual Country? Country { get; set; }
        public virtual ICollection<Company> Companies { get; set; }
    }
}
