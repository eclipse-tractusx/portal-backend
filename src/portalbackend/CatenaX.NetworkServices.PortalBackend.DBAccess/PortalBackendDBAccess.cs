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

        [Obsolete("user IUserRepository instead")]
        public CompanyUser CreateCompanyUser(string? firstName, string? lastName, string email, Guid companyId, CompanyUserStatusId companyUserStatusId) =>
            _dbContext.CompanyUsers.Add(
                new CompanyUser(
                    Guid.NewGuid(),
                    companyId,
                    companyUserStatusId,
                    DateTimeOffset.UtcNow)
                {
                    Firstname = firstName,
                    Lastname = lastName,
                    Email = email,
                }).Entity;

        [Obsolete("use IUserRolesRepository instead")]
        public CompanyUserAssignedRole CreateCompanyUserAssignedRole(Guid companyUserId, Guid userRoleId) =>
            _dbContext.CompanyUserAssignedRoles.Add(
                new CompanyUserAssignedRole(
                    companyUserId,
                    userRoleId
                )).Entity;

        [Obsolete("user IUserRepository instead")]
        public IamUser CreateIamUser(CompanyUser user, string iamUserEntityId) =>
            _dbContext.IamUsers.Add(
                new IamUser(
                    iamUserEntityId,
                    user.Id)).Entity;

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

        public Task<CompanyNameBpnIdpAlias?> GetCompanyNameIdpAliasUntrackedAsync(string iamUserId) =>
            _dbContext.IamUsers
                .AsNoTracking()
                .Where(iamUser => iamUser.UserEntityId == iamUserId)
                .Select(iamUser => iamUser!.CompanyUser!.Company)
                .Select(company => new CompanyNameBpnIdpAlias(
                    company!.Id,
                    company.Name)
                {
                    BusinessPartnerNumber = company!.BusinessPartnerNumber,
                    IdpAlias = company!.IdentityProviders
                        .Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                        .SingleOrDefault()!.IamIdentityProvider!.IamIdpAlias,
                }).SingleOrDefaultAsync();

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

        public Task<CompanyApplication?> GetCompanyApplicationAsync(Guid applicationId) =>
            _dbContext.CompanyApplications
                .Where(application => application.Id == applicationId)
                .SingleOrDefaultAsync();

        public Task<CompanyApplicationStatusId> GetApplicationStatusUntrackedAsync(Guid applicationId)
        {
            return _dbContext.CompanyApplications
                .Where(application => application.Id == applicationId)
                .AsNoTracking()
                .Select(application => application.ApplicationStatusId)
                .SingleOrDefaultAsync();
        }

        public CompanyUserAssignedRole RemoveCompanyUserAssignedRole(CompanyUserAssignedRole companyUserAssignedRole) =>
            _dbContext.Remove(companyUserAssignedRole).Entity;

        public IamUser RemoveIamUser(IamUser iamUser) =>
            _dbContext.Remove(iamUser).Entity;

        [Obsolete("user IUserRolesRepository instead")]
        public IAsyncEnumerable<UserRoleWithId> GetUserRoleWithIdsUntrackedAsync(string clientClientId, IEnumerable<string> userRoles) =>
            _dbContext.UserRoles
                .AsNoTracking()
                .Where(userRole => userRole.IamClient!.ClientClientId == clientClientId && userRoles.Contains(userRole.UserRoleText))
                .AsQueryable()
                .Select(userRole => new UserRoleWithId(
                    userRole.UserRoleText,
                    userRole.Id
                ))
                .AsAsyncEnumerable();

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

        public IAsyncEnumerable<UploadDocuments> GetUploadedDocumentsAsync(Guid applicationId, DocumentTypeId documentTypeId, string iamUserId) =>
            _dbContext.IamUsers
                .AsNoTracking()
                .Where(iamUser =>
                    iamUser.UserEntityId == iamUserId
                    && iamUser.CompanyUser!.Company!.CompanyApplications.Any(application => application.Id == applicationId))
                .SelectMany(iamUser => iamUser.CompanyUser!.Documents.Where(docu => docu.DocumentTypeId == documentTypeId))
                .Select(document =>
                    new UploadDocuments(
                        document!.Id,
                        document!.DocumentName))
                .AsAsyncEnumerable();

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
