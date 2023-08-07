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

public class CompanySsiDetailsRepositoryTests
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly Guid _validCompanyId = new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");
    private readonly Guid _userId = new("ac1cf001-7fbc-1f2f-817f-bce058020006");

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
        result.Should().HaveCount(4);
        result.Where(x => x.Description != null).Should().HaveCount(4).And.Satisfy(
            x => x.Description == "Traceability",
            x => x.Description == "Sustainability & CO2-Footprint",
            x => x.Description == "Quality Management",
            x => x.Description == "Circular Economy");
        var traceability = result.Single(x => x.CredentialType == VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK);
        traceability.VerifiedCredentials.Should().HaveCount(3).And.Satisfy(
            x => x.ExternalDetailData.Version == "1.0.0" && x.SsiDetailData.Single().ParticipationStatus == CompanySsiDetailStatusId.PENDING,
            x => x.ExternalDetailData.Version == "2.0.0" && !x.SsiDetailData.Any(),
            x => x.ExternalDetailData.Version == "3.0.0" && !x.SsiDetailData.Any());
    }

    #endregion

    #region GetAllCredentialDetails

    [Fact]
    public async Task GetAllCredentialDetails_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetAllCredentialDetails(null, null, null).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(5);
        result.Should().HaveCount(5);
        result.Where(x => x.CompanyId == _validCompanyId).Should().HaveCount(4)
            .And.Satisfy(
                x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK && x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.PENDING,
                x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.PCF_FRAMEWORK && x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.PENDING,
                x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE && x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.PENDING,
                x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.BEHAVIOR_TWIN_FRAMEWORK && x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.INACTIVE);
        result.Where(x => x.CompanyId == new Guid("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd")).Should().ContainSingle()
            .And.Satisfy(x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK);
    }

    [Fact]
    public async Task GetAllCredentialDetails_WithWithStatusId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetAllCredentialDetails(CompanySsiDetailStatusId.PENDING, null, null).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull().And.HaveCount(4);
        result.Count.Should().Be(4);
        result.Where(x => x.CompanyId == _validCompanyId).Should().HaveCount(3)
            .And.Satisfy(
                x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK,
                x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.PCF_FRAMEWORK,
                x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE);
        result.Should().ContainSingle(x => x.CompanyId == new Guid("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd"))
            .Which.Should().Match<CompanySsiDetail>(x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK);
    }

    [Fact]
    public async Task GetAllCredentialDetails_WithWithCredentialType_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetAllCredentialDetails(null, VerifiedCredentialTypeId.PCF_FRAMEWORK, null).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull().And.ContainSingle().Which.CompanyId.Should().Be(_validCompanyId);
        result.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetAllCredentialDetails_WithWithCompanyName_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetAllCredentialDetails(null, null, "Service").ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull().And.ContainSingle().Which.CompanyId.Should().Be(new Guid("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd"));
        result.Count.Should().Be(1);
    }

    #endregion

    #region GetSsiCertificates

    [Fact]
    public async Task GetSsiCertificates_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetSsiCertificates(_validCompanyId).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Match<Models.SsiCertificateTransferData>(x =>
                x.CredentialType == VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE &&
                x.SsiDetailData != null &&
                x.SsiDetailData.Count() == 1 &&
                x.SsiDetailData.Single().ParticipationStatus == CompanySsiDetailStatusId.PENDING);
    }

    #endregion

    #region CreateSsiDetails

    [Fact]
    public async Task CreateSsiDetails_WithValidData_ReturnsExpected()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();

        // Act
        sut.CreateSsiDetails(new("9f5b9934-4014-4099-91e9-7b1aee696b03"), VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, new Guid("00000000-0000-0000-0000-000000000001"), CompanySsiDetailStatusId.PENDING, _userId, null);

        // Assert
        context.ChangeTracker.HasChanges().Should().BeTrue();
        context.ChangeTracker.Entries().Should().ContainSingle()
            .Which.Entity.Should().BeOfType<CompanySsiDetail>()
            .Which.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.PENDING);
    }

    #endregion

    #region CheckSsiDetailsExistsForCompany

    [Fact]
    public async Task CheckCredentialDetailsExistsForCompany_WithExistingData_ReturnsTrue()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.CheckSsiDetailsExistsForCompany(_validCompanyId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, VerifiedCredentialTypeKindId.USE_CASE, new Guid("1268a76a-ca19-4dd8-b932-01f24071d560")).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckCredentialDetailsExistsForCompany_WithNotExistingData_ReturnsTrue()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.CheckSsiDetailsExistsForCompany(Guid.NewGuid(), VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, VerifiedCredentialTypeKindId.USE_CASE, new Guid("1268a76a-ca19-4dd8-b932-01f24071d560")).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckCredentialDetailsExistsForCompany_WithWrongTypeKindId_ReturnsTrue()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.CheckSsiDetailsExistsForCompany(Guid.NewGuid(), VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, VerifiedCredentialTypeKindId.CERTIFICATE, new Guid("1268a76a-ca19-4dd8-b932-01f24071d560")).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckCredentialDetailsExistsForCompany_WithInactive_ReturnsFalse()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.CheckSsiDetailsExistsForCompany(_validCompanyId, VerifiedCredentialTypeId.BEHAVIOR_TWIN_FRAMEWORK, VerifiedCredentialTypeKindId.USE_CASE, new Guid("1268a76a-ca19-4dd8-b932-01f24071d562")).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CheckUseCaseCredentialAndExternalTypeDetails

    [Theory]
    [InlineData(VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, "1268a76a-ca19-4dd8-b932-01f24071d560", true)]
    [InlineData(VerifiedCredentialTypeId.PCF_FRAMEWORK, "1268a76a-ca19-4dd8-b932-01f24071d561", true)]
#pragma warning disable xUnit1012
    [InlineData(default, "1268a76a-ca19-6666-b932-01f24071d561", false)]
#pragma warning restore xUnit1012
    public async Task CheckUseCaseCredentialAndExternalTypeDetails_WithTypeId_ReturnsTrue(VerifiedCredentialTypeId typeId, Guid detailId, bool exists)
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(detailId, typeId).ConfigureAwait(false);

        // Assert
        result.Should().Be(exists);
    }

    #endregion

    #region CheckSsiCertificateType

    [Theory]
    [InlineData(VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, false)]
    [InlineData(VerifiedCredentialTypeId.PCF_FRAMEWORK, false)]
    [InlineData(VerifiedCredentialTypeId.BEHAVIOR_TWIN_FRAMEWORK, false)]
    [InlineData(VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, true)]
    public async Task CheckSsiCertificateType_WithTypeId_ReturnsTrue(VerifiedCredentialTypeId typeId, bool expectedResult)
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.CheckSsiCertificateType(typeId).ConfigureAwait(false);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region GetSsiApprovalData

    [Fact]
    public async Task GetSsiApprovalData_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetSsiApprovalData(new("9f5b9934-4014-4099-91e9-7b1aee696b03")).ConfigureAwait(false);

        // Assert
        result.exists.Should().BeTrue();
        result.data.Bpn.Should().Be("BPNL00000003CRHK");
        result.data.UseCaseDetailData.Should().NotBeNull();
        result.data.UseCaseDetailData!.VerifiedCredentialExternalTypeId.Should().Be(VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL);
    }

    [Fact]
    public async Task GetSsiApprovalData_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetSsiApprovalData(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        result.exists.Should().BeFalse();
    }

    #endregion

    #region GetAllCredentialDetails

    [Fact]
    public async Task GetSsiRejectionData_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetSsiRejectionData(new("9f5b9934-4014-4099-91e9-7b1aee696b03")).ConfigureAwait(false);

        // Assert
        result.Exists.Should().BeTrue();
        result.Status.Should().Be(CompanySsiDetailStatusId.PENDING);
        result.Type.Should().Be(VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK);
    }

    [Fact]
    public async Task GetSsiRejectionData_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetSsiRejectionData(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    #endregion

    #region CreateCredentialDetails

    [Fact]
    public async Task CreateCredentialDetails_WithValidData_ReturnsExpected()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var (sut, context) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyCompanySsiDetails(new("9f5b9934-4014-4099-91e9-7b1aee696b03"), null, ssi =>
            {
                ssi.CompanySsiDetailStatusId = CompanySsiDetailStatusId.INACTIVE;
            });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle()
            .Which.Entity.Should().BeOfType<CompanySsiDetail>()
            .And.Match<CompanySsiDetail>(x => x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.INACTIVE);
    }

    [Fact]
    public async Task CreateCredentialDetails_WithNoChanges_ReturnsExpected()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var (sut, context) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyCompanySsiDetails(new("9f5b9934-4014-4099-91e9-7b1aee696b03"), ssi =>
        {
            ssi.CompanySsiDetailStatusId = CompanySsiDetailStatusId.INACTIVE;
        }, ssi =>
        {
            ssi.CompanySsiDetailStatusId = CompanySsiDetailStatusId.INACTIVE;
        });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeFalse();
        changedEntries.Should().ContainSingle()
            .Which.Entity.Should().BeOfType<CompanySsiDetail>()
            .And.Match<CompanySsiDetail>(x => x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.INACTIVE);
    }

    #endregion

    #region GetCertificateTypes

    [Fact]
    public async Task GetCertificateTypes_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetCertificateTypes().ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().ContainSingle().Which.Should().Be(VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE);
    }

    #endregion

    #region Setup

    private async Task<CompanySsiDetailsRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        return new CompanySsiDetailsRepository(context);
    }

    private async Task<(CompanySsiDetailsRepository sut, PortalDbContext context)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        return (new CompanySsiDetailsRepository(context), context);
    }

    #endregion
}
