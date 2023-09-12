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
        var (isOnboardingServiceProvider, ospDetailsExist, callbackUrl) = await companyRepository
            .GetCallbackEditData(companyId)
            .ConfigureAwait(false);

        if (!isOnboardingServiceProvider)
        {
            throw new ForbiddenException($"Only {CompanyRoleId.ONBOARDING_SERVICE_PROVIDER} are allowed to set the callback url");
        }

        if (ospDetailsExist)
        {
            companyRepository.AttachAndModifyOnboardingServiceProvider(companyId, osp =>
                {
                    if (!string.IsNullOrEmpty(callbackUrl))
                    {
                        osp.CallbackUrl = callbackUrl;
                    }
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
