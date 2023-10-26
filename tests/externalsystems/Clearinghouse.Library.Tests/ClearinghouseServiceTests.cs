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
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
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
            Password = "passWord",
            Scope = "test",
            Username = "user@name",
            BaseAddress = "https://base.address.com",
            ClientId = "CatenaX",
            ClientSecret = "pass@Secret",
            GrantType = "cred",
            TokenAddress = "https://key.cloak.com"
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
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<ClearinghouseService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        await _sut.TriggerCompanyDataPost(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        true.Should().BeTrue(); // One Assert is needed - just checking for no exception
    }

    [Fact]
    public async Task TriggerCompanyDataPost_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        var data = _fixture.Create<ClearinghouseTransferData>();
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<ClearinghouseService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);

        // Act
        async Task Act() => await _sut.TriggerCompanyDataPost(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Contain("call to external system clearinghouse-post failed with statuscode");
        ex.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TriggerCompanyDataPost_WitException_ThrowsServiceException()
    {
        // Arrange
        var data = _fixture.Create<ClearinghouseTransferData>();
        var error = new Exception("random exception");
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest, null, error);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<ClearinghouseService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);

        // Act
        async Task Act() => await _sut.TriggerCompanyDataPost(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Contain("call to external system clearinghouse-post failed");
        ex.InnerException.Should().Be(error);
    }

    #endregion
}
