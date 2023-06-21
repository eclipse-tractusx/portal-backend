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
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Net;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.Tests;

public class CustodianServiceTests
{
    #region Initialization

    private readonly ITokenService _tokenService;
    private readonly IOptions<CustodianSettings> _options;

    public CustodianServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _options = Options.Create(new CustodianSettings
        {
            Password = "passWord",
            Scope = "test",
            Username = "user@name",
            BaseAddress = "https://base.address.com",
            ClientId = "CatenaX",
            ClientSecret = "pass@Secret",
            GrantType = "cred",
            KeycloakTokenAddress = "https://key.cloak.com"
        });
        _tokenService = A.Fake<ITokenService>();
    }

    #endregion

    #region Create Wallet

    [Fact]
    public async Task CreateWallet_WithValidData_DoesNotThrowException()
    {
        // Arrange
        const string bpn = "123";
        const string name = "test";
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<CustodianService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new CustodianService(_tokenService, _options);

        // Act
        await sut.CreateWalletAsync(bpn, name, CancellationToken.None).ConfigureAwait(false);

        // Assert
        true.Should().BeTrue(); // One Assert is needed - just checking for no exception
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "{ \"message\": \"Wallet with given identifier already exists!\" }", "call to external system custodian-post failed with statuscode 409 - Message: Wallet with given identifier already exists!")]
    [InlineData(HttpStatusCode.BadRequest, "{ \"test\": \"123\" }", "call to external system custodian-post failed with statuscode 400")]
    [InlineData(HttpStatusCode.BadRequest, "this is no json", "call to external system custodian-post failed with statuscode 400")]
    [InlineData(HttpStatusCode.Forbidden, null, "call to external system custodian-post failed with statuscode 403")]
    public async Task CreateWallet_WithConflict_ThrowsServiceExceptionWithErrorContent(HttpStatusCode statusCode, string? content, string message)
    {
        // Arrange
        const string bpn = "123";
        const string name = "test";
        var httpMessageHandlerMock = content == null
            ? new HttpMessageHandlerMock(statusCode)
            : new HttpMessageHandlerMock(statusCode, new StringContent(content));
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<CustodianService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new CustodianService(_tokenService, _options);

        // Act
        async Task Act() => await sut.CreateWalletAsync(bpn, name, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    #endregion

    #region GetWallet By Bpn

    [Fact]
    public async Task GetWalletByBpnAsync_WithValidData_ReturnsWallets()
    {
        // Arrange
        const string validBpn = "BPNL00000003CRHK";
        var data = JsonSerializer.Serialize(new WalletData("abc",
                validBpn,
                "123",
                DateTime.UtcNow,
                false,
                null));
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK, data.ToFormContent("application/vc+ld+json"));
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<CustodianService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new CustodianService(_tokenService, _options);

        // Act
        var result = await sut.GetWalletByBpnAsync(validBpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Bpn.Should().NotBeNullOrEmpty();
        result.Bpn.Should().Be(validBpn);
        result.Did.Should().Be("123");
    }

    [Fact]
    public async Task GetWalletByBpnAsync_WithWalletDataNull_ThrowsServiceException()
    {
        // Arrange
        const string validBpn = "BPNL00000003CRHK";
        var data = JsonSerializer.Serialize(new WalletData("abc",
            validBpn,
            "123",
            DateTime.UtcNow,
            false,
            null));
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<CustodianService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new CustodianService(_tokenService, _options);

        // Act
        async Task Act() => await sut.GetWalletByBpnAsync(validBpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be("Couldn't resolve wallet data");
    }

    [Fact]
    public async Task GetWalletByBpnAsync_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<CustodianService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new CustodianService(_tokenService, _options);

        // Act
        async Task Act() => await sut.GetWalletByBpnAsync("invalidBpn", CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region SetMembership

    [Fact]
    public async Task SetMembership_WithValidData_DoesNotThrowException()
    {
        // Arrange
        const string bpn = "123";
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<CustodianService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new CustodianService(_tokenService, _options);

        // Act
        var result = await sut.SetMembership(bpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().Be("Membership Credential successfully created");
    }

    [Fact]
    public async Task SetMembership_WithConflict_DoesNotThrowException()
    {
        // Arrange
        const string bpn = "123";
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.Conflict);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<CustodianService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new CustodianService(_tokenService, _options);

        // Act
        var result = await sut.SetMembership(bpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().Be($"{bpn} already has a membership");
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "{ \"test\": \"123\" }", "call to external system custodian-membership-post failed with statuscode 400")]
    [InlineData(HttpStatusCode.BadRequest, "this is no json", "call to external system custodian-membership-post failed with statuscode 400")]
    [InlineData(HttpStatusCode.Forbidden, null, "call to external system custodian-membership-post failed with statuscode 403")]
    public async Task SetMembership_WithConflict_ThrowsServiceExceptionWithErrorContent(HttpStatusCode statusCode, string? content, string message)
    {
        // Arrange
        const string bpn = "123";
        var httpMessageHandlerMock = content == null
            ? new HttpMessageHandlerMock(statusCode)
            : new HttpMessageHandlerMock(statusCode, new StringContent(content));
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<CustodianService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new CustodianService(_tokenService, _options);

        // Act
        async Task Act() => await sut.SetMembership(bpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    #endregion
}
