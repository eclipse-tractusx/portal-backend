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
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyAppUsersPaginationSourceAsync(
            _validOfferId,
            _validCompanyUser,
            new[] { OfferSubscriptionStatusId.ACTIVE },
            new[] { UserStatusId.ACTIVE, UserStatusId.INACTIVE },
            new CompanyUserFilter(null, null, null, null, null))(0, 15).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetOwnCompanyAppUsersPaginationSourceAsync_Inactive_WithValidIamUser_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyAppUsersPaginationSourceAsync(
            _validOfferId,
            _validCompanyUser,
            new[] { OfferSubscriptionStatusId.ACTIVE },
            new[] { UserStatusId.INACTIVE },
            new CompanyUserFilter(null, null, null, null, null))(0, 15).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOwnCompanyAppUsersPaginationSourceAsync_Active_Inactive_Deleted_WithValidIamUser_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyAppUsersPaginationSourceAsync(
            _validOfferId,
            _validCompanyUser,
            new[] { OfferSubscriptionStatusId.ACTIVE },
            new[] { UserStatusId.ACTIVE, UserStatusId.INACTIVE, UserStatusId.DELETED },
            new CompanyUserFilter(null, null, null, null, null))(0, 15).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetOwnCompanyAppUsersPaginationSourceAsync_WithNotExistingIamUser_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyAppUsersPaginationSourceAsync(
            _validOfferId,
            Guid.NewGuid(),
            new[] { OfferSubscriptionStatusId.ACTIVE },
            new[] { UserStatusId.ACTIVE, UserStatusId.INACTIVE },
            new CompanyUserFilter(null, null, null, null, null))(0, 15).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetServiceProviderCompanyUserWithRoleId

    [Fact]
    public async Task GetServiceProviderCompanyUserWithRoleIdAsync_WithValidData_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetServiceProviderCompanyUserWithRoleIdAsync(
            new Guid("9b957704-3505-4445-822c-d7ef80f27fcd"), new List<Guid>
            {
                new ("58f897ec-0aad-4588-8ffa-5f45d6638632") // CX Admin
            }).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetServiceProviderCompanyUserWithRoleIdAsync_WithNotExistingRole_ReturnsEmptyList()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetServiceProviderCompanyUserWithRoleIdAsync(
            new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new List<Guid>
            {
                Guid.NewGuid()
            }).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetCompanyUserWithRoleIdForCompany

    [Fact]
    public async Task GetCompanyUserWithRoleIdForCompany_WithExistingUserForRole_ReturnsExpectedUserId()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut
            .GetCompanyUserWithRoleIdForCompany(new[] { new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632") }, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"))
            .ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(1)
            .And.Contain(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020006"));
    }

    [Fact]
    public async Task GetCompanyUserWithRoleIdForCompany_WithExistingUserForRole_WithoutCompanyId_ReturnsExpectedUserId()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut
            .GetCompanyUserWithRoleId(new[] { new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632") })
            .ToListAsync().ConfigureAwait(false);

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
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut
            .GetCompanyUserEmailForCompanyAndRoleId(new[] { new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632") }, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"))
            .ToListAsync().ConfigureAwait(false);

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
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut
            .GetCompanyBpnForIamUserAsync(_validCompanyUser)
            .ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be("BPNL00000003CRHK");
    }

    #endregion

    #region GetAppAssignedIamClientUserDataUntrackedAsync

    [Theory]
    [InlineData("a16e73b9-5277-4b69-9f8d-3b227495dfea", "78d664de-04a0-41c6-9a47-478d303403d2", ValidUserCompanyId, true, true, "f3e2bcd8-1b42-4a62-ab09-2d86e40d0f85", true, "SDE with EDC", "User", "Active")]
    [InlineData("deadbeef-dead-beef-dead-beefdeadbeef", "78d664de-04a0-41c6-9a47-478d303403d2", ValidUserCompanyId, true, false, "f3e2bcd8-1b42-4a62-ab09-2d86e40d0f85", true, null, "User", "Active")]
    [InlineData("a16e73b9-5277-4b69-9f8d-3b227495dfea", "78d664de-04a0-41c6-9a47-478d303403d2", "00000000-0000-0000-0000-000000000000", true, true, "f3e2bcd8-1b42-4a62-ab09-2d86e40d0f85", false, "SDE with EDC", "User", "Active")]
    [InlineData("a16e73b9-5277-4b69-9f8d-3b227495dfea", "deadbeef-dead-beef-dead-beefdeadbeef", ValidUserCompanyId, false, false, null, false, null, null, null)]
    public async Task GetAppAssignedIamClientUserDataUntrackedAsync_ReturnsExpected(Guid offerId, Guid companyUserId, Guid userCompanyId, bool found, bool validOffer, string resultIamUserId, bool sameCompany, string? offerName, string? firstName, string? lastName)
    {
        var sut = await CreateSut().ConfigureAwait(false);

        var iamUserData = await sut.GetAppAssignedIamClientUserDataUntrackedAsync(offerId, companyUserId, userCompanyId)
            .ConfigureAwait(false);

        if (found)
        {
            iamUserData.Should().NotBeNull();
            iamUserData!.IsValidOffer.Should().Be(validOffer);
            iamUserData.IamUserId.Should().Be(resultIamUserId);
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
    [InlineData("9b957704-3505-4445-822c-d7ef80f27fcd", "78d664de-04a0-41c6-9a47-478d303403d2", ValidUserCompanyId, true, true, "f3e2bcd8-1b42-4a62-ab09-2d86e40d0f85", true, "User", "Active")]
    [InlineData("deadbeef-dead-beef-dead-beefdeadbeef", "78d664de-04a0-41c6-9a47-478d303403d2", ValidUserCompanyId, true, false, "f3e2bcd8-1b42-4a62-ab09-2d86e40d0f85", true, "User", "Active")]
    [InlineData("9b957704-3505-4445-822c-d7ef80f27fcd", "78d664de-04a0-41c6-9a47-478d303403d2", "00000000-0000-0000-0000-000000000000", true, true, "f3e2bcd8-1b42-4a62-ab09-2d86e40d0f85", false, "User", "Active")]
    [InlineData("9b957704-3505-4445-822c-d7ef80f27fcd", "deadbeef-dead-beef-dead-beefdeadbeef", ValidUserCompanyId, false, false, null, false, null, null)]
    public async Task GetCoreOfferAssignedIamClientUserDataUntrackedAsync_ReturnsExpected(Guid offerId, Guid companyUserId, Guid userCompanyId, bool found, bool validOffer, string resultIamUserId, bool sameCompany, string? firstName, string? lastName)
    {
        var sut = await CreateSut().ConfigureAwait(false);

        var iamUserData = await sut.GetCoreOfferAssignedIamClientUserDataUntrackedAsync(offerId, companyUserId, userCompanyId)
            .ConfigureAwait(false);

        if (found)
        {
            iamUserData.Should().NotBeNull();
            iamUserData!.IsValidOffer.Should().Be(validOffer);
            iamUserData.IamUserId.Should().Be(resultIamUserId);
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
        var sut = await CreateSut().ConfigureAwait(false);
        var userRoleIds = new[]{
            new Guid("efc20368-9e82-46ff-b88f-6495b9810255"),
            new Guid("efc20368-9e82-46ff-b88f-6495b9810254")};

        // Act
        var result = await sut.GetUserDetailsUntrackedAsync(new("ac1cf001-7fbc-1f2f-817f-bce058019992"), userRoleIds).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.CompanyName.Should().Be("Security Company");
        result.CompanyUserId.Should().Be("ac1cf001-7fbc-1f2f-817f-bce058019992");
        result.Email.Should().Be("julia.jeroch@bmw.de");
        result.FirstName.Should().Be("Test User");
        result.BusinessPartnerNumbers.Should().BeEmpty();
        result.AdminDetails.Should().NotBeEmpty()
            .And.HaveCount(2)
            .And.Satisfy(
                x => x.CompanyUserId == new Guid("ac1cf001-7fbc-1f2f-817f-bce058019992") && x.Email == "julia.jeroch@bmw.de",
                x => x.CompanyUserId == new Guid("ac1cf001-7fbc-1f2f-817f-bce058019993") && x.Email == "julia.jeroch@bmw.de");
    }

    #endregion

    #region  GetAllFavouriteAppsForUserUntrackedAsync

    [Fact]
    public async Task GetAllFavouriteAppsForUserUntrackedAsync_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetAllFavouriteAppsForUserUntrackedAsync(new("ac1cf001-7fbc-1f2f-817f-bce058020006")).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull().And.NotBeEmpty()
            .And.HaveCount(2)
            .And.Satisfy(
            x => x == new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007"),
            x => x == new Guid("ac1cf001-7fbc-1f2f-817f-bce05744000b"));
    }

    #endregion

    private async Task<UserRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new UserRepository(context);
        return sut;
    }
}
