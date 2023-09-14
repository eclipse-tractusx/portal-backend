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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class RegistrationStatusBusinessLogic : IRegistrationStatusBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityService _identityService;

    public RegistrationStatusBusinessLogic(IPortalRepositories portalRepositories, IIdentityService identityService)
    {
        _portalRepositories = portalRepositories;
        _identityService = identityService;
    }

    public Task<OnboardingServiceProviderCallbackResponseData> GetCallbackAddress() =>
        _portalRepositories.GetInstance<ICompanyRepository>().GetCallbackData(_identityService.IdentityData.CompanyId);

    public async Task SetCallbackAddress(OnboardingServiceProviderCallbackData data)
    {
        var companyId = _identityService.IdentityData.CompanyId;
        var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();
        var (hasCompanyRole, ospDetailsExist, callbackUrl) = await companyRepository
            .GetCallbackEditData(companyId, CompanyRoleId.ONBOARDING_SERVICE_PROVIDER)
            .ConfigureAwait(false);

        if (!hasCompanyRole)
        {
            throw new ForbiddenException($"Only {CompanyRoleId.ONBOARDING_SERVICE_PROVIDER} are allowed to set the callback url");
        }

        if (ospDetailsExist)
        {
            companyRepository.AttachAndModifyOnboardingServiceProvider(companyId, osp =>
                {
                    osp.CallbackUrl = callbackUrl ?? throw new UnexpectedConditionException("callbackUrl should never be null here");
                },
                osp =>
                {
                    osp.CallbackUrl = data.CallbackUrl;
                });
        }
        else
        {
            companyRepository.CreateOnboardingServiceProviderDetails(companyId, data.CallbackUrl);
        }

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
