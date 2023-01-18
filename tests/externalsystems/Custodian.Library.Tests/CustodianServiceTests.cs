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

using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.Tests;

public class CustodianServiceTests
{
    #region Initialization

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenService _tokenService;
    private readonly string _accessToken;
    private readonly IOptions<CustodianSettings> _options;
    private readonly IFixture _fixture;

    public CustodianServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _accessToken = _fixture.Create<string>();
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
        _httpClientFactory = A.Fake<IHttpClientFactory>();
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
        SetupTokenService();
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _httpClientFactory.CreateClient(A<string>.That.Matches(x => x == "custodian")))
            .Returns(httpClient);
        var sut = new CustodianService(_httpClientFactory, _tokenService, _options);
        
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
        SetupTokenService();
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _httpClientFactory.CreateClient("custodian")).Returns(httpClient);
        var sut = new CustodianService(_httpClientFactory, _tokenService, _options);

        // Act
        async Task Act() => await sut.CreateWalletAsync(bpn, name, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Contain("Access to Custodian Failed with Status");
    }

    #endregion

    #region GetWallets

    [Fact]
    public async Task GetWallets_WithValidData_ReturnsWallets()
    {
        // Arrange
        var data = JsonSerializer.Serialize(new List<GetWallets>
        {
            new()
            {
                bpn = "abc",
                name = "test",
                wallet = new Wallet
                {
                    did = "123",
                    createdAt = DateTime.Now,
                    publicKey = "key"
                }
            }
        });
        SetupTokenService();
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK, data.ToFormContent("application/vc+ld+json"));
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _httpClientFactory.CreateClient(A<string>.That.Matches(x => x == "custodian")))
            .Returns(httpClient);
        var sut = new CustodianService(_httpClientFactory, _tokenService, _options);
        
        // Act
        var result = await sut.GetWalletsAsync(CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetWallets_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        SetupTokenService();
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _httpClientFactory.CreateClient("custodian")).Returns(httpClient);
        var sut = new CustodianService(_httpClientFactory, _tokenService, _options);

        // Act
        async Task Act() => await sut.GetWalletsAsync(CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Setup
    
    private void SetupTokenService()
    {
        A.CallTo(() => _tokenService.GetTokenAsync(A<GetTokenSettings>._, A<CancellationToken>._)).Returns(_accessToken);
    }

    #endregion
}
