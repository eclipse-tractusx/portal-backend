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
using System.Runtime.CompilerServices;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public static class TestResources
{
    public static readonly string Env;
    public static readonly string BasePortalUrl;
    public static readonly string BaseCentralIdpUrl;
    public static readonly string BasePortalBackendUrl;
    public static readonly string ClearingHouseUrl;
    public static readonly string ClearingHouseTokenUrl;
    public static readonly string BpdmUrl;
    public static readonly string NotificationOfferId;
    public static readonly string SdFactoryBaseUrl;
    public static readonly string WalletBaseUrl;
    public static readonly string PortalUserCompanyName;

    static TestResources()
    {
        var projectDir = Directory.GetParent(GetSourceFilePathName())!.FullName;
        var configPath = Path.Combine(projectDir, "appsettings.EndToEndTests.json");
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configPath)
            .AddEnvironmentVariables()
            .Build();
        Env = configuration["ENVIRONMENT"];
        NotificationOfferId = configuration["NOTIFICATION_OFFER_ID"];
        PortalUserCompanyName = configuration["PORTAL_USER_COMPANY_NAME"];
        BasePortalUrl = configuration["BASE_PORTAL_URL"].Replace("{Env}", Env);
        BaseCentralIdpUrl = configuration["BASE_CENTRAL_IDP_URL"].Replace("{Env}", Env);
        BasePortalBackendUrl = configuration["BASE_PORTAL_BACKEND_URL"].Replace("{Env}", Env);
        BpdmUrl = configuration["BPDM_URL"].Replace("{Env}", Env);
        ClearingHouseUrl = configuration["CLEARING_HOUSE_URL"];
        ClearingHouseTokenUrl = configuration["CLEARING_HOUSE_TOKEN_URL"];
        SdFactoryBaseUrl = configuration["SD_FACTORY_BASE_URL"].Replace("{Env}", Env);
        WalletBaseUrl = configuration["WALLET_BASE_URL"].Replace("{Env}", Env);
    }

    public static string GetSourceFilePathName([CallerFilePath] string? callerFilePath = null) => callerFilePath ?? "";
}
