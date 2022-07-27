using CatenaX.NetworkServices.Provisioning.ProvisioningEntities;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.Provisioning.DBAccess;

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

    public UserPasswordReset CreateUserPasswordResetInfo(string userEntityId, DateTimeOffset passwordModifiedAt, int resetCount) =>
        _dbContext.UserPasswordResets.Add(
            new UserPasswordReset(
                userEntityId,
                passwordModifiedAt,
                resetCount
            )
        ).Entity;

    public Task<UserPasswordReset?> GetUserPasswordResetInfo(string userEntityId)
    {
        return _dbContext.UserPasswordResets
            .Where(x => x.UserEntityId == userEntityId)
            .SingleOrDefaultAsync();
    }

    public Task<int> SaveAsync() =>
        _dbContext.SaveChangesAsync();
}
