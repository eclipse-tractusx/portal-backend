using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public interface IApplicationRepository
{
    Task<CompanyWithAddress?> GetCompanyWithAdressUntrackedAsync(Guid companyApplicationId);
    Pagination.AsyncSource<CompanyApplicationDetails> GetCompanyApplicationDetailsUntrackedAsync(int skip, int take);
    Task<CompanyApplication?> GetCompanyAndApplicationForSubmittedApplication(Guid applicationId);
    IAsyncEnumerable<CompanyInvitedUserData> GetInvitedUsersDataByApplicationIdUntrackedAsync(Guid applicationId);
    IAsyncEnumerable<WelcomeEmailData> GetWelcomeEmailDataUntrackedAsync(Guid applicationId);
}
