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
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class CompanyRoleCollectionRolesViewTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    public CompanyRoleCollectionRolesViewTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }
    [Fact]
    public async Task CompanyRoleCollectionRolesView_GetAll_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateContext();

        // Act
        var result = await sut.CompanyRoleCollectionRolesView.ToListAsync();
        result.Should().HaveCount(34);
    }

    [Fact]
    public async Task CompanyRoleCollectionRolesView_GetSpecific_ReturnsExpected()
    {
        // Arrange
        var clientName = "technical_roles_management";
        var userRole = "Dataspace Discovery";
        var sut = await CreateContext();

        // Act
        var result = await sut.CompanyRoleCollectionRolesView.FirstOrDefaultAsync(x => x.ClientName == clientName && x.UserRole == userRole);
        result.Should().NotBeNull();
        result!.ClientName.Should().Be(clientName);
        result.CollectionName.Should().Be("Operator");
        result.UserRole.Should().Be(userRole);
    }

    private async Task<PortalDbContext> CreateContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        return context;
    }
}
