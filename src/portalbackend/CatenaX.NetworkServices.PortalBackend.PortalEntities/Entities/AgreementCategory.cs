using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class AgreementCategory
    {
        public AgreementCategory()
        {
            Agreements = new HashSet<Agreement>();
        }

        [Key]
        public int AgreementCategoryId { get; set; }

        [MaxLength(255)]
        public string? Label { get; set; }

        public virtual ICollection<Agreement> Agreements { get; set; }
    }
}
