using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CatenaX.NetworkServices.Framework.DBAccess

{
    public class DBConnectionSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseSchema { get; set; }
    }

    public class DBConnectionSettingsMap : Dictionary<string,DBConnectionSettings>
    {
    }

    public static class DBConnectionSettingsExtention
    {
        public static IServiceCollection ConfigureDBConnectionSettings(
            this IServiceCollection services,
            IConfigurationSection section
            )
        {
            return services.Configure<DBConnectionSettings>(x => section.Bind(x));
        }
        public static IServiceCollection ConfigureDBConnectionSettingsMap(
            this IServiceCollection services,
            IConfigurationSection section
            )
        {
            return services.Configure<DBConnectionSettingsMap>(x => section.Bind(x));
        }
    }
}
