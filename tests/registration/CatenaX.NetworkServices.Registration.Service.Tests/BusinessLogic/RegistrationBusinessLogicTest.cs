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
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Registration.Service.BPN;
using CatenaX.NetworkServices.Registration.Service.BusinessLogic;
using CatenaX.NetworkServices.Registration.Service.Model;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CatenaX.NetworkServices.Registration.Service.Tests;

public class RegistrationBusinessLogicTest
{
    private readonly IFixture _fixture;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IInvitationRepository _invitationRepository;
    private readonly RegistrationBusinessLogic _logic;

    public RegistrationBusinessLogicTest()
    {
        _fixture = new Fixture();
        var mailingService = A.Fake<IMailingService>();
        var bpnAccess = A.Fake<IBPNAccess>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        var logger = A.Fake<ILogger<RegistrationBusinessLogic>>();
        var portalRepositories = A.Fake<IPortalRepositories>();
        _invitationRepository = A.Fake<IInvitationRepository>();
        var settings = A.Fake<IOptions<RegistrationSettings>>();

        A.CallTo(() => portalRepositories.GetInstance<IInvitationRepository>())
            .Returns(_invitationRepository);
        this._logic = new RegistrationBusinessLogic(settings, mailingService, bpnAccess, _provisioningManager, logger, portalRepositories);
    }

    [Fact]
    public async Task Get_WhenThereAreInvitedUser_ShouldReturnInvitedUserWithRoles()
    {
        //Arrange
        Guid id = new Guid("7eab8e16-8298-4b41-953b-515745423658");
        var invitedUserRole = _fixture.CreateMany<string>(2).AsEnumerable();
        var invitedUser = _fixture.CreateMany<InvitedUserDetail>(1).ToAsyncEnumerable<InvitedUserDetail>();
        A.CallTo(() => _invitationRepository.GetInvitedUserDetailsUntrackedAsync(id))
            .Returns(invitedUser);
        A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(A<string>._, A<string>._))
            .Returns(invitedUserRole);
        //Act
        var result = this._logic.GetInvitedUsersAsync(id);
        await foreach (var item in result)
        {
            //Assert
            A.CallTo(() => _invitationRepository.GetInvitedUserDetailsUntrackedAsync(id)).MustHaveHappened(1, Times.OrMore);
            A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(A<string>._, A<string>._)).MustHaveHappened(1, Times.OrMore);
            Assert.NotNull(item);
            Assert.IsType<InvitedUser>(item);
        }
    }

    [Fact]
    public async Task GetInvitedUsersDetail_ThrowException_WhenIdIsNull()
    {
        //Arrange
        Guid id = new Guid("7eab8e16-8298-4b41-953b-515745423658");
        var invitedUserRole = _fixture.CreateMany<string>(2).AsEnumerable();
        var invitedUser = _fixture.CreateMany<InvitedUserDetail>(1).ToAsyncEnumerable<InvitedUserDetail>();
        A.CallTo(() => _invitationRepository.GetInvitedUserDetailsUntrackedAsync(id))
            .Returns(invitedUser);
        A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(A<string>._, A<string>._))
            .Returns(invitedUserRole);
        //Act
        var result = this._logic.GetInvitedUsersAsync(Guid.Empty);
        await foreach (var item in result)
        {
            //Assert
            A.CallTo(() => _invitationRepository.GetInvitedUserDetailsUntrackedAsync(Guid.Empty)).Throws(new Exception());
            A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(string.Empty, string.Empty)).Throws(new Exception());
        }
    }
}
