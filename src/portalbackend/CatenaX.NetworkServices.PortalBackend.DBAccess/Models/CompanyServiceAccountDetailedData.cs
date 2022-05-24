namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyServiceAccountDetailedData
{
    public CompanyServiceAccountDetailedData(Guid serviceAccountId, string clientId, string clientClientId, string userEntityId, string name, string description)
    {
        ServiceAccountId = serviceAccountId;
        ClientId = clientId;
        ClientClientId = clientClientId;
        UserEntityId = userEntityId;
        Name = name;
        Description = description;
    }

    public Guid ServiceAccountId { get; set; }

    public string ClientId { get; set; }

    public string ClientClientId { get; set; }

    public string UserEntityId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }
}
