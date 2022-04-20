using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class Country
    {
        public Country()
        {
            Addresses = new HashSet<Address>();
        }

        public Country(string alpha2Code, string countryNameDe, string countryNameEn) : this()
        {
            Alpha2Code = alpha2Code;
            CountryNameDe = countryNameDe;
            CountryNameEn = countryNameEn;
        }

        [Key]
        [StringLength(2,MinimumLength = 2)]
        public string Alpha2Code { get; set; }

        [StringLength(3, MinimumLength = 3)]
        public string? Alpha3Code { get; set; }

        [MaxLength(255)]
        public string CountryNameDe { get; set; }

        [MaxLength(255)]
        public string CountryNameEn { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
    }
}
