using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Provisioning.DBAccess

{
    public interface IProvisioningDBAccess
    {
        Task<int> GetNextClientSequenceAsync();
        Task<int> GetNextIdentityProviderSequenceAsync();
    }
}
