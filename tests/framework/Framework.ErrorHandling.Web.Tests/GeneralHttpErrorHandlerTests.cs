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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web;
using System.Net;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Tests;

public class GeneralHttpErrorHandlerTests
{
    private readonly IFixture _fixture;

    public GeneralHttpErrorHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
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
}
