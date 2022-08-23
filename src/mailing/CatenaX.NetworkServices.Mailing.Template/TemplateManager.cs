using CatenaX.NetworkServices.Mailing.Template.Attributes;
using CatenaX.NetworkServices.Mailing.Template.Enums;
using CatenaX.NetworkServices.Mailing.Template.Model;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CatenaX.NetworkServices.Mailing.Template
{
    public class TemplateManager : ITemplateManager
    {
        private readonly TemplateSettings _settings;

        public TemplateManager(IOptions<TemplateSettings> templateSettings)
        {
            _settings = templateSettings.Value;
        }

        Mail ITemplateManager.ApplyTemplate(string id, IDictionary<string, string> parameters)
        {
            try
            {
                var template = _settings.Templates[id];
                return new Mail(
                    replaceValues(template.Subject,parameters),
                    replaceValues(template.EmailTemplateType.HasValue 
                        ? GetTemplateStringFromPath(GetTemplatePathFromType(template.EmailTemplateType.Value))
                        : template.Body,parameters),
                    template.EmailTemplateType.HasValue
                );
            }
            catch(ArgumentNullException)
            {
                throw new NoSuchTemplateException(id);
            }
            catch(KeyNotFoundException)
            {
                throw new NoSuchTemplateException(id);
            }
        }

        private static string GetTemplatePathFromType(EmailTemplateType value) =>
             typeof(EmailTemplateType)
                .GetMember(value.ToString())
                .FirstOrDefault(m => m.DeclaringType == typeof(EmailTemplateType))
                .GetCustomAttribute<PathAttribute>().Path;

        private static string GetTemplateStringFromPath(string path) =>
            File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/EmailTemplates/" + path);

        private string replaceValues(string template, IDictionary<string,string> parameters)
        {
            return Regex.Replace(
                template,
                @"\{(\w+)\}", //replaces any text surrounded by { and }
                m =>
                {
                    string value;
                    return parameters.TryGetValue(m.Groups[1].Value, out value) ? value : "null";
                }
            );
        }
    }
}
