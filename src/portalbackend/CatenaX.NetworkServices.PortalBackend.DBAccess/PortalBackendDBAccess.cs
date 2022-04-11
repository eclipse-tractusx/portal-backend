using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
namespace CatenaX.NetworkServices.PortalBackend.DBAccess

{
    public class PortalBackendDBAccess : IPortalBackendDBAccess
    {
        private readonly PortalDBContext _dbContext;

        public PortalBackendDBAccess(PortalDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<string> GetBpnForUserUntrackedAsync(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            return _dbContext.IamUsers
                    .Where(iamUser => 
                        iamUser.UserEntityId == userId
                        && iamUser.CompanyUser.Company.Bpn != null)
                    .Select(iamUser => iamUser.CompanyUser.Company.Bpn)
                    .AsNoTracking()
                    .SingleAsync();
        }

        public IAsyncEnumerable<UserBpn> GetBpnForUsersUntrackedAsync(IEnumerable<string> userIds)
        {
            return _dbContext.IamUsers
                    .Where(iamUser => 
                        userIds.Contains(iamUser.UserEntityId)
                        && iamUser.CompanyUser.Company.Bpn != null)
                    .Select(iamUser => new UserBpn {
                        userId = iamUser.UserEntityId,
                        bpn = iamUser.CompanyUser.Company.Bpn
                    })
                    .AsNoTracking()
                    .AsAsyncEnumerable();
        }

        public IAsyncEnumerable<string> GetIdpAliaseForCompanyIdUntrackedAsync(Guid companyId)
        {
            if (companyId == null)
            {
                throw new ArgumentNullException(nameof(companyId));
            }
            return _dbContext.CompanyIdentityProviders
                .Where(cip => cip.CompanyId == companyId
                    && cip.IdentityProvider.IamIdentityProvider.IamIdpAlias != null)
                .Select(cip => cip.IdentityProvider.IamIdentityProvider.IamIdpAlias)
                .AsNoTracking()
                .AsAsyncEnumerable();
        }

        public Company CreateCompany(string companyName)
        {
            return _dbContext.Companies.Add(new Company {
                Id = Guid.NewGuid(),
                Name = companyName,
                Shortname = companyName,
                CompanyStatusId = CompanyStatusId.PENDING
            }).Entity;
        }

        public CompanyApplication CreateCompanyApplication(Company company)
        {
            return _dbContext.CompanyApplications.Add(new CompanyApplication {
                Id = Guid.NewGuid(),
                ApplicationStatusId = CompanyApplicationStatusId.ADD_COMPANY_DATA,
                Company = company
            }).Entity;
        }

        public CompanyUser CreateCompanyUser(string firstName, string lastName, string email, Guid companyId)
        {
            return _dbContext.CompanyUsers.Add(new CompanyUser {
                Id = Guid.NewGuid(),
                Firstname = firstName,
                Lastname = lastName,
                Email = email,
                CompanyId = companyId
            }).Entity;
        }

        public Invitation CreateInvitation(Guid applicationId, CompanyUser user)
        {
            return _dbContext.Invitations.Add(new Invitation {
                Id = Guid.NewGuid(),
                InvitationStatusId = InvitationStatusId.CREATED,
                CompanyApplicationId = applicationId,
                CompanyUser = user
            }).Entity;
        }

        public IdentityProvider CreateSharedIdentityProvider(Company company)
        {
            var idp = new IdentityProvider() {
                Id = Guid.NewGuid(),
                IdentityProviderCategoryId = IdentityProviderCategoryId.KEYCLOAK_SHARED,
            };
            idp.Companies.Add(company);
            return _dbContext.IdentityProviders.Add(idp).Entity;
        }

        public IamIdentityProvider CreateIamIdentityProvider(IdentityProvider identityProvider, string idpAlias)
        {
            return _dbContext.IamIdentityProviders.Add(new IamIdentityProvider(idpAlias) {
                IdentityProvider = identityProvider
            }).Entity;
        }

        public IamUser CreateIamUser(CompanyUser user, string iamUserEntityId)
        {
            return _dbContext.IamUsers.Add(new IamUser {
                CompanyUser = user,
                UserEntityId = iamUserEntityId
            }).Entity;
        }

        public IAsyncEnumerable<CompanyApplicationWithStatus> GetApplicationsWithStatusUntrackedAsync(string iamUserId) =>
            _dbContext.IamUsers
                .Where(iamUser => iamUser.UserEntityId == iamUserId)
                .SelectMany(iamUser => iamUser.CompanyUser.Company.CompanyApplications)
                    .Select(companyApplication => new CompanyApplicationWithStatus {
                        ApplicationId = companyApplication.Id,
                        ApplicationStatus = companyApplication.ApplicationStatusId
                    })
                .AsAsyncEnumerable();

        public Task<CompanyWithAddress> GetCompanyWithAdressUntrackedAsync(Guid companyApplicationId) =>
            _dbContext.CompanyApplications
                .Where(companyApplication => companyApplication.Id == companyApplicationId)
                .Select(
                    companyApplication => new CompanyWithAddress {
                        CompanyId = companyApplication.CompanyId,
                        Bpn = companyApplication.Company.Bpn,
                        Name = companyApplication.Company.Name,
                        Shortname = companyApplication.Company.Shortname,
                        City = companyApplication.Company.Address.City,
                        Region = companyApplication.Company.Address.Region,
                        Streetadditional = companyApplication.Company.Address.Streetadditional,
                        Streetname = companyApplication.Company.Address.Streetname,
                        Streetnumber = companyApplication.Company.Address.Streetnumber,
                        Zipcode = companyApplication.Company.Address.Zipcode,
                        CountryAlpha2Code = companyApplication.Company.Address.CountryAlpha2Code,
                        CountryDe = companyApplication.Company.Address.Country.CountryNameDe // FIXME internationalization, maybe move to separate endpoint that returns Contrynames for all (or a specific) language
                    })
                .AsNoTracking()
                .SingleAsync();

        public async Task SetCompanyWithAdressAsync(Guid companyApplicationId, CompanyWithAddress companyWithAddress)
        {
            var company = (await _dbContext.CompanyApplications
                .Include(companyApplication => companyApplication.Company)
                .ThenInclude(company => company.Address)
                .Where(companyApplication => companyApplication.Id == companyApplicationId && companyApplication.Company.Id == companyWithAddress.CompanyId)
                .SingleAsync()
                .ConfigureAwait(false)).Company;
            company.Bpn = companyWithAddress.Bpn;
            company.Name = companyWithAddress.Name;
            company.Shortname = companyWithAddress.Shortname;
            company.Address.City = companyWithAddress.City;
            company.Address.Region = companyWithAddress.Region;
            company.Address.Streetadditional = companyWithAddress.Streetadditional;
            company.Address.Streetname = companyWithAddress.Streetname;
            company.Address.Streetnumber = companyWithAddress.Streetnumber;
            company.Address.Zipcode = companyWithAddress.Zipcode;
            company.Address.CountryAlpha2Code = companyWithAddress.CountryAlpha2Code;
            await _dbContext.SaveChangesAsync();
        }

        public Task<CompanyNameIdWithIdpAlias> GetCompanyNameIdWithSharedIdpAliasUntrackedAsync(Guid companyApplicationId) =>
            _dbContext.CompanyApplications
                .Where(companyApplication => companyApplication.Id == companyApplicationId)
                .Select(
                    companyApplication => new CompanyNameIdWithIdpAlias {
                        CompanyName = companyApplication.Company.Name,
                        CompanyId = companyApplication.CompanyId,
                        IdpAlias = companyApplication.Company.IdentityProviders
                            .Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                            .Select(identityProvider => identityProvider.IamIdentityProvider.IamIdpAlias)
                            .Single()
                    }
                )
            .AsNoTracking()
            .SingleAsync();

        public async Task<int> UpdateApplicationStatusAsync(Guid applicationId, CompanyApplicationStatusId applicationStatus)
        {
            (await _dbContext.CompanyApplications
                .Where(application => application.Id == applicationId)
                .SingleAsync().ConfigureAwait(false))
                .ApplicationStatusId = applicationStatus;
            return await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public Task<CompanyApplicationStatusId?> GetApplicationStatusAsync(Guid applicationId)
        {
            return _dbContext.CompanyApplications
                .Where(application => application.Id == applicationId)
                .AsNoTracking()
                .Select(application => application.ApplicationStatusId)
                .SingleAsync();
        }

        public Task<int> SaveAsync() =>
            _dbContext.SaveChangesAsync();
    }
}
