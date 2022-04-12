using System.Threading.Tasks;
using CatenaX.NetworkServices.Provisioning.ProvisioningEntities;
using System;

namespace CatenaX.NetworkServices.Provisioning.DBAccess

{
    public interface IProvisioningDBAccess
    {
        Task<int> GetNextClientSequenceAsync();
        Task<int> GetNextIdentityProviderSequenceAsync();
        Task<bool> SaveUserPasswordResetInfo(Guid userEntityId, DateTime passwordModifiedAt,int resetCount);
        Task<UserPasswordReset> GetUserPasswordResetInfo(Guid userId);
        Task<bool> SetUserPassword(Guid userEntityId,int count);
        Task<bool> SetUserPassword(Guid userEntityId,DateTime dateReset,int count);
    }
}
