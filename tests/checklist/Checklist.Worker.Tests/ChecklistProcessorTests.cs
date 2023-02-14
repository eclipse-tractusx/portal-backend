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

using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Worker.Tests;

public class ChecklistProcessorTests
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IApplicationChecklistRepository _applicationChecklistRepository;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly IChecklistHandlerService _checklistHandlerService;
    private readonly IChecklistHandlerService.ProcessStepExecution _firstExecution;
    private readonly IChecklistHandlerService.ProcessStepExecution _secondExecution;
    private readonly Func<IChecklistService.WorkerChecklistProcessStepData,CancellationToken,Task<(Action<ApplicationChecklistEntry>?,IEnumerable<ProcessStepTypeId>?,bool)>> _firstProcessFunc;
    private readonly Func<IChecklistService.WorkerChecklistProcessStepData,CancellationToken,Task<(Action<ApplicationChecklistEntry>?,IEnumerable<ProcessStepTypeId>?,bool)>> _secondProcessFunc;
    private readonly Func<Exception,IChecklistService.WorkerChecklistProcessStepData,CancellationToken,Task<(Action<ApplicationChecklistEntry>?,IEnumerable<ProcessStepTypeId>?,bool)>> _errorFunc;
    private readonly IMockLogger<ChecklistProcessor> _mockLogger;
    private readonly ILogger<ChecklistProcessor> _logger;
    private readonly ChecklistProcessor _processor;
    private readonly IFixture _fixture;
    private readonly IEnumerable<ProcessStepTypeId> _manualSteps;

    public ChecklistProcessorTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b =>_fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _applicationChecklistRepository = A.Fake<IApplicationChecklistRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();

        _checklistHandlerService = A.Fake<IChecklistHandlerService>();

        _firstProcessFunc = A.Fake<Func<IChecklistService.WorkerChecklistProcessStepData,CancellationToken,Task<(Action<ApplicationChecklistEntry>?,IEnumerable<ProcessStepTypeId>?,bool)>>>();
        _errorFunc = A.Fake<Func<Exception,IChecklistService.WorkerChecklistProcessStepData,CancellationToken,Task<(Action<ApplicationChecklistEntry>?,IEnumerable<ProcessStepTypeId>?,bool)>>>();
        _firstExecution = new IChecklistHandlerService.ProcessStepExecution(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER,_firstProcessFunc,_errorFunc);

        _secondProcessFunc = A.Fake<Func<IChecklistService.WorkerChecklistProcessStepData,CancellationToken,Task<(Action<ApplicationChecklistEntry>?,IEnumerable<ProcessStepTypeId>?,bool)>>>();
        _secondExecution = new IChecklistHandlerService.ProcessStepExecution(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION,_secondProcessFunc,null);

        _mockLogger = A.Fake<IMockLogger<ChecklistProcessor>>();
        _logger = new MockLogger<ChecklistProcessor>(_mockLogger);

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>())
            .Returns(_applicationChecklistRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>())
            .Returns(_processStepRepository);

        _processor = new ChecklistProcessor(
            _portalRepositories,
            _checklistHandlerService,
            _logger);

        _manualSteps = Enum.GetValues<ProcessStepTypeId>().Except(new [] { ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH, ProcessStepTypeId.ACTIVATE_APPLICATION }).ToImmutableArray();

        SetupFakes();
    }

    [Fact]
    public async Task ProcessChecklist_IgnoringDuplicates_Success()
    {
        // Arrange
        var checklist = Enum.GetValues<ApplicationChecklistEntryTypeId>()
            .Select(typeId => _fixture
                .Build<(ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId)>()
                .With(x => x.Item1, typeId)
                .With(x => x.Item2, ApplicationChecklistEntryStatusId.TO_DO)
                .Create())
            .ToImmutableArray();

        var processSteps = _fixture.CreateMany<ProcessStepTypeId>(100)
            .Select(typeId => new ProcessStep(Guid.NewGuid(), typeId, ProcessStepStatusId.TODO, DateTimeOffset.UtcNow))
            .ToImmutableArray();

        A.CallTo(() => _firstProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .Returns((
                (ApplicationChecklistEntry entry) => { entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS; },
                new [] {
                    ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH,
                    ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH
                },
                true))
            .Once()
            .Then
            .Returns((
                (ApplicationChecklistEntry entry) => { entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE; },
                null,
                true));

        A.CallTo(() => _secondProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .Returns((
                (ApplicationChecklistEntry entry) => { entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.TO_DO; },
                new [] {
                    ProcessStepTypeId.ACTIVATE_APPLICATION,
                    ProcessStepTypeId.ACTIVATE_APPLICATION
                },
                true))
            .Once()
            .Then
            .Returns((
                (ApplicationChecklistEntry entry) => { entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.FAILED; },
                null,
                true));

        var modifiedSteps = new List<ProcessStep>();

        A.CallTo(()=> _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .Invokes((Guid processStepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify) =>
            {
                var step = new ProcessStep(processStepId, default, default, default);
                initialize?.Invoke(step);
                modify(step);
                modifiedSteps.Add(step);
            });

        var createdSteps = new List<ProcessStep>();

        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._, A<ProcessStepStatusId>._))
            .ReturnsLazily((ProcessStepTypeId stepTypeId, ProcessStepStatusId stepStatusId) =>
            {
                var processStep = new ProcessStep(Guid.NewGuid(), stepTypeId, stepStatusId, DateTimeOffset.UtcNow);
                createdSteps.Add(processStep);
                return processStep;
            });

        var applicationId = Guid.NewGuid();

        // Act
        var result = await _processor.ProcessChecklist(applicationId, checklist, processSteps, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(4);
        result.Should().Contain((ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.IN_PROGRESS));
        result.Should().Contain((ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE));
        result.Should().Contain((ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO));
        result.Should().Contain((ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.FAILED));

        A.CallTo(() => _secondProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustHaveHappenedTwiceExactly();

        A.CallTo(() => _firstProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustHaveHappenedTwiceExactly();

        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>.That.Matches(step => _manualSteps.Contains(step)), ProcessStepStatusId.TODO)).MustNotHaveHappened();
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH, ProcessStepStatusId.TODO)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.ACTIVATE_APPLICATION, ProcessStepStatusId.TODO)).MustHaveHappenedOnceExactly();
        createdSteps.Should().HaveCount(2);
        createdSteps.Select(step => (step.ProcessStepTypeId, step.ProcessStepStatusId)).Should().Contain(
            new [] {
                (ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH, ProcessStepStatusId.TODO),
                (ProcessStepTypeId.ACTIVATE_APPLICATION, ProcessStepStatusId.TODO)
            });
        foreach (var processStep in createdSteps)
        {
            A.CallTo(() => _applicationChecklistRepository.CreateApplicationAssignedProcessStep(applicationId, processStep.Id)).MustHaveHappenedOnceExactly();
        }

        var automaticSteps = processSteps.Where(step => !_manualSteps.Contains(step.ProcessStepTypeId)).ToList();
        var numAutomaticProcessStepTypes = automaticSteps.GroupBy(step => step.ProcessStepTypeId).Count();

        A.CallTo(()=> _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._)).MustHaveHappened(automaticSteps.Count + numAutomaticProcessStepTypes, Times.Exactly);
        modifiedSteps.Should().HaveCount(automaticSteps.Count + numAutomaticProcessStepTypes);
        modifiedSteps.Where(step => step.ProcessStepStatusId == ProcessStepStatusId.DUPLICATE).Should().HaveCount(automaticSteps.Count-numAutomaticProcessStepTypes);
    }

    [Fact]
    public async Task ProcessChecklist_IgnoreManualSteps()
    {
        // Arrange
        var checklist = Enum.GetValues<ApplicationChecklistEntryTypeId>()
            .Select(typeId => _fixture
                .Build<(ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId)>()
                .With(x => x.Item1, typeId)
                .Create())
            .ToImmutableArray();

        var processSteps = _manualSteps.Select(steptTypeId => new ProcessStep(Guid.NewGuid(), steptTypeId, ProcessStepStatusId.TODO, DateTimeOffset.UtcNow)).ToImmutableArray();

        // Act
        var result = await _processor.ProcessChecklist(Guid.NewGuid(), checklist, processSteps, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().BeEmpty();

        A.CallTo(() => _firstProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => _secondProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>.That.Matches(step => _manualSteps.Contains(step)), ProcessStepStatusId.TODO)).MustNotHaveHappened();
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH, ProcessStepStatusId.TODO)).MustNotHaveHappened();
        A.CallTo(() => _applicationChecklistRepository.CreateApplicationAssignedProcessStep(A<Guid>._, A<Guid>._)).MustNotHaveHappened();
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.ACTIVATE_APPLICATION, ProcessStepStatusId.TODO)).MustNotHaveHappened();
    
        A.CallTo(()=> _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ProcessChecklist_ScheduleManualSteps_IgnoringDuplicates()
    {
        // Arrange
        var checklist = Enum.GetValues<ApplicationChecklistEntryTypeId>()
            .Select(typeId => _fixture
                .Build<(ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId)>()
                .With(x => x.Item1, typeId)
                .Create())
            .ToImmutableArray();

        var processSteps = new [] {
            ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH
        }.Select(steptTypeId => new ProcessStep(Guid.NewGuid(), steptTypeId, ProcessStepStatusId.TODO, DateTimeOffset.UtcNow)).ToImmutableArray();

        A.CallTo(() => _firstProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .Returns((
                (ApplicationChecklistEntry entry) => { entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE; },
                new [] {
                    ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL,
                    ProcessStepTypeId.END_CLEARING_HOUSE,
                    ProcessStepTypeId.VERIFY_REGISTRATION,
                    ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL,
                    ProcessStepTypeId.END_CLEARING_HOUSE,
                    ProcessStepTypeId.VERIFY_REGISTRATION
                },
                true));

        var createdSteps = new List<ProcessStep>();

        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._, A<ProcessStepStatusId>._))
            .ReturnsLazily((ProcessStepTypeId stepTypeId, ProcessStepStatusId stepStatusId) =>
            {
                var processStep = new ProcessStep(Guid.NewGuid(), stepTypeId, stepStatusId, DateTimeOffset.UtcNow);
                createdSteps.Add(processStep);
                return processStep;
            });

        var applicationId = Guid.NewGuid();

        // Act
        var result = await _processor.ProcessChecklist(applicationId, checklist, processSteps, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(1);

        A.CallTo(() => _firstProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _secondProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.VERIFY_REGISTRATION, ProcessStepStatusId.TODO)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL, ProcessStepStatusId.TODO)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.END_CLEARING_HOUSE, ProcessStepStatusId.TODO)).MustHaveHappenedOnceExactly();
        createdSteps.Should().HaveCount(3);
        createdSteps.Select(step => step.ProcessStepTypeId).Should().Contain(new [] { ProcessStepTypeId.VERIFY_REGISTRATION, ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL, ProcessStepTypeId.END_CLEARING_HOUSE });
        createdSteps.Should().AllSatisfy(step => step.ProcessStepStatusId.Should().Be(ProcessStepStatusId.TODO));
        createdSteps.Select(step => (step.ProcessStepTypeId, step.ProcessStepStatusId)).Should().Contain(
            new [] {
                (ProcessStepTypeId.VERIFY_REGISTRATION, ProcessStepStatusId.TODO),
                (ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL, ProcessStepStatusId.TODO),
                (ProcessStepTypeId.END_CLEARING_HOUSE, ProcessStepStatusId.TODO)
            });
        foreach (var processStep in createdSteps)
        {
            A.CallTo(() => _applicationChecklistRepository.CreateApplicationAssignedProcessStep(applicationId, processStep.Id)).MustHaveHappenedOnceExactly();
        }

        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH, ProcessStepStatusId.TODO)).MustNotHaveHappened();
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL, ProcessStepStatusId.TODO)).MustNotHaveHappened();
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.CREATE_IDENTITY_WALLET, ProcessStepStatusId.TODO)).MustNotHaveHappened();
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.START_CLEARING_HOUSE, ProcessStepStatusId.TODO)).MustNotHaveHappened();
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.START_SELF_DESCRIPTION_LP, ProcessStepStatusId.TODO)).MustNotHaveHappened();
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.ACTIVATE_APPLICATION, ProcessStepStatusId.TODO)).MustNotHaveHappened();

        A.CallTo(()=> _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ProcessChecklist_SkipStep_ReturnsExpected()
    {
        // Arrange
        var checklist = Enum.GetValues<ApplicationChecklistEntryTypeId>()
            .Select(typeId => _fixture
                .Build<(ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId)>()
                .With(x => x.Item1, typeId)
                .With(x => x.Item2, ApplicationChecklistEntryStatusId.TO_DO)
                .Create())
            .ToImmutableArray();

        var processSteps = new [] {
            ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH
        }.Select(steptTypeId => new ProcessStep(Guid.NewGuid(), steptTypeId, ProcessStepStatusId.TODO, DateTimeOffset.UtcNow)).ToImmutableArray();

        A.CallTo(() => _firstProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .Returns((
                null,
                null,
                false));

        // Act
        var result = await _processor.ProcessChecklist(Guid.NewGuid(), checklist, processSteps, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().BeEmpty();

        A.CallTo(() => _firstProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _secondProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => _errorFunc(A<Exception>._, A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._,A<ProcessStepStatusId>._)).MustNotHaveHappened();
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ProcessChecklist_ErrorFunc_ReturnsExpected()
    {
        // Arrange
        var checklist = Enum.GetValues<ApplicationChecklistEntryTypeId>()
            .Select(typeId => _fixture
                .Build<(ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId)>()
                .With(x => x.Item1, typeId)
                .With(x => x.Item2, ApplicationChecklistEntryStatusId.TO_DO)
                .Create())
            .ToImmutableArray();

        var processSteps = new [] {
            ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH
        }.Select(steptTypeId => new ProcessStep(Guid.NewGuid(), steptTypeId, ProcessStepStatusId.TODO, DateTimeOffset.UtcNow)).ToImmutableArray();

        var message = _fixture.Create<string>();

        A.CallTo(() => _firstProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .Throws(new TestException(message));

        A.CallTo(() => _secondProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .Returns((
                (ApplicationChecklistEntry entry) => { entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE; },
                null,
                true));

        A.CallTo(() => _errorFunc(A<Exception>._,A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .ReturnsLazily((Exception ex, IChecklistService.WorkerChecklistProcessStepData _, CancellationToken _) => (
                (ApplicationChecklistEntry entry) => { entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS; },
                new [] {
                    ProcessStepTypeId.ACTIVATE_APPLICATION,
                },
                true));

        var createdSteps = new List<ProcessStep>();

        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._, A<ProcessStepStatusId>._))
            .ReturnsLazily((ProcessStepTypeId stepTypeId, ProcessStepStatusId stepStatusId) =>
            {
                var processStep = new ProcessStep(Guid.NewGuid(), stepTypeId, stepStatusId, DateTimeOffset.UtcNow);
                createdSteps.Add(processStep);
                return processStep;
            });

        var applicationId = Guid.NewGuid();

        // Act
        var result = await _processor.ProcessChecklist(applicationId, checklist, processSteps, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain((ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.IN_PROGRESS));
        result.Should().Contain((ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.DONE));

        A.CallTo(() => _firstProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _secondProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _errorFunc(A<Exception>._, A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.ACTIVATE_APPLICATION, ProcessStepStatusId.TODO)).MustHaveHappenedOnceExactly();
        createdSteps.Should().HaveCount(1);
        createdSteps.Select(step => (step.ProcessStepTypeId, step.ProcessStepStatusId)).Should().Contain(
            new [] {
                (ProcessStepTypeId.ACTIVATE_APPLICATION, ProcessStepStatusId.TODO),
            });
        foreach (var processStep in createdSteps)
        {
            A.CallTo(() => _applicationChecklistRepository.CreateApplicationAssignedProcessStep(applicationId, processStep.Id)).MustHaveHappenedOnceExactly();
        }

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._)).MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task ProcessChecklist_DefaultError_ReturnsExpected()
    {
        // Arrange
        var checklist = Enum.GetValues<ApplicationChecklistEntryTypeId>()
            .Select(typeId => _fixture
                .Build<(ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId)>()
                .With(x => x.Item1, typeId)
                .With(x => x.Item2, ApplicationChecklistEntryStatusId.TO_DO)
                .Create())
            .ToImmutableArray();

        var processSteps = new [] {
            ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH
        }.Select(steptTypeId => new ProcessStep(Guid.NewGuid(), steptTypeId, ProcessStepStatusId.TODO, DateTimeOffset.UtcNow)).ToImmutableArray();

        var message = _fixture.Create<string>();

        A.CallTo(() => _firstProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .Returns((
                (ApplicationChecklistEntry entry) => { entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS; },
                new [] { ProcessStepTypeId.ACTIVATE_APPLICATION },
                true));

        A.CallTo(() => _secondProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .Throws(new TestException(message));

        var createdSteps = new List<ProcessStep>();

        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._, A<ProcessStepStatusId>._))
            .ReturnsLazily((ProcessStepTypeId stepTypeId, ProcessStepStatusId stepStatusId) =>
            {
                var processStep = new ProcessStep(Guid.NewGuid(), stepTypeId, stepStatusId, DateTimeOffset.UtcNow);
                createdSteps.Add(processStep);
                return processStep;
            });

        var applicationId = Guid.NewGuid();

        // Act
        var result = await _processor.ProcessChecklist(applicationId, checklist, processSteps, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain((ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.IN_PROGRESS));
        result.Should().Contain((ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.FAILED));

        A.CallTo(() => _firstProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _secondProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _errorFunc(A<Exception>._, A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._,A<ProcessStepStatusId>._)).MustHaveHappenedOnceExactly();
        createdSteps.Should().HaveCount(1);
        createdSteps.Select(step => (step.ProcessStepTypeId, step.ProcessStepStatusId)).Should().Contain(
            new [] {
                (ProcessStepTypeId.ACTIVATE_APPLICATION, ProcessStepStatusId.TODO),
            });
        foreach (var processStep in createdSteps)
        {
            A.CallTo(() => _applicationChecklistRepository.CreateApplicationAssignedProcessStep(applicationId, processStep.Id)).MustHaveHappenedOnceExactly();
        }

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._)).MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task ProcessChecklist_DefaultServiceError_ReturnsExpected()
    {
        // Arrange
        var checklist = Enum.GetValues<ApplicationChecklistEntryTypeId>()
            .Select(typeId => _fixture
                .Build<(ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId)>()
                .With(x => x.Item1, typeId)
                .With(x => x.Item2, ApplicationChecklistEntryStatusId.TO_DO)
                .Create())
            .ToImmutableArray();

        var processSteps = new [] {
            ProcessStepTypeId.ACTIVATE_APPLICATION
        }.Select(steptTypeId => new ProcessStep(Guid.NewGuid(), steptTypeId, ProcessStepStatusId.TODO, DateTimeOffset.UtcNow)).ToImmutableArray();

        var message = _fixture.Create<string>();

        A.CallTo(() => _secondProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .Throws(new ServiceException(message, System.Net.HttpStatusCode.ServiceUnavailable));

        var modifiedEntries = new List<ApplicationChecklistEntry>();

        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._,A<ApplicationChecklistEntryTypeId>._,A<Action<ApplicationChecklistEntry>>._))
            .ReturnsLazily((Guid applicationId, ApplicationChecklistEntryTypeId entryTypeId, Action<ApplicationChecklistEntry> setFields) =>
            {
                var entry = new ApplicationChecklistEntry(applicationId, entryTypeId, default, default);
                setFields(entry);
                modifiedEntries.Add(entry);
                return entry;
            });

        var modifiedSteps = new List<ProcessStep>();

        A.CallTo(()=> _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._))
            .Invokes((Guid processStepId, Action<ProcessStep> initialize, Action<ProcessStep> modify) =>
            {
                var step = new ProcessStep(processStepId, default, default, default);
                initialize?.Invoke(step);
                modify(step);
                modifiedSteps.Add(step);
            });

        // Act
        var result = await _processor.ProcessChecklist(Guid.NewGuid(), checklist, processSteps, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain((ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, default));

        modifiedEntries.Should().HaveCount(1);
        modifiedEntries.Single().Should().Match<ApplicationChecklistEntry>(entry => entry.ApplicationChecklistEntryTypeId == ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION && entry.Comment != null && entry.Comment.Contains($"ServiceException: {message}"));

        modifiedSteps.Should().HaveCount(1);
        modifiedSteps.Single().Should().Match<ProcessStep>(step => step.ProcessStepStatusId == ProcessStepStatusId.TODO);

        A.CallTo(() => _firstProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => _secondProcessFunc(A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _errorFunc(A<Exception>._, A<IChecklistService.WorkerChecklistProcessStepData>._,A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._,A<ProcessStepStatusId>._)).MustNotHaveHappened();
        A.CallTo(() => _applicationChecklistRepository.CreateApplicationAssignedProcessStep(A<Guid>._,A<Guid>._)).MustNotHaveHappened();
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessStep(A<Guid>._, A<Action<ProcessStep>>._, A<Action<ProcessStep>>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ProcessChecklist_MissingChecklistEntry_ThrowsExpected()
    {
        // Arrange
        var checklist = new [] {
                (ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO)
            }.ToImmutableArray();

        var processSteps = new [] {
            ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH
        }.Select(steptTypeId => new ProcessStep(Guid.NewGuid(), steptTypeId, ProcessStepStatusId.TODO, DateTimeOffset.UtcNow)).ToImmutableArray();

        // Act
        var Act = async () => await _processor.ProcessChecklist(Guid.NewGuid(), checklist, processSteps, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Assert
        result.Message.Should().Be("no checklist entry BUSINESS_PARTNER_NUMBER for CREATE_BUSINESS_PARTNER_NUMBER_PUSH");
    }

    #region Setup

    private void SetupFakes()
    {
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._,A<ApplicationChecklistEntryTypeId>._,A<Action<ApplicationChecklistEntry>>._))
            .ReturnsLazily((Guid applicationId, ApplicationChecklistEntryTypeId entryTypeId, Action<ApplicationChecklistEntry> setFields) =>
            {
                var entry = new ApplicationChecklistEntry(applicationId, entryTypeId, default, default);
                setFields(entry);
                return entry;
            });

        A.CallTo(() => _checklistHandlerService.GetProcessStepExecution(A<ProcessStepTypeId>._))
            .ReturnsLazily((ProcessStepTypeId stepTypeId) =>
                stepTypeId switch
                {
                    ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH => _firstExecution,
                    ProcessStepTypeId.ACTIVATE_APPLICATION => _secondExecution,
                    _ => throw new ConflictException($"no execution defined for processStep {stepTypeId}"),
                }
            );

        A.CallTo(() => _checklistHandlerService.IsManualProcessStep(A<ProcessStepTypeId>._))
            .ReturnsLazily((ProcessStepTypeId stepTypeId) => _manualSteps.Contains(stepTypeId));
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
