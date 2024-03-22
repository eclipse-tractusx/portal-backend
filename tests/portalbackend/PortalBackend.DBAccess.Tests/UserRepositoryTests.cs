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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="UserRepository"/>
/// </summary>
public class UserRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private const string ValidCompanyUserTxt = "ac1cf001-7fbc-1f2f-817f-bce058020006";
    private const string ValidUserCompanyId = "2dc4249f-b5ca-4d42-bef1-7a7a950a4f87";
    private readonly Guid _validCompanyUser = new(ValidCompanyUserTxt);
    private readonly Guid _validOfferId = new("ac1cf001-7fbc-1f2f-817f-bce0572c0007");

    public UserRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region Get own company app users

    [Fact]
    public async Task GetOwnCompanyAppUsersPaginationSourceAsync_WithValidIamUser_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanyAppUsersPaginationSourceAsync(
            _validOfferId,
            _validCompanyUser,
            new[] { OfferSubscriptionStatusId.ACTIVE },
            new[] { UserStatusId.ACTIVE, UserStatusId.INACTIVE },
            new CompanyUserFilter(null, null, null, null, null))(0, 15);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetOwnCompanyAppUsersPaginationSourceAsync_Inactive_WithValidIamUser_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanyAppUsersPaginationSourceAsync(
            _validOfferId,
            _validCompanyUser,
            new[] { OfferSubscriptionStatusId.ACTIVE },
            new[] { UserStatusId.INACTIVE },
            new CompanyUserFilter(null, null, null, null, null))(0, 15);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOwnCompanyAppUsersPaginationSourceAsync_Active_Inactive_Deleted_WithValidIamUser_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanyAppUsersPaginationSourceAsync(
            _validOfferId,
            _validCompanyUser,
            new[] { OfferSubscriptionStatusId.ACTIVE },
            new[] { UserStatusId.ACTIVE, UserStatusId.INACTIVE, UserStatusId.DELETED },
            new CompanyUserFilter(null, null, null, null, null))(0, 15);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetOwnCompanyAppUsersPaginationSourceAsync_WithNotExistingIamUser_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanyAppUsersPaginationSourceAsync(
            _validOfferId,
            Guid.NewGuid(),
            new[] { OfferSubscriptionStatusId.ACTIVE },
            new[] { UserStatusId.ACTIVE, UserStatusId.INACTIVE },
            new CompanyUserFilter(null, null, null, null, null))(0, 15);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetServiceProviderCompanyUserWithRoleId

    [Fact]
    public async Task GetServiceProviderCompanyUserWithRoleIdAsync_WithValidData_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetServiceProviderCompanyUserWithRoleIdAsync(
            new Guid("9b957704-3505-4445-822c-d7ef80f27fcd"), new List<Guid>
            {
                new ("58f897ec-0aad-4588-8ffa-5f45d6638632") // CX Admin
            }).ToListAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetServiceProviderCompanyUserWithRoleIdAsync_WithNotExistingRole_ReturnsEmptyList()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetServiceProviderCompanyUserWithRoleIdAsync(
            new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new List<Guid>
            {
                Guid.NewGuid()
            }).ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetCompanyUserWithRoleIdForCompany

    [Fact]
    public async Task GetCompanyUserWithRoleIdForCompany_WithExistingUserForRole_ReturnsExpectedUserId()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut
            .GetCompanyUserWithRoleIdForCompany(new[] { new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632") }, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"))
            .ToListAsync();

        // Assert
        result.Should().HaveCount(1)
            .And.Contain(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020006"));
    }

    [Fact]
    public async Task GetCompanyUserWithRoleIdForCompany_WithExistingUserForRole_WithoutCompanyId_ReturnsExpectedUserId()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut
            .GetCompanyUserWithRoleId(new[] { new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632") })
            .ToListAsync();

        // Assert
        result.Should().HaveCount(3)
            .And.Contain(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"));
    }

    #endregion

    #region GetCompanyUserWithRoleIdForCompany

    [Fact]
    public async Task GetCompanyUserEmailForCompanyAndRoleId_WithExistingUserForRole_ReturnsExpectedEmail()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut
            .GetCompanyUserEmailForCompanyAndRoleId(new[] { new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632") }, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"))
            .ToListAsync();

        // Assert
        result.Should().HaveCount(1)
            .And.Satisfy(
                x => x.Email == "tobeadded@cx.com"
            );
    }

    #endregion

    #region GetCompanyBpnForIamUserAsync

    [Fact]
    public async Task GetCompanyBpnForIamUserAsync_WithExistingUser_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut
            .GetCompanyBpnForIamUserAsync(_validCompanyUser)
            ;

        // Assert
        result.Should().NotBeNull();
        result.Should().Be("BPNL00000003CRHK");
    }

    #endregion

    #region GetAppAssignedIamClientUserDataUntrackedAsync

    [Theory]
    [InlineData("a16e73b9-5277-4b69-9f8d-3b227495dfea", "78d664de-04a0-41c6-9a47-478d303403d2", ValidUserCompanyId, true, true, true, "SDE with EDC", "User", "Active")]
    [InlineData("deadbeef-dead-beef-dead-beefdeadbeef", "78d664de-04a0-41c6-9a47-478d303403d2", ValidUserCompanyId, true, false, true, null, "User", "Active")]
    [InlineData("a16e73b9-5277-4b69-9f8d-3b227495dfea", "78d664de-04a0-41c6-9a47-478d303403d2", "00000000-0000-0000-0000-000000000000", true, true, false, "SDE with EDC", "User", "Active")]
    [InlineData("a16e73b9-5277-4b69-9f8d-3b227495dfea", "deadbeef-dead-beef-dead-beefdeadbeef", ValidUserCompanyId, false, false, false, null, null, null)]
    public async Task GetAppAssignedIamClientUserDataUntrackedAsync_ReturnsExpected(Guid offerId, Guid companyUserId, Guid userCompanyId, bool found, bool validOffer, bool sameCompany, string? offerName, string? firstName, string? lastName)
    {
        var sut = await CreateSut();

        var iamUserData = await sut.GetAppAssignedIamClientUserDataUntrackedAsync(offerId, companyUserId, userCompanyId)
            ;

        if (found)
        {
            iamUserData.Should().NotBeNull();
            iamUserData!.IsValidOffer.Should().Be(validOffer);
            iamUserData.IsSameCompany.Should().Be(sameCompany);
            iamUserData.OfferName.Should().Be(offerName);
            iamUserData.Firstname.Should().Be(firstName);
            iamUserData.Lastname.Should().Be(lastName);
        }
        else
        {
            iamUserData.Should().BeNull();
        }
    }

    #endregion

    #region GetCoreOfferAssignedIamClientUserDataUntrackedAsync

    [Theory]
    [InlineData("9b957704-3505-4445-822c-d7ef80f27fcd", "78d664de-04a0-41c6-9a47-478d303403d2", ValidUserCompanyId, true, true, true, "User", "Active")]
    [InlineData("deadbeef-dead-beef-dead-beefdeadbeef", "78d664de-04a0-41c6-9a47-478d303403d2", ValidUserCompanyId, true, false, true, "User", "Active")]
    [InlineData("9b957704-3505-4445-822c-d7ef80f27fcd", "78d664de-04a0-41c6-9a47-478d303403d2", "00000000-0000-0000-0000-000000000000", true, true, false, "User", "Active")]
    [InlineData("9b957704-3505-4445-822c-d7ef80f27fcd", "deadbeef-dead-beef-dead-beefdeadbeef", ValidUserCompanyId, false, false, false, null, null)]
    public async Task GetCoreOfferAssignedIamClientUserDataUntrackedAsync_ReturnsExpected(Guid offerId, Guid companyUserId, Guid userCompanyId, bool found, bool validOffer, bool sameCompany, string? firstName, string? lastName)
    {
        var sut = await CreateSut();

        var iamUserData = await sut.GetCoreOfferAssignedIamClientUserDataUntrackedAsync(offerId, companyUserId, userCompanyId)
            ;

        if (found)
        {
            iamUserData.Should().NotBeNull();
            iamUserData!.IsValidOffer.Should().Be(validOffer);
            iamUserData.IsSameCompany.Should().Be(sameCompany);
            iamUserData.Firstname.Should().Be(firstName);
            iamUserData.Lastname.Should().Be(lastName);
        }
        else
        {
            iamUserData.Should().BeNull();
        }
    }

    #endregion

    #region  GetUserDetails

    [Fact]
    public async Task GetUserDetailsUntrackedAsync_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();
        var userRoleIds = new[]{
            new Guid("efc20368-9e82-46ff-b88f-6495b9810255"),
            new Guid("efc20368-9e82-46ff-b88f-6495b9810254")};

        // Act
        var result = await sut.GetUserDetailsUntrackedAsync(new("ac1cf001-7fbc-1f2f-817f-bce058019992"), userRoleIds);

        // Assert
        result.Should().NotBeNull();
        result!.CompanyName.Should().Be("Security Company");
        result.CompanyUserId.Should().Be("ac1cf001-7fbc-1f2f-817f-bce058019992");
        result.Email.Should().Be("company.admin2@acme.corp");
        result.FirstName.Should().Be("Test User");
        result.BusinessPartnerNumbers.Should().BeEmpty();
        result.AdminDetails.Should().NotBeEmpty()
            .And.HaveCount(2)
            .And.Satisfy(
                x => x.CompanyUserId == new Guid("ac1cf001-7fbc-1f2f-817f-bce058019992") && x.Email == "company.admin2@acme.corp",
                x => x.CompanyUserId == new Guid("ac1cf001-7fbc-1f2f-817f-bce058019993") && x.Email == "it.admin2@acme.corp");
    }

    #endregion

    #region  GetAllFavouriteAppsForUserUntrackedAsync

    [Fact]
    public async Task GetAllFavouriteAppsForUserUntrackedAsync_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetAllFavouriteAppsForUserUntrackedAsync(new("ac1cf001-7fbc-1f2f-817f-bce058020006")).ToListAsync();

        // Assert
        result.Should().NotBeNull().And.NotBeEmpty()
            .And.HaveCount(2)
            .And.Satisfy(
            x => x == new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007"),
            x => x == new Guid("ac1cf001-7fbc-1f2f-817f-bce05744000b"));
    }

    #endregion

    #region GetApplicationsWithStatus

    [Fact]
    public async Task GetApplicationsWithStatusUntrackedAsync_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetApplicationsWithStatusUntrackedAsync(new("41fd2ab8-71cd-4546-9bef-a388d91b2542")).ToListAsync();

        // Assert
        result.Should().NotBeNull().And.Satisfy(x =>
            x.ApplicationId == new Guid("6b2d1263-c073-4a48-bfaf-704dc154ca9e") &&
            x.ApplicationStatus == CompanyApplicationStatusId.SUBMITTED &&
            x.ApplicationType == CompanyApplicationTypeId.INTERNAL);
        result.Single().ApplicationChecklist.Should().Satisfy(
            y => y.TypeId == ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION && y.StatusId == ApplicationChecklistEntryStatusId.TO_DO,
            y => y.TypeId == ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER && y.StatusId == ApplicationChecklistEntryStatusId.DONE,
            y => y.TypeId == ApplicationChecklistEntryTypeId.CLEARING_HOUSE && y.StatusId == ApplicationChecklistEntryStatusId.TO_DO,
            y => y.TypeId == ApplicationChecklistEntryTypeId.IDENTITY_WALLET && y.StatusId == ApplicationChecklistEntryStatusId.TO_DO,
            y => y.TypeId == ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION && y.StatusId == ApplicationChecklistEntryStatusId.DONE,
            y => y.TypeId == ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP && y.StatusId == ApplicationChecklistEntryStatusId.TO_DO);
    }

    #endregion

    #region AddCompanyUserAssignedIdentityProvider

    [Fact]
    public async Task AddCompanyUserAssignedIdentityProvider_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();

        // Act
        var results = sut.AddCompanyUserAssignedIdentityProvider(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020006"), new Guid("ac1cf001-7fbc-1f2f-817f-bce057770015"), "123", "testuser");

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        results.UserName.Should().Be("testuser");
        results.ProviderId.Should().Be("123");
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle()
            .Which.Entity.Should().BeOfType<CompanyUserAssignedIdentityProvider>()
            .Which.Should().Match<CompanyUserAssignedIdentityProvider>(x =>
                x.UserName == "testuser" &&
                x.ProviderId == "123"
            );
    }

    #endregion

    #region GetUserAssignedIdentityProviderForNetworkRegistration

    [Fact]
    public async Task GetUserAssignedIdentityProviderForNetworkRegistration_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var results = await sut.GetUserAssignedIdentityProviderForNetworkRegistration(new Guid("67ace0a9-b6df-438b-935a-fe858b8598dd")).ToListAsync();

        // Assert
        results.Should().ContainSingle()
            .Which.Should().Match<CompanyUserIdentityProviderProcessData>(x =>
                x.ProviderLinkData.Single().UserName == "drstrange" &&
                x.Email == "test@email.com" &&
                x.Bpn == "BPNL00000003AYRE");
    }

    #endregion

    private async Task<(UserRepository sut, PortalDbContext context)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        var sut = new UserRepository(context);
        return (sut, context);
    }

    private async Task<UserRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        var sut = new UserRepository(context);
        return sut;
    }
}
