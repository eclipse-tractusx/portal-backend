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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class NetworkControllerTests
{
    private readonly INetworkBusinessLogic _logic;
    private readonly NetworkController _controller;
    private readonly Fixture _fixture;

    public NetworkControllerTests()
    {
        _fixture = new Fixture();
        _logic = A.Fake<INetworkBusinessLogic>();
        _controller = new NetworkController(_logic);
    }

    [Fact]
    public async Task PartnerRegister_ReturnsExpected()
    {
        // Arrange
        var data = _fixture.Create<PartnerRegistrationData>();

        // Act
        var result = await _controller.PartnerRegister(data);

        // Assert
        result.StatusCode.Should().Be(200);
        A.CallTo(() => _logic.HandlePartnerRegistration(data)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerSynchronizeUser_ReturnsExpected()
    {
        // Arrange
        var externalId = Guid.NewGuid().ToString();

        // Act
        var result = await _controller.RetriggerSynchronizeUser(externalId);

        // Assert
        result.StatusCode.Should().Be(204);
        A.CallTo(() => _logic.RetriggerProcessStep(externalId, ProcessStepTypeId.RETRIGGER_SYNCHRONIZE_USER))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerCallbackOspApprove_ReturnsExpected()
    {
        // Arrange
        var externalId = Guid.NewGuid().ToString();

        // Act
        var result = await _controller.RetriggerCallbackOspApprove(externalId);

        // Assert
        result.StatusCode.Should().Be(204);
        A.CallTo(() => _logic.RetriggerProcessStep(externalId, ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_APPROVED))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerCallbackOspDecline_ReturnsExpected()
    {
        // Arrange
        var externalId = Guid.NewGuid().ToString();

        // Act
        var result = await _controller.RetriggerCallbackOspDecline(externalId);

        // Assert
        result.StatusCode.Should().Be(204);
        A.CallTo(() => _logic.RetriggerProcessStep(externalId, ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_DECLINED))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerCallbackOspSubmitted_ReturnsExpected()
    {
        // Arrange
        var externalId = Guid.NewGuid().ToString();

        // Act
        var result = await _controller.RetriggerCallbackOspSubmitted(externalId);

        // Assert
        result.StatusCode.Should().Be(204);
        A.CallTo(() => _logic.RetriggerProcessStep(externalId, ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_SUBMITTED))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetOspCompanyApplicationDetailsAsync_ReturnsCompanyApplicationDetails()
    {
        //Arrange
        var page = _fixture.Create<int>();
        var size = _fixture.Create<int>();
        var companyApplicationStatusFilter = _fixture.Create<CompanyApplicationStatusFilter>();
        var companyName = _fixture.Create<string>();
        var externalId = _fixture.Create<string>();
        var dateCreatedOrderFilter = _fixture.Create<DateCreatedOrderFilter>();

        var paginationResponse = new Pagination.Response<CompanyDetailsOspOnboarding>(new Pagination.Metadata(15, 1, 1, 15), _fixture.CreateMany<CompanyDetailsOspOnboarding>(5));
        A.CallTo(() => _logic.GetOspCompanyDetailsAsync(A<int>._, A<int>._, A<CompanyApplicationStatusFilter>._, A<string>._, A<string>._, A<DateCreatedOrderFilter>._))
            .Returns(paginationResponse);

        //Act
        var result = await _controller.GetOspCompanyDetailsAsync(page, size, companyApplicationStatusFilter, companyName, externalId, dateCreatedOrderFilter);

        //Assert
        A.CallTo(() => _logic.GetOspCompanyDetailsAsync(page, size, companyApplicationStatusFilter, companyName, externalId, dateCreatedOrderFilter)).MustHaveHappenedOnceExactly();
        Assert.IsType<Pagination.Response<CompanyDetailsOspOnboarding>>(result);
        result.Content.Should().HaveCount(5);
    }
}
