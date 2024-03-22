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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Library.Tests;

public class NetworkRegistrationProcessHelperTests
{
    private static readonly ProcessStepTypeId StepToRetrigger = ProcessStepTypeId.RETRIGGER_SYNCHRONIZE_USER;

    private readonly IPortalRepositories _portalRepositories;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly INetworkRepository _networkRepository;

    private readonly IFixture _fixture;
    private readonly NetworkRegistrationProcessHelper _sut;

    public NetworkRegistrationProcessHelperTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.ConfigureFixture();

        _portalRepositories = A.Fake<IPortalRepositories>();
        _processStepRepository = A.Fake<IProcessStepRepository>();
        _networkRepository = A.Fake<INetworkRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INetworkRepository>()).Returns(_networkRepository);

        _sut = new NetworkRegistrationProcessHelper(_portalRepositories);
    }

    [Fact]
    public async Task TriggerProcessStep_WithUntriggerableProcessStep_ThrowsConflictException()
    {
        // Act
        async Task Act() => await _sut.TriggerProcessStep(Guid.NewGuid().ToString(), ProcessStepTypeId.SYNCHRONIZE_USER);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Step {ProcessStepTypeId.SYNCHRONIZE_USER} is not retriggerable");
    }

    [Fact]
    public async Task TriggerProcessStep_WithNotExistingExternalId_ThrowsNotFoundException()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        A.CallTo(() => _networkRepository.IsValidRegistration(externalId.ToString(), A<IEnumerable<ProcessStepTypeId>>.That.IsSameSequenceAs(new[] { StepToRetrigger })))
            .Returns((false, _fixture.Create<VerifyProcessData>()));

        // Act
        async Task Act() => await _sut.TriggerProcessStep(externalId.ToString(), ProcessStepTypeId.RETRIGGER_SYNCHRONIZE_USER);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"external registration {externalId} does not exist");
    }

    [Theory]
    [InlineData(ProcessStepTypeId.RETRIGGER_SYNCHRONIZE_USER, ProcessStepTypeId.SYNCHRONIZE_USER)]
    [InlineData(ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_APPROVED, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED)]
    [InlineData(ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_DECLINED, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED)]
    [InlineData(ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_SUBMITTED, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED)]
    public async Task TriggerProcessStep_WithValidData_CallsExpected(ProcessStepTypeId processStepTypeId, ProcessStepTypeId retriggeredStep)
    {
        // Arrange
        var externalId = Guid.NewGuid();
        var processId = Guid.NewGuid();
        var processSteps = new List<ProcessStep>();

        var processStep = new ProcessStep(Guid.NewGuid(), processStepTypeId, ProcessStepStatusId.TODO, processId, DateTimeOffset.UtcNow);
        var data = new VerifyProcessData(
            new Process(processId, ProcessTypeId.PARTNER_REGISTRATION, Guid.NewGuid()),
            new[] { processStep });
        A.CallTo(() => _networkRepository.IsValidRegistration(externalId.ToString(), A<IEnumerable<ProcessStepTypeId>>.That.IsSameSequenceAs(new[] { processStepTypeId })))
            .Returns((true, data));
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<ValueTuple<ProcessStepTypeId, ProcessStepStatusId, Guid>>>._))
            .Invokes((IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> processStepTypeStatus) =>
                {
                    processSteps.AddRange(processStepTypeStatus.Select(x => new ProcessStep(Guid.NewGuid(), x.ProcessStepTypeId, x.ProcessStepStatusId, x.ProcessId, DateTimeOffset.UtcNow)));
                });
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<ValueTuple<Guid, Action<ProcessStep>?, Action<ProcessStep>>>>._))
            .Invokes((IEnumerable<(Guid ProcessStepId, Action<ProcessStep>? Initialize, Action<ProcessStep> Modify)> processStepIdsInitializeModifyData) =>
                {
                    var modify = processStepIdsInitializeModifyData.SingleOrDefault(x => processStep.Id == x.ProcessStepId);
                    if (modify == default)
                        return;

                    modify.Initialize?.Invoke(processStep);
                    modify.Modify.Invoke(processStep);
                });

        // Act
        await _sut.TriggerProcessStep(externalId.ToString(), processStepTypeId);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        processSteps.Should().ContainSingle()
            .Which.Should().Match<ProcessStep>(x =>
                x.ProcessStepTypeId == retriggeredStep &&
                x.ProcessStepStatusId == ProcessStepStatusId.TODO);
        processStep.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
    }
}
