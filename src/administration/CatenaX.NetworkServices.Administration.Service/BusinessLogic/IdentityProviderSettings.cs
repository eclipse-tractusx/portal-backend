using CatenaX.NetworkServices.Framework.ErrorHandling;
using System.Text;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class IdentityProviderSettings
{
    public string Charset { get; set; } = null!;
    public Encoding Encoding { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}

public static class IdentityProviderSettingsExtension
{
    public static IServiceCollection ConfigureIdentityProviderSettings(
        this IServiceCollection services,
        IConfigurationSection section) =>
        services.Configure<IdentityProviderSettings>(x =>
            {
                section.Bind(x);
                if (string.IsNullOrWhiteSpace(x.FileName))
                {
                    throw new ConfigurationException($"{nameof(IdentityProviderSettings)}: {nameof(x.FileName)} must not be null or empty");
                }
                if (string.IsNullOrWhiteSpace(x.ContentType))
                {
                    throw new ConfigurationException($"{nameof(IdentityProviderSettings)}: {nameof(x.ContentType)} must not be null or empty");
                }
                try
                {
                    x.Encoding = Encoding.GetEncoding(x.Charset);
                }
                catch(ArgumentException ae)
                {
                    throw new ConfigurationException($"{nameof(IdentityProviderSettings)}: {nameof(x.Charset)} is not a valid Charset", ae);
                }
            });
}
