using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
namespace CatenaX.NetworkServices.PortalBackend.DBAccess

{
    public class PortalBackendDBAccess : IPortalBackendDBAccess
    {
        private readonly PortalDbContext _dbContext;

        public PortalBackendDBAccess(PortalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IAsyncEnumerable<UserBpn> GetBpnForUsersUntrackedAsync(IEnumerable<string> userIds) =>
            _dbContext.IamUsers
                .Where(iamUser =>
                    userIds.Contains(iamUser.UserEntityId)
                    && iamUser.CompanyUser!.Company!.Bpn != null)
                .Select(iamUser => new UserBpn(
                    iamUser.UserEntityId,
                    iamUser.CompanyUser!.Company!.Bpn!))
                .AsNoTracking()
                .AsAsyncEnumerable();

        public IAsyncEnumerable<string> GetIdpAliaseForCompanyIdUntrackedAsync(Guid companyId) =>
            _dbContext.CompanyIdentityProviders
                .Where(cip => cip.CompanyId == companyId
                    && cip.IdentityProvider!.IamIdentityProvider!.IamIdpAlias != null)
                .Select(cip => cip.IdentityProvider!.IamIdentityProvider!.IamIdpAlias)
                .AsNoTracking()
                .AsAsyncEnumerable();

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

        public Address CreateAddress(string city, string streetname, decimal zipcode, string countryAlpha2Code) =>
            _dbContext.Addresses.Add(
                new Address(
                    Guid.NewGuid(),
                    city,
                    streetname,
                    zipcode,
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
                .Where(iamUser => iamUser.UserEntityId == iamUserId)
                .SelectMany(iamUser => iamUser.CompanyUser!.Company!.CompanyApplications)
                    .Select(companyApplication => new CompanyApplicationWithStatus
                    {
                        ApplicationId = companyApplication.Id,
                        ApplicationStatus = companyApplication.ApplicationStatusId
                    })
                .AsAsyncEnumerable();

        public Task<CompanyWithAddress> GetCompanyWithAdressUntrackedAsync(Guid companyApplicationId) =>
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
                        Bpn = companyApplication.Company!.Bpn,
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

        public Task<Company> GetCompanyWithAdressAsync(Guid companyApplicationId, Guid companyId) =>
            _dbContext.Companies
                .Include(company => company!.Address)
                .Where(company => company.Id == companyId && company.CompanyApplications.Any(application => application.Id == companyApplicationId))
                .SingleOrDefaultAsync();

        public Task<CompanyNameIdBpnIdpAlias> GetCompanyNameIdWithSharedIdpAliasUntrackedAsync(Guid applicationId, string iamUserId) =>
            _dbContext.IamUsers
                .AsNoTracking()
                .Where(iamUser =>
                    iamUser.UserEntityId == iamUserId
                    && iamUser.CompanyUser!.Company!.CompanyApplications.Any(application => application.Id == applicationId))
                .Select(iamUser => iamUser.CompanyUser!.Company)
                .Select(company => new CompanyNameIdBpnIdpAlias(
                        company!.Name,
                        company.Id)
                    {
                        Bpn = company.Bpn,
                        IdpAlias = company.IdentityProviders
                            .Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                            .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                            .SingleOrDefault()
                    })
                .SingleOrDefaultAsync();

        public Task<CompanyNameBpnIdpAlias> GetCompanyNameIdpAliasUntrackedAsync(string iamUserId) =>
            _dbContext.IamUsers
                .AsNoTracking()
                .Where(iamUser => iamUser.UserEntityId == iamUserId)
                .Select(iamUser => iamUser!.CompanyUser!.Company)
                .Select(company => new CompanyNameBpnIdpAlias(
                    company!.Id,
                    company.Name)
                {
                    Bpn = company!.Bpn,
                    IdpAlias = company!.IdentityProviders
                        .Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                        .SingleOrDefault()!.IamIdentityProvider!.IamIdpAlias,
                }).SingleOrDefaultAsync();

        public Task<CompanyApplication> GetCompanyApplicationAsync(Guid applicationId) =>
            _dbContext.CompanyApplications
                .Where(application => application.Id == applicationId)
                .SingleOrDefaultAsync();

        public Task<CompanyIdWithUserId> GetCompanyWithUserIdForUserApplicationUntrackedAsync(Guid applicationId, string iamUserId) =>
            _dbContext.IamUsers
                .Where(iamUser =>
                    iamUser.UserEntityId == iamUserId
                    && iamUser.CompanyUser!.Company!.CompanyApplications.Any(application => application.Id == applicationId))
                .Select(iamUser => new CompanyIdWithUserId(
                    iamUser.CompanyUser!.CompanyId,
                    iamUser.CompanyUserId
                ))
                .SingleOrDefaultAsync();

        public Task<Guid> GetCompanyUserIdForUserApplicationUntrackedAsync(Guid applicationId, string iamUserId) =>
            _dbContext.IamUsers
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
                .Where(companyRole => companyRoleIds.Contains(companyRole.CompanyRoleId))
                .Select(companyRole => new AgreementsAssignedCompanyRoleData(
                    companyRole.CompanyRoleId,
                    companyRole.AgreementAssignedCompanyRoles!.Select(agreementAssignedCompanyRole => agreementAssignedCompanyRole.AgreementId)
                )).AsAsyncEnumerable();

        public Task<CompanyRoleAgreementConsentData> GetCompanyRoleAgreementConsentDataAsync(Guid applicationId, string iamUserId) =>
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

        public Task<CompanyRoleAgreementConsents> GetCompanyRoleAgreementConsentStatusUntrackedAsync(Guid applicationId, string iamUserId) =>
            _dbContext.IamUsers
                .AsQueryable()
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
                    Id = companyRole.CompanyRoleId,
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

        public async IAsyncEnumerable<Guid> GetUserRoleIdsUntrackedAsync(IDictionary<string,IEnumerable<string>> clientRoles)
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
             join invitationStatus in _dbContext.InvitationStatuses on invitation.InvitationStatusId equals invitationStatus.InvitationStatusId
             join companyuser in _dbContext.CompanyUsers on invitation.CompanyUserId equals companyuser.Id
             join iamuser in _dbContext.IamUsers on companyuser.Id equals iamuser.CompanyUserId
             where invitation.CompanyApplicationId == applicationId
             select new InvitedUserDetail(
                 iamuser.UserEntityId,
                 invitationStatus.InvitationStatusId,
                 companyuser.Email
             ))
                .AsNoTracking()
                .AsAsyncEnumerable();

        public async IAsyncEnumerable<WelcomeEmailData> GetWelcomeEmailDataUntrackedAsync(Guid applicationId)
        {
            await foreach (var userData in _dbContext.CompanyApplications
            .AsNoTracking()
            .Where(application => application.Id == applicationId)
            .Select(application => application.Company)
            .SelectMany(company => company.CompanyUsers.Select(user => new
            {
                FirstName = user.Firstname,
                LastName = user.Lastname,
                Email = user.Email,
                CompanyName = user.Company!.Name

            })).AsAsyncEnumerable())
            {
                yield return new WelcomeEmailData(
                userData.FirstName + " " + userData.LastName,
                userData.Email,
                userData.CompanyName);
            }

        }
        public Task<IdpUser> GetIdpCategoryIdByUserId(Guid companyUserId, string adminUserId) =>
            _dbContext.IamUsers
                .Where(iamUser => iamUser.UserEntityId == adminUserId)
                .Select(iamUser => iamUser!.CompanyUser!.Company)
                .Select(company => new IdpUser {
                    TargetIamUserId = company!.CompanyUsers
                        .Where(companyUser => companyUser.Id == companyUserId)
                        .Select(companyUser => companyUser.IamUser!.UserEntityId)
                        .SingleOrDefault(),
                    IdpName = company!.IdentityProviders
                        .Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                        .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                        .SingleOrDefault()
                }).SingleOrDefaultAsync();

        public Task<int> SaveAsync() =>
            _dbContext.SaveChangesAsync();
    }
}
