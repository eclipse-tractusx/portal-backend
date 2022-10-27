using Microsoft.Extensions.Logging;

namespace Org.CatenaX.Ng.Portal.Backend.Framework.Web;

public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger;

    public LoggingHandler(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<LoggingHandler>();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Request: {Request}", request.ToString());
        if (request.Content is { } content)
        {
            _logger.LogDebug("Request Content: {Content}", await content.ReadAsStringAsync(cancellationToken));
        }
        var response = await base.SendAsync(request, cancellationToken);

        _logger.LogInformation("Response: {Response}", response.ToString());
        _logger.LogDebug("Responded with status code: {StatusCode} and data: {Data}", response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));

        return response;
    }
}