using CatenaX.NetworkServices.Mailing.Template;

using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace CatenaX.NetworkServices.Mailing.SendMail
{
    public class MailingService : IMailingService
    {
        private readonly ITemplateManager _templateManager;
        private readonly ISendMail _sendMail;

        public MailingService( ITemplateManager templateManager, ISendMail sendMail)
        {
            _templateManager = templateManager;
            _sendMail = sendMail;
        }

        public async Task SendMails(string eMail, Dictionary<string, string> parameters, List<string> templates)
        {
            foreach(var temp in templates)
            {
                var inviteMail = _templateManager.ApplyTemplate(temp, parameters);
                await _sendMail.Send("Notifications@catena-x.net", eMail, inviteMail.Subject, inviteMail.Body, inviteMail.isHtml);
            }
        }
    }
}
