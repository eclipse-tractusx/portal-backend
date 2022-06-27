using CatenaX.NetworkServices.Framework.ErrorHandling;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CatenaX.NetworkServices.Keycloak.Factory.Utils

{
    public class FlurlErrorLogging
    {
        public static void ConfigureLogger(ILogger logger)
        {
            FlurlHttp.Configure(settings => settings.OnError = (call) => {
                var request = call.Request == null ? "" : $"{call.Request.Method} {call.Request.RequestUri} HTTP/{call.Request.Version}\n{call.Request.Headers}\n";
                var requestBody = call.RequestBody == null ? "\n" : call.RequestBody.ToString() + "\n\n";
                var response = call.Response == null ? "" : call.Response.ReasonPhrase + "\n";
                var responseContent = call.Response?.Content == null ? "" : call.Response.Content.ReadAsStringAsync().Result + "\n";
                logger.LogError(call.Exception, request + requestBody + response + responseContent);
                switch (call.HttpStatus)
                {
                    case HttpStatusCode.NotFound:
                        throw new NotFoundException($"{call.Response?.ReasonPhrase}: {call.Request?.RequestUri}", call.Exception);

                    case HttpStatusCode.BadRequest:
                        throw new ArgumentException($"{call.Response?.ReasonPhrase}: {call.Request?.RequestUri}", call.Exception);

                    default:
                        throw new ServiceException($"{call.Response?.ReasonPhrase}: {call.Request?.RequestUri}", call.Exception, call.HttpStatus.GetValueOrDefault());
                }
            });
        }
    }
}
