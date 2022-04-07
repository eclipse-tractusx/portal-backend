using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace CatenaX.NetworkServices.Keycloak.Factory
{
    public class KeycloakSettings
    {
        public string ConnectionString { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthRealm { get; set; }
    }

    public class KeycloakSettingsMap : Dictionary<string,KeycloakSettings>
    {
    }

    public static class KeycloakSettingsExtention
    {
        public static IServiceCollection ConfigureKeycloakSettingsMap(
            this IServiceCollection services,
            IConfigurationSection section
            )
        {
            return services.Configure<KeycloakSettingsMap>(x => section.Bind(x));
        }

        public static IServiceCollection ConfigureKeycloakSettings(
            this IServiceCollection services,
            IConfigurationSection section
            )
        {
            return services.Configure<KeycloakSettings>(x => section.Bind(x));
        }
    }
}
