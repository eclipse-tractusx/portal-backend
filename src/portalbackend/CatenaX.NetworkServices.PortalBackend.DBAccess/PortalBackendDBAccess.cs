using CatenaX.NetworkServices.Framework.Models;
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

        public Task<string?> GetBpnUntrackedAsync(Guid companyId) =>
            _dbContext.Companies
                .AsNoTracking()
                .Where(company => company.Id == companyId)
                .Select(company => company.Bpn)
                .SingleOrDefaultAsync();

        public IAsyncEnumerable<string> GetIamUsersUntrackedAsync(Guid companyId) =>
            _dbContext.IamUsers
                .AsNoTracking()
                .Where(iamUser => iamUser.CompanyUser!.CompanyId == companyId)
                .Select(iamUser => iamUser.UserEntityId)
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

        public CompanyServiceAccount CreateCompanyServiceAccount(Guid companyId, CompanyServiceAccountStatusId companyServiceAccountStatusId, string name, string description) =>
            _dbContext.CompanyServiceAccounts.Add(
                new CompanyServiceAccount(
                    Guid.NewGuid(),
                    companyId,
                    companyServiceAccountStatusId,
                    name,
                    description,
                    DateTimeOffset.UtcNow)).Entity;

        public IamServiceAccount CreateIamServiceAccount(string clientId, string clientClientId, string userEntityId, Guid companyServiceAccountId) =>
            _dbContext.IamServiceAccounts.Add(
                new IamServiceAccount(
                    clientId,
                    clientClientId,
                    userEntityId,
                    companyServiceAccountId)).Entity;

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
                            document.Documenthash)
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
                        BusinessPartnerNumber = application.Company.Bpn
                    })
                    .AsAsyncEnumerable());

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

        public Task<Company?> GetCompanyWithAdressAsync(Guid companyApplicationId, Guid companyId) =>
            _dbContext.Companies
                .Include(company => company!.Address)
                .Where(company => company.Id == companyId && company.CompanyApplications.Any(application => application.Id == companyApplicationId))
                .SingleOrDefaultAsync();

        public Task<CompanyNameIdBpnIdpAlias?> GetCompanyNameIdWithSharedIdpAliasUntrackedAsync(Guid applicationId, string iamUserId) =>
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

        public Task<CompanyNameBpnIdpAlias?> GetCompanyNameIdpAliasUntrackedAsync(string iamUserId) =>
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

        public Task<string?> GetSharedIdentityProviderIamAliasUntrackedAsync(string iamUserId) =>
            _dbContext.IamUsers
                .AsNoTracking()
                .Where(iamUser => iamUser.UserEntityId == iamUserId)
                .SelectMany(iamUser => iamUser.CompanyUser!.Company!.IdentityProviders
                    .Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias))
                .SingleOrDefaultAsync();

        public Task<CompanyUserWithIdpData?> GetCompanyUserWithIdpAsync(string iamUserId) =>
            _dbContext.CompanyUsers
                .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId
                    && companyUser!.Company!.IdentityProviders
                        .Any(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED))
                .Include(companyUser => companyUser.CompanyUserAssignedRoles)
                .Include(companyUser => companyUser.IamUser)
                .Select(companyUser => new CompanyUserWithIdpData(
                    companyUser,
                    companyUser.Company!.IdentityProviders.Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                        .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                        .SingleOrDefault()!
                ))
                .SingleOrDefaultAsync();

        public Task<OwnCompanyUserDetails?> GetOwnCompanyUserDetailsUntrackedAsync(string iamUserId) =>
            _dbContext.CompanyUsers
                .AsNoTracking()
                .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)
                .Select(companyUser => new OwnCompanyUserDetails(
                    companyUser.Id,
                    companyUser.DateCreated,
                    companyUser.Company!.Name,
                    companyUser.CompanyUserStatusId)
                    {
                        FirstName = companyUser.Firstname,
                        LastName = companyUser.Lastname,
                        Email = companyUser.Email,
                        BusinessPartnerNumber = companyUser.Company.Bpn
                    })
                .SingleOrDefaultAsync();

        public Task<CompanyUserWithIdpData?> GetCompanyUserWithCompanyIdpAsync(string iamUserId) =>
        _dbContext.CompanyUsers
            .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId
                && companyUser!.Company!.IdentityProviders
                    .Any(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED))
            .Include(companyUser => companyUser.Company)
            .Include(companyUser => companyUser.IamUser)
            .Select(companyUser => new CompanyUserWithIdpData(
                companyUser,
                companyUser.Company!.IdentityProviders.Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                    .SingleOrDefault()!
            ))
            .SingleOrDefaultAsync();

        public IAsyncEnumerable<CompanyUser> GetCompanyUserRolesIamUsersAsync(IEnumerable<Guid> companyUserIds, string iamUserId) =>
            _dbContext.CompanyUsers
                .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)
                .SelectMany(companyUser => companyUser.Company!.CompanyUsers)
                .Where(companyUser => companyUserIds.Contains(companyUser.Id) && companyUser.IamUser!.UserEntityId != null)
                .Include(companyUser => companyUser.CompanyUserAssignedRoles)
                .Include(companyUser => companyUser.IamUser)
                .AsAsyncEnumerable();

        public IAsyncEnumerable<CompanyUserDetails> GetCompanyUserDetailsUntrackedAsync(
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
                .Select(companyUser => new CompanyUserDetails(
                    companyUser.IamUser!.UserEntityId,
                    companyUser.Id,
                    companyUser.CompanyUserStatusId)
                {
                    FirstName = companyUser.Firstname,
                    LastName = companyUser.Lastname,
                    Email = companyUser.Email
                })
                .AsAsyncEnumerable();

        public Task<Guid> GetCompanyIdForIamUserUntrackedAsync(string iamUserId) =>
            _dbContext.IamUsers
                .AsNoTracking()
                .Where(iamUser =>
                    iamUser.UserEntityId == iamUserId)
                .Select(iamUser =>
                    iamUser.CompanyUser!.Company!.Id)
                .SingleOrDefaultAsync();

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

        public IamServiceAccount RemoveIamServiceAccount(IamServiceAccount iamServiceAccount) =>
            _dbContext.Remove(iamServiceAccount).Entity;

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

        public IAsyncEnumerable<CompanyInvitedUser> GetInvitedUsersByApplicationIdUntrackedAsync(Guid applicationId) =>
            _dbContext.Invitations
                .AsNoTracking()
                .Where(invitation => invitation.CompanyApplicationId == applicationId)
                .Select(invitation => invitation.CompanyUser)
                .Where(companyUser => companyUser!.CompanyUserStatusId == CompanyUserStatusId.ACTIVE)
                .Select(companyUser => new CompanyInvitedUser(
                    companyUser!.Id,
                    companyUser.IamUser!.UserEntityId))
                .AsAsyncEnumerable();

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
        
        public Task<CompanyServiceAccountWithClientId?> GetOwnCompanyServiceAccountWithIamClientIdAsync(Guid serviceAccountId, string adminUserId) =>
            _dbContext.CompanyServiceAccounts
                .Where(serviceAccount =>
                    serviceAccount.Id == serviceAccountId
                    && serviceAccount.CompanyServiceAccountStatusId == CompanyServiceAccountStatusId.ACTIVE
                    && serviceAccount.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId))
                .Select( serviceAccount =>
                    new CompanyServiceAccountWithClientId(
                        serviceAccount,
                        serviceAccount.IamServiceAccount!.ClientId,
                        serviceAccount.IamServiceAccount.ClientClientId
                    )
                )
                .SingleOrDefaultAsync();

        public Task<CompanyServiceAccount?> GetOwnCompanyServiceAccountWithIamServiceAccountAsync(Guid serviceAccountId, string adminUserId) =>
            _dbContext.CompanyServiceAccounts
                .Where(serviceAccount =>
                    serviceAccount.Id == serviceAccountId
                    && serviceAccount.CompanyServiceAccountStatusId == CompanyServiceAccountStatusId.ACTIVE
                    && serviceAccount.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId))
                .Include(serviceAccount => serviceAccount.IamServiceAccount)
                .SingleOrDefaultAsync();

        public Task<CompanyServiceAccountDetailedData?> GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(Guid serviceAccountId, string adminUserId) =>
            _dbContext.CompanyServiceAccounts
                .AsNoTracking()
                .Where(serviceAccount =>
                    serviceAccount.Id == serviceAccountId
                    && serviceAccount.CompanyServiceAccountStatusId == CompanyServiceAccountStatusId.ACTIVE
                    && serviceAccount.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId))
                .Select(serviceAccount => new CompanyServiceAccountDetailedData(
                        serviceAccount.Id,
                        serviceAccount.IamServiceAccount!.ClientId,
                        serviceAccount.IamServiceAccount.ClientClientId,
                        serviceAccount.IamServiceAccount.UserEntityId,
                        serviceAccount.Name,
                        serviceAccount.Description))
                .SingleOrDefaultAsync();

        public Task<Pagination.Source<CompanyServiceAccountData>?> GetOwnCompanyServiceAccountDetailsUntracked(int skip, int take, string adminUserId) =>
            _dbContext.Companies
                .AsNoTracking()
                .Where(company =>
                    company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId))
                .Select(company => new Pagination.Source<CompanyServiceAccountData>(
                    company.CompanyServiceAccounts
                        .Where(serviceAccount => serviceAccount.CompanyServiceAccountStatusId == CompanyServiceAccountStatusId.ACTIVE)
                        .Count(),
                    company.CompanyServiceAccounts
                        .Where(serviceAccount => serviceAccount.CompanyServiceAccountStatusId == CompanyServiceAccountStatusId.ACTIVE)
                        .OrderBy(serviceAccount => serviceAccount.Name)
                        .Skip(skip)
                        .Take(take)
                        .Select(serviceAccount => new CompanyServiceAccountData(
                            serviceAccount.Id,
                            serviceAccount.IamServiceAccount!.ClientClientId,
                            serviceAccount.Name))))
                .SingleOrDefaultAsync();

        public Task<RegistrationData?> GetRegistrationDataAsync(Guid applicationId, string iamUserId) =>
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
                   Streetname = company.Address!.Streetname,
                   CountryAlpha2Code = company.Address!.CountryAlpha2Code,
                   Bpn = company.Bpn,
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

        public Task<int> SaveAsync() =>
            _dbContext.SaveChangesAsync();
    }
}
