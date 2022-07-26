using CatenaX.NetworkServices.Framework.DBAccess;
using CatenaX.NetworkServices.Registration.Service.Model;
using Dapper;

namespace CatenaX.NetworkServices.Registration.Service.RegistrationAccess
{
    public class RegistrationDBAccess : IRegistrationDBAccess
    {
        private readonly IDBConnectionFactory _dbConnection;
        private readonly string _dbSchema;

        public RegistrationDBAccess(IDBConnectionFactories dBConnectionFactories)
        {
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
    }
}
