using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.UserAdministration.Service.BusinessLogic
{
    public class CompanyAdministrationBusinessLogic : ICompanyAdministrationBusinessLogic
    {
        private readonly IPortalBackendDBAccess _portalDBAccess;
        private readonly ILogger<CompanyAdministrationBusinessLogic> _logger;

        public CompanyAdministrationBusinessLogic(IPortalBackendDBAccess portalDBAccess, ILogger<CompanyAdministrationBusinessLogic> logger)
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
    }
}
