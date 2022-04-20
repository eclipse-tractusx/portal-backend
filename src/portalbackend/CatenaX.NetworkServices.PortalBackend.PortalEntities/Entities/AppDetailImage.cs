using System;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities

{
    public class AppDetailImage
    {
        public AppDetailImage() {}

        public AppDetailImage(string imageUrl)
        {
            ImageUrl = imageUrl;
        }

        [Key]
        public Guid Id { get; set; }

        public Guid? AppId { get; set; }

        public string ImageUrl { get; set; }

        public virtual App? App { get; set; }
    }
}
