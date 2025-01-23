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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
/// <summary>
/// Business Logic for Handling User Management Operation
/// </summary>
public interface IUserBusinessLogic
{
    IAsyncEnumerable<string> CreateOwnCompanyUsersAsync(IEnumerable<UserCreationInfo> userList);
    Task<Guid> CreateOwnCompanyIdpUserAsync(Guid identityProviderId, UserCreationInfoIdp userCreationInfo);
    Task<Pagination.Response<CompanyUserData>> GetOwnCompanyUserDatasAsync(int page, int size, GetOwnCompanyUsersFilter filter);
    Task<CompanyUserDetailData> GetOwnCompanyUserDetailsAsync(Guid userId);
    Task AddOwnCompanyUsersBusinessPartnerNumbersAsync(Guid userId, IEnumerable<string> businessPartnerNumbers, CancellationToken cancellationToken);
    Task AddOwnCompanyUsersBusinessPartnerNumberAsync(Guid userId, string businessPartnerNumber, CancellationToken cancellationToken);
    Task<CompanyOwnUserDetails> GetOwnUserDetails();
    Task<CompanyUserDetails> UpdateOwnUserDetails(Guid companyUserId, OwnCompanyUserEditableDetails ownCompanyUserEditableDetails);

    /// <summary>
    /// Delete User Own Account using userId
    /// </summary>
    /// <param name="companyUserId"></param>
    /// <returns></returns>
    Task<int> DeleteOwnUserAsync(Guid companyUserId);
    IAsyncEnumerable<Guid> DeleteOwnCompanyUsersAsync(IEnumerable<Guid> userIds);
    Task<bool> ExecuteOwnCompanyUserPasswordReset(Guid companyUserId);
    Task<Pagination.Response<CompanyAppUserDetails>> GetOwnCompanyAppUsersAsync(Guid appId, int page, int size, CompanyUserFilter filter);
    Task DeleteOwnUserBusinessPartnerNumbersAsync(Guid userId, string businessPartnerNumber);
    Task RetriggerUserBpnProcess(Guid processId, ProcessStepTypeId processStepTypeId);
}
