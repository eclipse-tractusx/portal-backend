using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Registration.Service.Helper
{
    public class GeneralErrorHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public GeneralErrorHandler(RequestDelegate next, ILogger<GeneralErrorHandler> logger)
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
                response.ContentType = "application/json";
                _logger.LogError(error.ToString());
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var result = JsonSerializer.Serialize(new { message = "Internal Server Error" });
                await response.WriteAsync(result);
            }
        }
    }
}
