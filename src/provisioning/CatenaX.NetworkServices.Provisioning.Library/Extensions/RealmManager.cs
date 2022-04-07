using System.Text.Json;
using System.Threading.Tasks;
using Keycloak.Net.Models.RealmsAdmin;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningManager
    {
        private Task<bool> CreateSharedRealmAsync(string realm, string name)
        {
            var newRealm = CloneRealm(_Settings.SharedRealm);
            newRealm.Id = realm;
            newRealm._Realm = realm;
            newRealm.DisplayName = name;
            return _SharedIdp.ImportRealmAsync(realm, newRealm);
        }

        private Realm CloneRealm(Realm realm) =>
            JsonSerializer.Deserialize<Realm>(JsonSerializer.Serialize(realm));
    }
}
