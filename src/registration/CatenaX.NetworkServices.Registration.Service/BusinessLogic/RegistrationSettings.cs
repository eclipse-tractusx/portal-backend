using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatenaX.NetworkServices.Registration.Service.BusinessLogic
{
    public class RegistrationSettings
    {
        public string KeyCloakClientID { get; set; }
        public string BasePortalAddress { get; set; }
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

}
