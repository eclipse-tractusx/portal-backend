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
using Microsoft.Extensions.Logging;
using Xunit;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.CatenaX.Ng.Portal.Backend.Registration.Service.Tests;

public class RegistrationControllerTest
{
    private readonly IFixture _fixture;
    private readonly RegistrationController controller;
    private readonly IRegistrationBusinessLogic registrationBusineesLogicFake;
    private readonly ILogger<RegistrationController> registrationLoggerFake;
    public RegistrationControllerTest()
    {
        _fixture = new Fixture();
        registrationBusineesLogicFake = A.Fake<IRegistrationBusinessLogic>();
        registrationLoggerFake = A.Fake<ILogger<RegistrationController>>();
        this.controller = new RegistrationController(registrationLoggerFake, registrationBusineesLogicFake);
    }

    [Fact]
    public async Task Get_WhenThereAreInvitedUsers_ShouldReturnActionResultOfInvitedUsersWith200StatusCode()
    {
        //Arrange
        Guid id = new Guid("7eab8e16-8298-4b41-953b-515745423658");
        var invitedUserMapper = _fixture.CreateMany<InvitedUser>(3).ToAsyncEnumerable();
        A.CallTo(() => registrationBusineesLogicFake.GetInvitedUsersAsync(id))
            .Returns(invitedUserMapper);

        //Act
        var result = this.controller.GetInvitedUsersAsync(id);
        await foreach (var item in result)
        {
            //Assert
            A.CallTo(() => registrationBusineesLogicFake.GetInvitedUsersAsync(id)).MustHaveHappenedOnceExactly();
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
        A.CallTo(() => registrationBusineesLogicFake.GetInvitedUsersAsync(id))
            .Returns(invitedUserMapper);

        //Act
        var result = this.controller.GetInvitedUsersAsync(Guid.Empty);

        //Assert
        await foreach (var item in result)
        {
            A.CallTo(() => registrationBusineesLogicFake.GetInvitedUsersAsync(Guid.Empty)).Throws(new Exception());
        }
    }

    [Fact]
    public async Task GetUploadedDocumentsAsync_ReturnsExpectedResult()
    {
        //Arrange
        Guid applicationId = new Guid("7eab8e16-8298-4b41-953b-515745423658");
        var uploadDocuments = _fixture.CreateMany<UploadDocuments>(3).ToAsyncEnumerable();
        A.CallTo(() => registrationBusineesLogicFake.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT))
            .Returns(uploadDocuments);

        //Act
        var result = this.controller.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT);

        //Assert
        await foreach (var item in result)
        {
            A.CallTo(() => registrationBusineesLogicFake.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT)).MustHaveHappenedOnceExactly();
            Assert.NotNull(item);
            Assert.IsType<UploadDocuments>(item);
        }
    }
}
