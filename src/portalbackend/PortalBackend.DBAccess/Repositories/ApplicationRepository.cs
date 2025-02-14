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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class ApplicationRepository(PortalDbContext portalDbContext)
    : IApplicationRepository
{
    CompanyApplication IApplicationRepository.CreateCompanyApplication(Guid companyId, CompanyApplicationStatusId companyApplicationStatusId, CompanyApplicationTypeId applicationTypeId, Action<CompanyApplication>? setOptionalFields)
    {
        var companyApplication = new CompanyApplication(
            Guid.NewGuid(),
            companyId,
            companyApplicationStatusId,
            applicationTypeId,
            DateTimeOffset.UtcNow);
        setOptionalFields?.Invoke(companyApplication);
        return portalDbContext.CompanyApplications.Add(companyApplication).Entity;
    }

    public void AttachAndModifyCompanyApplication(Guid companyApplicationId, Action<CompanyApplication> setOptionalParameters)
    {
        var companyApplication = portalDbContext.Attach(new CompanyApplication(companyApplicationId, Guid.Empty, default, default, default)).Entity;
        setOptionalParameters.Invoke(companyApplication);
    }

    public void AttachAndModifyCompanyApplications(IEnumerable<(Guid companyApplicationId, Action<CompanyApplication>? Initialize, Action<CompanyApplication> Modify)> applicationData)
    {
        var initial = applicationData.Select(x =>
            {
                var companyApplication = new CompanyApplication(x.companyApplicationId, Guid.Empty, default, default, default);
                x.Initialize?.Invoke(companyApplication);
                return (CompanyApplication: companyApplication, x.Modify);
            }
        ).ToList();
        portalDbContext.AttachRange(initial.Select(x => x.CompanyApplication));
        initial.ForEach(x => x.Modify(x.CompanyApplication));
    }

    public Invitation CreateInvitation(Guid applicationId, Guid companyUserId) =>
        portalDbContext.Invitations.Add(
            new Invitation(
                Guid.NewGuid(),
                applicationId,
                companyUserId,
                InvitationStatusId.CREATED,
                DateTimeOffset.UtcNow)).Entity;

    public void DeleteInvitations(IEnumerable<Guid> invitationIds) =>
        portalDbContext.Invitations.RemoveRange(
            invitationIds.Select(
                invitationId => new Invitation(
                    invitationId,
                    Guid.Empty,
                    Guid.Empty,
                    default,
                    default)));

    public Task<(bool Exists, CompanyApplicationStatusId StatusId)> GetOwnCompanyApplicationUserDataAsync(Guid applicationId, Guid userCompanyId) =>
        portalDbContext.CompanyApplications
            .Where(application => application.Id == applicationId && application.CompanyId == userCompanyId)
            .Select(application => new ValueTuple<bool, CompanyApplicationStatusId>(true, application.ApplicationStatusId))
            .SingleOrDefaultAsync();

    public Task<(bool Exists, bool IsUserOfCompany, CompanyApplicationStatusId ApplicationStatus)> GetOwnCompanyApplicationStatusUserDataUntrackedAsync(Guid applicationId, Guid companyId) =>
        portalDbContext.CompanyApplications
            .AsNoTracking()
            .Where(application => application.Id == applicationId)
            .Select(application => new ValueTuple<bool, bool, CompanyApplicationStatusId>(true, application.CompanyId == companyId, application.ApplicationStatusId))
            .SingleOrDefaultAsync();

    public Task<CompanyApplicationUserEmailData?> GetOwnCompanyApplicationUserEmailDataAsync(Guid applicationId, Guid companyUserId, IEnumerable<DocumentTypeId> submitDocumentTypeIds) =>
        portalDbContext.CompanyApplications
            .AsSplitQuery()
            .AsNoTracking()
            .Where(application => application.Id == applicationId)
            .Select(application => new
            {
                Application = application,
                CompanyUser = application.Company!.Identities.Select(x => x.CompanyUser!).SingleOrDefault(companyUser => companyUser.Id == companyUserId),
                Documents = application.Company.Identities.Select(x => x.CompanyUser!).SelectMany(companyUser => companyUser.Documents).Where(doc => doc.DocumentStatusId != DocumentStatusId.LOCKED && submitDocumentTypeIds.Contains(doc.DocumentTypeId)),
                Company = application.Company,
                Consents = application.Company.Consents.Where(consent => consent.ConsentStatusId == ConsentStatusId.ACTIVE)
            })
            .Select(data => new CompanyApplicationUserEmailData(
                data.Application.ApplicationStatusId,
                data.CompanyUser != null,
                data.CompanyUser!.Email,
                data.Documents.Select(doc =>
                    new DocumentStatusData(
                        doc.Id,
                        doc.DocumentStatusId)),
                new CompanyData(
                    data.Company!.Name,
                    data.Company.AddressId,
                    data.Company.Address!.Streetname,
                    data.Company.Address.City,
                    data.Company.Address.Country!.CountryLongNames.Where(cln => cln.ShortName == "de").Select(cln => cln.LongName).SingleOrDefault(),
                    data.Company.CompanyIdentifiers.Select(x => x.UniqueIdentifierId),
                    data.Company.CompanyAssignedRoles.Select(companyAssignedRole => companyAssignedRole.CompanyRoleId)),
                data.Consents.Select(consent =>
                    new ValueTuple<Guid, ConsentStatusId>(
                        consent.AgreementId,
                        consent.ConsentStatusId))
                ))
            .SingleOrDefaultAsync();

    public IQueryable<CompanyApplication> GetCompanyApplicationsFilteredQuery(string? companyName, IEnumerable<CompanyApplicationStatusId> applicationStatusIds) =>
        portalDbContext.CompanyApplications.AsNoTracking()
            .Where(application =>
                (companyName == null || EF.Functions.ILike(application.Company!.Name, $"{companyName.EscapeForILike()}%")) &&
                (applicationStatusIds == null || applicationStatusIds.Contains(application.ApplicationStatusId)));

    public IQueryable<CompanyApplication> GetExternalCompanyApplicationsFilteredQuery(Guid onboardingServiceProviderId, string? companyName, string? externalId, IEnumerable<CompanyApplicationStatusId> applicationStatusIds) =>
        portalDbContext.CompanyApplications.AsNoTracking()
            .Where(application =>
                application.CompanyApplicationTypeId == CompanyApplicationTypeId.EXTERNAL &&
                application.OnboardingServiceProviderId == onboardingServiceProviderId &&
                (companyName == null || EF.Functions.ILike(application.Company!.Name, $"{companyName.EscapeForILike()}%")) &&
                (externalId == null || application.NetworkRegistration!.ExternalId == externalId) &&
                applicationStatusIds.Contains(application.ApplicationStatusId));

    public Task<CompanyApplicationDetailData?> GetCompanyApplicationDetailDataAsync(Guid applicationId, Guid userCompanyId, Guid? companyId) =>
        portalDbContext.CompanyApplications
            .AsNoTracking()
            .Where(application => application.Id == applicationId &&
                (companyId == null || application.CompanyId == companyId))
            .Select(application => new CompanyApplicationDetailData(
                application.ApplicationStatusId,
                application.Company!.Id,
                application.Company.Name,
                application.Company.Shortname,
                application.Company.BusinessPartnerNumber,
                application.Company.CompanyStatusId,
                application.Company.AddressId,
                application.Company.Address!.Streetname,
                application.Company.Address.Streetadditional,
                application.Company.Address.Streetnumber,
                application.Company.Address.Zipcode,
                application.Company.Address.City,
                application.Company.Address.Region,
                application.Company.Address.CountryAlpha2Code,
                application.CompanyId == userCompanyId,
                application.Company.CompanyIdentifiers
                    .Select(identifier => new ValueTuple<UniqueIdentifierId, string>(identifier.UniqueIdentifierId, identifier.Value))))
            .SingleOrDefaultAsync();

    public Task<(string CompanyName, string? FirstName, string? LastName, string? Email, IEnumerable<(Guid ApplicationId, CompanyApplicationStatusId ApplicationStatusId, IEnumerable<(string? FirstName, string? LastName, string? Email)> InvitedUsers)> Applications)> GetCompanyApplicationsDeclineData(Guid companyUserId, IEnumerable<CompanyApplicationStatusId> applicationStatusIds) =>
        portalDbContext.CompanyUsers
            .AsNoTracking()
            .AsSplitQuery()
            .Where(user =>
                user.Id == companyUserId &&
                user.Identity!.UserStatusId == UserStatusId.ACTIVE
                )
            .Select(user => new ValueTuple<string, string?, string?, string?, IEnumerable<(Guid, CompanyApplicationStatusId, IEnumerable<(string?, string?, string?)>)>>(
                user.Identity!.Company!.Name,
                user.Firstname,
                user.Lastname,
                user.Email,
                user.Identity.Company.CompanyApplications.Where(application => applicationStatusIds.Contains(application.ApplicationStatusId)).Select(application => new ValueTuple<Guid, CompanyApplicationStatusId, IEnumerable<(string?, string?, string?)>>(
                    application.Id,
                    application.ApplicationStatusId,
                    application.Invitations
                        .Where(invitation => invitation.CompanyUser!.Identity!.UserStatusId == UserStatusId.ACTIVE)
                        .Select(invitation => new ValueTuple<string?, string?, string?>(
                            invitation.CompanyUser!.Firstname,
                            invitation.CompanyUser.Lastname,
                            invitation.CompanyUser.Email
                    ))
                ))
            ))
            .SingleOrDefaultAsync();

    public Task<(bool IsValidApplicationId, Guid CompanyId, bool IsSubmitted)> GetCompanyIdSubmissionStatusForApplication(Guid applicationId) =>
        portalDbContext.CompanyApplications
            .AsNoTracking()
            .Where(companyApplication => companyApplication.Id == applicationId)
            .Select(x => new ValueTuple<bool, Guid, bool>(
                true,
                x.CompanyId,
                x.ApplicationStatusId == CompanyApplicationStatusId.SUBMITTED))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(Guid CompanyId, string CompanyName, string? BusinessPartnerNumber, CompanyApplicationTypeId ApplicationTypeId, VerifyProcessData<ProcessTypeId, ProcessStepTypeId>? NetworkRegistrationProcessData)> GetCompanyAndApplicationDetailsForApprovalAsync(Guid applicationId) =>
        portalDbContext.CompanyApplications
            .AsNoTracking()
            .Where(companyApplication =>
                companyApplication.Id == applicationId &&
                companyApplication.ApplicationStatusId == CompanyApplicationStatusId.SUBMITTED)
            .Select(ca => new ValueTuple<Guid, string, string?, CompanyApplicationTypeId, VerifyProcessData<ProcessTypeId, ProcessStepTypeId>?>(
                ca.CompanyId,
                ca.Company!.Name,
                ca.Company.BusinessPartnerNumber,
                ca.CompanyApplicationTypeId,
                ca.CompanyApplicationTypeId == CompanyApplicationTypeId.EXTERNAL
                    ? new(ca.Company.NetworkRegistration!.Process, ca.Company.NetworkRegistration!.Process!.ProcessSteps.Where(ps => ps.ProcessStepStatusId == ProcessStepStatusId.TODO))
                    : null))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(Guid CompanyId, string legalName, string? BusinessPartnerNumber, string? Alpha2Code, string? Region, IEnumerable<(UniqueIdentifierId Id, string Value)> UniqueIdentifiers)> GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(Guid applicationId) =>
        portalDbContext.CompanyApplications.Where(companyApplication =>
                companyApplication.Id == applicationId &&
                companyApplication.ApplicationStatusId == CompanyApplicationStatusId.SUBMITTED)
            .Select(ca => new ValueTuple<Guid, string, string?, string?, string?, IEnumerable<(UniqueIdentifierId Id, string Value)>>(
                ca.CompanyId,
                ca.Company!.Name,
                ca.Company!.BusinessPartnerNumber,
                ca.Company.Address!.Country!.Alpha2Code,
                ca.Company.Address!.Region,
                ca.Company.CompanyIdentifiers.Select(x => new ValueTuple<UniqueIdentifierId, string>(x.UniqueIdentifierId, x.Value))))
            .SingleOrDefaultAsync();

    public Task<(Guid CompanyId, string CompanyName, string? BusinessPartnerNumber)> GetCompanyAndApplicationDetailsForCreateWalletAsync(Guid applicationId) =>
        portalDbContext.CompanyApplications.Where(companyApplication =>
                companyApplication.Id == applicationId)
            .Select(ca => new ValueTuple<Guid, string, string?>(
                ca.CompanyId,
                ca.Company!.Name,
                ca.Company.BusinessPartnerNumber))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<CompanyInvitedUserData> GetInvitedUsersWithoutInitialRoles(Guid applicationId, IEnumerable<Guid> userRoleIds) =>
        portalDbContext.Invitations
            .Where(invitation => invitation.CompanyApplicationId == applicationId)
            .Select(invitation => invitation.CompanyUser)
            .Where(companyUser =>
                companyUser!.Identity!.UserStatusId == UserStatusId.ACTIVE &&
                userRoleIds.Any(x => !companyUser.Identity.IdentityAssignedRoles.Any(iar => iar.UserRoleId == x)))
            .Select(companyUser => new CompanyInvitedUserData(
                companyUser!.Id,
                companyUser.Identity!.IdentityAssignedRoles.Select(companyUserAssignedRole => companyUserAssignedRole.UserRoleId)))
            .Take(2)
            .AsAsyncEnumerable();

    public IAsyncEnumerable<Guid> GetInvitedUserDataByApplicationWithoutBpn(Guid applicationId) =>
        portalDbContext.Invitations
            .Where(invitation => invitation.CompanyApplicationId == applicationId)
            .Select(invitation => new
            {
                invitation.CompanyUser,
                invitation.CompanyApplication!.Company!.BusinessPartnerNumber
            })
            .Where(x =>
                x.CompanyUser!.Identity!.UserStatusId == UserStatusId.ACTIVE &&
                !x.CompanyUser.CompanyUserAssignedBusinessPartners.Any(cuabp => cuabp.BusinessPartnerNumber == x.BusinessPartnerNumber))
            .Select(x => x.CompanyUser!.Id)
            .Take(2)
            .AsAsyncEnumerable();

    public IAsyncEnumerable<EmailData> GetEmailDataUntrackedAsync(Guid applicationId) =>
        portalDbContext.CompanyApplications
            .Where(application => application.Id == applicationId)
            .SelectMany(application =>
                application.Company!.Identities.Where(x => x.IdentityTypeId == IdentityTypeId.COMPANY_USER).Select(x => x.CompanyUser!)
                    .Where(companyUser => companyUser.Identity!.UserStatusId == UserStatusId.ACTIVE)
                    .Select(companyUser => new EmailData(
                        companyUser.Id,
                        companyUser.Firstname,
                        companyUser.Lastname,
                        companyUser.Email)))
            .AsAsyncEnumerable();

    public IQueryable<CompanyApplication> GetAllCompanyApplicationsDetailsQuery(string? companyName = null) =>
        portalDbContext.CompanyApplications
            .AsNoTracking()
            .Where(application => companyName == null || EF.Functions.ILike(application.Company!.Name, $"%{companyName.EscapeForILike()}%"));

    public Task<CompanyUserRoleWithAddress?> GetCompanyUserRoleWithAddressUntrackedAsync(Guid companyApplicationId, IEnumerable<DocumentTypeId> documentTypeIds) =>
        portalDbContext.CompanyApplications
            .AsSplitQuery()
            .Where(companyApplication => companyApplication.Id == companyApplicationId)
            .Select(
                companyApplication => new CompanyUserRoleWithAddress(
                    companyApplication.CompanyId,
                    companyApplication.Company!.Name,
                    companyApplication.Company.Shortname,
                    companyApplication.Company.BusinessPartnerNumber,
                    companyApplication.Company.Address!.City,
                    companyApplication.Company.Address.Streetname,
                    companyApplication.Company.Address.CountryAlpha2Code,
                    companyApplication.Company.Address.Region,
                    companyApplication.Company.Address.Streetadditional,
                    companyApplication.Company.Address.Streetnumber,
                    companyApplication.Company.Address.Zipcode,
                    companyApplication.Company.Address.Country!.CountryLongNames.Where(cln => cln.ShortName == "de").Select(cln => cln.LongName).SingleOrDefault(),
                    companyApplication.Company.CompanyAssignedRoles.SelectMany(assigned =>
                        assigned.CompanyRole!.AgreementAssignedCompanyRoles.Select(x =>
                            new AgreementsData(
                                x.CompanyRoleId,
                                x.AgreementId,
                                x.Agreement!.Consents.SingleOrDefault(consent => consent.CompanyId == companyApplication.CompanyId)!.ConsentStatusId))),
                    companyApplication.Invitations.Select(x =>
                        new InvitedCompanyUserData(
                            x.CompanyUserId,
                            x.CompanyUser!.Firstname,
                            x.CompanyUser.Lastname,
                            x.CompanyUser.Email)),
                    companyApplication.Company.CompanyIdentifiers.Select(identifier => new ValueTuple<UniqueIdentifierId, string>(identifier.UniqueIdentifierId, identifier.Value)),
                    companyApplication.Invitations.SelectMany(invitation =>
                            invitation.CompanyUser!.Documents.Where(document => documentTypeIds.Contains(document.DocumentTypeId)).Select(document =>
                                new ValueTuple<Guid, DocumentTypeId>(document.Id, document.DocumentTypeId))),
                    companyApplication.DateCreated,
                    companyApplication.DateLastChanged))
            .AsNoTracking()
            .SingleOrDefaultAsync();

    public Task<(bool IsValidApplicationId, bool IsValidCompany, RegistrationData? Data)> GetRegistrationDataUntrackedAsync(Guid applicationId, Guid userCompanyId, IEnumerable<DocumentTypeId> documentTypes) =>
        portalDbContext.CompanyApplications
            .AsNoTracking()
            .AsSplitQuery()
            .Where(application =>
                application.Id == applicationId)
            .Select(application => new
            {
                IsSameCompanyUser = application.CompanyId == userCompanyId,
                Company = application.Company
            })
            .Select(x => new ValueTuple<bool, bool, RegistrationData?>(
                true,
                x.IsSameCompanyUser,
                x.IsSameCompanyUser ? new RegistrationData(
                    x.Company!.Id,
                    x.Company.Name,
                    x.Company.BusinessPartnerNumber,
                    x.Company.Shortname,
                    x.Company.Address!.City,
                    x.Company.Address.Region,
                    x.Company.Address.Streetadditional,
                    x.Company.Address.Streetname,
                    x.Company.Address.Streetnumber,
                    x.Company.Address.Zipcode,
                    x.Company.Address.CountryAlpha2Code,
                    x.Company.CompanyAssignedRoles.Select(companyAssignedRole => companyAssignedRole.CompanyRoleId),
                    x.Company.Consents.Where(consent => consent.ConsentStatusId == ConsentStatusId.ACTIVE)
                        .Select(consent => new ValueTuple<Guid, ConsentStatusId>(
                            consent.AgreementId, consent.ConsentStatusId)),
                    x.Company.Identities.Where(i => i.IdentityTypeId == IdentityTypeId.COMPANY_USER).Select(i => i.CompanyUser!).SelectMany(companyUser => companyUser.Documents.Where(document => documentTypes.Contains(document.DocumentTypeId)).Select(document => document.DocumentName)),
                    x.Company.CompanyIdentifiers.Select(identifier => new ValueTuple<UniqueIdentifierId, string>(identifier.UniqueIdentifierId, identifier.Value)))
                    : null))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(string? Bpn, IEnumerable<ApplicationChecklistEntryTypeId> ExistingChecklistEntryTypeIds)> GetBpnAndChecklistCheckForApplicationIdAsync(Guid applicationId) =>
        portalDbContext.CompanyApplications
            .AsNoTracking()
            .Where(a => a.Id == applicationId)
            .Select(x => new ValueTuple<string?, IEnumerable<ApplicationChecklistEntryTypeId>>(
                x.Company!.BusinessPartnerNumber,
                x.ApplicationChecklistEntries.Select(ace => ace.ApplicationChecklistEntryTypeId)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(CompanyApplicationStatusId ApplicationStatusId, ApplicationChecklistEntryStatusId RegistrationVerificationStatusId)> GetApplicationStatusWithChecklistTypeStatusAsync(Guid applicationId, ApplicationChecklistEntryTypeId checklistEntryTypeId) =>
        portalDbContext.CompanyApplications
            .AsNoTracking()
            .Where(ca => ca.Id == applicationId)
            .Select(ca => new ValueTuple<CompanyApplicationStatusId, ApplicationChecklistEntryStatusId>(
                ca.ApplicationStatusId,
                ca.ApplicationChecklistEntries
                    .Where(x => x.ApplicationChecklistEntryTypeId == checklistEntryTypeId)
                    .Select(x => x.ApplicationChecklistEntryStatusId)
                    .SingleOrDefault()))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<string?> GetBpnForApplicationIdAsync(Guid applicationId) =>
        portalDbContext.CompanyApplications.AsNoTracking()
            .Where(ca => ca.Id == applicationId)
            .Select(x => x.Company!.BusinessPartnerNumber)
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<ClearinghouseData?> GetClearinghouseDataForApplicationId(Guid applicationId) =>
        portalDbContext.CompanyApplications
            .AsNoTracking()
            .Where(ca => ca.Id == applicationId)
            .Select(ca => new { ca.ApplicationStatusId, ca.Company, ca.Company!.Address, ca.Company.CompanyIdentifiers })
            .Select(ca => new ClearinghouseData(
                ca.ApplicationStatusId,
                ca.Company!.BusinessPartnerNumber,
                ca.Company.Name,
                ca.Address == null
                    ? null
                    : new ClearinghouseAddressData(
                        ca.Address.CountryAlpha2Code,
                        ca.Address.Region,
                        ca.Address.City,
                        ca.Address.Zipcode,
                        ca.Address.Streetnumber,
                        ca.Address.Streetname
                    ),
                ca.CompanyIdentifiers.Select(ci => new CompanyUniqueIdentifier(ci.UniqueIdentifierId, ci.Value))
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetSubmittedApplicationIdsByBpn(string bpn) =>
        portalDbContext.CompanyApplications
            .AsNoTracking()
            .Where(ca =>
                ca.Company!.BusinessPartnerNumber == bpn &&
                ca.ApplicationStatusId == CompanyApplicationStatusId.SUBMITTED)
            .Select(ca => ca.Id)
            .AsAsyncEnumerable();

    public Task<(Guid CompanyId, BpdmData BpdmData)> GetBpdmDataForApplicationAsync(Guid applicationId) =>
        portalDbContext.Companies.AsNoTracking()
            .Where(company => company.CompanyApplications.Any(application => application.Id == applicationId))
            .Select(company => new ValueTuple<Guid, BpdmData>(
                company.Id,
                new BpdmData(
                    company!.Name,
                    company.Shortname,
                    company.BusinessPartnerNumber,
                    company.Address!.CountryAlpha2Code,
                    company.Address.Zipcode,
                    company.Address.City,
                    company.Address.Streetname,
                    company.Address.Streetnumber,
                    company.Address.Region,
                    company.CompanyIdentifiers.Join(
                        company.Address.Country!.CountryAssignedIdentifiers.Where(cai => cai.BpdmIdentifierId != null),
                        companyIdentifier => companyIdentifier.UniqueIdentifierId,
                        countryAssignedIdentifier => countryAssignedIdentifier.UniqueIdentifierId,
                        (companyIdentifier, countryAssignedIdentifier) => new ValueTuple<BpdmIdentifierId, string>(countryAssignedIdentifier.BpdmIdentifierId!.Value, companyIdentifier.Value)))))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool Exists, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId, string? Comment)> ChecklistData, IEnumerable<ProcessStepTypeId> ProcessStepTypeIds)> GetApplicationChecklistData(Guid applicationId, IEnumerable<ProcessStepTypeId> processStepTypeIds) =>
        portalDbContext.CompanyApplications
            .AsSplitQuery()
            .Where(x => x.Id == applicationId)
            .Select(x => new
            {
                x.ApplicationChecklistEntries,
                ProcessSteps = x.ChecklistProcess!.ProcessSteps
                    .Where(step =>
                        step.ProcessStepStatusId == ProcessStepStatusId.TODO &&
                        processStepTypeIds.Contains(step.ProcessStepTypeId))
            })
            .Select(x => new ValueTuple<bool, IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId, string?)>, IEnumerable<ProcessStepTypeId>>(
                    true,
                    x.ApplicationChecklistEntries
                        .Where(ace => ace.ApplicationChecklistEntryTypeId != ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION)
                        .Select(ace => new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId, string?>(
                            ace.ApplicationChecklistEntryTypeId,
                            ace.ApplicationChecklistEntryStatusId,
                            ace.Comment)),
                    x.ProcessSteps
                        .Select(ps => ps.ProcessStepTypeId)))
            .SingleOrDefaultAsync();

    /// <summary>
    /// Gets the company id for the submitted application
    /// </summary>
    /// <param name="applicationId">Id of the application</param>
    /// <returns>Returns the company id</returns>
    public Task<(Guid CompanyId, string CompanyName, Guid? NetworkRegistrationProcessId, IEnumerable<(Guid IdentityProviderId, string IamAlias, IdentityProviderTypeId TypeId, IEnumerable<Guid> LinkedCompanyUserIds)> Idps, IEnumerable<Guid> CompanyUserIds)> GetCompanyIdNameForSubmittedApplication(Guid applicationId) =>
        portalDbContext.CompanyApplications
            .AsSplitQuery()
            .Where(x => x.Id == applicationId && x.ApplicationStatusId == CompanyApplicationStatusId.SUBMITTED)
            .Select(x => new ValueTuple<Guid, string, Guid?, IEnumerable<(Guid, string, IdentityProviderTypeId, IEnumerable<Guid>)>, IEnumerable<Guid>>(
                x.CompanyId,
                x.Company!.Name,
                x.Company.NetworkRegistration!.ProcessId,
                x.Company.IdentityProviders.Select(idp => new ValueTuple<Guid, string, IdentityProviderTypeId, IEnumerable<Guid>>(
                    idp.Id,
                    idp.IamIdentityProvider!.IamIdpAlias,
                    idp.IdentityProviderTypeId,
                    idp.CompanyUserAssignedIdentityProviders.Select(assigned => assigned.CompanyUserId))),
                x.Company.Identities
                    .Where(i =>
                        i.IdentityTypeId == IdentityTypeId.COMPANY_USER &&
                        i.UserStatusId != UserStatusId.DELETED)
                    .Select(i => i.Id)))
            .SingleOrDefaultAsync();

    public Task<bool> IsValidApplicationForCompany(Guid applicationId, Guid companyId) =>
        portalDbContext.CompanyApplications
            .AnyAsync(application => application.Id == applicationId && application.CompanyId == companyId);

    public Task<(bool Exists, string? Did, IEnumerable<DateTimeOffset> ProcessStepsDateCreated)> GetDidApplicationId(Guid applicationId) =>
        portalDbContext.CompanyApplications
            .Where(ca => ca.Id == applicationId)
            .Select(x => new ValueTuple<bool, string?, IEnumerable<DateTimeOffset>>(
                true,
                x.Company!.CompanyWalletData!.Did,
                x.ChecklistProcess!.ProcessSteps
                    .Where(ps =>
                        ps.ProcessStepTypeId == ProcessStepTypeId.VALIDATE_DID_DOCUMENT &&
                        ps.ProcessStepStatusId == ProcessStepStatusId.TODO)
                    .Select(ps => ps.DateCreated)))
            .SingleOrDefaultAsync();

    public Task<(bool Exists, string? Holder, string? BusinessPartnerNumber, WalletInformation? WalletInformation)> GetBpnlCredentialIformationByApplicationId(Guid applicationId) =>
        portalDbContext.CompanyApplications
            .Where(ca => ca.Id == applicationId)
            .Select(ca => new
            {
                Company = ca.Company!,
                Wallet = ca.Company!.CompanyWalletData
            })
            .Select(c => new ValueTuple<bool, string?, string?, WalletInformation?>(
                true,
                c.Company.DidDocumentLocation,
                c.Company.BusinessPartnerNumber,
                c.Wallet == null ?
                    null :
                    new WalletInformation(
                        c.Wallet.ClientId,
                        c.Wallet.ClientSecret,
                        c.Wallet.InitializationVector,
                        c.Wallet.EncryptionMode,
                        c.Wallet.AuthenticationServiceUrl
                    )))
            .SingleOrDefaultAsync();

    public Task<(bool Exists, string? Did, string? Bpn)> GetDidAndBpnForApplicationId(Guid applicationId) =>
        portalDbContext.CompanyApplications
            .Where(ca => ca.Id == applicationId)
            .Select(ca => new ValueTuple<bool, string?, string?>(
                true,
                ca.Company!.CompanyWalletData!.Did,
                ca.Company.BusinessPartnerNumber))
            .SingleOrDefaultAsync();

    public Task<(bool Exists, string? Did)> GetDidForApplicationId(Guid applicationId) =>
        portalDbContext.CompanyApplications
            .Where(ca => ca.Id == applicationId)
            .Select(ca => new ValueTuple<bool, string?>(
                true,
                ca.Company!.CompanyWalletData!.Did))
            .SingleOrDefaultAsync();

    public Task<(bool IsValidApplicationId, bool IsValidCompany, ApplicationDeclineData? ApplicationDeclineData)> GetDeclineApplicationDataForApplicationId(Guid applicationId, Guid companyId, IEnumerable<CompanyApplicationStatusId> companyApplicationStatusIds) =>
        portalDbContext.CompanyApplications
            .AsNoTracking()
            .AsSplitQuery()
            .Where(application => application.Id == applicationId && companyApplicationStatusIds.Contains(application.ApplicationStatusId))
            .Select(application => new
            {
                IsValidCompany = application.CompanyId == companyId,
                Application = application,
                application.Company,
                application.Company!.Identities
            })
            .Select(x => new ValueTuple<bool, bool, ApplicationDeclineData?>(
                true,
                x.IsValidCompany,
                x.IsValidCompany
                    ? new ApplicationDeclineData(
                        x.Company!.IdentityProviders
                            .Select(x => new IdentityProviderStatusData(
                                x.Id,
                                x.IdentityProviderTypeId)),
                        x.Company.Name,
                        x.Application.ApplicationStatusId,
                        x.Application.Invitations
                            .Where(invitation => invitation.InvitationStatusId != InvitationStatusId.DECLINED)
                            .Select(invitation => new InvitationsStatusData(
                                invitation.Id,
                                invitation.InvitationStatusId)),
                        x.Identities
                            .Where(identity =>
                                identity.IdentityTypeId == IdentityTypeId.COMPANY_USER &&
                                identity.UserStatusId != UserStatusId.DELETED)
                            .Select(identity => new CompanyUserStatusData(
                                identity.Id,
                                identity.CompanyUser!.Firstname,
                                identity.CompanyUser.Lastname,
                                identity.CompanyUser.Email,
                                identity.UserStatusId,
                                identity.IdentityAssignedRoles.Select(iar => iar.UserRoleId))),
                        x.Identities.SelectMany(identity =>
                            identity.CompanyUser!.Documents
                                .Where(document => document.DocumentStatusId != DocumentStatusId.INACTIVE)
                                .Select(document => new DocumentStatusData(
                                    document.Id,
                                    document.DocumentStatusId))))
                    : null))
            .SingleOrDefaultAsync();

    public Task<(string CompanyName, string? BusinessPartnerNumber)> GetBpnAndCompanyNameForApplicationId(Guid applicationId) =>
        portalDbContext.CompanyApplications
            .Where(x => x.Id == applicationId)
            .Select(x => new ValueTuple<string, string?>(
                x.Company!.Name,
                x.Company.BusinessPartnerNumber))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<string> GetSharedIdpAliasseForApplicationId(Guid applicationId) =>
        portalDbContext.CompanyApplications
            .Where(x => x.Id == applicationId)
            .SelectMany(ca =>
                ca.Company!.IdentityProviders
                    .Where(x =>
                        x.IdentityProviderTypeId == IdentityProviderTypeId.SHARED &&
                        x.IamIdentityProvider != null)
                    .Select(x => x.IamIdentityProvider!.IamIdpAlias))
            .ToAsyncEnumerable()
    ;
}
