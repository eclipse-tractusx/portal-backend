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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Worker.Library.Tests;

public class ProcessExecutorTests
{
    private readonly IProcessTypeExecutor _processTypeExecutor;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly IProcessExecutor _sut;
    private readonly IFixture _fixture;

    public ProcessExecutorTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b =>_fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _processTypeExecutor = A.Fake<IProcessTypeExecutor>();

        _portalRepositories = A.Fake<IPortalRepositories>();
        _processStepRepository = A.Fake<IProcessStepRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>())
            .Returns(_processStepRepository);

        A.CallTo(() => _processTypeExecutor.GetProcessTypeId())
            .Returns(ProcessTypeId.APPLICATION_CHECKLIST);
        
        _sut = new ProcessExecutor(
            new [] { _processTypeExecutor },
            _portalRepositories);
    }

    #region GetRegisteredProcessTypeIds

    [Fact]
    public void GetRegisteredProcessTypeIds_ReturnsExpected()
    {
        // Act
        var result = _sut.GetRegisteredProcessTypeIds();

        // Assert
        result.Should().HaveCount(1).And.Contain(ProcessTypeId.APPLICATION_CHECKLIST);
    }

    #endregion

    #region ExecuteProcess

    [Fact]
    public async Task ExecuteProcess_WithInvalidProcessTypeId_Throws()
    {
        // Arrange
        var Act = async () => await _sut.ExecuteProcess(Guid.NewGuid(), (ProcessTypeId)default, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Act
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);

        // Assert
        result.Message.Should().Be($"processType {(ProcessTypeId)default} is not a registered executable processType.");
    }

    [Theory]
    [InlineData(ProcessStepStatusId.DONE)]
    [InlineData(ProcessStepStatusId.TODO)]
    public async Task ExecuteProcess_WithInitialSteps_ReturnsExpected(ProcessStepStatusId stepStatusId)
    {
        // Arrange
        var processId = Guid.NewGuid();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var processStepData = (Id: Guid.NewGuid(), processStepTypeId);
        var initialStepTypeIds = Enum.GetValues<ProcessStepTypeId>().Where(x => x != processStepTypeId).Take(3).ToImmutableArray();

        A.CallTo(() => _processStepRepository.GetProcessStepData(processId))
            .Returns(new [] { processStepData }.ToAsyncEnumerable());

        A.CallTo(() => _processTypeExecutor.IsExecutableStepTypeId(A<ProcessStepTypeId>._))
            .Returns(true);

        A.CallTo(() => _processTypeExecutor.InitializeProcess(processId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new IProcessTypeExecutor.InitializationResult(false, initialStepTypeIds));

        A.CallTo(() => _processTypeExecutor.ExecuteProcessStep(A<ProcessStepTypeId>._,A<IEnumerable<ProcessStepTypeId>>._,A<CancellationToken>._))
            .Returns(new IProcessTypeExecutor.StepExecutionResult(false, stepStatusId, null, null));

        IEnumerable<ProcessStep>? createdProcessSteps = null;;

        A.CallTo(() => _processStepRepository.CreateProcessStepRange(processId, A<IEnumerable<ProcessStepTypeId>>._))
            .ReturnsLazily((Guid _, IEnumerable<ProcessStepTypeId> stepTypeIds) =>
            {
                createdProcessSteps = stepTypeIds.Select(stepTypeId => new ProcessStep(Guid.NewGuid(), stepTypeId, ProcessStepStatusId.TODO, processId, DateTimeOffset.UtcNow)).ToImmutableList();
                return createdProcessSteps;
            });

        var modifiedProcessSteps = new List<ProcessStep>();

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .Invokes((Guid stepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify) =>
            {
                var step = new ProcessStep(stepId, default, default, Guid.Empty, default);
                initialize?.Invoke(step);
                modify(step);
                modifiedProcessSteps.Add(step);
            });

        // Act
        var result = await _sut.ExecuteProcess(processId, ProcessTypeId.APPLICATION_CHECKLIST, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(initialStepTypeIds.Length + 2)
            .And.ContainInOrder(
                stepStatusId == ProcessStepStatusId.DONE
                    ? new [] { true, true, true, true }
                    : new [] { true, false, false, false });

        A.CallTo(() => _processStepRepository.CreateProcessStepRange(processId, A<IEnumerable<ProcessStepTypeId>>.That.IsSameSequenceAs(initialStepTypeIds)))
            .MustHaveHappenedOnceExactly();

        createdProcessSteps
            .Should().NotBeNull()
            .And.HaveSameCount(initialStepTypeIds)
            .And.Satisfy(
                x => x.ProcessStepTypeId == initialStepTypeIds[0] && x.ProcessStepStatusId == ProcessStepStatusId.TODO,
                x => x.ProcessStepTypeId == initialStepTypeIds[1] && x.ProcessStepStatusId == ProcessStepStatusId.TODO,
                x => x.ProcessStepTypeId == initialStepTypeIds[2] && x.ProcessStepStatusId == ProcessStepStatusId.TODO);

        if (stepStatusId == ProcessStepStatusId.DONE)
        {
            A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
                .MustHaveHappened(initialStepTypeIds.Length + 1, Times.Exactly);
            modifiedProcessSteps
                .Should().HaveCount(initialStepTypeIds.Length + 1)
                .And.Satisfy(
                    x => x.Id == processStepData.Id && x.ProcessStepStatusId == stepStatusId,
                    x => x.Id == createdProcessSteps!.ElementAt(0).Id && x.ProcessStepStatusId == stepStatusId,
                    x => x.Id == createdProcessSteps!.ElementAt(1).Id && x.ProcessStepStatusId == stepStatusId,
                    x => x.Id == createdProcessSteps!.ElementAt(2).Id && x.ProcessStepStatusId == stepStatusId);
        }
        else
        {
            A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
                .MustNotHaveHappened();
        }
    }

    [Theory]
    [InlineData(ProcessStepStatusId.DONE)]
    [InlineData(ProcessStepStatusId.TODO)]
    public async Task ExecuteProcess_NoScheduleOrSkippedSteps_ReturnsExpected(ProcessStepStatusId stepStatusId)
    {
        // Arrange
        var processId = Guid.NewGuid();
        var processStepData = _fixture.CreateMany<ProcessStepTypeId>(3).Select(stepTypeId => (Id: Guid.NewGuid(), StepTypeId: stepTypeId)).OrderBy(x => x.StepTypeId).ToImmutableArray();

        A.CallTo(() => _processStepRepository.GetProcessStepData(processId))
            .Returns(processStepData.ToAsyncEnumerable());

        A.CallTo(() => _processTypeExecutor.IsExecutableStepTypeId(A<ProcessStepTypeId>._))
            .Returns(true);

        A.CallTo(() => _processTypeExecutor.InitializeProcess(processId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new IProcessTypeExecutor.InitializationResult(false, null));

        A.CallTo(() => _processTypeExecutor.ExecuteProcessStep(A<ProcessStepTypeId>._,A<IEnumerable<ProcessStepTypeId>>._,A<CancellationToken>._))
            .Returns(new IProcessTypeExecutor.StepExecutionResult(false, stepStatusId, null, null));

        var modifiedProcessSteps = new List<ProcessStep>();

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .Invokes((Guid stepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify) =>
            {
                var step = new ProcessStep(stepId, default, default, Guid.Empty, default);
                initialize?.Invoke(step);
                modify(step);
                modifiedProcessSteps.Add(step);
            });

        // Act
        var result = await _sut.ExecuteProcess(processId, ProcessTypeId.APPLICATION_CHECKLIST, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(processStepData.Length + 1)
            .And.ContainInOrder(stepStatusId == ProcessStepStatusId.DONE
                ? new [] { false, true, true, true }
                : new [] { false, false, false, false });

        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .MustNotHaveHappened();

        if (stepStatusId == ProcessStepStatusId.DONE)
        {
            A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
                .MustHaveHappened(processStepData.Length, Times.Exactly);

            modifiedProcessSteps
                .Should().HaveSameCount(processStepData)
                .And.Satisfy(
                    x => x.Id == processStepData[0].Id && x.ProcessStepStatusId == stepStatusId,
                    x => x.Id == processStepData[1].Id && x.ProcessStepStatusId == stepStatusId,
                    x => x.Id == processStepData[2].Id && x.ProcessStepStatusId == stepStatusId);             
        }
        else
        {
            A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
                .MustNotHaveHappened();
        }
    }

    [Fact]
    public async Task ExecuteProcess_NoExecutableSteps_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();
        var processStepData = _fixture.CreateMany<ProcessStepTypeId>().Select(stepTypeId => (Id: Guid.NewGuid(),StepTypeId: stepTypeId)).OrderBy(x => x.StepTypeId).ToImmutableArray();

        A.CallTo(() => _processStepRepository.GetProcessStepData(processId))
            .Returns(processStepData.ToAsyncEnumerable());

        A.CallTo(() => _processTypeExecutor.IsExecutableStepTypeId(A<ProcessStepTypeId>._))
            .Returns(false);

        A.CallTo(() => _processTypeExecutor.InitializeProcess(processId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new IProcessTypeExecutor.InitializationResult(false, null));

        // Act
        var result = await _sut.ExecuteProcess(processId, ProcessTypeId.APPLICATION_CHECKLIST, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(1).And.Contain(false);

        A.CallTo(() => _processTypeExecutor.ExecuteProcessStep(A<ProcessStepTypeId>._,A<IEnumerable<ProcessStepTypeId>>._,A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .MustNotHaveHappened();

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(ProcessStepStatusId.DONE)]
    [InlineData(ProcessStepStatusId.TODO)]
    public async Task ExecuteProcess_NoScheduleOrSkippedSteps_SingleStepTypeWithDuplicates_ReturnsExpected(ProcessStepStatusId stepStatusId)
    {
        // Arrange
        var processId = Guid.NewGuid();
        var stepTypeId = _fixture.Create<ProcessStepTypeId>();
        var processStepData = _fixture.CreateMany<Guid>(3).Select(x => (Id: x, StepTypeId: stepTypeId)).OrderBy(x => x.StepTypeId).ToImmutableArray();

        A.CallTo(() => _processStepRepository.GetProcessStepData(processId))
            .Returns(processStepData.ToAsyncEnumerable());

        A.CallTo(() => _processTypeExecutor.IsExecutableStepTypeId(A<ProcessStepTypeId>._))
            .Returns(true);

        A.CallTo(() => _processTypeExecutor.InitializeProcess(processId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new IProcessTypeExecutor.InitializationResult(false, null));

        A.CallTo(() => _processTypeExecutor.ExecuteProcessStep(A<ProcessStepTypeId>._,A<IEnumerable<ProcessStepTypeId>>._,A<CancellationToken>._))
            .Returns(new IProcessTypeExecutor.StepExecutionResult(false, stepStatusId, null, null));

        var modifiedProcessSteps = new List<ProcessStep>();

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .Invokes((Guid stepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify) =>
            {
                var step = new ProcessStep(stepId, default, default, Guid.Empty, default);
                initialize?.Invoke(step);
                modify(step);
                modifiedProcessSteps.Add(step);
            });

        // Act
        var result = await _sut.ExecuteProcess(processId, ProcessTypeId.APPLICATION_CHECKLIST, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(2).And.ContainInOrder(new [] { false, stepStatusId == ProcessStepStatusId.DONE });

        A.CallTo(() => _processTypeExecutor.ExecuteProcessStep(A<ProcessStepTypeId>._,A<IEnumerable<ProcessStepTypeId>>._,A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .MustNotHaveHappened();

        if (stepStatusId == ProcessStepStatusId.DONE)
        {
            A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
                .MustHaveHappened(processStepData.Length, Times.Exactly);
            modifiedProcessSteps
                .Should().HaveSameCount(processStepData)
                .And.Satisfy(
                    x => x.Id == processStepData[0].Id,
                    x => x.Id == processStepData[1].Id,
                    x => x.Id == processStepData[2].Id)
                .And.Satisfy(
                    x => x.ProcessStepStatusId == stepStatusId,
                    x => x.ProcessStepStatusId == ProcessStepStatusId.DUPLICATE,
                    x => x.ProcessStepStatusId == ProcessStepStatusId.DUPLICATE);
        }
        else
        {
            A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
                .MustNotHaveHappened();
        }

    }

    [Theory]
    [InlineData(ProcessStepStatusId.DONE)]
    [InlineData(ProcessStepStatusId.TODO)]
    public async Task ExecuteProcess_WithScheduledSteps_ReturnsExpected(ProcessStepStatusId stepStatusId)
    {
        // Arrange
        var processId = Guid.NewGuid();
        var processStepData = (Id: Guid.NewGuid(), StepTypeId: _fixture.Create<ProcessStepTypeId>());
        var scheduleStepTypeIds = Enum.GetValues<ProcessStepTypeId>().Where(x => x != processStepData.StepTypeId).Take(3).ToImmutableArray();

        A.CallTo(() => _processStepRepository.GetProcessStepData(processId))
            .Returns(new [] { processStepData }.ToAsyncEnumerable());

        A.CallTo(() => _processTypeExecutor.IsExecutableStepTypeId(A<ProcessStepTypeId>._))
            .Returns(true);

        A.CallTo(() => _processTypeExecutor.InitializeProcess(processId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new IProcessTypeExecutor.InitializationResult(false, null));

        A.CallTo(() => _processTypeExecutor.ExecuteProcessStep(processStepData.StepTypeId,A<IEnumerable<ProcessStepTypeId>>._,A<CancellationToken>._))
            .Returns(new IProcessTypeExecutor.StepExecutionResult(false, stepStatusId, scheduleStepTypeIds, null));

        A.CallTo(() => _processTypeExecutor.ExecuteProcessStep(A<ProcessStepTypeId>.That.Not.IsEqualTo(processStepData.StepTypeId),A<IEnumerable<ProcessStepTypeId>>._,A<CancellationToken>._))
            .Returns(new IProcessTypeExecutor.StepExecutionResult(false, stepStatusId, null, null));

        IEnumerable<ProcessStep>? createdProcessSteps = null;;

        A.CallTo(() => _processStepRepository.CreateProcessStepRange(processId, A<IEnumerable<ProcessStepTypeId>>._))
            .ReturnsLazily((Guid _, IEnumerable<ProcessStepTypeId> stepTypeIds) =>
            {
                createdProcessSteps = stepTypeIds.Select(stepTypeId => new ProcessStep(Guid.NewGuid(), stepTypeId, ProcessStepStatusId.TODO, processId, DateTimeOffset.UtcNow)).ToImmutableList();
                return createdProcessSteps;
            });

        var modifiedProcessSteps = new List<ProcessStep>();

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .Invokes((Guid stepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify) =>
            {
                var step = new ProcessStep(stepId, default, default, Guid.Empty, default);
                initialize?.Invoke(step);
                modify(step);
                modifiedProcessSteps.Add(step);
            });

        // Act
        var result = await _sut.ExecuteProcess(processId, ProcessTypeId.APPLICATION_CHECKLIST, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        result.
            Should().HaveCount(scheduleStepTypeIds.Length + 2)
            .And.ContainInOrder(
                stepStatusId == ProcessStepStatusId.DONE
                    ? new [] { false, true, true, true, true }
                    : new [] { false, true, false, false, false });

        A.CallTo(() => _processTypeExecutor.ExecuteProcessStep(A<ProcessStepTypeId>._,A<IEnumerable<ProcessStepTypeId>>._,A<CancellationToken>._))
            .MustHaveHappened(scheduleStepTypeIds.Length + 1, Times.Exactly);

        A.CallTo(() => _processStepRepository.CreateProcessStepRange(processId, A<IEnumerable<ProcessStepTypeId>>.That.IsSameSequenceAs(scheduleStepTypeIds)))
            .MustHaveHappenedOnceExactly();

        createdProcessSteps
            .Should().NotBeNull()
            .And.HaveSameCount(scheduleStepTypeIds)
            .And.Satisfy(
                x => x.ProcessStepTypeId == scheduleStepTypeIds[0] && x.ProcessStepStatusId == ProcessStepStatusId.TODO,
                x => x.ProcessStepTypeId == scheduleStepTypeIds[1] && x.ProcessStepStatusId == ProcessStepStatusId.TODO,
                x => x.ProcessStepTypeId == scheduleStepTypeIds[2] && x.ProcessStepStatusId == ProcessStepStatusId.TODO);

        if (stepStatusId == ProcessStepStatusId.DONE)
        {
            A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
                .MustHaveHappened(scheduleStepTypeIds.Length + 1, Times.Exactly);
            modifiedProcessSteps
                .Should().HaveCount(scheduleStepTypeIds.Length + 1)
                .And.Satisfy(
                    x => x.Id == processStepData.Id && x.ProcessStepStatusId == ProcessStepStatusId.DONE,
                    x => x.Id == createdProcessSteps!.ElementAt(0).Id && x.ProcessStepStatusId == ProcessStepStatusId.DONE,
                    x => x.Id == createdProcessSteps!.ElementAt(1).Id && x.ProcessStepStatusId == ProcessStepStatusId.DONE,
                    x => x.Id == createdProcessSteps!.ElementAt(2).Id && x.ProcessStepStatusId == ProcessStepStatusId.DONE);
        }
        else
        {
            A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
                .MustNotHaveHappened();
        }
    }

    [Theory]
    [InlineData(ProcessStepStatusId.DONE)]
    [InlineData(ProcessStepStatusId.TODO)]
    public async Task ExecuteProcess_WithDuplicateScheduledSteps_ReturnsExpected(ProcessStepStatusId stepStatusId)
    {
        // Arrange
        var processId = Guid.NewGuid();
        var stepTypeId = _fixture.Create<ProcessStepTypeId>();
        var processStepData = (Id: Guid.NewGuid(), StepTypeId: stepTypeId );
        var scheduleStepTypeIds = Enumerable.Repeat(stepTypeId, 3);

        A.CallTo(() => _processStepRepository.GetProcessStepData(processId))
            .Returns(new [] { processStepData }.ToAsyncEnumerable());

        A.CallTo(() => _processTypeExecutor.IsExecutableStepTypeId(A<ProcessStepTypeId>._))
            .Returns(true);

        A.CallTo(() => _processTypeExecutor.InitializeProcess(processId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new IProcessTypeExecutor.InitializationResult(false, null));

        A.CallTo(() => _processTypeExecutor.ExecuteProcessStep(stepTypeId,A<IEnumerable<ProcessStepTypeId>>._,A<CancellationToken>._))
            .Returns(new IProcessTypeExecutor.StepExecutionResult(false, stepStatusId, scheduleStepTypeIds, null))
            .Once()
            .Then
            .Returns(new IProcessTypeExecutor.StepExecutionResult(false, stepStatusId, null, null));

        IEnumerable<ProcessStep>? createdProcessSteps = null;;

        A.CallTo(() => _processStepRepository.CreateProcessStepRange(processId, A<IEnumerable<ProcessStepTypeId>>._))
            .ReturnsLazily((Guid _, IEnumerable<ProcessStepTypeId> stepTypeIds) =>
            {
                createdProcessSteps = stepTypeIds.Select(stepTypeId => new ProcessStep(Guid.NewGuid(), stepTypeId, ProcessStepStatusId.TODO, processId, DateTimeOffset.UtcNow)).ToImmutableList();
                return createdProcessSteps;
            });

        var modifiedProcessSteps = new List<ProcessStep>();

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .Invokes((Guid stepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify) =>
            {
                var step = new ProcessStep(stepId, default, default, Guid.Empty, default);
                initialize?.Invoke(step);
                modify(step);
                modifiedProcessSteps.Add(step);
            });

        // Act
        var result = await _sut.ExecuteProcess(processId, ProcessTypeId.APPLICATION_CHECKLIST, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        if (stepStatusId == ProcessStepStatusId.DONE)
        {
            result.Should().HaveCount(3).And.ContainInOrder(new [] { false, true, true });

            A.CallTo(() => _processStepRepository.CreateProcessStepRange(processId, A<IEnumerable<ProcessStepTypeId>>._))
                .MustHaveHappenedOnceExactly();

            createdProcessSteps
                .Should().NotBeNull()
                .And.HaveCount(1)
                .And.Satisfy(
                    x => x.ProcessStepTypeId == stepTypeId && x.ProcessStepStatusId == ProcessStepStatusId.TODO);

            A.CallTo(() => _processTypeExecutor.ExecuteProcessStep(A<ProcessStepTypeId>._,A<IEnumerable<ProcessStepTypeId>>._,A<CancellationToken>._))
                .MustHaveHappened(2, Times.Exactly);
            A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
                .MustHaveHappened(2, Times.Exactly);

            modifiedProcessSteps
                .Should().HaveCount(2)
                .And.Satisfy(
                    x => x.Id == processStepData.Id && x.ProcessStepStatusId == ProcessStepStatusId.DONE,
                    x => x.Id == createdProcessSteps!.ElementAt(0).Id && x.ProcessStepStatusId == ProcessStepStatusId.DONE);
        }
        else
        {
            A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
                .MustNotHaveHappened();
            A.CallTo(() => _processStepRepository.CreateProcessStepRange(processId, A<IEnumerable<ProcessStepTypeId>>._))
                .MustNotHaveHappened();
        }
    }

    [Theory]
    [InlineData(ProcessStepStatusId.DONE)]
    [InlineData(ProcessStepStatusId.TODO)]
    public async Task ExecuteProcess_WithSkippedSteps_ReturnsExpected(ProcessStepStatusId stepStatusId)
    {
        // Arrange
        var processId = Guid.NewGuid();
        var processStepData = _fixture.CreateMany<ProcessStepTypeId>(3).Select(stepTypeId => (Id: Guid.NewGuid(), StepTypeId: stepTypeId)).OrderBy(x => x.StepTypeId).ToImmutableArray();
        var skipStepTypeIds = processStepData.Skip(1).Select(x => x.StepTypeId).ToImmutableArray();

        A.CallTo(() => _processStepRepository.GetProcessStepData(processId))
            .Returns(processStepData.ToAsyncEnumerable());

        A.CallTo(() => _processTypeExecutor.IsExecutableStepTypeId(A<ProcessStepTypeId>._))
            .Returns(true);

        A.CallTo(() => _processTypeExecutor.InitializeProcess(processId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new IProcessTypeExecutor.InitializationResult(false, null));

        A.CallTo(() => _processTypeExecutor.ExecuteProcessStep(A<ProcessStepTypeId>._,A<IEnumerable<ProcessStepTypeId>>._,A<CancellationToken>._))
            .Returns(new IProcessTypeExecutor.StepExecutionResult(false, stepStatusId, null, skipStepTypeIds));

        var modifiedProcessSteps = new List<ProcessStep>();

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .Invokes((Guid stepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify) =>
            {
                var step = new ProcessStep(stepId, default, default, Guid.Empty, default);
                initialize?.Invoke(step);
                modify(step);
                modifiedProcessSteps.Add(step);
            });

        // Act
        var result = await _sut.ExecuteProcess(processId, ProcessTypeId.APPLICATION_CHECKLIST, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(2).And.ContainInOrder(new [] { false, true });

        A.CallTo(() => _processTypeExecutor.ExecuteProcessStep(A<ProcessStepTypeId>._,A<IEnumerable<ProcessStepTypeId>>._,A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .MustNotHaveHappened();

        if (stepStatusId == ProcessStepStatusId.DONE)
        {
            A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
                .MustHaveHappened(skipStepTypeIds.Length + 1, Times.Exactly);
            modifiedProcessSteps
                .Should().HaveCount(skipStepTypeIds.Length + 1)
                .And.Satisfy(
                    x => x.Id == processStepData[0].Id && x.ProcessStepStatusId == ProcessStepStatusId.DONE,
                    x => x.Id == processStepData[1].Id && x.ProcessStepStatusId == ProcessStepStatusId.SKIPPED,
                    x => x.Id == processStepData[2].Id && x.ProcessStepStatusId == ProcessStepStatusId.SKIPPED);
        }
        else
        {
            A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
                .MustHaveHappened(skipStepTypeIds.Length, Times.Exactly);
            modifiedProcessSteps
                .Should().HaveCount(skipStepTypeIds.Length)
                .And.Satisfy(
                    x => x.Id == processStepData[1].Id && x.ProcessStepStatusId == ProcessStepStatusId.SKIPPED,
                    x => x.Id == processStepData[2].Id && x.ProcessStepStatusId == ProcessStepStatusId.SKIPPED);
        }
    }

    [Fact]
    public async Task ExecuteProcess_ProcessThrowsTestException_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();
        var processStepData = _fixture.CreateMany<ProcessStepTypeId>(3).Select(stepTypeId => (Id: Guid.NewGuid(), StepTypeId: stepTypeId)).OrderBy(x => x.StepTypeId).ToImmutableArray();
        var error = _fixture.Create<TestException>();

        A.CallTo(() => _processStepRepository.GetProcessStepData(processId))
            .Returns(processStepData.ToAsyncEnumerable());

        A.CallTo(() => _processTypeExecutor.IsExecutableStepTypeId(A<ProcessStepTypeId>._))
            .Returns(true);

        A.CallTo(() => _processTypeExecutor.InitializeProcess(processId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new IProcessTypeExecutor.InitializationResult(false, null));

        A.CallTo(() => _processTypeExecutor.ExecuteProcessStep(A<ProcessStepTypeId>._,A<IEnumerable<ProcessStepTypeId>>._,A<CancellationToken>._))
            .Throws(error);

        var modifiedProcessSteps = new List<ProcessStep>();

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .Invokes((Guid stepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify) =>
            {
                var step = new ProcessStep(stepId, default, default, Guid.Empty, default);
                initialize?.Invoke(step);
                modify(step);
                modifiedProcessSteps.Add(step);
            });

        // Act
        var result = await _sut.ExecuteProcess(processId, ProcessTypeId.APPLICATION_CHECKLIST, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(processStepData.Length + 1).And.ContainInOrder(new [] { false }.Concat(Enumerable.Repeat<bool>(true, processStepData.Length)));

        A.CallTo(() => _processTypeExecutor.ExecuteProcessStep(A<ProcessStepTypeId>._,A<IEnumerable<ProcessStepTypeId>>._,A<CancellationToken>._))
            .MustHaveHappened(processStepData.Length, Times.Exactly);

        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .MustNotHaveHappened();

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .MustHaveHappened(processStepData.Length, Times.Exactly);

        modifiedProcessSteps
            .Should().HaveCount(processStepData.Length)
            .And.Satisfy(
                x => x.Id == processStepData[0].Id && x.ProcessStepStatusId == ProcessStepStatusId.FAILED,
                x => x.Id == processStepData[1].Id && x.ProcessStepStatusId == ProcessStepStatusId.FAILED,
                x => x.Id == processStepData[2].Id && x.ProcessStepStatusId == ProcessStepStatusId.FAILED);
    }

    [Fact]
    public async Task ExecuteProcess_ProcessThrowsSystemException_Throws()
    {
        // Arrange
        var processId = Guid.NewGuid();
        var processStepData = _fixture.CreateMany<ProcessStepTypeId>().Select(stepTypeId => (Id: Guid.NewGuid(),StepTypeId: stepTypeId)).OrderBy(x => x.StepTypeId).ToImmutableArray();
        var error = _fixture.Create<SystemException>();

        A.CallTo(() => _processStepRepository.GetProcessStepData(processId))
            .Returns(processStepData.ToAsyncEnumerable());

        A.CallTo(() => _processTypeExecutor.IsExecutableStepTypeId(A<ProcessStepTypeId>._))
            .Returns(true);

        A.CallTo(() => _processTypeExecutor.InitializeProcess(processId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new IProcessTypeExecutor.InitializationResult(false, null));

        A.CallTo(() => _processTypeExecutor.ExecuteProcessStep(A<ProcessStepTypeId>._,A<IEnumerable<ProcessStepTypeId>>._,A<CancellationToken>._))
            .Throws(error);

        var Act = async () => await _sut.ExecuteProcess(processId, ProcessTypeId.APPLICATION_CHECKLIST, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Act
        var result = await Assert.ThrowsAsync<SystemException>(Act).ConfigureAwait(false);

        // Assert
        result.Message.Should().Be(error.Message);
        
        A.CallTo(() => _processTypeExecutor.ExecuteProcessStep(A<ProcessStepTypeId>._,A<IEnumerable<ProcessStepTypeId>>._,A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .MustNotHaveHappened();

        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .MustNotHaveHappened();
    }

    #endregion

    [Serializable]
    public class TestException : Exception
    {
        public TestException() { }
        public TestException(string message) : base(message) { }
        public TestException(string message, Exception inner) : base(message, inner) { }
        protected TestException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
