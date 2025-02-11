/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web.Tests;

public class GeneralHttpExceptionFilterTests
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    public GeneralHttpExceptionFilterTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public void Invoke_WithArgumentException_ContentStatusCodeIs400()
    {
        // Arrange
        var expectedException = new ArgumentException("That's a test");

        // Act
        TestGeneralHttpExceptionFilter(A.Fake<IMockLogger<GeneralHttpExceptionFilter>>(), expectedException, (int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Invoke_WithForbiddenException_ContentStatusCodeIs403()
    {
        // Arrange
        var expectedException = new ForbiddenException("That's a test");

        // Act
        TestGeneralHttpExceptionFilter(A.Fake<IMockLogger<GeneralHttpExceptionFilter>>(), expectedException, (int)HttpStatusCode.Forbidden);
    }

    [Fact]
    public void Invoke_WithNotFoundException_ContentStatusCodeIs404()
    {
        // Arrange
        var expectedException = new NotFoundException("That's a test");

        // Act
        TestGeneralHttpExceptionFilter(A.Fake<IMockLogger<GeneralHttpExceptionFilter>>(), expectedException, (int)HttpStatusCode.NotFound);
    }

    [Fact]
    public void Invoke_WithUnsupportedMediaTypeException_ContentStatusCodeIs415()
    {
        // Arrange
        var expectedException = new UnsupportedMediaTypeException("That's a test");

        // Act
        TestGeneralHttpExceptionFilter(A.Fake<IMockLogger<GeneralHttpExceptionFilter>>(), expectedException, (int)HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public void Invoke_WithConflictException_ContentStatus409()
    {
        // Arrange
        var expectedException = new ConflictException("That's a test");

        // Act
        TestGeneralHttpExceptionFilter(A.Fake<IMockLogger<GeneralHttpExceptionFilter>>(), expectedException, (int)HttpStatusCode.Conflict);
    }

    [Fact]
    public void Invoke_WithServiceException_ContentStatusCodeIs502()
    {
        // Arrange
        var expectedException = new ServiceException("That's a test");

        // Act
        TestGeneralHttpExceptionFilter(A.Fake<IMockLogger<GeneralHttpExceptionFilter>>(), expectedException, (int)HttpStatusCode.BadGateway);
    }

    [Fact]
    public void Invoke_WithConfigurationException_ContentStatusCodeIs500()
    {
        // Arrange
        var expectedException = new ConfigurationException("That's a test");

        // Act
        TestGeneralHttpExceptionFilter(A.Fake<IMockLogger<GeneralHttpExceptionFilter>>(), expectedException, (int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Invoke_WithControllerArgumentException_ContentStatusCodeIs400()
    {
        // Arrange
        var expectedException = new ControllerArgumentException("That's a test", "testParam");

        // Act
        TestGeneralHttpExceptionFilter(A.Fake<IMockLogger<GeneralHttpExceptionFilter>>(), expectedException, (int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Invoke_WithUnhandledSpecificException_ContentStatusCodeIs500()
    {
        // Arrange
        var expectedException = new AggregateException("That's a test");

        // Act
        TestGeneralHttpExceptionFilter(A.Fake<IMockLogger<GeneralHttpExceptionFilter>>(), expectedException, (int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Invoke_WithDetailedException()
    {
        // Arrange
        var expectedException = ConflictException.Create(TestErrors.FIRST_ERROR, new ErrorParameter[] { new("first", "foo"), new("second", "bar") });
        var mockLogger = A.Fake<IMockLogger<GeneralHttpExceptionFilter>>();

        // Act
        var exceptionContext = TestGeneralHttpExceptionFilter(mockLogger, expectedException, (int)HttpStatusCode.Conflict);

        // Assert
        A.CallTo(() => mockLogger.Log(
                A<LogLevel>.That.IsEqualTo(LogLevel.Information),
                expectedException,
                A<string>.That.Matches(lm =>
                    lm.StartsWith("GeneralErrorHandler caught ConflictException with errorId:") &&
                    lm.EndsWith("resulting in response status code 409, message 'type: TestErrors code: 1 first: foo second: bar'"))))
            .MustHaveHappenedOnceExactly();
        exceptionContext.Result.Should().BeOfType<ContentResult>()
            .Which.Content.Should().NotBeNull();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(exceptionContext.Result.Should().BeOfType<ContentResult>()
            .Which.Content!, Options);
        errorResponse.Should().NotBeNull().And.BeOfType<ErrorResponse>().Which.Details.Should().ContainSingle().Which.Should().Match<ErrorDetails>(ed =>
            ed.Type == "TestErrors" &&
            ed.ErrorCode == "FIRST_ERROR" &&
            ed.Message == "type: TestErrors code: 1 first: {first} second: {second}" &&
            ed.Parameters.Count() == 2 &&
            ed.Parameters.First(p => p.Name == "first").Value == "foo" &&
            ed.Parameters.First(p => p.Name == "second").Value == "bar");
    }

    [Fact]
    public void Invoke_WithInnerException()
    {
        // Arrange
        var expectedException = ServiceException.Create(TestErrors.FIRST_ERROR, new ErrorParameter[] { new("first", "foo"), new("second", "bar") }, new ForbiddenException("You don't have access to this resource", new UnauthorizedAccessException("No access")));
        var mockLogger = A.Fake<IMockLogger<GeneralHttpExceptionFilter>>();

        // Act
        var exceptionContext = TestGeneralHttpExceptionFilter(mockLogger, expectedException, (int)HttpStatusCode.BadGateway);

        // Assert
        A.CallTo(() => mockLogger.Log(
                A<LogLevel>.That.IsEqualTo(LogLevel.Information),
                expectedException,
                A<string>.That.Matches(x =>
                    x.StartsWith("GeneralErrorHandler caught ServiceException with errorId:") &&
                    x.EndsWith("resulting in response status code 502, message 'type: TestErrors code: 1 first: foo second: bar'"))))
            .MustHaveHappenedOnceExactly();

        exceptionContext.Result.Should().BeOfType<ContentResult>()
            .Which.Content.Should().NotBeNull();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(exceptionContext.Result.Should().BeOfType<ContentResult>()
            .Which.Content!, Options);
        errorResponse.Should().NotBeNull().And.BeOfType<ErrorResponse>();
        errorResponse!.Errors.Should().HaveCount(2).And.Satisfy(
            x => x.Key == "unknown",
            x => x.Key == "inner");
        errorResponse.Details.Should().ContainSingle().Which.Should().Match<ErrorDetails>(x =>
            x.Type == "TestErrors" &&
            x.ErrorCode == "FIRST_ERROR" &&
            x.Message == "type: TestErrors code: 1 first: {first} second: {second}" &&
            x.Parameters.Count() == 2 &&
            x.Parameters.First(p => p.Name == "first").Value == "foo" &&
            x.Parameters.First(p => p.Name == "second").Value == "bar");
    }

    private static ExceptionContext TestGeneralHttpExceptionFilter(IMockLogger<GeneralHttpExceptionFilter> mockLogger, Exception expectedException, int expectedStatusCode)
    {
        var actionContext = new ActionContext
        {
            HttpContext = new DefaultHttpContext(),
            RouteData = new(),
            ActionDescriptor = new()
        };
        var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = expectedException
        };

        var logger = new MockLogger<GeneralHttpExceptionFilter>(mockLogger);

        var errorMessageService = A.Fake<IErrorMessageService>();
        A.CallTo(() => errorMessageService.GetMessage(A<Type>._, A<int>._))
            .ReturnsLazily((Type type, int code) => $"type: {type.Name} code: {code} first: {{first}} second: {{second}}");

        var generalHttpErrorHandler = new GeneralHttpExceptionFilter(logger, errorMessageService);

        // Act
        generalHttpErrorHandler.OnException(exceptionContext);

        // Assert
        exceptionContext.Result.Should().BeOfType<ContentResult>()
            .Which.StatusCode.Should().Be(expectedStatusCode);

        return exceptionContext;
    }

    private enum TestErrors
    {
        FIRST_ERROR = 1,
        SECOND_ERROR = 2
    }
}
