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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class InvitationControllerTests
{
    private readonly IInvitationBusinessLogic _logic;
    private readonly InvitationController _controller;
    private readonly Fixture _fixture;

    public InvitationControllerTests()
    {
        _fixture = new Fixture();
        _logic = A.Fake<IInvitationBusinessLogic>();
        _controller = new InvitationController(_logic);
    }

    [Fact]
    public async Task ExecuteInvitation_ReturnsExpected()
    {
        // Arrange
        var data = _fixture.Create<CompanyInvitationData>();

        // Act
        await this._controller.ExecuteInvitation(data).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.ExecuteInvitation(data)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerSetupIdp_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();

        // Act
        var result = await this._controller.RetriggerSetupIdp(processId).ConfigureAwait(false);

        // Assert
        result.StatusCode.Should().Be(204);
        A.CallTo(() => _logic.RetriggerProcessStep(processId, ProcessStepTypeId.RETRIGGER_INVITATION_SETUP_IDP))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerCreateDatabaseIdp_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();

        // Act
        var result = await this._controller.RetriggerCreateDatabaseIdp(processId).ConfigureAwait(false);

        // Assert
        result.StatusCode.Should().Be(204);
        A.CallTo(() => _logic.RetriggerProcessStep(processId, ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_DATABASE_IDP))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerInvitationCreateUser_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();

        // Act
        var result = await this._controller.RetriggerInvitationCreateUser(processId).ConfigureAwait(false);

        // Assert
        result.StatusCode.Should().Be(204);
        A.CallTo(() => _logic.RetriggerProcessStep(processId, ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_USER))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerInvitationSendMail_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();

        // Act
        var result = await this._controller.RetriggerInvitationSendMail(processId).ConfigureAwait(false);

        // Assert
        result.StatusCode.Should().Be(204);
        A.CallTo(() => _logic.RetriggerProcessStep(processId, ProcessStepTypeId.RETRIGGER_INVITATION_SEND_MAIL))
            .MustHaveHappenedOnceExactly();
    }
}
