using CatenaX.NetworkServices.Administration.Service.Custodian.Models;

using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Administration.Service.Custodian
{
    public interface ICustodianService
    {
        public Task<List<GetWallets>> GetWallets();

        public Task CreateWallet(string bpn, string name);
    }
}
