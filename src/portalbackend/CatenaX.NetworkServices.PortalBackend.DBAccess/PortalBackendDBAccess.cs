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

        public Company CreateCompany(string companyName) =>
            _dbContext.Companies.Add(
                new Company(
                    Guid.NewGuid(),
                    companyName,
                    CompanyStatusId.PENDING,
                    DateTimeOffset.UtcNow)).Entity;

        public CompanyApplication CreateCompanyApplication(Company company, CompanyApplicationStatusId companyApplicationStatusId) =>
            _dbContext.CompanyApplications.Add(
                new CompanyApplication(
                    Guid.NewGuid(),
                    company.Id,
                    companyApplicationStatusId,
                    DateTimeOffset.UtcNow)).Entity;

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

        public Invitation CreateInvitation(Guid applicationId, CompanyUser user) =>
            _dbContext.Invitations.Add(
                new Invitation(
                    Guid.NewGuid(),
                    applicationId,
                    user.Id,
                    InvitationStatusId.CREATED,
                    DateTimeOffset.UtcNow)).Entity;

        [Obsolete("use IUserRolesRepository instead")]
        public CompanyUserAssignedRole CreateCompanyUserAssignedRole(Guid companyUserId, Guid userRoleId) =>
            _dbContext.CompanyUserAssignedRoles.Add(
                new CompanyUserAssignedRole(
                    companyUserId,
                    userRoleId
                )).Entity;

        public IdentityProvider CreateSharedIdentityProvider(Company company)
        {
            var idp = new IdentityProvider(
                Guid.NewGuid(),
                IdentityProviderCategoryId.KEYCLOAK_SHARED,
                DateTimeOffset.UtcNow);
            idp.Companies.Add(company);
            return _dbContext.IdentityProviders.Add(idp).Entity;
        }

        public IamIdentityProvider CreateIamIdentityProvider(IdentityProvider identityProvider, string idpAlias) =>
            _dbContext.IamIdentityProviders.Add(
                new IamIdentityProvider(
                    idpAlias,
                    identityProvider.Id)).Entity;

        public IamUser CreateIamUser(CompanyUser user, string iamUserEntityId) =>
            _dbContext.IamUsers.Add(
                new IamUser(
                    iamUserEntityId,
                    user.Id)).Entity;

        public Address CreateAddress(string city, string streetname, string countryAlpha2Code) =>
            _dbContext.Addresses.Add(
                new Address(
                    Guid.NewGuid(),
                    city,
                    streetname,
                    countryAlpha2Code,
                    DateTimeOffset.UtcNow
                )).Entity;

        public Consent CreateConsent(Guid agreementId, Guid companyId, Guid companyUserId, ConsentStatusId consentStatusId, string? Comment = null, string? Target = null, Guid? DocumentId = null) =>
            _dbContext.Consents.Add(
                new Consent(
                    Guid.NewGuid(),
                    agreementId,
                    companyId,
                    companyUserId,
                    consentStatusId,
                    DateTimeOffset.UtcNow)
                {
                    Comment = Comment,
                    Target = Target,
                    DocumentId = DocumentId
                }).Entity;

        public CompanyAssignedRole CreateCompanyAssignedRole(Guid companyId, CompanyRoleId companyRoleId) =>
            _dbContext.CompanyAssignedRoles.Add(
                new CompanyAssignedRole(
                    companyId,
                    companyRoleId
                )).Entity;

        public Document CreateDocument(Guid applicationId, Guid companyUserId, string documentName, string documentContent, string hash, uint documentOId, DocumentTypeId documentTypeId) =>
            _dbContext.Documents.Add(
                new Document(
                    Guid.NewGuid(),
                    hash,
                    documentName,
                    DateTimeOffset.UtcNow)
                {
                    DocumentOid = documentOId,
                    DocumentTypeId = documentTypeId,
                    CompanyUserId = companyUserId
                }).Entity;

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

        [Obsolete("use IApplicationRepository instead")]
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

        public Task<CompanyNameIdIdpAlias?> GetCompanyNameIdWithSharedIdpAliasUntrackedAsync(Guid applicationId, string iamUserId) =>
            _dbContext.IamUsers
                .AsNoTracking()
                .Where(iamUser =>
                    iamUser.UserEntityId == iamUserId
                    && iamUser.CompanyUser!.Company!.CompanyApplications.Any(application => application.Id == applicationId))
                .Select(iamUser => iamUser.CompanyUser!.Company)
                .Select(company => new CompanyNameIdIdpAlias(
                        company!.Name,
                        company.Id)
                {
                    IdpAlias = company.IdentityProviders
                            .Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                            .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                            .SingleOrDefault()
                })
                .SingleOrDefaultAsync();

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

        public IAsyncEnumerable<CompanyUserData> GetCompanyUserDetailsUntrackedAsync(
            string adminUserId,
            Guid? companyUserId = null,
            string? userEntityId = null,
            string? firstName = null,
            string? lastName = null,
            string? email = null,
            CompanyUserStatusId? status = null) =>
            _dbContext.CompanyUsers
                .AsNoTracking()
                .Where(companyUser => companyUser.IamUser!.UserEntityId == adminUserId)
                .SelectMany(companyUser => companyUser.Company!.CompanyUsers)
                .Where(companyUser =>
                    userEntityId != null ? companyUser.IamUser!.UserEntityId == userEntityId : true
                    && companyUserId.HasValue ? companyUser.Id == companyUserId!.Value : true
                    && firstName != null ? companyUser.Firstname == firstName : true
                    && lastName != null ? companyUser.Lastname == lastName : true
                    && email != null ? companyUser.Email == email : true
                    && status.HasValue ? companyUser.CompanyUserStatusId == status : true)
                .Select(companyUser => new CompanyUserData(
                    companyUser.Id,
                    companyUser.CompanyUserStatusId)
                {
                    FirstName = companyUser.Firstname,
                    LastName = companyUser.Lastname,
                    Email = companyUser.Email
                })
                .AsAsyncEnumerable();

        public Task<CompanyApplication?> GetCompanyApplicationAsync(Guid applicationId) =>
            _dbContext.CompanyApplications
                .Where(application => application.Id == applicationId)
                .SingleOrDefaultAsync();

        public Task<Guid> GetCompanyUserIdForUserApplicationUntrackedAsync(Guid applicationId, string iamUserId) =>
            _dbContext.IamUsers
                .AsNoTracking()
                .Where(iamUser =>
                    iamUser.UserEntityId == iamUserId
                    && iamUser.CompanyUser!.Company!.CompanyApplications.Any(application => application.Id == applicationId))
                .Select(iamUser =>
                    iamUser.CompanyUserId
                )
                .SingleOrDefaultAsync();

        public Task<CompanyApplicationStatusId> GetApplicationStatusUntrackedAsync(Guid applicationId)
        {
            return _dbContext.CompanyApplications
                .Where(application => application.Id == applicationId)
                .AsNoTracking()
                .Select(application => application.ApplicationStatusId)
                .SingleOrDefaultAsync();
        }

        public IAsyncEnumerable<AgreementsAssignedCompanyRoleData> GetAgreementAssignedCompanyRolesUntrackedAsync(IEnumerable<CompanyRoleId> companyRoleIds) =>
            _dbContext.CompanyRoles
                .AsNoTracking()
                .Where(companyRole => companyRoleIds.Contains(companyRole.Id))
                .Select(companyRole => new AgreementsAssignedCompanyRoleData(
                    companyRole.Id,
                    companyRole.AgreementAssignedCompanyRoles!.Select(agreementAssignedCompanyRole => agreementAssignedCompanyRole.AgreementId)
                )).AsAsyncEnumerable();

        public Task<CompanyRoleAgreementConsentData?> GetCompanyRoleAgreementConsentDataAsync(Guid applicationId, string iamUserId) =>
            _dbContext.IamUsers
                .Where(iamUser =>
                    iamUser.UserEntityId == iamUserId
                    && iamUser.CompanyUser!.Company!.CompanyApplications.Any(application => application.Id == applicationId))
                .Select(iamUser => iamUser.CompanyUser)
                .Select(companyUser => new CompanyRoleAgreementConsentData(
                    companyUser!.Id,
                    companyUser!.CompanyId,
                    companyUser.Company!.CompanyAssignedRoles,
                    companyUser.Company.Consents.Where(consent => consent.ConsentStatusId == ConsentStatusId.ACTIVE)))
                .SingleOrDefaultAsync();

        public Task<CompanyRoleAgreementConsents?> GetCompanyRoleAgreementConsentStatusUntrackedAsync(Guid applicationId, string iamUserId) =>
            _dbContext.IamUsers
                .AsNoTracking()
                .Where(iamUser =>
                    iamUser.UserEntityId == iamUserId
                    && iamUser.CompanyUser!.Company!.CompanyApplications.Any(application => application.Id == applicationId))
                .Select(iamUser => iamUser.CompanyUser!.Company)
                .Select(company => new CompanyRoleAgreementConsents(
                    company!.CompanyAssignedRoles.Select(companyAssignedRole => companyAssignedRole.CompanyRoleId),
                    company.Consents.Where(consent => consent.ConsentStatusId == PortalBackend.PortalEntities.Enums.ConsentStatusId.ACTIVE).Select(consent => new AgreementConsentStatus(
                        consent.AgreementId,
                        consent.ConsentStatusId
                    )))).SingleOrDefaultAsync();

        public async IAsyncEnumerable<CompanyRoleData> GetCompanyRoleAgreementsUntrackedAsync()
        {
            await foreach (var role in _dbContext.CompanyRoles
                .AsNoTracking()
                .Select(companyRole => new
                {
                    Id = companyRole.Id,
                    Descriptions = companyRole.CompanyRoleDescriptions.Select(description => new { ShortName = description.LanguageShortName, Description = description.Description }),
                    Agreements = companyRole.AgreementAssignedCompanyRoles.Select(agreementAssignedCompanyRole => agreementAssignedCompanyRole.AgreementId)
                })
                .AsAsyncEnumerable())
            {
                yield return new CompanyRoleData(
                    role.Id,
                    role.Descriptions.ToDictionary(d => d.ShortName, d => d.Description),
                    role.Agreements);
            }
        }

        public IAsyncEnumerable<AgreementData> GetAgreementsUntrackedAsync() =>
            _dbContext.Agreements
                .AsNoTracking()
                .Select(agreement => new AgreementData(
                    agreement.Id,
                    agreement.Name))
                .AsAsyncEnumerable();

        public CompanyAssignedRole RemoveCompanyAssignedRole(CompanyAssignedRole companyAssignedRole) =>
            _dbContext.Remove(companyAssignedRole).Entity;

        public CompanyUserAssignedRole RemoveCompanyUserAssignedRole(CompanyUserAssignedRole companyUserAssignedRole) =>
            _dbContext.Remove(companyUserAssignedRole).Entity;

        public IamUser RemoveIamUser(IamUser iamUser) =>
            _dbContext.Remove(iamUser).Entity;

        [Obsolete("use IUserRolesRepository instead")]
        public async IAsyncEnumerable<Guid> GetUserRoleIdsUntrackedAsync(IDictionary<string, IEnumerable<string>> clientRoles)
        {
            foreach (var clientRole in clientRoles)
            {
                await foreach (var userRoleId in _dbContext.UserRoles
                    .AsNoTracking()
                    .Where(userRole => userRole.IamClient!.ClientClientId == clientRole.Key && clientRole.Value.Contains(userRole.UserRoleText))
                    .AsQueryable()
                    .Select(userRole => userRole.Id)
                    .AsAsyncEnumerable().ConfigureAwait(false))
                {
                    yield return userRoleId;
                }
            }
        }

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
                        document!.Documentname))
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
                   company.CompanyUsers.SelectMany(companyUser => companyUser!.Documents!.Select(document => new RegistrationDocumentNames(document.Documentname))),
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

        public Task<CompanyApplication?> GetCompanyAndApplicationForSubmittedApplication(Guid applicationId) =>
            _dbContext.CompanyApplications.Where(companyApplication =>
                companyApplication.Id == applicationId
                && companyApplication.ApplicationStatusId == CompanyApplicationStatusId.SUBMITTED)
                .Include(companyApplication => companyApplication.Company)
                .SingleOrDefaultAsync();

        public Task<bool> IsUserExisting(string iamUserId) =>
            _dbContext.IamUsers
                .AsNoTracking()
                .AnyAsync(iamUser => iamUser.UserEntityId == iamUserId);

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
