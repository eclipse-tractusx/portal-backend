using CatenaX.NetworkServices.Administration.Service.Custodian;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library;
using Microsoft.Extensions.Options;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class RegistrationBusinessLogic : IRegistrationBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IApplicationRepository _applicationRepository;
    private readonly RegistrationSettings _settings;
    private readonly IProvisioningManager _provisioningManager;
    private readonly ICustodianService _custodianService;
    private readonly IMailingService _mailingService;
    public RegistrationBusinessLogic(IPortalRepositories portalRepositories, IOptions<RegistrationSettings> configuration, IProvisioningManager provisioningManager, ICustodianService custodianService, IMailingService mailingService)
    {
        _portalRepositories = portalRepositories;
        _applicationRepository = portalRepositories.GetInstance<IApplicationRepository>();
        _settings = configuration.Value;
        _provisioningManager = provisioningManager;
        _custodianService = custodianService;
        _mailingService = mailingService;
    }

    public async Task<CompanyWithAddress> GetCompanyWithAddressAsync(Guid? applicationId)
    {
        if (!applicationId.HasValue)
        {
            throw new ArgumentNullException("applicationId must not be null");
        }
        var companyWithAddress = await _applicationRepository.GetCompanyWithAdressUntrackedAsync(applicationId.Value).ConfigureAwait(false);
        if (companyWithAddress == null)
        {
            throw new NotFoundException($"no company found for applicationId {applicationId.Value}");
        }
        return companyWithAddress;
    }

    public Task<Pagination.Response<CompanyApplicationDetails>> GetCompanyApplicationDetailsAsync(int page, int size) =>
        Pagination.CreateResponseAsync<CompanyApplicationDetails>(
            page,
            size,
            _settings.ApplicationsMaxPageSize,
            (skip, take) => _applicationRepository.GetCompanyApplicationDetailsUntrackedAsync(skip, take));

    public async Task<bool> ApprovePartnerRequest(Guid applicationId)
    {
        var companyApplication = await _applicationRepository.GetCompanyAndApplicationForSubmittedApplication(applicationId).ConfigureAwait(false);
        if (companyApplication == null)
        {
            throw new ArgumentException($"CompanyApplication {applicationId} is not in status SUBMITTED", "applicationId");
        }

        var businessPartnerNumber = companyApplication.Company!.BusinessPartnerNumber;
        if (String.IsNullOrWhiteSpace(businessPartnerNumber))
        {
            throw new ArgumentException($"BusinessPartnerNumber (bpn) for CompanyApplications {applicationId} company {companyApplication.CompanyId} is empty", "bpn");
        }

        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        var userBusinessPartnersRepository = _portalRepositories.GetInstance<IUserBusinessPartnerRepository>();

        var userRoleIds = await userRolesRepository.GetUserRoleIdsUntrackedAsync(_settings.ApplicationApprovalInitialRoles).ToListAsync().ConfigureAwait(false);

        await foreach (var item in _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(applicationId).ConfigureAwait(false))
        {
            await _provisioningManager.AssignClientRolesToCentralUserAsync(item.UserEntityId, _settings.ApplicationApprovalInitialRoles).ConfigureAwait(false);
            foreach (var userRoleId in userRoleIds)
            {
                if (!item.RoleIds.Contains(userRoleId))
                {
                    userRolesRepository.CreateCompanyUserAssignedRole(item.CompanyUserId, userRoleId);
                }
            }
            if (!item.BusinessPartnerNumbers.Contains(businessPartnerNumber))
            {
                userBusinessPartnersRepository.CreateCompanyUserAssignedBusinessPartner(item.CompanyUserId,businessPartnerNumber);
                await _provisioningManager.AddBpnAttributetoUserAsync(item.UserEntityId, Enumerable.Repeat(businessPartnerNumber, 1));
            }
        }
        companyApplication.Company!.CompanyStatusId = CompanyStatusId.ACTIVE;
        companyApplication.ApplicationStatusId = CompanyApplicationStatusId.CONFIRMED;
        companyApplication.DateLastChanged = DateTimeOffset.UtcNow;
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        await _custodianService.CreateWallet(businessPartnerNumber, companyApplication.Company.Name).ConfigureAwait(false);
        await PostRegistrationWelcomeEmailAsync(applicationId).ConfigureAwait(false);

        return true;
    }

    private async Task<bool> PostRegistrationWelcomeEmailAsync(Guid applicationId)
    {
        await foreach (var user in _applicationRepository.GetWelcomeEmailDataUntrackedAsync(applicationId).ConfigureAwait(false))
        {
            var userName = String.Join(" ", new[] { user.FirstName, user.LastName }.Where(item => !String.IsNullOrWhiteSpace(item)));

            if (String.IsNullOrWhiteSpace(user.Email))
            {
                throw new ArgumentException($"user {userName} has no assigned email");
            }

            var mailParameters = new Dictionary<string, string>
                {
                    { "userName", !String.IsNullOrWhiteSpace(userName) ?  userName : user.Email },
                    { "companyName", user.CompanyName }
                };

            await _mailingService.SendMails(user.Email, mailParameters, new List<string> { "EmailRegistrationWelcomeTemplate" }).ConfigureAwait(false);
        }
        return true;
    }
}
