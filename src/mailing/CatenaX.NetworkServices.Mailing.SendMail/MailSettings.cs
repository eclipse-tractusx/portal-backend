using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CatenaX.NetworkServices.Mailing.SendMail
{
    public class MailSettings
    {
        public const string Position = "MailingService:Mail";
        public string SmtpHost { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
        public int SmtpPort { get; set; }
        public string HttpProxy { get; set; }
        public int HttpProxyPort { get; set; }
    }
    public static class MailSettingsExtention
    {
        public static IServiceCollection ConfigureMailSettings(
            this IServiceCollection services,
            IConfigurationSection section
            )
        {
            return services.Configure<MailSettings>(x => section.Bind(x));
        }
    }
}
