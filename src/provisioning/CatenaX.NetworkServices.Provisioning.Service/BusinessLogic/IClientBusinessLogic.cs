using System.Threading.Tasks;

using CatenaX.NetworkServices.Provisioning.Service.Models;

namespace CatenaX.NetworkServices.Provisioning.Service.BusinessLogic

{
    public interface IClientBusinessLogic
    {
        Task<string> CreateClient(ClientSetupData clientSetupData);
    }
}
