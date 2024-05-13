/********************************************************************************
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.ApplicationActivation.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.ApplicationActivation.Library;

public class ApplicationActivationService : IApplicationActivationService
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly INotificationService _notificationService;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IDateTimeProvider _dateTime;
    private readonly ICustodianService _custodianService;
    private readonly IMailingProcessCreation _mailingProcessCreation;
    private readonly ApplicationActivationSettings _settings;

    public ApplicationActivationService(
        IPortalRepositories portalRepositories,
        INotificationService notificationService,
        IProvisioningManager provisioningManager,
        IDateTimeProvider dateTime,
        ICustodianService custodianService,
        IMailingProcessCreation mailingProcessCreation,
        IOptions<ApplicationActivationSettings> options)
    {
        _portalRepositories = portalRepositories;
        _notificationService = notificationService;
        _provisioningManager = provisioningManager;
        _dateTime = dateTime;
        _custodianService = custodianService;
        _mailingProcessCreation = mailingProcessCreation;
        _settings = options.Value;
    }

    public Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> HandleApplicationActivation(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        if (!InProcessingTime())
        {
            return Task.FromResult(new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(ProcessStepStatusId.TODO, null, null, null, false, null));
        }
        var prerequisiteEntries = context.Checklist.Where(entry => entry.Key != ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION);
        if (prerequisiteEntries.Any(entry => entry.Value != ApplicationChecklistEntryStatusId.DONE))
        {
            throw new ConflictException($"cannot activate application {context.ApplicationId}. Checklist entries that are not in status DONE: {string.Join(",", prerequisiteEntries)}");
        }
        return HandleApplicationActivationInternal(context, cancellationToken);
    }

    private async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> HandleApplicationActivationInternal(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        var applicationRepository = _portalRepositories.GetInstance<IApplicationRepository>();
        var result = await applicationRepository.GetCompanyAndApplicationDetailsForApprovalAsync(context.ApplicationId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw new ConflictException($"CompanyApplication {context.ApplicationId} is not in status SUBMITTED");
        }
        var (companyId, companyName, businessPartnerNumber, iamIdpAliasse, applicationTypeId, networkRegistrationProcessId) = result;

        if (string.IsNullOrWhiteSpace(businessPartnerNumber))
        {
            throw new ConflictException($"BusinessPartnerNumber (bpn) for CompanyApplications {context.ApplicationId} company {companyId} is empty");
        }

        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        var assignedRoles = await AssignRolesAndBpn(context.ApplicationId, userRolesRepository, applicationRepository, businessPartnerNumber).ConfigureAwait(ConfigureAwaitOptions.None);
        await RemoveRegistrationRoles(context.ApplicationId, userRolesRepository).ConfigureAwait(ConfigureAwaitOptions.None);
        await SetTheme(iamIdpAliasse).ConfigureAwait(ConfigureAwaitOptions.None);

        applicationRepository.AttachAndModifyCompanyApplication(context.ApplicationId, ca =>
        {
            ca.ApplicationStatusId = CompanyApplicationStatusId.CONFIRMED;
            ca.DateLastChanged = DateTimeOffset.UtcNow;
        });

        _portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyCompany(companyId, null, c =>
        {
            c.CompanyStatusId = CompanyStatusId.ACTIVE;
        });

        if (applicationTypeId == CompanyApplicationTypeId.EXTERNAL)
        {
            if (networkRegistrationProcessId == null)
            {
                throw new ConflictException("ProcessId should be set for external applications");
            }

            _portalRepositories.GetInstance<IProcessStepRepository>().CreateProcessStepRange(Enumerable.Repeat(new ValueTuple<ProcessStepTypeId, ProcessStepStatusId, Guid>(ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED, ProcessStepStatusId.TODO, networkRegistrationProcessId.Value), 1));
        }

        var notifications = _settings.WelcomeNotificationTypeIds.Select(x => (default(string), x));
        await _notificationService.CreateNotifications(_settings.CompanyAdminRoles, null, notifications, companyId).AwaitAll(cancellationToken).ConfigureAwait(false);

        string? resultMessage = null;
        if (!_settings.UseDimWallet)
        {
            resultMessage = await _custodianService.SetMembership(businessPartnerNumber, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        await PostRegistrationWelcomeEmailAsync(applicationRepository, context.ApplicationId, companyName, businessPartnerNumber).ConfigureAwait(ConfigureAwaitOptions.None);

        if (assignedRoles != null)
        {
            _settings.ApplicationApprovalInitialRoles
                .Select(initialClientRoles => (
                    Initial: initialClientRoles,
                    AssignedRoles: assignedRoles[initialClientRoles.ClientId]))
                .Select(x => (
                    x.Initial.ClientId,
                    Unassigned: x.Initial.UserRoleNames.Except(x.AssignedRoles.Roles),
                    x.AssignedRoles.Error))
                .Where(clientRoles => clientRoles.Unassigned.Any())
                .IfAny(unassigned =>
                    throw new UnexpectedConditionException($"inconsistent data, roles not assigned in keycloak: {string.Join(", ", unassigned.Select(clientRoles => $"client: {clientRoles.ClientId}, roles: [{string.Join(", ", clientRoles.Unassigned)}], error: {clientRoles.Error}"))}"));
        }

        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            entry =>
            {
                entry.Comment = resultMessage;
                entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE;
            },
            null,
            Enum.GetValues<ProcessStepTypeId>().Except(new[] { ProcessStepTypeId.ACTIVATE_APPLICATION }),
            true,
            null);
    }

    private bool InProcessingTime()
    {
        var startTime = _settings.StartTime;
        var endTime = _settings.EndTime;
        if (!startTime.HasValue || !endTime.HasValue)
        {
            return true;
        }

        var now = _dateTime.Now.TimeOfDay;
        return startTime > endTime ?
            now >= startTime || now <= endTime :
            now >= startTime && now <= endTime;
    }

    private async Task<IDictionary<string, (IEnumerable<string> Roles, Exception? Error)>?> AssignRolesAndBpn(Guid applicationId, IUserRolesRepository userRolesRepository, IApplicationRepository applicationRepository, string businessPartnerNumber)
    {
        var userBusinessPartnersRepository = _portalRepositories.GetInstance<IUserBusinessPartnerRepository>();

        var approvalInitialRoles = _settings.ApplicationApprovalInitialRoles;
        var initialRolesData = await GetRoleData(userRolesRepository, approvalInitialRoles).ConfigureAwait(ConfigureAwaitOptions.None);

        IDictionary<string, (IEnumerable<string> Roles, Exception? Error)>? assignedRoles = null;
        var invitedUsersData = applicationRepository
            .GetInvitedUsersDataByApplicationIdUntrackedAsync(applicationId);
        await foreach (var userData in invitedUsersData.ConfigureAwait(false))
        {
            var iamUserId = await _provisioningManager.GetUserByUserName(userData.CompanyUserId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None) ?? throw new ConflictException($"user {userData.CompanyUserId} not found in keycloak");

            assignedRoles = await _provisioningManager
                .AssignClientRolesToCentralUserAsync(iamUserId, approvalInitialRoles.ToDictionary(x => x.ClientId, x => x.UserRoleNames))
                .ToDictionaryAsync(assigned => assigned.Client, assigned => (assigned.Roles, assigned.Error))
                .ConfigureAwait(false);

            foreach (var roleData in initialRolesData.Where(roleData => !userData.RoleIds.Contains(roleData.UserRoleId) &&
                                                                        assignedRoles[roleData.ClientClientId].Roles.Contains(roleData.UserRoleText)))
            {
                userRolesRepository.CreateIdentityAssignedRole(userData.CompanyUserId, roleData.UserRoleId);
            }

            if (userData.BusinessPartnerNumbers.Contains(businessPartnerNumber))
                continue;

            userBusinessPartnersRepository.CreateCompanyUserAssignedBusinessPartner(userData.CompanyUserId, businessPartnerNumber);
            await _provisioningManager
                .AddBpnAttributetoUserAsync(iamUserId, Enumerable.Repeat(businessPartnerNumber, 1))
                .ConfigureAwait(ConfigureAwaitOptions.None);
        }

        return assignedRoles;
    }

    private async Task RemoveRegistrationRoles(Guid applicationId, IUserRolesRepository userRolesRepository)
    {
        var iamClientIds = _settings.ClientToRemoveRolesOnActivation;
        var clientRoleData = await userRolesRepository
            .GetUserRolesByClientId(iamClientIds)
            .ToListAsync()
            .ConfigureAwait(false);
        var invitedUsersData = userRolesRepository
            .GetUserWithUserRolesForApplicationId(applicationId, clientRoleData.SelectMany(data => data.UserRoles).Select(role => role.UserRoleId));

        var userRoles = clientRoleData.SelectMany(data => data.UserRoles.Select(role => (role.UserRoleId, data.ClientClientId, role.UserRoleText))).ToImmutableDictionary(x => x.UserRoleId, x => (x.ClientClientId, x.UserRoleText));

        await foreach (var userData in invitedUsersData.ConfigureAwait(false))
        {
            if (!userData.UserRoleIds.Any())
            {
                throw new UnexpectedConditionException("userRoleIds should never be empty here");
            }

            var iamUserId = await _provisioningManager.GetUserByUserName(userData.CompanyUserId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None) ?? throw new ConflictException($"user {userData.CompanyUserId} not found in keycloak");

            var roleNamesToDelete = userData.UserRoleIds
                .Select(roleId => userRoles[roleId])
                .GroupBy(clientRoleData => clientRoleData.ClientClientId)
                .ToImmutableDictionary(
                    clientRoleDataGroup => clientRoleDataGroup.Key,
                    clientRoleData => clientRoleData.Select(y => y.UserRoleText));

            await _provisioningManager.DeleteClientRolesFromCentralUserAsync(iamUserId, roleNamesToDelete)
                .ConfigureAwait(ConfigureAwaitOptions.None);
            userRolesRepository.DeleteCompanyUserAssignedRoles(userData.UserRoleIds.Select(roleId => (userData.CompanyUserId, roleId)));
        }
    }

    private async Task SetTheme(IEnumerable<string> iamIdpAliasse)
    {
        foreach (var alias in iamIdpAliasse)
        {
            await _provisioningManager.UpdateSharedRealmTheme(alias, _settings.LoginTheme).ConfigureAwait(false);
        }
    }

    private async Task PostRegistrationWelcomeEmailAsync(IApplicationRepository applicationRepository, Guid applicationId, string companyName, string businessPartnerNumber)
    {
        var failedUserNames = new List<string>();
        await foreach (var user in applicationRepository.GetEmailDataUntrackedAsync(applicationId).ConfigureAwait(false))
        {
            var userName = string.Join(" ", new[] { user.FirstName, user.LastName }.Where(item => !string.IsNullOrWhiteSpace(item)));
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                failedUserNames.Add(userName);
                continue;
            }

            var mailParameters = ImmutableDictionary.CreateRange(new[]
            {
                KeyValuePair.Create("userName", !string.IsNullOrWhiteSpace(userName) ? userName : user.Email),
                KeyValuePair.Create("companyName", companyName),
                KeyValuePair.Create("bpn", businessPartnerNumber),
                KeyValuePair.Create("homeUrl", _settings.PortalHomeAddress),
                KeyValuePair.Create("passwordResendUrl", _settings.PasswordResendAddress),
                KeyValuePair.Create("companyRolesParticipantUrl", _settings.CompanyRolesParticipantAddress),
                KeyValuePair.Create("dataspaceUrl", _settings.DataspaceAddress)
            });
            _mailingProcessCreation.CreateMailProcess(user.Email, "EmailRegistrationWelcomeTemplate", mailParameters);
        }

        if (failedUserNames.Any())
            throw new ConflictException($"user(s) {string.Join(",", failedUserNames)} has no assigned email");
    }

    private static async Task<IEnumerable<UserRoleData>> GetRoleData(IUserRolesRepository userRolesRepository, IEnumerable<UserRoleConfig> roles)
    {
        var roleData = await userRolesRepository
            .GetUserRoleDataUntrackedAsync(roles)
            .ToListAsync()
            .ConfigureAwait(false);
        if (roleData.Count < roles.Sum(clientRoles => clientRoles.UserRoleNames.Count()))
        {
            throw new ConfigurationException($"invalid configuration, at least one of the configured roles does not exist in the database: {string.Join(", ", roles.Select(clientRoles => $"client: {clientRoles.ClientId}, roles: [{string.Join(", ", clientRoles.UserRoleNames)}]"))}");
        }

        return roleData;
    }
}
