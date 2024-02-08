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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web.Tests;

public class GeneralHttpErrorHandlerTests
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    public GeneralHttpErrorHandlerTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task Invoke_WithArgumentException_ContentStatusCodeIs400()
    {
        // Arrange
        var expectedException = new ArgumentException("That's a test");
        Task MockNextMiddleware(HttpContext _) => Task.FromException(expectedException);
        var httpContext = new DefaultHttpContext();
        var logger = A.Fake<ILogger<GeneralHttpErrorHandler>>();

        var generalHttpErrorHandler = new GeneralHttpErrorHandler(MockNextMiddleware, logger);

        // Act
        await generalHttpErrorHandler.Invoke(httpContext);

        // Assert
        ((HttpStatusCode)httpContext.Response.StatusCode).Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Invoke_WithForbiddenException_ContentStatusCodeIs403()
    {
        // Arrange
        var expectedException = new ForbiddenException("That's a test");
        Task MockNextMiddleware(HttpContext _) => Task.FromException(expectedException);
        var httpContext = new DefaultHttpContext();
        var logger = A.Fake<ILogger<GeneralHttpErrorHandler>>();

        var generalHttpErrorHandler = new GeneralHttpErrorHandler(MockNextMiddleware, logger);

        // Act
        await generalHttpErrorHandler.Invoke(httpContext);

        // Assert
        ((HttpStatusCode)httpContext.Response.StatusCode).Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Invoke_WithNotFoundException_ContentStatusCodeIs404()
    {
        // Arrange
        var expectedException = new NotFoundException("That's a test");
        Task MockNextMiddleware(HttpContext _) => Task.FromException(expectedException);
        var httpContext = new DefaultHttpContext();
        var logger = A.Fake<ILogger<GeneralHttpErrorHandler>>();

        var generalHttpErrorHandler = new GeneralHttpErrorHandler(MockNextMiddleware, logger);

        // Act
        await generalHttpErrorHandler.Invoke(httpContext);

        // Assert
        ((HttpStatusCode)httpContext.Response.StatusCode).Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Invoke_WithUnsupportedMediaTypeException_ContentStatusCodeIs415()
    {
        // Arrange
        var expectedException = new UnsupportedMediaTypeException("That's a test");
        Task MockNextMiddleware(HttpContext _) => Task.FromException(expectedException);
        var httpContext = new DefaultHttpContext();
        var logger = A.Fake<ILogger<GeneralHttpErrorHandler>>();

        var generalHttpErrorHandler = new GeneralHttpErrorHandler(MockNextMiddleware, logger);

        // Act
        await generalHttpErrorHandler.Invoke(httpContext);

        // Assert
        ((HttpStatusCode)httpContext.Response.StatusCode).Should().Be(HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task Invoke_WithConflictException_ContentStatus409()
    {
        // Arrange
        var expectedException = new ConflictException("That's a test");
        Task MockNextMiddleware(HttpContext _) => Task.FromException(expectedException);
        var httpContext = new DefaultHttpContext();
        var logger = A.Fake<ILogger<GeneralHttpErrorHandler>>();

        var generalHttpErrorHandler = new GeneralHttpErrorHandler(MockNextMiddleware, logger);

        // Act
        await generalHttpErrorHandler.Invoke(httpContext);

        // Assert
        ((HttpStatusCode)httpContext.Response.StatusCode).Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Invoke_WithServiceException_ContentStatusCodeIs502()
    {
        // Arrange
        var expectedException = new ServiceException("That's a test");
        Task MockNextMiddleware(HttpContext _) => Task.FromException(expectedException);
        var httpContext = new DefaultHttpContext();
        var logger = A.Fake<ILogger<GeneralHttpErrorHandler>>();

        var generalHttpErrorHandler = new GeneralHttpErrorHandler(MockNextMiddleware, logger);

        // Act
        await generalHttpErrorHandler.Invoke(httpContext);

        // Assert
        ((HttpStatusCode)httpContext.Response.StatusCode).Should().Be(HttpStatusCode.BadGateway);
    }

    [Fact]
    public async Task Invoke_WithConfigurationException_ContentStatusCodeIs500()
    {
        // Arrange
        var expectedException = new ConfigurationException("That's a test");
        Task MockNextMiddleware(HttpContext _) => Task.FromException(expectedException);
        var httpContext = new DefaultHttpContext();
        var logger = A.Fake<ILogger<GeneralHttpErrorHandler>>();

        var generalHttpErrorHandler = new GeneralHttpErrorHandler(MockNextMiddleware, logger);

        // Act
        await generalHttpErrorHandler.Invoke(httpContext);

        // Assert
        ((HttpStatusCode)httpContext.Response.StatusCode).Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Invoke_WithControllerArgumentException_ContentStatusCodeIs400()
    {
        // Arrange
        var expectedException = new ControllerArgumentException("That's a test", "testParam");
        Task MockNextMiddleware(HttpContext _) => Task.FromException(expectedException);
        var httpContext = new DefaultHttpContext();
        var logger = A.Fake<ILogger<GeneralHttpErrorHandler>>();

        var generalHttpErrorHandler = new GeneralHttpErrorHandler(MockNextMiddleware, logger);

        // Act
        await generalHttpErrorHandler.Invoke(httpContext);

        // Assert
        ((HttpStatusCode)httpContext.Response.StatusCode).Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Invoke_WithUnhandledSpecificException_ContentStatusCodeIs500()
    {
        // Arrange
        var expectedException = new AggregateException("That's a test");
        Task MockNextMiddleware(HttpContext _) => Task.FromException(expectedException);
        var httpContext = new DefaultHttpContext();
        var logger = A.Fake<ILogger<GeneralHttpErrorHandler>>();

        var generalHttpErrorHandler = new GeneralHttpErrorHandler(MockNextMiddleware, logger);

        // Act
        await generalHttpErrorHandler.Invoke(httpContext);

        // Assert
        ((HttpStatusCode)httpContext.Response.StatusCode).Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Invoke_WithDetailedException()
    {
        // Arrange
        var expectedException = ConflictException.Create(TestErrors.FIRST_ERROR, new ErrorParameter[] { new("first", "foo"), new("second", "bar") });
        Task MockNextMiddleware(HttpContext _) => Task.FromException(expectedException);
        using var body = new MemoryStream();
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = body;

        var mockLogger = A.Fake<IMockLogger<GeneralHttpErrorHandler>>();
        var logger = new MockLogger<GeneralHttpErrorHandler>(mockLogger);

        var errorMessageService = A.Fake<IErrorMessageService>();
        A.CallTo(() => errorMessageService.GetMessage(A<Type>._, A<int>._))
            .ReturnsLazily((Type type, int code) => $"type: {type.Name} code: {code} first: {{first}} second: {{second}}");

        var generalHttpErrorHandler = new GeneralHttpErrorHandler(MockNextMiddleware, logger, errorMessageService);

        // Act
        await generalHttpErrorHandler.Invoke(httpContext);

        // Assert
        ((HttpStatusCode)httpContext.Response.StatusCode).Should().Be(HttpStatusCode.Conflict);
        A.CallTo(() => mockLogger.Log(
                A<LogLevel>.That.IsEqualTo(LogLevel.Information),
                expectedException,
                A<string>.That.Matches(x =>
                    x.StartsWith("GeneralErrorHandler caught ConflictException with errorId:") &&
                    x.EndsWith("resulting in response status code 409, message 'type: TestErrors code: 1 first: foo second: bar'"))))
            .MustHaveHappenedOnceExactly();

        body.Position = 0;
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(body, Options);
        errorResponse.Should().NotBeNull().And.BeOfType<ErrorResponse>().Which.Details.Should().ContainSingle().Which.Should().Match<ErrorDetails>(x =>
            x.Type == "TestErrors" &&
            x.ErrorCode == "FIRST_ERROR" &&
            x.Message == "type: TestErrors code: 1 first: {first} second: {second}" &&
            x.Parameters.Count() == 2 &&
            x.Parameters.First(p => p.Name == "first").Value == "foo" &&
            x.Parameters.First(p => p.Name == "second").Value == "bar");
    }

    [Fact]
    public async Task Invoke_WithInnerException()
    {
        // Arrange
        var expectedException = ServiceException.Create(TestErrors.FIRST_ERROR, new ErrorParameter[] { new("first", "foo"), new("second", "bar") }, new ForbiddenException("You don't have access to this resource", new UnauthorizedAccessException("No access")));
        Task MockNextMiddleware(HttpContext _) => Task.FromException(expectedException);
        using var body = new MemoryStream();
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = body;

        var mockLogger = A.Fake<IMockLogger<GeneralHttpErrorHandler>>();
        var logger = new MockLogger<GeneralHttpErrorHandler>(mockLogger);

        var errorMessageService = A.Fake<IErrorMessageService>();
        A.CallTo(() => errorMessageService.GetMessage(A<Type>._, A<int>._))
            .ReturnsLazily((Type type, int code) => $"type: {type.Name} code: {code} first: {{first}} second: {{second}}");

        var generalHttpErrorHandler = new GeneralHttpErrorHandler(MockNextMiddleware, logger, errorMessageService);

        // Act
        await generalHttpErrorHandler.Invoke(httpContext);

        // Assert
        ((HttpStatusCode)httpContext.Response.StatusCode).Should().Be(HttpStatusCode.BadGateway);
        A.CallTo(() => mockLogger.Log(
                A<LogLevel>.That.IsEqualTo(LogLevel.Information),
                expectedException,
                A<string>.That.Matches(x =>
                    x.StartsWith("GeneralErrorHandler caught ServiceException with errorId:") &&
                    x.EndsWith("resulting in response status code 502, message 'type: TestErrors code: 1 first: foo second: bar'"))))
            .MustHaveHappenedOnceExactly();

        body.Position = 0;
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(body, Options);
        errorResponse.Should().NotBeNull().And.BeOfType<ErrorResponse>();
        errorResponse!.Errors.Should().HaveCount(2).And.Satisfy(
            x => x.Key == "System.Private.CoreLib",
            x => x.Key == "inner");
        errorResponse.Details.Should().ContainSingle().Which.Should().Match<ErrorDetails>(x =>
            x.Type == "TestErrors" &&
            x.ErrorCode == "FIRST_ERROR" &&
            x.Message == "type: TestErrors code: 1 first: {first} second: {second}" &&
            x.Parameters.Count() == 2 &&
            x.Parameters.First(p => p.Name == "first").Value == "foo" &&
            x.Parameters.First(p => p.Name == "second").Value == "bar");
    }

    private enum TestErrors
    {
        FIRST_ERROR = 1,
        SECOND_ERROR = 2
    }
}
