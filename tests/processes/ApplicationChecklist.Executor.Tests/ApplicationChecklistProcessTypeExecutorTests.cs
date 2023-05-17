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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Executor.Tests;

public class ApplicationChecklistProcessTypeExecutorTests
{
	private readonly IPortalRepositories _portalRepositories;
	private readonly IApplicationChecklistRepository _checklistRepository;
	private readonly IApplicationChecklistHandlerService _checklistHandlerService;
	private readonly IApplicationChecklistCreationService _checklistCreationService;
	private readonly IApplicationChecklistHandlerService.ProcessStepExecution _firstExecution;
	private readonly IApplicationChecklistHandlerService.ProcessStepExecution _secondExecution;
	private readonly Func<IApplicationChecklistService.WorkerChecklistProcessStepData, CancellationToken, Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult>> _firstProcessFunc;
	private readonly Func<IApplicationChecklistService.WorkerChecklistProcessStepData, CancellationToken, Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult>> _secondProcessFunc;
	private readonly Func<Exception, IApplicationChecklistService.WorkerChecklistProcessStepData, CancellationToken, Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult>> _errorFunc;
	private readonly ProcessStepTypeId _firstStepTypeId;
	private readonly ProcessStepTypeId _secondStepTypeId;
	private readonly ApplicationChecklistProcessTypeExecutor _executor;
	private readonly IFixture _fixture;
	private readonly IEnumerable<ProcessStepTypeId> _executableSteps;

	public ApplicationChecklistProcessTypeExecutorTests()
	{
		_fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
		_fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
			.ForEach(b => _fixture.Behaviors.Remove(b));
		_fixture.Behaviors.Add(new OmitOnRecursionBehavior());

		_portalRepositories = A.Fake<IPortalRepositories>();
		_checklistRepository = A.Fake<IApplicationChecklistRepository>();

		_checklistHandlerService = A.Fake<IApplicationChecklistHandlerService>();
		_checklistCreationService = A.Fake<IApplicationChecklistCreationService>();

		_firstStepTypeId = ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH;
		_firstProcessFunc = A.Fake<Func<IApplicationChecklistService.WorkerChecklistProcessStepData, CancellationToken, Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult>>>();
		_errorFunc = A.Fake<Func<Exception, IApplicationChecklistService.WorkerChecklistProcessStepData, CancellationToken, Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult>>>();
		_firstExecution = new IApplicationChecklistHandlerService.ProcessStepExecution(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, true, _firstProcessFunc, _errorFunc);

		_secondStepTypeId = ProcessStepTypeId.ACTIVATE_APPLICATION;
		_secondProcessFunc = A.Fake<Func<IApplicationChecklistService.WorkerChecklistProcessStepData, CancellationToken, Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult>>>();
		_secondExecution = new IApplicationChecklistHandlerService.ProcessStepExecution(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, false, _secondProcessFunc, null);

		A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>())
			.Returns(_checklistRepository);

		_executor = new ApplicationChecklistProcessTypeExecutor(
			_checklistHandlerService,
			_checklistCreationService,
			_portalRepositories);

		_executableSteps = new[] { _firstStepTypeId, _secondStepTypeId };

		SetupFakes();
	}

	[Fact]
	public async Task InitializeProcess_CompleteChecklist_ReturnsExpected()
	{
		// Arrange
		var checklist = Enum.GetValues<ApplicationChecklistEntryTypeId>()
			.Select(typeId => (typeId, ApplicationChecklistEntryStatusId.TO_DO))
			.ToImmutableArray();

		var processId = Guid.NewGuid();
		var applicationId = Guid.NewGuid();

		A.CallTo(() => _checklistRepository.GetChecklistData(processId))
			.Returns((true, applicationId, CompanyApplicationStatusId.SUBMITTED, checklist));

		// Act
		var result = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

		// Assert
		result.Should().NotBeNull();
		result.Modified.Should().BeFalse();
		result.ScheduleStepTypeIds.Should().BeNull();
		A.CallTo(() => _checklistCreationService.CreateMissingChecklistItems(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._))
			.MustNotHaveHappened();
	}

	[Fact]
	public async Task InitializeProcess_InCompleteChecklist_ReturnsExpected()
	{
		// Arrange
		var missingEntryTypeIds = _fixture.CreateMany<ApplicationChecklistEntryTypeId>(2).ToImmutableArray();
		var entryTypeIds = Enum.GetValues<ApplicationChecklistEntryTypeId>()
			.Except(missingEntryTypeIds).ToImmutableArray();
		var checklist = entryTypeIds
			.Select(typeId => (typeId, ApplicationChecklistEntryStatusId.TO_DO))
			.ToImmutableArray();

		var stepTypeIds = _fixture.CreateMany<ProcessStepTypeId>().ToImmutableArray();

		var processId = Guid.NewGuid();
		var applicationId = Guid.NewGuid();

		A.CallTo(() => _checklistRepository.GetChecklistData(processId))
			.Returns((true, applicationId, CompanyApplicationStatusId.SUBMITTED, checklist));

		IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>? createdItems = null;

		A.CallTo(() => _checklistCreationService.CreateMissingChecklistItems(applicationId, A<IEnumerable<ApplicationChecklistEntryTypeId>>._))
			.ReturnsLazily((Guid _, IEnumerable<ApplicationChecklistEntryTypeId> existing) =>
			{
				createdItems = Enum.GetValues<ApplicationChecklistEntryTypeId>()
					.Except(existing)
					.Select(typeId => (typeId, ApplicationChecklistEntryStatusId.TO_DO))
					.ToImmutableArray();
				return createdItems;
			});

		A.CallTo(() => _checklistCreationService.GetInitialProcessStepTypeIds(A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._))
			.Returns(stepTypeIds);

		// Act
		var result = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

		// Assert
		result.Should().NotBeNull();
		result.Modified.Should().BeTrue();
		result.ScheduleStepTypeIds.Should().NotBeNull();
		result.ScheduleStepTypeIds.Should().HaveSameCount(stepTypeIds).And.Contain(stepTypeIds);
		A.CallTo(() => _checklistCreationService.CreateMissingChecklistItems(applicationId, A<IEnumerable<ApplicationChecklistEntryTypeId>>._))
			.MustHaveHappenedOnceExactly();
		createdItems.Should().NotBeNull();
		createdItems.Should().HaveSameCount(missingEntryTypeIds).And.Contain(missingEntryTypeIds.Select(typeId => (typeId, ApplicationChecklistEntryStatusId.TO_DO)));
		A.CallTo(() => _checklistCreationService.GetInitialProcessStepTypeIds(A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>.That.IsSameSequenceAs(createdItems!)))
			.MustHaveHappenedOnceExactly();
	}

	[Fact]
	public async Task InitializeProcess_InvalidProcessId_Throws()
	{
		// Arrange
		var processId = Guid.NewGuid();

		A.CallTo(() => _checklistRepository.GetChecklistData(processId))
			.Returns((false, Guid.Empty, default, null!));

		var Act = async () => await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

		// Act
		var result = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

		// Assert
		result.Message.Should().Be($"process {processId} does not exist");
		A.CallTo(() => _checklistCreationService.CreateMissingChecklistItems(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._))
			.MustNotHaveHappened();
	}

	[Fact]
	public async Task InitializeProcess_InvalidApplicationId_Throws()
	{
		// Arrange
		var processId = Guid.NewGuid();

		A.CallTo(() => _checklistRepository.GetChecklistData(processId))
			.Returns((true, Guid.Empty, default, null!));

		var Act = async () => await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

		// Act
		var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

		// Assert
		result.Message.Should().Be($"process {processId} is not associated with an application");
		A.CallTo(() => _checklistCreationService.CreateMissingChecklistItems(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._))
			.MustNotHaveHappened();
	}

	[Fact]
	public async Task InitializeProcess_InvalidApplicationStatus_Throws()
	{
		// Arrange
		var missingEntryTypeIds = _fixture.CreateMany<ApplicationChecklistEntryTypeId>(2).ToImmutableArray();
		var entryTypeIds = Enum.GetValues<ApplicationChecklistEntryTypeId>()
			.Except(missingEntryTypeIds).ToImmutableArray();
		var checklist = entryTypeIds
			.Select(typeId => (typeId, ApplicationChecklistEntryStatusId.TO_DO))
			.ToImmutableArray();

		var processId = Guid.NewGuid();
		var applicationId = Guid.NewGuid();

		A.CallTo(() => _checklistRepository.GetChecklistData(processId))
			.Returns((true, applicationId, CompanyApplicationStatusId.CREATED, checklist));

		var Act = async () => await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);
		;

		// Act
		var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

		// Assert
		result.Message.Should().Be($"application {applicationId} is not in status SUBMITTED");
		A.CallTo(() => _checklistCreationService.CreateMissingChecklistItems(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._))
			.MustNotHaveHappened();
	}

	#region ExecuteProcessStep

	[Fact]
	public async Task ExecuteProcessStep_InitializeNotCalled_Throws()
	{
		// Arrange
		var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
		var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();

		var Act = async () => await _executor.ExecuteProcessStep(processStepTypeId, processStepTypeIds, CancellationToken.None).ConfigureAwait(false);

		// Act
		var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);

		// Assert
		result.Message.Should().Be("applicationId or checklist should never be null or empty here");
	}

	[Fact]
	public async Task ExecuteProcessStep_InvalidChecklist_Throws()
	{
		// Arrange initialize
		var checklist = Enumerable.Empty<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>();

		var processId = Guid.NewGuid();
		var applicationId = Guid.NewGuid();

		A.CallTo(() => _checklistRepository.GetChecklistData(processId))
			.Returns((true, applicationId, CompanyApplicationStatusId.SUBMITTED, checklist));

		A.CallTo(() => _checklistCreationService.CreateMissingChecklistItems(A<Guid>._, A<IEnumerable<ApplicationChecklistEntryTypeId>>._))
			.Returns(Enumerable.Empty<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>());

		// Act initialize
		var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

		// Assert initialize
		initializationResult.Should().NotBeNull();
		initializationResult.Modified.Should().BeFalse();
		initializationResult.ScheduleStepTypeIds.Should().BeNull();

		// Arrange execute
		var executeProcessStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();

		var Act = async () => await _executor.ExecuteProcessStep(_firstStepTypeId, executeProcessStepTypeIds, CancellationToken.None).ConfigureAwait(false);

		// Act
		var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);

		// Assert
		result.Message.Should().Be($"checklist should always contain an entry for {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER} here");
	}

	[Fact]
	public async Task ExecuteProcessStep_ReturnsExpected()
	{
		// Arrange initialize
		var checklist = Enum.GetValues<ApplicationChecklistEntryTypeId>()
			.Select(typeId => (typeId, ApplicationChecklistEntryStatusId.TO_DO))
			.ToImmutableArray();

		var processId = Guid.NewGuid();
		var applicationId = Guid.NewGuid();

		A.CallTo(() => _checklistRepository.GetChecklistData(processId))
			.Returns((true, applicationId, CompanyApplicationStatusId.SUBMITTED, checklist));

		// Act initialize
		var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

		// Assert initialize
		initializationResult.Should().NotBeNull();
		initializationResult.Modified.Should().BeFalse();
		initializationResult.ScheduleStepTypeIds.Should().BeNull();

		// Arrange execute
		var executeProcessStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();
		var followupStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();

		A.CallTo(() => _firstProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.Returns(new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
				ProcessStepStatusId.DONE,
				(ApplicationChecklistEntry entry) => { entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS; },
				followupStepTypeIds,
				null,
				true,
				null));

		ApplicationChecklistEntry? checklistEntry = null;

		A.CallTo(() => _checklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._))
			.ReturnsLazily((Guid applicationId, ApplicationChecklistEntryTypeId entryTypeId, Action<ApplicationChecklistEntry> setFields) =>
			{
				checklistEntry = new ApplicationChecklistEntry(applicationId, entryTypeId, default, default);
				setFields(checklistEntry);
				return checklistEntry;
			});

		// Act execute
		var executionResult = await _executor.ExecuteProcessStep(_firstStepTypeId, executeProcessStepTypeIds, CancellationToken.None).ConfigureAwait(false);

		// Assert execute
		A.CallTo(() => _firstProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustHaveHappenedOnceExactly();
		A.CallTo(() => _errorFunc(A<Exception>._, A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustNotHaveHappened();
		A.CallTo(() => _secondProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustNotHaveHappened();

		executionResult.Modified.Should().BeTrue();
		executionResult.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
		executionResult.ScheduleStepTypeIds.Should().ContainInOrder(followupStepTypeIds);
		executionResult.SkipStepTypeIds.Should().BeNull();

		A.CallTo(() => _checklistRepository.AttachAndModifyApplicationChecklist(applicationId, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, A<Action<ApplicationChecklistEntry>>._))
			.MustHaveHappenedOnceExactly();

		checklistEntry.Should().NotBeNull();
		checklistEntry!.ApplicationId.Should().Be(applicationId);
		checklistEntry.ApplicationChecklistEntryTypeId.Should().Be(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER);
		checklistEntry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.IN_PROGRESS);
	}

	[Fact]
	public async Task ExecuteProcessStep_ThrowingTestException_WithErrorFunc_ReturnsExpected()
	{
		// Arrange initialize
		var checklist = Enum.GetValues<ApplicationChecklistEntryTypeId>()
			.Select(typeId => (typeId, ApplicationChecklistEntryStatusId.TO_DO))
			.ToImmutableArray();

		var processId = Guid.NewGuid();
		var applicationId = Guid.NewGuid();

		A.CallTo(() => _checklistRepository.GetChecklistData(processId))
			.Returns((true, applicationId, CompanyApplicationStatusId.SUBMITTED, checklist));

		// Act initialize
		var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

		// Assert initialize
		initializationResult.Should().NotBeNull();
		initializationResult.Modified.Should().BeFalse();
		initializationResult.ScheduleStepTypeIds.Should().BeNull();

		// Arrange execute

		var executeProcessStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();
		var followupStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();
		var error = _fixture.Create<TestException>();

		A.CallTo(() => _firstProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.Throws(error);

		A.CallTo(() => _errorFunc(A<Exception>._, A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.ReturnsLazily((Exception ex, IApplicationChecklistService.WorkerChecklistProcessStepData _, CancellationToken _) =>
				new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
					ProcessStepStatusId.FAILED,
					(ApplicationChecklistEntry entry) =>
						{
							entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.FAILED;
							entry.Comment = ex.Message;
						},
					followupStepTypeIds,
					null,
					true,
					"Test message"));

		ApplicationChecklistEntry? checklistEntry = null;

		A.CallTo(() => _checklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._))
			.ReturnsLazily((Guid applicationId, ApplicationChecklistEntryTypeId entryTypeId, Action<ApplicationChecklistEntry> setFields) =>
			{
				checklistEntry = new ApplicationChecklistEntry(applicationId, entryTypeId, default, default);
				setFields(checklistEntry);
				return checklistEntry;
			});

		// Act execute
		var executionResult = await _executor.ExecuteProcessStep(_firstStepTypeId, executeProcessStepTypeIds, CancellationToken.None).ConfigureAwait(false);

		// Assert execute
		A.CallTo(() => _firstProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustHaveHappenedOnceExactly();
		A.CallTo(() => _errorFunc(error, A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustHaveHappenedOnceExactly();
		A.CallTo(() => _secondProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustNotHaveHappened();

		executionResult.Modified.Should().BeTrue();
		executionResult.ProcessStepStatusId.Should().Be(ProcessStepStatusId.FAILED);
		executionResult.ScheduleStepTypeIds.Should().ContainInOrder(followupStepTypeIds);
		executionResult.SkipStepTypeIds.Should().BeNull();
		executionResult.ProcessMessage.Should().Be("Test message");

		A.CallTo(() => _checklistRepository.AttachAndModifyApplicationChecklist(applicationId, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, A<Action<ApplicationChecklistEntry>>._))
			.MustHaveHappenedOnceExactly();

		checklistEntry.Should().NotBeNull();
		checklistEntry!.ApplicationId.Should().Be(applicationId);
		checklistEntry.ApplicationChecklistEntryTypeId.Should().Be(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER);
		checklistEntry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.FAILED);
		checklistEntry.Comment.Should().Be(error.Message);
	}

	[Fact]
	public async Task ExecuteProcessStep_ThrowingTestException_WithErrorFuncIgnoringError_ReturnsExpected()
	{
		// Arrange initialize
		var checklist = Enum.GetValues<ApplicationChecklistEntryTypeId>()
			.Select(typeId => (typeId, ApplicationChecklistEntryStatusId.TO_DO))
			.ToImmutableArray();

		var processId = Guid.NewGuid();
		var applicationId = Guid.NewGuid();

		A.CallTo(() => _checklistRepository.GetChecklistData(processId))
			.Returns((true, applicationId, CompanyApplicationStatusId.SUBMITTED, checklist));

		// Act initialize
		var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

		// Assert initialize
		initializationResult.Should().NotBeNull();
		initializationResult.Modified.Should().BeFalse();
		initializationResult.ScheduleStepTypeIds.Should().BeNull();

		// Arrange execute

		var executeProcessStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();
		var followupStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();
		var error = _fixture.Create<TestException>();

		A.CallTo(() => _firstProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.Throws(error);

		A.CallTo(() => _errorFunc(A<Exception>._, A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.ReturnsLazily((Exception ex, IApplicationChecklistService.WorkerChecklistProcessStepData _, CancellationToken _) =>
				new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
					ProcessStepStatusId.FAILED,
					null,
					followupStepTypeIds,
					null,
					false,
					"Test Message"));

		// Act execute
		var executionResult = await _executor.ExecuteProcessStep(_firstStepTypeId, executeProcessStepTypeIds, CancellationToken.None).ConfigureAwait(false);

		// Assert execute
		A.CallTo(() => _firstProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustHaveHappenedOnceExactly();
		A.CallTo(() => _errorFunc(error, A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustHaveHappenedOnceExactly();
		A.CallTo(() => _secondProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustNotHaveHappened();

		executionResult.Modified.Should().BeFalse();
		executionResult.ProcessStepStatusId.Should().Be(ProcessStepStatusId.FAILED);
		executionResult.ScheduleStepTypeIds.Should().ContainInOrder(followupStepTypeIds);
		executionResult.SkipStepTypeIds.Should().BeNull();
		executionResult.ProcessMessage.Should().Be("Test Message");

		A.CallTo(() => _checklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._))
			.MustNotHaveHappened();
	}

	[Fact]
	public async Task ExecuteProcessStep_ThrowingTestException_WithoutErrorFunc_ReturnsExpected()
	{
		// Arrange initialize
		var checklist = Enum.GetValues<ApplicationChecklistEntryTypeId>()
			.Select(typeId => (typeId, ApplicationChecklistEntryStatusId.TO_DO))
			.ToImmutableArray();

		var processId = Guid.NewGuid();
		var applicationId = Guid.NewGuid();

		A.CallTo(() => _checklistRepository.GetChecklistData(processId))
			.Returns((true, applicationId, CompanyApplicationStatusId.SUBMITTED, checklist));

		// Act initialize
		var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

		// Assert initialize
		initializationResult.Should().NotBeNull();
		initializationResult.Modified.Should().BeFalse();
		initializationResult.ScheduleStepTypeIds.Should().BeNull();

		// Arrange execute
		var executeProcessStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();
		var followupStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();
		var error = _fixture.Create<TestException>();

		A.CallTo(() => _secondProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.Throws(error);

		ApplicationChecklistEntry? checklistEntry = null;

		A.CallTo(() => _checklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._))
			.ReturnsLazily((Guid applicationId, ApplicationChecklistEntryTypeId entryTypeId, Action<ApplicationChecklistEntry> setFields) =>
			{
				checklistEntry = new ApplicationChecklistEntry(applicationId, entryTypeId, default, default);
				setFields(checklistEntry);
				return checklistEntry;
			});

		// Act execute
		var executionResult = await _executor.ExecuteProcessStep(_secondStepTypeId, executeProcessStepTypeIds, CancellationToken.None).ConfigureAwait(false);

		// Assert execute
		A.CallTo(() => _firstProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustNotHaveHappened();
		A.CallTo(() => _errorFunc(A<Exception>._, A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustNotHaveHappened();
		A.CallTo(() => _secondProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustHaveHappenedOnceExactly();

		executionResult.Modified.Should().BeTrue();
		executionResult.ProcessStepStatusId.Should().Be(ProcessStepStatusId.FAILED);
		executionResult.ScheduleStepTypeIds.Should().BeNull();
		executionResult.SkipStepTypeIds.Should().BeNull();

		A.CallTo(() => _checklistRepository.AttachAndModifyApplicationChecklist(applicationId, ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, A<Action<ApplicationChecklistEntry>>._))
			.MustHaveHappenedOnceExactly();

		checklistEntry.Should().NotBeNull();
		checklistEntry!.ApplicationId.Should().Be(applicationId);
		checklistEntry.ApplicationChecklistEntryTypeId.Should().Be(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION);
		checklistEntry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.FAILED);
		checklistEntry.Comment.Should().Be(error.Message);
	}

	[Fact]
	public async Task ExecuteProcessStep_ThrowingRecoverableServiceException_WithoutErrorFunc_ReturnsExpected()
	{
		// Arrange initialize
		var checklist = Enum.GetValues<ApplicationChecklistEntryTypeId>()
			.Select(typeId => (typeId, ApplicationChecklistEntryStatusId.IN_PROGRESS))
			.ToImmutableArray();

		var processId = Guid.NewGuid();
		var applicationId = Guid.NewGuid();

		A.CallTo(() => _checklistRepository.GetChecklistData(processId))
			.Returns((true, applicationId, CompanyApplicationStatusId.SUBMITTED, checklist));

		// Act initialize
		var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

		// Assert initialize
		initializationResult.Should().NotBeNull();
		initializationResult.Modified.Should().BeFalse();
		initializationResult.ScheduleStepTypeIds.Should().BeNull();

		// Arrange execute
		var executeProcessStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();
		var followupStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();
		var error = new ServiceException(_fixture.Create<string>(), true);

		A.CallTo(() => _secondProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.Throws(error);

		ApplicationChecklistEntry? checklistEntry = null;

		A.CallTo(() => _checklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._))
			.ReturnsLazily((Guid applicationId, ApplicationChecklistEntryTypeId entryTypeId, Action<ApplicationChecklistEntry> setFields) =>
			{
				checklistEntry = new ApplicationChecklistEntry(applicationId, entryTypeId, default, default);
				setFields(checklistEntry);
				return checklistEntry;
			});

		// Act execute
		var executionResult = await _executor.ExecuteProcessStep(_secondStepTypeId, executeProcessStepTypeIds, CancellationToken.None).ConfigureAwait(false);

		// Assert execute
		A.CallTo(() => _firstProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustNotHaveHappened();
		A.CallTo(() => _errorFunc(A<Exception>._, A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustNotHaveHappened();
		A.CallTo(() => _secondProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustHaveHappenedOnceExactly();

		executionResult.Modified.Should().BeTrue();
		executionResult.ProcessStepStatusId.Should().Be(ProcessStepStatusId.TODO);
		executionResult.ScheduleStepTypeIds.Should().BeNull();
		executionResult.SkipStepTypeIds.Should().BeNull();

		A.CallTo(() => _checklistRepository.AttachAndModifyApplicationChecklist(applicationId, ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, A<Action<ApplicationChecklistEntry>>._))
			.MustHaveHappenedOnceExactly();

		checklistEntry.Should().NotBeNull();
		checklistEntry!.ApplicationId.Should().Be(applicationId);
		checklistEntry.ApplicationChecklistEntryTypeId.Should().Be(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION);
		checklistEntry.ApplicationChecklistEntryStatusId.Should().Be(default);
		checklistEntry.Comment.Should().Be(error.Message);
	}

	[Fact]
	public async Task ExecuteProcessStep_ThrowingSystemException_Throws()
	{
		// Arrange initialize
		var checklist = Enum.GetValues<ApplicationChecklistEntryTypeId>()
			.Select(typeId => (typeId, ApplicationChecklistEntryStatusId.IN_PROGRESS))
			.ToImmutableArray();

		var processId = Guid.NewGuid();
		var applicationId = Guid.NewGuid();

		A.CallTo(() => _checklistRepository.GetChecklistData(processId))
			.Returns((true, applicationId, CompanyApplicationStatusId.SUBMITTED, checklist));

		// Act initialize
		var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

		// Assert initialize
		initializationResult.Should().NotBeNull();
		initializationResult.Modified.Should().BeFalse();
		initializationResult.ScheduleStepTypeIds.Should().BeNull();

		// Arrange execute
		var executeProcessStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();
		var error = new SystemException(_fixture.Create<string>());

		A.CallTo(() => _secondProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.Throws(error);

		var Act = async () => await _executor.ExecuteProcessStep(_secondStepTypeId, executeProcessStepTypeIds, CancellationToken.None).ConfigureAwait(false);

		// Act execute
		var executionResult = await Assert.ThrowsAsync<SystemException>(Act).ConfigureAwait(false);

		// Assert execute
		A.CallTo(() => _firstProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustNotHaveHappened();
		A.CallTo(() => _errorFunc(A<Exception>._, A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustNotHaveHappened();
		A.CallTo(() => _secondProcessFunc(A<IApplicationChecklistService.WorkerChecklistProcessStepData>._, A<CancellationToken>._))
			.MustHaveHappenedOnceExactly();

		A.CallTo(() => _checklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._))
			.MustNotHaveHappened();

		executionResult.Message.Should().Be(error.Message);
	}

	#endregion

	#region GetProcessTypeId

	[Fact]
	public void GetProcessTypeId_ReturnsExpected()
	{
		// Act
		var result = _executor.GetProcessTypeId();

		// Assert
		result.Should().Be(ProcessTypeId.APPLICATION_CHECKLIST);
	}

	#endregion

	#region IsExecutableStepTypeId

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void IsExecutableProcessStep_ReturnsExpected(bool checklistHanderReturnValue)
	{
		// Arrange
		var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
		A.CallTo(() => _checklistHandlerService.IsExecutableProcessStep(A<ProcessStepTypeId>._))
			.Returns(checklistHanderReturnValue);

		// Act
		var result = _executor.IsExecutableStepTypeId(processStepTypeId);

		// Assert
		A.CallTo(() => _checklistHandlerService.IsExecutableProcessStep(processStepTypeId))
			.MustHaveHappenedOnceExactly();

		result.Should().Be(checklistHanderReturnValue);
	}

	#endregion

	#region IsLockRequested

	[Theory]
	[InlineData(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH, true)]
	[InlineData(ProcessStepTypeId.ACTIVATE_APPLICATION, false)]

	public async Task IsLockRequested_ReturnsExpected(ProcessStepTypeId stepTypeId, bool isLocked)
	{
		// Act
		var result = await _executor.IsLockRequested(stepTypeId).ConfigureAwait(false);

		// Assert
		result.Should().Be(isLocked);
		A.CallTo(() => _checklistHandlerService.GetProcessStepExecution(stepTypeId)).MustHaveHappenedOnceExactly();
	}

	[Fact]
	public async Task IsLockRequested_InvalidProcessStepTypeId_Throws()
	{
		// Arrange
		var stepTypeId = Enum.GetValues<ProcessStepTypeId>().Except(_executableSteps).First();
		var Act = async () => await _executor.IsLockRequested(stepTypeId).ConfigureAwait(false);

		// Assert
		var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
		result.Message.Should().Be($"no execution defined for processStep {stepTypeId}");
		A.CallTo(() => _checklistHandlerService.GetProcessStepExecution(stepTypeId)).MustHaveHappenedOnceExactly();
	}

	#endregion

	#region GetExecutableStepTypeIds

	[Fact]
	public void GetExecutableStepTypeIds_ReturnsExpected()
	{
		//Act
		var result = _executor.GetExecutableStepTypeIds();

		// Assert
		result.Should().HaveCount(2)
			.And.Satisfy(
				x => x == _firstStepTypeId,
				x => x == _secondStepTypeId
			);
	}

	#endregion

	#region Setup

	private void SetupFakes()
	{
		A.CallTo(() => _checklistHandlerService.GetProcessStepExecution(A<ProcessStepTypeId>._))
			.ReturnsLazily((ProcessStepTypeId stepTypeId) =>
				stepTypeId switch
				{
					_ when stepTypeId == _firstStepTypeId => _firstExecution,
					_ when stepTypeId == _secondStepTypeId => _secondExecution,
					_ => throw new ConflictException($"no execution defined for processStep {stepTypeId}"),
				}
			);

		A.CallTo(() => _checklistHandlerService.IsExecutableProcessStep(A<ProcessStepTypeId>._))
			.ReturnsLazily((ProcessStepTypeId stepTypeId) => _executableSteps.Contains(stepTypeId));

		A.CallTo(() => _checklistHandlerService.GetExecutableStepTypeIds())
			.Returns(_executableSteps);
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
