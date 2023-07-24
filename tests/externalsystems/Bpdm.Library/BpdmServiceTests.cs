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
            KeycloakTokenAddress = "https://key.cloak.com",
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
        var externalId = _fixture.Create<string>();
        var data = _fixture.Build<BpdmLegalEntityOutputData>()
            .With(x => x.ExternalId, externalId)
            .With(x => x.Bpn, "TESTBPN")
            .Create();
        // var pageOutputData = new PageOutputResponseBpdmLegalEntityData(
        //     Enumerable.Repeat(data, 1));
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
        };
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(
            HttpStatusCode.OK,
            data.ToJsonContent(
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
        result.ExternalId.Should().Be(data.ExternalId);
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
}
