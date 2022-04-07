using System.Threading.Tasks;
using CatenaX.NetworkServices.Provisioning.ProvisioningEntities;

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
    }
}
