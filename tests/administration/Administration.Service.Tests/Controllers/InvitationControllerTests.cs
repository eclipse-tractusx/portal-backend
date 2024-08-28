/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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
        var invitationId = Guid.NewGuid();
        A.CallTo(() => _logic.ExecuteInvitation(A<CompanyInvitationData>._)).Returns(invitationId);

        // Act
        var result = await _controller.ExecuteInvitation(data);

        // Assert
        A.CallTo(() => _logic.ExecuteInvitation(data)).MustHaveHappenedOnceExactly();
        result.Should().Be(invitationId);
    }

    [Fact]
    public async Task RetriggerCreateSharedIdpServiceAccount_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();

        // Act
        var result = await _controller.RetriggerCreateSharedIdpServiceAccount(processId);

        // Assert
        result.StatusCode.Should().Be(204);
        A.CallTo(() => _logic.RetriggerCreateSharedIdpServiceAccount(processId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerUpdateCentralIdpUrls_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();

        // Act
        var result = await _controller.RetriggerUpdateCentralIdpUrls(processId);

        // Assert
        result.StatusCode.Should().Be(204);
        A.CallTo(() => _logic.RetriggerUpdateCentralIdpUrls(processId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerCreateCentralIdpOrgMapper_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();

        // Act
        var result = await _controller.RetriggerCreateCentralIdpOrgMapper(processId);

        // Assert
        result.StatusCode.Should().Be(204);
        A.CallTo(() => _logic.RetriggerCreateCentralIdpOrgMapper(processId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerCreateSharedRealmIdpClient_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();

        // Act
        var result = await _controller.RetriggerCreateSharedRealmIdpClient(processId);

        // Assert
        result.StatusCode.Should().Be(204);
        A.CallTo(() => _logic.RetriggerCreateSharedRealmIdpClient(processId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerEnableCentralIdp_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();

        // Act
        var result = await _controller.RetriggerEnableCentralIdp(processId);

        // Assert
        result.StatusCode.Should().Be(204);
        A.CallTo(() => _logic.RetriggerEnableCentralIdp(processId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerCreateDatabaseIdp_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();

        // Act
        var result = await _controller.RetriggerCreateDatabaseIdp(processId);

        // Assert
        result.StatusCode.Should().Be(204);
        A.CallTo(() => _logic.RetriggerCreateDatabaseIdp(processId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerInvitationCreateUser_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();

        // Act
        var result = await _controller.RetriggerInvitationCreateUser(processId);

        // Assert
        result.StatusCode.Should().Be(204);
        A.CallTo(() => _logic.RetriggerInvitationCreateUser(processId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetApplicationAndCompanyDetails_ReturnsExpected()
    {
        // Arrange
        var expectedResult = new CompanyInvitationDetails(Guid.NewGuid(), Guid.NewGuid());
        var invitationId = Guid.NewGuid();
        A.CallTo(() => _logic.GetApplicationAndCompanyDetails(A<Guid>._)).Returns(expectedResult);
        // Act
        var result = await _controller.GetApplicationAndCompanyDetails(invitationId);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _logic.GetApplicationAndCompanyDetails(invitationId))
            .MustHaveHappenedOnceExactly();
    }
}
