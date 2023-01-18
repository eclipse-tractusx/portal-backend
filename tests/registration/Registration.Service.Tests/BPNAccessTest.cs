/********************************************************************************
 * Copyright (c) 2021,2022 Microsoft and BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Bpn;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Bpn.Model;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests;

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

    #region FetchLegalEntities

    [Fact]
    public async Task FetchLegalEntities_Success_ReturnsExpected()
    {
        //Arrange
        HttpRequestMessage? request = null;

        var json = "{\"content\": [{\"legalEntity\": {\"bankAccounts\": [{\"currency\": {\"name\": \"string\", \"technicalKey\": \"UNDEFINED\"}, \"internationalBankAccountIdentifier\": \"string\", \"internationalBankIdentifier\": \"string\", \"nationalBankAccountIdentifier\": \"string\", \"nationalBankIdentifier\": \"string\", \"trustScores\": [0]}], \"bpn\": \"string\", \"currentness\": \"2023-01-17T20:51:04.502Z\", \"identifiers\": [{\"issuingBody\": {\"name\": \"string\", \"technicalKey\": \"string\", \"url\": \"string\"}, \"status\": {\"name\": \"string\", \"technicalKey\": \"string\"}, \"type\": {\"name\": \"string\", \"technicalKey\": \"string\", \"url\": \"string\"}, \"value\": \"string\"}], \"legalForm\": {\"categories\": [{\"name\": \"string\", \"url\": \"string\"}], \"language\": {\"name\": \"string\", \"technicalKey\": \"undefined\"}, \"mainAbbreviation\": \"string\", \"name\": \"string\", \"technicalKey\": \"string\", \"url\": \"string\"}, \"names\": [{\"language\": {\"name\": \"string\", \"technicalKey\": \"undefined\"}, \"shortName\": \"string\", \"type\": {\"name\": \"string\", \"technicalKey\": \"ACRONYM\", \"url\": \"string\"}, \"value\": \"string\"}], \"profileClassifications\": [{\"code\": \"string\", \"type\": {\"name\": \"string\", \"url\": \"string\"}, \"value\": \"string\"}], \"relations\": [{\"endedAt\": \"2023-01-17T20:51:04.502Z\", \"endNode\": \"string\", \"relationClass\": {\"name\": \"string\", \"technicalKey\": \"CDQ_HIERARCHY\"}, \"startedAt\": \"2023-01-17T20:51:04.502Z\", \"startNode\": \"string\", \"type\": {\"name\": \"string\", \"technicalKey\": \"CX_LEGAL_SUCCESSOR_OF\"}}], \"roles\": [{\"name\": \"string\", \"technicalKey\": \"string\"}], \"status\": {\"officialDenotation\": \"string\", \"type\": {\"name\": \"string\", \"technicalKey\": \"ACTIVE\", \"url\": \"string\"}, \"validFrom\": \"2023-01-17T20:51:04.502Z\", \"validUntil\": \"2023-01-17T20:51:04.502Z\"}, \"types\": [{\"name\": \"string\", \"technicalKey\": \"BRAND\", \"url\": \"string\"}]}, \"score\": 5}], \"contentSize\": 1, \"page\": 0, \"totalElements\": 1, \"totalPages\": 1}";

        ConfigureHttpClientFactoryFixture(new HttpResponseMessage 
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        }, requestMessage => request = requestMessage);

        var parameters = _fixture.Build<FetchLegalEntitiesQueryParameters>()
            .With(x => x.Name, "Name")
            .With(x => x.LegalForm, "LegalForm")
            .With(x => x.Status, "Status")
            .With(x => x.Classification, "Classification")
            .With(x => x.AdministrativeArea, "AdministrativeArea")
            .With(x => x.PostCode, "PostCode")
            .With(x => x.Locality, "Locality")
            .With(x => x.Thoroughfare, "Thoroughfare")
            .With(x => x.Premise, "Premise")
            .With(x => x.PostalDeliveryPoint, "PostalDeliveryPoint")
            .With(x => x.SiteName, "SiteName")
            .With(x => x.Page, 1)
            .With(x => x.Size, 2)
            .Create();

        var httpClient = _fixture.Create<HttpClient>();
        var sut = _fixture.Create<BpnAccess>();
        
        //Act
        var result = await sut.FetchLegalEntities(parameters, _fixture.Create<string>(), CancellationToken.None);

        //Assert
        request.Should().NotBeNull();
        request!.RequestUri.Should().NotBeNull();
        request.RequestUri.Should().NotBeNull();
        request.RequestUri!.LocalPath.Should().Be("/api/catena/legal-entities");
        request.RequestUri.Query.Should().Be("?name=Name&legalForm=LegalForm&status=Status&classification=Classification&administrativeArea=AdministrativeArea&postCode=PostCode&locality=Locality&thoroughfare=Thoroughfare&premise=Premise&postalDeliveryPoint=PostalDeliveryPoint&siteName=SiteName&page=1&size=2");

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        var entity = result!.First();
        entity.Currentness.Should().BeExactly(DateTimeOffset.Parse("2023-01-17T20:51:04.502Z"));
        entity.BankAccounts.Should().NotBeNull();
        entity.BankAccounts.Should().HaveCount(1);
        var account = entity.BankAccounts.First();
        account.Currency.Should().NotBeNull();
        account.Currency.TechnicalKey.Should().Be("UNDEFINED");
    }

    [Fact]
    public async Task FetchLegalEntities_InvalidJsonResponse_Throws()
    {
        //Arrange
        var json = _fixture.Create<string>();
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage 
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        });

        var httpClient = _fixture.Create<HttpClient>();
        var sut = _fixture.Create<BpnAccess>();

        var Act = () => sut.FetchLegalEntities(_fixture.Create<FetchLegalEntitiesQueryParameters>(), _fixture.Create<string>(), CancellationToken.None);

        //Act
        var result = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);

        //Assert
        result.Should().NotBeNull();
        result.Message.Should().StartWith("Access to external system bpdm did not return a valid json response");
    }

    [Fact]
    public async Task FetchLegalEntities_InvalidJsonType_Throws()
    {
        //Arrange
        var json = "{\"some\": [{\"other\": \"json\"}]}";
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage 
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        });

        var httpClient = _fixture.Create<HttpClient>();
        var sut = _fixture.Create<BpnAccess>();

        var Act = () => sut.FetchLegalEntities(_fixture.Create<FetchLegalEntitiesQueryParameters>(), _fixture.Create<string>(), CancellationToken.None);

        //Act
        var result = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);

        //Assert
        result.Should().NotBeNull();
        result.Message.Should().StartWith("Access to external system bpdm did not return a valid legal entity response");
    }

    [Fact]
    public async Task FetchLegalEntities_UnsuccessfulCall_Throws()
    {
        //Arrange
        var json = _fixture.Create<string>();
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage 
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent(json)
        });

        var httpClient = _fixture.Create<HttpClient>();
        var sut = _fixture.Create<BpnAccess>();

        var Act = () => sut.FetchLegalEntities(_fixture.Create<FetchLegalEntitiesQueryParameters>(), _fixture.Create<string>(), CancellationToken.None);

        //Act
        var result = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);

        //Assert
        result.Message.Should().Be($"Access to external system bpdm failed with Status Code {HttpStatusCode.BadRequest}");
    }

    #endregion

    #region FetchLegalEntityByBpns

    [Fact]
    public async Task FetchLegalEntityByBpn_Success_ReturnsExpected()
    {
        //Arrange
        HttpRequestMessage? request = null;

        var json = "{\"bankAccounts\": [{\"currency\": {\"name\": \"string\", \"technicalKey\": \"UNDEFINED\"}, \"internationalBankAccountIdentifier\": \"string\", \"internationalBankIdentifier\": \"string\", \"nationalBankAccountIdentifier\": \"string\", \"nationalBankIdentifier\": \"string\", \"trustScores\": [0]}], \"bpn\": \"string\", \"currentness\": \"2023-01-18T12:52:41.710Z\", \"identifiers\": [{\"issuingBody\": {\"name\": \"string\", \"technicalKey\": \"string\", \"url\": \"string\"}, \"status\": {\"name\": \"string\", \"technicalKey\": \"string\"}, \"type\": {\"name\": \"string\", \"technicalKey\": \"string\", \"url\": \"string\"}, \"value\": \"string\"}], \"legalForm\": {\"categories\": [{\"name\": \"string\", \"url\": \"string\"}], \"language\": {\"name\": \"string\", \"technicalKey\": \"undefined\"}, \"mainAbbreviation\": \"string\", \"name\": \"string\", \"technicalKey\": \"string\", \"url\": \"string\"}, \"names\": [{\"language\": {\"name\": \"string\", \"technicalKey\": \"undefined\"}, \"shortName\": \"string\", \"type\": {\"name\": \"string\", \"technicalKey\": \"ACRONYM\", \"url\": \"string\"}, \"value\": \"string\"}], \"profileClassifications\": [{\"code\": \"string\", \"type\": {\"name\": \"string\", \"url\": \"string\"}, \"value\": \"string\"}], \"relations\": [{\"endedAt\": \"2023-01-18T12:52:41.710Z\", \"endNode\": \"string\", \"relationClass\": {\"name\": \"string\", \"technicalKey\": \"CDQ_HIERARCHY\"}, \"startedAt\": \"2023-01-18T12:52:41.710Z\", \"startNode\": \"string\", \"type\": {\"name\": \"string\", \"technicalKey\": \"CX_LEGAL_SUCCESSOR_OF\"}}], \"roles\": [{\"name\": \"string\", \"technicalKey\": \"string\"}], \"status\": {\"officialDenotation\": \"string\", \"type\": {\"name\": \"string\", \"technicalKey\": \"ACTIVE\", \"url\": \"string\"}, \"validFrom\": \"2023-01-18T12:52:41.709Z\", \"validUntil\": \"2023-01-18T12:52:41.709Z\"}, \"types\": [{\"name\": \"string\", \"technicalKey\": \"BRAND\", \"url\": \"string\"}]}";

        ConfigureHttpClientFactoryFixture(new HttpResponseMessage 
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        }, requestMessage => request = requestMessage);

        var businessPartnerNumber = _fixture.Create<string>();
        var httpClient = _fixture.Create<HttpClient>();
        var sut = _fixture.Create<BpnAccess>();
        
        //Act
        var result = await sut.FetchLegalEntityByBpn(businessPartnerNumber, _fixture.Create<string>(), CancellationToken.None);

        //Assert
        request.Should().NotBeNull();
        request!.RequestUri.Should().NotBeNull();
        request.RequestUri.Should().NotBeNull();
        request.RequestUri!.LocalPath.Should().Be($"/api/catena/legal-entities/{businessPartnerNumber}");
        request.RequestUri.Query.Should().Be("?idType=BPN");

        result.Should().NotBeNull();
        result.Currentness.Should().BeExactly(DateTimeOffset.Parse("2023-01-18T12:52:41.710Z"));
        result.BankAccounts.Should().NotBeNull();
        result.BankAccounts.Should().HaveCount(1);
        var account = result.BankAccounts.First();
        account.Currency.Should().NotBeNull();
        account.Currency.TechnicalKey.Should().Be("UNDEFINED");
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

        var httpClient = _fixture.Create<HttpClient>();
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
        var json = "{\"some\": [{\"other\": \"json\"}]}";
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage 
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        });

        var httpClient = _fixture.Create<HttpClient>();
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

        var httpClient = _fixture.Create<HttpClient>();
        var sut = _fixture.Create<BpnAccess>();

        var Act = () => sut.FetchLegalEntityByBpn(_fixture.Create<string>(), _fixture.Create<string>(), CancellationToken.None);

        //Act
        var result = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);

        //Assert
        result.Message.Should().Be($"Access to external system bpdm failed with Status Code {HttpStatusCode.BadRequest}");
    }

    #endregion        

    #region FetchLegalEntityAdressByBpn

    [Fact]
    public async Task FetchLegalEntityAddressByBpn_Success_ReturnsExpected()
    {
        //Arrange
        HttpRequestMessage? request = null;

        var json = "[{\"legalAddress\": {\"administrativeAreas\": [], \"careOf\": null, \"contexts\": [], \"country\": {\"name\": \"Germany\", \"technicalKey\": \"DE\"}, \"geographicCoordinates\": null, \"localities\": [{\"language\": {\"name\": \"English\", \"technicalKey\": \"en\"}, \"shortName\": null, \"type\": {\"name\": \"Other\", \"technicalKey\": \"OTHER\", \"url\": \"\"}, \"value\": \"Bremen\"}], \"postalDeliveryPoints\": [], \"postCodes\": [{\"type\": {\"name\": \"Other type\", \"technicalKey\": \"OTHER\", \"url\": \"\"}, \"value\": \"28777\"}], \"premises\": [], \"thoroughfares\": [{\"direction\": null, \"language\": {\"name\": \"English\", \"technicalKey\": \"en\"}, \"name\": null, \"number\": \"30\", \"shortName\": null, \"type\": {\"name\": \"Other type\", \"technicalKey\": \"OTHER\", \"url\": \"\"}, \"value\": \"Heidlerchenstr.\"}], \"types\": [], \"version\": {\"characterSet\": {\"name\": \"Latin\", \"technicalKey\": \"LATIN\"}, \"language\": {\"name\": \"English\", \"technicalKey\": \"en\"}}}, \"legalEntity\": \"BPNL000000055EPN\"}]";

        ConfigureHttpClientFactoryFixture(new HttpResponseMessage 
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        }, requestMessage => request = requestMessage);

        var businessPartnerNumber = "BPNL000000055EPN";
        var httpClient = _fixture.Create<HttpClient>();
        var sut = _fixture.Create<BpnAccess>();
        
        //Act
        var result = await sut.FetchLegalEntityAddressByBpn(businessPartnerNumber, _fixture.Create<string>(), CancellationToken.None).ToListAsync().ConfigureAwait(false);

        //Assert
        request.Should().NotBeNull();
        request!.RequestUri.Should().NotBeNull();
        request.RequestUri.Should().NotBeNull();
        request.RequestUri!.LocalPath.Should().Be($"/api/catena/legal-entities/legal-addresses/search");
        request.RequestUri.Query.Should().Be("");

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        var legalEntityAddress = result.First();
        legalEntityAddress.Should().NotBeNull();
        legalEntityAddress.LegalEntity.Should().Be(businessPartnerNumber);
        var legalAddress = legalEntityAddress.LegalAddress;
        legalAddress.Should().NotBeNull();
        legalAddress.Country.Should().NotBeNull();
        legalAddress.Country.TechnicalKey.Should().Be("DE");
        legalAddress.PostCodes.Should().NotBeNull();
        legalAddress.PostCodes.Should().HaveCount(1);
        legalAddress.PostCodes.First().Value.Should().Be("28777");
        legalAddress.Localities.Should().NotBeNull();
        legalAddress.Localities.Should().HaveCount(1);
        legalAddress.Localities.First().Value.Should().Be("Bremen");
        legalAddress.Thoroughfares.Should().NotBeNull();
        legalAddress.Thoroughfares.Should().HaveCount(1);
        var thoroughfare = legalAddress.Thoroughfares.First();
        thoroughfare.Value.Should().Be("Heidlerchenstr.");
        thoroughfare.Number.Should().Be("30");
    }

    [Fact]
    public async Task FetchLegalEntityAddressByBpn_InvalidJsonResponse_Throws()
    {
        //Arrange
        var json = _fixture.Create<string>();
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage 
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        });

        var httpClient = _fixture.Create<HttpClient>();
        var sut = _fixture.Create<BpnAccess>();

        var Act = async () => await sut.FetchLegalEntityAddressByBpn(_fixture.Create<string>(), _fixture.Create<string>(), CancellationToken.None).ToListAsync().ConfigureAwait(false);

        //Act
        var result = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);

        //Assert
        result.Should().NotBeNull();
        result.Message.Should().StartWith("Access to external system bpdm did not return a valid json response");
    }

    [Fact]
    public async Task FetchLegalEntityAddressByBpn_InvalidJsonType_Throws()
    {
        //Arrange
        var json = "[{\"some\": [{\"other\": \"json\"}]}]";
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage 
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        });

        var httpClient = _fixture.Create<HttpClient>();
        var sut = _fixture.Create<BpnAccess>();

        var Act = async () => await sut.FetchLegalEntityAddressByBpn(_fixture.Create<string>(), _fixture.Create<string>(), CancellationToken.None).ToListAsync().ConfigureAwait(false);

        //Act
        var result = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);

        //Assert
        result.Should().NotBeNull();
        result.Message.Should().Be("Access to external system bpdm did not return a valid legal entity address response");
    }

    [Fact]
    public async Task FetchLegalEntityAddressByBpn_UnsuccessfulCall_Throws()
    {
        //Arrange
        var json = _fixture.Create<string>();
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage 
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent(json)
        });

        var httpClient = _fixture.Create<HttpClient>();
        var sut = _fixture.Create<BpnAccess>();

        var Act = async () => await sut.FetchLegalEntityAddressByBpn(_fixture.Create<string>(), _fixture.Create<string>(), CancellationToken.None).ToListAsync().ConfigureAwait(false);

        //Act
        var result = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);

        //Assert
        result.Message.Should().Be($"Access to external system bpdm failed with Status Code {HttpStatusCode.BadRequest}");
    }

    #endregion
}
