using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

using CatenaX.NetworkServices.Provisioning.DBAccess.Model;
using CatenaX.NetworkServices.Framework.DBAccess;

namespace CatenaX.NetworkServices.Provisioning.DBAccess

{
    public class ProvisioningDBAccess : IProvisioningDBAccess
    {
        private readonly IDBConnectionFactory _DBConnection;
        private readonly string _dbSchema;

        public ProvisioningDBAccess(IDBConnectionFactories dbConnectionFactories)
           : this(dbConnectionFactories.Get("Provisioning"))
        {
        }

        public ProvisioningDBAccess(IDBConnectionFactory dbConnectionFactory)
        {
            _DBConnection = dbConnectionFactory;
            _dbSchema = dbConnectionFactory.Schema();
        }

        public async Task<Sequence> GetNextClientSequenceAsync()
        {
            string sql =
                $"INSERT INTO {_dbSchema}.client_sequence VALUES(DEFAULT) RETURNING sequence_id";
            using (var connection = _DBConnection.Connection())
            {
                return await connection.QueryFirstAsync<Sequence>(sql).ConfigureAwait(false);
            }
        }

        public async Task<Sequence> GetNextIdentityProviderSequenceAsync()
        {
            string sql =
                $"INSERT INTO {_dbSchema}.identity_provider_sequence VALUES(DEFAULT) RETURNING sequence_id";
            using (var connection = _DBConnection.Connection())
            {
                return await connection.QueryFirstAsync<Sequence>(sql).ConfigureAwait(false);
            }
        }
    }
}
