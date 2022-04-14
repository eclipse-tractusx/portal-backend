using System;
using System.Collections.Generic;

namespace CatenaX.NetworkServices.UserAdministration.Service.Models
{
    public class UserPasswordReset
    {
        public string UserEntityId { get; set; }
        public DateTime PasswordModifiedAt { get; set; }
        public int ResetCount { get; set; }
    }
}