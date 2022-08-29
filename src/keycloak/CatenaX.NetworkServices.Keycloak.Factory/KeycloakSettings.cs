using CatenaX.NetworkServices.Framework.ErrorHandling;
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

    public void Validate(string key)
    {
        if (ConnectionString == null)
        {
            throw new ConfigurationException($"{nameof(KeycloakSettings)}: {nameof(ConnectionString)} must not be null");
        }

        if ((User != null && Password != null) ||
            (ClientId != null && ClientSecret != null)) return;

        new ConfigurationValidation<KeycloakSettings>()
            .NotNullOrWhiteSpace(User, () => nameof(User))
            .NotNullOrWhiteSpace(Password, () => nameof(Password))
            .NotNullOrWhiteSpace(ClientId, () => nameof(ClientId))
            .NotNullOrWhiteSpace(ClientSecret, () => nameof(ClientSecret));
    }
}

public class KeycloakSettingsMap : Dictionary<string, KeycloakSettings>
{
    public bool Validate()
    {
        if (!Values.Any())
        {
            throw new ConfigurationException();
        }

        foreach (var (key, settings) in this)
        {
            settings.Validate(key);
        }

        return true;
    }
}

public static class KeycloakSettingsExtention
{
    public static IServiceCollection ConfigureKeycloakSettingsMap(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<KeycloakSettingsMap>()
                    .Bind(section)
                    .Validate(x => x.Validate())
                    .ValidateOnStart();
        return services;
    }
}
