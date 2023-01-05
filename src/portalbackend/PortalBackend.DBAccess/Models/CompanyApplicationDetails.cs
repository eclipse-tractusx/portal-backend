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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

public class CompanyApplicationDetails
{
    public CompanyApplicationDetails(Guid applicationId, CompanyApplicationStatusId companyApplicationStatusId, DateTimeOffset dateCreated, string companyName, IEnumerable<DocumentDetails> documents, IEnumerable<string> companyRoles)
    {
        ApplicationId = applicationId;
        CompanyApplicationStatusId = companyApplicationStatusId;
        DateCreated = dateCreated;
        CompanyName = companyName;
        CompanyRoles = companyRoles;
        Documents = documents;
    }

    [JsonPropertyName("applicationId")]
    public Guid ApplicationId { get; set; }
    [JsonPropertyName("applicationStatus")]
    public CompanyApplicationStatusId CompanyApplicationStatusId { get; set; }
    [JsonPropertyName("dateCreated")]
    public DateTimeOffset DateCreated { get; set; }
    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; }
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    [JsonPropertyName("bpn")]
    public string? BusinessPartnerNumber { get; set; }
    [JsonPropertyName("documents")]
    public IEnumerable<DocumentDetails> Documents { get; set; }
    [JsonPropertyName("companyRoles")]
    public IEnumerable<string> CompanyRoles { get; set; }
}

public class DocumentDetails
{
    public DocumentDetails(Guid documentId)
    {
        DocumentId = documentId;
    }

    [JsonPropertyName("documentType")]
    public DocumentTypeId? DocumentTypeId { get; set; }
    [JsonPropertyName("documentId")]
    public Guid DocumentId { get; set; }
}
