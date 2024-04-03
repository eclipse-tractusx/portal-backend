/********************************************************************************
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

using RestAssured.Response.Logging;
using Xunit;
using Xunit.Abstractions;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

[Trait("Category", "InterfaceHC")]
[Collection("InterfaceHC")]
public class WalletEndpointTests : EndToEndTestBase
{
    private static readonly string WalletBaseUrl = TestResources.WalletBaseUrl;

    private static readonly string TokenUrl =
        TestResources.BaseCentralIdpUrl + "/auth/realms/CX-Central/protocol/openid-connect/token";

    private const string WalletEndPoint = "/api/wallets";
    private readonly string InterfaceHealthCheckTechUserToken;
    private string? Bpn;

    private static readonly Secrets Secrets = new();

    public WalletEndpointTests(ITestOutputHelper output) : base(output)
    {
        InterfaceHealthCheckTechUserToken = TechTokenRetriever.GetToken(TokenUrl, Secrets.InterfaceHealthCheckTechClientId, Secrets.InterfaceHealthCheckTechClientSecret);
        if (string.IsNullOrEmpty(InterfaceHealthCheckTechUserToken))
            throw new Exception("Could not fetch token for interface partner health check");
    }

    [Fact]
    public async Task WalletCreationInterface_CreateAndDuplicationCheck()
    {
        Bpn = $"BPNLTEST{DateTime.Now:DDhhmmss}";
        GetListOfWallets();
        await Task.Delay(3000).ConfigureAwait(false);
        CreateWallet(201);
        await Task.Delay(3000).ConfigureAwait(false);
        CreateWallet(409);
    }

    //GET: /api/wallets
    private void GetListOfWallets()
    {
        Given()
             .DisableSslCertificateValidation()
             .Header(
                 "authorization",
                 $"Bearer {InterfaceHealthCheckTechUserToken}")
             .When()
             .Get($"{WalletBaseUrl}{WalletEndPoint}")
             .Then()
             .Log(ResponseLogLevel.OnError)
             .StatusCode(200);
    }

    //POST: /api/wallets
    private void CreateWallet(int statusCode)
    {
        Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {InterfaceHealthCheckTechUserToken}")
            .When()
            .Body($"{{\"bpn\": \"{Bpn}\", \"name\": \"bpn\"}}")
            .Post($"{WalletBaseUrl}{WalletEndPoint}")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(statusCode);
    }
}
