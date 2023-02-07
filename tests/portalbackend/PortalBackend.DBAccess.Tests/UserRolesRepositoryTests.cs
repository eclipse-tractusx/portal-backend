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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class UserRolesRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private static readonly Guid ApplicationWithBpn = new("4829b64c-de6a-426c-81fc-c0bcf95bcb76");
    private readonly IFixture _fixture;
    private readonly TestDbFixture _dbTestDbFixture;

    public UserRolesRepositoryTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetUserWithUserRolesForApplicationId

    [Fact]
    public async Task GetUserWithUserRolesForApplicationId_WithValidData_ReturnsExpected()
    {
        var userRoleIds = new [] { 
            new Guid("58f897ec-0aad-4588-8ffa-5f45d6638633"),
            new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632"),
            new Guid("58f897ec-0aad-4588-8ffa-5f45d6638633"),
            new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632"),
            new Guid("7410693c-c893-409e-852f-9ee886ce94a6"),
            new Guid("607818be-4978-41f4-bf63-fa8d2de51154"),
        };        

        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);
        
        // Act
        var data = await sut.GetUserWithUserRolesForApplicationId(ApplicationWithBpn, userRoleIds).ToListAsync().ConfigureAwait(false);
        
        // Assert
        data.Should().HaveCount(6);
        data.Should().AllSatisfy(((Guid,string,IEnumerable<Guid> UserRoleIds) userData) => userData.UserRoleIds.Should().NotBeEmpty().And.AllSatisfy(userRoleId => userRoleIds.Should().Contain(userRoleId)));
    }
    
    #endregion

    #region GetUserRolesByClientId
    
    [Fact]
    public async Task GetUserRolesByClientId_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);
        
        // Act
        var data = await sut.GetUserRolesByClientId(Enumerable.Repeat("Cl1-CX-Registration", 1)).ToListAsync().ConfigureAwait(false);
        
        // Assert
        data.Should().HaveCount(1);
        var clientData = data.Single();
        clientData.ClientClientId.Should().Be("Cl1-CX-Registration");
        clientData.UserRoles.Should().HaveCount(4);
    }

    #endregion

    private async Task<UserRolesRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new UserRolesRepository(context);
        return sut;
    }
}
