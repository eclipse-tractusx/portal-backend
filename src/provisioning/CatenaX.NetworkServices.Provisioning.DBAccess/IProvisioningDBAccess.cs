using System.Threading.Tasks;
using CatenaX.NetworkServices.Provisioning.ProvisioningEntities;
using System;

namespace CatenaX.NetworkServices.Provisioning.DBAccess

{
    public interface IProvisioningDBAccess
    {
        Task<int> GetNextClientSequenceAsync();
        Task<int> GetNextIdentityProviderSequenceAsync();
        UserPasswordReset CreateUserPasswordResetInfo(string userEntityId, DateTimeOffset passwordModifiedAt, int resetCount);
        Task<UserPasswordReset> GetUserPasswordResetInfo(string userEntityId);
        Task<int> SaveAsync();
    }
}
