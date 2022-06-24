using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly PortalDbContext _dbContext;

    public ApplicationRepository(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public CompanyApplication CreateCompanyApplication(Company company, CompanyApplicationStatusId companyApplicationStatusId) =>
        _dbContext.CompanyApplications.Add(
            new CompanyApplication(
                Guid.NewGuid(),
                company.Id,
                companyApplicationStatusId,
                DateTimeOffset.UtcNow)).Entity;

    public Invitation CreateInvitation(Guid applicationId, CompanyUser user) =>
        _dbContext.Invitations.Add(
            new Invitation(
                Guid.NewGuid(),
                applicationId,
                user.Id,
                InvitationStatusId.CREATED,
                DateTimeOffset.UtcNow)).Entity;

    public Task<CompanyApplicationUserData?> GetCompanyApplicationUserDataAsync(Guid applicationId, string iamUserId) =>
        _dbContext.CompanyApplications
            .Where(application => application.Id == applicationId)
            .Select(application => new {
                Application = application, 
                CompanyUser = application.Company!.CompanyUsers.Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId).SingleOrDefault()
            })
            .Select(data => new CompanyApplicationUserData(data.Application)
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

    public Task<Company?> GetCompanyWithAdressAsync(Guid companyApplicationId, Guid companyId) =>
        _dbContext.Companies
            .Include(company => company!.Address)
            .Where(company => company.Id == companyId && company.CompanyApplications.Any(application => application.Id == companyApplicationId))
            .SingleOrDefaultAsync();

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

    public Pagination.AsyncSource<CompanyApplicationDetails> GetCompanyApplicationDetailsUntrackedAsync(int skip, int take) =>
        new Pagination.AsyncSource<CompanyApplicationDetails>(
            _dbContext.CompanyApplications
                .AsNoTracking()
                .CountAsync(),
            _dbContext.CompanyApplications
                .AsNoTracking()
                .OrderByDescending(application => application.DateCreated)
                .Skip(skip)
                .Take(take)
                .Select(application => new CompanyApplicationDetails(
                    application.Id,
                    application.ApplicationStatusId,
                    application.DateCreated,
                    application.Company!.Name,
                    application.Invitations.SelectMany(invitation => invitation.CompanyUser!.Documents.Select(document => new DocumentDetails(
                        Convert.ToHexString(document.DocumentHash).ToLower())
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
                .AsAsyncEnumerable());

    public Task<CompanyApplication?> GetCompanyAndApplicationForSubmittedApplication(Guid applicationId) =>
        _dbContext.CompanyApplications.Where(companyApplication =>
            companyApplication.Id == applicationId
            && companyApplication.ApplicationStatusId == CompanyApplicationStatusId.SUBMITTED)
            .Include(companyApplication => companyApplication.Company)
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

    public IAsyncEnumerable<WelcomeEmailData> GetWelcomeEmailDataUntrackedAsync(Guid applicationId) =>
        _dbContext.CompanyApplications
            .AsNoTracking()
            .Where(application => application.Id == applicationId)
            .SelectMany(application =>
                application.Company!.CompanyUsers
                    .Where(companyUser => companyUser.CompanyUserStatusId == CompanyUserStatusId.ACTIVE)
                    .Select(companyUser => new WelcomeEmailData(
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
                        companyUser.Firstname,
                        companyUser.Lastname,
                        companyUser.Email,
                        companyUser.Company!.Name)))
            .AsAsyncEnumerable();
}
