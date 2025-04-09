/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Tests;

public class ClearinghouseServiceTests
{
    #region Initialization

    private readonly ITokenService _tokenService;
    private readonly IOptions<ClearinghouseSettings> _options;
    private readonly ClearinghouseService _sut;
    private readonly IFixture _fixture;

    public ClearinghouseServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _options = Options.Create(new ClearinghouseSettings
        {
            CallbackUrl = "https://callback.address.com",
            RetriggerEndClearinghouseIntervalInDays = 30,
            DefaultClearinghouseCredentials = new ClearinghouseCredentialsSettings
            {
                Password = "defaultPassword",
                Scope = "defaultScope",
                Username = "defaultUserName",
                BaseAddress = "https://defaultBase.address.com",
                ValidationPath = "/api/default/validation",
                ClientId = "defaultClientId",
                ClientSecret = "defaultClientSecret",
                GrantType = "DefaultCred",
                TokenAddress = "https://defaultKey.cloak.com",
                CountryAlpha2Code = "Default"
            },
            RegionalClearinghouseCredentials = [
                new ClearinghouseCredentialsSettings {
                    Password = "regionalPassword",
                    Scope = "regionalScope",
                    Username = "regionalUserName",
                    BaseAddress = "https://regionalBase.address.com",
                    ValidationPath = "/api/regional/validation",
                    ClientId = "regionalClientId",
                    ClientSecret = "regionalClientSecret",
                    GrantType = "regionalCred",
                    TokenAddress = "https://regionalKey.cloak.com",
                    CountryAlpha2Code = "CN"
                }
            ]
        });
        _tokenService = A.Fake<ITokenService>();
        _sut = new ClearinghouseService(_tokenService, _options);
    }

    #endregion

    #region TriggerCompanyDataPost

    [Fact]
    public async Task TriggerCompanyDataPost_WithValidData_DoesNotThrowException()
    {
        // Arrange
        var data = _fixture.Create<ClearinghouseTransferData>();
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK);
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://defaultBase.address.com")
        };
        var credentials = _options.Value.GetCredentials(data.LegalEntity.Address.CountryAlpha2Code);
        A.CallTo(() => _tokenService.GetAuthorizedClient($"{nameof(ClearinghouseService)}{credentials.CountryAlpha2Code}", credentials, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        await _sut.TriggerCompanyDataPost(data, CancellationToken.None);

        // Assert
        true.Should().BeTrue(); // One Assert is needed - just checking for no exception
    }

    [Theory]
    [InlineData("DE")]
    [InlineData("NL")]
    [InlineData("FR")]
    [InlineData("CN")]
    public async Task TriggerCompanyDataPost_WithCountryAlpha2Code_SuccessResult(string countryAlpha2Code)
    {
        // Arrange
        var legalAddress = _fixture.Build<LegalAddress>()
            .With(x => x.CountryAlpha2Code, countryAlpha2Code)
            .Create();
        var legalEntity = _fixture.Build<LegalEntity>()
            .With(x => x.Address, legalAddress)
            .Create();
        var data = _fixture.Build<ClearinghouseTransferData>()
            .With(x => x.LegalEntity, legalEntity)
            .Create();
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK);
        var baseAddress = countryAlpha2Code == "CN" ? "https://regionalBase.address.com" : "https://defaultBase.address.com";
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri(baseAddress)
        };
        var credentials = _options.Value.GetCredentials(data.LegalEntity.Address.CountryAlpha2Code);
        A.CallTo(() => _tokenService.GetAuthorizedClient($"{nameof(ClearinghouseService)}{credentials.CountryAlpha2Code}", credentials, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        await _sut.TriggerCompanyDataPost(data, CancellationToken.None);

        // Assert
        credentials.BaseAddress.Should().Be(baseAddress);
        true.Should().BeTrue();
    }

    [Fact]
    public async Task TriggerCompanyDataPost_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        var data = _fixture.Create<ClearinghouseTransferData>();
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://defaultBase.address.com")
        };
        var credentials = _options.Value.GetCredentials(data.LegalEntity.Address.CountryAlpha2Code);
        A.CallTo(() => _tokenService.GetAuthorizedClient($"{nameof(ClearinghouseService)}{credentials.CountryAlpha2Code}", credentials, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        async Task Act() => await _sut.TriggerCompanyDataPost(data, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be("call to external system clearinghouse-post failed with statuscode 400");
        ex.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TriggerCompanyDataPost_WitException_ThrowsServiceException()
    {
        // Arrange
        var data = _fixture.Create<ClearinghouseTransferData>();
        var error = new Exception("random exception");
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest, null, error);
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://defaultBase.address.com")
        };
        var credentials = _options.Value.GetCredentials(data.LegalEntity.Address.CountryAlpha2Code);
        A.CallTo(() => _tokenService.GetAuthorizedClient($"{nameof(ClearinghouseService)}{credentials.CountryAlpha2Code}", credentials, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        async Task Act() => await _sut.TriggerCompanyDataPost(data, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be("call to external system clearinghouse-post failed");
        ex.InnerException.Should().Be(error);
    }

    [Fact]
    public async Task TriggerCompanyDataPost_WitErrorContent_LogsContent()
    {
        // Arrange
        var data = _fixture.Create<ClearinghouseTransferData>();
        using var stringContent = new StringContent("{ \"message\": \"Framework test!\" }");
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest, stringContent);
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://defaultBase.address.com")
        };
        var credentials = _options.Value.GetCredentials(data.LegalEntity.Address.CountryAlpha2Code);
        A.CallTo(() => _tokenService.GetAuthorizedClient($"{nameof(ClearinghouseService)}{credentials.CountryAlpha2Code}", credentials, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        async Task Act() => await _sut.TriggerCompanyDataPost(data, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be("call to external system clearinghouse-post failed with statuscode 400 - Message: { \"message\": \"Framework test!\" }");
    }

    #endregion
}
