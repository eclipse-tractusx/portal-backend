using System.Text.Json;
using Keycloak.Net.Models.RealmsAdmin;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningManager
    {
        private async Task CreateSharedRealmAsync(string realm, string name)
        {
            var newRealm = CloneRealm(_Settings.SharedRealm);
            newRealm.Id = realm;
            newRealm._Realm = realm;
            newRealm.DisplayName = name;
            if (!await _SharedIdp.ImportRealmAsync(realm, newRealm).ConfigureAwait(false))
            {
                throw new Exception($"failed to create shared realm {realm} for {name}");
            }
        }

        private Realm CloneRealm(Realm realm) =>
            JsonSerializer.Deserialize<Realm>(JsonSerializer.Serialize(realm))!;
    }
}
