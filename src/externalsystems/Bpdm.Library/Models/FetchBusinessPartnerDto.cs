/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models
{
    [Obsolete("delete as soon endpoint GET /api/registration/company/{bpn} is being removed")]
    public class FetchBusinessPartnerDto
    {
        [JsonPropertyName("bpn")]
        public string Bpn { get; set; } = null!;

        [JsonPropertyName("identifiers")]
        public Identifier[] Identifiers { get; set; } = null!;

        [JsonPropertyName("names")]
        public Name[] Names { get; set; } = null!;

        [JsonPropertyName("legalForm")]
        public Legalform? LegalForm { get; set; }

        [JsonPropertyName("status")]
        public Status? Status { get; set; }

        [JsonPropertyName("addresses")]
        public Address[] Addresses { get; set; } = null!;

        [JsonPropertyName("profileClassifications")]
        public object[] ProfileClassifications { get; set; } = null!;

        [JsonPropertyName("types")]
        public Type[] Types { get; set; } = null!;

        [JsonPropertyName("bankAccounts")]
        public Bankaccount[] BankAccounts { get; set; } = null!;

        [JsonPropertyName("roles")]
        public string[] Roles { get; set; } = null!;

        [JsonPropertyName("relations")]
        public object[] Relations { get; set; } = null!;
    }

    public class Legalform
    {
        [JsonPropertyName("technicalKey")]
        public string TechnicalKey { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("url")]
        public string Url { get; set; } = null!;

        [JsonPropertyName("mainAbbreviation")]
        public string MainAbbreviation { get; set; } = null!;

        [JsonPropertyName("language")]
        public Language? Language { get; set; }

        [JsonPropertyName("category")]
        public Category[] Category { get; set; } = null!;
    }

    public class Bankaccount
    {
        [JsonPropertyName("trustScores")]
        public double[] TrustScores { get; set; } = null!;

        [JsonPropertyName("currencyCode")]
        public string CurrencyCode { get; set; } = null!;

        [JsonPropertyName("internationalBankAccountIdentifier")]
        public string InternationalBankAccountIdentifier { get; set; } = null!;

        [JsonPropertyName("internationalBankIdentifier")]
        public string InternationalBankIdentifier { get; set; } = null!;

        [JsonPropertyName("nationalBankAccountIdentifier")]
        public string NationalBankAccountIdentifier { get; set; } = null!;

        [JsonPropertyName("nationalBankIdentifier")]
        public string NationalBankIdentifier { get; set; } = null!;
    }

    public class Language
    {
        [JsonPropertyName("technicalKey")]
        public string TechnicalKey { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }

    public class Category
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("url")]
        public string Url { get; set; } = null!;
    }

    public class Status
    {
        [JsonPropertyName("officialDenotation")]
        public object OfficialDenotation { get; set; } = null!;

        [JsonPropertyName("validFrom")]
        public DateTime? ValidFrom { get; set; }

        [JsonPropertyName("validUntil")]
        public object ValidUntil { get; set; } = null!;

        [JsonPropertyName("type")]
        public Type Type { get; set; } = null!;
    }

    public class Type
    {
        [JsonPropertyName("technicalKey")]
        public string TechnicalKey { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("url")]
        public string Url { get; set; } = null!;
    }

    public class Identifier
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;

        [JsonPropertyName("type")]
        public Type Type { get; set; } = null!;

        [JsonPropertyName("issuingBody")]
        public Issuingbody IssuingBody { get; set; } = null!;

        [JsonPropertyName("status")]
        public Status Status { get; set; } = null!;
    }

    public class Issuingbody
    {
        [JsonPropertyName("technicalKey")]
        public string TechnicalKey { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("url")]
        public string Url { get; set; } = null!;
    }

    public class Name
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;

        [JsonPropertyName("shortName")]
        public object ShortName { get; set; } = null!;

        [JsonPropertyName("type")]
        public Type Type { get; set; } = null!;

        [JsonPropertyName("language")]
        public Language Language { get; set; } = null!;
    }

    public class Address
    {
        [JsonPropertyName("versions")]
        public Versions Versions { get; set; } = null!;

        [JsonPropertyName("careOf")]
        public object CareOf { get; set; } = null!;

        [JsonPropertyName("contexts")]
        public object[] Contexts { get; set; } = null!;

        [JsonPropertyName("country")]
        public Country Country { get; set; } = null!;

        [JsonPropertyName("administrativeAreas")]
        public Administrativearea[] AdministrativeAreas { get; set; } = null!;

        [JsonPropertyName("postCodes")]
        public Postcode[] PostCodes { get; set; } = null!;

        [JsonPropertyName("localities")]
        public Locality[] Localities { get; set; } = null!;

        [JsonPropertyName("thoroughfares")]
        public Thoroughfare[] Thoroughfares { get; set; } = null!;

        [JsonPropertyName("premises")]
        public Premis[] Premises { get; set; } = null!;

        [JsonPropertyName("postalDeliveryPoints")]
        public Postaldeliverypoint[] PostalDeliveryPoints { get; set; } = null!;

        [JsonPropertyName("geographicCoordinates")]
        public object GeographicCoordinates { get; set; } = null!;

        [JsonPropertyName("types")]
        public Type[] Types { get; set; } = null!;
    }

    public class Postaldeliverypoint
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;

        [JsonPropertyName("shortName")]
        public string ShortName { get; set; } = null!;

        [JsonPropertyName("number")]
        public int? Number { get; set; }
    }

    public class Premis
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;

        [JsonPropertyName("shortName")]
        public string ShortName { get; set; } = null!;

        [JsonPropertyName("number")]
        public int? Number { get; set; }
    }

    public class Versions
    {
        [JsonPropertyName("characterSet")]
        public Characterset CharacterSet { get; set; } = null!;

        [JsonPropertyName("language")]
        public Language Language { get; set; } = null!;
    }

    public class Characterset
    {
        [JsonPropertyName("technicalKey")]
        public string TechnicalKey { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }

    public class Country
    {
        [JsonPropertyName("technicalKey")]
        public string TechnicalKey { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }

    public class Administrativearea
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;

        [JsonPropertyName("shortName")]
        public string ShortName { get; set; } = null!;

        [JsonPropertyName("fipsCode")]
        public string FipsCode { get; set; } = null!;

        [JsonPropertyName("type")]
        public Type Type { get; set; } = null!;

        [JsonPropertyName("language")]
        public Language Language { get; set; } = null!;
    }

    public class Postcode
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;

        [JsonPropertyName("type")]
        public Type Type { get; set; } = null!;
    }

    public class Locality
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;

        [JsonPropertyName("shortName")]
        public object ShortName { get; set; } = null!;

        [JsonPropertyName("type")]
        public Type Type { get; set; } = null!;

        [JsonPropertyName("language")]
        public Language Language { get; set; } = null!;
    }

    public class Thoroughfare
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;

        [JsonPropertyName("name")]
        public object Name { get; set; } = null!;

        [JsonPropertyName("shortName")]
        public object ShortName { get; set; } = null!;

        [JsonPropertyName("number")]
        public string Number { get; set; } = null!;

        [JsonPropertyName("direction")]
        public object Direction { get; set; } = null!;

        [JsonPropertyName("type")]
        public Type Type { get; set; } = null!;

        [JsonPropertyName("language")]
        public Language Language { get; set; } = null!;
    }
}

