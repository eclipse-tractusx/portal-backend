using System.Collections.Generic;
using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Mailing.SendMail
{
    public interface IMailingService
    {
        Task SendMails(string eMail, Dictionary<string, string> parameters, List<string> templates);
    }
}
