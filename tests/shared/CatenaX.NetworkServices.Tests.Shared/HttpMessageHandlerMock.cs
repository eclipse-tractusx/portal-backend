using System.Net;

namespace CatenaX.NetworkServices.Tests.Shared;

public class HttpMessageHandlerMock : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;

    public HttpMessageHandlerMock(HttpStatusCode statusCode)
    {
        _statusCode = statusCode;
    }
    
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage(_statusCode));
    }
}