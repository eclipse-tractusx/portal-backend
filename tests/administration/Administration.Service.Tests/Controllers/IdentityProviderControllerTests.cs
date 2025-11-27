/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class IdentityProviderControllerTests
{
    private readonly IIdentityProviderBusinessLogic _logic;
    private readonly IdentityProviderController _controller;
    private readonly Fixture _fixture;

    public IdentityProviderControllerTests()
    {
        _fixture = new Fixture();
        _logic = A.Fake<IIdentityProviderBusinessLogic>();
        _controller = new IdentityProviderController(_logic);
    }

    [Fact]
    public async Task UpdateOwnCompanyIdentityProvider_WithValidData_ReturnsOk()
    {
        //Arrange
        var id = Guid.NewGuid();
        var data = _fixture.Create<IdentityProviderDetails>();
        A.CallTo(() => _logic.UpdateOwnCompanyIdentityProviderAsync(A<Guid>._, A<IdentityProviderEditableDetails>._, A<CancellationToken>._))
            .Returns(data);
        var cancellationToken = CancellationToken.None;

        //Act
        var result = await _controller.UpdateOwnCompanyIdentityProvider(id, new IdentityProviderEditableDetails("test"), cancellationToken);

        //Assert
        result.Should().Be(data);
        A.CallTo(() => _logic.UpdateOwnCompanyIdentityProviderAsync(id, A<IdentityProviderEditableDetails>.That.Matches(x => x.DisplayName == "test"), cancellationToken)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteOwnCompanyIdentityProvider_WithValidData_ReturnsOk()
    {
        //Arrange
        var id = Guid.NewGuid();

        //Act
        await _controller.DeleteOwnCompanyIdentityProvider(id);

        //Assert
        A.CallTo(() => _logic.DeleteCompanyIdentityProviderAsync(id)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetOwnIdentityProviderWithConnectedCompanies_WithValidData_ReturnsOk()
    {
        //Arrange
        var id = Guid.NewGuid();

        //Act
        await _controller.GetOwnIdentityProviderWithConnectedCompanies(id);

        //Assert
        A.CallTo(() => _logic.GetOwnIdentityProviderWithConnectedCompanies(id)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetOwnCompanyIdentityProviderDetails_WithValidData_ReturnsOk()
    {
        //Arrange
        var displayName = _fixture.Create<string>();
        var alias = _fixture.Create<string>();
        var data = _fixture.CreateMany<IdentityProviderDetails>(3);
        A.CallTo(() => _logic.GetOwnCompanyIdentityProvidersAsync(displayName, alias))
            .Returns(data.ToAsyncEnumerable());

        //Act
        var result = await _controller.GetOwnCompanyIdentityProviderDetails(displayName, alias);

        //Assert
        A.CallTo(() => _logic.GetOwnCompanyIdentityProvidersAsync(displayName, alias)).MustHaveHappenedOnceExactly();
        result.Should().HaveSameCount(data).And.ContainInOrder(data);
    }

    [Fact]
    public async Task CreateOwnCompanyIdentityProvider_WithValidData_ReturnsOk()
    {
        //Arrange
        var iamIdentityProvider = _fixture.Create<IamIdentityProviderProtocol>();
        var identityProviderType = _fixture.Create<IdentityProviderTypeId>();
        var displayName = _fixture.Create<string>();

        //Act
        await _controller.CreateOwnCompanyIdentityProvider(iamIdentityProvider, identityProviderType, displayName);

        //Assert
        A.CallTo(() => _logic.CreateOwnCompanyIdentityProviderAsync(iamIdentityProvider, identityProviderType, displayName)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SetOwnCompanyIdentityProviderStatus_WithValidData_ReturnsOk()
    {
        //Arrange
        var identityProviderId = Guid.NewGuid();
        var enabled = _fixture.Create<bool>();

        //Act
        await _controller.SetOwnCompanyIdentityProviderStatus(identityProviderId, enabled);

        //Assert
        A.CallTo(() => _logic.SetOwnCompanyIdentityProviderStatusAsync(identityProviderId, enabled)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetOwnCompanyUsersIdentityProviderDataAsync_WithValidData_ReturnsOk()
    {
        //Arrange
        var identityProviderIds = _fixture.Create<IEnumerable<Guid>>();
        var unlinkedUsersOnly = _fixture.Create<bool>();

        //Act
        _controller.GetOwnCompanyUsersIdentityProviderDataAsync(identityProviderIds, unlinkedUsersOnly);

        //Assert
        A.CallTo(() => _logic.GetOwnCompanyUsersIdentityProviderDataAsync(identityProviderIds, unlinkedUsersOnly)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetOwnCompanyUsersIdentityProviderFileAsync_WithValidData_ReturnsOk()
    {
        //Arrange
        var identityProviderIds = _fixture.Create<IEnumerable<Guid>>();
        var unlinkedUsersOnly = _fixture.Create<bool>();
        const string fileName = "test.pdf";
        const string contentType = "application/pdf";
        var encoding = Encoding.UTF8;
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", "application/pdf");
        var stream = file.OpenReadStream();
        A.CallTo(() => _logic.GetOwnCompanyUsersIdentityProviderLinkDataStream(identityProviderIds, unlinkedUsersOnly))
            .Returns((stream, contentType, fileName, encoding));

        //Act
        _controller.GetOwnCompanyUsersIdentityProviderFileAsync(identityProviderIds, unlinkedUsersOnly);

        //Assert
        A.CallTo(() => _logic.GetOwnCompanyUsersIdentityProviderLinkDataStream(identityProviderIds, unlinkedUsersOnly)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UploadOwnCompanyUsersIdentityProviderFileAsync_WithValidData_ReturnsOk()
    {
        //Arrange
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", "application/pdf");

        //Act
        await _controller.UploadOwnCompanyUsersIdentityProviderFileAsync(file, CancellationToken.None);

        //Assert
        A.CallTo(() => _logic.UploadOwnCompanyUsersIdentityProviderLinkDataAsync(file, CancellationToken.None)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateOrUpdateOwnCompanyUserIdentityProviderDataAsync_WithValidData_ReturnsOk()
    {
        //Arrange
        var companyUserId = Guid.NewGuid();
        var identityProviderId = Guid.NewGuid();
        var userLinkData = _fixture.Create<UserLinkData>();

        //Act
        await _controller.CreateOrUpdateOwnCompanyUserIdentityProviderDataAsync(companyUserId, identityProviderId, userLinkData);

        //Assert
        A.CallTo(() => _logic.CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId, userLinkData)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteOwnCompanyUserIdentityProviderDataAsync_WithValidData_ReturnsOk()
    {
        //Arrange
        var companyUserId = Guid.NewGuid();
        var identityProviderId = Guid.NewGuid();

        //Act
        await _controller.DeleteOwnCompanyUserIdentityProviderDataAsync(companyUserId, identityProviderId);

        //Assert
        A.CallTo(() => _logic.DeleteOwnCompanyUserIdentityProviderDataAsync(companyUserId, identityProviderId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetOwnCompanyIdentityProvider_WithValidData_ReturnsOk()
    {
        //Arrange
        var identityProviderId = Guid.NewGuid();

        //Act
        await _controller.GetOwnCompanyIdentityProvider(identityProviderId);

        //Assert
        A.CallTo(() => _logic.GetOwnCompanyIdentityProviderAsync(identityProviderId)).MustHaveHappenedOnceExactly();
    }
}
