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

using Castle.Core.Internal;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;
using RestAssured.Response.Logging;
using Xunit;
using Xunit.Abstractions;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

[Trait("Category", "PortalHC")]
[TestCaseOrderer("Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests.AlphabeticalOrderer",
    "EndToEnd.Tests")]
[Collection("PortalHC")]
public class BaseDataLoadCheck : EndToEndTestBase
{
    private static readonly string BaseUrl = TestResources.BasePortalBackendUrl;
    private static readonly string EndPoint = "/api/administration";
    private static readonly Secrets Secrets = new();
    private static string? PortalUserToken;
    private static readonly string PortalUserCompanyName = TestResources.PortalUserCompanyName;

    public BaseDataLoadCheck(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetAccessToken() // in order to just get token once, ensure that method name is alphabetically before other tests cases
    {
        PortalUserToken = await new AuthFlow(PortalUserCompanyName).GetAccessToken(Secrets.PortalUserName,
            Secrets.PortalUserPassword);

        PortalUserToken.Should().NotBeNullOrEmpty("Token for the portal user could not be fetched correctly");
    }

    // GET: /api/administration/staticdata/usecases
    [Fact]
    public void GetUseCaseData()
    {
        if (PortalUserToken.IsNullOrEmpty())
            throw new Exception("Portal user token could not be fetched");

        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/staticdata/usecases")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        response.Content.ReadAsStringAsync().Result.Should().NotBeNullOrEmpty("Error: Response body is null or empty");
    }

    //     GET: /api/administration/staticdata/languagetags
    [Fact]
    public void GetAppLanguageTags()
    {
        if (PortalUserToken.IsNullOrEmpty())
            throw new Exception("Portal user token could not be fetched");

        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/staticdata/languagetags")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        response.Content.ReadAsStringAsync().Result.Should().NotBeNullOrEmpty("Response body is null or empty");
    }

    //     GET: /api/administration/staticdata/licenseType
    [Fact]
    public void GetAllLicenseTypes()
    {
        if (PortalUserToken.IsNullOrEmpty())
            throw new Exception("Portal user token could not be fetched");

        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/staticdata/licenseType")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        response.Content.ReadAsStringAsync().Result.Should().NotBeNullOrEmpty("Response body is null or empty");
    }

    //     GET: api/administration/user/owncompany/users
    [Fact]
    public void GetCompanyUserData()
    {
        if (PortalUserToken.IsNullOrEmpty())
            throw new Exception("Portal user token could not be fetched");

        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/user/owncompany/users?page=0&size=5")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        response.Content.ReadAsStringAsync().Result.Should().NotBeNullOrEmpty("Response body is null or empty");
    }

    //     GET: api/administration/companydata/ownCompanyDetails
    [Fact]
    public void GetOwnCompanyDetails()
    {
        if (PortalUserToken.IsNullOrEmpty())
            throw new Exception("Portal user token could not be fetched");

        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/companydata/ownCompanyDetails")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        response.Content.ReadAsStringAsync().Result.Should().NotBeNullOrEmpty("Response body is null or empty");
    }
}
