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

public class CompanyIdpViewTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    public CompanyIdpViewTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    [Fact]
    public async Task CompanyIdpView_GetAll_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateContext();

        // Act
        var result = await sut.CompanyIdpView.ToListAsync();
        result.Should().HaveCount(6);
    }

    [Fact]
    public async Task CompanyIdpView_GetSpecific_ReturnsExpected()
    {
        // Arrange
        var companyId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f88");
        var sut = await CreateContext();

        // Act
        var result = await sut.CompanyIdpView.SingleOrDefaultAsync(x => x.CompanyId == companyId);
        result.Should().NotBeNull();
        result!.CompanyId.Should().Be(companyId);
        result.CompanyName.Should().Be("CX-Test-Access");
        result.IdpAlias.Should().Be("Test-Alias");
    }

    private async Task<PortalDbContext> CreateContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        return context;
    }
}
