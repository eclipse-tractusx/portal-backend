/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;

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
    public async Task<ProviderDetailReturnData> GetServiceProviderCompanyDetailsAsync(Guid serviceProviderDetailDataId, string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<ICompanyRepository>()
            .GetProviderCompanyDetailAsync(serviceProviderDetailDataId, CompanyRoleId.SERVICE_PROVIDER, iamUserId)
            .ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"serviceProviderDetail {serviceProviderDetailDataId} does not exist");
        }
        if (!result.IsCompanyUser)
        {
            throw new ForbiddenException($"User {iamUserId} is not allowed to request the service provider detail data.");
        }
        if (!result.IsProviderCompany)
        {
            throw new ForbiddenException($"users {iamUserId} company is not a service-provider");
        }

        return result.ProviderDetailReturnData;
    }

    /// <inheritdoc />
    public Task<Guid> CreateServiceProviderCompanyDetailsAsync(ServiceProviderDetailData data, string iamUserId)
    {
        ValidateServiceProviderDetailData(data);
        return CreateServiceProviderCompanyDetailsInternalAsync(data, iamUserId);
    }

    private async Task<Guid> CreateServiceProviderCompanyDetailsInternalAsync(ServiceProviderDetailData data, string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<ICompanyRepository>().GetCompanyIdMatchingRoleAndIamUser(iamUserId, CompanyRoleId.SERVICE_PROVIDER).ConfigureAwait(false);
        if (result == default)
        {
            throw new ConflictException($"IAmUser {iamUserId} is not assigned to company");
        }
        if (!result.IsServiceProviderCompany)
        {
            throw new ForbiddenException($"users {iamUserId} company is not a service-provider");
        }

        var companyDetails = _portalRepositories.GetInstance<ICompanyRepository>()
            .CreateProviderCompanyDetail(result.CompanyId, data.Url);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return companyDetails.Id;
    }

    /// <inheritdoc />
    public Task UpdateServiceProviderCompanyDetailsAsync(Guid serviceProviderDetailDataId, ServiceProviderDetailData data, string iamUserId)
    {
        ValidateServiceProviderDetailData(data);
        return UpdateServiceProviderCompanyDetailsInternalAsync(serviceProviderDetailDataId, data, iamUserId);
    }

    private static void ValidateServiceProviderDetailData(ServiceProviderDetailData data)
    {
        if (string.IsNullOrWhiteSpace(data.Url) || !data.Url.StartsWith("https://") || data.Url.Length > 100)
        {
            throw new ControllerArgumentException("Url must start with https and the maximum allowed length is 100 characters", nameof(data.Url));
        }
    }
    
    private async Task UpdateServiceProviderCompanyDetailsInternalAsync(Guid serviceProviderDetailDataId,
        ServiceProviderDetailData data, string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<ICompanyRepository>()
            .CheckProviderCompanyDetailsExistsForUser(iamUserId, serviceProviderDetailDataId)
            .ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"ServiceProviderDetailData {serviceProviderDetailDataId} does not exists.");
        }
        if (!result.IsSameCompany)
        {
            throw new ForbiddenException($"users {iamUserId} is not associated with service-provider company");
        }

        _portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyProviderCompanyDetails(
            serviceProviderDetailDataId,
            details => { details.AutoSetupUrl = data.Url; });
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
