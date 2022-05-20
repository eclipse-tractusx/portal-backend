namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class ServiceAccountDetailedData
{
    public ServiceAccountDetailedData(Guid serviceAccountId, string clientId, string name, string description)
    {
        ServiceAccountId = serviceAccountId;
        ClientId = clientId;
        Name = name;
        Description = description;
    }

    public Guid ServiceAccountId { get; set; }

    public string ClientId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }
}
