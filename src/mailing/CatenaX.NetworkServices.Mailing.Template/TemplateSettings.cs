using CatenaX.NetworkServices.Mailing.Template.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatenaX.NetworkServices.Mailing.Template
{
    public class TemplateSettings
    {
        public TemplateSettings()
        {
            Templates = new Dictionary<string, TemplateSetting>();
        }

        public Dictionary<string, TemplateSetting> Templates { get; set; }
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
            IConfigurationSection section)
        {
            services.AddOptions<TemplateSettings>()
                .Bind(section)
                .ValidateOnStart();
            return services;
        }
    }
}
