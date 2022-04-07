using System.Collections.Generic;
using System.Threading.Tasks;

using Dapper;

using CatenaX.NetworkServices.Framework.DBAccess;
using CatenaX.NetworkServices.Keycloak.DBAccess.Model;

namespace CatenaX.NetworkServices.Keycloak.DBAccess

{
    public class KeycloakDBAccess : IKeycloakDBAccess
    {
        private readonly IDBConnectionFactory _dbConnection;
        private readonly string _dbSchema;

        public KeycloakDBAccess(IDBConnectionFactories dBConnectionFactories)
        : this(dBConnectionFactories.Get("Keycloak"))
        {
        }

        public KeycloakDBAccess(IDBConnectionFactory dbConnection)
        {
            _dbConnection = dbConnection;
            _dbSchema = _dbConnection.Schema();
        }

        public async Task<IEnumerable<UserJoinedFederatedIdentity>> GetUserJoinedFederatedIdentityAsync(
                string idpName,
                string realmId,
                string userId = null,
                string providerUserId = null,
                string userName = null,
                string firstName = null,
                string lastName = null,
                string email = null
            )
        {
            string sql =
                "SELECT id, email, enabled, first_name, last_name, federated_user_id, federated_username" +
                $" FROM {_dbSchema}.user_entity c JOIN {_dbSchema}.federated_identity f" +
                " ON c.realm_id = f.realm_id AND c.id = f.user_id" +
                " WHERE f.identity_provider = @idpName AND f.realm_id = @realmId" +
                (userId         == null ? "" : " AND c.id = @userId") +
                (providerUserId == null ? "" : " AND f.federated_user_id = @providerUserId") +
                (userName       == null ? "" : " AND f.federated_username = @userName") +
                (firstName      == null ? "" : " AND c.first_name = @firstName") +
                (lastName       == null ? "" : " AND c.last_name = @lastName") + 
                (email          == null ? "" : " AND c.email = @email");
            using (var connection = _dbConnection.Connection())
            {
                return await connection.QueryAsync<UserJoinedFederatedIdentity>(sql, new {
                        idpName        = idpName,
                        realmId        = realmId,
                        userId         = userId,
                        providerUserId = providerUserId,
                        userName       = userName,
                        firstName      = firstName,
                        lastName       = lastName,
                        email          = email
                    }).ConfigureAwait(false);
            }
        }
    }
}
