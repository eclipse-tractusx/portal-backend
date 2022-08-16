namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Entity for the core components
/// </summary>
public class CoreComponent
{
    /// <summary>
    /// Only needed for ef
    /// </summary>
    private CoreComponent()
    {
        Name = null!;
    }

    /// <summary>
    /// 
    /// </summary>
    public CoreComponent(Guid id, string name, Guid iamClientId)
    {
        Id = id;
        Name = name;
        IamClientId = iamClientId;
    }
    
    public Guid Id { get; set; }
    
    public string Name { get; set; }

    public Guid IamClientId { get; set; }

    public virtual IamClient? IamClient { get; set; }
}