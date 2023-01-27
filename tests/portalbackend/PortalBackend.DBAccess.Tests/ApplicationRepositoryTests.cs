/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class ApplicationRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private static readonly Guid ApplicationWithBpn = new("4829b64c-de6a-426c-81fc-c0bcf95bcb76");
    private static readonly Guid SubmittedApplicationWithBpn = new("2bb2005f-6e8d-41eb-967b-cde67546cafc");
    private static readonly Guid ApplicationWithoutBpn = new("1b86d973-3aac-4dcd-a9e9-0c222766202b");
    private static readonly Guid CompanyId = new("27538eac-27a3-4f74-9306-e5149b93ade5");
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
            .GetCompanyUserRoleWithAddressUntrackedAsync(new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76"))
            .ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();

        result!.CompanyId.Should().Be(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"));
        result.Name.Should().Be("Catena-X");
        result.Shortname.Should().Be("Cat-X");
        result.BusinessPartnerNumber.Should().Be("CAXSDUMMYCATENAZZ");
        result.CountryAlpha2Code.Should().Be("DE");
        result.City.Should().Be("Munich");
        result.StreetName.Should().Be("Street");
        result.Streetadditional.Should().Be("foo");
        result.Streetnumber.Should().Be("1");
        result.Region.Should().Be("BY");
        result.Zipcode.Should().Be("00001");

        result!.AgreementsData.Should().HaveCount(1);
        result.AgreementsData.First().ConsentStatusId.Should().Be(ConsentStatusId.ACTIVE);

        result.InvitedCompanyUserData.Should().HaveCount(2);
        result.InvitedCompanyUserData.Should().ContainSingle(u => u.FirstName == "Test User 1" && u.LastName == "cx-user-2" && u.Email == "tester.user1@test.de");
        result.InvitedCompanyUserData.Should().ContainSingle(u => u.FirstName == "Test User 2" && u.LastName == "cx-admin-2" && u.Email == "tester.user2@test.de");

        result.CompanyIdentifiers.Should().HaveCount(1);
        result.CompanyIdentifiers.First().Should().Match<(UniqueIdentifierId UniqueIdentifierId,string Value)>(identifier => identifier.UniqueIdentifierId == UniqueIdentifierId.COMMERCIAL_REG_NUMBER && identifier.Value == "REG08154711");
    }

    #endregion GetRegistrationDataUntrackedAsync

    #region GetRegistrationDataUntrackedAsync

    [Fact]
    public async Task GetRegistrationDataUntrackedAsync_WithApplicationIdAndDocumentType_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);
        var applicationId = new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76");

        // Act
        var result = await sut.GetRegistrationDataUntrackedAsync(applicationId, "623770c5-cf38-4b9f-9a35-f8b9ae972e2d", new [] { DocumentTypeId.CX_FRAME_CONTRACT, DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT }).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsValidApplicationId.Should().BeTrue();
        result.IsSameCompanyUser.Should().BeTrue();
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
        var result = await sut.GetRegistrationDataUntrackedAsync(applicationId, "623770c5-cf38-4b9f-9a35-f8b9ae972e2d", new [] { DocumentTypeId.CX_FRAME_CONTRACT, DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT }).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsValidApplicationId.Should().BeFalse();
        result.IsSameCompanyUser.Should().BeFalse();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetRegistrationDataUntrackedAsync_WithInvalidUser_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);
        var applicationId = new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76");
        // Act
        var result = await sut.GetRegistrationDataUntrackedAsync(applicationId, _fixture.Create<string>(), new [] { DocumentTypeId.CX_FRAME_CONTRACT, DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT }).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsValidApplicationId.Should().BeTrue();
        result.IsSameCompanyUser.Should().BeFalse();
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
        var (bpn, existingChecklistEntryTypeIds) = await sut.GetBpnAndChecklistCheckForApplicationIdAsync(ApplicationWithBpn).ConfigureAwait(false);
        
        // Assert
        bpn.Should().Be("CAXSDUMMYCATENAZZ");
        existingChecklistEntryTypeIds.Should().HaveCount(5);
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
        var (bpn, existingChecklistEntryTypeIds) = await sut.GetBpnAndChecklistCheckForApplicationIdAsync(ApplicationWithoutBpn).ConfigureAwait(false);
        
        // Assert
        bpn.Should().BeNull();
        existingChecklistEntryTypeIds.Should().HaveCount(5);
    }

    #endregion
    
    #region GetApplicationStatusWithChecklistTypeStatusAsync
    
    [Fact]
    public async Task GetApplicationStatusWithChecklistTypeStatusAsync_WithValidApplicationId_ReturnExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);
        
        // Act
        var (applicationStatus, checklistEntryStatus) = await sut.GetApplicationStatusWithChecklistTypeStatusAsync(ApplicationWithoutBpn, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION).ConfigureAwait(false);
        
        // Assert
        applicationStatus.Should().Be(CompanyApplicationStatusId.SUBMITTED);
        checklistEntryStatus.Should().Be(ApplicationChecklistEntryStatusId.TO_DO);
    }

    [Fact]
    public async Task GetApplicationStatusWithChecklistTypeStatusAsync_WithApplicationWithoutChecklist_ReturnExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);
        
        // Act
        var (applicationStatus, checklistEntryStatus) = await sut.GetApplicationStatusWithChecklistTypeStatusAsync(ApplicationWithBpn, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION).ConfigureAwait(false);
        
        // Assert
        applicationStatus.Should().Be(CompanyApplicationStatusId.CONFIRMED);
        checklistEntryStatus.Should().Be(ApplicationChecklistEntryStatusId.TO_DO);
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
        var bpn = await sut.GetBpnForApplicationIdAsync(ApplicationWithBpn).ConfigureAwait(false);
        
        // Assert
        bpn.Should().Be("CAXSDUMMYCATENAZZ");
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
        var data = await sut.GetClearinghouseDataForApplicationId(ApplicationWithBpn).ConfigureAwait(false);
        
        // Assert
        data.Should().NotBeNull();
        data!.ParticipantDetails.Bpn.Should().Be("CAXSDUMMYCATENAZZ");
        data.ApplicationStatusId.Should().Be(CompanyApplicationStatusId.CONFIRMED);
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

    #region GetSubmittedIdAndClearinghouseChecklistStatusByBpn
    
    [Fact]
    public async Task GetSubmittedIdAndClearinghouseChecklistStatusByBpn_WithValidApplicationId_ReturnsCorrectData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);
        
        // Act
        var data = await sut.GetSubmittedIdAndClearinghouseChecklistStatusByBpn("CAXSTESTYCATENAZZ").ConfigureAwait(false);
        
        // Assert
        data.Should().NotBeNull();
        data.ApplicationId.Should().Be(new Guid("2bb2005f-6e8d-41eb-967b-cde67546cafc"));
        data.StatusId.Should().Be(ApplicationChecklistEntryStatusId.TO_DO);
    }

    [Fact]
    public async Task GetSubmittedIdAndClearinghouseChecklistStatusByBpn_WithNotExistingApplicationId_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);
        
        // Act
        var result = await sut.GetSubmittedIdAndClearinghouseChecklistStatusByBpn("notexisting").ConfigureAwait(false);
        
        // Assert
        result.Should().Be(default);
    }
    
    #endregion
    
    #region GetCompanyIdForSubmittedApplicationId
    
    [Fact]
    public async Task GetCompanyIdForSubmittedApplicationId_WithValidApplicationId_ReturnsCorrectData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);
        
        // Act
        var data = await sut.GetCompanyIdForSubmittedApplicationId(SubmittedApplicationWithBpn).ConfigureAwait(false);
        
        // Assert
        data.Should().NotBeEmpty();
        data.Should().Be(CompanyId);
    }

    [Fact]
    public async Task GetCompanyIdForSubmittedApplicationId_WithNotExistingApplicationId_ReturnsDefault()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);
        
        // Act
        var data = await sut.GetCompanyIdForSubmittedApplicationId(Guid.NewGuid()).ConfigureAwait(false);
        
        // Assert
        data.Should().Be(Guid.Empty);
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
        data.companyId.Should().Be(CompanyId);
        data.businessPartnerNumber.Should().NotBeNullOrEmpty().And.Be("CAXSTESTYCATENAZZ");
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
        data.BusinessPartnerNumber.Should().NotBeNullOrEmpty().And.Be("CAXSTESTYCATENAZZ");
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
    
    private async Task<ApplicationRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new ApplicationRepository(context);
        return sut;
    }
}
