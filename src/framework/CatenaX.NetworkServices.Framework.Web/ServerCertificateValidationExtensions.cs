/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace CatenaX.NetworkServices.Framework.Web;

/// <summary>
/// Provides methods for the server certificate validation
/// </summary>
public static class ServerCertificateValidationExtensions
{
    public static bool ServerCertificateCustomValidation(HttpRequestMessage requestMessage, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors sslErrors)
    {
        // It is possible inpect the certificate provided by server
        Console.WriteLine($"Requested URI: {requestMessage.RequestUri}");
        if (certificate != null)
        {
            Console.WriteLine($"Effective date: {certificate.GetEffectiveDateString()}");
            Console.WriteLine($"Exp date: {certificate.GetExpirationDateString()}");
            var certificateIssuer = certificate.Issuer;
            Console.WriteLine($"Issuer: {certificateIssuer}");
            Console.WriteLine($"Subject: {certificate.Subject}");

            if (certificateIssuer == "CN=devenv-ca")
            {
                return true;
            }
        }
        
        // Based on the custom logic it is possible to decide whether the client considers certificate valid or not
        Console.WriteLine($"Errors: {sslErrors}");
        return sslErrors == SslPolicyErrors.None;
    }
}