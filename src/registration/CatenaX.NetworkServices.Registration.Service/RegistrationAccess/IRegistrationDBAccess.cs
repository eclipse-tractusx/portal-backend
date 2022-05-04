using CatenaX.NetworkServices.Registration.Service.Model;
using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Registration.Service.RegistrationAccess
{
    public interface IRegistrationDBAccess
    {
        Task SetIdp(SetIdp idpToSet);
        Task UploadDocument(string name,string document, string hash, string username);
    }
}
