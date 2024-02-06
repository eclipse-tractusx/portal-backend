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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class CompanyCertificateRepositoryTests
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly Guid _validCompanyId = new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");
    private readonly Guid _userId = new("ac1cf001-7fbc-1f2f-817f-bce058020006");

    public CompanyCertificateRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region CheckCompanyCertificateType

    [Theory]
    [InlineData(CompanyCertificateTypeId.IATF, true)]
    [InlineData(CompanyCertificateTypeId.AEO_CTPAT_Security_Declaration, true)]
#pragma warning disable xUnit1012
    [InlineData(default, false)]
#pragma warning restore xUnit1012
    public async Task CheckCompanyCertificateType_WithTypeId_ReturnsTrue(CompanyCertificateTypeId typeId, bool exists)
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.CheckCompanyCertificateType(typeId).ConfigureAwait(false);

        // Assert
        result.Should().Be(exists);
    }

    #endregion

    #region CheckCompanyCertificateId

    [Fact]
    public async Task CheckCompanyCertificateId_WithNoExistingData_ReturnsFalse()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.CheckCompanyCertificateId(new Guid("1268a76a-ca19-4dd8-b932-01f24071d560")).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CreateCertificate
    [Fact]
    public async Task CreateCompanyCertificateData_WithValidData_ReturnsExpected()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();

        // Act
        sut.CreateCompanyCertificateData(new("9f5b9934-4014-4099-91e9-7b1aee696b03"), CompanyCertificateTypeId.IATF, new Guid("00000000-0000-0000-0000-000000000001"), DateTime.UtcNow, null);

        // Assert
        context.ChangeTracker.HasChanges().Should().BeTrue();
        context.ChangeTracker.Entries().Should().ContainSingle()
            .Which.Entity.Should().BeOfType<CompanyCertificate>()
            .Which.CompanyCertificateStatusId.Should().Be(CompanyCertificateStatusId.ACTIVE);
    }
    #endregion

    #region Setup
    private async Task<CompanyCertificateRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        return new CompanyCertificateRepository(context);
    }

    private async Task<(CompanyCertificateRepository sut, PortalDbContext context)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        return (new CompanyCertificateRepository(context), context);
    }

    #endregion
}
