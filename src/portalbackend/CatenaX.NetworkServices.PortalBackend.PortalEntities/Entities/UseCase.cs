using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class UseCase
    {
        private UseCase()
        {
            Name = null!;
            Shortname = null!;
            Agreements = new HashSet<Agreement>();
            Companies = new HashSet<Company>();
            Apps = new HashSet<App>();
        }
        
        public UseCase(Guid id, string name, string shortname) : this()
        {
            Id = id;
            Name = name;
            Shortname = name;
        }

        [Key]
        public Guid Id { get; private set; }

        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Shortname { get; set; }

        // Navigation properties
        public virtual ICollection<Agreement> Agreements { get; private set; }
        public virtual ICollection<Company> Companies { get; private set; }
        public virtual ICollection<App> Apps { get; private set; }
    }
}
