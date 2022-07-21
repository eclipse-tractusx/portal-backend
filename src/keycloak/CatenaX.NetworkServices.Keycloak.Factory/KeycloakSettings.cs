using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CatenaX.NetworkServices.Keycloak.Factory;

public class KeycloakSettings
{
    public KeycloakSettings()
    {
        ConnectionString = null!;
    }

    public string ConnectionString { get; set; }
    public string? User { get; set; }
    public string? Password { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? AuthRealm { get; set; }
}

public class KeycloakSettingsMap : Dictionary<string,KeycloakSettings>
{
}

public static class KeycloakSettingsExtention
{
    public static IServiceCollection ConfigureKeycloakSettingsMap(
        this IServiceCollection services,
        IConfigurationSection section) =>
        services.Configure<KeycloakSettingsMap>(settingsMap =>
            {
                section.Bind(settingsMap);
                foreach (var (key, settings) in settingsMap)
                {
                    if (settings.ConnectionString == null)
                    {
                        throw new Exception($"{nameof(KeycloakSettings)}: {nameof(settings.ConnectionString)} must not be null");
                    }
                    if ((settings.User == null || settings.Password == null) && (settings.ClientId == null || settings.ClientSecret == null))
                    {
                        if (settings.User != null)
                        {
                            throw new Exception($"{nameof(KeycloakSettings)}, Key {key}: {nameof(settings.Password)} must not be null if {nameof(settings.User)} has a non-null value");
                        }
                        if (settings.Password != null)
                        {
                            throw new Exception($"{nameof(KeycloakSettings)}, Key {key}: {nameof(settings.User)} must not be null if {nameof(settings.Password)} has a non-null value");
                        }
                        if (settings.ClientId != null)
                        {
                            throw new Exception($"{nameof(KeycloakSettings)}, Key {key}: {nameof(settings.ClientSecret)} must not be null if {nameof(settings.ClientId)} has a non-null value");
                        }
                        if (settings.ClientSecret != null)
                        {
                            throw new Exception($"{nameof(KeycloakSettings)}, Key {key}1: {nameof(settings.ClientId)} must not be null if {nameof(settings.ClientSecret)} has a non-null value");
                        }
                    }
                }
            });
}
