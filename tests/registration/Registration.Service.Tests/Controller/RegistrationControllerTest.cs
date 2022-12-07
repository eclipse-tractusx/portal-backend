/********************************************************************************
 * Copyright (c) 2021,2022 Microsoft and BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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
using Org.CatenaX.Ng.Portal.Backend.Registration.Service.BusinessLogic;
using Org.CatenaX.Ng.Portal.Backend.Registration.Service.Controllers;
using Org.CatenaX.Ng.Portal.Backend.Registration.Service.Model;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared.Extensions;
using Xunit;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;


namespace Org.CatenaX.Ng.Portal.Backend.Registration.Service.Tests;

public class RegistrationControllerTest
{
    private readonly IFixture _fixture;
    private readonly RegistrationController _controller;
    private readonly IRegistrationBusinessLogic _registrationBusinessLogicFake;
    private readonly string _iamUserId = "7478542d-7878-47a8-a931-08bd8779532d";
    private readonly string _accessToken = "ac-token";

    public RegistrationControllerTest()
    {
        _fixture = new Fixture();
        _registrationBusinessLogicFake = A.Fake<IRegistrationBusinessLogic>();
        ILogger<RegistrationController> registrationLoggerFake = A.Fake<ILogger<RegistrationController>>();
        _controller = new RegistrationController(registrationLoggerFake, _registrationBusinessLogicFake);
        _controller.AddControllerContextWithClaimAndBearer(_iamUserId, _accessToken);
    }

    [Fact]
    public async Task Get_WhenThereAreInvitedUsers_ShouldReturnActionResultOfInvitedUsersWith200StatusCode()
    {
        //Arrange
        Guid id = new Guid("7eab8e16-8298-4b41-953b-515745423658");
        var invitedUserMapper = _fixture.CreateMany<InvitedUser>(3).ToAsyncEnumerable();
        A.CallTo(() => _registrationBusinessLogicFake.GetInvitedUsersAsync(id))
            .Returns(invitedUserMapper);

        //Act
        var result = this._controller.GetInvitedUsersAsync(id);
        await foreach (var item in result)
        {
            //Assert
            A.CallTo(() => _registrationBusinessLogicFake.GetInvitedUsersAsync(id)).MustHaveHappenedOnceExactly();
            Assert.NotNull(item);
            Assert.IsType<InvitedUser>(item);
        }
    }

    [Fact]
    public async Task GetInvitedUsersDetail_WhenIdisNull_ShouldThrowException()
    {
        //Arrange
        Guid id = new Guid("7eab8e16-8298-4b41-953b-515745423658");
        var invitedUserMapper = _fixture.CreateMany<InvitedUser>(3).ToAsyncEnumerable();
        A.CallTo(() => _registrationBusinessLogicFake.GetInvitedUsersAsync(id))
            .Returns(invitedUserMapper);

        //Act
        var result = this._controller.GetInvitedUsersAsync(Guid.Empty);

        //Assert
        await foreach (var item in result)
        {
            A.CallTo(() => _registrationBusinessLogicFake.GetInvitedUsersAsync(Guid.Empty)).Throws(new Exception());
        }
    }

    [Fact]
    public async Task GetUploadedDocumentsAsync_ReturnsExpectedResult()
    {
        //Arrange
        Guid applicationId = new Guid("7eab8e16-8298-4b41-953b-515745423658");
        var uploadDocuments = _fixture.CreateMany<UploadDocuments>(3).ToAsyncEnumerable();
        A.CallTo(() => _registrationBusinessLogicFake.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT))
            .Returns(uploadDocuments);

        //Act
        var result = this._controller.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT);

        //Assert
        await foreach (var item in result)
        {
            A.CallTo(() => _registrationBusinessLogicFake.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT)).MustHaveHappenedOnceExactly();
            Assert.NotNull(item);
            Assert.IsType<UploadDocuments>(item);
        }
    }
    [Fact]
    public async Task SubmitCompanyRoleConsentToAgreementsAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var data = _fixture.Create<CompanyRoleAgreementConsents>();
        A.CallTo(() => _registrationBusinessLogicFake.SubmitRoleConsentAsync(applicationId, data, _iamUserId))
            .ReturnsLazily(() => 1);

        //Act
        var result = await this._controller.SubmitCompanyRoleConsentToAgreementsAsync(applicationId, data).ConfigureAwait(false);
        
        // Assert
        A.CallTo(() => _registrationBusinessLogicFake.SubmitRoleConsentAsync(applicationId, data, _iamUserId)).MustHaveHappenedOnceExactly();
        result.Should().Be(1);
    }
}
