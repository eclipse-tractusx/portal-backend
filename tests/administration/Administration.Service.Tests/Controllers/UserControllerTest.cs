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

using Microsoft.AspNetCore.Http;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class UserControllerTest
{
    private readonly IIdentityData _identity;
    private readonly IUserBusinessLogic _logic;
    private readonly IUserUploadBusinessLogic _uploadLogic;
    private readonly IUserRolesBusinessLogic _rolesLogic;
    private readonly UserController _controller;
    private readonly Fixture _fixture;

    public UserControllerTest()
    {
        _fixture = new Fixture();
        _identity = A.Fake<IIdentityData>();
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        _logic = A.Fake<IUserBusinessLogic>();
        _uploadLogic = A.Fake<IUserUploadBusinessLogic>();
        _rolesLogic = A.Fake<IUserRolesBusinessLogic>();
        _controller = new UserController(_logic, _uploadLogic, _rolesLogic);
        _controller.AddControllerContextWithClaim(_identity);
        _controller.AddControllerContextWithClaimAndBearer("ac-token", _identity);
    }

    [Fact]
    public async Task GetOwnUserDetails_ReturnsExpectedResult()
    {
        // Arrange
        var data = _fixture.Create<CompanyOwnUserDetails>();
        A.CallTo(() => _logic.GetOwnUserDetails())
            .Returns(data);

        // Act
        var result = await _controller.GetOwnUserDetails();

        // Assert
        A.CallTo(() => _logic.GetOwnUserDetails()).MustHaveHappenedOnceExactly();
        result.Should().Be(data);
    }

    [Fact]
    public async Task AddOwnCompanyUserBusinessPartnerNumbers_ReturnsExpectedCalls()
    {
        //Arrange
        var bpns = _fixture.CreateMany<string>();
        var data = _fixture.Create<CompanyUsersBpnDetails>();
        A.CallTo(() => _logic.AddOwnCompanyUsersBusinessPartnerNumbersAsync(A<Guid>._, A<string>._, A<IEnumerable<string>>._, CancellationToken.None))
            .Returns(data);

        // Act
        var result = await this._controller.AddOwnCompanyUserBusinessPartnerNumbers(_identity.IdentityId, bpns, CancellationToken.None);

        // Assert
        A.CallTo(() => _logic.AddOwnCompanyUsersBusinessPartnerNumbersAsync(A<Guid>._, A<string>._, A<IEnumerable<string>>._, CancellationToken.None)).MustHaveHappenedOnceExactly();
        result.Should().Be(data);
    }

    [Fact]
    public async Task CreateOwnCompanyUsers_ReturnsExpectedCalls()
    {
        //Arrange
        var usersToCreate = _fixture.CreateMany<UserCreationInfo>(3);

        // Act
        _controller.CreateOwnCompanyUsers(usersToCreate);

        // Assert
        A.CallTo(() => _logic.CreateOwnCompanyUsersAsync(usersToCreate)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UploadOwnCompanySharedIdpUsersFileAsync_ReturnsExpectedCalls()
    {
        //Arrange
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", "application/pdf");

        // Act
        await _controller.UploadOwnCompanySharedIdpUsersFileAsync(file, CancellationToken.None);

        // Assert
        A.CallTo(() => _uploadLogic.UploadOwnCompanySharedIdpUsersAsync(file, CancellationToken.None)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateOwnIdpOwnCompanyUser_ReturnsExpectedCalls()
    {
        //Arrange
        var userToCreate = _fixture.Create<UserCreationInfoIdp>();
        var identityProviderId = Guid.NewGuid();

        // Act
        await _controller.CreateOwnIdpOwnCompanyUser(userToCreate, identityProviderId);

        // Assert
        A.CallTo(() => _logic.CreateOwnCompanyIdpUserAsync(identityProviderId, userToCreate)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UploadOwnCompanyUsersIdentityProviderFileAsync_ReturnsExpectedCalls()
    {
        //Arrange
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", "application/pdf");
        var identityProviderId = Guid.NewGuid();

        // Act
        await _controller.UploadOwnCompanyUsersIdentityProviderFileAsync(identityProviderId, file, CancellationToken.None);

        // Assert
        A.CallTo(() => _uploadLogic.UploadOwnCompanyIdpUsersAsync(identityProviderId, file, CancellationToken.None)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetOwnCompanyUserDatasAsync_WithValidRequest_ReturnsExpected()
    {
        //Arrange
        var paginationResponse = new Pagination.Response<CompanyUserData>(new Pagination.Metadata(15, 1, 1, 15), _fixture.CreateMany<CompanyUserData>(5));
        A.CallTo(() => _logic.GetOwnCompanyUserDatasAsync(0, 15, new(null, null, null, null)))
            .Returns(paginationResponse);

        // Act
        var result = await _controller.GetOwnCompanyUserDatasAsync();

        // Assert
        A.CallTo(() => _logic.GetOwnCompanyUserDatasAsync(0, 15, new(null, null, null, null))).MustHaveHappenedOnceExactly();
        result.Content.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetOwnCompanyUserDetails_WithValidRequest_ReturnsExpected()
    {
        //Arrange
        var companyUserId = Guid.NewGuid();

        // Act
        await _controller.GetOwnCompanyUserDetails(companyUserId);

        // Assert
        A.CallTo(() => _logic.GetOwnCompanyUserDetailsAsync(companyUserId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ModifyCoreUserRolesAsync_WithValidRequest_ReturnsExpected()
    {
        //Arrange
        var companyUserId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var roles = _fixture.CreateMany<string>(3);

        // Act
        await _controller.ModifyCoreUserRolesAsync(companyUserId, offerId, roles);

        // Assert
        A.CallTo(() => _rolesLogic.ModifyCoreOfferUserRolesAsync(offerId, companyUserId, roles)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ModifyAppUserRolesAsync_WithValidRequest_ReturnsExpected()
    {
        //Arrange
        var companyUserId = Guid.NewGuid();
        var appId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var roles = _fixture.CreateMany<string>(3);

        // Act
        await _controller.ModifyAppUserRolesAsync(companyUserId, appId, subscriptionId, roles);

        // Assert
        A.CallTo(() => _rolesLogic.ModifyAppUserRolesAsync(appId, companyUserId, subscriptionId, roles)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteOwnCompanyUsers_WithValidRequest_ReturnsExpected()
    {
        //Arrange
        var usersToDelete = _fixture.CreateMany<Guid>(3);

        // Act
        _controller.DeleteOwnCompanyUsers(usersToDelete);

        // Assert
        A.CallTo(() => _logic.DeleteOwnCompanyUsersAsync(usersToDelete)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ResetOwnCompanyUserPassword_WithValidRequest_ReturnsExpected()
    {
        //Arrange
        var companyUserId = Guid.NewGuid();

        // Act
        await _controller.ResetOwnCompanyUserPassword(companyUserId);

        // Assert
        A.CallTo(() => _logic.ExecuteOwnCompanyUserPasswordReset(companyUserId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCoreOfferRoles_WithValidRequest_ReturnsExpected()
    {
        // Act
        _controller.GetCoreOfferRoles();

        // Assert
        A.CallTo(() => _rolesLogic.GetCoreOfferRoles(A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetAppRolesAsync_WithValidRequest_ReturnsExpected()
    {
        // Arrange
        var appId = Guid.NewGuid();

        // Act
        _controller.GetAppRolesAsync(appId);

        // Assert
        A.CallTo(() => _rolesLogic.GetAppRolesAsync(A<Guid>._, A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateOwnUserDetails_WithValidRequest_ReturnsExpected()
    {
        // Arrange
        var companyUserId = Guid.NewGuid();
        var ownCompanyUserEditableDetails = _fixture.Create<OwnCompanyUserEditableDetails>();

        // Act
        await _controller.UpdateOwnUserDetails(companyUserId, ownCompanyUserEditableDetails);

        // Assert
        A.CallTo(() => _logic.UpdateOwnUserDetails(companyUserId, ownCompanyUserEditableDetails)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteOwnUser_WithValidRequest_ReturnsExpected()
    {
        // Arrange
        var companyUserId = Guid.NewGuid();

        // Act
        await _controller.DeleteOwnUser(companyUserId);

        // Assert
        A.CallTo(() => _logic.DeleteOwnUserAsync(companyUserId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompanyAppUsersAsync_WithValidRequest_ReturnsExpected()
    {
        //Arrange
        var paginationResponse = new Pagination.Response<CompanyAppUserDetails>(new Pagination.Metadata(15, 1, 1, 15), _fixture.CreateMany<CompanyAppUserDetails>(5));
        var appId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        A.CallTo(() => _logic.GetOwnCompanyAppUsersAsync(appId, subscriptionId, 0, 15, new(null, null, null, null, null)))
            .Returns(paginationResponse);

        // Act
        var result = await _controller.GetCompanyAppUsersAsync(appId, subscriptionId, 0, 15);

        // Assert
        A.CallTo(() => _logic.GetOwnCompanyAppUsersAsync(appId, subscriptionId, 0, 15, new(null, null, null, null, null))).MustHaveHappenedOnceExactly();
        result.Content.Should().HaveCount(5);
    }

    [Fact]
    public async Task DeleteOwnCompanyUserBusinessPartnerNumber_WithValidRequest_ReturnsExpected()
    {
        // Arrange
        var companyUserId = Guid.NewGuid();
        var bpn = _fixture.Create<string>();

        // Act
        await _controller.DeleteOwnCompanyUserBusinessPartnerNumber(companyUserId, bpn);

        // Assert
        A.CallTo(() => _logic.DeleteOwnUserBusinessPartnerNumbersAsync(companyUserId, bpn)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AddOwnCompanyUserBusinessPartnerNumber_ReturnsExpectedCalls()
    {
        //Arrange
        var bpn = _fixture.Create<string>();
        var data = _fixture.Create<CompanyUsersBpnDetails>();
        A.CallTo(() => _logic.AddOwnCompanyUsersBusinessPartnerNumberAsync(A<Guid>._, A<string>._, A<string>._, CancellationToken.None))
            .Returns(data);

        // Act
        var result = await this._controller.AddOwnCompanyUserBusinessPartnerNumber(_identity.IdentityId, bpn, CancellationToken.None);

        // Assert
        A.CallTo(() => _logic.AddOwnCompanyUsersBusinessPartnerNumberAsync(A<Guid>._, A<string>._, A<string>._, CancellationToken.None)).MustHaveHappenedOnceExactly();
        result.Should().Be(data);
    }
}
