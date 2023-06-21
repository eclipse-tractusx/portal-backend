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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class CompanySsiDetailsRepositoryTests
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly Guid _validCompanyId = new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");

    public CompanySsiDetailsRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetDetailsForCompany

    [Fact]
    public async Task GetDetailsForCompany_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetUseCaseParticipationForCompany(_validCompanyId, "en").ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(5);
        result.Where(x => x.Description != null).Should().HaveCount(2).And.Satisfy(
            x => x.Description == "Traceability",
            x => x.Description == "Sustainability & CO2-Footprint");
        result.Where(x => x.CredentialId != null).Should().HaveCount(2).And.Satisfy(
            x => x.CredentialId == new Guid("9f5b9934-4014-4099-91e9-7b1aee696b03"),
            x => x.CredentialId == new Guid("9f5b9934-4014-4099-91e9-7b1aee696b04"));
    }

    #endregion

    private async Task<CompanySsiDetailsRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        return new CompanySsiDetailsRepository(context);
    }
}
