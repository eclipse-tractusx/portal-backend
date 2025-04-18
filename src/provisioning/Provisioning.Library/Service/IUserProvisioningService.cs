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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

public interface IUserProvisioningService
{
    IAsyncEnumerable<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)> CreateOwnCompanyIdpUsersAsync(CompanyNameIdpAliasData companyNameIdpAliasData, IAsyncEnumerable<UserCreationRoleDataIdpInfo> userCreationInfos, Action<UserCreationCallbackData>? onSuccessfulCreation = null, CancellationToken cancellationToken = default);
    Task HandleCentralKeycloakCreation(UserCreationRoleDataIdpInfo user, Guid companyUserId, string companyName, string? businessPartnerNumber, Identity? identity, IEnumerable<IdentityProviderLink> identityProviderLinks, IUserRepository userRepository, IUserRolesRepository userRolesRepository);
    Task<(CompanyNameIdpAliasData IdpAliasData, string NameCreatedBy)> GetCompanyNameIdpAliasData(Guid identityProviderId, Guid companyUserId);
    Task<(CompanyNameIdpAliasData IdpAliasData, string NameCreatedBy)> GetCompanyNameSharedIdpAliasData(Guid companyUserId, Guid? applicationId = null);
    IAsyncEnumerable<UserRoleData> GetRoleDatas(IEnumerable<UserRoleConfig> clientRoles);
    Task<IEnumerable<UserRoleData>> GetOwnCompanyPortalRoleDatas(string clientId, IEnumerable<string> roles, Guid companyId);
    Task<(Identity? Identity, Guid CompanyUserId)> GetOrCreateCompanyUser(IUserRepository userRepository, string alias, UserCreationRoleDataIdpInfo user, Guid companyId, Guid identityProviderId, string? businessPartnerNumber);
    Task AssignRolesToNewUserAsync(IUserRolesRepository userRolesRepository, IEnumerable<UserRoleData> roleDatas, (string IamUserId, Guid CompanyUserId) userdata);
}
