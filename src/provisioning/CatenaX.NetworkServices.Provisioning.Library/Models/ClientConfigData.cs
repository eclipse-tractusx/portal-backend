using CatenaX.NetworkServices.Provisioning.Library.Enums;

namespace CatenaX.NetworkServices.Provisioning.Library.Models;

public class ClientConfigData
{
    public ClientConfigData(string name, string description, IamClientAuthMethod iamClientAuthMethod)
    {
        Name = name;
        Description = description;
        IamClientAuthMethod = iamClientAuthMethod;
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public IamClientAuthMethod IamClientAuthMethod { get; set; }
}
