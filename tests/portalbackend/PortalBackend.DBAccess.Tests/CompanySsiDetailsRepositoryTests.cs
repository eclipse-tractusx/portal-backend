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
        result.Should().HaveCount(3);
        result.Where(x => x.Description != null).Should().HaveCount(3).And.Satisfy(
            x => x.Description == "Traceability",
            x => x.Description == "Sustainability & CO2-Footprint",
            x => x.Description == "Quality Management");
        var traceability = result.Single(x => x.CredentialType == VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK);
        traceability.VerifiedCredentials.Should().HaveCount(3).And.Satisfy(
            x => x.ExternalDetailData.Version == "1.0.0" && x.SsiDetailData.Single().ParticipationStatus == CompanySsiDetailStatusId.PENDING,
            x => x.ExternalDetailData.Version == "2.0.0" && !x.SsiDetailData.Any(),
            x => x.ExternalDetailData.Version == "3.0.0" && !x.SsiDetailData.Any());
    }

    #endregion

    #region GetDetailsForCompany

    [Fact]
    public async Task GetSsiCertificates_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetSsiCertificates(_validCompanyId).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().ContainSingle();
        var certificate = result.Single();
        certificate.CredentialType.Should().Be(VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE);
        certificate.SsiDetailData.Should().NotBeNull();
        certificate.SsiDetailData.Single().ParticipationStatus.Should().Be(CompanySsiDetailStatusId.PENDING);
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
        var result = sut.CreateSsiDetails(_validCompanyId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, new Guid("00000000-0000-0000-0000-000000000001"), CompanySsiDetailStatusId.PENDING, _userId,
            ssi =>
            {
                ssi.ExpiryDate = now;
            });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        result.CompanyId.Should().Be(_validCompanyId);
        result.ExpiryDate.Should().Be(now);
        result.VerifiedCredentialTypeId.Should().Be(VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK);
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<CompanySsiDetail>();
        var ssiDetail = changedEntries.Single().Entity as CompanySsiDetail;
        ssiDetail!.ExpiryDate.Should().Be(now);
        ssiDetail.CompanyId.Should().Be(_validCompanyId);
    }

    #endregion

    #region CreateCredentialDetails

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
        var result = await sut.GetCredentialTypeIdForExternalTypeDetailId(detailId).ConfigureAwait(false);

        // Assert
        result.Exists.Should().Be(exists);
        result.CredentialTypeId.Should().Be(typeId);
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
