/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public interface IRegistrationBusinessLogic
{
    Task<CompanyWithAddressData> GetCompanyWithAddressAsync(Guid applicationId);
    Task<Pagination.Response<CompanyApplicationDetails>> GetCompanyApplicationDetailsAsync(int page, int size, CompanyApplicationStatusFilter? companyApplicationStatusFilter = null, string? companyName = null);
    Task<bool> ApprovePartnerRequest(string iamUserId, string accessToken, Guid applicationId, CancellationToken cancellationToken);
    Task<bool> DeclinePartnerRequest(Guid applicationId);
    Task<Pagination.Response<CompanyApplicationWithCompanyUserDetails>> GetAllCompanyApplicationsDetailsAsync(int page, int size, string? companyName = null);
    Task TriggerBpnDataPushAsync(string iamUserId, Guid applicationId, CancellationToken cancellationToken);
    Task UpdateCompanyBpn(Guid applicationId, string bpn);
    
    /// <summary>
    /// Sets the registration verification state for the given application.
    /// </summary>
    /// <param name="applicationId">Id of the application</param>
    /// <param name="approve"><c>true</c> if the application is approved, otherwise <c>false</c></param>
    /// <param name="comment">An additional comment, only set if the application got declined</param>
    Task SetRegistrationVerification(Guid applicationId, bool approve, string? comment = null);
}
