/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Org.Eclipse.TractusX.Portal.Backend.ApplicationActivation.Library;

public class ApplicationActivationService(
    IPortalRepositories portalRepositories,
    INotificationService notificationService,
    IProvisioningManager provisioningManager,
    IDateTimeProvider dateTime,
    ICustodianService custodianService,
    IMailingProcessCreation mailingProcessCreation,
    IOptions<ApplicationActivationSettings> options)
    : IApplicationActivationService
{
    private readonly ApplicationActivationSettings _settings = options.Value;

    public Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> StartApplicationActivation(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        if (!InProcessingTime())
        {
            return Task.FromResult(new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(ProcessStepStatusId.TODO, null, null, null, false, null));
        }

        var prerequisiteEntries = context.Checklist.Where(entry => entry.Key != ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION);
        if (prerequisiteEntries.Any(entry => entry.Value != ApplicationChecklistEntryStatusId.DONE && entry is not { Key: ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, Value: ApplicationChecklistEntryStatusId.SKIPPED }))
        {
            throw new ConflictException($"cannot activate application {context.ApplicationId}. Checklist entries that are not in status DONE: {string.Join(",", prerequisiteEntries)}");
        }

        return Task.FromResult(new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            entry =>
            {
                entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS;
            },
            Enumerable.Repeat(ProcessStepTypeId.ASSIGN_INITIAL_ROLES, 1),
            Enum.GetValues<ProcessStepTypeId>().Except(new[] { ProcessStepTypeId.START_APPLICATION_ACTIVATION }),
            true,
            null));
    }

    private bool InProcessingTime()
    {
        var startTime = _settings.StartTime;
        var endTime = _settings.EndTime;
        if (!startTime.HasValue || !endTime.HasValue)
        {
            return true;
        }

        var now = dateTime.Now.TimeOfDay;
        return startTime > endTime ?
            now >= startTime || now <= endTime :
            now >= startTime && now <= endTime;
    }

    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> AssignRoles(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        var userRolesRepository = portalRepositories.GetInstance<IUserRolesRepository>();
        var approvalInitialRoles = _settings.ApplicationApprovalInitialRoles;
        var initialRoles = await userRolesRepository
            .GetUserRoleDataUntrackedAsync(approvalInitialRoles)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (initialRoles.Count < approvalInitialRoles.Sum(clientRoles => clientRoles.UserRoleNames.Count()))
        {
            throw new ConfigurationException($"invalid configuration, at least one of the configured roles does not exist in the database: {string.Join(", ", approvalInitialRoles.Select(clientRoles => $"client: {clientRoles.ClientId}, roles: [{string.Join(", ", clientRoles.UserRoleNames)}]"))}");
        }

        var invitedUsersData = portalRepositories.GetInstance<IApplicationRepository>()
            .GetInvitedUsersWithoutInitialRoles(context.ApplicationId, initialRoles.Select(x => x.UserRoleId));
        await using var enumerator = invitedUsersData.GetAsyncEnumerator(cancellationToken);
        if (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            var userData = enumerator.Current;
            var iamUserId = await provisioningManager.GetUserByUserName(userData.CompanyUserId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None) ?? throw new ConflictException($"user {userData.CompanyUserId} not found in keycloak");

            var assignedRoles = await provisioningManager
                .AssignClientRolesToCentralUserAsync(iamUserId, approvalInitialRoles.ToDictionary(x => x.ClientId, x => x.UserRoleNames))
                .ToDictionaryAsync(assigned => assigned.Client, assigned => (assigned.Roles, assigned.Error), cancellationToken)
                .ConfigureAwait(false);

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

            initialRoles
                .ExceptBy(userData.RoleIds, x => x.UserRoleId)
                .IntersectBy(
                    assignedRoles.SelectMany(x => x.Value.Roles.Select(role => (x.Key, role))),
                    roleData => (roleData.ClientClientId, roleData.UserRoleText))
                .IfAny(unassigned => userRolesRepository.CreateIdentityAssignedRoleRange(unassigned.Select(roleData => (userData.CompanyUserId, roleData.UserRoleId))));

            var nextStepTypeIds = await enumerator.MoveNextAsync().ConfigureAwait(false)
                ? ProcessStepTypeId.ASSIGN_INITIAL_ROLES // in case there are further users eligible to add initial roles the same step is created again
                : ProcessStepTypeId.ASSIGN_BPN_TO_USERS;
            return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
                ProcessStepStatusId.DONE,
                null,
                Enumerable.Repeat(nextStepTypeIds, 1),
                null,
                true,
                null);
        }

        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            null,
            Enumerable.Repeat(ProcessStepTypeId.ASSIGN_BPN_TO_USERS, 1),
            null,
            true,
            null);
    }

    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> AssignBpn(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        var userBusinessPartnersRepository = portalRepositories.GetInstance<IUserBusinessPartnerRepository>();
        var applicationRepository = portalRepositories.GetInstance<IApplicationRepository>();
        var businessPartnerNumber = await applicationRepository
            .GetBpnForApplicationIdAsync(context.ApplicationId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (businessPartnerNumber is null)
        {
            throw new ConflictException("BusinessPartnerNumber must be set");
        }

        var invitedUsersData = applicationRepository
            .GetInvitedUserDataByApplicationWithoutBpn(context.ApplicationId);
        await using var enumerator = invitedUsersData.GetAsyncEnumerator(cancellationToken);
        if (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            var companyUserId = enumerator.Current;
            var iamUserId = await provisioningManager.GetUserByUserName(companyUserId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None) ?? throw new ConflictException($"user {companyUserId} not found in keycloak");

            userBusinessPartnersRepository.CreateCompanyUserAssignedBusinessPartner(companyUserId, businessPartnerNumber);
            await provisioningManager
                .AddBpnAttributetoUserAsync(iamUserId, Enumerable.Repeat(businessPartnerNumber, 1))
                .ConfigureAwait(ConfigureAwaitOptions.None);

            var nextStepTypeIds = await enumerator.MoveNextAsync().ConfigureAwait(false)
                ? ProcessStepTypeId.ASSIGN_BPN_TO_USERS // in case there are further users eligible to add the bpn the same step is created again
                : ProcessStepTypeId.REMOVE_REGISTRATION_ROLES;
            return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
                ProcessStepStatusId.DONE,
                null,
                Enumerable.Repeat(nextStepTypeIds, 1),
                null,
                true,
                null);
        }

        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            null,
            Enumerable.Repeat(ProcessStepTypeId.REMOVE_REGISTRATION_ROLES, 1),
            null,
            true,
            null);
    }

    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> RemoveRegistrationRoles(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        var iamClientIds = _settings.ClientToRemoveRolesOnActivation;
        var userRolesRepository = portalRepositories.GetInstance<IUserRolesRepository>();
        var invitedUsersData = userRolesRepository.GetUsersWithUserRolesForApplicationId(context.ApplicationId, iamClientIds);

        await using var enumerator = invitedUsersData.GetAsyncEnumerator(cancellationToken);
        if (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            var userData = enumerator.Current;
            if (!userData.InstanceRoleData.Any())
            {
                throw new UnexpectedConditionException("userRoleIds should never be empty here");
            }

            var roleNamesToDelete = userData.InstanceRoleData
                .GroupBy(clientRoleData => clientRoleData.ClientClientId)
                .ToImmutableDictionary(
                    clientRoleDataGroup => clientRoleDataGroup.Key,
                    clientRoleData => clientRoleData.Select(y => y.UserRoleText));

            var iamUserId =
                await provisioningManager.GetUserByUserName(userData.IdentityId.ToString())
                    .ConfigureAwait(ConfigureAwaitOptions.None) ??
                throw new ConflictException($"user {userData.IdentityId} not found in keycloak");

            await provisioningManager.DeleteClientRolesFromCentralUserAsync(iamUserId, roleNamesToDelete)
                .ConfigureAwait(ConfigureAwaitOptions.None);
            userRolesRepository.DeleteCompanyUserAssignedRoles(userData.InstanceRoleData.Select(roleId => (userData.IdentityId, roleId.UserRoleId)));

            var nextStepTypeIds = await enumerator.MoveNextAsync().ConfigureAwait(false)
                ? ProcessStepTypeId.REMOVE_REGISTRATION_ROLES // in case there are further users eligible to remove the roles from the same step is created again
                : ProcessStepTypeId.SET_THEME;
            return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
                ProcessStepStatusId.DONE,
                null,
                Enumerable.Repeat(nextStepTypeIds, 1),
                null,
                true,
                null);
        }

        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            null,
            Enumerable.Repeat(ProcessStepTypeId.SET_THEME, 1),
            null,
            true,
            null);
    }

    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> SetTheme(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        var iamIdpAliasse = await portalRepositories.GetInstance<IApplicationRepository>()
            .GetSharedIdpAliasseForApplicationId(context.ApplicationId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (var alias in iamIdpAliasse)
        {
            await provisioningManager.UpdateSharedRealmTheme(alias, _settings.LoginTheme).ConfigureAwait(false);
        }

        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            null,
            Enumerable.Repeat(_settings.UseDimWallet ? ProcessStepTypeId.FINISH_APPLICATION_ACTIVATION : ProcessStepTypeId.SET_MEMBERSHIP, 1),
            null,
            true,
            null);
    }

    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> SetMembership(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        var businessPartnerNumber = await portalRepositories.GetInstance<IApplicationRepository>()
            .GetBpnForApplicationIdAsync(context.ApplicationId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (businessPartnerNumber is null)
        {
            throw new ConflictException("BusinessPartnerNumber must be set");
        }

        var resultMessage = await custodianService.SetMembership(businessPartnerNumber, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            entry =>
            {
                entry.Comment = resultMessage;
            },
            Enumerable.Repeat(ProcessStepTypeId.FINISH_APPLICATION_ACTIVATION, 1),
            null,
            true,
            null);
    }

    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> SaveApplicationActivationToDatabase(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        var applicationRepository = portalRepositories.GetInstance<IApplicationRepository>();
        var result = await applicationRepository.GetCompanyAndApplicationDetailsForApprovalAsync(context.ApplicationId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw new ConflictException($"CompanyApplication {context.ApplicationId} is not in status SUBMITTED");
        }

        var (companyId, companyName, businessPartnerNumber, applicationTypeId, networkRegistrationProcessData) = result;
        if (string.IsNullOrWhiteSpace(businessPartnerNumber))
        {
            throw new ConflictException(
                $"BusinessPartnerNumber (bpn) for CompanyApplications {context.ApplicationId} company {companyId} is empty");
        }

        applicationRepository.AttachAndModifyCompanyApplication(context.ApplicationId, ca =>
        {
            ca.ApplicationStatusId = CompanyApplicationStatusId.CONFIRMED;
            ca.DateLastChanged = DateTimeOffset.UtcNow;
        });

        portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyCompany(companyId, null, c =>
        {
            c.CompanyStatusId = CompanyStatusId.ACTIVE;
        });

        if (applicationTypeId == CompanyApplicationTypeId.EXTERNAL)
        {
            if (networkRegistrationProcessData == null)
            {
                throw new ConflictException("ProcessId should be set for external applications");
            }
            var networkRegistrationContext = networkRegistrationProcessData.CreateManualProcessData(null, portalRepositories, () => "NetworkRegistration");
            networkRegistrationContext.ScheduleProcessSteps([ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED]);
            networkRegistrationContext.FinalizeProcessStep();
        }

        var notifications = _settings.WelcomeNotificationTypeIds.Select(x => (default(string), x));
        await notificationService.CreateNotifications(_settings.CompanyAdminRoles, null, notifications, companyId)
            .AwaitAll(cancellationToken).ConfigureAwait(false);
        var failedUserNames = await PostRegistrationWelcomeEmailAsync(context.ApplicationId, companyName, businessPartnerNumber, cancellationToken).ToListAsync().ConfigureAwait(false);
        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            entry =>
            {
                entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE;
            },
            null,
            null,
            true,
            failedUserNames.Count == 0
                ? null
                : $"user(s) {string.Join(",", failedUserNames)} had no assigned email");
    }

    private async IAsyncEnumerable<string> PostRegistrationWelcomeEmailAsync(Guid applicationId, string companyName, string businessPartnerNumber, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var user in portalRepositories.GetInstance<IApplicationRepository>().GetEmailDataUntrackedAsync(applicationId).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            var userName = string.Join(" ", new[] { user.FirstName, user.LastName }.Where(item => !string.IsNullOrWhiteSpace(item)));
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                yield return userName;
                continue;
            }

            var mailParameters = ImmutableDictionary.CreateRange<string, string>([
                new("userName", !string.IsNullOrWhiteSpace(userName) ? userName : user.Email),
                new("companyName", companyName),
                new("bpn", businessPartnerNumber),
                new("homeUrl", _settings.PortalHomeAddress),
                new("passwordResendUrl", _settings.PasswordResendAddress),
                new("companyRolesParticipantUrl", _settings.CompanyRolesParticipantAddress),
                new("dataspaceUrl", _settings.DataspaceAddress)
            ]);
            mailingProcessCreation.CreateMailProcess(user.Email, "EmailRegistrationWelcomeTemplate", mailParameters);
        }
    }
}
