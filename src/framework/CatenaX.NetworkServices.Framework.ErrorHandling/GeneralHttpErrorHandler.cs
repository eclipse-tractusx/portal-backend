using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Framework.ErrorHandling
{
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
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                string _message;
                if (error is ArgumentException)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    _message = "Bad Request";
                }
                else if (error is NotFoundException)
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    _message = "Resource Not Found";
                }
                else if (error is ForbiddenException)
                {
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    _message = "Forbidden";
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    _message = "Internal Server Error";
                }
                response.ContentType = "application/json";
                _logger.LogError(error.ToString());
                var result = JsonSerializer.Serialize(new { message = _message });
                await response.WriteAsync(result);
            }
        }
    }
}
