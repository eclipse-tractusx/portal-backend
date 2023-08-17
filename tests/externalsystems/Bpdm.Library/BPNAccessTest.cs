/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Tests;

public class BPNAccessTest
{
    private readonly IFixture _fixture;

    public BPNAccessTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
    }

    private void ConfigureHttpClientFactoryFixture(HttpResponseMessage httpResponseMessage, Action<HttpRequestMessage?>? setMessage = null)
    {
        var messageHandler = A.Fake<HttpMessageHandler>();
        A.CallTo(messageHandler) // mock protected method
            .Where(x => x.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .ReturnsLazily(call =>
            {
                var message = call.Arguments.Get<HttpRequestMessage>(0);
                setMessage?.Invoke(message);
                return Task.FromResult(httpResponseMessage);
            });
        var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("http://localhost") };
        _fixture.Inject(httpClient);

        var httpClientFactory = _fixture.Freeze<Fake<IHttpClientFactory>>();
        A.CallTo(() => httpClientFactory.FakedObject.CreateClient("bpn")).Returns(httpClient);
    }

    #region FetchLegalEntityByBpns

    [Fact]
    public async Task FetchLegalEntityByBpn_Success_ReturnsExpected()
    {
        //Arrange
        HttpRequestMessage? request = null;

        const string json = @"{
            ""legalName"": ""CX-Test-Access"",
            ""bpnl"": ""BPNL00000007OR16"",
            ""identifiers"": [
                {
                    ""value"": ""HRB 12345"",
                    ""type"": {
                        ""technicalKey"": ""EU_VAT_ID_DE"",
                        ""name"": ""Value added tax identification number""
                    },
                    ""issuingBody"": null
                }
            ],
            ""legalShortName"": ""CX-Test-Access"",
            ""legalForm"": null,
            ""states"": [],
            ""classifications"": [],
            ""relations"": [],
            ""currentness"": ""2023-07-26T15:27:12.739650Z"",
            ""createdAt"": ""2023-07-26T15:27:13.756713Z"",
            ""updatedAt"": ""2023-07-26T15:27:13.756719Z"",
            ""legalAddress"": {
                ""bpna"": ""BPNA000000000LKR"",
                ""name"": null,
                ""states"": [],
                ""identifiers"": [],
                ""physicalPostalAddress"": {
                    ""geographicCoordinates"": null,
                    ""country"": {
                        ""technicalKey"": ""DE"",
                        ""name"": ""Germany""
                    },
                    ""postalCode"": ""1"",
                    ""city"": ""Dresden"",
                    ""street"": {
                        ""name"": ""Onisamili Road 236"",
                        ""houseNumber"": """",
                        ""milestone"": null,
                        ""direction"": null
                    },
                    ""administrativeAreaLevel1"": null,
                    ""administrativeAreaLevel2"": null,
                    ""administrativeAreaLevel3"": null,
                    ""district"": null,
                    ""companyPostalCode"": null,
                    ""industrialZone"": null,
                    ""building"": null,
                    ""floor"": null,
                    ""door"": null
                },
                ""alternativePostalAddress"": null,
                ""bpnLegalEntity"": ""BPNL00000007OR16"",
                ""isLegalAddress"": true,
                ""bpnSite"": null,
                ""isMainAddress"": false,
                ""createdAt"": ""2023-07-26T15:27:13.756112Z"",
                ""updatedAt"": ""2023-07-26T15:27:14.267585Z""
            }
        }";

        ConfigureHttpClientFactoryFixture(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        }, requestMessage => request = requestMessage);

        var businessPartnerNumber = _fixture.Create<string>();
        var sut = _fixture.Create<BpnAccess>();

        //Act
        var result = await sut.FetchLegalEntityByBpn(businessPartnerNumber, _fixture.Create<string>(), CancellationToken.None);

        //Assert
        request.Should().NotBeNull();
        request!.RequestUri.Should().NotBeNull();
        request.RequestUri.Should().NotBeNull();
        request.RequestUri!.LocalPath.Should().Be($"/pool/api/catena/legal-entities/{businessPartnerNumber}");
        request.RequestUri.Query.Should().Be("?idType=BPN");

        result.Should().NotBeNull();
        result.LegalEntityAddress.Should().NotBeNull();
        result.LegalEntityAddress!.Bpna.Should().Be("BPNA000000000LKR");
    }

    [Fact]
    public async Task FetchLegalEntityByBpn_InvalidJsonResponse_Throws()
    {
        //Arrange
        var json = _fixture.Create<string>();
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        });

        var sut = _fixture.Create<BpnAccess>();

        var Act = () => sut.FetchLegalEntityByBpn(_fixture.Create<string>(), _fixture.Create<string>(), CancellationToken.None);

        //Act
        var result = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);

        //Assert
        result.Should().NotBeNull();
        result.Message.Should().StartWith("Access to external system bpdm did not return a valid json response");
    }

    [Fact]
    public async Task FetchLegalEntityByBpn_EmptyResponse_Throws()
    {
        //Arrange
        const string json = "";
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        });

        var sut = _fixture.Create<BpnAccess>();

        var Act = () => sut.FetchLegalEntityByBpn(_fixture.Create<string>(), _fixture.Create<string>(), CancellationToken.None);

        //Act
        var result = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);

        //Assert
        result.Should().NotBeNull();
        result.Message.Should().StartWith("Access to external system bpdm did not return a valid json response");
    }

    [Fact]
    public async Task FetchLegalEntityByBpn_NoContentResponse_Throws()
    {
        //Arrange
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
        });

        var sut = _fixture.Create<BpnAccess>();

        var Act = () => sut.FetchLegalEntityByBpn(_fixture.Create<string>(), _fixture.Create<string>(), CancellationToken.None);

        //Act
        var result = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);

        //Assert
        result.Should().NotBeNull();
        result.Message.Should().StartWith("Access to external system bpdm did not return a valid json response");
    }

    [Fact]
    public async Task FetchLegalEntityByBpn_InvalidJsonType_Throws()
    {
        //Arrange
        const string json = "{\"some\": [{\"other\": \"json\"}]}";
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        });

        var sut = _fixture.Create<BpnAccess>();

        var Act = () => sut.FetchLegalEntityByBpn(_fixture.Create<string>(), _fixture.Create<string>(), CancellationToken.None);

        //Act
        var result = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);

        //Assert
        result.Should().NotBeNull();
        result.Message.Should().StartWith("Access to external system bpdm did not return a valid legal entity response");
    }

    [Fact]
    public async Task FetchLegalEntityByBpn_UnsuccessfulCall_Throws()
    {
        //Arrange
        var json = _fixture.Create<string>();
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent(json)
        });

        var sut = _fixture.Create<BpnAccess>();

        var Act = () => sut.FetchLegalEntityByBpn(_fixture.Create<string>(), _fixture.Create<string>(), CancellationToken.None);

        //Act
        var result = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);

        //Assert
        result.Message.Should().Be($"call to external system bpn-fetch-legal-entity failed with statuscode {(int)HttpStatusCode.BadRequest}");
    }

    #endregion
}
