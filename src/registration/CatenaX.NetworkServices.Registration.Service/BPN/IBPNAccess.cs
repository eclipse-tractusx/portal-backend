using CatenaX.NetworkServices.Registration.Service.BPN.Model;

namespace CatenaX.NetworkServices.Registration.Service.BPN
{
    public interface IBPNAccess
    {
        Task<List<FetchBusinessPartnerDto>> FetchBusinessPartner(string bpn, string token);
    }
}
