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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public interface IServiceAccountRepository
{
    CompanyServiceAccount CreateCompanyServiceAccount(Guid identityId,
        string name,
        string description,
        string? clientClientId,
        CompanyServiceAccountTypeId companyServiceAccountTypeId,
        CompanyServiceAccountKindId companyServiceAccountKindId,
        Action<CompanyServiceAccount>? setOptionalParameters = null);

    void AttachAndModifyCompanyServiceAccount(Guid id, Guid version, Action<CompanyServiceAccount>? initialize, Action<CompanyServiceAccount> modify);
    Task<CompanyServiceAccountWithRoleDataClientId?> GetOwnCompanyServiceAccountWithIamClientIdAsync(Guid serviceAccountId, Guid userCompanyId);
    Task<OwnServiceAccountData?> GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(Guid serviceAccountId, Guid companyId, IEnumerable<ProcessStepTypeId> processStepsToFilter);
    Task<CompanyServiceAccountDetailedData?> GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(Guid serviceAccountId, Guid companyId);
    Func<int, int, Task<Pagination.Source<CompanyServiceAccountData>?>> GetOwnCompanyServiceAccountsUntracked(Guid userCompanyId, string? clientId, bool? isOwner, IEnumerable<UserStatusId> userStatusIds);
    Task<bool> CheckActiveServiceAccountExistsForCompanyAsync(Guid technicalUserId, Guid companyId);
    public Task<(Guid IdentityId, Guid CompanyId)> GetServiceAccountDataByClientId(string clientId);
    void CreateDimCompanyServiceAccount(Guid serviceAccountId, string authenticationServiceUrl, byte[] secret, byte[] initializationVector, int encryptionMode);
    void CreateDimUserCreationData(Guid serviceAccountId, Guid processId);
    Task<(bool IsValid, string? Bpn, string Name)> GetDimServiceAccountData(Guid dimServiceAccountId);
    Task<Guid> GetDimServiceAccountIdForProcess(Guid processId);
}
