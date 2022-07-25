using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Mailing.SendMail
{
    public interface ISendMail
    {
        Task Send(string sender, string recipient, string subject, string body, bool useHtml = false);
    }
}
