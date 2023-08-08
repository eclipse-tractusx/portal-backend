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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public interface IServiceAccountBusinessLogic
{
    Task<ServiceAccountDetails> CreateOwnCompanyServiceAccountAsync(ServiceAccountCreationInfo serviceAccountCreationInfos, Guid companyId);
    Task<int> DeleteOwnCompanyServiceAccountAsync(Guid serviceAccountId, Guid companyId);
    Task<ServiceAccountConnectorOfferData> GetOwnCompanyServiceAccountDetailsAsync(Guid serviceAccountId, Guid companyId);
    Task<ServiceAccountDetails> UpdateOwnCompanyServiceAccountDetailsAsync(Guid serviceAccountId, ServiceAccountEditableDetails serviceAccountDetails, Guid companyId);
    Task<ServiceAccountDetails> ResetOwnCompanyServiceAccountSecretAsync(Guid serviceAccountId, Guid companyId);
    Task<Pagination.Response<CompanyServiceAccountData>> GetOwnCompanyServiceAccountsDataAsync(int page, int size, Guid companyId, string? clientId, bool? isOwner);
    IAsyncEnumerable<UserRoleWithDescription> GetServiceAccountRolesAsync(Guid companyId, string? languageShortName);
}
