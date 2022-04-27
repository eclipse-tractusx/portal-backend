using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic
{
    public class RegistrationAdministrationBusinessLogic : IRegistrationAdministrationBusinessLogic
    {
        private readonly IPortalBackendDBAccess _portalDBAccess;
        private readonly ILogger<RegistrationAdministrationBusinessLogic> _logger;

        public RegistrationAdministrationBusinessLogic(IPortalBackendDBAccess portalDBAccess, ILogger<RegistrationAdministrationBusinessLogic> logger)
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
