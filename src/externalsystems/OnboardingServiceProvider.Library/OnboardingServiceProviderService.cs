/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library;

public class OnboardingServiceProviderService : IOnboardingServiceProviderService
{
    private readonly ITokenService _tokenService;
    private readonly OnboardingServiceProviderSettings _settings;

    /// <summary>
    /// Creates a new instance of <see cref="OnboardingServiceProviderService"/>
    /// </summary>
    /// <param name="tokenService"></param>
    /// <param name="options"></param>
    public OnboardingServiceProviderService(ITokenService tokenService, IOptions<OnboardingServiceProviderSettings> options)
    {
        _tokenService = tokenService;
        _settings = options.Value;
    }

    public async Task<bool> TriggerProviderCallback(OspDetails ospDetails, OnboardingServiceProviderCallbackData callbackData, CancellationToken cancellationToken)
    {
        var toEncryptArray = Convert.FromBase64String(ospDetails.ClientSecret);
        var md5 = MD5.Create();
        var securityKeyArray = md5.ComputeHash(Encoding.UTF8.GetBytes(_settings.EncryptionKey));
        md5.Clear();

        var des = TripleDES.Create();
        des.Key = securityKeyArray;
        des.Mode = CipherMode.ECB;
        des.Padding = PaddingMode.PKCS7;

        var decryptor = des.CreateDecryptor();
        var resultArray = decryptor.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        des.Clear();
        var secret = Encoding.UTF8.GetString(resultArray);

        var settings = new KeyVaultAuthSettings
        {
            KeycloakTokenAddress = ospDetails.AuthUrl,
            ClientId = ospDetails.ClientId,
            ClientSecret = secret
        };
        var httpClient = await _tokenService.GetAuthorizedClient<OnboardingServiceProviderService>(settings, cancellationToken)
            .ConfigureAwait(false);
        await httpClient.PostAsJsonAsync(ospDetails.CallbackUrl, callbackData, cancellationToken)
            .CatchingIntoServiceExceptionFor("trigger-onboarding-provider")
            .ConfigureAwait(false);

        return true;
    }
}
