using System;
using System.Threading.Tasks;
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

        public async Task SetIdp(SetIdp idpToSet)
        {
            using (var connection = _dbConnection.Connection())
            {
                    var parameters = new { companyId = idpToSet.companyId, idp = idpToSet.idp };
                    string sql = $"Insert Into {_dbSchema}.company_selected_idp (company_id, idp) values(@companyId, @idp)";
                    await connection.ExecuteAsync(sql, parameters).ConfigureAwait(false);
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
    }
}
