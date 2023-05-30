/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public interface IServiceAccountRepository
{
    CompanyServiceAccount CreateCompanyServiceAccount(Guid companyId,
        CompanyServiceAccountStatusId companyServiceAccountStatusId,
        string name,
        string description,
        CompanyServiceAccountTypeId companyServiceAccountTypeId,
        Action<CompanyServiceAccount>? setOptionalParameters = null);

    void AttachAndModifyCompanyServiceAccount(Guid id, Action<CompanyServiceAccount>? initialize, Action<CompanyServiceAccount> modify);
    IamServiceAccount CreateIamServiceAccount(string clientId, string clientClientId, string userEntityId, Guid companyServiceAccountId);
    void RemoveIamServiceAccount(string clientId);
    void CreateCompanyServiceAccountAssignedRoles(IEnumerable<(Guid CompanyServiceAccountId, Guid UserRoleId)> companyServiceAccountAssignedRoleIds);
    void RemoveCompanyServiceAccountAssignedRoles(IEnumerable<(Guid CompanyServiceAccountId, Guid UserRoleId)> companyServiceAccountAssignedRoleIds);
    Task<CompanyServiceAccountWithRoleDataClientId?> GetOwnCompanyServiceAccountWithIamClientIdAsync(Guid serviceAccountId, string adminUserId);
    Task<(IEnumerable<Guid> UserRoleIds, Guid? ConnectorId, string? ClientId)> GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(Guid serviceAccountId, string adminUserId);
    Task<CompanyServiceAccountDetailedData?> GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(Guid serviceAccountId, string iamAdminId);
    Func<int,int,Task<Pagination.Source<CompanyServiceAccountData>?>> GetOwnCompanyServiceAccountsUntracked(string adminUserId);
    Task<bool> CheckActiveServiceAccountExistsForCompanyAsync(Guid technicalUserId, Guid companyId);
}
