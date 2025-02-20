/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web.Tests;

public class CustomAuthorizationMiddlewareTests
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
    private readonly CustomAuthorizationMiddleware _customAuthorizationMiddleware;

    public CustomAuthorizationMiddlewareTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var errorMessageService = A.Fake<IErrorMessageService>();
        A.CallTo(() => errorMessageService.GetMessage(A<Type>._, A<int>._))
            .ReturnsLazily((Type type, int code) => $"type: {type.Name} code: {code} first: {{first}} second: {{second}}");

        _customAuthorizationMiddleware = new CustomAuthorizationMiddleware(errorMessageService);
    }

    [Fact]
    public async Task Invoke_WithForbidden()
    {
        // Arrange
        Task MockNextMiddleware(HttpContext _) => Task.CompletedTask;
        using var body = new MemoryStream();
        var httpContext = new DefaultHttpContext { Response = { Body = body, StatusCode = StatusCodes.Status403Forbidden } };

        // Act
        await _customAuthorizationMiddleware.InvokeAsync(httpContext, MockNextMiddleware);

        // Assert
        ((HttpStatusCode)httpContext.Response.StatusCode).Should().Be(HttpStatusCode.Forbidden);

        body.Position = 0;
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(body, Options);
        errorResponse.Should().NotBeNull().And.BeOfType<ErrorResponse>().Which.Details.Should().ContainSingle().Which.Should().Match<ErrorDetails>(x =>
            x.Type == "ForbiddenException" &&
            x.ErrorCode == "ForbiddenAccess" &&
            x.Message == "Access forbidden" &&
            !x.Parameters.Any());
    }

    [Fact]
    public async Task Invoke_WithUnauthorized()
    {
        // Arrange
        Task MockNextMiddleware(HttpContext _) => Task.CompletedTask;
        using var body = new MemoryStream();
        var httpContext = new DefaultHttpContext { Response = { Body = body, StatusCode = StatusCodes.Status401Unauthorized } };

        // Act
        await _customAuthorizationMiddleware.InvokeAsync(httpContext, MockNextMiddleware);

        // Assert
        ((HttpStatusCode)httpContext.Response.StatusCode).Should().Be(HttpStatusCode.Unauthorized);

        body.Position = 0;
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(body, Options);
        errorResponse.Should().NotBeNull().And.BeOfType<ErrorResponse>().Which.Details.Should().ContainSingle().Which.Should().Match<ErrorDetails>(x =>
            x.Type == "UnauthorizedAccessException" &&
            x.ErrorCode == "UnauthorizedAccess" &&
            x.Message == "Unauthorized access" &&
            !x.Parameters.Any());
    }
}
