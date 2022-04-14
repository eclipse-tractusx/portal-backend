using System;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.Provisioning.ProvisioningEntities
{
    public class UserPasswordReset
    {
        [Key]
        public Guid UserEntityId { get; set; }

        public DateTime PasswordModifiedAt { get; set; }
        
        public int ResetCount { get; set; }
    }
}
