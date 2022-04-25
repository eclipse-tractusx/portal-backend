using System;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.Provisioning.ProvisioningEntities
{
    public class UserPasswordReset
    {
        private UserPasswordReset()
        {
            SharedUserEntityId = default!;
            ResetCount = default!;
        }

        public UserPasswordReset(string SharedUserEntityId, int ResetCount)
        {
            this.SharedUserEntityId = SharedUserEntityId;
            this.ResetCount = ResetCount;
        }

        [Key]
        [StringLength(36)]
        public string SharedUserEntityId { get; private set; }
        public DateTimeOffset? PasswordModifiedAt { get; set; }
        public int ResetCount { get; set; }
    }
}
