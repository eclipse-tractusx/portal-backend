/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library;

public class OnboardingServiceProviderBusinessLogic(
    IOnboardingServiceProviderService onboardingServiceProviderService,
    IPortalRepositories portalRepositories,
    IOptions<OnboardingServiceProviderSettings> options)
    : IOnboardingServiceProviderBusinessLogic
{
    private readonly OnboardingServiceProviderSettings _settings = options.Value;

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> TriggerProviderCallback(Guid networkRegistrationId, ProcessStepTypeId processStepTypeId, CancellationToken cancellationToken)
    {
        var result = await portalRepositories.GetInstance<INetworkRepository>().GetCallbackData(networkRegistrationId, processStepTypeId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (result == default)
        {
            throw new UnexpectedConditionException($"data should never be default here (networkRegistrationId: {networkRegistrationId})");
        }

        if (result.ospCallbackDetails == null || string.IsNullOrWhiteSpace(result.ospCallbackDetails.CallbackUrl))
        {
            return (Enumerable.Empty<ProcessStepTypeId>(), ProcessStepStatusId.SKIPPED, false, "No callback url set");
        }

        if (processStepTypeId == ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED && result.ospCallbackData.Comments.Count() != 1)
        {
            throw new UnexpectedConditionException("Message for decline should be set");
        }

        string comment;
        CompanyApplicationStatusId applicationStatusId;
        var applicationId = result.ospCallbackData.ApplicationId;
        switch (processStepTypeId)
        {
            case ProcessStepTypeId.TRIGGER_CALLBACK_OSP_CREATED:
                comment = $"Application {applicationId} has been created for further processing";
                applicationStatusId = CompanyApplicationStatusId.CREATED;
                break;
            case ProcessStepTypeId.TRIGGER_CALLBACK_OSP_INVITED:
                comment = $"Application {applicationId} has been invited for further processing";
                applicationStatusId = CompanyApplicationStatusId.INVITE_USER;
                break;
            case ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED:
                comment = $"Application {applicationId} has been submitted for further processing";
                applicationStatusId = CompanyApplicationStatusId.SUBMITTED;
                break;
            case ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED:
                comment = $"Application {applicationId} has been approved";
                applicationStatusId = CompanyApplicationStatusId.CONFIRMED;
                break;
            case ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED:
                comment = $"Application {applicationId} has been declined with reason: {result.ospCallbackData.Comments.Single()}";
                applicationStatusId = CompanyApplicationStatusId.DECLINED;
                break;
            default:
                throw new ArgumentException($"{processStepTypeId} is not supported");
        }

        var cryptoConfig = _settings.EncryptionConfigs.SingleOrDefault(x => x.Index == result.ospCallbackDetails.EncryptionMode) ?? throw new ConfigurationException($"EncryptionModeIndex {result.ospCallbackDetails.EncryptionMode} is not configured");
        var secret = CryptoHelper.Decrypt(result.ospCallbackDetails.ClientSecret, result.ospCallbackDetails.InitializationVector, Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);

        await onboardingServiceProviderService.TriggerProviderCallback(
                new OspTriggerDetails(
                    result.ospCallbackDetails.CallbackUrl,
                    result.ospCallbackDetails.AuthUrl,
                    result.ospCallbackDetails.ClientId, secret),
                new OnboardingServiceProviderCallbackData(
                    result.ospCallbackData.ExternalId,
                    result.ospCallbackData.ApplicationId,
                    result.ospCallbackData.CompanyId,
                    result.ospCallbackData.BusinessPartnerNumber,
                    applicationStatusId,
                    comment,
                    result.ospCallbackData.ApplicationDateCreated,
                    result.ospCallbackData.DateCreated,
                    result.ospCallbackData.ApplicationDateLastChanged,
                    result.ospCallbackData.CompanyName,
                    result.ospCallbackData.CompanyAssignedRoles),
                cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        var nextStep = processStepTypeId == ProcessStepTypeId.TRIGGER_CALLBACK_OSP_CREATED ? [ProcessStepTypeId.SYNCHRONIZE_USER] : Enumerable.Empty<ProcessStepTypeId>();
        return (nextStep, ProcessStepStatusId.DONE, false, null);
    }
}
