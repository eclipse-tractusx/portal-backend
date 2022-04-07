using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace CatenaX.NetworkServices.Keycloak.Factory.Utils

{
    public class KeycloakUntrustedCertExceptionHandler
    {
        public static void ConfigureExceptions(IConfigurationSection section)
        {
            foreach (var urlToTrust in section.Get<KeycloakSettingsMap>().Values
                .Select(config => new Uri(config.ConnectionString))
                .Where(uri => uri.Scheme == "https")
                .Select(uri => uri.Scheme + "://" + uri.Host)
                .Distinct())
            {
                FlurlHttp.ConfigureClient(urlToTrust, cli => cli.Settings.HttpClientFactory = new UntrustedCertClientFactory());
            }
        }
    }

    public class UntrustedCertClientFactory : DefaultHttpClientFactory
    {
        public override HttpMessageHandler CreateMessageHandler() =>
            new HttpClientHandler { ServerCertificateCustomValidationCallback = (a,b,c,d) => true };
    }

}
