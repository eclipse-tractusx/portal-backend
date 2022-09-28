/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 ********************************************************************************/

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.ClientAttributeCertificate;
using Flurl.Http;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

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
