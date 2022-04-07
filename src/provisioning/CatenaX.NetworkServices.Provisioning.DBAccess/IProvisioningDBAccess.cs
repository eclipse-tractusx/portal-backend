using System.Threading.Tasks;
using CatenaX.NetworkServices.Provisioning.DBAccess.Model;

namespace CatenaX.NetworkServices.Provisioning.DBAccess

{
    public interface IProvisioningDBAccess
    {
        Task<Sequence> GetNextClientSequenceAsync();
        Task<Sequence> GetNextIdentityProviderSequenceAsync();
    }
}
