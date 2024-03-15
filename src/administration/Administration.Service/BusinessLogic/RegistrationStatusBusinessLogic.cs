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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class RegistrationStatusBusinessLogic : IRegistrationStatusBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityData _identityData;
    private readonly OnboardingServiceProviderSettings _settings;

    public RegistrationStatusBusinessLogic(IPortalRepositories portalRepositories, IIdentityService identityService, IOptions<OnboardingServiceProviderSettings> options)
    {
        _portalRepositories = portalRepositories;
        _identityData = identityService.IdentityData;
        _settings = options.Value;
    }

    public Task<OnboardingServiceProviderCallbackResponseData> GetCallbackAddress() =>
        _portalRepositories.GetInstance<ICompanyRepository>().GetCallbackData(_identityData.CompanyId);

    public async Task SetCallbackAddress(OnboardingServiceProviderCallbackRequestData requestData)
    {
        var companyId = _identityData.CompanyId;
        var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();
        var (hasCompanyRole, ospDetailId, ospDetails) = await companyRepository
            .GetCallbackEditData(companyId, CompanyRoleId.ONBOARDING_SERVICE_PROVIDER)
            .ConfigureAwait(false);

        if (!hasCompanyRole)
        {
            throw new ForbiddenException($"Only {CompanyRoleId.ONBOARDING_SERVICE_PROVIDER} are allowed to set the callback url");
        }

        var cryptoConfig = _settings.EncryptionConfigs.SingleOrDefault(x => x.Index == _settings.EncryptionConfigIndex) ?? throw new ConfigurationException($"EncryptionModeIndex {_settings.EncryptionConfigIndex} is not configured");
        var (secret, initializationVector) = CryptoHelper.Encrypt(requestData.ClientSecret, Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);

        if (ospDetailId.HasValue && ospDetails != null)
        {
            companyRepository.AttachAndModifyOnboardingServiceProvider(ospDetailId.Value, osp =>
                {
                    osp.CallbackUrl = ospDetails.CallbackUrl;
                    osp.AuthUrl = ospDetails.AuthUrl;
                    osp.ClientId = ospDetails.ClientId;
                    osp.ClientSecret = ospDetails.ClientSecret;
                    osp.EncryptionMode = ospDetails.EncryptionMode;
                    osp.InitializationVector = ospDetails.InitializationVector;
                },
                osp =>
                {
                    osp.CallbackUrl = requestData.CallbackUrl;
                    osp.AuthUrl = requestData.AuthUrl;
                    osp.ClientId = requestData.ClientId;
                    osp.ClientSecret = secret;
                    osp.EncryptionMode = cryptoConfig.Index;
                    osp.InitializationVector = initializationVector;
                });
        }
        else
        {
            companyRepository.CreateOnboardingServiceProviderDetails(companyId, requestData.CallbackUrl, requestData.AuthUrl, requestData.ClientId, secret, initializationVector, cryptoConfig.Index);
        }
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
