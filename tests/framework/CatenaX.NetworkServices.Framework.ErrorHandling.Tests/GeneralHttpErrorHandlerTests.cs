using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CatenaX.NetworkServices.Framework.ErrorHandling.Tests;

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
}