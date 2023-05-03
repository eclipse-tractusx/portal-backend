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

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Net;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Web.Tests;

public class JwtBearerConfigurationHealthCheckTests
{
    private readonly IFixture _fixture;
    public JwtBearerConfigurationHealthCheckTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task CheckHealthAsync_Success_ReturnsExpected()
    {
        // Arrange
        var config = _fixture.Create<OpenIdConnectConfiguration>();

        var jsonOptions = new JsonSerializerOptions() { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
        
        var jwtOptions = new JwtBearerOptions() {
            MetadataAddress = "https://foo.bar",
            BackchannelHttpHandler = new HttpMessageHandlerMock(
            HttpStatusCode.OK,
            config.ToJsonContent(
                jsonOptions,
                "application/json")
            )
        };

        var context = _fixture.Create<HealthCheckContext>();

        var sut = new JwtBearerConfigurationHealthCheck(Options.Create(jwtOptions));

        // Act
        var result = await sut.CheckHealthAsync(context).ConfigureAwait(false);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_Failure_ReturnsExpected()
    {
        // Arrange
        var jwtOptions = new JwtBearerOptions() {
            MetadataAddress = "https://foo.bar",
            BackchannelHttpHandler = new HttpMessageHandlerMock(HttpStatusCode.BadRequest)
        };

        var context = _fixture.Create<HealthCheckContext>();

        var sut = new JwtBearerConfigurationHealthCheck(Options.Create(jwtOptions));

        // Act
        var result = await sut.CheckHealthAsync(context).ConfigureAwait(false);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
    }
}
