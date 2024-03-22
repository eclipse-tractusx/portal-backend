/********************************************************************************
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

using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.Controllers.Tests;

public class AppChangeControllerTest
{
    private readonly IIdentityData _identity;
    private readonly IFixture _fixture;
    private readonly AppChangeController _controller;
    private readonly IAppChangeBusinessLogic _logic;

    public AppChangeControllerTest()
    {
        _fixture = new Fixture();
        _logic = A.Fake<IAppChangeBusinessLogic>();
        _identity = A.Fake<IIdentityData>();
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        _controller = new AppChangeController(_logic);
        _controller.AddControllerContextWithClaim(_identity);
    }

    [Fact]
    public async Task AddActiveAppUserRole_ReturnsExpectedCount()
    {
        var appId = _fixture.Create<Guid>();
        var appUserRoles = _fixture.CreateMany<AppUserRole>(3);
        var appRoleData = _fixture.CreateMany<AppRoleData>(3);
        A.CallTo(() => _logic.AddActiveAppUserRoleAsync(appId, appUserRoles))
            .Returns(appRoleData);

        //Act
        var result = await _controller.AddActiveAppUserRole(appId, appUserRoles);
        foreach (var item in result)
        {
            //Assert
            A.CallTo(() => _logic.AddActiveAppUserRoleAsync(appId, appUserRoles)).MustHaveHappenedOnceExactly();
            Assert.NotNull(item);
            Assert.IsType<AppRoleData>(item);
        }
    }

    [Fact]
    public async Task GetAppUpdateDescriptionsAsync_ReturnsExpected()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var offerDescriptionData = _fixture.CreateMany<LocalizedDescription>(3);

        A.CallTo(() => _logic.GetAppUpdateDescriptionByIdAsync(A<Guid>._))
            .Returns(offerDescriptionData);

        //Act
        var result = await _controller.GetAppUpdateDescriptionsAsync(appId);

        //Assert
        A.CallTo(() => _logic.GetAppUpdateDescriptionByIdAsync(A<Guid>._)).MustHaveHappened();
        result.Should().NotBeNull()
            .And.ContainInOrder(offerDescriptionData);
    }

    [Fact]
    public async Task CreateOrUpdateAppDescriptionsAsync_ReturnsExpected()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var offerDescriptionData = _fixture.CreateMany<LocalizedDescription>(3);

        //Act
        var result = await _controller.CreateOrUpdateAppDescriptionsByIdAsync(appId, offerDescriptionData);

        //Assert
        A.CallTo(() => _logic.CreateOrUpdateAppDescriptionByIdAsync(A<Guid>._, A<IEnumerable<LocalizedDescription>>._)).MustHaveHappened();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UploadOfferAssignedAppLeadImageDocumentByIdAsync_ReturnsExpected()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("Test Image", "TestImage.jpeg", "image/jpeg");
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _controller.UploadOfferAssignedAppLeadImageDocumentByIdAsync(appId, file, cancellationToken);

        // Assert
        A.CallTo(() => _logic.UploadOfferAssignedAppLeadImageDocumentByIdAsync(appId, file, cancellationToken)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeactivateApp_ReturnsNoContent()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();

        //Act
        var result = await _controller.DeactivateApp(appId);

        //Assert
        A.CallTo(() => _logic.DeactivateOfferByAppIdAsync(appId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateTenantUrl_ReturnsExpected()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var subscriptionId = _fixture.Create<Guid>();
        var data = new UpdateTenantData("http://test.com");

        //Act
        var result = await _controller.UpdateTenantUrl(appId, subscriptionId, data);

        //Assert
        A.CallTo(() => _logic.UpdateTenantUrlAsync(appId, subscriptionId, data)).MustHaveHappened();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetActiveAppDocuments_ReturnsExpected()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();

        //Act
        await _controller.GetActiveAppDocuments(appId);

        //Assert
        A.CallTo(() => _logic.GetActiveAppDocumentTypeDataAsync(appId)).MustHaveHappened();
    }

    [Fact]
    public async Task DeleteMulitipleActiveAppDocumentsAsync_ReturnsExpected()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();

        // Act
        await _controller.DeleteMulitipleActiveAppDocumentsAsync(appId, documentId);

        // Assert
        A.CallTo(() => _logic.DeleteActiveAppDocumentAsync(appId, documentId)).MustHaveHappened();
    }

    [Fact]
    public async Task CreateActiveAppDocumentAsync_ReturnsExpected()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("Test Image1", "TestImage1.jpeg", "image/jpeg");
        var documentTypeId = DocumentTypeId.APP_IMAGE;

        // Act
        await _controller.CreateActiveAppDocumentAsync(appId, documentTypeId, file, CancellationToken.None);

        // Assert
        A.CallTo(() => _logic.CreateActiveAppDocumentAsync(appId, documentTypeId, file, CancellationToken.None)).MustHaveHappened();
    }
}
