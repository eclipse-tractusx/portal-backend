namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class RegistrationSettings
{
    public int ApplicationsMaxPageSize { get; set; }
    public IDictionary<string, IEnumerable<string>> ApplicationApprovalInitialRoles { get; set; }
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
