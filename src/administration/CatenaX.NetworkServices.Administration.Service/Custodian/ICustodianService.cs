using CatenaX.NetworkServices.Administration.Service.Custodian.Models;

namespace CatenaX.NetworkServices.Administration.Service.Custodian;

public interface ICustodianService
{
    public Task<List<GetWallets>> GetWallets();

    public Task CreateWallet(string bpn, string name);
}
