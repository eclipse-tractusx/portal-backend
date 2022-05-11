using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public interface IRegistrationBusinessLogic
{
    Task<CompanyWithAddress> GetCompanyWithAddressAsync(Guid? applicationId);
    IAsyncEnumerable<CompanyApplicationDetails> GetCompanyApplicationDetailsAsync();
}
