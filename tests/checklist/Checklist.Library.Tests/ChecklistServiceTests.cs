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

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.Tests;

public class ChecklistServiceTests
{
    private readonly IFixture _fixture;
    
    private readonly IApplicationChecklistRepository _applicationChecklistRepository;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly IPortalRepositories _portalRepositories;

    private readonly IChecklistService _service;

    public ChecklistServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _applicationChecklistRepository = A.Fake<IApplicationChecklistRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>()).Returns(_applicationChecklistRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);

        _service = new ChecklistService(_portalRepositories);
    }
    
    #region VerifyChecklistEntryAndProcessSteps
    
    [Fact]
    public async Task VerifyChecklistEntryAndProcessSteps()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(Enum.GetValues<ApplicationChecklistEntryStatusId>().Length-1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var entryTypeIds = _fixture.CreateMany<ApplicationChecklistEntryTypeId>(Enum.GetValues<ApplicationChecklistEntryTypeId>().Length-2).ToImmutableArray();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(Enum.GetValues<ProcessStepTypeId>().Length-2).ToImmutableArray();

        IEnumerable<(ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId)>? checklistData = null;
        IEnumerable<ProcessStep>? processSteps = null;

        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .ReturnsLazily((Guid appId, IEnumerable<ApplicationChecklistEntryTypeId> entryTypes, IEnumerable<ProcessStepTypeId> processStepTypes) => {
                checklistData = entryTypes.Append(entryTypeId).Distinct().Zip(ProduceEntryStatusIds(entryStatusIds), (typeId, statusId) => (typeId,statusId)).ToImmutableArray();
                processSteps = processStepTypes.Select(typeId => new ProcessStep(Guid.NewGuid(), typeId, ProcessStepStatusId.TODO, DateTimeOffset.UtcNow)).ToImmutableArray();
                return (
                    applicationId == appId,
                    true,
                    checklistData,
                    processSteps);
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
        result.Checklist.Should().ContainKey(entryTypeId);
        result.Checklist.Should().ContainKeys(entryTypeIds);
        result.ProcessSteps.Select(step => step.ProcessStepTypeId).Should().Contain(processStepTypeIds);
        result.ProcessSteps.Select(step => step.ProcessStepStatusId).Should().AllSatisfy(statusId => statusId.Should().Be(ProcessStepStatusId.TODO));
    }

    [Fact]
    public async Task VerifyChecklistEntry_InvalidApplicationId_Throws()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(Enum.GetValues<ApplicationChecklistEntryStatusId>().Length-1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var entryTypeIds = _fixture.CreateMany<ApplicationChecklistEntryTypeId>(Enum.GetValues<ApplicationChecklistEntryTypeId>().Length-2).ToImmutableArray();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(Enum.GetValues<ProcessStepTypeId>().Length-2).ToImmutableArray();

        // (bool IsValidApplicationId, bool IsSubmitted, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)
        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns((false, false, null, null));

        var Act = () => _service.VerifyChecklistEntryAndProcessSteps(applicationId, entryTypeId, entryStatusIds, processStepTypeId, entryTypeIds, processStepTypeIds);

        // Act
        var result = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);;

        // Assert
        result.Message.Should().Be($"application {applicationId} does not exist");
    }

    [Fact]
    public async Task VerifyChecklistEntry_InvalidApplicationStatus_Throws()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(Enum.GetValues<ApplicationChecklistEntryStatusId>().Length-1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var entryTypeIds = _fixture.CreateMany<ApplicationChecklistEntryTypeId>(Enum.GetValues<ApplicationChecklistEntryTypeId>().Length-2).ToImmutableArray();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(Enum.GetValues<ProcessStepTypeId>().Length-2).ToImmutableArray();

        // (bool IsValidApplicationId, bool IsSubmitted, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)
        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns((true, false, null, null));

        var Act = () => _service.VerifyChecklistEntryAndProcessSteps(applicationId, entryTypeId, entryStatusIds, processStepTypeId, entryTypeIds, processStepTypeIds);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);;

        // Assert
        result.Message.Should().Be($"application {applicationId} is not in status SUBMITTED");
    }

    [Fact]
    public async Task VerifyChecklistEntry_UnexpectedEntryData_Throws()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        IEnumerable<ApplicationChecklistEntryTypeId>? entryTypeIds = null;
        IEnumerable<ProcessStepTypeId>? processStepTypeIds = null;

        var entryData = Enum.GetValues<ApplicationChecklistEntryTypeId>().Except(new [] { entryTypeId }).Select(entryTypeId => (entryTypeId, entryStatusIds.First())).ToImmutableArray();
        var processSteps = new ProcessStep [] { new (Guid.NewGuid(), processStepTypeId, ProcessStepStatusId.TODO, DateTimeOffset.UtcNow) };

        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns((true, true, entryData, processSteps));

        var Act = () => _service.VerifyChecklistEntryAndProcessSteps(applicationId, entryTypeId, entryStatusIds, processStepTypeId, entryTypeIds, processStepTypeIds);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);;

        // Assert
        result.Message.Should().Be($"application {applicationId} does not have a checklist entry for {entryTypeId} in status {string.Join(", ",entryStatusIds)}");
    }

    [Fact]
    public async Task VerifyChecklistEntry_UnexpectedQueryResult_Throws()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        IEnumerable<ApplicationChecklistEntryTypeId>? entryTypeIds = null;
        IEnumerable<ProcessStepTypeId>? processStepTypeIds = null;

        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns((true, true, null, null));

        var Act = () => _service.VerifyChecklistEntryAndProcessSteps(applicationId, entryTypeId, entryStatusIds, processStepTypeId, entryTypeIds, processStepTypeIds);

        // Act
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);;

        // Assert
        result.Message.Should().Be($"checklist or processSteps should never be null here");
    }

    [Fact]
    public async Task VerifyChecklistEntry_UnexpectedEntryStatusData_Throws()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        IEnumerable<ApplicationChecklistEntryTypeId>? entryTypeIds = null;
        IEnumerable<ProcessStepTypeId>? processStepTypeIds = null;

        var entryData = new [] { (entryTypeId, Enum.GetValues<ApplicationChecklistEntryStatusId>().Except(entryStatusIds).First()) };
        var processSteps = new ProcessStep [] { new (Guid.NewGuid(), processStepTypeId, ProcessStepStatusId.TODO, DateTimeOffset.UtcNow) };

        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns((true, true, entryData, processSteps));

        var Act = () => _service.VerifyChecklistEntryAndProcessSteps(applicationId, entryTypeId, entryStatusIds, processStepTypeId, entryTypeIds, processStepTypeIds);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);;

        // Assert
        result.Message.Should().Be($"application {applicationId} does not have a checklist entry for {entryTypeId} in status {string.Join(", ",entryStatusIds)}");
    }

    [Fact]
    public async Task VerifyChecklistEntry_UnexpectedProcessStepData_Throws()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        IEnumerable<ApplicationChecklistEntryTypeId>? entryTypeIds = null;
        IEnumerable<ProcessStepTypeId>? processStepTypeIds = null;

        var entryData = new [] { (entryTypeId, entryStatusIds.First()) };
        var processSteps = new ProcessStep [] { new (Guid.NewGuid(), Enum.GetValues<ProcessStepTypeId>().Except( new [] { processStepTypeId } ).First(), ProcessStepStatusId.TODO, DateTimeOffset.UtcNow) };

        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns((true, true, entryData, processSteps));

        var Act = () => _service.VerifyChecklistEntryAndProcessSteps(applicationId, entryTypeId, entryStatusIds, processStepTypeId, entryTypeIds, processStepTypeIds);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);;

        // Assert
        result.Message.Should().Be($"application {applicationId} checklist entry {entryTypeId}, process step {processStepTypeId} is not eligible to run");
    }

    [Fact]
    public async Task VerifyChecklistEntry_UnexpectedProcessStepStatus_Throws()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var entryTypeId = _fixture.Create<ApplicationChecklistEntryTypeId>();
        var entryStatusIds = _fixture.CreateMany<ApplicationChecklistEntryStatusId>(1).ToImmutableArray();
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        IEnumerable<ApplicationChecklistEntryTypeId>? entryTypeIds = null;
        IEnumerable<ProcessStepTypeId>? processStepTypeIds = null;

        var entryData = new [] { (entryTypeId, entryStatusIds.First()) };
        var processSteps = new ProcessStep [] { new (Guid.NewGuid(), processStepTypeId, ProcessStepStatusId.SKIPPED, DateTimeOffset.UtcNow) };

        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns((true, true, entryData, processSteps));

        var Act = () => _service.VerifyChecklistEntryAndProcessSteps(applicationId, entryTypeId, entryStatusIds, processStepTypeId, entryTypeIds, processStepTypeIds);

        // Act
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);;

        // Assert
        result.Message.Should().Be($"processSteps should never have other status then TODO here");
    }

    #endregion

    #region FinalizeChecklistEntryAndProcessSteps

    [Fact]
    public void FinalizeChecklistEntryAndProcessSteps_ReturnsExpected()
    {
        // Arrange
        var context = new IChecklistService.ManualChecklistProcessStepData(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fixture.Create<ApplicationChecklistEntryTypeId>(),
            _fixture.Create<Dictionary<ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId>>().ToImmutableDictionary(),
            new ProcessStep[] { new (Guid.NewGuid(), _fixture.Create<ProcessStepTypeId>(), ProcessStepStatusId.TODO, DateTimeOffset.UtcNow) }
        );

        ApplicationChecklistEntry? modifiedChecklistEntry = null;

        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._,A<ApplicationChecklistEntryTypeId>._,A<Action<ApplicationChecklistEntry>>._))
            .Invokes((Guid applicationId, ApplicationChecklistEntryTypeId entryTypeId, Action<ApplicationChecklistEntry> modify) =>
            {
                modifiedChecklistEntry = new ApplicationChecklistEntry(applicationId, entryTypeId, default, default);
                modify(modifiedChecklistEntry);
            });

        ProcessStep? modifiedProcessStep = null;

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._,A<Action<ProcessStep>>._,A<Action<ProcessStep>>._))
            .Invokes((Guid processStepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify) =>
            {
                modifiedProcessStep = new ProcessStep(processStepId,default,default, default);
                initialize?.Invoke(modifiedProcessStep);
                modify(modifiedProcessStep);
            });

        var newProcessSteps = new List<ProcessStep>();

        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._,A<ProcessStepStatusId>._))
            .ReturnsLazily((ProcessStepTypeId stepTypeId, ProcessStepStatusId statusId) =>
            {
                var step = new ProcessStep(Guid.NewGuid(),stepTypeId,statusId, DateTimeOffset.UtcNow);
                newProcessSteps.Add(step);
                return step;
            });

        var newApplicationAssignedProcessSteps = new List<ApplicationAssignedProcessStep>();

        A.CallTo(() => _applicationChecklistRepository.CreateApplicationAssignedProcessStep(A<Guid>._,A<Guid>._))
            .ReturnsLazily((Guid applicationId, Guid processStepId) =>
            {
                var assigned = new ApplicationAssignedProcessStep(applicationId,processStepId);
                newApplicationAssignedProcessSteps.Add(assigned);
                return assigned;
            });

        var nextProcessStepTypeIds = Enum.GetValues<ProcessStepTypeId>().Except(context.ProcessSteps.Select(step => step.ProcessStepTypeId)).ToImmutableArray();

        // Act
        _service.FinalizeChecklistEntryAndProcessSteps(
            context,
            entry => { entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE; },
            Enum.GetValues<ProcessStepTypeId>());

        // Assert
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._,A<ApplicationChecklistEntryTypeId>._,A<Action<ApplicationChecklistEntry>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._,A<Action<ProcessStep>>._,A<Action<ProcessStep>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._,A<ProcessStepStatusId>._))
            .MustHaveHappened(nextProcessStepTypeIds.Length,Times.Exactly);
        A.CallTo(() => _applicationChecklistRepository.CreateApplicationAssignedProcessStep(A<Guid>._,A<Guid>._))
            .MustHaveHappened(nextProcessStepTypeIds.Length,Times.Exactly);

        modifiedChecklistEntry.Should().NotBeNull();
        modifiedChecklistEntry!.ApplicationChecklistEntryTypeId.Should().Be(context.EntryTypeId);
        modifiedChecklistEntry!.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);

        modifiedProcessStep.Should().NotBeNull();
        modifiedProcessStep!.Id.Should().Be(context.ProcessStepId);
        modifiedProcessStep.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);

        newProcessSteps.Count.Should().Be(nextProcessStepTypeIds.Length);
        newApplicationAssignedProcessSteps.Count.Should().Be(nextProcessStepTypeIds.Length);

        newProcessSteps.Select(step => step.ProcessStepTypeId).Should().Contain(nextProcessStepTypeIds);
        newApplicationAssignedProcessSteps.Should().AllSatisfy(assigned => assigned.CompanyApplicationId.Should().Be(context.ApplicationId));
        newApplicationAssignedProcessSteps.Select(assigned => assigned.ProcessStepId).Should().Contain(newProcessSteps.Select(step => step.Id));
    }

    #endregion

    #region SkipProcessSteps

    [Fact]
    public void SkipProcessSteps()
    {
        // Arrange
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(3);
        var processSteps = _fixture.CreateMany<ProcessStepTypeId>(100).Select(stepTypeId => new ProcessStep(Guid.NewGuid(), stepTypeId, ProcessStepStatusId.TODO, DateTimeOffset.UtcNow)).ToImmutableArray();

        var context = new IChecklistService.ManualChecklistProcessStepData(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fixture.Create<ApplicationChecklistEntryTypeId>(),
            _fixture.Create<Dictionary<ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId>>().ToImmutableDictionary(),
            processSteps
        );

        var modifiedProcessSteps = new List<ProcessStep>();

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._,A<Action<ProcessStep>>._,A<Action<ProcessStep>>._))
            .Invokes((Guid processStepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify) =>
            {
                var step = new ProcessStep(processStepId,default,default, default);
                initialize?.Invoke(step);
                modify(step);
                modifiedProcessSteps.Add(step);
            });

        // Act
        _service.SkipProcessSteps(context, processStepTypeIds);

        // Assert
        var eligibleSteps = processSteps.Where(step => processStepTypeIds.Contains(step.ProcessStepTypeId)).ToImmutableArray();

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._,A<Action<ProcessStep>>._,A<Action<ProcessStep>>._))
            .MustHaveHappened(eligibleSteps.Length, Times.Exactly);
        
        modifiedProcessSteps.Should().HaveCount(eligibleSteps.Length);
        modifiedProcessSteps.Where(step => step.ProcessStepStatusId == ProcessStepStatusId.SKIPPED).Should().HaveCount(3);
        modifiedProcessSteps.Where(step => step.ProcessStepStatusId == ProcessStepStatusId.DUPLICATE).Should().HaveCount(eligibleSteps.Length-3);
        var modifiedWithType = modifiedProcessSteps.Join(
            processSteps,
            modified => modified.Id,
            step => step.Id,
            (modified,step) => (step.ProcessStepTypeId, modified.ProcessStepStatusId)).ToImmutableArray();
        modifiedWithType.Length.Should().Be(eligibleSteps.Length);
        modifiedWithType.Should().AllSatisfy(step => processStepTypeIds.Should().Contain(step.ProcessStepTypeId));
    }

    #endregion

    #region ScheduleProcessSteps

    [Fact]
    public void ScheduleProcessSteps()
    {
        // Arrange
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(3);

        var context = new IChecklistService.WorkerChecklistProcessStepData(
            Guid.NewGuid(),
            default,
            _fixture.Create<Dictionary<ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId>>().ToImmutableDictionary(),
            processStepTypeIds
        );

        var newProcessSteps = new List<ProcessStep>();

        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._,A<ProcessStepStatusId>._))
            .ReturnsLazily((ProcessStepTypeId stepTypeId, ProcessStepStatusId statusId) =>
            {
                var step = new ProcessStep(Guid.NewGuid(),stepTypeId,statusId, DateTimeOffset.UtcNow);
                newProcessSteps.Add(step);
                return step;
            });

        var newApplicationAssignedProcessSteps = new List<ApplicationAssignedProcessStep>();

        A.CallTo(() => _applicationChecklistRepository.CreateApplicationAssignedProcessStep(A<Guid>._,A<Guid>._))
            .ReturnsLazily((Guid applicationId, Guid processStepId) =>
            {
                var assigned = new ApplicationAssignedProcessStep(applicationId,processStepId);
                newApplicationAssignedProcessSteps.Add(assigned);
                return assigned;
            });

        var nextProcessStepTypeIds = Enum.GetValues<ProcessStepTypeId>().Except(context.ProcessStepTypeIds).ToImmutableArray();

        // Act
        var result = _service.ScheduleProcessSteps(context, Enum.GetValues<ProcessStepTypeId>());

        // Assert
        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._,A<ProcessStepStatusId>._))
            .MustHaveHappened(nextProcessStepTypeIds.Length,Times.Exactly);
        A.CallTo(() => _applicationChecklistRepository.CreateApplicationAssignedProcessStep(A<Guid>._,A<Guid>._))
            .MustHaveHappened(nextProcessStepTypeIds.Length,Times.Exactly);

        result.Should().BeTrue();
        newProcessSteps.Select(step => step.ProcessStepTypeId).Should().Contain(nextProcessStepTypeIds);
        var stepAssigned = newProcessSteps
            .Join(
                newApplicationAssignedProcessSteps,
                step => step.Id,
                assigned => assigned.ProcessStepId,
                (step,assigned) => (assigned.CompanyApplicationId, step.ProcessStepStatusId))
            .ToImmutableArray();
        stepAssigned.Should().HaveCount(nextProcessStepTypeIds.Length);
        stepAssigned.Should().AllSatisfy(x => x.Should().Be((context.ApplicationId, ProcessStepStatusId.TODO)));
    }

    [Fact]
    public void ScheduleProcessSteps_AlreadyScheduled_ReturnsNotModified()
    {
        // Arrange
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(3);

        var context = new IChecklistService.WorkerChecklistProcessStepData(
            Guid.NewGuid(),
            default,
            _fixture.Create<Dictionary<ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId>>().ToImmutableDictionary(),
            Enum.GetValues<ProcessStepTypeId>()
        );

        // Act
        var result = _service.ScheduleProcessSteps(context, processStepTypeIds);

        // Assert
        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._,A<ProcessStepStatusId>._))
            .MustNotHaveHappened();
        A.CallTo(() => _applicationChecklistRepository.CreateApplicationAssignedProcessStep(A<Guid>._,A<Guid>._))
            .MustNotHaveHappened();

        result.Should().BeFalse();
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
        var result = await ChecklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Item1.Should().NotBeNull();
        result.Item1?.Invoke(entity);
        entity.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.FAILED);
        entity.Comment.Should().Be("Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.ServiceException: Test error");
        result.Item2.Should().NotBeNull();
        result.Item2.Should().ContainSingle().And.Match(x => x.Single() == ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE);
        result.Item3.Should().BeTrue();
    }

    [Fact]
    public async Task HandleServiceErrorAsync_WithHttpRequestException_OnlyCommentAdded()
    {
        // Arrange
        var entity = new ApplicationChecklistEntry(Guid.NewGuid(), ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var ex = new HttpRequestException("Test error");

        // Act
        var result = await ChecklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Item1.Should().NotBeNull();
        result.Item1?.Invoke(entity);
        entity.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.TO_DO);
        entity.Comment.Should().Be("System.Net.Http.HttpRequestException: Test error");
        result.Item2.Should().BeNull();
        result.Item3.Should().BeTrue();
    }

    #endregion
}
