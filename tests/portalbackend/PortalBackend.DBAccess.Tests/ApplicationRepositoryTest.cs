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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using FluentAssertions;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class ApplicationRepositoryTest : IAssemblyFixture<TestDbFixture>
{

    private static readonly Guid ApplicationWithBpn = new("4829b64c-de6a-426c-81fc-c0bcf95bcb76");
    private static readonly Guid ApplicationWithoutBpn = new("1b86d973-3aac-4dcd-a9e9-0c222766202b");
    private readonly IFixture _fixture;
    private readonly TestDbFixture _dbTestDbFixture;

    public ApplicationRepositoryTest(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetCompanyUserRoleWithAdressUntrackedAsync
    
    [Fact]
    public async Task GetCompanyUserRoleWithAdressUntrackedAsync_WithExistingEntry_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut
            .GetCompanyUserRoleWithAdressUntrackedAsync(new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76"))
            .ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
        results!.AgreementsData.Should().HaveCount(1);
        results.AgreementsData.First().ConsentStatusId.Should().Be(ConsentStatusId.ACTIVE);
        results.InvitedCompanyUserData.Should().HaveCount(2);
        results.InvitedCompanyUserData.Should().ContainSingle(u => u.FirstName == "Test User 1" && u.LastName == "cx-user-2" && u.Email == "tester.user1@test.de");
        results.InvitedCompanyUserData.Should().ContainSingle(u => u.FirstName == "Test User 2" && u.LastName == "cx-admin-2" && u.Email == "tester.user2@test.de");
    }

    #endregion GetRegistrationDataUntrackedAsync

    #region 
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

    #region GetBpnForApplicationId
    
    [Fact]
    public async Task GetBpnForApplicationIdAsync_WithValidApplicationId_ReturnsBpn()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);
        
        // Act
        var (bpn, alreadyExists) = await sut.GetBpnAndChecklistCheckForApplicationIdAsync(ApplicationWithBpn).ConfigureAwait(false);
        
        // Assert
        bpn.Should().Be("CAXSDUMMYCATENAZZ");
        alreadyExists.Should().BeFalse();
    }

    [Fact]
    public async Task GetBpnForApplicationIdAsync_WithNotExistingApplicationId_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);
        
        // Act
        var result = await sut.GetBpnAndChecklistCheckForApplicationIdAsync(Guid.NewGuid()).ConfigureAwait(false);
        
        // Assert
        result.Should().Be(default);
    }

    [Fact]
    public async Task GetBpnForApplicationIdAsync_WithApplicationIdWithoutBpn_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);
        
        // Act
        var (bpn, alreadyExists) = await sut.GetBpnAndChecklistCheckForApplicationIdAsync(ApplicationWithoutBpn).ConfigureAwait(false);
        
        // Assert
        bpn.Should().BeNull();
        alreadyExists.Should().BeTrue();
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
        checklistEntryStatus.Should().Be(default);
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
    
    private async Task<ApplicationRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new ApplicationRepository(context);
        return sut;
    }
}
