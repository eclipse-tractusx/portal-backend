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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

/// <inheritdoc />
public class ServiceProviderBusinessLogic : IServiceProviderBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    /// Creates a new instance of <see cref="ServiceProviderBusinessLogic"/>
    /// </summary>
    /// <param name="portalRepositories">Access to the portal repositories</param>
    public ServiceProviderBusinessLogic(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    /// <inheritdoc />
    public async Task<ProviderDetailReturnData> GetServiceProviderCompanyDetailsAsync(string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<ICompanyRepository>()
            .GetProviderCompanyDetailAsync( CompanyRoleId.SERVICE_PROVIDER, iamUserId)
            .ConfigureAwait(false);
        if (result == default)
        {
            throw new ConflictException($"IAmUser {iamUserId} is not assigned to company");
        }
        if (!result.IsProviderCompany)
        {
            throw new ForbiddenException($"users {iamUserId} company is not a service-provider");
        }

        return result.ProviderDetailReturnData;
    }

    /// <inheritdoc />
    public Task SetServiceProviderCompanyDetailsAsync(ServiceProviderDetailData data, string iamUserId)
    {
        if (string.IsNullOrWhiteSpace(data.Url) || !data.Url.StartsWith("https://") || data.Url.Length > 100)
        {
            throw new ControllerArgumentException(
                "Url must start with https and the maximum allowed length is 100 characters", nameof(data.Url));
        }

        return SetServiceProviderCompanyDetailsInternalAsync(data, iamUserId);
    }

    private async Task SetServiceProviderCompanyDetailsInternalAsync(ServiceProviderDetailData data, string iamUserId)
    {
        var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();
        var serviceProviderDetailData = await companyRepository
            .GetProviderCompanyDetailsExistsForUser(iamUserId)
            .ConfigureAwait(false);
        if (serviceProviderDetailData == default)
        {
            var result = await companyRepository
                .GetCompanyIdMatchingRoleAndIamUserOrTechnicalUserAsync(iamUserId, CompanyRoleId.SERVICE_PROVIDER)
                .ConfigureAwait(false);
            if (result == default)
            {
                throw new ConflictException($"IAmUser {iamUserId} is not assigned to company");
            }
            if (!result.IsServiceProviderCompany)
            {
                throw new ForbiddenException($"users {iamUserId} company is not a service-provider");
            }
            companyRepository.CreateProviderCompanyDetail(result.CompanyId, data.Url);
        }
        else
        {
            companyRepository.AttachAndModifyProviderCompanyDetails(
                serviceProviderDetailData.ProviderCompanyDetailId,
                details => { details.AutoSetupUrl = serviceProviderDetailData.Url; },
                details => { details.AutoSetupUrl = data.Url; });
        }
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
