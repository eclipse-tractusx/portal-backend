using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public interface IRegistrationBusinessLogic
{
    Task<CompanyWithAddress> GetCompanyWithAddressAsync(Guid? applicationId);
    Task<Pagination.Response<CompanyApplicationDetails>> GetCompanyApplicationDetailsAsync(int page, int size, string? companyName = null);
    Task<bool> ApprovePartnerRequest(Guid applicationId);
    Task<bool> DeclinePartnerRequest(Guid applicationId);
    Task<Pagination.Response<CompanyApplicationWithCompanyUserDetails>> GetAllCompanyApplicationsDetailsAsync(int page, int size);
}
