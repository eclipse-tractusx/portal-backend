using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System.Net;
using System.Text.Json;

namespace CatenaX.NetworkServices.Framework.ErrorHandling;

public class GeneralHttpErrorHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

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
                errorResponse = new ErrorResponse(
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    "One or more validation errors occurred.",
                    (int)HttpStatusCode.BadRequest,
                    new Dictionary<String,IEnumerable<string>>()
                    {
                        { (error as ArgumentException)!.ParamName ?? error.Source ?? "unknown", Enumerable.Repeat(error.Message, 1) }
                    }
                );
                _logger.LogInformation(error.Message);
            }
            else if (error is NotFoundException)
            {
                errorResponse = new ErrorResponse(
                    "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
                    "Cannot find representation of target resource.",
                    (int)HttpStatusCode.NotFound,
                    new Dictionary<String,IEnumerable<string>>()
                    {
                        { error.Source ?? "unknown", Enumerable.Repeat(error.Message, 1) }
                    }
                );
                _logger.LogInformation(error.Message);
            }
            else if (error is ForbiddenException)
            {
                errorResponse = new ErrorResponse(
                    "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
                    "Access to requested resource is not permitted.",
                    (int)HttpStatusCode.Forbidden,
                    new Dictionary<String,IEnumerable<string>>()
                    {
                        { error.Source ?? "unknown", Enumerable.Repeat(error.Message, 1) }
                    }
                );
                _logger.LogInformation(error.Message);
            }
            else
            {
                errorResponse = new ErrorResponse(
                    "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                    "The server encountered an unexpected condition.",
                    (int)HttpStatusCode.InternalServerError,
                    new Dictionary<String,IEnumerable<string>>()
                    {
                        { error.Source ?? "unknown", Enumerable.Repeat(error.Message, 1) }
                    }
                );
                _logger.LogError(error.Message);
            }
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = errorResponse.Status;
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse)).ConfigureAwait(false);
        }
    }
}
