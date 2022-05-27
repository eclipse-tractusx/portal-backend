namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class RegistrationSettings
{
    public RegistrationSettings()
    {
        ApplicationApprovalInitialRoles = null!;
    }

    public int ApplicationsMaxPageSize { get; set; }
    public IDictionary<string, IEnumerable<string>> ApplicationApprovalInitialRoles { get; set; }
}

public static class RegistrationSettingsExtension
{
    public static IServiceCollection ConfigureRegistrationSettings(
        this IServiceCollection services,
        IConfigurationSection section) =>
        services.Configure<RegistrationSettings>(x =>
            {
                section.Bind(x);
                if (x.ApplicationApprovalInitialRoles == null)
                {
                    throw new Exception($"{nameof(RegistrationSettings)}: {nameof(x.ApplicationApprovalInitialRoles)} must not be null");
                }
            });
}
