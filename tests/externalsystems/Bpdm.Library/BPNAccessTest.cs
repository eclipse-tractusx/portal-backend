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

using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Net;
using System.Text.Json;

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

    [Fact]
    public async Task FetchBusinessPartner_Success()
    {
        var resultSet = _fixture.Create<FetchBusinessPartnerDto>();
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(resultSet))
        });

        var httpClient = _fixture.Create<HttpClient>();
        var sut = _fixture.Create<BpnAccess>();

        var result = await sut.FetchBusinessPartner("testpbn", "token", CancellationToken.None).ToListAsync().ConfigureAwait(false);
        Assert.Equal(resultSet.Bpn, result.First().Bpn);
        Assert.Equal("token", httpClient.DefaultRequestHeaders.Authorization?.Parameter);
    }

    [Fact]
    public async Task FetchBusinessPartner_Failure()
    {
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

        var httpClient = _fixture.Create<HttpClient>();
        var sut = _fixture.Create<BpnAccess>();

        async Task Act() => await sut.FetchBusinessPartner("testpbn", "token", CancellationToken.None).ToListAsync().ConfigureAwait(false);

        await Assert.ThrowsAsync<ServiceException>(Act);
        Assert.Equal("token", httpClient.DefaultRequestHeaders.Authorization?.Parameter);
    }

    #region FetchLegalEntityByBpns

    [Fact]
    public async Task FetchLegalEntityByBpn_Success_ReturnsExpected()
    {
        //Arrange
        HttpRequestMessage? request = null;

        const string json = "{ \"legalName\": \"string\", \"bpnl\": \"string\", \"identifiers\": [ { \"value\": \"string\", \"type\": { \"technicalKey\": \"string\", \"name\": \"string\" }, \"issuingBody\": \"string\" } ], \"legalShortName\": \"string\", \"legalForm\": { \"technicalKey\": \"string\", \"name\": \"string\", \"abbreviation\": \"string\" }, \"states\": [ { \"officialDenotation\": \"string\", \"validFrom\": \"2023-07-24T11:23:20.887Z\", \"validTo\": \"2023-07-24T11:23:20.887Z\", \"type\": { \"technicalKey\": \"ACTIVE\", \"name\": \"string\" } } ], \"classifications\": [ { \"value\": \"string\", \"code\": \"string\", \"type\": { \"technicalKey\": \"NACE\", \"name\": \"string\" } } ], \"relations\": [ { \"type\": { \"technicalKey\": \"CX_LEGAL_SUCCESSOR_OF\", \"name\": \"string\" }, \"startBpn\": \"string\", \"endBpn\": \"string\", \"validFrom\": \"2023-07-24T11:23:20.887Z\", \"validTo\": \"2023-07-24T11:23:20.887Z\" } ], \"currentness\": \"2023-07-24T11:23:20.887Z\", \"createdAt\": \"2023-07-24T11:23:20.887Z\", \"updatedAt\": \"2023-07-24T11:23:20.887Z\", \"legalAddress\": { \"bpna\": \"string\", \"name\": \"string\", \"states\": [ { \"description\": \"string\", \"validFrom\": \"2023-07-24T11:23:20.887Z\", \"validTo\": \"2023-07-24T11:23:20.887Z\", \"type\": { \"technicalKey\": \"ACTIVE\", \"name\": \"string\" } } ], \"identifiers\": [ { \"value\": \"string\", \"type\": { \"technicalKey\": \"string\", \"name\": \"string\" } } ], \"physicalPostalAddress\": { \"geographicCoordinates\": { \"longitude\": 0, \"latitude\": 0, \"altitude\": 0 }, \"country\": { \"technicalKey\": \"UNDEFINED\", \"name\": \"string\" }, \"postalCode\": \"string\", \"city\": \"string\", \"street\": { \"name\": \"string\", \"houseNumber\": \"string\", \"milestone\": \"string\", \"direction\": \"string\" }, \"administrativeAreaLevel1\": { \"name\": \"string\", \"regionCode\": \"string\" }, \"administrativeAreaLevel2\": \"string\", \"administrativeAreaLevel3\": \"string\", \"district\": \"string\", \"companyPostalCode\": \"string\", \"industrialZone\": \"string\", \"building\": \"string\", \"floor\": \"string\", \"door\": \"string\" }, \"alternativePostalAddress\": { \"geographicCoordinates\": { \"longitude\": 0, \"latitude\": 0, \"altitude\": 0 }, \"country\": { \"technicalKey\": \"UNDEFINED\", \"name\": \"string\" }, \"postalCode\": \"string\", \"city\": \"string\", \"administrativeAreaLevel1\": { \"name\": \"string\", \"regionCode\": \"string\" }, \"deliveryServiceNumber\": \"string\", \"type\": \"PO_BOX\", \"deliveryServiceQualifier\": \"string\" }, \"bpnLegalEntity\": \"string\", \"bpnSite\": \"string\", \"createdAt\": \"2023-07-24T11:23:20.887Z\", \"updatedAt\": \"2023-07-24T11:23:20.887Z\", \"isLegalAddress\": true, \"isMainAddress\": true }}";

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
        result.Currentness.Should().BeExactly(DateTimeOffset.Parse("2023-07-24T11:23:20.887Z"));
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
        result.Message.Should().Be($"Access to external system bpdm failed with Status Code {HttpStatusCode.BadRequest}");
    }

    #endregion
}
