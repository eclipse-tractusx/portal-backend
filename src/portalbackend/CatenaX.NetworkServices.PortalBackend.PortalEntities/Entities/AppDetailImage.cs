using System;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities

{
    public class AppDetailImage
    {
        private AppDetailImage()
        {
            ImageUrl = null!;
        }

        public AppDetailImage(Guid appId, string imageUrl)
        {
            AppId = appId;
            ImageUrl = imageUrl;
        }

        [Key]
        public Guid Id { get; private set; }

        public Guid AppId { get; set; }

        public string ImageUrl { get; set; }

        // Navigation properties
        public virtual App? App { get; set; }
    }
}
