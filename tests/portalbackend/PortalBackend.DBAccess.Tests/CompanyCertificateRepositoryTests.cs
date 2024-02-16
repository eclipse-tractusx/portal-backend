/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class CompanyCertificateRepositoryTests
{
    private readonly TestDbFixture _dbTestDbFixture;

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

    #region CreateCertificate
    [Fact]
    public async Task CreateCompanyCertificateData_WithValidData_ReturnsExpected()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();

        // Act
        sut.CreateCompanyCertificate(new("9f5b9934-4014-4099-91e9-7b1aee696b03"), CompanyCertificateTypeId.IATF, new Guid("00000000-0000-0000-0000-000000000001"), x =>
        {
            x.ValidTill = DateTime.UtcNow;
        });

        // Assert
        context.ChangeTracker.HasChanges().Should().BeTrue();
        context.ChangeTracker.Entries().Should().ContainSingle()
            .Which.Entity.Should().BeOfType<CompanyCertificate>()
            .Which.CompanyCertificateStatusId.Should().Be(CompanyCertificateStatusId.ACTIVE);
    }
    #endregion

    #region GetAllCertificateData

    [Theory]
    [InlineData(CertificateSorting.CertificateTypeAsc)]
    [InlineData(CertificateSorting.CertificateTypeDesc)]
    public async Task GetAllCertificates_ReturnsExpectedResult(CertificateSorting sorting)
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);
        var certificateTypeStatus = new CertificateTypeStatusFilter(0, 0);

        // Act
        var companyCertificateDetail = await sut.GetActiveCompanyCertificatePaginationSource(sorting, certificateTypeStatus, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"))(0, 15).ConfigureAwait(false);

        // Assert
        companyCertificateDetail.Should().NotBeNull();
        companyCertificateDetail!.Count.Should().Be(6);
        companyCertificateDetail.Data.Should().HaveCount(6);
        if (sorting == CertificateSorting.CertificateTypeAsc)
        {
            companyCertificateDetail.Data.Select(data => data.companyCertificateType).Should().BeInAscendingOrder();
        }
        if (sorting == CertificateSorting.CertificateTypeDesc)
        {
            companyCertificateDetail.Data.Select(data => data.companyCertificateType).Should().BeInDescendingOrder();
        }
    }

    [Theory]
    [InlineData(CompanyCertificateStatusId.ACTIVE, CompanyCertificateTypeId.AEO_CTPAT_Security_Declaration, 0, 2, 1, 1)]
    [InlineData(CompanyCertificateStatusId.ACTIVE, CompanyCertificateTypeId.ISO_9001, 0, 2, 1, 1)]
    [InlineData(CompanyCertificateStatusId.INACTVIE, CompanyCertificateTypeId.IATF, 0, 2, 0, 0)]
    public async Task GetAllCertificates_WithExistingCompanyCertificateAndCertificateType_ReturnsExpectedResult(CompanyCertificateStatusId companyCertificateStatusId, CompanyCertificateTypeId companyCertificateTypeId, int page, int size, int count, int numData)
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);
        var certificateTypeStatus = new CertificateTypeStatusFilter(companyCertificateStatusId, companyCertificateTypeId);

        // Act
        var companyCertificateDetail = await sut.GetActiveCompanyCertificatePaginationSource(null, certificateTypeStatus, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"))(page, size).ConfigureAwait(false);

        // Assert
        if (count == 0)
        {
            companyCertificateDetail.Should().BeNull();
        }
        else
        {
            companyCertificateDetail.Should().NotBeNull();
            companyCertificateDetail!.Count.Should().Be(count);
            companyCertificateDetail.Data.Should().HaveCount(numData);
        }
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
