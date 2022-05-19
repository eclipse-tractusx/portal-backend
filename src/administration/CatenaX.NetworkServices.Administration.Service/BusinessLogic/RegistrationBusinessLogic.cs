using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.Framework.Models;
using Microsoft.Extensions.Options;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class RegistrationBusinessLogic : IRegistrationBusinessLogic
{
    private readonly IPortalBackendDBAccess _portalDBAccess;
    private readonly RegistrationSettings _settings;

    public RegistrationBusinessLogic(IPortalBackendDBAccess portalDBAccess, IOptions<RegistrationSettings> configuration)
    {
        _portalDBAccess = portalDBAccess;
        _settings = configuration.Value;
    }

    public async Task<CompanyWithAddress> GetCompanyWithAddressAsync(Guid? applicationId)
    {
        if (!applicationId.HasValue)
        {
            throw new ArgumentNullException("applicationId must not be null");
        }
        var companyWithAddress = await _portalDBAccess.GetCompanyWithAdressUntrackedAsync(applicationId.Value).ConfigureAwait(false);
        if (companyWithAddress == null)
        {
            throw new NotFoundException($"no company found for applicationId {applicationId.Value}");
        }
        return companyWithAddress;
    }

    public Task<PaginationResponse<CompanyApplicationDetails>> GetCompanyApplicationDetailsAsync(int page, int size) =>
        PaginationResponse<CompanyApplicationDetails>.CreatePaginationResponseAsync(
            page,
            size,
            _settings.ApplicationsMaxPageSize,
            () => _portalDBAccess.GetApplicationsCountAsync(),
            (skip, take) => _portalDBAccess.GetCompanyApplicationDetailsUntrackedAsync(skip, take));
}
