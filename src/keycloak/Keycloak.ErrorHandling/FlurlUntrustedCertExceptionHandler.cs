/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Flurl.Http;
using Flurl.Http.Configuration;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;

public static class FlurlUntrustedCertExceptionHandler
{
    public static void ConfigureExceptions(IEnumerable<string> urlsToTrust)
    {
        foreach (var urlToTrust in urlsToTrust)
        {
            FlurlHttp.ConfigureClient(urlToTrust, cli => cli.Settings.HttpClientFactory = new UntrustedCertHttpClientFactory());
        }
    }
}

public class UntrustedCertHttpClientFactory : DefaultHttpClientFactory
{
    public override HttpMessageHandler CreateMessageHandler()
    {
        var handler = base.CreateMessageHandler();
        var httpClientHander = handler as HttpClientHandler;
        if (httpClientHander != null)
        {
            httpClientHander.ServerCertificateCustomValidationCallback = (_,_,_,_) => true;
        }
        else
        {
            throw new ConfigurationException($"flurl HttpMessageHandler's type is not HttpClientHandler but {handler.GetType()}");
        }
        return handler;
    }
}
