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
using AutoFixture.Dsl;
using AutoFixture.Kernel;
using FakeItEasy;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.Linq.Expressions;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

public static class AutoFixtureExtensions
{
    public static readonly string OrgNameRegex = @"[a-z]{40}";
    public static readonly string EmailRegex = @"[a-z]{20}@[a-z]{10}\.[a-z]{2}";
    public static readonly string NameRegex = @"^[a-z]{20}$";

    public static IFixture ConfigureFixture(this IFixture fixture)
    {
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        fixture.Customize<JsonDocument>(x => x.FromFactory(() => JsonDocument.Parse("{}")));
        return fixture;
    }

    public static IPostprocessComposer<T> WithEmailPattern<T>(this IPostprocessComposer<T> composer, Expression<Func<T, object?>> propertyPicker)
    {
        return composer.With(propertyPicker, new SpecimenContext(composer).Resolve(new RegularExpressionRequest(EmailRegex)));
    }

    public static IPostprocessComposer<T> WithNamePattern<T>(this IPostprocessComposer<T> composer, Expression<Func<T, object?>> propertyPicker)
    {
        return composer.With(propertyPicker, new SpecimenContext(composer).Resolve(new RegularExpressionRequest(NameRegex)));
    }

    public static IPostprocessComposer<T> WithOrgNamePattern<T>(this IPostprocessComposer<T> composer, Expression<Func<T, object?>> propertyPicker)
    {
        return composer.With(propertyPicker, new SpecimenContext(composer).Resolve(new RegularExpressionRequest(OrgNameRegex)));
    }

    public static void ConfigureTokenServiceFixture<T>(this IFixture fixture, HttpResponseMessage httpResponseMessage, Action<HttpRequestMessage?>? setMessage = null)
    {
        var messageHandler = A.Fake<HttpMessageHandler>();
        A.CallTo(messageHandler) // mock protected method
            .Where(x => x.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .ReturnsLazily(call =>
            {
                var message = call.Arguments.Get<HttpRequestMessage>(0);
                setMessage?.Invoke(message);
                return Task.FromResult(httpResponseMessage);
            });
        var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("https://example.com/path/test/") };
        fixture.Inject(httpClient);

        var tokenService = fixture.Freeze<Fake<ITokenService>>();
        A.CallTo(() => tokenService.FakedObject.GetAuthorizedClient<T>(A<KeyVaultAuthSettings>._, A<CancellationToken>._)).Returns(httpClient);
    }
}
