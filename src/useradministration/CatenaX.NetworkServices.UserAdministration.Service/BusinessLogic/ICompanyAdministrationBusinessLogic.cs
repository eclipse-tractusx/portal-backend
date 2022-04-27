using System;
using System.Threading.Tasks;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.UserAdministration.Service.BusinessLogic
{
    public interface ICompanyAdministrationBusinessLogic
    {
        Task<CompanyWithAddress> GetCompanyWithAddressAsync(Guid? applicationId);
    }
}
