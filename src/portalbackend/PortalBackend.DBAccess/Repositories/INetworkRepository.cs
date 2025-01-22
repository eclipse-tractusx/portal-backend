/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public interface INetworkRepository
{
    NetworkRegistration CreateNetworkRegistration(string externalId, Guid companyId, Guid processId, Guid ospId, Guid applicationId);
    Task<bool> CheckExternalIdExists(string externalId, Guid onboardingServiceProviderId);
    Task<Guid> GetNetworkRegistrationDataForProcessIdAsync(Guid processId);
    Task<(bool RegistrationIdExists, VerifyProcessData<ProcessTypeId, ProcessStepTypeId> processData)> IsValidRegistration(string externalId, IEnumerable<ProcessStepTypeId> processStepTypeIds);
    Task<(bool Exists, IEnumerable<(Guid CompanyApplicationId, CompanyApplicationStatusId CompanyApplicationStatusId, string? CallbackUrl)> CompanyApplications, IEnumerable<(CompanyRoleId CompanyRoleId, IEnumerable<Guid> AgreementIds)> CompanyRoleAgreementIds, Guid? ProcessId)> GetSubmitData(Guid companyId);
    Task<(OspDetails? OspDetails, string ExternalId, string? Bpn, Guid ApplicationId, IEnumerable<string> Comments)> GetCallbackData(Guid networkRegistrationId, ProcessStepTypeId processStepTypeId);
    Task<string?> GetOspCompanyName(Guid networkRegistrationId);
    Task<(
        bool Exists,
        bool IsValidTypeId,
        bool IsValidStatusId,
        bool IsValidCompany,
        (
        (CompanyStatusId CompanyStatusId, IEnumerable<(Guid IdentityId, UserStatusId UserStatus)> Identities) CompanyData,
        IEnumerable<(Guid InvitationId, InvitationStatusId StatusId)> InvitationData,
        VerifyProcessData<ProcessTypeId, ProcessStepTypeId> ProcessData
        )? Data)> GetDeclineDataForApplicationId(Guid applicationId, CompanyApplicationTypeId validTypeId, IEnumerable<CompanyApplicationStatusId> validStatusIds, Guid companyId);
}
