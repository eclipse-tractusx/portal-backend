using CatenaX.NetworkServices.Provisioning.Library.Enums;

namespace CatenaX.NetworkServices.Provisioning.Library.Models;

public class ClientConfigRolesData
{
    public ClientConfigRolesData(string name, string description, IamClientAuthMethod iamClientAuthMethod, IDictionary<string, IEnumerable<string>> clientRoles)
    {
        Name = name;
        Description = description;
        IamClientAuthMethod = iamClientAuthMethod;
        ClientRoles = clientRoles;
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public IamClientAuthMethod IamClientAuthMethod { get; set; }
    public IDictionary<string,IEnumerable<string>> ClientRoles { get; set; }
}
