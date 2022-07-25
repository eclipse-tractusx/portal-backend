using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.Mailing.Template;

namespace CatenaX.NetworkServices.Provisioning.Mail
{
    public class UserEmail : IUserEmail
    {
        public static readonly string ProviderPosition = "MailingService:Provider";
        public static readonly string TemplatePosition = "MailingService:Templates";
        public static readonly string UserEmailPosition = "MailingService:UserEmail";
        private readonly UserEmailSettings _Settings;
        private readonly ITemplateManager _TemplateManager;
        private readonly ISendMail _SendMail;

        public UserEmail( ITemplateManager templateManager, ISendMail sendMail, IOptions<UserEmailSettings> settings)
        {
            _TemplateManager = templateManager;
            _SendMail = sendMail;
            _Settings = settings.Value;
        }

        public Task SendMailAsync(string email, string firstName, string lastName, string realm)
        {
            var templateParams = new Dictionary<string, string> {
                { "firstname", firstName },
                { "lastname", lastName },
                { "realm", realm }
            };
            var inviteMail = _TemplateManager.ApplyTemplate(_Settings.Template, templateParams);
            return _SendMail.Send(_Settings.SenderEmail, email, inviteMail.Subject, inviteMail.Body, inviteMail.isHtml);
        }
    }
}
