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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class PortalDbContextTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    public PortalDbContextTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region SaveAuditableEntity

    [Fact]
    public async Task SaveAuditableEntity_SetsLastEditorId()
    {
        // Arrange
        var sut = await CreateContext().ConfigureAwait(false);
        using var trans = await sut.Database.BeginTransactionAsync().ConfigureAwait(false);
        var ca = await sut.CompanyApplications.FirstAsync().ConfigureAwait(false);

        // Act
        ca.DateLastChanged = DateTimeOffset.UtcNow;
        await sut.SaveChangesAsync().ConfigureAwait(false);

        // Assert
        ca.LastEditorId.Should().NotBeNull().And.Be(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"));
        await trans.RollbackAsync().ConfigureAwait(false);
    }

    #endregion

    private async Task<PortalDbContext> CreateContext() =>
        await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
}
