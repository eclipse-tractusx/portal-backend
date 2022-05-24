using CatenaX.NetworkServices.Provisioning.Library.Enums;

namespace CatenaX.NetworkServices.Provisioning.Library.Models;

public class ClientAuthData
{
    public ClientAuthData(IamClientAuthMethod iamClientAuthMethod)
    {
        IamClientAuthMethod = iamClientAuthMethod;
    }

    public IamClientAuthMethod IamClientAuthMethod { get; set; }
    public string? Secret { get; set; }
}
