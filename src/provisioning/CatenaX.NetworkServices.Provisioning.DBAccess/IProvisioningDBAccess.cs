using System.Threading.Tasks;
using CatenaX.NetworkServices.Provisioning.ProvisioningEntities;
using System;

namespace CatenaX.NetworkServices.Provisioning.DBAccess

{
    public interface IProvisioningDBAccess
    {
        Task<int> GetNextClientSequenceAsync();
        Task<int> GetNextIdentityProviderSequenceAsync();
        Task SaveUserPasswordResetInfo(string userEntityId, DateTimeOffset passwordModifiedAt, int resetCount);
        Task<UserPasswordReset> GetUserPasswordResetInfoNoTracking(string userEntityId);
        Task SetUserPassword(string userEntityId, int count);
        Task SetUserPassword(string userEntityId, DateTimeOffset dateReset, int count);
    }
}
