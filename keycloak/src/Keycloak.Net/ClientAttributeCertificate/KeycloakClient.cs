using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Keycloak.Net.Models.ClientAttributeCertificate;

namespace Keycloak.Net
{
    public partial class KeycloakClient
    {
        public async Task<Certificate> GetKeyInfoAsync(string realm, string clientId, string attribute) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/certificates/")
            .AppendPathSegment(attribute, true)
            .GetJsonAsync<Certificate>()
            .ConfigureAwait(false);

        public async Task<byte[]> GetKeyStoreForClientAsync(string realm, string clientId, string attribute, KeyStoreConfig keyStoreConfig) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/certificates/")
            .AppendPathSegment(attribute, true)
            .AppendPathSegment("/download")
            .PostJsonAsync(keyStoreConfig)
            .ReceiveBytes()
            .ConfigureAwait(false);

        public async Task<Certificate> GenerateCertificateWithNewKeyPairAsync(string realm, string clientId, string attribute) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/certificates/")
            .AppendPathSegment(attribute, true)
            .AppendPathSegment("/generate")
            .PostAsync(new StringContent(""))
            .ReceiveJson<Certificate>()
            .ConfigureAwait(false);

        public async Task<byte[]> GenerateCertificateWithNewKeyPairAndGetKeyStoreAsync(string realm, string clientId, string attribute, KeyStoreConfig keyStoreConfig) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/certificates/")
            .AppendPathSegment(attribute, true)
            .AppendPathSegment("/generate-and-download")
            .PostJsonAsync(keyStoreConfig)
            .ReceiveBytes()
            .ConfigureAwait(false);

        public async Task<Certificate> UploadCertificateWithPrivateKeyAsync(string realm, string clientId, string attribute, string fileName) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/certificates/")
            .AppendPathSegment(attribute, true)
            .AppendPathSegment("/upload")
            .PostMultipartAsync(content => content.AddFile(Path.GetFileName(fileName), Path.GetDirectoryName(fileName)))
            .ReceiveJson<Certificate>()
            .ConfigureAwait(false);

        public async Task<Certificate> UploadCertificateWithoutPrivateKeyAsync(string realm, string clientId, string attribute, string fileName) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/certificates/")
            .AppendPathSegment(attribute, true)
            .AppendPathSegment("/upload-certificate")
            .PostMultipartAsync(content => content.AddFile(Path.GetFileName(fileName), Path.GetDirectoryName(fileName)))
            .ReceiveJson<Certificate>()
            .ConfigureAwait(false);
    }
}
