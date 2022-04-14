using System.Threading.Tasks;
using CatenaX.NetworkServices.Provisioning.ProvisioningEntities;
using System;

namespace CatenaX.NetworkServices.Provisioning.DBAccess

{
    public interface IProvisioningDBAccess
    {
        Task<int> GetNextClientSequenceAsync();
        Task<int> GetNextIdentityProviderSequenceAsync();
        Task SaveUserPasswordResetInfo(string userEntityId, DateTime passwordModifiedAt, int resetCount);
        Task<UserPasswordReset> GetUserPasswordResetInfo(string userEntityId);
        Task SetUserPassword(string userEntityId, int count);
        Task SetUserPassword(string userEntityId, DateTime dateReset, int count);
    }
}
