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

using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
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
        var result = await _controller.GetInvitedUsersAsync(id).ToListAsync();

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetInvitedUsersAsync(id)).MustHaveHappenedOnceExactly();
        result.Should().HaveSameCount(invitedUserMapper).And.ContainInOrder(invitedUserMapper);
    }

    [Fact]
    public async Task GetCompanyBpdmDetailDataAsync_ReturnsExpectedResult()
    {
        //Arrange
        var bpn = "THISBPNISVALID12";
        var token = "ac-token";
        var companyBpdmDetailData = _fixture.Create<CompanyBpdmDetailData>();
        A.CallTo(() => _registrationBusinessLogicFake.GetCompanyBpdmDetailDataByBusinessPartnerNumber(bpn, token, CancellationToken.None))
            .Returns(companyBpdmDetailData);

        //Act
        var result = await _controller.GetCompanyBpdmDetailDataAsync(bpn, CancellationToken.None);

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetCompanyBpdmDetailDataByBusinessPartnerNumber(bpn, token, CancellationToken.None)).MustHaveHappenedOnceExactly();
        result.Should().NotBeNull();
        result.BusinessPartnerNumber.Should().Be(companyBpdmDetailData.BusinessPartnerNumber);
    }

    [Fact]
    public async Task UploadDocumentAsync_ReturnsExpectedResult()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        var documentTypeId = _fixture.Create<DocumentTypeId>();
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", "application/pdf");

        var companyBpdmDetailData = _fixture.Create<CompanyBpdmDetailData>();
        A.CallTo(() => _registrationBusinessLogicFake.UploadDocumentAsync(applicationId, file, documentTypeId, CancellationToken.None));

        //Act
        var result = await _controller.UploadDocumentAsync(applicationId, documentTypeId, file, CancellationToken.None);

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.UploadDocumentAsync(applicationId, file, documentTypeId, CancellationToken.None)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetClientRolesComposite_ReturnsExpectedResult()
    {
        //Arrange
        var roles = _fixture.CreateMany<string>(3);
        A.CallTo(() => _registrationBusinessLogicFake.GetClientRolesCompositeAsync())
            .Returns(roles.ToAsyncEnumerable());

        //Act
        var result = _controller.GetClientRolesComposite().ToEnumerable();

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetClientRolesCompositeAsync()).MustHaveHappenedOnceExactly();
        result.Should().HaveSameCount(roles).And.ContainInOrder(roles);
    }

    [Fact]
    public async Task GetApplicationsDeclineData_ReturnsExpectedResult()
    {
        //Arrange
        var companyApplicationDeclineData = _fixture.CreateMany<CompanyApplicationDeclineData>(3);
        A.CallTo(() => _registrationBusinessLogicFake.GetApplicationsDeclineData())
            .Returns(companyApplicationDeclineData);

        //Act
        var result = await _controller.GetApplicationsDeclineData();

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetApplicationsDeclineData()).MustHaveHappenedOnceExactly();
        result.Should().HaveSameCount(companyApplicationDeclineData).And.ContainInOrder(companyApplicationDeclineData);
    }

    [Fact]
    public async Task SetApplicationStatusAsync_ReturnsExpectedResult()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        var companyApplicationStatusId = _fixture.Create<CompanyApplicationStatusId>();
        A.CallTo(() => _registrationBusinessLogicFake.SetOwnCompanyApplicationStatusAsync(applicationId, companyApplicationStatusId))
            .Returns(0);

        //Act
        var result = await _controller.SetApplicationStatusAsync(applicationId, companyApplicationStatusId);

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.SetOwnCompanyApplicationStatusAsync(applicationId, companyApplicationStatusId)).MustHaveHappenedOnceExactly();
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetApplicationStatusAsync_ReturnsExpectedResult()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        var companyApplicationDeclineData = _fixture.Create<CompanyApplicationStatusId>();
        A.CallTo(() => _registrationBusinessLogicFake.GetOwnCompanyApplicationStatusAsync(applicationId))
            .Returns(companyApplicationDeclineData);

        //Act
        var result = await _controller.GetApplicationStatusAsync(applicationId);

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetOwnCompanyApplicationStatusAsync(applicationId)).MustHaveHappenedOnceExactly();
        result.Should().Be(companyApplicationDeclineData);
    }

    [Fact]
    public async Task GetCompanyDetailDataAsync_ReturnsExpectedResult()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        var companyDetailData = _fixture.Create<CompanyDetailData>();
        A.CallTo(() => _registrationBusinessLogicFake.GetCompanyDetailData(applicationId))
            .Returns(companyDetailData);

        //Act
        var result = await _controller.GetCompanyDetailDataAsync(applicationId);

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetCompanyDetailData(applicationId)).MustHaveHappenedOnceExactly();
        result.Should().Be(companyDetailData);
    }

    [Fact]
    public async Task SetCompanyDetailDataAsync_ReturnsExpectedResult()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        var companyDetailData = _fixture.Create<CompanyDetailData>();

        //Act
        await _controller.SetCompanyDetailDataAsync(applicationId, companyDetailData);

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.SetCompanyDetailDataAsync(applicationId, companyDetailData)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetAgreementConsentStatusesAsync_ReturnsExpectedResult()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        var companyRoleAgreementConsents = _fixture.Create<CompanyRoleAgreementConsents>();
        A.CallTo(() => _registrationBusinessLogicFake.GetRoleAgreementConsentsAsync(applicationId))
            .Returns(companyRoleAgreementConsents);

        //Act
        var result = await _controller.GetAgreementConsentStatusesAsync(applicationId);

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetRoleAgreementConsentsAsync(applicationId)).MustHaveHappenedOnceExactly();
        result.Should().Be(companyRoleAgreementConsents);
    }

    [Fact]
    public async Task GetCompanyRoleAgreementDataAsync_ReturnsExpectedResult()
    {
        //Arrange
        var companyRoleAgreementData = _fixture.Create<CompanyRoleAgreementData>();
        A.CallTo(() => _registrationBusinessLogicFake.GetCompanyRoleAgreementDataAsync())
            .Returns(companyRoleAgreementData);

        //Act
        var result = await _controller.GetCompanyRoleAgreementDataAsync();

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetCompanyRoleAgreementDataAsync()).MustHaveHappenedOnceExactly();
        result.Should().Be(companyRoleAgreementData);
    }

    [Fact]
    public async Task SubmitRegistrationAsync_ReturnsExpectedResult()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        A.CallTo(() => _registrationBusinessLogicFake.SubmitRegistrationAsync(applicationId))
            .Returns(true);

        //Act
        var result = await _controller.SubmitRegistrationAsync(applicationId);

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.SubmitRegistrationAsync(applicationId)).MustHaveHappenedOnceExactly();
        result.Should().Be(true);
    }

    [Fact]
    public async Task SetInvitationStatusAsync_ReturnsExpectedResult()
    {
        //Arrange
        A.CallTo(() => _registrationBusinessLogicFake.SetInvitationStatusAsync())
            .Returns(1);

        //Act
        var result = await _controller.SetInvitationStatusAsync();

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.SetInvitationStatusAsync()).MustHaveHappenedOnceExactly();
        result.Should().Be(1);
    }

    [Fact]
    public async Task GetRegistrationDataAsync_ReturnsExpectedResult()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        var companyRegistrationData = _fixture.Create<CompanyRegistrationData>();
        A.CallTo(() => _registrationBusinessLogicFake.GetRegistrationDataAsync(applicationId))
            .Returns(companyRegistrationData);

        //Act
        var result = await _controller.GetRegistrationDataAsync(applicationId);

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetRegistrationDataAsync(applicationId)).MustHaveHappenedOnceExactly();
        result.Should().Be(companyRegistrationData);
    }

    [Fact]
    public async Task GetCompanyRolesAsync_ReturnsExpectedResult()
    {
        //Arrange
        var languageShortName = "en";
        var companyRolesDetails = _fixture.CreateMany<CompanyRolesDetails>(3);
        A.CallTo(() => _registrationBusinessLogicFake.GetCompanyRoles(languageShortName))
            .Returns(companyRolesDetails.ToAsyncEnumerable());

        //Act
        var result = _controller.GetCompanyRolesAsync(languageShortName).ToEnumerable();

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetCompanyRoles(languageShortName)).MustHaveHappenedOnceExactly();
        result.Should().HaveSameCount(companyRolesDetails).And.ContainInOrder(companyRolesDetails);
    }

    [Fact]
    public async Task DeleteRegistrationDocument_ReturnsExpectedResult()
    {
        //Arrange
        var documentId = Guid.NewGuid();

        //Act
        var result = await _controller.DeleteRegistrationDocument(documentId);

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.DeleteRegistrationDocumentAsync(documentId)).MustHaveHappenedOnceExactly();
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
        var result = await _controller.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT);

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
        var result = await _controller.SubmitCompanyRoleConsentToAgreementsAsync(applicationId, data);

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
        var result = await _controller.GetCompanyIdentifiers("DE");

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
        var result = await _controller.GetDocumentContentFileAsync(id);

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
        var result = await _controller.GetRegistrationDocumentAsync(documentId);

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
        var result = await _controller.InviteNewUserAsync(applicationId, creationInfo);

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
        var result = await _controller.GetApplicationsWithStatusAsync().ToListAsync();

        // Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetAllApplicationsForUserWithStatus()).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeclineApplicationRegistrationAsync_ReturnsExpectedCalls()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();

        // Act
        var result = await _controller.DeclineApplicationRegistrationAsync(applicationId);

        // Assert
        A.CallTo(() => _registrationBusinessLogicFake.DeclineApplicationRegistrationAsync(applicationId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }
}
