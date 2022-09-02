using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class AppDetailImage
{
    public AppDetailImage(Guid id, Guid appId, string imageUrl)
    {
        Id = id;
        AppId = appId;
        ImageUrl = imageUrl;
    }

    public Guid Id { get; private set; }

    public Guid AppId { get; set; }

    [MaxLength(255)]
    public string ImageUrl { get; set; }

    // Navigation properties
    public virtual App? App { get; set; }
}
