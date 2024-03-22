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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
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
        var result = await sut.CheckCompanyCertificateType(typeId);

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
        var sut = await CreateSut();

        // Act
        var companyCertificateDetail = await sut.GetActiveCompanyCertificatePaginationSource(sorting, null, null, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"))(0, 15);

        // Assert
        companyCertificateDetail.Should().NotBeNull();
        companyCertificateDetail!.Count.Should().Be(8);
        companyCertificateDetail.Data.Should().HaveCount(8);
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
    [InlineData(CompanyCertificateStatusId.INACTIVE, CompanyCertificateTypeId.IATF, 0, 2, 0, 0)]
    public async Task GetAllCertificates_WithExistingCompanyCertificateAndCertificateType_ReturnsExpectedResult(CompanyCertificateStatusId companyCertificateStatusId, CompanyCertificateTypeId companyCertificateTypeId, int page, int size, int count, int numData)
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var companyCertificateDetail = await sut.GetActiveCompanyCertificatePaginationSource(null, companyCertificateStatusId, companyCertificateTypeId, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"))(page, size);

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

    #region GetCompanyCertificatesBpn

    [Fact]
    public async Task GetCompanyId_WithExistingData()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetCompanyIdByBpn("BPNL07800HZ01643");

        // Assert
        result.Should().NotBe(Guid.Empty);
        result.Should().Be(new Guid("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd"));
    }

    [Fact]
    public async Task GetCompanyId_WithNoExistingData()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetCompanyIdByBpn("BPNL07800HZ01644");

        // Assert
        result.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task GetCompanyCertificateData_NoResults_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetCompanyCertificateData(Guid.NewGuid()).ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetCompanyCertificateDocumentByCompanyUserIdContentFile

    [Fact]
    public async Task GetCompanyCertificateDocumentByCompanyUserIdContentFile_WithValidData_ReturnsExpectedDocument()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetCompanyCertificateDocumentByCompanyIdDataAsync(new Guid("aaf53459-c36b-408e-a805-0b406ce9752d"), new Guid("41fd2ab8-71cd-4546-9bef-a388d91b2542"), DocumentTypeId.COMPANY_CERTIFICATE);

        // Assert
        result.Should().NotBe(default);
        result.FileName.Should().Be("AdditionalServiceDetails3.pdf");
        result.MediaTypeId.Should().Be(MediaTypeId.PDF);
    }

    [Fact]
    public async Task GetCompanyCertificateDocumentByCompanyUserIdContentFile_WithNotExistingDocument_ReturnsDefault()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetCompanyCertificateDocumentByCompanyIdDataAsync(Guid.NewGuid(), Guid.NewGuid(), DocumentTypeId.COMPANY_CERTIFICATE);

        // Assert
        result.Should().Be(default);
    }

    #endregion

    #region GetCompanyCertificateDocumentContentFile

    [Fact]
    public async Task GetCompanyCertificateDocumentContentFile_WithValidData_ReturnsExpectedDocument()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetCompanyCertificateDocumentDataAsync(new Guid("aaf53459-c36b-408e-a805-0b406ce9751f"), DocumentTypeId.COMPANY_CERTIFICATE);

        // Assert
        result.Should().NotBe(default);
        result.FileName.Should().Be("AdditionalServiceDetails2.pdf");
        result.MediaTypeId.Should().Be(MediaTypeId.PDF);
    }

    [Fact]
    public async Task GetCompanyCertificateDocumentContentFile_WithNotExistingDocument_ReturnsDefault()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetCompanyCertificateDocumentDataAsync(Guid.NewGuid(), DocumentTypeId.COMPANY_CERTIFICATE);

        // Assert
        result.Should().Be(default);
    }

    #endregion

    #region DeleteCertificate

    [Fact]
    public async Task GetCompanyCertificateDocumentDetailsForIdUntrackedAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var companyCertificateDetail = await sut.GetCompanyCertificateDocumentDetailsForIdUntrackedAsync(new Guid("aaf53459-c36b-408e-a805-0b406ce9751e"), new Guid("41fd2ab8-71cd-4546-9bef-a388d91b2542"));

        // Assert
        companyCertificateDetail.Should().NotBeNull();
        companyCertificateDetail.IsSameCompany.Should().Be(true);
        companyCertificateDetail.DocumentStatusId.Should().Be(DocumentStatusId.LOCKED);
    }

    [Fact]
    public async Task GetCompanyCertificateDocumentDetailsForIdUntrackedAsync_ReturnsInvalidExpectedResult()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var companyCertificateDetail = await sut.GetCompanyCertificateDocumentDetailsForIdUntrackedAsync(new Guid("aaf53459-c36b-408e-a805-0b406ce9751e"), new Guid("41fd2ab8-71cd-4546-9bef-a388d91b2544"));

        // Assert
        companyCertificateDetail.Should().NotBeNull();
        companyCertificateDetail.IsSameCompany.Should().Be(false);
    }

    [Fact]
    public async Task CompanyCertificateDetailsModify_WithValidData_ReturnsExpected()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var (sut, context) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyCompanyCertificateDetails(new("9f5b9934-4014-4099-91e9-7b1aee696c12"), null, x =>
            {
                x.CompanyCertificateStatusId = CompanyCertificateStatusId.INACTIVE;
            });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle()
            .Which.Entity.Should().BeOfType<CompanyCertificate>()
            .And.Match<CompanyCertificate>(x => x.CompanyCertificateStatusId == CompanyCertificateStatusId.INACTIVE);
    }

    [Fact]
    public async Task CompanyCertificateDocumentDetailsModify_WithValidData_ReturnsExpected()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var (sut, context) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyCompanyCertificateDocumentDetails(new("aaf53459-c36b-408e-a805-0b406ce9751e"), null, x =>
            {
                x.DocumentStatusId = DocumentStatusId.INACTIVE;
                x.DateLastChanged = now;
            });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle()
            .Which.Entity.Should().BeOfType<Document>()
            .And.Match<Document>(x => x.DocumentStatusId == DocumentStatusId.INACTIVE);
    }

    #endregion

    #region Setup

    private async Task<CompanyCertificateRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        return new CompanyCertificateRepository(context);
    }

    private async Task<(CompanyCertificateRepository sut, PortalDbContext context)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        return (new CompanyCertificateRepository(context), context);
    }

    #endregion
}
