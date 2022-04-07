using Microsoft.Extensions.Options;

namespace CatenaX.NetworkServices.Framework.DBAccess
{
    public class PostgreConnectionFactories : IDBConnectionFactories
    {
        private readonly DBConnectionSettingsMap _dbSettingsMap;

        public PostgreConnectionFactories(IOptions<DBConnectionSettingsMap> dbConnectionSettingsMap)
        {
            _dbSettingsMap = dbConnectionSettingsMap.Value;
        }

        public IDBConnectionFactory Get(string identifier)
        {
            var dbSettings = _dbSettingsMap[identifier];
            return new PostgreConnectionFactory(dbSettings.ConnectionString, dbSettings.DatabaseSchema);
        }
    }
}
