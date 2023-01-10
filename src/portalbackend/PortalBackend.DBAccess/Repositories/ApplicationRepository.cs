/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly PortalDbContext _dbContext;

    public ApplicationRepository(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public CompanyApplication CreateCompanyApplication(Guid companyId, CompanyApplicationStatusId companyApplicationStatusId) =>
        _dbContext.CompanyApplications.Add(
            new CompanyApplication(
                Guid.NewGuid(),
                companyId,
                companyApplicationStatusId,
                DateTimeOffset.UtcNow)).Entity;

    public void AttachAndModifyCompanyApplication(Guid companyApplicationId, Action<CompanyApplication> setOptionalParameters)
    {
        var companyApplication = _dbContext.Attach(new CompanyApplication(companyApplicationId, Guid.Empty, default, default)).Entity;
        setOptionalParameters.Invoke(companyApplication);
    }

    public Invitation CreateInvitation(Guid applicationId, Guid companyUserId) =>
        _dbContext.Invitations.Add(
            new Invitation(
                Guid.NewGuid(),
                applicationId,
                companyUserId,
                InvitationStatusId.CREATED,
                DateTimeOffset.UtcNow)).Entity;

    public void DeleteInvitations(IEnumerable<Guid> invitationIds) =>
        _dbContext.Invitations.RemoveRange(
            invitationIds.Select(
                invitationId => new Invitation(
                    invitationId,
                    Guid.Empty,
                    Guid.Empty,
                    default,
                    default)));

    public Task<CompanyApplicationUserData?> GetOwnCompanyApplicationUserDataAsync(Guid applicationId, string iamUserId) =>
        _dbContext.CompanyApplications
            .Where(application => application.Id == applicationId)
            .Select(application => new CompanyApplicationUserData(application)
            {
                CompanyUserId = application.Company!.CompanyUsers.Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId).Select(companyUser => companyUser.Id).SingleOrDefault()
            })
            .SingleOrDefaultAsync();
    public Task<CompanyApplicationStatusUserData?> GetOwnCompanyApplicationStatusUserDataUntrackedAsync(Guid applicationId, string iamUserId) =>
        _dbContext.CompanyApplications
            .AsNoTracking()
            .Where(application => application.Id == applicationId)
            .Select(application => new CompanyApplicationStatusUserData(application.ApplicationStatusId)
            {
                CompanyUserId = application.Company!.CompanyUsers.Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId).Select(companyUser => companyUser.Id).SingleOrDefault()
            })
            .SingleOrDefaultAsync();

    public Task<CompanyApplicationUserEmailData?> GetOwnCompanyApplicationUserEmailDataAsync(Guid applicationId, string iamUserId) =>
        _dbContext.CompanyApplications
            .Where(application => application.Id == applicationId)
            .Select(application => new {
                Application = application, 
                CompanyUser = application.Company!.CompanyUsers.Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId).SingleOrDefault(),
                Documents = application.Company.CompanyUsers.SelectMany(companyUser => companyUser.Documents).Where(Doc => Doc.DocumentStatusId != DocumentStatusId.LOCKED)
            })
            .Select(data => new CompanyApplicationUserEmailData(
                data.Application.ApplicationStatusId,
                data.CompanyUser!.Id,
                data.CompanyUser.Email,
                data.Documents.Select(doc => new DocumentStatusData(doc.Id, doc.DocumentStatusId))
                ))
            .SingleOrDefaultAsync();

    public Task<CompanyWithAddress?> GetCompanyWithAdressUntrackedAsync(Guid companyApplicationId) =>
        _dbContext.CompanyApplications
            .Where(companyApplication => companyApplication.Id == companyApplicationId)
            .Select(
                companyApplication => new CompanyWithAddress(
                    companyApplication.CompanyId,
                    companyApplication.Company!.Name,
                    companyApplication.Company.Address!.City ?? "",
                    companyApplication.Company.Address.Streetname ?? "",
                    companyApplication.Company.Address.CountryAlpha2Code ?? "")
                {
                    BusinessPartnerNumber = companyApplication.Company!.BusinessPartnerNumber,
                    Shortname = companyApplication.Company.Shortname,
                    Region = companyApplication.Company.Address.Region,
                    Streetadditional = companyApplication.Company.Address.Streetadditional,
                    Streetnumber = companyApplication.Company.Address.Streetnumber,
                    Zipcode = companyApplication.Company.Address.Zipcode,
                    CountryDe = companyApplication.Company.Address.Country!.CountryNameDe // FIXME internationalization, maybe move to separate endpoint that returns Contrynames for all (or a specific) language
                })
            .AsNoTracking()
            .SingleOrDefaultAsync();

    public IQueryable<CompanyApplication> GetCompanyApplicationsFilteredQuery(string? companyName = null, IEnumerable<CompanyApplicationStatusId>? applicationStatusIds = null) =>
        _dbContext.Companies
            .AsNoTracking()
            .Where(company => companyName != null ? EF.Functions.ILike(company!.Name, $"{companyName}%") : true)
            .SelectMany(company => company.CompanyApplications.Where(companyApplication =>
                applicationStatusIds != null ? applicationStatusIds.Contains(companyApplication.ApplicationStatusId) : true));

    public Task<CompanyApplicationWithCompanyAddressUserData?> GetCompanyApplicationWithCompanyAdressUserDataAsync (Guid applicationId, Guid companyId, string iamUserId) =>
        _dbContext.CompanyApplications
            .AsNoTracking()
            .Where(application => application.Id == applicationId
                && application.CompanyId == companyId)
            .Select(application => new CompanyApplicationWithCompanyAddressUserData(
                application.ApplicationStatusId,
                application.Company!.Name,
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
                application.Company.Address.Country!.CountryNameDe,
                application.Company.CompanyUsers
                    .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)
                    .Select(companyUser => companyUser.Id)
                    .SingleOrDefault()))
            .SingleOrDefaultAsync();

    public Task<CompanyApplication?> GetCompanyAndApplicationForSubmittedApplication(Guid applicationId) =>
        _dbContext.CompanyApplications.Where(companyApplication =>
            companyApplication.Id == applicationId
            && companyApplication.ApplicationStatusId == CompanyApplicationStatusId.SUBMITTED)
            .Include(companyApplication => companyApplication.Company)
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(Guid companyId, string companyName, string? businessPartnerNumber, string countryCode)> GetCompanyAndApplicationDetailsForSubmittedApplicationAsync(Guid applicationId) =>
        _dbContext.CompanyApplications.Where(companyApplication =>
                companyApplication.Id == applicationId
                && companyApplication.ApplicationStatusId == CompanyApplicationStatusId.SUBMITTED)
            .Select(ca => new ValueTuple<Guid, string, string?, string>(
                ca.CompanyId,
                ca.Company!.Name,
                ca.Company!.BusinessPartnerNumber,
                ca.Company!.Address!.Country!.Alpha2Code))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<CompanyInvitedUserData> GetInvitedUsersDataByApplicationIdUntrackedAsync(Guid applicationId) =>
        _dbContext.Invitations
            .AsNoTracking()
            .Where(invitation => invitation.CompanyApplicationId == applicationId)
            .Select(invitation => invitation.CompanyUser)
            .Where(companyUser => companyUser!.CompanyUserStatusId == CompanyUserStatusId.ACTIVE)
            .Select(companyUser => new CompanyInvitedUserData(
                companyUser!.Id,
                companyUser.IamUser!.UserEntityId,
                companyUser.CompanyUserAssignedBusinessPartners.Select(companyUserAssignedBusinessPartner => companyUserAssignedBusinessPartner.BusinessPartnerNumber),
                companyUser.CompanyUserAssignedRoles.Select(companyUserAssignedRole => companyUserAssignedRole.UserRoleId)))
            .AsAsyncEnumerable();

    public IAsyncEnumerable<WelcomeEmailData> GetWelcomeEmailDataUntrackedAsync(Guid applicationId, IEnumerable<Guid> roleIds) =>
        _dbContext.CompanyApplications
            .AsNoTracking()
            .Where(application => application.Id == applicationId)
            .SelectMany(application =>
                application.Company!.CompanyUsers
                    .Where(companyUser => companyUser.CompanyUserStatusId == CompanyUserStatusId.ACTIVE)
                    .Select(companyUser => new WelcomeEmailData(
                        companyUser.Id,
                        companyUser.Firstname,
                        companyUser.Lastname,
                        companyUser.Email,
                        companyUser.Company!.Name)))
            .AsAsyncEnumerable();

    public IAsyncEnumerable<WelcomeEmailData> GetRegistrationDeclineEmailDataUntrackedAsync(Guid applicationId, IEnumerable<Guid> roleIds) =>
        _dbContext.CompanyApplications
            .AsNoTracking()
            .Where(application => application.Id == applicationId)
            .SelectMany(application =>
                application.Company!.CompanyUsers
                    .Where(companyUser => companyUser.CompanyUserStatusId == CompanyUserStatusId.ACTIVE && companyUser.UserRoles.Any(userRole => roleIds.Contains(userRole.Id)))
                    .Select(companyUser => new WelcomeEmailData(
                        companyUser.Id,
                        companyUser.Firstname,
                        companyUser.Lastname,
                        companyUser.Email,
                        companyUser.Company!.Name)))
            .AsAsyncEnumerable();

    public IQueryable<CompanyApplication> GetAllCompanyApplicationsDetailsQuery(string? companyName = null) =>
         _dbContext.CompanyApplications
            .AsNoTracking()
            .Where(application => companyName != null ? EF.Functions.ILike(application.Company!.Name, $"%{companyName}%") : true);

    public Task<CompanyUserRoleWithAddress?> GetCompanyUserRoleWithAdressUntrackedAsync(Guid companyApplicationId) =>
        _dbContext.CompanyApplications
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
                    companyApplication.Company.Address.Country!.CountryNameDe,
                    companyApplication.Company.CompanyRoles.SelectMany(companyRole => companyRole.AgreementAssignedCompanyRoles.Select(x => new AgreementsData(x.CompanyRoleId, x.AgreementId, x.Agreement!.Consents.SingleOrDefault(consent => consent.CompanyId == companyApplication.CompanyId)!.ConsentStatusId))),
                    companyApplication.Invitations.Select(x => new InvitedCompanyUserData(x.CompanyUserId, x.CompanyUser!.Firstname, x.CompanyUser.Lastname, x.CompanyUser.Email))))
            .AsNoTracking()
            .SingleOrDefaultAsync();
}
