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
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using RestAssured.Response.Logging;
using Xunit;
using Xunit.Abstractions;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

[Trait("Category", "InterfaceHC")]
[Collection("InterfaceHC")]
public class ClearinghouseEndToEndTests : EndToEndTestBase
{
    private static readonly string BaseUrl = TestResources.ClearingHouseUrl;
    private static readonly string BaseTokenUrl = TestResources.ClearingHouseTokenUrl;
    private const string EndPoint = "/api/v1/validation";
    private string? ClearingHouseUserToken;

    private static readonly Secrets Secrets = new();

    public ClearinghouseEndToEndTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task ClearinghouseInterface_HealthCheck()
    {
        ClearingHouseUserToken =
            TechTokenRetriever.GetToken(BaseTokenUrl,
                Secrets.ClearingHouseClientId,
                Secrets.ClearingHouseClientSecret);
        if (ClearingHouseUserToken.IsNullOrEmpty())
            throw new Exception("Could not fetch token for clearing house health check.");

        var body = DataHandleHelper.SerializeData(
            new ClearinghouseTransferData(
                new ParticipantDetails(
                    "SmokeTest CH", "Stuttgart", "Test Street", "BPNL000SMOKE0011", "Bavaria", "01108",
                    "Germany", "DE"
                ),
                new IdentityDetails(
                    "did:sov:RPgthNMDkVdzYQhXzahh3P", // hardcode due to initial requirements in CPLP-2803
                    new List<UniqueIdData> { new("local", "HB8272819") }
                ),
                $"{TestResources.BasePortalBackendUrl}/api/administration/registration/clearinghouse",
                false)
        );

        var data = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {ClearingHouseUserToken}")
            .When()
            .Body(body)
            .Post($"{BaseUrl}{EndPoint}")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();
        var result = await data.Content.ReadAsStringAsync().ConfigureAwait(false);
        result.Should().NotBeNullOrEmpty("Response should not be null or empty");
    }
}
