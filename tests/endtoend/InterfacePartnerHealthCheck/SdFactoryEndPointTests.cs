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

using Castle.Core.Internal;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using RestAssured.Response.Logging;
using Xunit;
using Xunit.Abstractions;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

[Trait("Category", "InterfaceHC")]
[Collection("InterfaceHC")]
public class SdFactoryEndpointTests : EndToEndTestBase
{
    private static readonly string BaseUrl = TestResources.SdFactoryBaseUrl;

    private static readonly string TokenUrl =
        TestResources.BaseCentralIdpUrl + "/auth/realms/CX-Central/protocol/openid-connect/token";

    private const string EndPoint = "/api/rel3/selfdescription";
    private string? InterfaceHealthCheckTechUserToken;

    private static readonly Secrets Secrets = new();

    public SdFactoryEndpointTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void InterfaceHealthCheck_SdDocCreation()
    {
        InterfaceHealthCheckTechUserToken = TechTokenRetriever.GetToken(TokenUrl,
            Secrets.InterfaceHealthCheckTechClientId,
            Secrets.InterfaceHealthCheckTechClientSecret);
        if (InterfaceHealthCheckTechUserToken.IsNullOrEmpty())
            throw new Exception("Could not fetch token for interface partner health check");

        var body = DataHandleHelper.SerializeData(
            new SdFactoryRequestModel(
                "TestAutomation",
                new List<RegistrationNumber> { new("local", "o12345678") },
                "DE",
                "DE",
                SdFactoryRequestModelSdType.LegalParticipant,
                "BPNL000000000000",
                "BPNL000000000000",
                "CAXSDUMMYCATENAZZ"
            )
        );

        Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {InterfaceHealthCheckTechUserToken}")
            .When()
            .Body(body)
            .Post($"{BaseUrl}{EndPoint}")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(202);
    }
}
