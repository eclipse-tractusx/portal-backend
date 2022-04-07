using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CatenaX.NetworkServices.Provisioning.Mail
{
    public class UserEmailSettings
    {
        public string SenderEmail { get; set; }
        public string Template { get; set; }
    }
    public static class UserEmailSettingsExtention
    {
        public static IServiceCollection ConfigureUserEmailSettings(
            this IServiceCollection services,
            IConfigurationSection section
            )
        {
            return services.Configure<UserEmailSettings>(x => section.Bind(x));
        }
    }
}
