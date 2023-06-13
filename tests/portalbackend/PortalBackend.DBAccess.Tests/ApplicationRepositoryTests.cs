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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class ApplicationRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private static readonly Guid SubmittedApplicationWithBpn = new("6b2d1263-c073-4a48-bfaf-704dc154ca9f");
    private static readonly Guid ApplicationWithoutBpn = new("4829b64c-de6a-426c-81fc-c0bcf95bcb76");
    private static readonly Guid CompanyId = new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f88");
    private readonly IFixture _fixture;
    private readonly TestDbFixture _dbTestDbFixture;

    public ApplicationRepositoryTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetCompanyUserRoleWithAddressUntrackedAsync

    [Fact]
    public async Task GetCompanyUserRoleWithAddressUntrackedAsync_WithExistingEntry_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut
            .GetCompanyUserRoleWithAddressUntrackedAsync(new Guid("4f0146c6-32aa-4bb1-b844-df7e8babdcb2"))
            .ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();

        result!.CompanyId.Should().Be(new Guid("ac861325-bc54-4583-bcdc-9e9f2a38ff84"));
        result.Name.Should().Be("Bayerische Motorenwerke AG");
        result.Shortname.Should().Be("BMW AG");
        result.BusinessPartnerNumber.Should().Be("BPNL00000003AYRE");
        result.CountryAlpha2Code.Should().Be("DE");
        result.City.Should().Be("Munich");
        result.StreetName.Should().Be("Street");
        result.Streetadditional.Should().BeNull();
        result.Streetnumber.Should().Be("2");
        result.Region.Should().BeNull();
        result.Zipcode.Should().Be("00001");

        result.AgreementsData.Should().HaveCount(4);
        result.AgreementsData.Where(x => x.CompanyRoleId == CompanyRoleId.APP_PROVIDER).Should().HaveCount(1);
        result.AgreementsData.Where(x => x.CompanyRoleId == CompanyRoleId.ACTIVE_PARTICIPANT).Should().HaveCount(3);

        result.InvitedCompanyUserData.Should().BeEmpty();

        result.CompanyIdentifiers.Should().HaveCount(1);
        result.CompanyIdentifiers.First().Should().Match<(UniqueIdentifierId UniqueIdentifierId, string Value)>(identifier => identifier.UniqueIdentifierId == UniqueIdentifierId.VAT_ID && identifier.Value == "DE123456789");
    }

    #endregion GetRegistrationDataUntrackedAsync

    #region GetRegistrationDataUntrackedAsync

    [Fact]
    public async Task GetRegistrationDataUntrackedAsync_WithApplicationIdAndDocumentType_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetRegistrationDataUntrackedAsync(SubmittedApplicationWithBpn, new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f88"), new[] { DocumentTypeId.CX_FRAME_CONTRACT, DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT }).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsValidApplicationId.Should().BeTrue();
        result.IsValidCompany.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.DocumentNames.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRegistrationDataUntrackedAsync_WithInvalidApplicationId_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);
        var applicationId = Guid.NewGuid();

        // Act
        var result = await sut.GetRegistrationDataUntrackedAsync(applicationId, new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f88"), new[] { DocumentTypeId.CX_FRAME_CONTRACT, DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT }).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsValidApplicationId.Should().BeFalse();
        result.IsValidCompany.Should().BeFalse();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetRegistrationDataUntrackedAsync_WithInvalidUser_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetRegistrationDataUntrackedAsync(SubmittedApplicationWithBpn, Guid.NewGuid(), new[] { DocumentTypeId.CX_FRAME_CONTRACT, DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT }).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsValidApplicationId.Should().BeTrue();
        result.IsValidCompany.Should().BeFalse();
        result.Data.Should().BeNull();
    }

    #endregion

    #region GetBpnAndChecklistCheckForApplicationIdAsync

    [Fact]
    public async Task GetBpnAndChecklistCheckForApplicationIdAsync_WithValidApplicationId_ReturnsBpn()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var (bpn, existingChecklistEntryTypeIds) = await sut.GetBpnAndChecklistCheckForApplicationIdAsync(SubmittedApplicationWithBpn).ConfigureAwait(false);

        // Assert
        bpn.Should().Be("BPNL00000003CRHL");
        existingChecklistEntryTypeIds.Should().HaveCount(6);
    }

    [Fact]
    public async Task GetBpnAndChecklistCheckForApplicationIdAsync_WithNotExistingApplicationId_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetBpnAndChecklistCheckForApplicationIdAsync(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    [Fact]
    public async Task GetBpnAndChecklistCheckForApplicationIdAsync_WithApplicationIdWithoutBpn_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var (bpn, _) = await sut.GetBpnAndChecklistCheckForApplicationIdAsync(ApplicationWithoutBpn).ConfigureAwait(false);

        // Assert
        bpn.Should().BeNull();
    }

    #endregion

    #region GetApplicationStatusWithChecklistTypeStatusAsync

    [Fact]
    public async Task GetApplicationStatusWithChecklistTypeStatusAsync_WithValidApplicationId_ReturnExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var (applicationStatus, checklistEntryStatus) = await sut.GetApplicationStatusWithChecklistTypeStatusAsync(SubmittedApplicationWithBpn, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION).ConfigureAwait(false);

        // Assert
        applicationStatus.Should().Be(CompanyApplicationStatusId.SUBMITTED);
        checklistEntryStatus.Should().Be(ApplicationChecklistEntryStatusId.DONE);
    }

    [Fact]
    public async Task GetApplicationStatusWithRegistrationVerificationStatusAsync_WithNotExistingApplication_ReturnExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetApplicationStatusWithChecklistTypeStatusAsync(Guid.NewGuid(), ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION).ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    #endregion

    #region GetBpnForApplicationIdAsync

    [Fact]
    public async Task GetBpnForApplicationIdAsync_WithValidApplicationId_ReturnsBpn()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var bpn = await sut.GetBpnForApplicationIdAsync(SubmittedApplicationWithBpn).ConfigureAwait(false);

        // Assert
        bpn.Should().Be("BPNL00000003CRHL");
    }

    [Fact]
    public async Task GetBpnForApplicationIdAsync_WithNotExistingApplicationId_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var bpn = await sut.GetBpnForApplicationIdAsync(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        bpn.Should().BeNull();
    }

    [Fact]
    public async Task GetBpnForApplicationIdAsync_WithApplicationIdWithoutBpn_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var bpn = await sut.GetBpnForApplicationIdAsync(ApplicationWithoutBpn).ConfigureAwait(false);

        // Assert
        bpn.Should().BeNull();
    }

    #endregion

    #region GetClearinghouseDataForApplicationId

    [Fact]
    public async Task GetClearinghouseDataForApplicationId_WithValidApplicationId_ReturnsCorrectData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetClearinghouseDataForApplicationId(SubmittedApplicationWithBpn).ConfigureAwait(false);

        // Assert
        data.Should().NotBeNull();
        data!.ParticipantDetails.Bpn.Should().Be("BPNL00000003CRHL");
        data.ApplicationStatusId.Should().Be(CompanyApplicationStatusId.SUBMITTED);
    }

    [Fact]
    public async Task GetClearinghouseDataForApplicationId_WithNotExistingApplicationId_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetClearinghouseDataForApplicationId(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    #endregion

    #region GetCompanyAndApplicationDetailsForApprovalAsync

    [Fact]
    public async Task GetCompanyAndApplicationDetailsForApprovalAsync_WithSubmittedApplication_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetCompanyAndApplicationDetailsForApprovalAsync(SubmittedApplicationWithBpn).ConfigureAwait(false);

        // Assert
        data.CompanyId.Should().Be(CompanyId);
        data.BusinessPartnerNumber.Should().NotBeNullOrEmpty().And.Be("BPNL00000003CRHL");
    }

    [Fact]
    public async Task GetCompanyAndApplicationDetailsForApprovalAsync_WithNotExistingApplication_ReturnsDefault()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetCompanyAndApplicationDetailsForApprovalAsync(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        data.Should().Be(default);
    }

    #endregion

    #region GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync

    [Fact]
    public async Task GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync_WithSubmittedApplication_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(SubmittedApplicationWithBpn).ConfigureAwait(false);

        // Assert
        data.CompanyId.Should().Be(CompanyId);
        data.BusinessPartnerNumber.Should().NotBeNullOrEmpty().And.Be("BPNL00000003CRHL");
        data.Alpha2Code.Should().Be("DE");
        data.UniqueIdentifiers.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync_WithNotExistingApplication_ReturnsDefault()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        data.Should().Be(default);
    }

    #endregion

    #region GetApplicationChecklistData

    [Fact]
    public async Task GetApplicationChecklistData_WithExistingApplication_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetApplicationChecklistData(SubmittedApplicationWithBpn, Enum.GetValues<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert
        data.Exists.Should().BeTrue();
        data.ChecklistData.Should().HaveCount(5);
        data.ProcessStepTypeIds.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetApplicationChecklistData_WithNoProcessStepTypeIds_ReturnsNoProcessSteps()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetApplicationChecklistData(new Guid("4f0146c6-32aa-4bb1-b844-df7e8babdcb2"), Enumerable.Empty<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert
        data.Should().NotBe(default);
        data.ProcessStepTypeIds.Should().BeEmpty();
    }

    [Fact]
    public async Task GetApplicationChecklistData_WithNotExistingApplication_ReturnsDefault()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetApplicationChecklistData(Guid.NewGuid(), Enum.GetValues<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert
        data.Should().Be(default);
    }

    #endregion

    #region GetCompanyAndApplicationDetailsForCreateWalletAsync

    [Fact]
    public async Task GetCompanyAndApplicationDetailsForCreateWalletAsync_WithExistingApplication_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetCompanyAndApplicationDetailsForCreateWalletAsync(SubmittedApplicationWithBpn).ConfigureAwait(false);

        // Assert
        data.CompanyId.Should().Be(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f88"));
        data.BusinessPartnerNumber.Should().Be("BPNL00000003CRHL");
        data.CompanyName.Should().Be("CX-Test-Access");
    }

    [Fact]
    public async Task GetUserDataForRoleDeletionByIamClientIdsAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetCompanyAndApplicationDetailsForCreateWalletAsync(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        data.Should().Be(default);
    }

    #endregion

    #region GetCompanyIdSubmissionStatusForApplication

    [Fact]
    public async Task GetCompanyIdSubmissionStatusForApplication_WithExistingApplication_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetCompanyIdSubmissionStatusForApplication(SubmittedApplicationWithBpn).ConfigureAwait(false);

        // Assert
        data.Should().Be((true, CompanyId, true));
    }

    [Fact]
    public async Task GetCompanyIdSubmissionStatusForApplication_WithNotExistingApplication_ReturnsGuidEmpty()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetCompanyIdSubmissionStatusForApplication(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        data.Should().Be(((bool, Guid, bool))default);
    }

    #endregion

    private async Task<ApplicationRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new ApplicationRepository(context);
        return sut;
    }
}
