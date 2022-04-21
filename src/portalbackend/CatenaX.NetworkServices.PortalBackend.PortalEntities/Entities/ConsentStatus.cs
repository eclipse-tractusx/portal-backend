using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class ConsentStatus
    {
        private ConsentStatus()
        {
            Label = null!;
            Consents = new HashSet<Consent>();
        }

        public ConsentStatus(ConsentStatusId consentStatusId) : this()
        {
            ConsentStatusId = consentStatusId;
            Label = consentStatusId.ToString();
        }

        [Key]
        public ConsentStatusId ConsentStatusId { get; private set; }

        [MaxLength(255)]
        public string Label { get; private set; }

        public virtual ICollection<Consent> Consents { get; private set; }
    }
}
