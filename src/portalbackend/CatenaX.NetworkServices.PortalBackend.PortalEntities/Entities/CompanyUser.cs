using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyUser
    {
        private CompanyUser()
        {
            Consents = new HashSet<Consent>();
            Documents = new HashSet<Document>();
            Invitations = new HashSet<Invitation>();
            Apps = new HashSet<App>();
            CompanyUserRoles = new HashSet<CompanyUserRole>();
        }
     
        public CompanyUser(Guid id, Guid companyId, DateTimeOffset dateCreated) : this()
        {
            Id = id;
            DateCreated = dateCreated;
            CompanyId = companyId;
        }

        [Key]
        public Guid Id { get; private set; }

        public DateTimeOffset DateCreated { get; private set; }

        public DateTimeOffset? DateLastChanged { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Firstname { get; set; }

        public byte[]? Lastlogin { get; set; }

        [MaxLength(255)]
        public string? Lastname { get; set; }

        public Guid CompanyId { get; private set; }

        // Navigation properties
        public virtual Company? Company { get; private set; }
        public virtual IamUser? IamUser { get; set; }
        public virtual ICollection<Consent> Consents { get; private set; }
        public virtual ICollection<Document> Documents { get; private set; }
        public virtual ICollection<Invitation> Invitations { get; private set; }
        public virtual ICollection<App> Apps { get; private set; }
        public virtual ICollection<CompanyUserRole> CompanyUserRoles { get; private set; }
    }
}
