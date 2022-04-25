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
        
        public Task SetCompanyWithAddressAsync(Guid? applicationId, CompanyWithAddress? companyWithAddress)
        {
            if (!applicationId.HasValue)
            {
                throw new ArgumentNullException("applicationId must not be null");
            }
            if (companyWithAddress == null)
            {
                throw new ArgumentNullException("companyWithAddress must not be null");
            }
            //FIXMX: add update of company status within same transpaction
            return _portalDBAccess.SetCompanyWithAdressAsync(applicationId.Value, companyWithAddress);
        }
    }
}
