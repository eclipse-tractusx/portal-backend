/********************************************************************************
 * Copyright (c) 2022 BMW Group AG
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

public record CompanyUserRoleWithAddress(
    Guid CompanyId,
    string Name,
    string? Shortname,
    string? BusinessPartnerNumber,
    string? City,
    string? StreetName,
    string? CountryAlpha2Code,
    string? Region,
    string? Streetadditional,
    string? Streetnumber,
    string? Zipcode,
    string? CountryDe,
    IEnumerable<AgreementsData> AgreementsData,
    IEnumerable<InvitedCompanyUserData> InvitedCompanyUserData,
    IEnumerable<(UniqueIdentifierId UniqueIdentifierId, string Value)> CompanyIdentifiers,
    IEnumerable<Document> Documents,
    DateTimeOffset? Created,
    DateTimeOffset? LastChanged
);

public record AgreementsData(CompanyRoleId CompanyRoleId, Guid AgreementId, ConsentStatusId? ConsentStatusId);

public record InvitedCompanyUserData(Guid UserId, string? FirstName, string? LastName, string? Email);
