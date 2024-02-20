using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Net;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Tests;

public class DimServiceTests
{
    private readonly IDimService _sut;
    private readonly ITokenService _tokenService;
    private readonly IHttpClientFactory _clientFactory;
    private readonly DimSettings _dimSettings;

    public DimServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.ConfigureFixture();

        _tokenService = A.Fake<ITokenService>();
        _clientFactory = A.Fake<IHttpClientFactory>();

        _dimSettings = new DimSettings
        {
            BaseAddress = "https://example.org/",
            GrantType = "client_credentials",
            TokenAddress = "https://example.org/token",
            ClientSecret = "katze123",
            ClientId = "cl1",
            Username = "testuser",
            Password = "katze123",
            Scope = "credentials",
            DidDocumentBaseLocation = "https://example.org/did/",
            UniversalResolverAddress = "https://example.org/resolver/"
        };
        _sut = new DimService(_tokenService, _clientFactory, Options.Create(_dimSettings));
    }

    #region CreateWalletAsync

    [Fact]
    public async Task CreateWalletAsync_CallsExpected_DoesNothing()
    {
        // Arrange
        const string companyName = "testCompany";
        const string bpn = "bpnl0000123";
        var httpMessageHandlerMock = new HttpMessageHandlerMock(
            HttpStatusCode.OK);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<DimService>(_dimSettings, A<CancellationToken>._)).Returns(httpClient);

        // Act
        var result = await _sut.CreateWalletAsync(companyName, bpn, "https://example.org/did/", CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CreateWalletAsync_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        const string companyName = "testCompany";
        const string bpn = "bpnl0000123";
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<DimService>(_dimSettings, A<CancellationToken>._)).Returns(httpClient);

        // Act
        async Task Act() => await _sut.CreateWalletAsync(companyName, bpn, "https://example.org/did/", CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be("call to external system dim-post failed with statuscode 400");
    }

    #endregion

    #region ValidateDid

    [Fact]
    public async Task ValidateDid_WithoutError_ReturnsTrue()
    {
        // Arrange
        const string did = "did:web:123";
        var didValidationResult = new DidValidationResult(new DidResolutionMetadata(null));
        var httpMessageHandlerMock = new HttpMessageHandlerMock(
            HttpStatusCode.OK,
            new StringContent(JsonSerializer.Serialize(didValidationResult)));
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _clientFactory.CreateClient("universalResolver")).Returns(httpClient);

        // Act
        var result = await _sut.ValidateDid(did, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateDid_WithError_ReturnsFalse()
    {
        // Arrange
        const string did = "did:web:123";
        var didValidationResult = new DidValidationResult(new DidResolutionMetadata("This is an error"));
        var httpMessageHandlerMock = new HttpMessageHandlerMock(
            HttpStatusCode.OK,
            new StringContent(JsonSerializer.Serialize(didValidationResult)));
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _clientFactory.CreateClient("universalResolver")).Returns(httpClient);

        // Act
        var result = await _sut.ValidateDid(did, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateDid_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        const string did = "did:web:123";
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _clientFactory.CreateClient("universalResolver")).Returns(httpClient);

        // Act
        async Task Act() => await _sut.ValidateDid(did, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be("call to external system validate-did failed with statuscode 400");
    }

    #endregion
}
