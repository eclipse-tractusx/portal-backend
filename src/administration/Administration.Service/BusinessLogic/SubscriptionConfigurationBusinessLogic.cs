using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class SubscriptionConfigurationBusinessLogic : ISubscriptionConfigurationBusinessLogic
{
    private readonly IOfferSubscriptionProcessService _offerSubscriptionProcessService;
    private readonly IPortalRepositories _portalRepositories;

    public SubscriptionConfigurationBusinessLogic(IOfferSubscriptionProcessService offerSubscriptionProcessService, IPortalRepositories portalRepositories)
    {
        _offerSubscriptionProcessService = offerSubscriptionProcessService;
        _portalRepositories = portalRepositories;
    }
    
    /// <inheritdoc />
    public async Task<ProviderDetailReturnData> GetProviderCompanyDetailsAsync(string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<ICompanyRepository>()
            .GetProviderCompanyDetailAsync(CompanyRoleId.SERVICE_PROVIDER, iamUserId)
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
    public Task SetProviderCompanyDetailsAsync(ProviderDetailData data, string iamUserId)
    {
        data.Url.EnsureValidHttpUrl(() => nameof(data.Url));
        data.CallbackUrl?.EnsureValidHttpUrl(() => nameof(data.CallbackUrl));

        if (!data.Url.StartsWith("https://") || data.Url.Length > 100)
        {
            throw new ControllerArgumentException(
                "Url must start with https and the maximum allowed length is 100 characters", nameof(data.Url));
        }

        return SetOfferProviderCompanyDetailsInternalAsync(data, iamUserId);
    }

    private async Task SetOfferProviderCompanyDetailsInternalAsync(ProviderDetailData data, string iamUserId)
    {
        var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();
        var providerDetailData = await companyRepository
            .GetProviderCompanyDetailsExistsForUser(iamUserId)
            .ConfigureAwait(false);
        if (providerDetailData == default)
        {
            var result = await companyRepository
                .GetCompanyIdMatchingRoleAndIamUserOrTechnicalUserAsync(iamUserId, new []{CompanyRoleId.APP_PROVIDER, CompanyRoleId.SERVICE_PROVIDER})
                .ConfigureAwait(false);
            if (result == default)
            {
                throw new ConflictException($"IAmUser {iamUserId} is not assigned to company");
            }
            if (!result.IsServiceProviderCompany)
            {
                throw new ForbiddenException($"users {iamUserId} company is not a service-provider");
            }
            companyRepository.CreateProviderCompanyDetail(result.CompanyId, data.Url, providerDetails =>
            {
                if (data.CallbackUrl != null)
                {
                    providerDetails.AutoSetupCallbackUrl = data.CallbackUrl;
                }
            });
        }
        else
        {
            companyRepository.AttachAndModifyProviderCompanyDetails(
                providerDetailData.ProviderCompanyDetailId,
                details => { details.AutoSetupUrl = providerDetailData.Url; },
                details => { details.AutoSetupUrl = data.Url; });
        }
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    async Task ISubscriptionConfigurationBusinessLogic.TriggerProcessStep(Guid offerSubscriptionId, ProcessStepTypeId stepToTrigger, bool mustBePending)
    {
        var nextStep = stepToTrigger.GetStepToRetrigger();
        var context = await _offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(offerSubscriptionId, stepToTrigger, null)
            .ConfigureAwait(false);
        
        _offerSubscriptionProcessService.FinalizeProcessSteps(context, Enumerable.Repeat(nextStep, 1));
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<ProcessStepData> GetProcessStepsForSubscription(Guid offerSubscriptionId) =>
        _portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetProcessStepsForSubscription(offerSubscriptionId);
}