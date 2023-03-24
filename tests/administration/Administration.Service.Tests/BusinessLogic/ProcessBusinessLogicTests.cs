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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class ProcessBusinessLogicTests
{
    private static readonly Guid OfferSubscriptionId = Guid.NewGuid();
    private readonly IOfferSubscriptionProcessService _offerSubscriptionProcessService;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IFixture _fixture;
    private readonly IProcessBusinessLogic _sut;

    public ProcessBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());  

        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerSubscriptionProcessService = A.Fake<IOfferSubscriptionProcessService>();

        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>())
            .Returns(_offerSubscriptionsRepository);
        
        _sut = new ProcessBusinessLogic(_offerSubscriptionProcessService, _portalRepositories);
    }

    #region GetProcessStepData
    
    [Fact]
    public async Task GetProcessStepData_WithValidInput_ReturnsExpected()
    {
        // Arrange
        var list = _fixture.CreateMany<ProcessStepData>(5);
        A.CallTo(() => _offerSubscriptionsRepository.GetProcessStepsForSubscription(OfferSubscriptionId))
            .Returns(list.ToAsyncEnumerable());
        
        // Act
        var result = await _sut.GetProcessStepsForSubscription(OfferSubscriptionId).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
    }
    
    #endregion

    #region GetProcessStepData
    
    [Theory]
    [InlineData(ProcessStepTypeId.RETRIGGER_PROVIDER, ProcessStepTypeId.TRIGGER_PROVIDER)]
    [InlineData(ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION, ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION)]
    [InlineData(ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION, ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION)]
    [InlineData(ProcessStepTypeId.RETRIGGER_SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION, ProcessStepTypeId.SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION)]
    [InlineData(ProcessStepTypeId.RETRIGGER_ACTIVATE_SUBSCRIPTION, ProcessStepTypeId.ACTIVATE_SUBSCRIPTION)]
    [InlineData(ProcessStepTypeId.RETRIGGER_PROVIDER_CALLBACK, ProcessStepTypeId.TRIGGER_PROVIDER_CALLBACK, false)]
    public async Task TriggerProcessStep_WithValidInput_ReturnsExpected(ProcessStepTypeId retriggerStep, ProcessStepTypeId nextStep, bool mustBePending = true)
    {
        // Arrange
        var processStepId = Guid.NewGuid();
        var processId = Guid.NewGuid();
        var processStep = new ProcessStep(processStepId, retriggerStep, ProcessStepStatusId.TODO, processId, DateTimeOffset.Now);
        A.CallTo(() => _offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(OfferSubscriptionId, retriggerStep, null, mustBePending))
            .ReturnsLazily(() => new IOfferSubscriptionProcessService.ManualOfferSubscriptionProcessStepData(OfferSubscriptionId, _fixture.Create<Process>(), processStepId, Enumerable.Repeat(processStep, 1)));
        
        // Act
        await _sut.TriggerProcessStep(OfferSubscriptionId, retriggerStep, mustBePending);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionProcessService.FinalizeProcessSteps(
                A<IOfferSubscriptionProcessService.ManualOfferSubscriptionProcessStepData>._,
                A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == nextStep)))
            .MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task TriggerProcessStep_WithInvalidStep_ThrowsConflictException()
    {
        // Act
        async Task Act() => await _sut.TriggerProcessStep(OfferSubscriptionId, ProcessStepTypeId.START_AUTOSETUP, false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Step {ProcessStepTypeId.START_AUTOSETUP} is not retriggerable");
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _offerSubscriptionProcessService.FinalizeProcessSteps(
                A<IOfferSubscriptionProcessService.ManualOfferSubscriptionProcessStepData>._,
                A<IEnumerable<ProcessStepTypeId>>._))
            .MustNotHaveHappened();
    }
    
    #endregion

}