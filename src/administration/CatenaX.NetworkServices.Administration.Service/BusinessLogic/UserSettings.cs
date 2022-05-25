using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic
{
    public class UserSettings
    {
        public string RegistrationBasePortalAddress { get; set; }
        public UserSetting Portal { get; set; }
        public PasswordReset PasswordReset { get; set; }
        public IDictionary<string, IEnumerable<string>> ApplicationApprovalInitialRoles { get; set; }
    }

    public class UserSetting
    {
        public string KeyCloakClientID { get; set; }
        public string BasePortalAddress { get; set; }
    }
    public class PasswordReset
    {
        public int NoOfHours { get; set; }
        public int MaxNoOfReset { get; set; }
    }
    public static class UserSettingsExtension
    {
        public static IServiceCollection ConfigureUserSettings(
            this IServiceCollection services,
            IConfigurationSection section
            )
        {
            return services.Configure<UserSettings>(x => section.Bind(x));
        }
    }

}
