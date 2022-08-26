using System.ComponentModel.DataAnnotations;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class AppType
{
    private AppType()
    {
        Label = null!;
        Apps = new HashSet<App>();
    }

    public AppType(AppTypeId appTypeId) : this()
    {
        Id = appTypeId;
        Label = appTypeId.ToString();
    }

    public AppTypeId Id { get; private set; }

    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<App> Apps { get; private set; }
}