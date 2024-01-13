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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library.Tests;

public class ChecklistServiceTests
{
    private readonly IFixture _fixture;

    private readonly IApplicationChecklistRepository _applicationChecklistRepository;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly IPortalRepositories _portalRepositories;

    private readonly IApplicationChecklistService _service;

    public ChecklistServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _applicationChecklistRepository = A.Fake<IApplicationChecklistRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>()).Returns(_applicationChecklistRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);

        _service = new ApplicationChecklistService(_portalRepositories);
    }

    #region VerifyChecklistEntryAndProcessSteps

    [Fact]
    public async Task VerifyChecklistEntryAndProcessSteps()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var process = new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid());
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(Enum.GetValues<ApplicationChecklistEntryStatusId>().Length - 1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var entryTypeIds = _fixture.CreateMany<ApplicationChecklistEntryTypeId>(Enum.GetValues<ApplicationChecklistEntryTypeId>().Length - 2).ToImmutableArray();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(Enum.GetValues<ProcessStepTypeId>().Length - 2).ToImmutableArray();
        var allEntryTypeIds = entryTypeIds.Append(entryTypeId).Distinct().ToImmutableArray();
        var allProcessStepTypeIds = processStepTypeIds.Append(processStepTypeId).Distinct().ToImmutableArray();
        var comment = "Test Purpose";

        IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId, string?)>? checklistData = null;
        IEnumerable<ProcessStep>? processSteps = null;

        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .ReturnsLazily((Guid appId, IEnumerable<ApplicationChecklistEntryTypeId> entryTypes, IEnumerable<ProcessStepTypeId> processStepTypes) =>
            {
                checklistData = entryTypes.Zip(ProduceEntryStatusIds(entryStatusIds), (typeId, statusId) => (typeId, statusId, comment)).ToImmutableArray();
                processSteps = processStepTypes.Select(typeId => new ProcessStep(Guid.NewGuid(), typeId, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow)).ToImmutableArray();
                return applicationId == appId ?
                    new VerifyChecklistData(
                    true,
                    process,
                    checklistData,
                    processSteps) :
                    null;
            });

        // Act
        var result = await _service.VerifyChecklistEntryAndProcessSteps(applicationId, entryTypeId, entryStatusIds, processStepTypeId, entryTypeIds, processStepTypeIds).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.ApplicationId.Should().Be(applicationId);
        result.ProcessStepId.Should().NotBeEmpty();
        var processStep = processSteps?.SingleOrDefault(step => step.Id == result.ProcessStepId);
        processStep.Should().NotBeNull();
        processStep!.ProcessStepTypeId.Should().Be(processStepTypeId);
        processStep.ProcessStepStatusId.Should().Be(ProcessStepStatusId.TODO);
        result.EntryTypeId.Should().Be(entryTypeId);
        result.Checklist.Keys.Should().HaveSameCount(allEntryTypeIds);
        result.Checklist.Should().ContainKeys(entryTypeIds);
        result.ProcessSteps.Select(step => (step.ProcessStepTypeId, step.ProcessStepStatusId, step.ProcessId))
            .Should().HaveSameCount(allProcessStepTypeIds)
            .And.Contain(allProcessStepTypeIds.Select(stepTypeId => (stepTypeId, ProcessStepStatusId.TODO, process.Id)));
    }

    [Fact]
    public async Task VerifyChecklistEntry_InvalidProcessId_Throws()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(Enum.GetValues<ApplicationChecklistEntryStatusId>().Length - 1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var entryTypeIds = _fixture.CreateMany<ApplicationChecklistEntryTypeId>(Enum.GetValues<ApplicationChecklistEntryTypeId>().Length - 2).ToImmutableArray();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(Enum.GetValues<ProcessStepTypeId>().Length - 2).ToImmutableArray();

        // (bool IsValidApplicationId, bool IsSubmitted, Guid? ProcessId, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)
        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new VerifyChecklistData(true, null, null, null));

        var Act = () => _service.VerifyChecklistEntryAndProcessSteps(applicationId, entryTypeId, entryStatusIds, processStepTypeId, entryTypeIds, processStepTypeIds);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Assert
        result.Message.Should().Be($"application {applicationId} is not associated with a checklist-process");
    }

    [Fact]
    public async Task VerifyChecklistEntry_LockedProcess_Throws()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var lockExpiryDate = DateTimeOffset.UtcNow;
        var process = new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid()) { LockExpiryDate = lockExpiryDate };
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(Enum.GetValues<ApplicationChecklistEntryStatusId>().Length - 1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var entryTypeIds = _fixture.CreateMany<ApplicationChecklistEntryTypeId>(Enum.GetValues<ApplicationChecklistEntryTypeId>().Length - 2).ToImmutableArray();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(Enum.GetValues<ProcessStepTypeId>().Length - 2).ToImmutableArray();

        // (bool IsValidApplicationId, bool IsSubmitted, Guid? ProcessId, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)
        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new VerifyChecklistData(true, process, null, null));

        var Act = () => _service.VerifyChecklistEntryAndProcessSteps(applicationId, entryTypeId, entryStatusIds, processStepTypeId, entryTypeIds, processStepTypeIds);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Assert
        result.Message.Should().Be($"checklist-process {process.Id} of {applicationId} is locked, lock expiry is set to {lockExpiryDate}");
    }

    [Fact]
    public async Task VerifyChecklistEntry_InvalidApplicationId_Throws()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(Enum.GetValues<ApplicationChecklistEntryStatusId>().Length - 1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var entryTypeIds = _fixture.CreateMany<ApplicationChecklistEntryTypeId>(Enum.GetValues<ApplicationChecklistEntryTypeId>().Length - 2).ToImmutableArray();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(Enum.GetValues<ProcessStepTypeId>().Length - 2).ToImmutableArray();

        // (bool IsValidApplicationId, bool IsSubmitted, Guid? ProcessId, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)
        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns((VerifyChecklistData?)null);

        var Act = () => _service.VerifyChecklistEntryAndProcessSteps(applicationId, entryTypeId, entryStatusIds, processStepTypeId, entryTypeIds, processStepTypeIds);

        // Act
        var result = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

        // Assert
        result.Message.Should().Be($"application {applicationId} does not exist");
    }

    [Fact]
    public async Task VerifyChecklistEntry_InvalidApplicationStatus_Throws()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var process = new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid());
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(Enum.GetValues<ApplicationChecklistEntryStatusId>().Length - 1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var entryTypeIds = _fixture.CreateMany<ApplicationChecklistEntryTypeId>(Enum.GetValues<ApplicationChecklistEntryTypeId>().Length - 2).ToImmutableArray();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(Enum.GetValues<ProcessStepTypeId>().Length - 2).ToImmutableArray();

        // (bool IsValidApplicationId, bool IsSubmitted, Guid? ProcessId, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)
        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new VerifyChecklistData(false, process, null, null));

        var Act = () => _service.VerifyChecklistEntryAndProcessSteps(applicationId, entryTypeId, entryStatusIds, processStepTypeId, entryTypeIds, processStepTypeIds);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Assert
        result.Message.Should().Be($"application {applicationId} is not in status SUBMITTED");
    }

    [Fact]
    public async Task VerifyChecklistEntry_UnexpectedEntryData_Throws()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var process = new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid());
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        IEnumerable<ApplicationChecklistEntryTypeId>? entryTypeIds = null;
        IEnumerable<ProcessStepTypeId>? processStepTypeIds = null;

        var entryData = Enum.GetValues<ApplicationChecklistEntryTypeId>().Except(new[] { entryTypeId }).Select(entryTypeId => (entryTypeId, entryStatusIds.First(), "Test Purpose")).ToImmutableArray();
        var processSteps = new ProcessStep[] { new(Guid.NewGuid(), processStepTypeId, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow) };

        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new VerifyChecklistData(true, process, entryData, processSteps));

        var Act = () => _service.VerifyChecklistEntryAndProcessSteps(applicationId, entryTypeId, entryStatusIds, processStepTypeId, entryTypeIds, processStepTypeIds);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Assert
        result.Message.Should().Be($"application {applicationId} does not have a checklist entry for {entryTypeId} in status {string.Join(", ", entryStatusIds)}");
    }

    [Fact]
    public async Task VerifyChecklistEntry_UnexpectedQueryResult_Throws()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var process = new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid());
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        IEnumerable<ApplicationChecklistEntryTypeId>? entryTypeIds = null;
        IEnumerable<ProcessStepTypeId>? processStepTypeIds = null;

        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new VerifyChecklistData(true, process, null, null));

        var Act = () => _service.VerifyChecklistEntryAndProcessSteps(applicationId, entryTypeId, entryStatusIds, processStepTypeId, entryTypeIds, processStepTypeIds);

        // Act
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);

        // Assert
        result.Message.Should().Be($"checklist or processSteps should never be null here");
    }

    [Fact]
    public async Task VerifyChecklistEntry_UnexpectedEntryStatusData_Throws()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var process = new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid());
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        IEnumerable<ApplicationChecklistEntryTypeId>? entryTypeIds = null;
        IEnumerable<ProcessStepTypeId>? processStepTypeIds = null;

        var entryData = new[] { (entryTypeId, Enum.GetValues<ApplicationChecklistEntryStatusId>().Except(entryStatusIds).First(), "Test Purpose") };
        var processSteps = new ProcessStep[] { new(Guid.NewGuid(), processStepTypeId, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow) };

        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new VerifyChecklistData(true, process, entryData!, processSteps));

        var Act = () => _service.VerifyChecklistEntryAndProcessSteps(applicationId, entryTypeId, entryStatusIds, processStepTypeId, entryTypeIds, processStepTypeIds);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Assert
        result.Message.Should().Be($"application {applicationId} does not have a checklist entry for {entryTypeId} in status {string.Join(", ", entryStatusIds)}");
    }

    [Fact]
    public async Task VerifyChecklistEntry_UnexpectedProcessStepData_Throws()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var process = new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid());
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        IEnumerable<ApplicationChecklistEntryTypeId>? entryTypeIds = null;
        IEnumerable<ProcessStepTypeId>? processStepTypeIds = null;

        var entryData = new[] { (entryTypeId, entryStatusIds.First(), "Test Purpose") };
        var processSteps = new ProcessStep[] { new(Guid.NewGuid(), Enum.GetValues<ProcessStepTypeId>().Except(new[] { processStepTypeId }).First(), ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow) };

        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new VerifyChecklistData(true, process, entryData!, processSteps));

        var Act = () => _service.VerifyChecklistEntryAndProcessSteps(applicationId, entryTypeId, entryStatusIds, processStepTypeId, entryTypeIds, processStepTypeIds);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Assert
        result.Message.Should().Be($"application {applicationId} checklist entry {entryTypeId}, process step {processStepTypeId} is not eligible to run");
    }

    [Fact]
    public async Task VerifyChecklistEntry_UnexpectedProcessStepStatus_Throws()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var process = new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid());
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        IEnumerable<ApplicationChecklistEntryTypeId>? entryTypeIds = null;
        IEnumerable<ProcessStepTypeId>? processStepTypeIds = null;

        var entryData = new[] { (entryTypeId, entryStatusIds.First(), "Test Purpose") };
        var processSteps = new ProcessStep[] { new(Guid.NewGuid(), processStepTypeId, ProcessStepStatusId.SKIPPED, process.Id, DateTimeOffset.UtcNow) };

        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new VerifyChecklistData(true, process, entryData!, processSteps));

        var Act = () => _service.VerifyChecklistEntryAndProcessSteps(applicationId, entryTypeId, entryStatusIds, processStepTypeId, entryTypeIds, processStepTypeIds);

        // Act
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);

        // Assert
        result.Message.Should().Be($"processSteps should never have other status than TODO here");
    }

    #endregion

    #region RequestLock

    [Fact]
    public void RequestLock_UnlockedProcess_Success()
    {
        // Arrange
        var version = Guid.NewGuid();
        var process = new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, version);
        var context = new IApplicationChecklistService.ManualChecklistProcessStepData(
            Guid.NewGuid(),
            process,
            Guid.NewGuid(),
            _fixture.Create<ApplicationChecklistEntryTypeId>(),
            _fixture.Create<Dictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>>().ToImmutableDictionary(),
            new ProcessStep[] { new(Guid.NewGuid(), _fixture.Create<ProcessStepTypeId>(), ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow) }
        );
        var lockExpiryDate = _fixture.Create<DateTimeOffset>();

        // Act
        _service.RequestLock(context, lockExpiryDate);

        // Assert
        A.CallTo(() => _portalRepositories.Attach(A<Process>._, null)).MustHaveHappenedOnceExactly();
        context.Process.Should().Match<Process>(x => x.Version != version && x.LockExpiryDate == lockExpiryDate);
    }

    [Fact]
    public void RequestLock_LockedProcess_Throws()
    {
        // Arrange
        var version = Guid.NewGuid();
        var lockExpiryDate = _fixture.Create<DateTimeOffset>();
        var process = new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, version) { LockExpiryDate = lockExpiryDate };
        var context = new IApplicationChecklistService.ManualChecklistProcessStepData(
            Guid.NewGuid(),
            process,
            Guid.NewGuid(),
            _fixture.Create<ApplicationChecklistEntryTypeId>(),
            _fixture.Create<Dictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>>().ToImmutableDictionary(),
            new ProcessStep[] { new(Guid.NewGuid(), _fixture.Create<ProcessStepTypeId>(), ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow) }
        );

        // Act
        var result = Assert.Throws<UnexpectedConditionException>(() => _service.RequestLock(context, _fixture.Create<DateTimeOffset>()));

        // Assert
        result.Message.Should().Be("process TryLock should never fail here");
        A.CallTo(() => _portalRepositories.Attach(A<Process>._, null)).MustHaveHappenedOnceExactly();
        context.Process.Should().Match<Process>(x => x.Version == version && x.LockExpiryDate == lockExpiryDate);
    }

    #endregion

    #region FinalizeChecklistEntryAndProcessSteps

    [Fact]
    public void FinalizeChecklistEntryAndProcessSteps_ReturnsExpected()
    {
        // Arrange
        var process = new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid());
        var context = new IApplicationChecklistService.ManualChecklistProcessStepData(
            Guid.NewGuid(),
            process,
            Guid.NewGuid(),
            _fixture.Create<ApplicationChecklistEntryTypeId>(),
            _fixture.Create<Dictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>>().ToImmutableDictionary(),
            new ProcessStep[] { new(Guid.NewGuid(), _fixture.Create<ProcessStepTypeId>(), ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow) }
        );

        ApplicationChecklistEntry? modifiedChecklistEntry = null;

        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._))
            .Invokes((Guid applicationId, ApplicationChecklistEntryTypeId entryTypeId, Action<ApplicationChecklistEntry> initialize, Action<ApplicationChecklistEntry> modify) =>
            {
                modifiedChecklistEntry = new ApplicationChecklistEntry(applicationId, entryTypeId, default, default);
                modify(modifiedChecklistEntry);
            });

        ProcessStep? modifiedProcessStep = null;

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .Invokes((Guid processStepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify) =>
            {
                modifiedProcessStep = new ProcessStep(processStepId, default, default, default, default);
                initialize?.Invoke(modifiedProcessStep);
                modify(modifiedProcessStep);
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
        _service.FinalizeChecklistEntryAndProcessSteps(
            context,
            null,
            entry => { entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE; },
            Enum.GetValues<ProcessStepTypeId>());

        // Assert
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId, ProcessStepStatusId, Guid)>>._))
            .MustHaveHappenedOnceExactly();

        modifiedChecklistEntry.Should().NotBeNull();
        modifiedChecklistEntry!.ApplicationChecklistEntryTypeId.Should().Be(context.EntryTypeId);
        modifiedChecklistEntry!.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);

        modifiedProcessStep.Should().NotBeNull();
        modifiedProcessStep!.Id.Should().Be(context.ProcessStepId);
        modifiedProcessStep.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);

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
    public void FinalizeChecklistEntry_NoModifyEnty_ReturnsExpected()
    {
        // Arrange
        var process = new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid());
        var context = new IApplicationChecklistService.ManualChecklistProcessStepData(
            Guid.NewGuid(),
            process,
            Guid.NewGuid(),
            _fixture.Create<ApplicationChecklistEntryTypeId>(),
            _fixture.Create<Dictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>>().ToImmutableDictionary(),
            new ProcessStep[] { new(Guid.NewGuid(), _fixture.Create<ProcessStepTypeId>(), ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow) }
        );

        ProcessStep? modifiedProcessStep = null;

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .Invokes((Guid processStepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify) =>
            {
                modifiedProcessStep = new ProcessStep(processStepId, default, default, default, default);
                initialize?.Invoke(modifiedProcessStep);
                modify(modifiedProcessStep);
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
        _service.FinalizeChecklistEntryAndProcessSteps(
            context,
            null,
            null,
            Enum.GetValues<ProcessStepTypeId>());

        // Assert
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId, ProcessStepStatusId, Guid)>>._))
            .MustHaveHappenedOnceExactly();

        modifiedProcessStep.Should().NotBeNull();
        modifiedProcessStep!.Id.Should().Be(context.ProcessStepId);
        modifiedProcessStep.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);

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
    public void FinalizeChecklistEntryNullProcessSteps_ReturnsExpected()
    {
        // Arrange
        var process = new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid());
        var context = new IApplicationChecklistService.ManualChecklistProcessStepData(
            Guid.NewGuid(),
            process,
            Guid.NewGuid(),
            _fixture.Create<ApplicationChecklistEntryTypeId>(),
            _fixture.Create<Dictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>>().ToImmutableDictionary(),
            new ProcessStep[] { new(Guid.NewGuid(), _fixture.Create<ProcessStepTypeId>(), ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow) }
        );

        ApplicationChecklistEntry? modifiedChecklistEntry = null;

        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._))
            .Invokes((Guid applicationId, ApplicationChecklistEntryTypeId entryTypeId, Action<ApplicationChecklistEntry> initialize, Action<ApplicationChecklistEntry> modify) =>
            {
                modifiedChecklistEntry = new ApplicationChecklistEntry(applicationId, entryTypeId, default, default);
                modify(modifiedChecklistEntry);
            });

        ProcessStep? modifiedProcessStep = null;

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .Invokes((Guid processStepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify) =>
            {
                modifiedProcessStep = new ProcessStep(processStepId, default, default, default, default);
                initialize?.Invoke(modifiedProcessStep);
                modify(modifiedProcessStep);
            });

        // Act
        _service.FinalizeChecklistEntryAndProcessSteps(
            context,
            null,
            entry => { entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE; },
            null);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId, ProcessStepStatusId, Guid)>>._))
            .MustNotHaveHappened();

        modifiedChecklistEntry.Should().NotBeNull();
        modifiedChecklistEntry!.ApplicationChecklistEntryTypeId.Should().Be(context.EntryTypeId);
        modifiedChecklistEntry!.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);

        modifiedProcessStep.Should().NotBeNull();
        modifiedProcessStep!.Id.Should().Be(context.ProcessStepId);
        modifiedProcessStep.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
    }

    #endregion

    #region SkipProcessSteps

    [Fact]
    public void SkipProcessSteps()
    {
        // Arrange
        var process = new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid());
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(3);
        var processSteps = _fixture.CreateMany<ProcessStepTypeId>(100).Select(stepTypeId => new ProcessStep(Guid.NewGuid(), stepTypeId, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow)).ToImmutableArray();

        var context = new IApplicationChecklistService.ManualChecklistProcessStepData(
            Guid.NewGuid(),
            process,
            Guid.NewGuid(),
            _fixture.Create<ApplicationChecklistEntryTypeId>(),
            _fixture.Create<Dictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>>().ToImmutableDictionary(),
            processSteps
        );

        var modifiedProcessSteps = new List<ProcessStep>();

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .Invokes((Guid processStepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify) =>
            {
                var step = new ProcessStep(processStepId, default, default, default, default);
                initialize?.Invoke(step);
                modify(step);
                modifiedProcessSteps.Add(step);
            });

        // Act
        _service.SkipProcessSteps(context, processStepTypeIds);

        // Assert
        var eligibleSteps = processSteps.Where(step => processStepTypeIds.Contains(step.ProcessStepTypeId)).ToImmutableArray();

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .MustHaveHappened(eligibleSteps.Length, Times.Exactly);

        modifiedProcessSteps.Should().HaveCount(eligibleSteps.Length);
        modifiedProcessSteps.Where(step => step.ProcessStepStatusId == ProcessStepStatusId.SKIPPED).Should().HaveCount(3);
        modifiedProcessSteps.Where(step => step.ProcessStepStatusId == ProcessStepStatusId.DUPLICATE).Should().HaveCount(eligibleSteps.Length - 3);
        var modifiedWithType = modifiedProcessSteps.Join(
            processSteps,
            modified => modified.Id,
            step => step.Id,
            (modified, step) => (step.ProcessStepTypeId, modified.ProcessStepStatusId)).ToImmutableArray();
        modifiedWithType.Length.Should().Be(eligibleSteps.Length);
        modifiedWithType.Should().AllSatisfy(step => processStepTypeIds.Should().Contain(step.ProcessStepTypeId));
    }

    #endregion

    #region Setup

    private static IEnumerable<ApplicationChecklistEntryStatusId> ProduceEntryStatusIds(IEnumerable<ApplicationChecklistEntryStatusId> statusIds)
    {
        while (true)
        {
            foreach (var statusId in statusIds)
            {
                yield return statusId;
            }
        }
    }

    #endregion

    #region HandleServiceErrorAsync

    [Fact]
    public async Task HandleServiceErrorAsync_WithServiceException_OnlyCommentAdded()
    {
        // Arrange
        var entity = new ApplicationChecklistEntry(Guid.NewGuid(), ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var ex = new ServiceException("Test error");

        // Act
        var result = await _service.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.ModifyChecklistEntry.Should().NotBeNull();
        result.ModifyChecklistEntry!.Invoke(entity);
        entity.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.FAILED);
        entity.Comment.Should().Be("Test error");
        result.ScheduleStepTypeIds.Should().NotBeNull();
        result.ScheduleStepTypeIds.Should().ContainSingle().And.Match(x => x.Single() == ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE);
        result.Modified.Should().BeTrue();
    }

    [Fact]
    public async Task HandleServiceErrorAsync_WithHttpRequestException_OnlyCommentAdded()
    {
        // Arrange
        var entity = new ApplicationChecklistEntry(Guid.NewGuid(), ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var ex = new ServiceException("Test error", true);

        // Act
        var result = await _service.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.ModifyChecklistEntry.Should().NotBeNull();
        result.ModifyChecklistEntry!.Invoke(entity);
        entity.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.TO_DO);
        entity.Comment.Should().Be("Test error");
        result.ScheduleStepTypeIds.Should().BeNull();
        result.Modified.Should().BeTrue();
    }

    #endregion
}
