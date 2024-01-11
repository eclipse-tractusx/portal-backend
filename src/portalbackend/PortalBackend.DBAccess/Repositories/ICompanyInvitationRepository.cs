/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public interface ICompanyInvitationRepository
{
    CompanyInvitation CreateCompanyInvitation(string firstName, string lastName, string email, string organisationName, Guid processId, Action<CompanyInvitation>? setOptionalFields);
    Task<Guid> GetCompanyInvitationForProcessId(Guid processId);
    Task<string?> GetOrganisationNameForInvitation(Guid invitationId);
    Task<(bool Exists, Guid? ApplicationId, Guid? CompanyId, string CompanyName, IEnumerable<(Guid IdpId, string IdpName)> IdpInformation, UserInvitationInformation UserInformation)> GetInvitationUserData(Guid companyInvitationId);
    Task<(bool Exists, string OrgName, string? IdpName)> GetInvitationIdpCreationData(Guid invitationId);
    void AttachAndModifyCompanyInvitation(Guid invitationId, Action<CompanyInvitation>? initialize, Action<CompanyInvitation> modify);
    Task<string?> GetIdpNameForInvitationId(Guid invitationId);
    Task<(string orgName, string? idpName, string? clientId, byte[]? clientSecret)> GetUpdateCentralIdpUrlData(Guid invitationId);
    Task<(string orgName, string? idpName)> GetIdpAndOrgNameAsync(Guid invitationId);
    Task<string?> GetServiceAccountUserIdForInvitation(Guid invitationId);
}
