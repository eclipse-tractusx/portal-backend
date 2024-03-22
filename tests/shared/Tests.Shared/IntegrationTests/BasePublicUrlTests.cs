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
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.TestSeeds;
using Org.Eclipse.TractusX.Portal.Backend.Web.PublicInfos;
using System.Linq.Expressions;
using System.Net;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.IntegrationTests;

public class BasePublicUrlTests<TController, TSeeding> : IClassFixture<IntegrationTestFactory<TController, TSeeding>>
    where TController : class
    where TSeeding : class, IBaseSeeding
{
    protected readonly IntegrationTestFactory<TController, TSeeding> Factory;

    public BasePublicUrlTests(IntegrationTestFactory<TController, TSeeding> factory)
    {
        Factory = factory;
    }

    protected async Task OpenInformationController_ReturnsCorrectAmount(int resultCount, params Expression<Func<UrlInformation, bool>>[] satisfyPredicates)
    {
        // Arrange
        var client = Factory.CreateClient();
        var endpoint = new InformationEndpoints(client);

        // Act
        var response = await endpoint.Get();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.GetResultFromContent<List<UrlInformation>>();
        result.Should().HaveCount(resultCount);
        if (satisfyPredicates.Any())
        {
            result.Should().Satisfy(satisfyPredicates);
        }
    }
}
