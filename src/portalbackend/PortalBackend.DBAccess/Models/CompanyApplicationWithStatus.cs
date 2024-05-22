/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models
{
    public record CompanyApplicationWithStatus(
        Guid ApplicationId,
        CompanyApplicationStatusId ApplicationStatus,
        CompanyApplicationTypeId ApplicationType,
        IEnumerable<ApplicationChecklistData> ApplicationChecklist
    );

    public record ApplicationChecklistData(
        ApplicationChecklistEntryTypeId TypeId,
        ApplicationChecklistEntryStatusId StatusId
    );

    public record CompanyApplicationDeclineData(
        Guid ApplicationId,
        CompanyApplicationStatusId ApplicationStatus,
        string User,
        string CompanyName,
        IEnumerable<string> Users
    );

    public record ApplicationDeclineData(
        IEnumerable<IdentityProviderStatusData> IdentityProviderStatusDatas,
        string CompanyName,
        CompanyApplicationStatusId CompanyApplicationStatusId,
        IEnumerable<InvitationsStatusData> InvitationsStatusDatas,
        IEnumerable<CompanyUserStatusData> CompanyUserStatusDatas,
        IEnumerable<DocumentStatusData> DocumentStatusDatas
    );

    public record IdentityProviderStatusData(
        Guid IdentityProviderId,
        IdentityProviderTypeId IdentityProviderTypeId
    );

    public record InvitationsStatusData(
        Guid InvitationId,
        InvitationStatusId InvitationStatusId
    );

    public record CompanyUserStatusData(
        Guid CompanyUserId,
        string? FirstName,
        string? LastName,
        string? Email,
        UserStatusId UserStatusId,
        IEnumerable<Guid> IdentityAssignedRoleIds
    );

    public record IdpData(
        Guid IdentityProviderId,
        string IamAlias,
        IdentityProviderTypeId IdentityProviderTypeId
    );
}
