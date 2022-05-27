using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Administration.Service.Custodian;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.Extensions.Options;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class RegistrationBusinessLogic : IRegistrationBusinessLogic
{
    private readonly IPortalBackendDBAccess _portalDBAccess;
    private readonly RegistrationSettings _settings;
    private readonly IProvisioningManager _provisioningManager;
    private readonly ICustodianService _custodianService;
    private readonly IMailingService _mailingService;
    public RegistrationBusinessLogic(IPortalBackendDBAccess portalDBAccess, IOptions<RegistrationSettings> configuration, IProvisioningManager provisioningManager, ICustodianService custodianService, IMailingService mailingService)
    {
        _portalDBAccess = portalDBAccess;
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
        var companyWithAddress = await _portalDBAccess.GetCompanyWithAdressUntrackedAsync(applicationId.Value).ConfigureAwait(false);
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
            (skip, take) => _portalDBAccess.GetCompanyApplicationDetailsUntrackedAsync(skip, take));

    public async Task<bool> ApprovePartnerRequest(Guid applicationId)
    {
        var companyApplication = await _portalDBAccess.GetCompanyAndApplicationForSubmittedApplication(applicationId).ConfigureAwait(false);
        if (companyApplication == null)
        {
            throw new ArgumentException($"CompanyApplication {applicationId} is not in status SUBMITTED", "applicationId");
        }
        if (companyApplication.Company!.Bpn == null)
        {
            throw new ArgumentException($"BusinessPartnerNumber (bpn) for CompanyApplications {applicationId} company {companyApplication.CompanyId} is null", "bpn");
        }
        var userRoleIds = await _portalDBAccess.GetUserRoleIdsUntrackedAsync(_settings.ApplicationApprovalInitialRoles).ToListAsync().ConfigureAwait(false);

        await foreach (var item in _portalDBAccess.GetInvitedUsersByApplicationIdUntrackedAsync(applicationId).ConfigureAwait(false))
        {
            await _provisioningManager.AssignClientRolesToCentralUserAsync(item.UserEntityId, _settings.ApplicationApprovalInitialRoles).ConfigureAwait(false);
            await _provisioningManager.AddBpnAttributetoUserAsync(item.UserEntityId, Enumerable.Repeat(companyApplication.Company.Bpn, 1));
            foreach (var userRoleId in userRoleIds)
            {
                _portalDBAccess.CreateCompanyUserAssignedRole(item.CompanyUserId, userRoleId);
            }
        }
        companyApplication.Company!.CompanyStatusId = CompanyStatusId.ACTIVE;
        companyApplication.ApplicationStatusId = CompanyApplicationStatusId.CONFIRMED;
        companyApplication.DateLastChanged = DateTimeOffset.UtcNow;

        await _portalDBAccess.SaveAsync().ConfigureAwait(false);

        await _custodianService.CreateWallet(companyApplication.Company.Bpn, companyApplication.Company.Name).ConfigureAwait(false);
        await PostRegistrationWelcomeEmailAsync(applicationId).ConfigureAwait(false);

        return true;
    }

    private async Task<bool> PostRegistrationWelcomeEmailAsync(Guid applicationId)
    {
        await foreach (var user in _portalDBAccess.GetWelcomeEmailDataUntrackedAsync(applicationId).ConfigureAwait(false))
        {
            if (String.IsNullOrWhiteSpace(user.EmailId))
            {
                throw new ArgumentException($"user {user.UserName} has no assigned email");
            }

            var mailParameters = new Dictionary<string, string>
                {
                    { "userName", user.UserName },
                    { "companyName", user.CompanyName }
                };

            await _mailingService.SendMails(user.EmailId, mailParameters, new List<string> { "EmailRegistrationWelcomeTemplate" }).ConfigureAwait(false);
        }
        return true;
    }
}
