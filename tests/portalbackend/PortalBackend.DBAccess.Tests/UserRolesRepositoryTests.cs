/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class UserRolesRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private static readonly Guid ApplicationWithBpn = new("6b2d1263-c073-4a48-bfaf-704dc154ca9e");
    private const string ClientId = "technical_roles_management";
    private readonly Guid _validCompanyId = new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");
    private readonly IFixture _fixture;
    private readonly TestDbFixture _dbTestDbFixture;

    public UserRolesRepositoryTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetCoreOfferRolesAsync

    [Fact]
    public async Task GetCoreOfferRolesAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetCoreOfferRolesAsync(_validCompanyId, "en", ClientId).ToListAsync();

        // Assert
        data.Should().HaveCount(19);
    }

    #endregion

    #region GetUserWithUserRolesForApplicationId

    [Fact]
    public async Task GetUserWithUserRolesForApplicationId_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetUsersWithUserRolesForApplicationId(ApplicationWithBpn, Enumerable.Repeat("Cl1-CX-Registration", 1)).ToListAsync();

        // Assert
        data.Should().HaveCount(2);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract - null could happen if the database doesn't have the include
        data.Should().AllSatisfy(((Guid, IEnumerable<(string, Guid, string)> UserRoleIds) userData) => userData.UserRoleIds.Should().ContainSingle().And.Satisfy(x => x.Item1 != null));
    }

    #endregion

    #region GetRolesForClient

    [Fact]
    public async Task GetRolesForClient_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetRolesForClient("Cl1-CX-Registration").ToListAsync();

        // Assert
        data.Should().HaveCount(3);
    }

    #endregion

    #region GetServiceAccountRolesAsync

    [Fact]
    public async Task GetServiceAccountRolesAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetServiceAccountRolesAsync(_validCompanyId, ClientId, Constants.DefaultLanguage).ToListAsync();

        // Assert
        data.Should().HaveCount(19);
        data.Should().OnlyHaveUniqueItems();
    }

    #endregion

    #region GetUserRoleDataUntrackedAsync

    [Fact]
    public async Task GetUserRoleDataUntrackedAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var userRoleConfig = new[]{
            new UserRoleConfig("Cl1-CX-Registration", new []
            {
                "Company Admin"
            })};
        var sut = await CreateSut();

        // Act
        var data = await sut.GetUserRoleDataUntrackedAsync(userRoleConfig).ToListAsync();

        // Assert
        data.Should().HaveCount(1);
        var clientData = data.Single();
        clientData.ClientClientId.Should().Be("Cl1-CX-Registration");
        clientData.UserRoleText.Should().Be("Company Admin");
    }

    [Fact]
    public async Task GetUserRoleDataUntrackedAsync_WithNotMatchingClient_ReturnsEmpty()
    {
        // Arrange
        var userRoleConfig = new[]{
            new UserRoleConfig("not-existing-client",
            [
                "Company Admin"
            ])};
        var sut = await CreateSut();

        // Act
        var data = await sut.GetUserRoleDataUntrackedAsync(userRoleConfig).ToListAsync();

        // Assert
        data.Should().BeEmpty();
    }

    #endregion

    #region GetUserRoleIdsUntrackedAsync

    [Fact]
    public async Task GetUserRoleIdsUntrackedAsync()
    {
        // Arrange
        var userRoleConfig = new[]{
            new UserRoleConfig("Cl1-CX-Registration", new []
            {
                "Company Admin"
            })};
        var sut = await CreateSut();

        // Act
        var data = await sut.GetUserRoleIdsUntrackedAsync(userRoleConfig).ToListAsync();

        // Assert
        data.Should().ContainSingle().Which.Should().Be(new Guid("7410693c-c893-409e-852f-9ee886ce94a6"));
    }
    #endregion

    #region GetActiveOfferRoles

    [Fact]
    public async Task GetActiveOfferRolesAsync_NonExistingApp_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetActiveOfferRolesAsync(new Guid("deadbeef-dead-beef-dead-beefdeadbeef"), OfferTypeId.APP, "de", Constants.DefaultLanguage);

        // Assert
        data.IsValid.Should().BeFalse();
        data.IsActive.Should().BeFalse();
        data.AppRoleDetails.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveOfferRolesAsync_InActiveApp_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetActiveOfferRolesAsync(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA7"), OfferTypeId.APP, "de", Constants.DefaultLanguage);

        // Assert
        data.IsValid.Should().BeTrue();
        data.IsActive.Should().BeFalse();
        data.AppRoleDetails.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveOfferRolesAsync_ActiveApp_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetActiveOfferRolesAsync(new Guid("ac1cf001-7fbc-1f2f-817f-bce05744000b"), OfferTypeId.APP, "de", Constants.DefaultLanguage);

        // Assert
        data.IsValid.Should().BeTrue();
        data.IsActive.Should().BeTrue();
        data.AppRoleDetails.Should().HaveCount(2)
            .And.Satisfy(
                x => x.RoleId == new Guid("efc20368-9e82-46ff-b88f-6495b9810253") && x.Role == "EarthCommerce.AdministratorRC_QAS2" && x.Descriptions.Count() == 2 && x.Descriptions.Any(x => x.LanguageCode == "de") && x.Descriptions.Any(x => x.LanguageCode == "en"),
                x => x.RoleId == new Guid("aabcdfeb-6669-4c74-89f0-19cda090873f") && x.Role == "EarthCommerce.Advanced.BuyerRC_QAS2" && x.Descriptions.Count() == 2 && x.Descriptions.Any(x => x.LanguageCode == "de") && x.Descriptions.Any(x => x.LanguageCode == "en"));
    }

    #endregion

    #region GetOfferProviderRoles

    [Fact]
    public async Task GetOfferProviderRolesAsync_NonExistingApp_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetOfferProviderRolesAsync(new Guid("deadbeef-dead-beef-dead-beefdeadbeef"), OfferTypeId.APP, _validCompanyId, "de", Constants.DefaultLanguage);

        // Assert
        data.IsValid.Should().BeFalse();
        data.IsProvider.Should().BeFalse();
        data.AppRoleDetails.Should().BeNull();
    }

    [Fact]
    public async Task GetOfferProviderRolesAsync_NotProviderCompany_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetOfferProviderRolesAsync(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA7"), OfferTypeId.APP, _fixture.Create<Guid>(), "de", Constants.DefaultLanguage);

        // Assert
        data.IsValid.Should().BeTrue();
        data.IsProvider.Should().BeFalse();
        data.AppRoleDetails.Should().BeNull();
    }

    [Fact]
    public async Task GetOfferProviderRolesAsync_ValidAppAndProvider_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetOfferProviderRolesAsync(new Guid("ac1cf001-7fbc-1f2f-817f-bce05744000b"), OfferTypeId.APP, new Guid("0dcd8209-85e2-4073-b130-ac094fb47106"), "de", Constants.DefaultLanguage);

        // Assert
        data.IsValid.Should().BeTrue();
        data.IsProvider.Should().BeTrue();
        data.AppRoleDetails.Should().HaveCount(2)
            .And.Satisfy(
                x => x.RoleId == new Guid("efc20368-9e82-46ff-b88f-6495b9810253") && x.Role == "EarthCommerce.AdministratorRC_QAS2" && x.Descriptions.Count() == 2 && x.Descriptions.Any(x => x.LanguageCode == "de") && x.Descriptions.Any(x => x.LanguageCode == "en"),
                x => x.RoleId == new Guid("aabcdfeb-6669-4c74-89f0-19cda090873f") && x.Role == "EarthCommerce.Advanced.BuyerRC_QAS2" && x.Descriptions.Count() == 2 && x.Descriptions.Any(x => x.LanguageCode == "de") && x.Descriptions.Any(x => x.LanguageCode == "en"));
    }

    #endregion

    private async Task<IUserRolesRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        return new UserRolesRepository(context);
    }
}
