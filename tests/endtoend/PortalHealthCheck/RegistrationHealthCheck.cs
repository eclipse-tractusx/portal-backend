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

using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

[Trait("Category", "PortalHC")]
[TestCaseOrderer("Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests.AlphabeticalOrderer", "EndToEnd.Tests")]
[Collection("PortalHC")]
public class RegistrationHealthCheck : EndToEndTestBase, IAsyncLifetime
{
    private readonly string _baseUrl = TestResources.BasePortalBackendUrl;
    private readonly string _endPoint = "/api/registration";
    private readonly string _portalUserCompanyName = TestResources.PortalUserCompanyName;
    private string? _portalUserToken;

    private static readonly Secrets Secrets = new();

    public RegistrationHealthCheck(ITestOutputHelper output) : base(output)
    {
    }

    public async Task InitializeAsync()
    {
        _portalUserToken = await new AuthFlow(_portalUserCompanyName).GetAccessToken(Secrets.PortalUserName,
            Secrets.PortalUserPassword);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetCompanyRoles()
    {
        _portalUserToken.Should().NotBeNullOrEmpty("Token for the portal user could not be fetched correctly");

        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/company/companyRoles")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();

        var data = await response.Content.ReadAsStringAsync();
        data.Should().NotBeNullOrEmpty("Response body is null or empty");
    }

    [Fact]
    public async Task GetCompanyRoleAgreementData()
    {
        _portalUserToken.Should().NotBeNullOrEmpty("Token for the portal user could not be fetched correctly");

        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/companyRoleAgreementData")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();

        var data = await response.Content.ReadAsStringAsync();
        data.Should().NotBeNullOrEmpty("Response body is null or empty");
    }

    [Fact]
    public async Task GetClientRolesComposite()
    {
        _portalUserToken.Should().NotBeNullOrEmpty("Token for the portal user could not be fetched correctly");

        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/rolesComposite")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();

        var data = await response.Content.ReadAsStringAsync();
        data.Should().NotBeNullOrEmpty("Response body is null or empty");
    }

    [Fact]
    public async Task GetApplicationsWithStatus()
    {
        _portalUserToken.Should().NotBeNullOrEmpty("Token for the portal user could not be fetched correctly");

        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/applications")
            .Then()
            .StatusCode(200)
            .Extract().Response();

        var data = await response.Content.ReadAsStringAsync();
        data.Should().NotBeNullOrEmpty("Response body is null or empty");
    }
}
