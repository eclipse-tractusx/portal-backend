using System.Data;
using Microsoft.Extensions.Options;
using Npgsql;

namespace CatenaX.NetworkServices.Framework.DBAccess

{
    public class PostgreConnectionFactory : IDBConnectionFactory
    {
        private readonly string _ConnectionString;
        private readonly string _Schema;

        public PostgreConnectionFactory(IOptions<DBConnectionSettings> options)
        {
            _ConnectionString = options.Value.ConnectionString;
            _Schema = options.Value.DatabaseSchema;
        }

        public PostgreConnectionFactory(string connectionString, string databaseSchema)
        {
            _ConnectionString = connectionString;
            _Schema = databaseSchema;
        }

        public IDbConnection Connection() =>
            new NpgsqlConnection(_ConnectionString);

        public string Schema()
        {
            return _Schema;
        }
    }
}
