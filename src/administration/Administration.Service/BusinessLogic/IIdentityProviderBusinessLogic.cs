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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public interface IIdentityProviderBusinessLogic
{
    IAsyncEnumerable<IdentityProviderDetails> GetOwnCompanyIdentityProvidersAsync();
    ValueTask<IdentityProviderDetails> CreateOwnCompanyIdentityProviderAsync(IamIdentityProviderProtocol protocol, IdentityProviderTypeId typeId, string? displayName);
    ValueTask<IdentityProviderDetails> GetOwnCompanyIdentityProviderAsync(Guid identityProviderId);
    ValueTask<IdentityProviderDetails> SetOwnCompanyIdentityProviderStatusAsync(Guid identityProviderId, bool enabled);
    ValueTask<IdentityProviderDetails> UpdateOwnCompanyIdentityProviderAsync(Guid identityProviderId, IdentityProviderEditableDetails details, CancellationToken cancellationToken);
    ValueTask DeleteCompanyIdentityProviderAsync(Guid identityProviderId);
    IAsyncEnumerable<UserIdentityProviderData> GetOwnCompanyUsersIdentityProviderDataAsync(IEnumerable<Guid> identityProviderIds, bool unlinkedUsersOnly);
    (Stream FileStream, string ContentType, string FileName, Encoding Encoding) GetOwnCompanyUsersIdentityProviderLinkDataStream(IEnumerable<Guid> identityProviderIds, bool unlinkedUsersOnly);
    ValueTask<IdentityProviderUpdateStats> UploadOwnCompanyUsersIdentityProviderLinkDataAsync(IFormFile document, CancellationToken cancellationToken);
    ValueTask<UserIdentityProviderLinkData> CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId, UserLinkData userLinkData);
    ValueTask<UserIdentityProviderLinkData> GetOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId);
    ValueTask DeleteOwnCompanyUserIdentityProviderDataAsync(Guid companyUserId, Guid identityProviderId);
    ValueTask<IdentityProviderDetailsWithConnectedCompanies> GetOwnIdentityProviderWithConnectedCompanies(Guid identityProviderId);
}
