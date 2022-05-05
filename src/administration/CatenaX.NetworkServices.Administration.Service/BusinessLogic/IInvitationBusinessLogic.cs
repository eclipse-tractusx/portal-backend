using System.Threading.Tasks;
using CatenaX.NetworkServices.Administration.Service.Models;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic
{
    public interface IInvitationBusinessLogic
    {
        Task ExecuteInvitation(CompanyInvitationData invitationData);
    }
}
