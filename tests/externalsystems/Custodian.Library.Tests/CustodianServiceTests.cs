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
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.Tests;

public class CustodianServiceTests
{
    #region Initialization

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly ITokenService _tokenService;
    private readonly IOptions<CustodianSettings> _options;
    private readonly IFixture _fixture;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CustodianServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _options = Options.Create(new CustodianSettings
        {
            MembershipErrorMessage = "Credential of type MembershipCredential is already exists",
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
        _dateTimeProvider = A.Fake<IDateTimeProvider>();
    }

    #endregion

    #region Create Wallet

    [Fact]
    public async Task CreateWallet_WithValidData_DoesNotThrowException()
    {
        // Arrange
        const string bpn = "123";
        const string name = "test";
        const string did = "did:sov:GamAMqXnXr1chS4viYXoxB";
        var now = DateTimeOffset.Parse("2023-09-04T07:11:21.2022371+00:00");
        A.CallTo(() => _dateTimeProvider.OffsetNow)
            .Returns(now);
        var data = JsonSerializer.Serialize(new WalletCreationResponse(did), JsonOptions);
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK, data.ToFormContent("application/json"));
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<CustodianService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new CustodianService(_tokenService, _dateTimeProvider, _options);

        // Act
        var result = await sut.CreateWalletAsync(bpn, name, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be($"{{\"did\":\"{did}\",\"createdAt\":\"{now:O}\"}}");
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "{ \"message\": \"Wallet with given identifier already exists!\" }", "call to external system https://base.address.com/api/wallets failed with statuscode 409 - Message: Wallet with given identifier already exists!", true)]
    [InlineData(HttpStatusCode.BadRequest, "{ \"test\": \"123\" }", "call to external system custodian-post failed with statuscode 400", false)]
    [InlineData(HttpStatusCode.BadRequest, "this is no json", "call to external system https://base.address.com/api/wallets failed with statuscode 400", true)]
    [InlineData(HttpStatusCode.Forbidden, null, "call to external system https://base.address.com/api/wallets failed with statuscode 403", true)]
    [InlineData(HttpStatusCode.InternalServerError, "Internal Server Error", "call to external system https://base.address.com/api/wallets failed with statuscode 500 - Message: Internal Server Error", true)]
    [InlineData(HttpStatusCode.InternalServerError, null, "call to external system custodian-post failed with statuscode 500", false)]
    public async Task CreateWallet_WithConflict_ThrowsServiceExceptionWithErrorContent(HttpStatusCode statusCode, string? content, string message, bool isRequiredUri)
    {
        // Arrange
        const string bpn = "123";
        const string name = "test";
        var httpMessageHandlerMock = content == null
            ? new HttpMessageHandlerMock(statusCode, null, null, isRequiredUri)
            : new HttpMessageHandlerMock(statusCode, new StringContent(content), null, isRequiredUri);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<CustodianService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new CustodianService(_tokenService, _dateTimeProvider, _options);

        // Act
        async Task Act() => await sut.CreateWalletAsync(bpn, name, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    [Fact]
    public async Task CreateWallet_withSuccessStatusCode_JsonException()
    {
        // Arrange
        const string bpn = "123";
        const string name = "test";
        var content = "this is no json data";
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK, new StringContent(content));
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<CustodianService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new CustodianService(_tokenService, _dateTimeProvider, _options);

        // Act
        var result = await sut.CreateWalletAsync(bpn, name, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be("Service Response deSerialization failed for custodian-post");
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
        var sut = new CustodianService(_tokenService, _dateTimeProvider, _options);

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
        var sut = new CustodianService(_tokenService, _dateTimeProvider, _options);

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
        var sut = new CustodianService(_tokenService, _dateTimeProvider, _options);

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
        var sut = new CustodianService(_tokenService, _dateTimeProvider, _options);

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
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.Conflict, new StringContent(JsonSerializer.Serialize(new MembershipErrorResponse("Credential of type MembershipCredential is already exists "))));
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<CustodianService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new CustodianService(_tokenService, _dateTimeProvider, _options);

        // Act
        var result = await sut.SetMembership(bpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().Be($"{bpn} already has a membership");
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "{ \"title\": \"Credential of type MembershipCredential is already exists \" }", "call to external system custodian-membership-post failed with statuscode 400")]
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
        var sut = new CustodianService(_tokenService, _dateTimeProvider, _options);

        // Act
        async Task Act() => await sut.SetMembership(bpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    #endregion

    #region TriggerFramework

    [Fact]
    public async Task TriggerFramework_WithValidData_DoesNotThrowException()
    {
        // Arrange
        const string bpn = "123";
        var expiry = DateTimeOffset.UtcNow;
        var data = _fixture.Build<CustodianFrameworkRequest>()
            .With(x => x.HolderIdentifier, bpn)
            .With(x => x.Expiry, expiry)
            .Create();
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<CustodianService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new CustodianService(_tokenService, _dateTimeProvider, _options);

        // Act
        await sut.TriggerFrameworkAsync(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        httpMessageHandlerMock.RequestMessage.Should().Match<HttpRequestMessage>(x =>
            x.Content is JsonContent &&
            (x.Content as JsonContent)!.ObjectType == typeof(CustodianFrameworkRequest) &&
            ((x.Content as JsonContent)!.Value as CustodianFrameworkRequest)!.HolderIdentifier == bpn &&
            ((x.Content as JsonContent)!.Value as CustodianFrameworkRequest)!.Type == data.Type &&
            ((x.Content as JsonContent)!.Value as CustodianFrameworkRequest)!.Template == data.Template &&
            ((x.Content as JsonContent)!.Value as CustodianFrameworkRequest)!.Version == data.Version
        );
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "{ \"message\": \"Framework test!\" }", "call to external system custodian-framework-post failed with statuscode 409")]
    [InlineData(HttpStatusCode.BadRequest, "{ \"test\": \"123\" }", "call to external system custodian-framework-post failed with statuscode 400")]
    [InlineData(HttpStatusCode.BadRequest, "this is no json", "call to external system custodian-framework-post failed with statuscode 400")]
    [InlineData(HttpStatusCode.Forbidden, null, "call to external system custodian-framework-post failed with statuscode 403")]
    public async Task TriggerFramework_WithConflict_ThrowsServiceExceptionWithErrorContent(HttpStatusCode statusCode, string content, string message)
    {
        // Arrange
        const string bpn = "123";
        var expiry = DateTimeOffset.UtcNow;
        var data = _fixture.Create<CustodianFrameworkRequest>();
        var httpMessageHandlerMock = content == null
            ? new HttpMessageHandlerMock(statusCode)
            : new HttpMessageHandlerMock(statusCode, new StringContent(content));
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<CustodianService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new CustodianService(_tokenService, _dateTimeProvider, _options);

        // Act
        async Task Act() => await sut.TriggerFrameworkAsync(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    #endregion

    #region TriggerDismantler

    [Fact]
    public async Task TriggerDismantler_WithValidData_DoesNotThrowException()
    {
        // Arrange
        const string bpn = "123";
        var expiry = DateTimeOffset.UtcNow;
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK, null, null, true);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<CustodianService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new CustodianService(_tokenService, _dateTimeProvider, _options);

        // Act
        await sut.TriggerDismantlerAsync(bpn, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, expiry, CancellationToken.None).ConfigureAwait(false);

        // Assert
        httpMessageHandlerMock.RequestMessage.Should().Match<HttpRequestMessage>(x =>
            x.Content is JsonContent &&
            (x.Content as JsonContent)!.ObjectType == typeof(CustodianDismantlerRequest) &&
            ((x.Content as JsonContent)!.Value as CustodianDismantlerRequest)!.Bpn == bpn &&
            ((x.Content as JsonContent)!.Value as CustodianDismantlerRequest)!.Type == VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE
        );
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "{ \"message\": \"Dismantler test!\" }", "call to external system custodian-dismantler-post failed with statuscode 409")]
    [InlineData(HttpStatusCode.BadRequest, "{ \"test\": \"123\" }", "call to external system custodian-dismantler-post failed with statuscode 400")]
    [InlineData(HttpStatusCode.BadRequest, "this is no json", "call to external system custodian-dismantler-post failed with statuscode 400")]
    [InlineData(HttpStatusCode.Forbidden, null, "call to external system custodian-dismantler-post failed with statuscode 403")]
    public async Task TriggerDismantler_WithConflict_ThrowsServiceExceptionWithErrorContent(HttpStatusCode statusCode, string content, string message)
    {
        // Arrange
        const string bpn = "123";
        var expiry = DateTimeOffset.UtcNow;
        var httpMessageHandlerMock = content == null
            ? new HttpMessageHandlerMock(statusCode)
            : new HttpMessageHandlerMock(statusCode, new StringContent(content));
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<CustodianService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new CustodianService(_tokenService, _dateTimeProvider, _options);

        // Act
        async Task Act() => await sut.TriggerDismantlerAsync(bpn, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, expiry, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    #endregion
}
