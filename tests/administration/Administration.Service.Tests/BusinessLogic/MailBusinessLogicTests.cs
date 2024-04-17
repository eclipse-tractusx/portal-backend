/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class MailBusinessLogicTests
{
    private readonly Guid UserId = Guid.NewGuid();
    private readonly IFixture _fixture;
    private readonly IMailBusinessLogic _sut;
    private readonly IMailingProcessCreation _mailingService;
    private readonly IUserRepository _userRepository;

    public MailBusinessLogicTests()
    {
        _fixture = new Fixture();
        var identity = A.Fake<IIdentityData>();
        A.CallTo(() => identity.IdentityId).Returns(UserId);

        _userRepository = A.Fake<IUserRepository>();
        var portalRepositories = A.Fake<IPortalRepositories>();
        _mailingService = A.Fake<IMailingProcessCreation>();

        A.CallTo(() => portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);

        _sut = new MailBusinessLogic(portalRepositories, _mailingService);
    }

    #region SendMail

    [Fact]
    public async Task SendMail_WithoutExistingUser_ThrowsNotFoundException()
    {
        // Arrange
        var data = _fixture.Build<MailData>().With(x => x.Requester, UserId).With(x => x.Template, "CredentialExpiry").Create();
        A.CallTo(() => _userRepository.GetUserMailData(UserId)).Returns((false, null));
        async Task Act() => await _sut.SendMail(data).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be(AdministrationMailErrors.USER_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task SendMail_WithInvalidTemplate_CallsExpected()
    {
        // Arrange
        var data = new MailData(UserId, "testTemplate", Enumerable.Empty<MailParameter>());
        async Task Act() => await _sut.SendMail(data).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be(AdministrationMailErrors.INVALID_TEMPLATE.ToString());
    }

    [Fact]
    public async Task SendMail_WithUserWithoutEmail_DoesntCallService()
    {
        // Arrange
        var data = _fixture.Build<MailData>().With(x => x.Requester, UserId).With(x => x.Template, "CredentialExpiry").Create();
        A.CallTo(() => _userRepository.GetUserMailData(UserId)).Returns((true, null));

        // Act
        await _sut.SendMail(data);

        // Assert
        A.CallTo(() => _mailingService.CreateMailProcess(A<string>._, A<string>._, A<Dictionary<string, string>>._)).MustNotHaveHappened();
    }

    [Theory]
    [InlineData("CredentialExpiry")]
    [InlineData("CredentialRejected")]
    [InlineData("CredentialApproval")]
    public async Task SendMail_WithValid_CallsExpected(string template)
    {
        // Arrange
        var data = new MailData(UserId, template, Enumerable.Empty<MailParameter>());
        A.CallTo(() => _userRepository.GetUserMailData(UserId)).Returns((true, "test@email.com"));

        // Act
        await _sut.SendMail(data);

        // Assert
        A.CallTo(() => _mailingService.CreateMailProcess("test@email.com", template, A<IReadOnlyDictionary<string, string>>._)).MustHaveHappenedOnceExactly();
    }

    #endregion
}
