using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities

{
    public class DocumentType
    {
        public DocumentType()
        {
            Documents = new HashSet<Document>();
        }
        
        [Key]
        public DocumentTypeId DocumentTypeId { get; set; }

        public string Label { get; set; }

        public virtual ICollection<Document> Documents { get; set; }
   }
}
