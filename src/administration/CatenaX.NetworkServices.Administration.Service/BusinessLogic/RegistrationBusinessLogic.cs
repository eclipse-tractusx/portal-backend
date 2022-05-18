using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
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
