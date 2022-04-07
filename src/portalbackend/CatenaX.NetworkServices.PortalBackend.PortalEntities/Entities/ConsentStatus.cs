using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class ConsentStatus
    {
        public ConsentStatus()
        {
            Consents = new HashSet<Consent>();
        }

        [Key]
        public int ConsentStatusId { get; set; }

        [MaxLength(255)]
        public string? Label { get; set; }

        public virtual ICollection<Consent> Consents { get; set; }
    }
}
