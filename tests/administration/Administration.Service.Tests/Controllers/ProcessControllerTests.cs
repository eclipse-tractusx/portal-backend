/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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

using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class ProcessControllerTests
{
    private const string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private static readonly Guid OfferSubscriptionId = new("4C1A6851-D4E7-4E10-A011-3732CD049999");
    private readonly IProcessBusinessLogic _logic;
    private readonly ProcessController _controller;
    private readonly Fixture _fixture;

    public ProcessControllerTests()
    {
        _fixture = new Fixture();
        _logic = A.Fake<IProcessBusinessLogic>();
        _controller = new ProcessController(_logic);
        _controller.AddControllerContextWithClaim(IamUserId);
    }

    [Fact]
    public async Task GetProcessStepData_WithValidData_ReturnsExpected()
    {
        //Arrange
        var list = _fixture.CreateMany<ProcessStepData>(5);
        A.CallTo(() => _logic.GetProcessStepsForSubscription(OfferSubscriptionId))
            .Returns(list.ToAsyncEnumerable());

        //Act
        var result = await this._controller.GetProcessStepsForSubscription(OfferSubscriptionId).ToListAsync().ConfigureAwait(false);

        //Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task RetriggerProvider_WithValidData_ReturnsNoContent()
    {
        //Arrange
        A.CallTo(() => _logic.TriggerProcessStep(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_PROVIDER, true))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        var result = await this._controller.RetriggerProvider(OfferSubscriptionId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.TriggerProcessStep(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_PROVIDER, true)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RetriggerCreateClient_WithValidData_ReturnsNoContent()
    {
        //Arrange
        A.CallTo(() => _logic.TriggerProcessStep(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION, true))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        var result = await this._controller.RetriggerCreateClient(OfferSubscriptionId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.TriggerProcessStep(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION, true)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }
    
    [Fact]
    public async Task RetriggerSingleInstance_WithValidData_ReturnsNoContent()
    {
        //Arrange
        A.CallTo(() => _logic.TriggerProcessStep(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION, true))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        var result = await this._controller.RetriggerSingleInstanceDetailsCreation(OfferSubscriptionId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.TriggerProcessStep(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION, true)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RetriggerCreateTechnicalUser_WithValidData_ReturnsNoContent()
    {
        //Arrange
        A.CallTo(() => _logic.TriggerProcessStep(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION, true))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        var result = await this._controller.RetriggerCreateTechnicalUser(OfferSubscriptionId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.TriggerProcessStep(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION, true)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RetriggerActivation_WithValidData_ReturnsNoContent()
    {
        //Arrange
        A.CallTo(() => _logic.TriggerProcessStep(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_ACTIVATE_SUBSCRIPTION, true))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        var result = await this._controller.RetriggerActivation(OfferSubscriptionId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.TriggerProcessStep(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_ACTIVATE_SUBSCRIPTION, true)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RetriggerProviderCallback_WithValidData_ReturnsNoContent()
    {
        //Arrange
        A.CallTo(() => _logic.TriggerProcessStep(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_PROVIDER_CALLBACK, false))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        var result = await this._controller.RetriggerProviderCallback(OfferSubscriptionId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.TriggerProcessStep(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_PROVIDER_CALLBACK, false)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }
}
