/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.Custodian.Models;
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
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _options = Options.Create(new CustodianSettings
        {
            Password = "passWord",
            Scope = "test",
            Username = "user@name",
            BaseAdress = "https://base.address.com",
            ClientId = "CatenaX",
            ClientSecret = "pass@Secret",
            GrantType = "cred",
            KeyCloakTokenAdress = "https://key.cloak.com"
        });
        _tokenService = A.Fake<ITokenService>();
    }

    #endregion

    #region Create Wallet

    [Fact]
    public async Task CreateWallet_WithValidData_DoesNotThrowException()
    {
        // Arrange
        var bpn = "123";
        var name = "test";
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

    [Fact]
    public async Task CreateWallet_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        var bpn = "123";
        var name = "test";
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
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
        ex.Message.Should().Contain("Access to Custodian Failed with Status");
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
}
