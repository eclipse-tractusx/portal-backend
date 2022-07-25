using System.Threading.Tasks;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Service.Models;

namespace CatenaX.NetworkServices.Provisioning.Service.BusinessLogic
{
    public class ClientBusinessLogic : IClientBusinessLogic
    {
        private readonly IProvisioningManager _provisioningManager;

        public ClientBusinessLogic(IProvisioningManager provisioningManager)
        {
            _provisioningManager = provisioningManager;
        }

        public async Task<string> CreateClient(ClientSetupData clientData)
        {
            return await _provisioningManager.SetupClientAsync(
                clientData.redirectUrl
            ).ConfigureAwait(false);
        }
    }
}
