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

public class AgreementViewTests : IAssemblyFixture<TestDbFixture>
{

    private readonly TestDbFixture _dbTestDbFixture;

    public AgreementViewTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    [Fact]
    public async Task AgreementView_GetAll_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateContext();

        // Act
        var result = await sut.AgreementView.ToListAsync();
        result.Should().HaveCount(8);
    }

    [Fact]
    public async Task AgreementView_GetSpecific_ReturnsExpected()
    {
        // Arrange
        var agreementId = new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1094");
        var sut = await CreateContext();

        // Act
        var result = await sut.AgreementView.SingleOrDefaultAsync(x => x.AgreementId == agreementId);
        result.Should().NotBeNull();
        result!.AgreementCompanyRole.Should().Be("SERVICE_PROVIDER");
        result.AgreementName.Should().Be("Terms & Conditions - Consultant");
        result.AgreementStatus.Should().Be("ACTIVE");
        result.Mandatory.Should().BeTrue();

    }

    private async Task<PortalDbContext> CreateContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        return context;
    }
}
