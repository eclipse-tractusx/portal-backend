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
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Security.Cryptography;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library;

public class OnboardingServiceProviderBusinessLogic : IOnboardingServiceProviderBusinessLogic
{
    private readonly IOnboardingServiceProviderService _onboardingServiceProviderService;
    private readonly IPortalRepositories _portalRepositories;
    private readonly OnboardingServiceProviderSettings _settings;

    public OnboardingServiceProviderBusinessLogic(IOnboardingServiceProviderService onboardingServiceProviderService, IPortalRepositories portalRepositories, IOptions<OnboardingServiceProviderSettings> options)
    {
        _onboardingServiceProviderService = onboardingServiceProviderService;
        _portalRepositories = portalRepositories;
        _settings = options.Value;
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> TriggerProviderCallback(Guid networkRegistrationId, ProcessStepTypeId processStepTypeId, CancellationToken cancellationToken)
    {
        var data = await _portalRepositories.GetInstance<INetworkRepository>().GetCallbackData(networkRegistrationId, processStepTypeId).ConfigureAwait(false);

        if (data.OspDetails == null || string.IsNullOrWhiteSpace(data.OspDetails.CallbackUrl))
        {
            return (Enumerable.Empty<ProcessStepTypeId>(), ProcessStepStatusId.SKIPPED, false, "No callback url set");
        }

        if (data.ExternalId == null)
        {
            throw new UnexpectedConditionException("No external registration found");
        }

        if (processStepTypeId == ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED && data.Comments.Count() != 1)
        {
            throw new UnexpectedConditionException("Message for decline should be set");
        }

        string comment;
        CompanyApplicationStatusId applicationStatusId;
        switch (processStepTypeId)
        {
            case ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED:
                comment = $"Application {data.ApplicationId} has been submitted for further processing";
                applicationStatusId = CompanyApplicationStatusId.SUBMITTED;
                break;
            case ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED:
                comment = $"Application {data.ApplicationId} has been approved";
                applicationStatusId = CompanyApplicationStatusId.CONFIRMED;
                break;
            case ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED:
                comment = $"Application {data.ApplicationId} has been declined with reason: {data.Comments.Single()}";
                applicationStatusId = CompanyApplicationStatusId.DECLINED;
                break;
            default:
                throw new ArgumentException($"{processStepTypeId} is not supported");
        }

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_settings.EncryptionKey);
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using (var msDecrypt = new MemoryStream(data.OspDetails.ClientSecret))
        {
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8);
            var secret = srDecrypt.ReadToEnd();
            await _onboardingServiceProviderService.TriggerProviderCallback(
                    new OspTriggerDetails(data.OspDetails.CallbackUrl, data.OspDetails.AuthUrl, data.OspDetails.ClientId, secret),
                    new OnboardingServiceProviderCallbackData(data.ExternalId!, data.ApplicationId, data.Bpn, applicationStatusId, comment),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        return (Enumerable.Empty<ProcessStepTypeId>(), ProcessStepStatusId.DONE, false, null);
    }
}
