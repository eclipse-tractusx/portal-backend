
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic
{
    public class UserAdministrationSettings
    {
        public string RegistrationBasePortalAddress { get; set; }
        public UserAdministrationSetting Portal { get; set; }
        public PasswordReset PasswordReset { get; set; }
    }

    public class UserAdministrationSetting
    {
        public string KeyCloakClientID { get; set; }
        public string BasePortalAddress { get; set; }
    }
    public class PasswordReset
    {
        public int NoOfHours { get; set; }
        public int MaxNoOfReset { get; set; }
    }
    public static class UserAdministrationSettingsExtension
    {
        public static IServiceCollection ConfigureUserAdministrationSettings(
            this IServiceCollection services,
            IConfigurationSection section
            )
        {
            return services.Configure<UserAdministrationSettings>(x => section.Bind(x));
        }
    }

}
