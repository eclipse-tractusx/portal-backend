using System;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class IamUser
    {
        private IamUser()
        {
            UserEntityId = null!;
        }

        public IamUser(string iamUserId, Guid companyUserId)
        {
            UserEntityId = iamUserId;
            CompanyUserId = companyUserId;
        }

        [Key]
        [StringLength(36)]
        public string UserEntityId { get; private set; }

        public Guid CompanyUserId { get; private set; }

        public virtual CompanyUser? CompanyUser { get; private set; }
    }
}
