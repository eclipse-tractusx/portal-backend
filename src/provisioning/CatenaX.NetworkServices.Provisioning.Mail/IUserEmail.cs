using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Provisioning.Mail
{
    public interface IUserEmail
    {
        Task SendMailAsync(string email, string firstName, string lastName, string realm);
    }
}
