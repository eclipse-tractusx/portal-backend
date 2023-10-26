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
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Net;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Tests;

public class BpdmServiceTests
{
    #region Initialization

    private readonly ITokenService _tokenService;
    private readonly IOptions<BpdmServiceSettings> _options;
    private readonly IFixture _fixture;

    public BpdmServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _options = Options.Create(new BpdmServiceSettings
        {
            Password = "passWord",
            Scope = "test",
            Username = "user@name",
            BaseAddress = "https://base.address.com",
            ClientId = "CatenaX",
            ClientSecret = "pass@Secret",
            GrantType = "cred",
            TokenAddress = "https://key.cloak.com",
        });
        _tokenService = A.Fake<ITokenService>();
    }

    #endregion

    #region Trigger PutInputLegalEntity

    [Fact]
    public async Task PutInputLegalEntity_WithValidData_DoesNotThrowException()
    {
        // Arrange
        var data = _fixture.Create<BpdmTransferData>();
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        var result = await sut.PutInputLegalEntity(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task PutInputLegalEntity_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        var data = _fixture.Create<BpdmTransferData>();
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        async Task Act() => await sut.PutInputLegalEntity(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Contain("call to external system bpdm-put-legal-entities failed with statuscode");
    }

    #endregion

    #region FetchInputLegalEntity

    [Fact]
    public async Task FetchInputLegalEntity_WithValidResult_ReturnsExpected()
    {
        // Arrange
        var externalId = _fixture.Create<string>();
        var data = _fixture.Build<BpdmLegalEntityOutputData>()
            .With(x => x.ExternalId, externalId)
            .With(x => x.Bpn, "TESTBPN")
            .CreateMany(1);
        var pagination = new PageOutputResponseBpdmLegalEntityData(data);

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
        };

        var httpMessageHandlerMock = new HttpMessageHandlerMock(
            HttpStatusCode.OK,
            pagination.ToJsonContent(
                options,
                "application/json")
            );
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        var result = await sut.FetchInputLegalEntity(externalId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.ExternalId.Should().Be(externalId);
        result.Bpn.Should().Be("TESTBPN");
    }

    [Fact]
    public async Task FetchInputLegalEntity_WithEmtpyObjectResult_ThrowsServiceException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(
            HttpStatusCode.OK,
            new StringContent("{}"));

        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com"),
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        async Task Act() => await sut.FetchInputLegalEntity(_fixture.Create<string>(), CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be("Access to external system bpdm did not return a valid legal entity response");
        ex.IsRecoverable.Should().BeTrue();
    }

    [Fact]
    public async Task FetchInputLegalEntity_WithEmtpyResult_ThrowsServiceException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(
            HttpStatusCode.OK,
            new StringContent(""));

        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com"),
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        async Task Act() => await sut.FetchInputLegalEntity(_fixture.Create<string>(), CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);
        ex.Message.Should().StartWith("Access to external system bpdm did not return a valid json response");
        ex.IsRecoverable.Should().BeFalse();
    }

    [Fact]
    public async Task FetchInputLegalEntity_WithNotFoundResult_ReturnsNull()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.NotFound);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        var Act = () => sut.FetchInputLegalEntity(_fixture.Create<string>(), CancellationToken.None);

        // Assert
        var result = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FetchInputLegalEntity_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        async Task Act() => await sut.FetchInputLegalEntity(_fixture.Create<string>(), CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be("call to external system bpdm-search-legal-entities failed with statuscode 400");
    }

    #endregion

    #region GetSharingState

    [Fact]
    public async Task GetSharingState_WithValidResult_ReturnsExpected()
    {
        // Arrange
        var applicationId = new Guid("aa2eeac5-a0e9-46b0-80f0-d48dde49aa23");
        const string json = @"{
            ""totalElements"": 1,
            ""totalPages"": 1,
            ""page"": 0,
            ""contentSize"": 1,
            ""content"": [
                {
                    ""businessPartnerType"": ""LEGAL_ENTITY"",
                    ""externalId"": ""aa2eeac5-a0e9-46b0-80f0-d48dde49aa23"",
                    ""sharingStateType"": ""Error"",
                    ""sharingErrorCode"": ""SharingProcessError"",
                    ""sharingErrorMessage"": ""Address Identifier Type 'Cheese Region' does not exist (LegalAddressRegionNotFound)"",
                    ""bpn"": null,
                    ""sharingProcessStarted"": ""2023-08-04T14:35:30.478594""
                }
            ]
        }";

        var httpMessageHandlerMock = new HttpMessageHandlerMock(
            HttpStatusCode.OK,
            new StringContent(json));
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        var result = await sut.GetSharingState(applicationId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.ExternalId.Should().Be(applicationId);
        result.Bpn.Should().BeNull();
        result.SharingErrorCode.Should().Be("SharingProcessError");
        result.SharingStateType.Should().Be(BpdmSharingStateType.Error);
        result.BusinessPartnerType.Should().Be(BpdmSharingStateBusinessPartnerType.LEGAL_ENTITY);
        result.SharingErrorMessage.Should().Be("Address Identifier Type 'Cheese Region' does not exist (LegalAddressRegionNotFound)");
    }

    [Fact]
    public async Task GetSharingState_WithEmtpyObjectResult_ThrowsServiceException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var httpMessageHandlerMock = new HttpMessageHandlerMock(
            HttpStatusCode.OK,
            new StringContent("{}"));

        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com"),
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        async Task Act() => await sut.GetSharingState(applicationId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be("Access to sharing state did not return a valid legal entity response");
        ex.IsRecoverable.Should().BeTrue();
    }

    [Fact]
    public async Task GetSharingState_WithEmtpyResult_ThrowsServiceException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var httpMessageHandlerMock = new HttpMessageHandlerMock(
            HttpStatusCode.OK,
            new StringContent(""));

        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com"),
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        async Task Act() => await sut.GetSharingState(applicationId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);
        ex.Message.Should().StartWith("Access to sharing state did not return a valid json response");
        ex.IsRecoverable.Should().BeFalse();
    }

    [Fact]
    public async Task GetSharingState_WithNotFoundResult_ReturnsNull()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.NotFound);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        var Act = () => sut.GetSharingState(applicationId, CancellationToken.None);

        // Assert
        var result = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSharingState_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        async Task Act() => await sut.GetSharingState(applicationId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be("call to external system bpdm-sharing-state failed with statuscode 400");
    }

    #endregion
}
