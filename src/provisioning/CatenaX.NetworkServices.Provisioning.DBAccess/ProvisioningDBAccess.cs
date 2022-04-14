using System.Threading.Tasks;
using CatenaX.NetworkServices.Provisioning.ProvisioningEntities;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.Provisioning.DBAccess

{
    public class ProvisioningDBAccess : IProvisioningDBAccess
    {
        private readonly ProvisioningDBContext _dbContext;

        public ProvisioningDBAccess(ProvisioningDBContext provisioningDBContext)
        {
            _dbContext = provisioningDBContext;
        }

        public async Task<int> GetNextClientSequenceAsync()
        {
            var nextSequence = _dbContext.ClientSequences.Add(new ClientSequence()).Entity;
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            return nextSequence.SequenceId;
        }

        public async Task<int> GetNextIdentityProviderSequenceAsync()
        {
            var nextSequence = _dbContext.IdentityProviderSequences.Add(new IdentityProviderSequence()).Entity;
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            return nextSequence.SequenceId;
        }

        public async Task SaveUserPasswordResetInfo(string userEntityId, DateTime passwordModifiedAt, int resetCount)
        {
            await _dbContext.UserPasswordResets.AddAsync
            (
                new UserPasswordReset
                {
                    SharedUserEntityId = userEntityId,
                    PasswordModifiedAt = passwordModifiedAt,
                    ResetCount = resetCount,
                }
            );
            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserPasswordReset> GetUserPasswordResetInfo(string userEntityId)
        {
            return await _dbContext.UserPasswordResets
            .Where(x => x.SharedUserEntityId == userEntityId)
            .Select(
              userPasswordReset => new UserPasswordReset
              {
                  PasswordModifiedAt = userPasswordReset.PasswordModifiedAt,
                  ResetCount = userPasswordReset.ResetCount,
              }).FirstOrDefaultAsync();
        }

        public async Task SetUserPassword(string userEntityId, int count)
        {
            var passwordReset = await _dbContext.UserPasswordResets
                  .Where(x => x.SharedUserEntityId == userEntityId)
                  .SingleAsync();
            passwordReset.ResetCount = count;
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetUserPassword(string userEntityId, DateTime dateReset, int count)
        {
            var passwordReset = await _dbContext.UserPasswordResets
                  .Where(x => x.SharedUserEntityId == userEntityId)
                  .SingleAsync();
            passwordReset.PasswordModifiedAt = dateReset;
            passwordReset.ResetCount = count;
            await _dbContext.SaveChangesAsync();
        }
    }
}
