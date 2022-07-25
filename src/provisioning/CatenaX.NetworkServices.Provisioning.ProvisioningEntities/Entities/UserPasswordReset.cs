using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.Provisioning.ProvisioningEntities
{
    public class UserPasswordReset
    {
        private UserPasswordReset()
        {
            UserEntityId = default!;
            PasswordModifiedAt = default!;
            ResetCount = default!;
        }

        public UserPasswordReset(string UserEntityId, DateTimeOffset PasswordModifiedAt, int ResetCount)
        {
            this.UserEntityId = UserEntityId;
            this.PasswordModifiedAt = PasswordModifiedAt;
            this.ResetCount = ResetCount;
        }

        [Key]
        [StringLength(36)]
        public string UserEntityId { get; private set; }
        public DateTimeOffset PasswordModifiedAt { get; set; }
        public int ResetCount { get; set; }
    }
}
