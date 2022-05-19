using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public interface IRegistrationBusinessLogic
{
    Task<CompanyWithAddress> GetCompanyWithAddressAsync(Guid? applicationId);
    public Task<Pagination.Response<CompanyApplicationDetails>> GetCompanyApplicationDetailsAsync(int page, int size);
}
