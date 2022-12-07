/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models
{
    public class RegistrationData
    {
        public RegistrationData(Guid companyId, string name, IEnumerable<CompanyRoleId> companyRoleIds, IEnumerable<RegistrationDocumentNames> documents, IEnumerable<AgreementConsentStatusForRegistrationData> agreementConsentStatuses)
        {
            CompanyId = companyId;
            Name = name;
            CompanyRoleIds = companyRoleIds;
            Documents = documents;
            AgreementConsentStatuses = agreementConsentStatuses;
        }

        [JsonPropertyName("companyId")]
        public Guid CompanyId { get; set; }

        [JsonPropertyName("bpn")]
        public string? BusinessPartnerNumber { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("shortName")]
        public string? Shortname { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("streetAdditional")]
        public string? Streetadditional { get; set; }

        [JsonPropertyName("streetName")]
        public string? Streetname { get; set; }

        [JsonPropertyName("streetNumber")]
        public string? Streetnumber { get; set; }

        [JsonPropertyName("zipCode")]
        public string? Zipcode { get; set; }

        [JsonPropertyName("countryAlpha2Code")]
        public string? CountryAlpha2Code { get; set; }

        [JsonPropertyName("countryDe")]
        public string? CountryDe { get; set; }

        [JsonPropertyName("taxId")]
        public string? TaxId { get; set; }

        [JsonPropertyName("companyRoles")]
        public IEnumerable<CompanyRoleId> CompanyRoleIds { get; set; }

        [JsonPropertyName("agreements")]
        public IEnumerable<AgreementConsentStatusForRegistrationData> AgreementConsentStatuses { get; set; }
        public IEnumerable<RegistrationDocumentNames> Documents { get; set; }

    }

    public class AgreementConsentStatusForRegistrationData
    {
        public AgreementConsentStatusForRegistrationData(Guid agreementId, ConsentStatusId consentStatusId)
        {
            AgreementId = agreementId;
            ConsentStatusId = consentStatusId;
        }

        private AgreementConsentStatusForRegistrationData() {}

        [JsonPropertyName("agreementId")]
        public Guid AgreementId { get; set; }

        [JsonPropertyName("consentStatus")]
        public ConsentStatusId ConsentStatusId { get; set; }
    }

}
