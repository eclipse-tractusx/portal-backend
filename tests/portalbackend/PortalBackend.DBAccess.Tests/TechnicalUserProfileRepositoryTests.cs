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
using System.Collections.Immutable;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="ServiceAccountRepository"/>
/// </summary>
public class TechnicalUserProfileRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly IFixture _fixture;
    private readonly Guid _validCompanyId = new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");
    private readonly Guid _validServiceId = new("ac1cf001-7fbc-1f2f-817f-bce0000c0001");
    private readonly Guid _validAppId = new("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4");

    public TechnicalUserProfileRepositoryTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetOfferProfileData

    [Fact]
    public async Task GetOfferProfileData_Service_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetOfferProfileData(_validServiceId, OfferTypeId.SERVICE, _validCompanyId);

        // Assert
        result.Should().NotBeNull();
        result!.IsProvidingCompanyUser.Should().BeTrue();
        result.ProfileData.Should().HaveCount(2);
        result.ServiceTypeIds.Should().NotBeNull().And.HaveCount(2);
    }

    [Fact]
    public async Task GetOfferProfileData_App_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetOfferProfileData(_validAppId, OfferTypeId.APP, _validCompanyId);

        // Assert
        result.Should().NotBeNull();
        result!.IsProvidingCompanyUser.Should().BeTrue();
        result.ProfileData.Should().BeEmpty();
        result.ServiceTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task GetOfferProfileData_WithUnknownUser_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetOfferProfileData(_validServiceId, OfferTypeId.SERVICE, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.IsProvidingCompanyUser.Should().BeFalse();
        result.ProfileData.Should().HaveCount(2);
        result.ServiceTypeIds.Should().NotBeNull().And.HaveCount(2);
    }

    [Fact]
    public async Task GetOfferProfileData_IncorrectOfferId_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetOfferProfileData(_validServiceId, OfferTypeId.APP, _validCompanyId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOfferProfileData_WithoutExistingProfile_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetOfferProfileData(Guid.NewGuid(), OfferTypeId.SERVICE, _validCompanyId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateTechnicalUserProfiles

    [Fact]
    public async Task CreateTechnicalUserProfiles_ReturnsExpectedResult()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var (sut, context) = await CreateSutWithContext();

        // Act
        var result = sut.CreateTechnicalUserProfile(profileId, _validServiceId);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        result.OfferId.Should().Be(_validServiceId);
        result.Id.Should().Be(profileId);
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<TechnicalUserProfile>().Which.Id.Should().Be(profileId);
    }

    #endregion

    #region CreateDeleteTechnicalUserProfileAssignedRoles

    [Fact]
    public async Task CreateDeleteTechnicalUserProfileAssignedRoles_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();

        var profileRoleIds = _fixture.CreateMany<(Guid ProfileId, Guid RoleId)>(10).ToImmutableArray();

        // Act
        sut.CreateDeleteTechnicalUserProfileAssignedRoles(profileRoleIds.Take(7), profileRoleIds.Skip(3));

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().HaveCount(6);
        var addedEntities = changedEntries.Where(x => x.State == EntityState.Added).Select(x => x.Entity);
        var removedEntities = changedEntries.Where(x => x.State == EntityState.Deleted).Select(x => x.Entity);
        addedEntities.Should().HaveCount(3).And.AllBeOfType<TechnicalUserProfileAssignedUserRole>();
        addedEntities.Cast<TechnicalUserProfileAssignedUserRole>().Should().Satisfy(
            x => x.TechnicalUserProfileId == profileRoleIds[7].ProfileId && x.UserRoleId == profileRoleIds[7].RoleId,
            x => x.TechnicalUserProfileId == profileRoleIds[8].ProfileId && x.UserRoleId == profileRoleIds[8].RoleId,
            x => x.TechnicalUserProfileId == profileRoleIds[9].ProfileId && x.UserRoleId == profileRoleIds[9].RoleId
        );
        removedEntities.Should().HaveCount(3).And.AllBeOfType<TechnicalUserProfileAssignedUserRole>();
        removedEntities.Cast<TechnicalUserProfileAssignedUserRole>().Should().Satisfy(
            x => x.TechnicalUserProfileId == profileRoleIds[0].ProfileId && x.UserRoleId == profileRoleIds[0].RoleId,
            x => x.TechnicalUserProfileId == profileRoleIds[1].ProfileId && x.UserRoleId == profileRoleIds[1].RoleId,
            x => x.TechnicalUserProfileId == profileRoleIds[2].ProfileId && x.UserRoleId == profileRoleIds[2].RoleId
        );
    }

    #endregion

    #region RemoveTechnicalUserProfiles

    [Fact]
    public async Task RemoveTechnicalUserProfiles_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();

        var profileIds = _fixture.CreateMany<Guid>(3).ToImmutableArray();

        // Act
        sut.RemoveTechnicalUserProfiles(profileIds);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().HaveCount(3);
        var removedEntities = changedEntries.Where(x => x.State == EntityState.Deleted).Select(x => x.Entity);
        removedEntities.Should().HaveCount(3).And.AllBeOfType<TechnicalUserProfile>();
        removedEntities.Cast<TechnicalUserProfile>().Should().Satisfy(
            x => x.Id == profileIds[0],
            x => x.Id == profileIds[1],
            x => x.Id == profileIds[2]
        );
    }

    #endregion

    #region RemoveTechnicalUserProfilesForOffer

    [Fact]
    public async Task RemoveTechnicalUserProfilesForOffer_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();

        // Act
        sut.RemoveTechnicalUserProfilesForOffer(_validServiceId);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(5);
        var removedEntries = changedEntries.Where(x => x.State == EntityState.Deleted);
        removedEntries.Should().HaveCount(5);
        removedEntries.Where(x => x.Entity.GetType() == typeof(TechnicalUserProfile)).Should().HaveCount(2);
        removedEntries.Where(x => x.Entity.GetType() == typeof(TechnicalUserProfileAssignedUserRole)).Should().HaveCount(3);
    }

    #endregion

    #region GetOfferProfileData

    [Fact]
    public async Task GetTechnicalUserProfileInformation_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetTechnicalUserProfileInformation(_validServiceId, _validCompanyId, OfferTypeId.SERVICE);

        // Assert
        result.Should().NotBeNull();
        result.IsUserOfProvidingCompany.Should().BeTrue();
        result.Information.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTechnicalUserProfileInformation_WithUnknownUser_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetTechnicalUserProfileInformation(_validServiceId, Guid.NewGuid(), OfferTypeId.SERVICE);

        // Assert
        result.Should().NotBeNull();
        result.IsUserOfProvidingCompany.Should().BeFalse();
    }

    [Fact]
    public async Task GetTechnicalUserProfileInformation_WithoutExistingProfile_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetTechnicalUserProfileInformation(Guid.NewGuid(), _validCompanyId, OfferTypeId.SERVICE);

        // Assert
        result.Should().Be(default);
    }

    [Fact]
    public async Task GetTechnicalUserProfileInformation_WithWrongType_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetTechnicalUserProfileInformation(_validServiceId, _validCompanyId, OfferTypeId.APP);

        // Assert
        result.Should().Be(default);
    }

    #endregion

    #region Setup

    private async Task<(TechnicalUserProfileRepository, PortalDbContext)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new TechnicalUserProfileRepository(context);
        return (sut, context);
    }

    private async Task<TechnicalUserProfileRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new TechnicalUserProfileRepository(context);
        return sut;
    }

    #endregion
}
