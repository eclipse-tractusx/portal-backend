using CatenaX.NetworkServices.Framework.ErrorHandling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatenaX.NetworkServices.Provisioning.Library;

public partial class ProvisioningSettings
{
    public string CentralRealm { get; set; } = null!;
    public string IdpPrefix { get; set; }
    public string ClientPrefix { get; set; }
    public string MappedIdpAttribute { get; set; }
    public string MappedCompanyAttribute { get; set; }
    public string MappedBpnAttribute { get; set; }
    public string UserNameMapperTemplate { get; set; }
}

public static class ProvisioningSettingsExtension
{
    public static IServiceCollection ConfigureProvisioningSettings(
        this IServiceCollection services,
        IConfigurationSection section) =>
        services.Configure<ProvisioningSettings>(x =>
            {
                section.Bind(x);
                if (string.IsNullOrWhiteSpace(x.CentralRealm))
                {
                    throw new ConfigurationException($"{nameof(ProvisioningSettings)}: {nameof(x.CentralRealm)} must not be null or empty");
                }
            });
}
