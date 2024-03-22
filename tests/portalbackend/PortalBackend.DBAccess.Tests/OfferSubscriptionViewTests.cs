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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class OfferSubscriptionViewTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    public OfferSubscriptionViewTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region OfferSubscriptionView

    [Fact]
    public async Task OfferSubscriptionView_GetAll_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateContext();

        // Act
        var result = await sut.OfferSubscriptionView.ToListAsync();
        result.Should().HaveCount(14);
    }

    [Fact]
    public async Task OfferSubscriptionView_GetSpecific_ReturnsExpected()
    {
        // Arrange
        var subscriptionId = new Guid("3de6a31f-a5d1-4f60-aa3a-4b1a769becbf");
        var sut = await CreateContext();

        // Act
        var result = await sut.OfferSubscriptionView.SingleOrDefaultAsync(x => x.SubscriptionId == subscriptionId);
        result.Should().NotBeNull();
        result!.SubscriptionId.Should().Be(subscriptionId);
        result.OfferTypeId.Should().Be(OfferTypeId.SERVICE);
        result.TechnicalUser.Should().Be(new Guid("d0c8ae19-d4f3-49cc-9cb4-6c766d4680f2"));
        result.AppInstance.Should().Be(new Guid("ab25c218-9ab3-4f1a-b6f4-6394fbc33c5b"));
    }

    #endregion

    private async Task<PortalDbContext> CreateContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        return context;
    }
}
