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

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;

public record CompanyApplicationDetails(
    [property: JsonPropertyName("applicationId")] Guid ApplicationId,
    [property: JsonPropertyName("applicationStatus")] CompanyApplicationStatusId CompanyApplicationStatusId,
    [property: JsonPropertyName("dateCreated")] DateTimeOffset DateCreated,
    [property: JsonPropertyName("companyName")] string CompanyName,
    [property: JsonPropertyName("documents")] IEnumerable<DocumentDetails> Documents,
    [property: JsonPropertyName("companyRoles")] IEnumerable<CompanyRoleId> CompanyRoles,
    [property: JsonPropertyName("applicationChecklist")] IEnumerable<ApplicationChecklistEntryDetails> ApplicationChecklist,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("bpn")] string? BusinessPartnerNumber
);

public record DocumentDetails(
    [property: JsonPropertyName("documentId")] Guid DocumentId,
    [property: JsonPropertyName("documentType")] DocumentTypeId? DocumentTypeId
);

public record ApplicationChecklistEntryDetails(
    ApplicationChecklistEntryTypeId TypeId,
    ApplicationChecklistEntryStatusId StatusId
);
