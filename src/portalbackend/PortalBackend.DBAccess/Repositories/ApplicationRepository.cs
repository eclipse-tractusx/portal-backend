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

using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;

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

    public CompanyApplication AttachAndModifyCompanyApplication(Guid companyApplicationId, Action<CompanyApplication>? setOptionalParameters = null)
    {
        var companyApplication = _dbContext.Attach(new CompanyApplication(companyApplicationId, Guid.Empty, default, default)).Entity;
        setOptionalParameters?.Invoke(companyApplication);
        return companyApplication;
    }

    public Invitation CreateInvitation(Guid applicationId, Guid companyUserId) =>
        _dbContext.Invitations.Add(
            new Invitation(
                Guid.NewGuid(),
                applicationId,
                companyUserId,
                InvitationStatusId.CREATED,
                DateTimeOffset.UtcNow)).Entity;

    public Invitation DeleteInvitation(Guid invitationId) =>
        _dbContext.Invitations.Remove(
            new Invitation(
                invitationId,
                Guid.Empty,
                Guid.Empty,
                default,
                default)).Entity;

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
                CompanyUser = application.Company!.CompanyUsers.Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId).SingleOrDefault()
            })
            .Select(data => new CompanyApplicationUserEmailData(data.Application)
            {
                CompanyUserId = data.CompanyUser!.Id,
                Email = data.CompanyUser!.Email
            })
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
                    CountryDe = companyApplication.Company.Address.Country!.CountryNameDe, // FIXME internationalization, maybe move to separate endpoint that returns Contrynames for all (or a specific) language
                    TaxId = companyApplication.Company.TaxId
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
            .Where(application => application.Id == applicationId
                && application.CompanyId == companyId)
            .Include(application => application.Company)
            .Include(application => application.Company!.Address)
            .Select(application => new CompanyApplicationWithCompanyAddressUserData(
                application)
            {
                CompanyUserId = application.Company!.CompanyUsers
                    .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)
                    .Select(companyUser => companyUser.Id)
                    .SingleOrDefault()
            })
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
}
