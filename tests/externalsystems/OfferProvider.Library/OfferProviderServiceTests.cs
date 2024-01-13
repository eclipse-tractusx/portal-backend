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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.Models;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.Tests;

public class OfferProviderServiceTests
{
    #region Initialization

    private readonly IFixture _fixture;
    private readonly ITokenService _tokenService;
    private readonly IOptions<OfferProviderSettings> _options;

    public OfferProviderServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _options = Options.Create(new OfferProviderSettings
        {
            Password = "passWord",
            Scope = "test",
            Username = "user@name",
            ClientId = "CatenaX",
            ClientSecret = "pass@Secret",
            GrantType = "cred",
            TokenAddress = "https://key.cloak.com",
        });
        _tokenService = A.Fake<ITokenService>();
    }

    #endregion

    #region TriggerOfferProvider

    [Fact]
    public async Task TriggerOfferProvider_WithValidCall_ReturnsExpected()
    {
        // Arrange
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK);
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _tokenService.GetAuthorizedClient<OfferProviderService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        const string url = "https://trigger.com";
        var data = _fixture.Create<OfferThirdPartyAutoSetupData>();
        var service = new OfferProviderService(_tokenService, _options);

        // Act
        var result = await service.TriggerOfferProvider(data, url, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public async Task TriggerOfferProvider_WithUnsuccessfulStatusCode_ThrowsException()
    {
        // Arrange
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _tokenService.GetAuthorizedClient<OfferProviderService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var data = _fixture.Create<OfferThirdPartyAutoSetupData>();
        var service = new OfferProviderService(_tokenService, _options);

        // Act
        async Task Act() => await service.TriggerOfferProvider(data, "https://callback.com", CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);
    }

    [Fact]
    public async Task TriggerOfferProvider_WithException_ThrowsException()
    {
        // Arrange
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.BadRequest, ex: new HttpRequestException("DNS Error"));
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _tokenService.GetAuthorizedClient<OfferProviderService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var data = _fixture.Create<OfferThirdPartyAutoSetupData>();
        var service = new OfferProviderService(_tokenService, _options);

        // Act
        async Task Act() => await service.TriggerOfferProvider(data, "https://callback.com", CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);
    }

    #endregion

    #region TriggerOfferProviderCallback

    [Fact]
    public async Task TriggerOfferProviderCallback_WithValidCall_ReturnsExpected()
    {
        // Arrange
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK);
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _tokenService.GetAuthorizedClient<OfferProviderService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        const string url = "https://trigger.com";
        var data = _fixture.Create<OfferProviderCallbackData>();
        var service = new OfferProviderService(_tokenService, _options);

        // Act
        var result = await service.TriggerOfferProviderCallback(data, url, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public async Task TriggerOfferProviderCallback_WithUnsuccessfulStatusCode_ThrowsException()
    {
        // Arrange
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _tokenService.GetAuthorizedClient<OfferProviderService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var data = _fixture.Create<OfferProviderCallbackData>();
        var service = new OfferProviderService(_tokenService, _options);

        // Act
        async Task Act() => await service.TriggerOfferProviderCallback(data, "https://callback.com", CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);
    }

    [Fact]
    public async Task TriggerOfferProviderCallback_WithException_ThrowsException()
    {
        // Arrange
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.BadRequest, ex: new HttpRequestException("DNS Error"));
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _tokenService.GetAuthorizedClient<OfferProviderService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var data = _fixture.Create<OfferProviderCallbackData>();
        var service = new OfferProviderService(_tokenService, _options);

        // Act
        async Task Act() => await service.TriggerOfferProviderCallback(data, "https://callback.com", CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);
    }

    #endregion
}
