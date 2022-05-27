using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System.Net;
using System.Text.Json;

namespace CatenaX.NetworkServices.Framework.ErrorHandling;

public class GeneralHttpErrorHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    private static readonly IReadOnlyDictionary<HttpStatusCode,MetaData> _metadata = new Dictionary<HttpStatusCode,MetaData>()
    {
        { HttpStatusCode.BadRequest, new MetaData("https://tools.ietf.org/html/rfc7231#section-6.5.1", "One or more validation errors occurred.") },
        { HttpStatusCode.NotFound, new MetaData("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4", "Cannot find representation of target resource.") },
        { HttpStatusCode.Forbidden, new MetaData("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3", "Access to requested resource is not permitted.") },
        { HttpStatusCode.BadGateway, new MetaData("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.3", "Error accessing external resource.") },
        { HttpStatusCode.ServiceUnavailable, new MetaData("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.4", "Service is currently unavailable.") },
        { HttpStatusCode.InternalServerError, new MetaData("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1", "The server encountered an unexpected condition.") }
    };

    public GeneralHttpErrorHandler(RequestDelegate next, ILogger<GeneralHttpErrorHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception error)
        {
            ErrorResponse errorResponse = null!;
            if (error is ArgumentException)
            {
                errorResponse = CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    error,
                    (error) => ((error as ArgumentException)!.ParamName, Enumerable.Repeat(error.Message, 1)));
                _logger.LogInformation(error.Message);
            }
            else if (error is NotFoundException)
            {
                errorResponse = CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    error,
                    null);
                _logger.LogInformation(error.Message);
            }
            else if (error is ForbiddenException)
            {
                errorResponse = CreateErrorResponse(
                    HttpStatusCode.Forbidden,
                    error,
                    null);
                _logger.LogInformation(error.Message);
            }
            else if (error is ServiceException)
            {
                var statusCode = (error as ServiceException)!.StatusCode;
                errorResponse = CreateErrorResponse(
                    HttpStatusCode.BadGateway,
                    error,
                    (error) => (error.Source, new [] { $"remote service returned status code: {(int)statusCode} {statusCode}", error.Message } ));
                _logger.LogInformation(error.Message);
            }
            else
            {
                errorResponse = CreateErrorResponse(
                    HttpStatusCode.InternalServerError,
                    error,
                    null);
                _logger.LogError(error.Message);
            }
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = errorResponse.Status;
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse)).ConfigureAwait(false);
        }
    }

    private ErrorResponse CreateErrorResponse(HttpStatusCode statusCode, Exception error, Func<Exception,(string?,IEnumerable<string>)>? getSourceAndMessages)
    {
        var meta = _metadata.GetValueOrDefault(statusCode, _metadata[HttpStatusCode.InternalServerError]);
        var (source, messages) = getSourceAndMessages == null
            ? (error.Source, Enumerable.Repeat(error.Message,1))
            : getSourceAndMessages(error);

        var messageMap = new Dictionary<string,IEnumerable<string>>() { { source ?? "unknown", messages } };
        while (error.InnerException != null)
        {
            error = error.InnerException;
            source = error.Source ?? "inner";

            messageMap[source] = messageMap.TryGetValue(source, out messages)
                ? Enumerable.Append(messages, error.Message)
                : Enumerable.Repeat(error.Message,1);
        }

        return new ErrorResponse(
            meta.Url,
            meta.Description,
            (int)statusCode,
            messageMap
        );
    }

    private class MetaData
    {
        public MetaData(string url, string description)
        {
            Url = url;
            Description = description;
        }
        public string Url;
        public string Description;
    }
}
