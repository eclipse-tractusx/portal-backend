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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Web.Tests;

public class HealthCheckExtensionsTests : IClassFixture<WebApplicationFactory<HealthCheckExtensionsTests>>
{
    private static readonly JsonSerializerOptions _options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IFixture _fixture;
    private readonly IHealthCheck _healthCheck;

    public HealthCheckExtensionsTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _healthCheck = A.Fake<IHealthCheck>();
    }

    [Fact]
    public void MapDefaultHealthChecks_AmbiguousPathes_Throws()
    {
        var settings = new HealthCheckSettings[] {
            new () { Path = "/foo" },
            new () { Path = "/foo" }
        };

        using var app = WebApplication.Create();

        var result = Assert.Throws<ConfigurationException>(() => app.MapDefaultHealthChecks(settings));
        result.Message.Should().Be("HealthChecks mapping /foo, /foo contains ambiguous pathes");
    }

    [Fact]
    public async Task MapDefaultHealthChecks_MatchingTag_Healthy_ReturnsExpected()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var tag = _fixture.Create<string>();

        var settings = new HealthCheckSettings[] {
            new () { Path = "/health", Tags = new [] { tag }}
        };

        var description = _fixture.Create<string>();

        A.CallTo(() => _healthCheck.CheckHealthAsync(A<HealthCheckContext>._, A<CancellationToken>._))
            .Returns(Task.FromResult(HealthCheckResult.Healthy(description)));

        var sut = CreateSut(name, new[] { tag });

        // Act
        sut.MapDefaultHealthChecks(settings);

        // Assert
        var response = await GetHealthResponse(sut).ConfigureAwait(false);
        response.Should().NotBeNull().And.BeOfType<HealthResponse>();
        response!.Status.Should().Be("Healthy");
        response.Info.Should().ContainSingle().Which.Should().Match<HealthInfo>(x => x.Key == name && x.Description == description && x.Status == "Healthy" && x.Error == null);

        A.CallTo(() => _healthCheck.CheckHealthAsync(A<HealthCheckContext>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task MapDefaultHealthChecks_MatchingTag_Unhealthy_ReturnsExpected()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var tag = _fixture.Create<string>();

        var settings = new HealthCheckSettings[] {
            new () { Path = "/health", Tags = new [] { tag }}
        };

        var description = _fixture.Create<string>();
        var error = new Exception(_fixture.Create<string>());

        A.CallTo(() => _healthCheck.CheckHealthAsync(A<HealthCheckContext>._, A<CancellationToken>._))
            .Returns(Task.FromResult(HealthCheckResult.Unhealthy(description, error)));

        var sut = CreateSut(name, new[] { tag });

        // Act
        sut.MapDefaultHealthChecks(settings);

        // Assert
        var response = await GetHealthResponse(sut).ConfigureAwait(false);
        response.Should().NotBeNull().And.BeOfType<HealthResponse>();
        response!.Status.Should().Be("Unhealthy");
        response.Info.Should().ContainSingle().Which.Should().Match<HealthInfo>(x => x.Key == name && x.Description == description && x.Status == "Unhealthy" && x.Error == error.Message);

        A.CallTo(() => _healthCheck.CheckHealthAsync(A<HealthCheckContext>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task MapDefaultHealthChecks_NoTags_ReturnsExpected()
    {
        // Arrange
        var settings = new HealthCheckSettings[] {
            new () { Path = "/health", Tags = null }
        };

        var description = _fixture.Create<string>();
        var error = new Exception(_fixture.Create<string>());

        A.CallTo(() => _healthCheck.CheckHealthAsync(A<HealthCheckContext>._, A<CancellationToken>._))
            .Returns(Task.FromResult(HealthCheckResult.Unhealthy(description, error)));

        var sut = CreateSut(_fixture.Create<string>(), _fixture.CreateMany<string>());

        // Act
        sut.MapDefaultHealthChecks(settings);

        // Assert
        var response = await GetHealthResponse(sut).ConfigureAwait(false);
        response.Should().NotBeNull().And.BeOfType<HealthResponse>();
        response!.Status.Should().Be("Healthy");
        response.Info.Should().BeEmpty();

        A.CallTo(() => _healthCheck.CheckHealthAsync(A<HealthCheckContext>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task MapDefaultHealthChecks_NoMatchingTag_ReturnsExpected()
    {
        // Arrange
        var settings = new HealthCheckSettings[] {
            new () { Path = "/health", Tags = _fixture.CreateMany<string>() }
        };

        var description = _fixture.Create<string>();
        var error = new Exception(_fixture.Create<string>());

        A.CallTo(() => _healthCheck.CheckHealthAsync(A<HealthCheckContext>._, A<CancellationToken>._))
            .Returns(Task.FromResult(HealthCheckResult.Unhealthy(description, error)));

        var sut = CreateSut(_fixture.Create<string>(), _fixture.CreateMany<string>());

        // Act
        sut.MapDefaultHealthChecks(settings);

        // Assert
        var response = await GetHealthResponse(sut).ConfigureAwait(false);
        response.Should().NotBeNull().And.BeOfType<HealthResponse>();
        response!.Status.Should().Be("Healthy");
        response.Info.Should().BeEmpty();

        A.CallTo(() => _healthCheck.CheckHealthAsync(A<HealthCheckContext>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    #region Setup

    private WebApplication CreateSut(string name, IEnumerable<string> tags)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHealthChecks().AddCheck(name, _healthCheck, null, tags);
        return builder.Build();
    }

    private static async Task<HealthResponse?> GetHealthResponse(WebApplication app)
    {
        await app.StartAsync().ConfigureAwait(false);
        try
        {
            using var httpClient = new HttpClient();
            using var result = await httpClient.GetAsync("http://localhost:5000/health").ConfigureAwait(false);
            await using var responseStream = await result.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return await JsonSerializer.DeserializeAsync<HealthResponse>(responseStream, _options).ConfigureAwait(false);
        }
        finally
        {
            await app.StopAsync().ConfigureAwait(false);
        }
    }

    public record HealthInfo(string Key, string? Description, TimeSpan Duration, string? Status, string? Error, IDictionary<string, object> Data);

    public record HealthResponse(string Status, TimeSpan Duration, IEnumerable<HealthInfo> Info);

    #endregion
}
