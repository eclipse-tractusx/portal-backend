using System;
using System.ComponentModel.DataAnnotations;
namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class IamUser
    {
        public IamUser() {}
        public IamUser(string iamUserId, Guid companyUserId)
        {
            IamUserId = iamUserId;
            CompanyUserId = companyUserId;
        }

        [Key]
        [StringLength(36)]
        public string IamUserId { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateLastChanged { get; set; }

        public Guid CompanyUserId { get; set; }

        public virtual CompanyUser? CompanyUser { get; set; }
    }
}
