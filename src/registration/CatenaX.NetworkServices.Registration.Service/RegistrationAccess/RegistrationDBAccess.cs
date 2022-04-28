using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatenaX.NetworkServices.Consent.Library.Data;
using CatenaX.NetworkServices.Framework.DBAccess;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.Registration.Service.Model;
using Microsoft.EntityFrameworkCore;

using Dapper;

namespace CatenaX.NetworkServices.Registration.Service.RegistrationAccess
{
    public class RegistrationDBAccess : IRegistrationDBAccess
    {
        private readonly IDBConnectionFactory _dbConnection;
        private readonly string _dbSchema;
        private readonly PortalDbContext _dbContext;

        public RegistrationDBAccess(IDBConnectionFactories dBConnectionFactories, PortalDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbConnection = dBConnectionFactories.Get("Registration");
            _dbSchema = _dbConnection.Schema();
        }

        public async Task<IEnumerable<CompanyRole>> GetAllCompanyRoles()
        {
            string sql = $"SELECT * from {_dbSchema}.companyroles";

            using (var connection = _dbConnection.Connection())
            {
                return await connection.QueryAsync<CompanyRole>(sql).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<ConsentForCompanyRole>> GetConsentForCompanyRole(int roleId)
        {
            var sql = $"select * from get_company_role({roleId})";

            using (var connection = _dbConnection.Connection())
            {
                return await connection.QueryAsync<ConsentForCompanyRole>(sql).ConfigureAwait(false);
            }
        }

        public async Task SetCompanyRoles(CompanyToRoles rolesToSet)
        {
            foreach (var role in rolesToSet.roles)
            {
                var parameters = new { roleId = role, companyId = rolesToSet.CompanyId };
                string sql = $"Insert Into {_dbSchema}.company_selected_roles (company_id, role_id) values(@companyId, @roleId)";

                using (var connection = _dbConnection.Connection())
                {
                    await connection.ExecuteAsync(sql, parameters).ConfigureAwait(false);
                }
            }
        }

        public async Task SetIdp(SetIdp idpToSet)
        {
            using (var connection = _dbConnection.Connection())
            {
                    var parameters = new { companyId = idpToSet.companyId, idp = idpToSet.idp };
                    string sql = $"Insert Into {_dbSchema}.company_selected_idp (company_id, idp) values(@companyId, @idp)";
                    await connection.ExecuteAsync(sql, parameters).ConfigureAwait(false);
            }
        }

        public async Task SignConsent(SignConsentRequest signedConsent)
        {
            var sql = $"SELECT sign_consent('{signedConsent.companyId}',{signedConsent.consentId},{signedConsent.companyRoleId}, '{signedConsent.userName}')";

            using (var connection = _dbConnection.Connection())
            {
                await connection.ExecuteAsync(sql).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<SignedConsent>> SignedConsentsByCompanyId(string companyId)
        {
            var sql = $"select * from get_signed_consents_for_company_id('{companyId}')";
            using (var connection = _dbConnection.Connection())
            {
                return await connection.QueryAsync<SignedConsent>(sql).ConfigureAwait(false);
            }
        }

        public async Task UploadDocument(string name, string document, string hash, string username)
        {
            var parameters = new { documentName = name, document = document, documentHash = hash, documentuser = username, documentuploaddate = DateTime.UtcNow };
            string sql = $"Insert Into {_dbSchema}.documents (documentName, document, documentHash, documentuser, documentuploaddate) values(@documentName, @document, @documentHash, @documentuser, @documentuploaddate)";
            using (var connection = _dbConnection.Connection())
            {
                await connection.ExecuteAsync(sql, parameters).ConfigureAwait(false);
            }
        }

        public IAsyncEnumerable<AgreementConsentStatus> GetAgreementConsentStatusNoTrackingAsync(Guid applicationId) =>
            _dbContext.CompanyApplications
                .AsQueryable()
                .AsNoTracking()
                .Where(application => application.Id == applicationId)
                .SelectMany(application => application.Company!.Consents.Select(consent =>
                    new AgreementConsentStatus(
                        consent.ConsentStatusId,
                        consent.Agreement!.AgreementCategoryId,
                        consent.Agreement!.Name
                    ){
                        AgreementType = consent.Agreement!.AgreementType!
                    }))
                .AsAsyncEnumerable();
    }
}
