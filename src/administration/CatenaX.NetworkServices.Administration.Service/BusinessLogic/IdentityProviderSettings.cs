using CatenaX.NetworkServices.Framework.ErrorHandling;
using System.Text;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class IdentityProviderSettings
{
    public IdentityProviderCSVSettings CSVSettings { get; set; } = null!;
}

public class IdentityProviderCSVSettings
{
    public string Charset { get; set; } = null!;
    public Encoding Encoding { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string Separator { get; set; } = null!;
    public string HeaderUserId { get; set; } = null!;
    public string HeaderFirstName { get; set; } = null!;
    public string HeaderLastName { get; set; } = null!;
    public string HeaderEmail { get; set; } = null!;
    public string HeaderProviderAlias { get; set; } = null!;
    public string HeaderProviderUserId { get; set; } = null!;
    public string HeaderProviderUserName { get; set; } = null!;
}

public static class IdentityProviderSettingsExtension
{
    public static IServiceCollection ConfigureIdentityProviderSettings(
        this IServiceCollection services,
        IConfigurationSection section) =>
        services.Configure<IdentityProviderSettings>(x =>
            {
                section.Bind(x);
                if(x.CSVSettings == null)
                {
                    throw new ConfigurationException($"{nameof(IdentityProviderSettings)}: {nameof(x.CSVSettings)} must not be null");
                }
                var csvSettings = x.CSVSettings;
                if (string.IsNullOrWhiteSpace(csvSettings.FileName))
                {
                    throw new ConfigurationException($"{nameof(IdentityProviderSettings)}: {nameof(csvSettings.FileName)} must not be null or empty");
                }
                if (string.IsNullOrWhiteSpace(csvSettings.ContentType))
                {
                    throw new ConfigurationException($"{nameof(IdentityProviderSettings)}: {nameof(csvSettings.ContentType)} must not be null or empty");
                }
                try
                {
                    csvSettings.Encoding = Encoding.GetEncoding(csvSettings.Charset);
                }
                catch(ArgumentException ae)
                {
                    throw new ConfigurationException($"{nameof(IdentityProviderSettings)}: {nameof(csvSettings.Charset)} is not a valid Charset", ae);
                }
                if (string.IsNullOrWhiteSpace(csvSettings.Separator))
                {
                    throw new ConfigurationException($"{nameof(IdentityProviderSettings)}: {nameof(csvSettings.Separator)} must not be null or empty");
                }
                if (string.IsNullOrWhiteSpace(csvSettings.HeaderUserId))
                {
                    throw new ConfigurationException($"{nameof(IdentityProviderSettings)}: {nameof(csvSettings.HeaderUserId)} must not be null or empty");
                }
                if (string.IsNullOrWhiteSpace(csvSettings.HeaderFirstName))
                {
                    throw new ConfigurationException($"{nameof(IdentityProviderSettings)}: {nameof(csvSettings.HeaderFirstName)} must not be null or empty");
                }
                if (string.IsNullOrWhiteSpace(csvSettings.HeaderLastName))
                {
                    throw new ConfigurationException($"{nameof(IdentityProviderSettings)}: {nameof(csvSettings.HeaderLastName)} must not be null or empty");
                }
                if (string.IsNullOrWhiteSpace(csvSettings.HeaderEmail))
                {
                    throw new ConfigurationException($"{nameof(IdentityProviderSettings)}: {nameof(csvSettings.HeaderEmail)} must not be null or empty");
                }
                if (string.IsNullOrWhiteSpace(csvSettings.HeaderProviderAlias))
                {
                    throw new ConfigurationException($"{nameof(IdentityProviderSettings)}: {nameof(csvSettings.HeaderProviderAlias)} must not be null or empty");
                }
                if (string.IsNullOrWhiteSpace(csvSettings.HeaderProviderUserId))
                {
                    throw new ConfigurationException($"{nameof(IdentityProviderSettings)}: {nameof(csvSettings.HeaderProviderUserId)} must not be null or empty");
                }
                if (string.IsNullOrWhiteSpace(csvSettings.HeaderProviderUserName))
                {
                    throw new ConfigurationException($"{nameof(IdentityProviderSettings)}: {nameof(csvSettings.HeaderProviderUserName)} must not be null or empty");
                }
            });
}
