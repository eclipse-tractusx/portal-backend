using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Net;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.BpnDidResolver.Library.Tests;

public class BpnDidResolveServiceTests
{
    private const string BPN = "BPNL0000000000XX";
    private readonly IBpnDidResolverService _sut;
    private readonly IHttpClientFactory _clientFactory;

    public BpnDidResolveServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.ConfigureFixture();

        _clientFactory = A.Fake<IHttpClientFactory>();

        _sut = new BpnDidResolverService(_clientFactory);
    }

    #region ValidateDid

    [Fact]
    public async Task ValidateDid_WithoutError_ReturnsTrue()
    {
        // Arrange
        const string did = "did:web:123";
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _clientFactory.CreateClient(nameof(BpnDidResolverService))).Returns(httpClient);

        // Act
        var result = await _sut.TransmitDidAndBpn(did, BPN, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateDid_WithError_ReturnsFalse()
    {
        // Arrange
        const string did = "did:web:123";
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _clientFactory.CreateClient(nameof(BpnDidResolverService))).Returns(httpClient);

        // Act
        var result = await _sut.TransmitDidAndBpn(did, BPN, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
