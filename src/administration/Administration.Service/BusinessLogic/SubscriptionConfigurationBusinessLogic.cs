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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class SubscriptionConfigurationBusinessLogic(
    IOfferSubscriptionProcessService offerSubscriptionProcessService,
    IPortalRepositories portalRepositories,
    IIdentityService identityService)
    : ISubscriptionConfigurationBusinessLogic
{
    private readonly IIdentityData _identityData = identityService.IdentityData;

    /// <inheritdoc />
    public async Task<ProviderDetailReturnData> GetProviderCompanyDetailsAsync()
    {
        var companyId = _identityData.CompanyId;
        var result = await portalRepositories.GetInstance<ICompanyRepository>()
            .GetProviderCompanyDetailAsync(CompanyRoleId.SERVICE_PROVIDER, companyId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw ConflictException.Create(AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_CONFLICT_COMPANY_NOT_FOUND, new ErrorParameter[] { new(nameof(companyId), companyId.ToString()) });
        }

        if (!result.IsProviderCompany)
        {
            throw ForbiddenException.Create(AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_FORBIDDEN_COMPANY_NOT_SERVICE_PROVIDER, new ErrorParameter[] { new(nameof(companyId), companyId.ToString()) });
        }

        return result.ProviderDetailReturnData;
    }

    /// <inheritdoc />
    public Task SetProviderCompanyDetailsAsync(ProviderDetailData data)
    {
        data.Url?.EnsureValidHttpsUrl(() => nameof(data.Url));
        data.CallbackUrl?.EnsureValidHttpsUrl(() => nameof(data.CallbackUrl));

        if (data.Url is { Length: > 100 })
        {
            throw ControllerArgumentException.Create(AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_ARGUMENT_MAX_LENGTH_ALLOW_HUNDRED_CHAR, new ErrorParameter[] { new("Url", nameof(data.Url)) });
        }

        return SetOfferProviderCompanyDetailsInternalAsync(data, _identityData.CompanyId);
    }

    private async Task SetOfferProviderCompanyDetailsInternalAsync(ProviderDetailData data, Guid companyId)
    {
        var companyRepository = portalRepositories.GetInstance<ICompanyRepository>();
        var providerDetailData = await companyRepository
            .GetProviderCompanyDetailsExistsForUser(companyId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        var hasChanges = false;
        if (providerDetailData == default && data.Url != null)
        {
            await HandleCreateProviderCompanyDetails(data, companyId, companyRepository);
            hasChanges = true;
        }
        else if (providerDetailData != default && data.Url != null)
        {
            companyRepository.AttachAndModifyProviderCompanyDetails(
                providerDetailData.ProviderCompanyDetailId,
                details =>
                {
                    details.AutoSetupUrl = providerDetailData.Url;
                    details.AutoSetupCallbackUrl = providerDetailData.CallbackUrl;
                },
                details =>
                {
                    details.AutoSetupCallbackUrl = data.CallbackUrl;
                    details.AutoSetupUrl = data.Url;
                    details.DateLastChanged = DateTimeOffset.UtcNow;
                });
            hasChanges = true;
        }
        else if (providerDetailData != default && data.Url == null)
        {
            companyRepository.RemoveProviderCompanyDetails(providerDetailData.ProviderCompanyDetailId);
            hasChanges = true;
        }

        if (providerDetailData.CallbackUrl is not null && data.CallbackUrl is null)
        {
            await HandleOfferSetupProcesses(companyId, companyRepository).ConfigureAwait(ConfigureAwaitOptions.None);
            hasChanges = true;
        }

        if (hasChanges)
        {
            await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private async Task HandleOfferSetupProcesses(Guid companyId, ICompanyRepository companyRepository)
    {
        var processData = await companyRepository
            .GetOfferSubscriptionProcessesForCompanyId(companyId)
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (var context in processData
                     .Where(x => x.Process != null && x.ProcessSteps?.Any(ps => ps is
                     {
                         ProcessStepStatusId: ProcessStepStatusId.TODO,
                         ProcessStepTypeId: ProcessStepTypeId.RETRIGGER_PROVIDER
                     }) == true)
                     .Select(data => data.CreateManualProcessData(ProcessStepTypeId.RETRIGGER_PROVIDER, portalRepositories, () => $"processId {data.Process!.Id}")))
        {
            context.FinalizeProcessStep();
            context.ScheduleProcessSteps(Enumerable.Repeat(ProcessStepTypeId.AWAIT_START_AUTOSETUP, 1));
        }
    }

    private static async Task HandleCreateProviderCompanyDetails(ProviderDetailData data, Guid companyId, ICompanyRepository companyRepository)
    {
        var result = await companyRepository
            .IsValidCompanyRoleOwner(companyId, new[] { CompanyRoleId.APP_PROVIDER, CompanyRoleId.SERVICE_PROVIDER })
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (!result.IsValidCompanyId)
        {
            throw ControllerArgumentException.Create(AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_ARGUMENT_MAX_LENGTH_ALLOW_HUNDRED_CHAR, new ErrorParameter[] { new("Url", nameof(data.Url)) });
        }

        if (!result.IsCompanyRoleOwner)
        {
            throw ForbiddenException.Create(AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_FORBIDDEN_COMPANY_NOT_SERVICE_PROVIDER, new ErrorParameter[] { new(nameof(companyId), companyId.ToString()) });
        }

        companyRepository.CreateProviderCompanyDetail(companyId, data.Url!, providerDetails =>
        {
            providerDetails.AutoSetupCallbackUrl = data.CallbackUrl;
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
