using System;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class CompanyWithAddress
    {
        private CompanyWithAddress()
        {
            Name = null!;
            City = null!;
            Streetname = null!;
            CountryAlpha2Code = null!;
        }

        public CompanyWithAddress(Guid companyId, string name, string city, string streetName, decimal zipcode, string countryAlpha2Code)
        {
            CompanyId = companyId;
            Name = name;
            City = city;
            Streetname = streetName;
            Zipcode = zipcode;
            CountryAlpha2Code = countryAlpha2Code;
        }

        public Guid CompanyId { get; set; }
        public string? Bpn { get; set; }
        public string Name { get; set; }
        public string? Shortname { get; set; }
        public string City { get; set; }
        public string? Region { get; set; }
        public string? Streetadditional { get; set; }
        public string Streetname { get; set; }
        public string? Streetnumber { get; set; }
        public decimal Zipcode { get; set; }
        public string CountryAlpha2Code { get; set; }
        public string? CountryDe { get; set; }
        public string? TaxId { get; set; }
    }
}
