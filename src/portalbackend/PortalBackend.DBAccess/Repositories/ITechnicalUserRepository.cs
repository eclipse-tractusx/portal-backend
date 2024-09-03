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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public interface ITechnicalUserRepository
{
    TechnicalUser CreateTechnicalUser(Guid identityId,
        string name,
        string description,
        string? clientClientId,
        TechnicalUserTypeId technicalUserTypeId,
        TechnicalUserKindId technicalUserKindId,
        Action<TechnicalUser>? setOptionalParameters = null);

    void AttachAndModifyTechnicalUser(Guid id, Guid version, Action<TechnicalUser>? initialize, Action<TechnicalUser> modify);
    Task<TechnicalUserWithRoleDataClientId?> GetTechnicalUserWithRoleDataClientIdAsync(Guid technicalUserId, Guid userCompanyId);
    Task<OwnTechnicalUserData?> GetOwnTechnicalUserWithIamUserRolesAsync(Guid technicalUserId, Guid companyId, IEnumerable<ProcessStepTypeId> processStepsToFilter);
    Task<TechnicalUserDetailedData?> GetOwnTechnicalUserDataUntrackedAsync(Guid technicalUserId, Guid companyId);
    Func<int, int, Task<Pagination.Source<CompanyServiceAccountData>?>> GetOwnTechnicalUsersUntracked(Guid userCompanyId, string? clientId, bool? isOwner, IEnumerable<UserStatusId> userStatusIds);
    Task<bool> CheckActiveServiceAccountExistsForCompanyAsync(Guid technicalUserId, Guid companyId);
    public Task<(Guid IdentityId, Guid CompanyId)> GetTechnicalUserDataByClientId(string clientId);
    void CreateExternalTechnicalUser(Guid technicalUserId, string authenticationServiceUrl, byte[] secret, byte[] initializationVector, int encryptionMode);
    void CreateExternalTechnicalUserCreationData(Guid technicalUserId, Guid processId);
    Task<(bool IsValid, string? Bpn, string Name)> GetExternalTechnicalUserData(Guid externalTechnicalUserId);
    Task<Guid> GetExternalTechnicalUserIdForProcess(Guid processId);
    public Task<(ProcessTypeId ProcessTypeId, VerifyProcessData<ProcessTypeId, ProcessStepTypeId> ProcessData, Guid? TechnicalUserId, Guid? TechnicalUserVersion)> GetProcessDataForTechnicalUserCallback(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds);
    public Task<(ProcessTypeId ProcessTypeId, VerifyProcessData<ProcessTypeId, ProcessStepTypeId> ProcessData, Guid? TechnicalUserId)> GetProcessDataForTechnicalUserDeletionCallback(Guid processId, IEnumerable<ProcessStepTypeId>? processStepTypeIds);
}
