using CatenaX.NetworkServices.Mailing.Template.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace CatenaX.NetworkServices.Mailing.Template
{
    public class TemplateSettings: Dictionary<string,TemplateSetting>
    {
        public const string Position = "MailingService:Templates";
    }

    /// <summary>
    /// Configuration for templated emails that a service can send.
    /// </summary>
    public class TemplateSetting
    {
        /// <summary>
        /// Subject of the email to be sent.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Body of the email to be sent (in case of non-html-templated emails)
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Template type to be used for the email (in case of html template).
        /// </summary>
        public EmailTemplateType? EmailTemplateType { get; set; }
    }
    public static class TemplateSettingsExtention
    {
        public static IServiceCollection ConfigureTemplateSettings(
            this IServiceCollection services,
            IConfigurationSection section
            )
        {
            return services.Configure<TemplateSettings>(x => section
                .GetChildren()
                .Aggregate(x,(y,z) => {
                    y.Add(z.Key,z.Get<TemplateSetting>());
                    return y;
                }));
        }
    }
}
