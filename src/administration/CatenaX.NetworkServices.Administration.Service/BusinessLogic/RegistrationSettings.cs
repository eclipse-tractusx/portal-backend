namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class RegistrationSettings
{
    public int ApplicationsPageSize { get; set; }
}

public static class RegistrationSettingsExtension
{
    public static IServiceCollection ConfigureRegistrationSettings(
        this IServiceCollection services,
        IConfigurationSection section
        )
    {
        return services.Configure<RegistrationSettings>(x => section.Bind(x));
    }
}
