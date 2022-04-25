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

        public async Task SaveUserPasswordResetInfo(string userEntityId, DateTimeOffset passwordModifiedAt, int resetCount)
        {
            _dbContext.UserPasswordResets.Add(
                new UserPasswordReset(
                    userEntityId,
                    resetCount
                )
                {
                    PasswordModifiedAt = passwordModifiedAt,
                }
            );
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public Task<UserPasswordReset> GetUserPasswordResetInfoNoTracking(string userEntityId)
        {
            return _dbContext.UserPasswordResets
                .Where(x => x.UserEntityId == userEntityId)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task SetUserPassword(string userEntityId, int count)
        {
            var passwordReset = await _dbContext.UserPasswordResets
                  .Where(x => x.UserEntityId == userEntityId)
                  .SingleAsync();
            passwordReset.ResetCount = count;
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task SetUserPassword(string userEntityId, DateTimeOffset dateReset, int count)
        {
            var passwordReset = await _dbContext.UserPasswordResets
                  .Where(x => x.UserEntityId == userEntityId)
                  .SingleAsync();
            passwordReset.PasswordModifiedAt = dateReset;
            passwordReset.ResetCount = count;
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
