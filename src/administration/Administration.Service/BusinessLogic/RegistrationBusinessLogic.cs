/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class RegistrationBusinessLogic : IRegistrationBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly RegistrationSettings _settings;
    private readonly IMailingService _mailingService;
    private readonly IChecklistService _checklistService;
    private readonly IClearinghouseBusinessLogic _clearinghouseBusinessLogic;

    public RegistrationBusinessLogic(
        IPortalRepositories portalRepositories, 
        IOptions<RegistrationSettings> configuration, 
        IMailingService mailingService,
        IChecklistService checklistService,
        IClearinghouseBusinessLogic clearinghouseBusinessLogic)
    {
        _portalRepositories = portalRepositories;
        _settings = configuration.Value;
        _mailingService = mailingService;
        _checklistService = checklistService;
        _clearinghouseBusinessLogic = clearinghouseBusinessLogic;
    }

    public Task<CompanyWithAddressData> GetCompanyWithAddressAsync(Guid applicationId)
    {
        if (applicationId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(applicationId));
        }
        return GetCompanyWithAddressAsyncInternal(applicationId);
    }

    private async Task<CompanyWithAddressData> GetCompanyWithAddressAsyncInternal(Guid applicationId)
    {
        var companyWithAddress = await _portalRepositories.GetInstance<IApplicationRepository>().GetCompanyUserRoleWithAddressUntrackedAsync(applicationId).ConfigureAwait(false);
        if (companyWithAddress == null)
        {
            throw new NotFoundException($"applicationId {applicationId} not found");
        }
        return new CompanyWithAddressData(
            companyWithAddress.CompanyId,
            companyWithAddress.Name,
            companyWithAddress.Shortname ?? "",
            companyWithAddress.BusinessPartnerNumber ?? "",
            companyWithAddress.City ?? "",
            companyWithAddress.StreetName ?? "",
            companyWithAddress.CountryAlpha2Code ?? "",
            companyWithAddress.Region ?? "",
            companyWithAddress.Streetadditional ?? "",
            companyWithAddress.Streetnumber ?? "",
            companyWithAddress.Zipcode ?? "",
            companyWithAddress.CountryDe ?? "",
            companyWithAddress.AgreementsData
                .GroupBy(x => x.CompanyRoleId)
                .Select(g => new AgreementsRoleData(
                    g.Key,
                    g.Select(y => new AgreementConsentData(
                        y.AgreementId,
                        y.ConsentStatusId ?? ConsentStatusId.INACTIVE)))),
            companyWithAddress.InvitedCompanyUserData
                .Select(x => new InvitedUserData(
                    x.UserId,
                    x.FirstName ?? "",
                    x.LastName ?? "",
                    x.Email ?? "")),
            companyWithAddress.CompanyIdentifiers.Select(identifier => new IdentifierData(identifier.UniqueIdentifierId, identifier.Value))
        );
    }

    public Task<Pagination.Response<CompanyApplicationDetails>> GetCompanyApplicationDetailsAsync(int page, int size, CompanyApplicationStatusFilter? companyApplicationStatusFilter = null, string? companyName = null)
    {
        var applications = _portalRepositories.GetInstance<IApplicationRepository>()
            .GetCompanyApplicationsFilteredQuery(
                companyName?.Length >= 3 ? companyName : null,
                GetCompanyApplicationStatusIds(companyApplicationStatusFilter));

        return Pagination.CreateResponseAsync(
            page,
            size,
            _settings.ApplicationsMaxPageSize,
            (skip, take) => new Pagination.AsyncSource<CompanyApplicationDetails>(
                applications.CountAsync(),
                applications
                    .AsSplitQuery()
                    .OrderByDescending(application => application.DateCreated)
                    .Skip(skip)
                    .Take(take)
                    .Select(application => new CompanyApplicationDetails(
                        application.Id,
                        application.ApplicationStatusId,
                        application.DateCreated,
                        application.Company!.Name,
                        application.Invitations.SelectMany(invitation =>
                            invitation.CompanyUser!.Documents.Where(document => _settings.DocumentTypeIds.Contains(document.DocumentTypeId)).Select(document =>
                                new DocumentDetails(document.Id, document.DocumentTypeId))),
                        application.Company!.CompanyAssignedRoles.Select(companyAssignedRoles => companyAssignedRoles.CompanyRoleId),
                        application.ApplicationChecklistEntries.Select(x => new ApplicationChecklistEntryDetails(x.ApplicationChecklistEntryTypeId, x.ApplicationChecklistEntryStatusId)),
                        application.Invitations
                            .Select(invitation => invitation.CompanyUser)
                            .Where(companyUser => companyUser!.CompanyUserStatusId == CompanyUserStatusId.ACTIVE
                                && companyUser.Email != null)
                            .Select(companyUser => companyUser!.Email)
                            .FirstOrDefault(),
                        application.Company.BusinessPartnerNumber))
                    .AsAsyncEnumerable()));
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

    /// <inheritdoc />
    public Task UpdateCompanyBpn(Guid applicationId, string bpn)
    {
        var regex = new Regex(@"(\w|\d){16}", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
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
        var result = await _portalRepositories.GetInstance<IUserRepository>()
            .GetBpnForIamUserUntrackedAsync(applicationId, bpn).ToListAsync().ConfigureAwait(false);
        if (!result.Any(item => item.IsApplicationCompany))
        {
            throw new NotFoundException($"application {applicationId} not found");
        }

        if (result.Any(item => !item.IsApplicationCompany))
        {
            throw new ConflictException("BusinessPartnerNumber is already assigned to a different company");
        }

        var applicationCompanyData = result.Single(item => item.IsApplicationCompany);
        if (!applicationCompanyData.IsApplicationPending)
        {
            throw new ConflictException(
                $"application {applicationId} for company {applicationCompanyData.CompanyId} is not pending");
        }

        if (!string.IsNullOrWhiteSpace(applicationCompanyData.BusinessPartnerNumber))
        {
            throw new ConflictException(
                $"BusinessPartnerNumber of company {applicationCompanyData.CompanyId} has already been set.");
        }

        _portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyCompany(applicationCompanyData.CompanyId, null, 
            c => { c.BusinessPartnerNumber = bpn; });

        _portalRepositories.GetInstance<IApplicationChecklistRepository>()
            .AttachAndModifyApplicationChecklist(applicationId, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER,
                checklist => { checklist.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE; });
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ProcessClearinghouseResponseAsync(string bpn, ClearinghouseResponseData data, CancellationToken cancellationToken) => 
        _clearinghouseBusinessLogic.ProcessClearinghouseResponseAsync(bpn, data, cancellationToken);

    /// <inheritdoc />
    public async Task SetRegistrationVerification(Guid applicationId, bool approve, string? comment = null)
    {
        if (!approve && string.IsNullOrWhiteSpace(comment))
        {
            throw new ControllerArgumentException("Application is denied but no comment set.");
        }

        var result = await _portalRepositories.GetInstance<IApplicationRepository>()
            .GetApplicationStatusWithChecklistTypeStatusAsync(applicationId, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION)
            .ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"CompanyApplication {applicationId} does not exist.");
        }
        
        if (result.ApplicationStatusId != CompanyApplicationStatusId.SUBMITTED)
        {
            throw new ConflictException($"CompanyApplication {applicationId} is not in status SUBMITTED");
        }

        if (result.RegistrationVerificationStatusId == default)
        {
            throw new ConflictException($"No ChecklistEntry of type {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION} exists for application {applicationId}");
        }

        if (result.RegistrationVerificationStatusId != ApplicationChecklistEntryStatusId.TO_DO)
        {
            throw new ConflictException($"ChecklistEntry {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION} is not in state {ApplicationChecklistEntryStatusId.TO_DO}");
        }
        
        _portalRepositories.GetInstance<IApplicationChecklistRepository>().AttachAndModifyApplicationChecklist(applicationId, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION,
            entry =>
            {
                entry.ApplicationChecklistEntryStatusId = approve
                    ? ApplicationChecklistEntryStatusId.DONE
                    : ApplicationChecklistEntryStatusId.FAILED;
                entry.Comment = comment;
            });
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task TriggerBpnDataPushAsync(string iamUserId, Guid applicationId, CancellationToken cancellationToken)
    {
        await _checklistService.TriggerBpnDataPush(applicationId, iamUserId, cancellationToken).ConfigureAwait(false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private static IEnumerable<CompanyApplicationStatusId> GetCompanyApplicationStatusIds(CompanyApplicationStatusFilter? companyApplicationStatusFilter = null)
     {
        switch(companyApplicationStatusFilter)
        {
            case CompanyApplicationStatusFilter.Closed :
            {
                return new [] { CompanyApplicationStatusId.CONFIRMED, CompanyApplicationStatusId.DECLINED };
            }
            case CompanyApplicationStatusFilter.InReview :
            {
                return new [] { CompanyApplicationStatusId.SUBMITTED };  
            }
            default :
            {
                return new [] { CompanyApplicationStatusId.SUBMITTED, CompanyApplicationStatusId.CONFIRMED, CompanyApplicationStatusId.DECLINED };                 
            }
        }  
    }
}
