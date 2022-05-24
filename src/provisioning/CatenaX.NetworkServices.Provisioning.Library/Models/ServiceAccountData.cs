namespace CatenaX.NetworkServices.Provisioning.Library.Models;

public class ServiceAccountData
{
    public ServiceAccountData(string internalClientId, string userEntityId, ClientAuthData authData)
    {
        InternalClientId = internalClientId;
        UserEntityId = userEntityId;
        AuthData = authData;
    }
    
    public string InternalClientId { get; set; }
    public string UserEntityId { get; set; }
    public ClientAuthData AuthData { get; set; }
}
