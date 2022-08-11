using Flurl.Http;
using Keycloak.Net.Models.SAMLMetaData;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Keycloak.Net
{
    public partial class KeycloakClient
    {
        public async Task<EntityDescriptorType> GetSAMLMetaDataAsync(string realm) =>
            (EntityDescriptorType)new XmlSerializer(typeof(EntityDescriptorType))
                .Deserialize(await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                    .AppendPathSegment("/realms/")
                    .AppendPathSegment(realm, true)
                    .AppendPathSegment("/protocol/saml/descriptor")
                    .GetStreamAsync()
                    .ConfigureAwait(false));
    }
}
