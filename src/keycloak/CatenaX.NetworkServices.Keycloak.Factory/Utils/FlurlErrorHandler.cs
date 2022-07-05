using CatenaX.NetworkServices.Framework.ErrorHandling;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CatenaX.NetworkServices.Keycloak.Factory.Utils

{
    public class FlurlErrorHandler
    {
        public static void ConfigureErrorHandler(ILogger logger, bool debugEnabled)
        {
            FlurlHttp.Configure(settings => settings.OnError = (call) =>
            {
                var message = $"{call.Response?.ReasonPhrase}: {call.Request?.RequestUri}";

                if (debugEnabled)
                {
                    var request = call.Request == null ? "" : $"{call.Request.Method} {call.Request.RequestUri} HTTP/{call.Request.Version}\n{call.Request.Headers}\n";
                    var requestBody = call.RequestBody == null ? "\n" : call.RequestBody.ToString() + "\n\n";
                    var response = call.Response == null ? "" : call.Response.ReasonPhrase + "\n";
                    var responseContent = call.Response?.Content == null ? "" : call.Response.Content.ReadAsStringAsync().Result + "\n";
                    logger.LogError(call.Exception, request + requestBody + response + responseContent);
                }
                else
                {
                    logger.LogError(call.Exception, message);
                }

                switch (call.HttpStatus)
                {
                    case HttpStatusCode.NotFound:
                        throw new NotFoundException(message, call.Exception);

                    case HttpStatusCode.BadRequest:
                        throw new ArgumentException(message, call.Exception);

                    default:
                        throw new ServiceException(message, call.Exception, call.HttpStatus.GetValueOrDefault());
                }
            });
        }
    }
}
