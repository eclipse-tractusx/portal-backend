using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public interface IApplicationRepository
{
    CompanyApplication CreateCompanyApplication(Company company, CompanyApplicationStatusId companyApplicationStatusId);
    Invitation CreateInvitation(Guid applicationId, CompanyUser user);
    Task<CompanyApplicationUserData?> GetCompanyApplicationUserDataAsync(Guid applicationId, string iamUserId);
    Task<CompanyWithAddress?> GetCompanyWithAdressUntrackedAsync(Guid companyApplicationId);
    Task<Company?> GetCompanyWithAdressAsync(Guid companyApplicationId, Guid companyId);
    Task<CompanyApplicationWithCompanyAddressUserData?> GetCompanyApplicationWithCompanyAdressUserDataAsync (Guid applicationId, Guid companyId, string iamUserId);
    Pagination.AsyncSource<CompanyApplicationDetails> GetCompanyApplicationDetailsUntrackedAsync(int skip, int take);
    Task<CompanyApplication?> GetCompanyAndApplicationForSubmittedApplication(Guid applicationId);
    IAsyncEnumerable<CompanyInvitedUserData> GetInvitedUsersDataByApplicationIdUntrackedAsync(Guid applicationId);
    IAsyncEnumerable<WelcomeEmailData> GetWelcomeEmailDataUntrackedAsync(Guid applicationId);
    IAsyncEnumerable<WelcomeEmailData> GetRegistrationDeclineEmailDataUntrackedAsync(Guid applicationId, IEnumerable<Guid> roleIds);
}
