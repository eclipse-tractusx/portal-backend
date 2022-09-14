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

        public Task<string> CreateClient(ClientSetupData clientSetupData) =>
            _provisioningManager.SetupClientAsync(clientSetupData.redirectUrl);
    }
}
