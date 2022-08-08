using System.Text.Json;
using CatenaX.NetworkServices.Keycloak.ErrorHandling;
using Keycloak.Net.Models.RealmsAdmin;

namespace CatenaX.NetworkServices.Provisioning.Library;

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

    private async ValueTask UpdateSharedRealmAsync(string alias, string displayName, bool enabled)
    {
        var realm = await _SharedIdp.GetRealmAsync(alias).ConfigureAwait(false);
        realm.DisplayName = displayName;
        realm.Enabled = enabled;
        if (!await _SharedIdp.UpdateRealmAsync(alias, realm).ConfigureAwait(false))
        {
            throw new KeycloakNoSuccessException($"failed to update shared realm {alias}");
        };
    }

    public async ValueTask DeleteSharedRealmAsync(string alias)
    {
        if (! await _SharedIdp.DeleteRealmAsync(alias))
        {
            throw new KeycloakNoSuccessException($"failed to delete shared realm {alias}");
        }
    }

    private Realm CloneRealm(Realm realm) =>
        JsonSerializer.Deserialize<Realm>(JsonSerializer.Serialize(realm))!;
}
