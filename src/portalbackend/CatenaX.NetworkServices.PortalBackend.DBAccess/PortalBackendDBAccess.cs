using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess

{
    public class PortalBackendDBAccess : IPortalBackendDBAccess
    {
        private const string DEFAULT_LANGUAGE = "en";
        private readonly PortalDbContext _dbContext;

        public PortalBackendDBAccess(PortalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IAsyncEnumerable<CompanyApplicationWithStatus> GetApplicationsWithStatusUntrackedAsync(string iamUserId) =>
            _dbContext.IamUsers
                .AsNoTracking()
                .Where(iamUser => iamUser.UserEntityId == iamUserId)
                .SelectMany(iamUser => iamUser.CompanyUser!.Company!.CompanyApplications)
                    .Select(companyApplication => new CompanyApplicationWithStatus
                    {
                        ApplicationId = companyApplication.Id,
                        ApplicationStatus = companyApplication.ApplicationStatusId
                    })
                .AsAsyncEnumerable();

        public Task<string?> GetSharedIdentityProviderIamAliasUntrackedAsync(string iamUserId) =>
            _dbContext.IamUsers
                .AsNoTracking()
                .Where(iamUser => iamUser.UserEntityId == iamUserId)
                .SelectMany(iamUser => iamUser.CompanyUser!.Company!.IdentityProviders
                    .Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias))
                .SingleOrDefaultAsync();

        public IAsyncEnumerable<CompanyUser> GetCompanyUserRolesIamUsersAsync(IEnumerable<Guid> companyUserIds, string iamUserId) =>
            _dbContext.CompanyUsers
                .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)
                .SelectMany(companyUser => companyUser.Company!.CompanyUsers)
                .Where(companyUser => companyUserIds.Contains(companyUser.Id) && companyUser.IamUser!.UserEntityId != null)
                .Include(companyUser => companyUser.CompanyUserAssignedRoles)
                .Include(companyUser => companyUser.IamUser)
                .AsAsyncEnumerable();

        public CompanyUserAssignedRole RemoveCompanyUserAssignedRole(CompanyUserAssignedRole companyUserAssignedRole) =>
            _dbContext.Remove(companyUserAssignedRole).Entity;

        public IamUser RemoveIamUser(IamUser iamUser) =>
            _dbContext.Remove(iamUser).Entity;

        public IAsyncEnumerable<InvitedUserDetail> GetInvitedUserDetailsUntrackedAsync(Guid applicationId) =>
            (from invitation in _dbContext.Invitations
             join invitationStatus in _dbContext.InvitationStatuses on invitation.InvitationStatusId equals invitationStatus.Id
             join companyuser in _dbContext.CompanyUsers on invitation.CompanyUserId equals companyuser.Id
             join iamuser in _dbContext.IamUsers on companyuser.Id equals iamuser.CompanyUserId
             where invitation.CompanyApplicationId == applicationId
             select new InvitedUserDetail(
                 iamuser.UserEntityId,
                 invitationStatus.Id,
                 companyuser.Email
             ))
                .AsNoTracking()
                .AsAsyncEnumerable();

        public Task<IdpUser?> GetIdpCategoryIdByUserIdAsync(Guid companyUserId, string adminUserId) =>
            _dbContext.CompanyUsers.AsNoTracking()
                .Where(companyUser => companyUser.Id == companyUserId
                    && companyUser.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId))
                .Select(companyUser => new IdpUser
                {
                    TargetIamUserId = companyUser.IamUser!.UserEntityId,
                    IdpName = companyUser.Company!.IdentityProviders
                        .Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                        .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                        .SingleOrDefault()
                }).SingleOrDefaultAsync();

        public Task<Invitation?> GetInvitationStatusAsync(string iamUserId) =>
            _dbContext.Invitations
            .Where(invitation => invitation.CompanyUser!.IamUser!.UserEntityId == iamUserId)
            .SingleOrDefaultAsync();

        public Task<RegistrationData?> GetRegistrationDataUntrackedAsync(Guid applicationId, string iamUserId) =>
           _dbContext.IamUsers
               .AsNoTracking()
               .Where(iamUser =>
                   iamUser.UserEntityId == iamUserId
                   && iamUser.CompanyUser!.Company!.CompanyApplications.Any(application => application.Id == applicationId))
               .Select(iamUser => iamUser.CompanyUser!.Company)
               .Select(company => new RegistrationData(
                   company!.Id,
                   company.Name,
                   company.CompanyAssignedRoles!.Select(companyAssignedRole => companyAssignedRole.CompanyRoleId),
                   company.CompanyUsers.SelectMany(companyUser => companyUser!.Documents!.Select(document => new RegistrationDocumentNames(document.DocumentName))),
                   company.Consents.Where(consent => consent.ConsentStatusId == PortalBackend.PortalEntities.Enums.ConsentStatusId.ACTIVE)
                                                   .Select(consent => new AgreementConsentStatusForRegistrationData(
                                                           consent.AgreementId, consent.ConsentStatusId)))
               {
                   City = company.Address!.City,
                   Streetname = company.Address.Streetname,
                   CountryAlpha2Code = company.Address.CountryAlpha2Code,
                   BusinessPartnerNumber = company.BusinessPartnerNumber,
                   Shortname = company.Shortname,
                   Region = company.Address.Region,
                   Streetadditional = company.Address.Streetadditional,
                   Streetnumber = company.Address.Streetnumber,
                   Zipcode = company.Address.Zipcode,
                   CountryDe = company.Address.Country!.CountryNameDe,
                   TaxId = company.TaxId
               }).SingleOrDefaultAsync();

        public IAsyncEnumerable<ClientRoles> GetClientRolesAsync(Guid appId, string? languageShortName = null) =>
           _dbContext.AppAssignedClients
               .Where(client => client.AppId == appId)
               .SelectMany(clients => clients.IamClient!.UserRoles!)
               .Select(roles => new ClientRoles(
                   roles.Id,
                   roles.UserRoleText,
                   languageShortName == null ?
                   roles.UserRoleDescriptions.SingleOrDefault(desc => desc.LanguageShortName == DEFAULT_LANGUAGE)!.Description :
                   roles.UserRoleDescriptions.SingleOrDefault(desc => desc.LanguageShortName == languageShortName)!.Description
               )).AsAsyncEnumerable();

        public Task<string?> GetLanguageAsync(string LanguageShortName) =>
            _dbContext.Languages
                .AsNoTracking()
                .Where(language => language.ShortName == LanguageShortName)
                .Select(language => language.ShortName)
                .SingleOrDefaultAsync();

        public Task<Guid> GetAppAssignedClientsAsync(Guid appId) =>
            _dbContext.AppAssignedClients
                .AsNoTracking()
                .Where(app => app.AppId == appId)
                .Select(app => app.AppId)
                .SingleOrDefaultAsync();

        public Task<int> SaveAsync() =>
            _dbContext.SaveChangesAsync();
    }
}
