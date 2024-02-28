/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
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
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Text;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests;

public class RegistrationControllerTest
{
    private readonly IIdentityData _identity;
    private readonly IFixture _fixture;
    private readonly RegistrationController _controller;
    private readonly IRegistrationBusinessLogic _registrationBusinessLogicFake;

    public RegistrationControllerTest()
    {
        _fixture = new Fixture();
        _identity = A.Fake<IIdentityData>();
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        _registrationBusinessLogicFake = A.Fake<IRegistrationBusinessLogic>();
        _controller = new RegistrationController(_registrationBusinessLogicFake);
        _controller.AddControllerContextWithClaimAndBearer("ac-token", _identity);
    }

    [Fact]
    public async Task Get_WhenThereAreInvitedUsers_ShouldReturnActionResultOfInvitedUsersWith200StatusCode()
    {
        //Arrange
        var id = new Guid("7eab8e16-8298-4b41-953b-515745423658");
        var invitedUserMapper = _fixture.CreateMany<InvitedUser>(3);
        A.CallTo(() => _registrationBusinessLogicFake.GetInvitedUsersAsync(id))
            .Returns(invitedUserMapper.ToAsyncEnumerable());

        //Act
        var result = await _controller.GetInvitedUsersAsync(id).ToListAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetInvitedUsersAsync(id)).MustHaveHappenedOnceExactly();
        result.Should().HaveSameCount(invitedUserMapper).And.ContainInOrder(invitedUserMapper);
    }

    [Fact]
    public async Task GetUploadedDocumentsAsync_ReturnsExpectedResult()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();

        var uploadDocuments = _fixture.CreateMany<UploadDocuments>(3);
        A.CallTo(() => _registrationBusinessLogicFake.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT))
            .Returns(uploadDocuments);

        //Act
        var result = await _controller.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT)).MustHaveHappenedOnceExactly();

        result.Should().HaveSameCount(uploadDocuments).And.ContainInOrder(uploadDocuments);
    }

    [Fact]
    public async Task SubmitCompanyRoleConsentToAgreementsAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var data = _fixture.Create<CompanyRoleAgreementConsents>();
        A.CallTo(() => _registrationBusinessLogicFake.SubmitRoleConsentAsync(applicationId, data))
            .Returns(1);

        //Act
        var result = await _controller.SubmitCompanyRoleConsentToAgreementsAsync(applicationId, data).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _registrationBusinessLogicFake.SubmitRoleConsentAsync(applicationId, data)).MustHaveHappenedOnceExactly();
        result.Should().Be(1);
    }

    [Fact]
    public async Task GetCompanyIdentifiers_WithValidData_ReturnsExpected()
    {
        // Arrange
        var data = _fixture.CreateMany<UniqueIdentifierData>(1);
        A.CallTo(() => _registrationBusinessLogicFake.GetCompanyIdentifiers("DE"))
            .Returns(data);

        //Act
        var result = await _controller.GetCompanyIdentifiers("DE").ConfigureAwait(false);

        // Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetCompanyIdentifiers("DE")).MustHaveHappenedOnceExactly();
        result.Should().HaveSameCount(data).And.ContainInOrder(data);
    }

    [Fact]
    public async Task GetDocumentContentFileAsync_WithValidData_ReturnsOk()
    {
        //Arrange
        const string fileName = "test.pdf";
        const string contentType = "application/pdf";
        var id = Guid.NewGuid();
        var content = Encoding.UTF8.GetBytes("This is just test content");
        A.CallTo(() => _registrationBusinessLogicFake.GetDocumentContentAsync(id))
            .Returns((fileName, content, contentType));

        //Act
        var result = await _controller.GetDocumentContentFileAsync(id).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetDocumentContentAsync(id)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<FileContentResult>();
        ((FileContentResult)result).ContentType.Should().Be(contentType);
    }

    [Fact]
    public async Task GetRegistrationDocumentAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var documentId = _fixture.Create<Guid>();
        var content = new byte[7];
        A.CallTo(() => _registrationBusinessLogicFake.GetRegistrationDocumentAsync(documentId))
            .Returns(new ValueTuple<string, byte[], string>("test.json", content, "application/json"));

        //Act
        var result = await _controller.GetRegistrationDocumentAsync(documentId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetRegistrationDocumentAsync(documentId)).MustHaveHappenedOnceExactly();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task InviteNewUserAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var creationInfo = _fixture.Create<UserCreationInfoWithMessage>();

        A.CallTo(() => _registrationBusinessLogicFake.InviteNewUserAsync(A<Guid>._, A<UserCreationInfoWithMessage>._))
            .Returns(1);

        //Act
        var result = await _controller.InviteNewUserAsync(applicationId, creationInfo).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _registrationBusinessLogicFake.InviteNewUserAsync(applicationId, creationInfo)).MustHaveHappenedOnceExactly();
        result.Should().Be(1);
    }

    [Fact]
    public async Task GetApplicationsWithStatusAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var companyApplicationWithStatus = _fixture.CreateMany<CompanyApplicationWithStatus>(2);
        A.CallTo(() => _registrationBusinessLogicFake.GetAllApplicationsForUserWithStatus())
            .Returns(companyApplicationWithStatus.ToAsyncEnumerable());

        //Act
        var result = await _controller.GetApplicationsWithStatusAsync().ToListAsync().ConfigureAwait(false);

        // Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetAllApplicationsForUserWithStatus()).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(2);
    }
}
