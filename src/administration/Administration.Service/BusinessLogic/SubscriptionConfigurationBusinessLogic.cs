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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="ISubscriptionConfigurationBusinessLogic"/>.
/// </summary>
public class SubscriptionConfigurationBusinessLogic(
    IOfferSubscriptionProcessService offerSubscriptionProcessService,
    IPortalRepositories portalRepositories,
    IIdentityService identityService,
    IOptions<SubscriptionConfigurationSettings> options) : ISubscriptionConfigurationBusinessLogic
{
    private readonly SubscriptionConfigurationSettings _settings = options.Value;
    private readonly IIdentityData _identityData = identityService.IdentityData;

    /// <inheritdoc />
    public async Task<ProviderDetailReturnData> GetProviderCompanyDetailsAsync()
    {
        var companyId = _identityData.CompanyId;
        var result = await portalRepositories.GetInstance<ICompanyRepository>()
            .GetProviderCompanyDetailAsync([CompanyRoleId.SERVICE_PROVIDER, CompanyRoleId.APP_PROVIDER], companyId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw ConflictException.Create(AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_CONFLICT_COMPANY_NOT_FOUND, [new(nameof(companyId), companyId.ToString())]);
        }

        if (!result.IsProviderCompany)
        {
            throw ForbiddenException.Create(AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_FORBIDDEN_COMPANY_NOT_PROVIDER, [new(nameof(companyId), companyId.ToString())]);
        }

        return result.ProviderDetailReturnData;
    }

    /// <inheritdoc />
    public Task SetProviderCompanyDetailsAsync(ProviderDetailData data)
    {
        data.Url.EnsureValidHttpsUrl(() => nameof(data.Url));
        data.AuthUrl.EnsureValidHttpsUrl(() => nameof(data.AuthUrl));
        data.CallbackUrl?.EnsureValidHttpsUrl(() => nameof(data.CallbackUrl));

        if (data.Url is { Length: > 100 })
        {
            throw ControllerArgumentException.Create(AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_ARGUMENT_MAX_LENGTH_ALLOW_HUNDRED_CHAR, [new ErrorParameter("Url", nameof(data.Url))]);
        }

        if (string.IsNullOrWhiteSpace(data.ClientId))
        {
            throw ControllerArgumentException.Create(AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_CONFLICT_CLIENT_MUST_SET);
        }

        if (string.IsNullOrWhiteSpace(data.ClientSecret))
        {
            throw ControllerArgumentException.Create(AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_CONFLICT_SECRET_MUST_SET);
        }

        return SetOfferProviderCompanyDetailsInternalAsync(data, _identityData.CompanyId);
    }

    private async Task SetOfferProviderCompanyDetailsInternalAsync(ProviderDetailData data, Guid companyId)
    {
        var companyRepository = portalRepositories.GetInstance<ICompanyRepository>();
        var providerDetailData = await companyRepository
            .GetProviderCompanyDetailsExistsForUser(companyId)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(_settings.EncryptionConfigIndex);
        var (secret, initializationVector) = cryptoHelper.Encrypt(data.ClientSecret);

        if (providerDetailData.ProviderDetails == default && data.Url != null)
        {
            await HandleCreateProviderCompanyDetails(data, companyId, companyRepository, secret, initializationVector, _settings.EncryptionConfigIndex);
        }
        else if (providerDetailData.ProviderDetails != default && data.Url != null)
        {
            companyRepository.AttachAndModifyProviderCompanyDetails(
                providerDetailData.ProviderCompanyDetailId,
                details =>
                {
                    details.AutoSetupUrl = providerDetailData.ProviderDetails.Url;
                    details.AutoSetupCallbackUrl = providerDetailData.ProviderDetails.CallbackUrl;
                    details.AuthUrl = providerDetailData.ProviderDetails.AuthUrl;
                    details.ClientId = providerDetailData.ProviderDetails.ClientId;
                    details.ClientSecret = providerDetailData.ProviderDetails.ClientSecret;
                    details.InitializationVector = providerDetailData.ProviderDetails.InitializationVector;
                    details.EncryptionMode = providerDetailData.ProviderDetails.EncryptionMode;
                },
                details =>
                {
                    details.AutoSetupCallbackUrl = data.CallbackUrl;
                    details.AutoSetupUrl = data.Url;
                    details.AutoSetupCallbackUrl = data.CallbackUrl;
                    details.AuthUrl = data.AuthUrl;
                    details.ClientId = data.ClientId;
                    details.ClientSecret = secret;
                    details.InitializationVector = initializationVector;
                    details.EncryptionMode = _settings.EncryptionConfigIndex;
                    details.DateLastChanged = DateTimeOffset.UtcNow;
                });

        }
        if (providerDetailData.ProviderDetails?.CallbackUrl is not null && data.CallbackUrl is null)
        {
            await HandleOfferSetupProcesses(companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        }
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task DeleteOfferProviderCompanyDetailsAsync()
    {
        var companyRepository = portalRepositories.GetInstance<ICompanyRepository>();
        var companyId = _identityData.CompanyId;
        (var providerCompanyDetailId, _) = await companyRepository
            .GetProviderCompanyDetailsExistsForUser(companyId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (providerCompanyDetailId == Guid.Empty)
        {
            throw ConflictException.Create(AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_CONFLICT_AUTO_SETUP_NOT_FOUND, [new(nameof(companyId), companyId.ToString())]);
        }

        companyRepository.RemoveProviderCompanyDetails(providerCompanyDetailId);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }
    private async Task HandleOfferSetupProcesses(Guid companyId)
    {
        var processData = await portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetOfferSubscriptionRetriggerProcessesForCompanyId(companyId)
            .PreSortedGroupBy(x => x.Process, x => x.ProcessStep, (left, right) => left.Id == right.Id)
            .Select(group => new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(group.Key, group))
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (var context in processData.Select(data =>
                     data.CreateManualProcessData(ProcessStepTypeId.RETRIGGER_PROVIDER, portalRepositories, () => $"processId {data.Process!.Id}")))
        {
            context.FinalizeProcessStep();
            context.ScheduleProcessSteps(Enumerable.Repeat(ProcessStepTypeId.AWAIT_START_AUTOSETUP, 1));
        }
    }

    private static async Task HandleCreateProviderCompanyDetails(ProviderDetailData data, Guid companyId, ICompanyRepository companyRepository, byte[] secret, byte[]? initializationVector, int index)
    {
        var (isValidCompanyId, isCompanyRoleOwner) = await companyRepository
            .IsValidCompanyRoleOwner(companyId, [CompanyRoleId.APP_PROVIDER, CompanyRoleId.SERVICE_PROVIDER])
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (!isValidCompanyId)
        {
            throw ControllerArgumentException.Create(AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_ARGUMENT_MAX_LENGTH_ALLOW_HUNDRED_CHAR, [new ErrorParameter("Url", nameof(data.Url))]);
        }

        if (!isCompanyRoleOwner)
        {
            throw ForbiddenException.Create(AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_FORBIDDEN_COMPANY_NOT_PROVIDER, [new ErrorParameter(nameof(companyId), companyId.ToString())]);
        }

        companyRepository.CreateProviderCompanyDetail(companyId, new ProviderDetailsCreationData(data.Url!, data.AuthUrl, data.ClientId, secret, index), providerDetails =>
        {
            if (data.CallbackUrl != null)
            {
                providerDetails.AutoSetupCallbackUrl = data.CallbackUrl;
            }
            providerDetails.InitializationVector = initializationVector;
            providerDetails.DateLastChanged = DateTimeOffset.UtcNow;
        });
    }

    /// <inheritdoc />
    public Task RetriggerProvider(Guid offerSubscriptionId) =>
        TriggerProcessStep(offerSubscriptionId, ProcessStepTypeId.RETRIGGER_PROVIDER, true);

    /// <inheritdoc />
    public Task RetriggerCreateClient(Guid offerSubscriptionId) =>
        TriggerProcessStep(offerSubscriptionId, ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION, true);

    /// <inheritdoc />
    public Task RetriggerCreateTechnicalUser(Guid offerSubscriptionId) =>
        TriggerProcessStep(offerSubscriptionId, ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION, true);

    /// <inheritdoc />
    public Task RetriggerProviderCallback(Guid offerSubscriptionId) =>
        TriggerProcessStep(offerSubscriptionId, ProcessStepTypeId.RETRIGGER_PROVIDER_CALLBACK, false);

    /// <inheritdoc />
    public Task RetriggerCreateDimTechnicalUser(Guid offerSubscriptionId) =>
        TriggerProcessStep(offerSubscriptionId, ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CREATE_DIM_TECHNICAL_USER, true);

    private async Task TriggerProcessStep(Guid offerSubscriptionId, ProcessStepTypeId stepToTrigger, bool mustBePending)
    {
        var nextStep = stepToTrigger.GetOfferSubscriptionStepToRetrigger();
        var context = await offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(offerSubscriptionId, stepToTrigger, null, mustBePending)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        offerSubscriptionProcessService.FinalizeProcessSteps(context, Enumerable.Repeat(nextStep, 1));
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<ProcessStepData> GetProcessStepsForSubscription(Guid offerSubscriptionId) =>
        portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetProcessStepsForSubscription(offerSubscriptionId);
}
