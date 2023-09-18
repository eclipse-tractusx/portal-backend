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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using System.Security.Cryptography;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class RegistrationStatusBusinessLogic : IRegistrationStatusBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityService _identityService;
    private readonly OnboardingServiceProviderSettings _settings;

    public RegistrationStatusBusinessLogic(IPortalRepositories portalRepositories, IIdentityService identityService, IOptions<OnboardingServiceProviderSettings> options)
    {
        _portalRepositories = portalRepositories;
        _identityService = identityService;
        _settings = options.Value;
    }

    public Task<OnboardingServiceProviderCallbackResponseData> GetCallbackAddress() =>
        _portalRepositories.GetInstance<ICompanyRepository>().GetCallbackData(_identityService.IdentityData.CompanyId);

    public async Task SetCallbackAddress(OnboardingServiceProviderCallbackRequestData requestData)
    {
        var companyId = _identityService.IdentityData.CompanyId;
        var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();
        var (hasCompanyRole, ospDetails) = await companyRepository
            .GetCallbackEditData(companyId, CompanyRoleId.ONBOARDING_SERVICE_PROVIDER)
            .ConfigureAwait(false);

        if (!hasCompanyRole)
        {
            throw new ForbiddenException($"Only {CompanyRoleId.ONBOARDING_SERVICE_PROVIDER} are allowed to set the callback url");
        }

        var toEncryptedArray = Encoding.UTF8.GetBytes(requestData.ClientSecret);
        var md5 = MD5.Create();
        var securityKeyArray = md5.ComputeHash(Encoding.UTF8.GetBytes(_settings.EncryptionKey));
        md5.Clear();
        var tripleDes = TripleDES.Create();
        tripleDes.Key = securityKeyArray;
        tripleDes.Mode = CipherMode.ECB;
        tripleDes.Padding = PaddingMode.PKCS7;

        var encryptor = tripleDes.CreateEncryptor();
        var secretResult = encryptor.TransformFinalBlock(toEncryptedArray, 0, toEncryptedArray.Length);
        tripleDes.Clear();
        var secret = Convert.ToBase64String(secretResult, 0, secretResult.Length);

        if (ospDetails != null)
        {
            companyRepository.AttachAndModifyOnboardingServiceProvider(companyId, osp =>
                {
                    osp.CallbackUrl = ospDetails.CallbackUrl;
                    osp.AuthUrl = ospDetails.AuthUrl;
                    osp.ClientId = ospDetails.ClientId;
                    osp.ClientSecret = secret;
                },
                osp =>
                {
                    osp.CallbackUrl = requestData.CallbackUrl;
                    osp.AuthUrl = requestData.AuthUrl;
                    osp.ClientId = requestData.ClientId;
                    osp.ClientSecret = secret;
                });
        }
        else
        {
            companyRepository.CreateOnboardingServiceProviderDetails(companyId, requestData.CallbackUrl, requestData.AuthUrl, requestData.ClientId, secret);
        }

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
