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
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using System.Text.RegularExpressions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public sealed class RegistrationBusinessLogic : IRegistrationBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly RegistrationSettings _settings;
    private readonly IMailingService _mailingService;
    private readonly IApplicationChecklistService _checklistService;
    private readonly IClearinghouseBusinessLogic _clearinghouseBusinessLogic;
    private readonly ISdFactoryBusinessLogic _sdFactoryBusinessLogic;

    public RegistrationBusinessLogic(
        IPortalRepositories portalRepositories, 
        IOptions<RegistrationSettings> configuration, 
        IMailingService mailingService,
        IApplicationChecklistService checklistService,
        IClearinghouseBusinessLogic clearinghouseBusinessLogic,
        ISdFactoryBusinessLogic sdFactoryBusinessLogic)
    {
        _portalRepositories = portalRepositories;
        _settings = configuration.Value;
        _mailingService = mailingService;
        _checklistService = checklistService;
        _clearinghouseBusinessLogic = clearinghouseBusinessLogic;
        _sdFactoryBusinessLogic = sdFactoryBusinessLogic;
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
                        application.ApplicationChecklistEntries.Where(x => x.ApplicationChecklistEntryTypeId != ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION).OrderBy(x => x.ApplicationChecklistEntryTypeId).Select(x => new ApplicationChecklistEntryDetails(x.ApplicationChecklistEntryTypeId, x.ApplicationChecklistEntryStatusId)),
                        application.Invitations
                            .Select(invitation => invitation.CompanyUser)
                            .Where(companyUser => companyUser!.CompanyUserStatusId == CompanyUserStatusId.ACTIVE
                                && companyUser.Email != null)
                            .Select(companyUser => companyUser!.Email)
                            .FirstOrDefault(),
                        application.Company.BusinessPartnerNumber))
                    .AsAsyncEnumerable()));
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
        
        return UpdateCompanyBpnInternal(applicationId, bpn);
    }

    private async Task UpdateCompanyBpnInternal(Guid applicationId, string bpn)
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

        var context = await _checklistService
            .VerifyChecklistEntryAndProcessSteps(
                applicationId,
                ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER,
                new [] {
                    ApplicationChecklistEntryStatusId.TO_DO,
                    ApplicationChecklistEntryStatusId.IN_PROGRESS,
                    ApplicationChecklistEntryStatusId.FAILED
                },
                ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL,
                entryTypeIds: new [] {
                    ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION
                },
                processStepTypeIds: new [] {
                    ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH,
                    ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL,
                    ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL,
                    ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH,
                    ProcessStepTypeId.CREATE_IDENTITY_WALLET
                })
            .ConfigureAwait(false);

        _portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyCompany(applicationCompanyData.CompanyId, null, 
            c => { c.BusinessPartnerNumber = bpn; });

        var registrationValidationFailed = context.Checklist[ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION] == ApplicationChecklistEntryStatusId.FAILED;

        _checklistService.SkipProcessSteps(
            context,
            new [] {
                ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH,
                ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL,
                ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL,
                ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH
            });

        _checklistService.FinalizeChecklistEntryAndProcessSteps(
            context,
            entry => entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE,
            registrationValidationFailed
                ? null
                : new [] { ProcessStepTypeId.CREATE_IDENTITY_WALLET });

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ProcessClearinghouseResponseAsync(ClearinghouseResponseData data, CancellationToken cancellationToken)
    { 
        var result = await _portalRepositories.GetInstance<IApplicationRepository>().GetSubmittedApplicationIdsByBpn(data.BusinessPartnerNumber).ToListAsync(cancellationToken).ConfigureAwait(false);
        if (!result.Any())
        {
            throw new NotFoundException($"No companyApplication for BPN {data.BusinessPartnerNumber} is not in status SUBMITTED");
        }
        if (result.Count > 1)
        {
            throw new ConflictException($"more than one companyApplication in status SUBMITTED found for BPN {data.BusinessPartnerNumber} [{string.Join(", ",result)}]");
        }
        await _clearinghouseBusinessLogic.ProcessEndClearinghouse(result.Single(), data, cancellationToken).ConfigureAwait(false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChecklistDetails>> GetChecklistForApplicationAsync(Guid applicationId)
    {
        var data = await _portalRepositories.GetInstance<IApplicationRepository>()
            .GetApplicationChecklistData(applicationId, Enum.GetValues<ApplicationChecklistEntryTypeId>().GetManualTriggerProcessStepIds())
            .ConfigureAwait(false);
        if (data == default)
        {
            throw new NotFoundException($"Application {applicationId} does not exists");
        }

        return data.ChecklistData
            .OrderBy(x => x.TypeId)
            .Select(x =>
                new ChecklistDetails(
                    x.TypeId, 
                    x.StatusId, 
                    x.Comment,
                    data.ProcessStepTypeIds.Intersect(x.TypeId.GetManualTriggerProcessStepIds())));
    }

    /// <inheritdoc />
    public Task TriggerChecklistAsync(Guid applicationId, ApplicationChecklistEntryTypeId entryTypeId, ProcessStepTypeId processStepTypeId)
    {
        var possibleSteps = entryTypeId.GetManualTriggerProcessStepIds();
        if (!possibleSteps.Contains(processStepTypeId))
        {
            throw new ControllerArgumentException($"The processStep {processStepTypeId} is not retriggerable");
        }

        var nextStepData = processStepTypeId.GetNextProcessStepDataForManualTriggerProcessStepId();
        if (nextStepData == default)
        {
            throw new UnexpectedConditionException($"While the processStep {processStepTypeId} is configured to be retriggerable there is no next step configured");
        }
        return TriggerChecklistInternal(applicationId, entryTypeId, processStepTypeId, nextStepData.ProcessStepTypeId, nextStepData.ChecklistEntryStatusId);
    }
    private async Task TriggerChecklistInternal(Guid applicationId, ApplicationChecklistEntryTypeId entryTypeId, ProcessStepTypeId processStepTypeId, ProcessStepTypeId nextProcessStepTypeId, ApplicationChecklistEntryStatusId checklistEntryStatusId)
    {
        var context = await _checklistService
            .VerifyChecklistEntryAndProcessSteps(
                applicationId,
                entryTypeId,
                new [] { ApplicationChecklistEntryStatusId.FAILED },
                processStepTypeId,
                processStepTypeIds: new [] { nextProcessStepTypeId })
            .ConfigureAwait(false);

        _checklistService.FinalizeChecklistEntryAndProcessSteps(
            context,
            item =>
            {
                item.ApplicationChecklistEntryStatusId = checklistEntryStatusId;
            },
            new [] { nextProcessStepTypeId });
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ProcessClearinghouseSelfDescription(SelfDescriptionResponseData data, CancellationToken cancellationToken)
    {
        var result = await _portalRepositories.GetInstance<IApplicationRepository>()
            .GetCompanyIdSubmissionStatusForApplication(data.ExternalId)
            .ConfigureAwait(false);
        if (!result.IsValidApplicationId)
        {
            throw new NotFoundException($"companyApplication {data.ExternalId} not found");
        }
        if (!result.IsSubmitted)
        {
            throw new ConflictException($"companyApplication {data.ExternalId} is not in status SUBMITTED");
        }

        await _sdFactoryBusinessLogic.ProcessFinishSelfDescriptionLpForApplication(data, result.CompanyId, cancellationToken).ConfigureAwait(false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ApproveRegistrationVerification(Guid applicationId)
    {
        var context = await _checklistService
            .VerifyChecklistEntryAndProcessSteps(
                applicationId,
                ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION,
                new [] { ApplicationChecklistEntryStatusId.TO_DO },
                ProcessStepTypeId.VERIFY_REGISTRATION,
                new [] { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER },
                new [] { ProcessStepTypeId.CREATE_IDENTITY_WALLET })
            .ConfigureAwait(false);

        var businessPartnerSuccess = context.Checklist[ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER] == ApplicationChecklistEntryStatusId.DONE;

        _checklistService.FinalizeChecklistEntryAndProcessSteps(
            context,
            entry =>
            {
                entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE;
            },
            businessPartnerSuccess
                ? new [] { ProcessStepTypeId.CREATE_IDENTITY_WALLET }
                : null);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async Task DeclineRegistrationVerification(Guid applicationId, string comment)
    {
        var companyId = await _portalRepositories.GetInstance<IApplicationRepository>().GetCompanyIdForSubmittedApplication(applicationId).ConfigureAwait(false);
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException($"CompanyApplication {applicationId} is not in status SUBMITTED", nameof(applicationId));
        }

        var context = await _checklistService
            .VerifyChecklistEntryAndProcessSteps(
                applicationId,
                ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION,
                new [] { ApplicationChecklistEntryStatusId.TO_DO, ApplicationChecklistEntryStatusId.DONE },
                ProcessStepTypeId.DECLINE_APPLICATION,
                null,
                new [] { ProcessStepTypeId.VERIFY_REGISTRATION, })
            .ConfigureAwait(false);

        _checklistService.SkipProcessSteps(context, new [] { ProcessStepTypeId.VERIFY_REGISTRATION });

        _checklistService.FinalizeChecklistEntryAndProcessSteps(
            context,
            entry =>
            {
                entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.FAILED;
                entry.Comment = comment;
            },
            null);

        _portalRepositories.GetInstance<IApplicationRepository>().AttachAndModifyCompanyApplication(applicationId, application =>
        {
            application.ApplicationStatusId = CompanyApplicationStatusId.DECLINED;
            application.DateLastChanged = DateTimeOffset.UtcNow;
        });
        _portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyCompany(companyId, null, company =>
        {
            company.CompanyStatusId = CompanyStatusId.REJECTED;
        });

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        await PostRegistrationCancelEmailAsync(applicationId, comment).ConfigureAwait(false);
    }

    private async Task PostRegistrationCancelEmailAsync(Guid applicationId, string comment)
    {
        var userRoleIds = await _portalRepositories.GetInstance<IUserRolesRepository>()
            .GetUserRoleIdsUntrackedAsync(_settings.PartnerUserInitialRoles).ToListAsync().ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(comment))
        {
            throw new ConflictException("No comment set.");
        }
        
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
                { "companyName", user.CompanyName },
                { "declineComment", comment}
            };

            await _mailingService.SendMails(user.Email, mailParameters, new List<string> { "EmailRegistrationDeclineTemplate" }).ConfigureAwait(false);
        }
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
    
    /// <inheritdoc />
    public async Task<(string fileName, byte[] content, string contentType)> GetDocumentAsync(Guid documentId)
    {
        var document = await _portalRepositories.GetInstance<IDocumentRepository>()
            .GetDocumentByIdAsync(documentId)
            .ConfigureAwait(false);
        if (document == null)
        {
            throw new NotFoundException($"Document {documentId} does not exist");
        }

        return (document.DocumentName, document.DocumentContent, document.MediaTypeId.MapToMediaType());
    }
}
