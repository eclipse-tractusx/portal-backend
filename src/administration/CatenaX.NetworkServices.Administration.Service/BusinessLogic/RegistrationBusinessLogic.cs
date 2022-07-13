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
using Microsoft.EntityFrameworkCore;

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

    public Task<Pagination.Response<CompanyApplicationDetails>> GetCompanyApplicationDetailsAsync(int page, int size, string? companyName = null)
    {
        var applications = _applicationRepository.GetCompanyApplicationsFilteredQuery(
            companyName?.Length >= 3 ? companyName : null,
            new CompanyApplicationStatusId[] { CompanyApplicationStatusId.SUBMITTED, CompanyApplicationStatusId.CONFIRMED, CompanyApplicationStatusId.DECLINED });

        return Pagination.CreateResponseAsync<CompanyApplicationDetails>(
            page,
            size,
            _settings.ApplicationsMaxPageSize,
            (int skip, int take) => new Pagination.AsyncSource<CompanyApplicationDetails>(
                applications.CountAsync(),
                applications.OrderByDescending(application => application.DateCreated)
                    .Skip(skip)
                    .Take(take)
                    .Select(application => new CompanyApplicationDetails(
                        application.Id,
                        application.ApplicationStatusId,
                        application.DateCreated,
                        application.Company!.Name,
                        application.Invitations.SelectMany(invitation =>
                            invitation.CompanyUser!.Documents.Select(document =>
                                new DocumentDetails(document.DocumentHash)
                                {
                                    DocumentTypeId = document.DocumentTypeId,
                                })))
                    {
                        Email = application.Invitations
                            .Select(invitation => invitation.CompanyUser)
                            .Where(companyUser => companyUser!.CompanyUserStatusId == CompanyUserStatusId.ACTIVE
                                && companyUser.Email != null)
                            .Select(companyUser => companyUser!.Email)
                            .FirstOrDefault(),
                        BusinessPartnerNumber = application.Company.BusinessPartnerNumber
                    })
                    .AsAsyncEnumerable()));
    }

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

        var initialRolesData = await userRolesRepository.GetUserRoleDataUntrackedAsync(_settings.ApplicationApprovalInitialRoles).ToListAsync().ConfigureAwait(false);
        if (initialRolesData.Count() < _settings.ApplicationApprovalInitialRoles.Sum(clientRoles => clientRoles.Value.Count()))
        {
            throw new Exception($"invalid configuration, at least one of the configured roles does not exist in the database: {String.Join(", ", _settings.ApplicationApprovalInitialRoles.Select(clientRoles => $"client: {clientRoles.Key}, roles: [{String.Join(", ", clientRoles.Value)}]"))}");
        }

        IDictionary<string, IEnumerable<string>>? assignedRoles = null;
        await foreach (var userData in _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(applicationId).ConfigureAwait(false))
        {
            assignedRoles  = await _provisioningManager.AssignClientRolesToCentralUserAsync(userData.UserEntityId, _settings.ApplicationApprovalInitialRoles).ConfigureAwait(false);
            
            foreach (var roleData in initialRolesData)
            {
                if (!userData.RoleIds.Contains(roleData.UserRoleId) && assignedRoles[roleData.ClientClientId].Contains(roleData.UserRoleText))
                {
                    userRolesRepository.CreateCompanyUserAssignedRole(userData.CompanyUserId, roleData.UserRoleId);
                }
            }
            if (!userData.BusinessPartnerNumbers.Contains(businessPartnerNumber))
            {
                userBusinessPartnersRepository.CreateCompanyUserAssignedBusinessPartner(userData.CompanyUserId, businessPartnerNumber);
                await _provisioningManager.AddBpnAttributetoUserAsync(userData.UserEntityId, Enumerable.Repeat(businessPartnerNumber, 1));
            }
        }
        companyApplication.Company!.CompanyStatusId = CompanyStatusId.ACTIVE;
        companyApplication.ApplicationStatusId = CompanyApplicationStatusId.CONFIRMED;
        companyApplication.DateLastChanged = DateTimeOffset.UtcNow;
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        await _custodianService.CreateWallet(businessPartnerNumber, companyApplication.Company.Name).ConfigureAwait(false);
        await PostRegistrationWelcomeEmailAsync(applicationId).ConfigureAwait(false);

        if (assignedRoles != null)
        {
            var unassignedClientRoles = _settings.ApplicationApprovalInitialRoles
                .Select(initialClientRoles => (client: initialClientRoles.Key, roles: initialClientRoles.Value.Except(assignedRoles[initialClientRoles.Key])))
                .Where(clientRoles => clientRoles.roles.Count() > 0);

            if (unassignedClientRoles.Count() > 0)
            {
                throw new Exception($"inconsistend data, roles not assigned in keycloak: {String.Join(", ", unassignedClientRoles.Select(clientRoles => $"client: {clientRoles.client}, roles: [{String.Join(", ", clientRoles.roles)}]"))}");
            }
        }
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

    public async Task<bool> DeclinePartnerRequest(Guid applicationId)
    {
        var companyApplication = await _applicationRepository.GetCompanyAndApplicationForSubmittedApplication(applicationId).ConfigureAwait(false);
        if (companyApplication == null)
        {
            throw new ArgumentException($"CompanyApplication {applicationId} is not in status SUBMITTED", "applicationId");
        }
        companyApplication.ApplicationStatusId = CompanyApplicationStatusId.DECLINED;
        companyApplication.DateLastChanged = DateTimeOffset.UtcNow;
        companyApplication.Company!.CompanyStatusId = CompanyStatusId.REJECTED;
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        await PostRegistrationCancelEmailAsync(applicationId).ConfigureAwait(false);
        return true;
    }

    private async Task<bool> PostRegistrationCancelEmailAsync(Guid applicationId)
    {
        var userRoleIds = await _portalRepositories.GetInstance<IUserRolesRepository>()
            .GetUserRoleIdsUntrackedAsync(_settings.PartnerUserInitialRoles).ToListAsync().ConfigureAwait(false);

        await foreach (var user in _applicationRepository.GetRegistrationDeclineEmailDataUntrackedAsync(applicationId, userRoleIds).ConfigureAwait(false))
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

            await _mailingService.SendMails(user.Email, mailParameters, new List<string> { "EmailRegistrationDeclineTemplate" }).ConfigureAwait(false);
        }
        return true;
    }

    public Task<Pagination.Response<CompanyApplicationWithCompanyUserDetails>> GetAllCompanyApplicationsDetailsAsync(int page, int size)
    {
        var applications = _applicationRepository.GetAllCompanyApplicationsDetailsQuery();

        return Pagination.CreateResponseAsync<CompanyApplicationWithCompanyUserDetails>(
            page,
            size,
            _settings.ApplicationsMaxPageSize,
            (int skip, int take) => new Pagination.AsyncSource<CompanyApplicationWithCompanyUserDetails>(
                applications.CountAsync(),
                applications.OrderByDescending(application => application.DateCreated)
                    .Skip(skip)
                    .Take(take)
                    .Select(application => new CompanyApplicationWithCompanyUserDetails(
                        application.ApplicationStatusId,
                        application.DateCreated,
                        application.Company!.Name)
                    {
                        FirstName = application.Invitations
                            .Select(invitation => invitation.CompanyUser)
                            .Where(companyUser => companyUser!.CompanyUserStatusId == CompanyUserStatusId.ACTIVE
                                && companyUser!.Firstname != null)
                            .Select(companyUser => companyUser!.Firstname)
                            .FirstOrDefault(),
                        LastName = application.Invitations
                            .Select(invitation => invitation.CompanyUser)
                            .Where(companyUser => companyUser!.CompanyUserStatusId == CompanyUserStatusId.ACTIVE
                                && companyUser!.Lastname != null)
                            .Select(companyUser => companyUser!.Lastname)
                            .FirstOrDefault(),
                        Email = application.Invitations
                            .Select(invitation => invitation.CompanyUser)
                            .Where(companyUser => companyUser!.CompanyUserStatusId == CompanyUserStatusId.ACTIVE
                                && companyUser!.Email != null)
                            .Select(companyUser => companyUser!.Email)
                            .FirstOrDefault()
                    })
                    .AsAsyncEnumerable()));
    }
}
