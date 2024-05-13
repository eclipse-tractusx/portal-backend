/********************************************************************************
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

using Microsoft.Extensions.Configuration;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public class Secrets
{
    public string TempMailApiKey { get; set; }
    public string InterfaceHealthCheckTechClientId { get; set; }
    public string InterfaceHealthCheckTechClientSecret { get; set; }
    public string ClearingHouseClientId { get; set; }
    public string ClearingHouseClientSecret { get; set; }
    public string PortalUserName { get; set; }
    public string PortalUserPassword { get; set; }

    public Secrets()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Secrets>()
            .AddEnvironmentVariables()
            .Build();

        TempMailApiKey = configuration["TEMPMAIL_APIKEY"] ?? throw new ArgumentNullException(nameof(TempMailApiKey));
        InterfaceHealthCheckTechClientId = configuration["INTERFACE_HEALTH_CHECK_TECH_CLIENT_ID"] ?? throw new ArgumentNullException(nameof(InterfaceHealthCheckTechClientId));
        InterfaceHealthCheckTechClientSecret = configuration["INTERFACE_HEALTH_CHECK_TECH_CLIENT_SECRET"] ?? throw new ArgumentNullException(nameof(InterfaceHealthCheckTechClientSecret));
        ClearingHouseClientId = configuration["CLEARING_HOUSE_CLIENT_ID"] ?? throw new ArgumentNullException(nameof(ClearingHouseClientId));
        ClearingHouseClientSecret = configuration["CLEARING_HOUSE_CLIENT_SECRET"] ?? throw new ArgumentNullException(nameof(ClearingHouseClientSecret));
        PortalUserName = configuration["PORTAL_USER_NAME"] ?? throw new ArgumentNullException(nameof(PortalUserName));
        PortalUserPassword = configuration["PORTAL_USER_PASSWORD"] ?? throw new ArgumentNullException(nameof(PortalUserPassword));
    }
}
