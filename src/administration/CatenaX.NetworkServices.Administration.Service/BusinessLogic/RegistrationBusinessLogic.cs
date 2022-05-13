using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.Administration.Service.Models;
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

    public Task<CompanyWithAddress> GetCompanyWithAddressAsync(Guid? applicationId)
    {
        if (!applicationId.HasValue)
        {
            throw new ArgumentNullException("applicationId must not be null");
        }
        return _portalDBAccess.GetCompanyWithAdressUntrackedAsync(applicationId.Value);
    }

    public IAsyncEnumerable<CompanyApplicationDetails> GetCompanyApplicationDetailsAsync(int page)
    {
        if (page <= 0)
        {
            throw new ArgumentException("parameter page must be > 0", "page");
        }
        return _portalDBAccess.GetCompanyApplicationDetailsUntrackedAsync(
            _settings.ApplicationsPageSize * (page-1),
            _settings.ApplicationsPageSize
        );
    }

    public async Task<PaginationData> GetApplicationPaginationDataAsync()
    {
        int count = await _portalDBAccess.GetApplicationsCountAsync().ConfigureAwait(false);
        return new PaginationData(
            count,
            count / _settings.ApplicationsPageSize + 1,
            _settings.ApplicationsPageSize
        );
    }
}
