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
using Org.Eclipse.TractusX.Portal.Backend.Processes.Library;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library.Tests;

public class OfferSubscriptionProcessServiceTests
{
    private readonly IFixture _fixture;

    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly IProcessStepRepository _processStepRepository;

    private readonly IOfferSubscriptionProcessService _service;

    public OfferSubscriptionProcessServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);

        _service = new OfferSubscriptionProcessService(_portalRepositories);
    }

    #region VerifySubscriptionAndProcessSteps

    [Fact]
    public async Task VerifySubscriptionAndProcessSteps()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var process = new Process(Guid.NewGuid(), ProcessTypeId.OFFER_SUBSCRIPTION, Guid.NewGuid());
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(Enum.GetValues<ProcessStepTypeId>().Length - 2).ToImmutableArray();
        var allProcessStepTypeIds = processStepTypeIds.Append(processStepTypeId).Distinct().ToImmutableArray();

        IEnumerable<ProcessStep>? processSteps = null;

        A.CallTo(() => _offerSubscriptionsRepository.IsActiveOfferSubscription(A<Guid>._))
            .Returns((true, false));
        A.CallTo(() => _offerSubscriptionsRepository.GetProcessStepData(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .ReturnsLazily((Guid id, IEnumerable<ProcessStepTypeId> processStepTypes) =>
            {
                processSteps = processStepTypes.Select(typeId => new ProcessStep(Guid.NewGuid(), typeId, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow)).ToImmutableArray();
                return subscriptionId == id ?
                    new VerifyProcessData(
                    process,
                    processSteps) :
                    null;
            });

        // Act
        var result = await _service.VerifySubscriptionAndProcessSteps(subscriptionId, processStepTypeId, processStepTypeIds, true);

        // Assert
        result.Should().NotBeNull();
        result.ProcessStepTypeId.Should().Be(processStepTypeId);
        processSteps.Should()
            .ContainSingle(step => step.ProcessStepTypeId == processStepTypeId)
            .Which.ProcessStepStatusId.Should().Be(ProcessStepStatusId.TODO);
        result.ProcessSteps.Select(step => (step.ProcessStepTypeId, step.ProcessStepStatusId, step.ProcessId))
            .Should().HaveSameCount(allProcessStepTypeIds)
            .And.Contain(allProcessStepTypeIds.Select(stepTypeId => (stepTypeId, ProcessStepStatusId.TODO, process.Id)));
    }

    [Fact]
    public async Task VerifySubscriptionAndProcessSteps_InvalidSubscriptionId_Throws()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(Enum.GetValues<ProcessStepTypeId>().Length - 2).ToImmutableArray();

        A.CallTo(() => _offerSubscriptionsRepository.IsActiveOfferSubscription(A<Guid>._))
            .Returns((false, false));
        A.CallTo(() => _offerSubscriptionsRepository.GetProcessStepData(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns<VerifyProcessData?>(null);

        var Act = () => _service.VerifySubscriptionAndProcessSteps(subscriptionId, processStepTypeId, processStepTypeIds, true);

        // Act
        var result = await Assert.ThrowsAsync<NotFoundException>(Act);
        ;

        // Assert
        result.Message.Should().Be($"offer subscription {subscriptionId} does not exist");
    }

    [Fact]
    public async Task VerifySubscriptionAndProcessSteps_WithActive_Throws()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var lockExpiryDate = DateTimeOffset.UtcNow;
        var process = new Process(Guid.NewGuid(), ProcessTypeId.OFFER_SUBSCRIPTION, Guid.NewGuid()) { LockExpiryDate = lockExpiryDate };
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(Enum.GetValues<ProcessStepTypeId>().Length - 2).ToImmutableArray();

        A.CallTo(() => _offerSubscriptionsRepository.IsActiveOfferSubscription(A<Guid>._))
            .Returns((true, true));
        A.CallTo(() => _offerSubscriptionsRepository.GetProcessStepData(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new VerifyProcessData(process, null));

        var Act = () => _service.VerifySubscriptionAndProcessSteps(subscriptionId, processStepTypeId, processStepTypeIds, true);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act);
        ;

        // Assert
        result.Message.Should().Be($"offer subscription {subscriptionId} is already activated");
    }

    [Fact]
    public async Task VerifySubscriptionAndProcessSteps_WithProcessNull_Throws()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(Enum.GetValues<ProcessStepTypeId>().Length - 2).ToImmutableArray();

        A.CallTo(() => _offerSubscriptionsRepository.IsActiveOfferSubscription(A<Guid>._))
            .Returns((true, false));
        A.CallTo(() => _offerSubscriptionsRepository.GetProcessStepData(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new VerifyProcessData(null, null));

        var Act = () => _service.VerifySubscriptionAndProcessSteps(subscriptionId, processStepTypeId, processStepTypeIds, true);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act);
        ;

        // Assert
        result.Message.Should().Be($"offer subscription {subscriptionId} is not associated with any process");
    }

    [Fact]
    public async Task VerifySubscriptionAndProcessSteps_LockedProcess_Throws()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var lockExpiryDate = DateTimeOffset.UtcNow;
        var process = new Process(Guid.NewGuid(), ProcessTypeId.OFFER_SUBSCRIPTION, Guid.NewGuid()) { LockExpiryDate = lockExpiryDate };
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(Enum.GetValues<ProcessStepTypeId>().Length - 2).ToImmutableArray();

        A.CallTo(() => _offerSubscriptionsRepository.IsActiveOfferSubscription(A<Guid>._))
            .Returns((true, false));
        A.CallTo(() => _offerSubscriptionsRepository.GetProcessStepData(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new VerifyProcessData(process, null));

        var Act = () => _service.VerifySubscriptionAndProcessSteps(subscriptionId, processStepTypeId, processStepTypeIds, true);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act);
        ;

        // Assert
        result.Message.Should().Be($"process {process.Id} associated with offer subscription {subscriptionId} is locked, lock expiry is set to {process.LockExpiryDate}");
    }

    [Fact]
    public async Task VerifySubscriptionAndProcessSteps_WithoutProcessSteps_Throws()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var process = new Process(Guid.NewGuid(), ProcessTypeId.OFFER_SUBSCRIPTION, Guid.NewGuid());
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        IEnumerable<ProcessStepTypeId>? processStepTypeIds = null;

        A.CallTo(() => _offerSubscriptionsRepository.IsActiveOfferSubscription(A<Guid>._))
            .Returns((true, false));
        A.CallTo(() => _offerSubscriptionsRepository.GetProcessStepData(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new VerifyProcessData(process, null));

        var Act = () => _service.VerifySubscriptionAndProcessSteps(subscriptionId, processStepTypeId, processStepTypeIds, true);

        // Act
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        // Assert
        result.Message.Should().Be("processSteps should never be null here");
    }

    [Fact]
    public async Task VerifySubscriptionAndProcessSteps_UnexpectedProcessStepData_Throws()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var process = new Process(Guid.NewGuid(), ProcessTypeId.OFFER_SUBSCRIPTION, Guid.NewGuid());
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        IEnumerable<ProcessStepTypeId>? processStepTypeIds = null;

        var processSteps = new ProcessStep[] { new(Guid.NewGuid(), processStepTypeId, ProcessStepStatusId.SKIPPED, process.Id, DateTimeOffset.UtcNow) };

        A.CallTo(() => _offerSubscriptionsRepository.IsActiveOfferSubscription(A<Guid>._))
            .Returns((true, false));
        A.CallTo(() => _offerSubscriptionsRepository.GetProcessStepData(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new VerifyProcessData(process, processSteps));

        var Act = () => _service.VerifySubscriptionAndProcessSteps(subscriptionId, processStepTypeId, processStepTypeIds, true);

        // Act
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        // Assert
        result.Message.Should().Be("processSteps should never have any other status than TODO here");
    }

    [Fact]
    public async Task VerifySubscriptionAndProcessSteps_NoEligibleProcessStep_Throws()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var process = new Process(Guid.NewGuid(), ProcessTypeId.OFFER_SUBSCRIPTION, Guid.NewGuid());
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        IEnumerable<ProcessStepTypeId>? processStepTypeIds = null;

        var processSteps = _fixture.CreateMany<ProcessStepTypeId>(5).Where(typeId => typeId != processStepTypeId).Select(typeId => new ProcessStep(Guid.NewGuid(), typeId, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow)).ToImmutableArray();

        A.CallTo(() => _offerSubscriptionsRepository.IsActiveOfferSubscription(A<Guid>._))
            .Returns((true, false));
        A.CallTo(() => _offerSubscriptionsRepository.GetProcessStepData(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new VerifyProcessData(process, processSteps));

        var Act = () => _service.VerifySubscriptionAndProcessSteps(subscriptionId, processStepTypeId, processStepTypeIds, true);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        result.Message.Should().Be($"offer subscription {subscriptionId}, process step {processStepTypeId} is not eligible to run");
    }

    #endregion

    #region FinalizeProcessSteps

    [Fact]
    public void FinalizeProcessSteps_ReturnsExpected()
    {
        // Arrange
        var process = new Process(Guid.NewGuid(), ProcessTypeId.OFFER_SUBSCRIPTION, Guid.NewGuid());
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var processStepId = Guid.NewGuid();
        var context = new ManualProcessStepData(
            processStepTypeId,
            process,
            new ProcessStep[] { new(processStepId, processStepTypeId, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow) },
            _portalRepositories
        );

        IEnumerable<ProcessStep>? modifiedProcessSteps = null;

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid, Action<ProcessStep>?, Action<ProcessStep>)>>._))
            .Invokes((IEnumerable<(Guid ProcessStepId, Action<ProcessStep>? Initialize, Action<ProcessStep> Modify)> stepsToModify) =>
            {
                modifiedProcessSteps = stepsToModify.Select(
                    stepToModify =>
                    {
                        var step = new ProcessStep(processStepId, default, default, default, default);
                        stepToModify.Initialize?.Invoke(step);
                        stepToModify.Modify(step);
                        return step;
                    });
            });

        IEnumerable<ProcessStep>? newProcessSteps = null;

        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId, ProcessStepStatusId, Guid)>>._))
            .ReturnsLazily((IEnumerable<(ProcessStepTypeId StepTypeId, ProcessStepStatusId StepStatusId, Guid ProcessId)> processStepTypeStatus) =>
            {
                newProcessSteps = processStepTypeStatus.Select(x => new ProcessStep(Guid.NewGuid(), x.StepTypeId, x.StepStatusId, x.ProcessId, DateTimeOffset.UtcNow)).ToList();
                return newProcessSteps;
            });

        var nextProcessStepTypeIds = Enum.GetValues<ProcessStepTypeId>().Except(context.ProcessSteps.Select(step => step.ProcessStepTypeId)).ToImmutableArray();

        // Act
        _service.FinalizeProcessSteps(
            context,
            Enum.GetValues<ProcessStepTypeId>());

        // Assert
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid, Action<ProcessStep>?, Action<ProcessStep>)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId, ProcessStepStatusId, Guid)>>._))
            .MustHaveHappenedOnceExactly();

        modifiedProcessSteps.Should().NotBeNull().And.ContainSingle().Which.Should().Match<ProcessStep>(
            step =>
                step.Id == processStepId &&
                step.ProcessStepStatusId == ProcessStepStatusId.DONE
            );

        newProcessSteps.Should().NotBeNull()
            .And.HaveCount(nextProcessStepTypeIds.Length)
            .And.AllSatisfy(
                x =>
                {
                    x.ProcessId.Should().Be(process.Id);
                    x.ProcessStepStatusId.Should().Be(ProcessStepStatusId.TODO);
                });
        newProcessSteps!.Select(x => x.ProcessStepTypeId).Should().Contain(nextProcessStepTypeIds);
    }

    [Fact]
    public void FinalizeProcessSteps_NoModifyEnty_ReturnsExpected()
    {
        // Arrange
        var process = new Process(Guid.NewGuid(), ProcessTypeId.OFFER_SUBSCRIPTION, Guid.NewGuid());
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var processStepId = Guid.NewGuid();
        var context = new ManualProcessStepData(
            processStepTypeId,
            process,
            new ProcessStep[] { new(processStepId, processStepTypeId, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow) },
            _portalRepositories
        );

        IEnumerable<ProcessStep>? modifiedProcessSteps = null;

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid, Action<ProcessStep>?, Action<ProcessStep>)>>._))
            .Invokes((IEnumerable<(Guid ProcessStepId, Action<ProcessStep>? Initialize, Action<ProcessStep> Modify)> stepsToModify) =>
            {
                modifiedProcessSteps = stepsToModify.Select(
                    stepToModify =>
                    {
                        var step = new ProcessStep(processStepId, default, default, default, default);
                        stepToModify.Initialize?.Invoke(step);
                        stepToModify.Modify(step);
                        return step;
                    });
            });

        IEnumerable<ProcessStep>? newProcessSteps = null;

        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId, ProcessStepStatusId, Guid)>>._))
            .ReturnsLazily((IEnumerable<(ProcessStepTypeId StepTypeId, ProcessStepStatusId StepStatusId, Guid ProcessId)> processStepTypeStatus) =>
            {
                newProcessSteps = processStepTypeStatus.Select(x => new ProcessStep(Guid.NewGuid(), x.StepTypeId, x.StepStatusId, x.ProcessId, DateTimeOffset.UtcNow)).ToList();
                return newProcessSteps;
            });

        var nextProcessStepTypeIds = Enum.GetValues<ProcessStepTypeId>().Except(context.ProcessSteps.Select(step => step.ProcessStepTypeId)).ToImmutableArray();

        // Act
        _service.FinalizeProcessSteps(
            context,
            Enum.GetValues<ProcessStepTypeId>());

        // Assert
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid, Action<ProcessStep>?, Action<ProcessStep>)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId, ProcessStepStatusId, Guid)>>._))
            .MustHaveHappenedOnceExactly();

        modifiedProcessSteps
            .Should().NotBeNull().And.ContainSingle().Which.Should().Match<ProcessStep>(
                step =>
                    step.Id == processStepId &&
                    step.ProcessStepStatusId == ProcessStepStatusId.DONE
            );

        newProcessSteps.Should().NotBeNull()
            .And.HaveCount(nextProcessStepTypeIds.Length)
            .And.AllSatisfy(
                x =>
                {
                    x.ProcessId.Should().Be(process.Id);
                    x.ProcessStepStatusId.Should().Be(ProcessStepStatusId.TODO);
                });
        newProcessSteps!.Select(x => x.ProcessStepTypeId).Should().Contain(nextProcessStepTypeIds);
    }

    [Fact]
    public void FinalizeProcessSteps_NullProcessSteps_ReturnsExpected()
    {
        // Arrange
        var process = new Process(Guid.NewGuid(), ProcessTypeId.OFFER_SUBSCRIPTION, Guid.NewGuid());
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var processStepId = Guid.NewGuid();
        var context = new ManualProcessStepData(
            processStepTypeId,
            process,
            new ProcessStep[] { new(processStepId, processStepTypeId, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow) },
            _portalRepositories
        );
        IEnumerable<ProcessStep>? modifiedProcessSteps = null;

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid, Action<ProcessStep>?, Action<ProcessStep>)>>._))
            .Invokes((IEnumerable<(Guid ProcessStepId, Action<ProcessStep>? Initialize, Action<ProcessStep> Modify)> stepsToModify) =>
            {
                modifiedProcessSteps = stepsToModify.Select(
                    stepToModify =>
                    {
                        var step = new ProcessStep(processStepId, default, default, default, default);
                        stepToModify.Initialize?.Invoke(step);
                        stepToModify.Modify(step);
                        return step;
                    });
            });

        // Act
        _service.FinalizeProcessSteps(
            context,
            null);

        // Assert
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid, Action<ProcessStep>?, Action<ProcessStep>)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId, ProcessStepStatusId, Guid)>>._))
            .MustNotHaveHappened();

        modifiedProcessSteps.Should().NotBeNull().And.ContainSingle().Which.Should().Match<ProcessStep>(
            step =>
                step.Id == processStepId &&
                step.ProcessStepStatusId == ProcessStepStatusId.DONE);
    }

    #endregion
}
