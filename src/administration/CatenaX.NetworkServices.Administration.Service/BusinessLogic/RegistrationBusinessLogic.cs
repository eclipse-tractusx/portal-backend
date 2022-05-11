using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class RegistrationBusinessLogic : IRegistrationBusinessLogic
{
    private readonly IPortalBackendDBAccess _portalDBAccess;
    private readonly ILogger<RegistrationBusinessLogic> _logger;

    public RegistrationBusinessLogic(IPortalBackendDBAccess portalDBAccess, ILogger<RegistrationBusinessLogic> logger)
    {
        _portalDBAccess = portalDBAccess;
        _logger = logger;
    }

    public Task<CompanyWithAddress> GetCompanyWithAddressAsync(Guid? applicationId)
    {
        if (!applicationId.HasValue)
        {
            throw new ArgumentNullException("applicationId must not be null");
        }
        return _portalDBAccess.GetCompanyWithAdressUntrackedAsync(applicationId.Value);
    }

    public IAsyncEnumerable<CompanyApplicationDetails> GetCompanyApplicationDetailsAsync()
    {
        return _portalDBAccess.GetCompanyApplicationDetailsUntrackedAsync();
    }
}
