using System;

namespace CatenaX.NetworkServices.UserAdministration.Service.Models
{
    public class UserPasswordReset
    {
        public string? UserEntityId { get; set; }
        public DateTimeOffset? PasswordModifiedAt { get; set; }
        public int? ResetCount { get; set; }
    }
}