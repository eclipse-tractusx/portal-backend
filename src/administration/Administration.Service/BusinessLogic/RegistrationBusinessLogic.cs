/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Custodian;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.Mailing.SendMail;
using Org.CatenaX.Ng.Portal.Backend.Notification.Library;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;

public class RegistrationBusinessLogic : IRegistrationBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly RegistrationSettings _settings;
    private readonly IProvisioningManager _provisioningManager;
    private readonly ICustodianService _custodianService;
    private readonly IMailingService _mailingService;
    private readonly INotificationService _notificationService;
    private readonly ISdFactoryService _sdFactoryService;

    public RegistrationBusinessLogic(
        IPortalRepositories portalRepositories, 
        IOptions<RegistrationSettings> configuration, 
        IProvisioningManager provisioningManager, 
        ICustodianService custodianService, 
        IMailingService mailingService,
        INotificationService notificationService,
        ISdFactoryService sdFactoryService)
    {
        _portalRepositories = portalRepositories;
        _settings = configuration.Value;
        _provisioningManager = provisioningManager;
        _custodianService = custodianService;
        _mailingService = mailingService;
        _notificationService = notificationService;
        _sdFactoryService = sdFactoryService;
    }

    public Task<CompanyWithAddress> GetCompanyWithAddressAsync(Guid applicationId)
    {
        if (applicationId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(applicationId));
        }
        return GetCompanyWithAddressAsyncInternal(applicationId);
    }
    private async Task<CompanyWithAddress> GetCompanyWithAddressAsyncInternal(Guid applicationId)
    {
        var companyWithAddress = await _portalRepositories.GetInstance<IApplicationRepository>().GetCompanyWithAdressUntrackedAsync(applicationId).ConfigureAwait(false);
        if (companyWithAddress == null)
        {
            throw new NotFoundException($"no company found for applicationId {applicationId}");
        }
        return companyWithAddress;
    }

    public Task<Pagination.Response<CompanyApplicationDetails>> GetCompanyApplicationDetailsAsync(int page, int size, string? companyName = null)
    {
        var applications = _portalRepositories.GetInstance<IApplicationRepository>().GetCompanyApplicationsFilteredQuery(
            companyName?.Length >= 3 ? companyName : null,
            new[] { CompanyApplicationStatusId.SUBMITTED, CompanyApplicationStatusId.CONFIRMED, CompanyApplicationStatusId.DECLINED });

        return Pagination.CreateResponseAsync(
            page,
            size,
            _settings.ApplicationsMaxPageSize,
            (skip, take) => new Pagination.AsyncSource<CompanyApplicationDetails>(
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
                            invitation.CompanyUser!.Documents.Where(document => _settings.DocumentTypeIds.Contains(document.DocumentTypeId)).Select(document =>
                                new DocumentDetails(document.Id)
                                {
                                    DocumentTypeId = document.DocumentTypeId
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

    public Task<bool> ApprovePartnerRequest(string iamUserId, string accessToken, Guid applicationId, CancellationToken cancellationToken)
    {
        if (applicationId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(applicationId));
        }
        return ApprovePartnerRequestInternal(iamUserId, accessToken, applicationId, cancellationToken);
    }

    private async Task<bool> ApprovePartnerRequestInternal(string iamUserId, string accessToken, Guid applicationId, CancellationToken cancellationToken)
    {
        var creatorId = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyUserIdForIamUserUntrackedAsync(iamUserId).ConfigureAwait(false);
        if (creatorId == Guid.Empty)
        {
            throw new UnexpectedConditionException($"user {iamUserId} is not associated with a companyuser");
        }
        var applicationRepository = _portalRepositories.GetInstance<IApplicationRepository>();
        var result = await applicationRepository.GetCompanyAndApplicationDetailsForSubmittedApplicationAsync(applicationId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"CompanyApplication {applicationId} is not in status SUBMITTED");
        }
        var (companyId, companyName, businessPartnerNumber, countryCode) = result;

        if (string.IsNullOrWhiteSpace(businessPartnerNumber))
        {
            throw new ControllerArgumentException($"BusinessPartnerNumber (bpn) for CompanyApplications {applicationId} company {companyId} is empty", "bpn");
        }

        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        var assignedRoles = await AssignRolesAndBpn(applicationId, userRolesRepository, applicationRepository, businessPartnerNumber).ConfigureAwait(false);

        Guid? documentId = null;
        try
        {
            await _custodianService.CreateWalletAsync(businessPartnerNumber, companyName, cancellationToken).ConfigureAwait(false);

            documentId = await _sdFactoryService.RegisterSelfDescriptionAsync(accessToken, applicationId, countryCode, businessPartnerNumber, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            applicationRepository.AttachAndModifyCompanyApplication(applicationId, ca =>
            {
                ca.ApplicationStatusId = CompanyApplicationStatusId.CONFIRMED;
                ca.DateLastChanged = DateTimeOffset.UtcNow;    
            });

            _portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyCompany(companyId, c =>
            {
                c.CompanyStatusId = CompanyStatusId.ACTIVE;
                c.SelfDescriptionDocumentId = documentId;
            });

            var notifications = _settings.WelcomeNotificationTypeIds.Select(x => (default(string), x));
            await _notificationService.CreateNotifications(_settings.CompanyAdminRoles, creatorId, notifications).ConfigureAwait(false);
            await _portalRepositories.SaveAsync().ConfigureAwait(false);
        }

        await PostRegistrationWelcomeEmailAsync(userRolesRepository, applicationRepository, applicationId).ConfigureAwait(false);

        if (assignedRoles == null) return true;
        
        var unassignedClientRoles = _settings.ApplicationApprovalInitialRoles
            .Select(initialClientRoles => (
                client: initialClientRoles.Key,
                roles: initialClientRoles.Value.Except(assignedRoles[initialClientRoles.Key])))
            .Where(clientRoles => clientRoles.roles.Any())
            .ToList();

        if (unassignedClientRoles.Any())
        {
            throw new UnexpectedConditionException($"inconsistent data, roles not assigned in keycloak: {string.Join(", ", unassignedClientRoles.Select(clientRoles => $"client: {clientRoles.client}, roles: [{String.Join(", ", clientRoles.roles)}]"))}");
        }

        return true;
    }

    public Task<bool> DeclinePartnerRequest(Guid applicationId)
    {
        if (applicationId == Guid.NewGuid())
        {
            throw new ArgumentNullException(nameof(applicationId));
        }
        return DeclinePartnerRequestInternal(applicationId);
    }

    private async Task<bool> DeclinePartnerRequestInternal(Guid applicationId)
    {
        var companyApplication = await _portalRepositories.GetInstance<IApplicationRepository>().GetCompanyAndApplicationForSubmittedApplication(applicationId).ConfigureAwait(false);
        if (companyApplication == null)
        {
            throw new ArgumentException($"CompanyApplication {applicationId} is not in status SUBMITTED", nameof(applicationId));
        }
        companyApplication.ApplicationStatusId = CompanyApplicationStatusId.DECLINED;
        companyApplication.DateLastChanged = DateTimeOffset.UtcNow;
        companyApplication.Company!.CompanyStatusId = CompanyStatusId.REJECTED;
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        await PostRegistrationCancelEmailAsync(applicationId).ConfigureAwait(false);
        return true;
    }

    private async Task PostRegistrationCancelEmailAsync(Guid applicationId)
    {
        var userRoleIds = await _portalRepositories.GetInstance<IUserRolesRepository>()
            .GetUserRoleIdsUntrackedAsync(_settings.PartnerUserInitialRoles).ToListAsync().ConfigureAwait(false);

        await foreach (var user in _portalRepositories.GetInstance<IApplicationRepository>().GetRegistrationDeclineEmailDataUntrackedAsync(applicationId, userRoleIds).ConfigureAwait(false))
        {
            var userName = string.Join(" ", new[] { user.FirstName, user.LastName }.Where(item => !string.IsNullOrWhiteSpace(item)));

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new ArgumentException($"user {userName} has no assigned email");
            }

            var mailParameters = new Dictionary<string, string>
                {
                    { "userName", !string.IsNullOrWhiteSpace(userName) ?  userName : user.Email },
                    { "companyName", user.CompanyName }
                };

            await _mailingService.SendMails(user.Email, mailParameters, new List<string> { "EmailRegistrationDeclineTemplate" }).ConfigureAwait(false);
        }
    }

    public Task<Pagination.Response<CompanyApplicationWithCompanyUserDetails>> GetAllCompanyApplicationsDetailsAsync(int page, int size, string? companyName = null)
    {
        var applications = _portalRepositories.GetInstance<IApplicationRepository>().GetAllCompanyApplicationsDetailsQuery(companyName);

        return Pagination.CreateResponseAsync(
            page,
            size,
            _settings.ApplicationsMaxPageSize,
            (skip, take) => new Pagination.AsyncSource<CompanyApplicationWithCompanyUserDetails>(
                applications.CountAsync(),
                applications.OrderByDescending(application => application.DateCreated)
                    .Skip(skip)
                    .Take(take)
                    .Select(application => new
                    {
                        Application = application,
                        CompanyUser = application.Invitations.Select(invitation => invitation.CompanyUser)
                    .FirstOrDefault(companyUser =>
                        companyUser!.CompanyUserStatusId == CompanyUserStatusId.ACTIVE
                        && companyUser.Firstname != null
                        && companyUser.Lastname != null
                        && companyUser.Email != null
                    )
                    })
                    .Select(s => new CompanyApplicationWithCompanyUserDetails(
                        s.Application.Id,
                        s.Application.ApplicationStatusId,
                        s.Application.DateCreated,
                        s.Application.Company!.Name)
                    {
                        FirstName = s.CompanyUser!.Firstname,
                        LastName = s.CompanyUser.Lastname,
                        Email = s.CompanyUser.Email
                    })
                    .AsAsyncEnumerable()));
    }

    private async Task PostRegistrationWelcomeEmailAsync(IUserRolesRepository userRolesRepository, IApplicationRepository applicationRepository, Guid applicationId)
    {
        var failedUserNames = new List<string>();
        var initialRolesData = await GetRoleData(userRolesRepository, _settings.CompanyAdminRoles).ConfigureAwait(false);
        await foreach (var user in applicationRepository.GetWelcomeEmailDataUntrackedAsync(applicationId, initialRolesData.Select(x => x.UserRoleId)).ConfigureAwait(false))
        {
            var userName = string.Join(" ", new[] { user.FirstName, user.LastName }.Where(item => !string.IsNullOrWhiteSpace(item)));
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                failedUserNames.Add(userName);
                continue;
            }

            var mailParameters = new Dictionary<string, string>
                {
                    { "userName", !string.IsNullOrWhiteSpace(userName) ?  userName : user.Email },
                    { "companyName", user.CompanyName }
                };

            await _mailingService.SendMails(user.Email, mailParameters, new List<string> { "EmailRegistrationWelcomeTemplate" }).ConfigureAwait(false);
        }

        if (failedUserNames.Any())
            throw new ArgumentException($"user(s) {string.Join(",", failedUserNames)} has no assigned email");
    }

    private async Task<IDictionary<string, IEnumerable<string>>?> AssignRolesAndBpn(Guid applicationId, IUserRolesRepository userRolesRepository, IApplicationRepository applicationRepository, string businessPartnerNumber)
    {
        var userBusinessPartnersRepository = _portalRepositories.GetInstance<IUserBusinessPartnerRepository>();

        var applicationApprovalInitialRoles = _settings.ApplicationApprovalInitialRoles;
        var initialRolesData = await GetRoleData(userRolesRepository, applicationApprovalInitialRoles).ConfigureAwait(false);

        IDictionary<string, IEnumerable<string>>? assignedRoles = null;
        var invitedUsersData = applicationRepository
            .GetInvitedUsersDataByApplicationIdUntrackedAsync(applicationId);
        await foreach (var userData in invitedUsersData.ConfigureAwait(false))
        {
            assignedRoles = await _provisioningManager
                .AssignClientRolesToCentralUserAsync(userData.UserEntityId, applicationApprovalInitialRoles)
                .ToDictionaryAsync(assigned => assigned.Client, assigned => assigned.Roles)
                .ConfigureAwait(false);

            foreach (var roleData in initialRolesData)
            {
                if (!userData.RoleIds.Contains(roleData.UserRoleId) &&
                    assignedRoles[roleData.ClientClientId].Contains(roleData.UserRoleText))
                {
                    userRolesRepository.CreateCompanyUserAssignedRole(userData.CompanyUserId, roleData.UserRoleId);
                }
            }

            if (userData.BusinessPartnerNumbers.Contains(businessPartnerNumber)) continue;

            userBusinessPartnersRepository.CreateCompanyUserAssignedBusinessPartner(userData.CompanyUserId, businessPartnerNumber);
            await _provisioningManager
                .AddBpnAttributetoUserAsync(userData.UserEntityId, Enumerable.Repeat(businessPartnerNumber, 1))
                .ConfigureAwait(false);
        }

        return assignedRoles;
    }

    private static async Task<List<UserRoleData>> GetRoleData(IUserRolesRepository userRolesRepository, IDictionary<string, IEnumerable<string>> roles)
    {
        var roleData = await userRolesRepository
            .GetUserRoleDataUntrackedAsync(roles)
            .ToListAsync()
            .ConfigureAwait(false);
        if (roleData.Count < roles.Sum(clientRoles => clientRoles.Value.Count()))
        {
            throw new ConfigurationException($"invalid configuration, at least one of the configured roles does not exist in the database: {string.Join(", ", roles.Select(clientRoles => $"client: {clientRoles.Key}, roles: [{string.Join(", ", clientRoles.Value)}]"))}");
        }

        return roleData;
    }

    public Task UpdateCompanyBpn(Guid applicationId, string bpn)
    {
        var regex = new Regex(@"(\w|\d){16}");
        if (!regex.IsMatch(bpn))
        {
            throw new ControllerArgumentException("BPN must contain exactly 16 characters long.", nameof(bpn));
        }
        if (!bpn.StartsWith("BPNL", StringComparison.OrdinalIgnoreCase))
        {
            throw new ControllerArgumentException("businessPartnerNumbers must prefixed with BPNL", nameof(bpn));
        }
        return UpdateCompanyBpnAsync(applicationId, bpn);
    }

    private async Task UpdateCompanyBpnAsync(Guid applicationId, string bpn)
    {
        var result = await _portalRepositories.GetInstance<IUserRepository>().GetBpnForIamUserUntrackedAsync(applicationId, bpn).ToListAsync().ConfigureAwait(false);
        if (!result.Any(item => item.IsApplicationCompany))
        {
            throw new NotFoundException($"application {applicationId} not found");
        }
        if (result.Any(item => !item.IsApplicationCompany))
        {
            throw new ConflictException($"BusinessPartnerNumber is already assigned to a different company");
        }
        var applicationCompanyData = result.Single(item => item.IsApplicationCompany);
        if (!applicationCompanyData.IsApplicationPending)
        {
            throw new ConflictException($"application {applicationId} for company {applicationCompanyData.CompanyId} is not pending");
        }
        if (!string.IsNullOrWhiteSpace(applicationCompanyData.BusinessPartnerNumber))
        {
            throw new ConflictException($"BusinessPartnerNumber of company {applicationCompanyData.CompanyId} has already been set.");
        }

        _portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyCompany(applicationCompanyData.CompanyId, c =>
        {
            c.BusinessPartnerNumber = bpn;
        });

        await _portalRepositories.SaveAsync().ConfigureAwait(false);

    }
}
