using CatenaX.NetworkServices.Registration.Service.BPN.Model;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Registration.Service.BPN
{
    public interface IBPNAccess
    {
        Task<List<FetchBusinessPartnerDto>> FetchBusinessPartner(string bpn, string token);
    }
}
