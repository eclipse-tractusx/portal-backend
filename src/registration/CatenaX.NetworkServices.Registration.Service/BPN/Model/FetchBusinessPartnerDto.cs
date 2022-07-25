using CatenaX.NetworkServices.Registration.Service.BPN.Model;

using System;

namespace CatenaX.NetworkServices.Registration.Service.BPN.Model
{
    public class FetchBusinessPartnerDto
    {
        public string bpn { get; set; }
        public Identifier[] identifiers { get; set; }
        public Name[] names { get; set; }
        public Legalform? legalForm { get; set; }
        public Status? status { get; set; }
        public Address[] addresses { get; set; }
        public object[] profileClassifications { get; set; }
        public Type[] types { get; set; }
        public Bankaccount[] bankAccounts { get; set; }
        public string[] roles { get; set; }
        public object[] relations { get; set; }
    }

    public class Legalform
    {
        public string technicalKey { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string mainAbbreviation { get; set; }
        public Language? language { get; set; }
        public Category[] category { get; set; }
    }

    public class Bankaccount
    {
        public double[] trustScores { get; set; }
        public string currencyCode { get; set; }
        public string internationalBankAccountIdentifier { get; set; }
        public string internationalBankIdentifier { get; set; }
        public string nationalBankAccountIdentifier { get; set; }
        public string nationalBankIdentifier { get; set; }
    }

    public class Language
    {
        public string technicalKey { get; set; }
        public string name { get; set; }
    }

    public class Category
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Status
    {
        public object officialDenotation { get; set; }
        public DateTime? validFrom { get; set; }
        public object validUntil { get; set; }
        public Type type { get; set; }
    }

    public class Type
    {
        public string technicalKey { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Identifier
    {
        public string value { get; set; }
        public Type type { get; set; }
        public Issuingbody issuingBody { get; set; }
        public Status status { get; set; }
    }

    public class Issuingbody
    {
        public string technicalKey { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }
    public class Name
    {
        public string value { get; set; }
        public object shortName { get; set; }
        public Type type { get; set; }
        public Language language { get; set; }
    }


    public class Address
    {
        public Versions versions { get; set; }
        public object careOf { get; set; }
        public object[] contexts { get; set; }
        public Country country { get; set; }
        public Administrativearea[] administrativeAreas { get; set; }
        public Postcode[] postCodes { get; set; }
        public Locality[] localities { get; set; }
        public Thoroughfare[] thoroughfares { get; set; }
        public Premis[] premises { get; set; }
        public Postaldeliverypoint[] postalDeliveryPoints { get; set; }
        public object geographicCoordinates { get; set; }
        public Type[] types { get; set; }
    }
    public class Postaldeliverypoint
    {
        public string type { get; set; }
        public string value { get; set; }
        public string shortName { get; set; }
        public int? number { get; set; }
    }
    public class Premis
    {
        public string type { get; set; }
        public string value { get; set; }
        public string shortName { get; set; }
        public int? number { get; set; }
    }

    public class Versions
    {
        public Characterset characterSet { get; set; }
        public Language language { get; set; }
    }

    public class Characterset
    {
        public string technicalKey { get; set; }
        public string name { get; set; }
    }

    public class Country
    {
        public string technicalKey { get; set; }
        public string name { get; set; }
    }

    public class Administrativearea
    {
        public string value { get; set; }
        public string shortName { get; set; }
        public string fipsCode { get; set; }
        public Type type { get; set; }
        public Language language { get; set; }
    }

    public class Postcode
    {
        public string value { get; set; }
        public Type type { get; set; }
    }
    public class Locality
    {
        public string value { get; set; }
        public object shortName { get; set; }
        public Type type { get; set; }
        public Language language { get; set; }
    }


    public class Thoroughfare
    {
        public string value { get; set; }
        public object name { get; set; }
        public object shortName { get; set; }
        public string number { get; set; }
        public object direction { get; set; }
        public Type type { get; set; }
        public Language language { get; set; }
    }

}








