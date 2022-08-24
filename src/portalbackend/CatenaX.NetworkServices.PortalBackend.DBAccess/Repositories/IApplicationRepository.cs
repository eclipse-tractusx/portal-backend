using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public interface IApplicationRepository
{
    CompanyApplication CreateCompanyApplication(Company company, CompanyApplicationStatusId companyApplicationStatusId);
    Invitation CreateInvitation(Guid applicationId, CompanyUser user);
    Task<CompanyApplicationUserData?> GetOwnCompanyApplicationUserDataAsync(Guid applicationId, string iamUserId);
    Task<CompanyApplicationStatusUserData?> GetOwnCompanyApplicationStatusUserDataUntrackedAsync(Guid applicationId, string iamUserId);
    Task<CompanyApplicationUserEmailData?> GetOwnCompanyApplicationUserEmailDataAsync(Guid applicationId, string iamUserId);
    Task<CompanyWithAddress?> GetCompanyWithAdressUntrackedAsync(Guid companyApplicationId);
    IQueryable<CompanyApplication> GetCompanyApplicationsFilteredQuery(string? companyName = null, IEnumerable<CompanyApplicationStatusId>? applicationStatusIds = null);
    Task<CompanyApplicationWithCompanyAddressUserData?> GetCompanyApplicationWithCompanyAdressUserDataAsync (Guid applicationId, Guid companyId, string iamUserId);
    Task<CompanyApplication?> GetCompanyAndApplicationForSubmittedApplication(Guid applicationId);
    IAsyncEnumerable<CompanyInvitedUserData> GetInvitedUsersDataByApplicationIdUntrackedAsync(Guid applicationId);
    IAsyncEnumerable<WelcomeEmailData> GetWelcomeEmailDataUntrackedAsync(Guid applicationId, IEnumerable<Guid> roleIds);
    IAsyncEnumerable<WelcomeEmailData> GetRegistrationDeclineEmailDataUntrackedAsync(Guid applicationId, IEnumerable<Guid> roleIds);
    IQueryable<CompanyApplication> GetAllCompanyApplicationsDetailsQuery(string? companyName = null);
}
