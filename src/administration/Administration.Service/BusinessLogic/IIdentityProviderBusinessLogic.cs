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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public interface IIdentityProviderBusinessLogic
{
    IAsyncEnumerable<IdentityProviderDetails> GetOwnCompanyIdentityProvidersAsync(Guid companyId);
    ValueTask<IdentityProviderDetails> CreateOwnCompanyIdentityProviderAsync(IamIdentityProviderProtocol protocol, string? displayName, Guid companyId);
    ValueTask<IdentityProviderDetails> GetOwnCompanyIdentityProviderAsync(Guid identityProviderId, Guid companyId);
    ValueTask<IdentityProviderDetails> SetOwnCompanyIdentityProviderStatusAsync(Guid identityProviderId, bool enabled, Guid companyId);
    ValueTask<IdentityProviderDetails> UpdateOwnCompanyIdentityProviderAsync(Guid identityProviderId, IdentityProviderEditableDetails details, Guid companyId);
    ValueTask DeleteCompanyIdentityProviderAsync(Guid identityProviderId, Guid companyId);
    IAsyncEnumerable<UserIdentityProviderData> GetOwnCompanyUsersIdentityProviderDataAsync(IEnumerable<Guid> identityProviderIds, Guid companyId, bool unlinkedUsersOnly);
    (Stream FileStream, string ContentType, string FileName, Encoding Encoding) GetOwnCompanyUsersIdentityProviderLinkDataStream(IEnumerable<Guid> identityProviderIds, Guid companyId, bool unlinkedUsersOnly);
    ValueTask<IdentityProviderUpdateStats> UploadOwnCompanyUsersIdentityProviderLinkDataAsync(IFormFile document, (Guid UserId, Guid CompanyId) identity, CancellationToken cancellationToken);
    ValueTask<UserIdentityProviderLinkData> CreateOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, UserIdentityProviderLinkData identityProviderLinkData, Guid companyId);
    ValueTask<UserIdentityProviderLinkData> CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId, UserLinkData userLinkData, Guid companyId);
    ValueTask<UserIdentityProviderLinkData> GetOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId, Guid companyId);
    ValueTask DeleteOwnCompanyUserIdentityProviderDataAsync(Guid companyUserId, Guid identityProviderId, Guid companyId);
}
