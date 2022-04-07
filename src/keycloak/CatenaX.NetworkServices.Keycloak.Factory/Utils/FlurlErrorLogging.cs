using Flurl.Http;
using Microsoft.Extensions.Logging;

namespace CatenaX.NetworkServices.Keycloak.Factory.Utils

{
    public class FlurlErrorLogging
    {
        public static void ConfigureLogger(ILogger logger)
        {
            FlurlHttp.Configure(settings => settings.OnError = (call) => {
                logger.LogError($"{call.Request.Method} {call.Request.RequestUri} HTTP/{call.Request.Version}\n{call.Request.Headers}\n{call.RequestBody.ToString()}\n\n{call.Response.ReasonPhrase}\n{call.Response.Content.ReadAsStringAsync().Result}\n");
                call.ExceptionHandled = true;
            });
        }
    }
}
