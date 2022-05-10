using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class AppTag
{
    private AppTag()
    {
        Name = null!;
    }

    public AppTag(Guid appId, string name): this()
    {
        AppId = appId;
        Name = name;
    }

    public Guid AppId { get; set; }

    [MaxLength(255)]
    [Column("tag_name")]
    public string Name { get; set; }

    // Navigation properties
    public virtual App? App { get; set; }
}
