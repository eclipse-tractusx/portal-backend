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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Worker.Tests;

public class ChecklistExecutionServiceTests
{
    private readonly IApplicationChecklistRepository _applicationChecklistRepository;

    private readonly IPortalRepositories _portalRepositories;
    private readonly IChecklistProcessor _checklistProcessor;
    private readonly IChecklistCreationService _checklistCreationService;
    private readonly IMockLogger<ChecklistExecutionService> _mockLogger;
    private readonly ILogger<ChecklistExecutionService> _logger;
    private readonly ChecklistExecutionService _service;
    private readonly IFixture _fixture;

    public ChecklistExecutionServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b =>_fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _applicationChecklistRepository = A.Fake<IApplicationChecklistRepository>();
        _checklistProcessor = A.Fake<IChecklistProcessor>();
        _checklistCreationService = A.Fake<IChecklistCreationService>();

        _mockLogger = A.Fake<IMockLogger<ChecklistExecutionService>>();
        _logger = new MockLogger<ChecklistExecutionService>(_mockLogger);

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>())
            .Returns(_applicationChecklistRepository);

        var serviceProvider = A.Fake<IServiceProvider>();
        A.CallTo(() => serviceProvider.GetService(typeof(IPortalRepositories))).Returns(_portalRepositories);
        A.CallTo(() => serviceProvider.GetService(typeof(IChecklistProcessor))).Returns(_checklistProcessor);
        A.CallTo(() => serviceProvider.GetService(typeof(IChecklistCreationService))).Returns(_checklistCreationService);
        var serviceScope = A.Fake<IServiceScope>();
        A.CallTo(() => serviceScope.ServiceProvider).Returns(serviceProvider);
        var serviceScopeFactory = A.Fake<IServiceScopeFactory>();
        A.CallTo(() => serviceScopeFactory.CreateScope()).Returns(serviceScope);
        A.CallTo(() => serviceProvider.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactory);

        _service = new ChecklistExecutionService(serviceScopeFactory, _logger);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoPendingItems_NoServiceCall()
    {
        // Arrange
        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData())
            .Returns(new List<(Guid ApplicationId, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)> Checklist, IEnumerable<ProcessStep> ProcessSteps)>().ToAsyncEnumerable());

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _checklistProcessor.ProcessChecklist(A<Guid>._,A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._,A<IEnumerable<ProcessStep>>._,A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WithPendingItems_CallsProcessExpectedNumberOfTimes()
    {
        var stepData = _fixture.CreateMany<(Guid ApplicationId, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)> Checklist, IEnumerable<ProcessStep> ProcessSteps)>().ToImmutableArray();
        // Arrange
        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData())
            .Returns(stepData.ToAsyncEnumerable());

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _checklistProcessor.ProcessChecklist(A<Guid>._,A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._,A<IEnumerable<ProcessStep>>._,A<CancellationToken>._))
            .MustHaveHappened(stepData.Length,Times.Exactly);
    }

    [Fact]
    public async Task ExecuteAsync_WithException_LogsError()
    {
        // Arrange
        var stepData = _fixture.CreateMany<int>(5)
            .Select(_ => _fixture
                .Build<(Guid ApplicationId, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)> Checklist, IEnumerable<ProcessStep> ProcessSteps)>()
                .With(x => x.Checklist, Enum.GetValues<ApplicationChecklistEntryTypeId>().Select(type => (type, _fixture.Create<ApplicationChecklistEntryStatusId>())))
                .With(x => x.ProcessSteps, _fixture.CreateMany<int>(5).Select(_ => _fixture.Build<ProcessStep>().With(x => x.ProcessStepStatusId, ProcessStepStatusId.TODO).Create()))
                .Create()).ToImmutableArray();

        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData())
            .Returns(stepData.ToAsyncEnumerable());
        A.CallTo(() => _checklistProcessor.ProcessChecklist(A<Guid>._,A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._,A<IEnumerable<ProcessStep>>._,A<CancellationToken>._))
            .Throws(() => new Exception("Only a test"));

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        Environment.ExitCode.Should().Be(1);
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WithPendingApplications_NoEntriesCreation_NoExecutableProcessSteps_CallsProcessAndNotSaveExpectedNumberOfTimes()
    {
        // Arrange
        var stepData = _fixture.CreateMany<int>(5)
            .Select(_ => _fixture
                .Build<(Guid ApplicationId, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)> Checklist, IEnumerable<ProcessStep> ProcessSteps)>()
                .With(x => x.Checklist, Enum.GetValues<ApplicationChecklistEntryTypeId>().Select(type => (type, _fixture.Create<ApplicationChecklistEntryStatusId>())))
                .With(x => x.ProcessSteps, _fixture.CreateMany<int>(5).Select(_ => _fixture.Build<ProcessStep>().With(x => x.ProcessStepStatusId, ProcessStepStatusId.DONE).Create()))
                .Create()).ToImmutableArray();

        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData())
            .Returns(stepData.ToAsyncEnumerable());

        A.CallTo(() => _checklistProcessor.ProcessChecklist(A<Guid>._,A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._,A<IEnumerable<ProcessStep>>._,A<CancellationToken>._))
            .ReturnsLazily((Guid _, IEnumerable<(ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId)> _, IEnumerable<ProcessStep> processSteps, CancellationToken _) =>
                processSteps.Where(step => step.ProcessStepStatusId == ProcessStepStatusId.TODO).Select(step => (_fixture.Create<ApplicationChecklistEntryTypeId>(), _fixture.Create<ApplicationChecklistEntryStatusId>())).ToAsyncEnumerable());

        // Act
        await _service.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _checklistProcessor.ProcessChecklist(A<Guid>._,A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._,A<IEnumerable<ProcessStep>>._,A<CancellationToken>._))
            .MustHaveHappened(stepData.Length,Times.Exactly);
        A.CallTo(() => _checklistCreationService.CreateMissingChecklistItems(A<Guid>._,A<IEnumerable<ApplicationChecklistEntryTypeId>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _checklistCreationService.CreateInitialProcessSteps(A<Guid>._,A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WithPendingApplications_NoEntriesCreation_CallsProcessAndSaveExpectedNumberOfTimes()
    {
        // Arrange
        var stepData = _fixture.CreateMany<int>(5)
            .Select(_ => _fixture
                .Build<(Guid ApplicationId, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)> Checklist, IEnumerable<ProcessStep> ProcessSteps)>()
                .With(x => x.Checklist, Enum.GetValues<ApplicationChecklistEntryTypeId>().Select(type => (type, _fixture.Create<ApplicationChecklistEntryStatusId>())))
                .With(x => x.ProcessSteps, _fixture.CreateMany<ProcessStepStatusId>(5).Select(status => _fixture.Build<ProcessStep>().With(x => x.ProcessStepStatusId, status).Create()))
                .Create()).ToImmutableArray();

        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData())
            .Returns(stepData.ToAsyncEnumerable());

        A.CallTo(() => _checklistProcessor.ProcessChecklist(A<Guid>._,A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._,A<IEnumerable<ProcessStep>>._,A<CancellationToken>._))
            .ReturnsLazily((Guid _, IEnumerable<(ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId)> _, IEnumerable<ProcessStep> processSteps, CancellationToken _) =>
                processSteps.Where(step => step.ProcessStepStatusId == ProcessStepStatusId.TODO).Select(step => (_fixture.Create<ApplicationChecklistEntryTypeId>(), _fixture.Create<ApplicationChecklistEntryStatusId>())).ToAsyncEnumerable());

        // Act
        await _service.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _checklistProcessor.ProcessChecklist(A<Guid>._,A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._,A<IEnumerable<ProcessStep>>._,A<CancellationToken>._))
            .MustHaveHappened(stepData.Length,Times.Exactly);
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappened(stepData.SelectMany(entry => entry.ProcessSteps).Where(step => step.ProcessStepStatusId == ProcessStepStatusId.TODO).Count(),Times.Exactly);
    }

    [Fact]
    public async Task ExecuteAsync_WithPendingApplications_WithEntriesCreation_NoExecutableProcessSteps_CallsProcessAndSaveExpectedNumberOfTimes()
    {
        // Arrange
        var stepData = _fixture.CreateMany<int>(5)
            .Select(_ => _fixture
                .Build<(Guid ApplicationId, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)> Checklist, IEnumerable<ProcessStep> ProcessSteps)>()
                .With(x => x.Checklist, _fixture.CreateMany<ApplicationChecklistEntryTypeId>(Enum.GetValues<ApplicationChecklistEntryTypeId>().Length-2).Select(type => (type, _fixture.Create<ApplicationChecklistEntryStatusId>())))
                .With(x => x.ProcessSteps, _fixture.CreateMany<int>(5).Select(_ => _fixture.Build<ProcessStep>().With(x => x.ProcessStepStatusId, ProcessStepStatusId.DONE).Create()))
                .Create()).ToImmutableArray();

        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData())
            .Returns(stepData.ToAsyncEnumerable());

        A.CallTo(() => _checklistCreationService.CreateMissingChecklistItems(A<Guid>._,A<IEnumerable<ApplicationChecklistEntryTypeId>>._))
            .ReturnsLazily((Guid _, IEnumerable<ApplicationChecklistEntryTypeId> typeIds) =>
                Enum.GetValues<ApplicationChecklistEntryTypeId>()
                    .Except(typeIds)
                    .Select(typeId => (typeId, _fixture.Create<ApplicationChecklistEntryStatusId>())));

        A.CallTo(() => _checklistProcessor.ProcessChecklist(A<Guid>._,A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._,A<IEnumerable<ProcessStep>>._,A<CancellationToken>._))
            .ReturnsLazily((Guid _, IEnumerable<(ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId)> _, IEnumerable<ProcessStep> processSteps, CancellationToken _) =>
                processSteps.Where(step => step.ProcessStepStatusId == ProcessStepStatusId.TODO).Select(step => (_fixture.Create<ApplicationChecklistEntryTypeId>(), _fixture.Create<ApplicationChecklistEntryStatusId>())).ToAsyncEnumerable());

        var messages = new List<string>();

        A.CallTo(() => _mockLogger.Log(A<LogLevel>.That.IsEqualTo(LogLevel.Information), A<Exception?>._, A<string>.That.StartsWith("Processed application")))
            .Invokes((LogLevel _, Exception? _, string message) => messages.Add(message));

        // Act
        await _service.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _checklistProcessor.ProcessChecklist(A<Guid>._,A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._,A<IEnumerable<ProcessStep>>._,A<CancellationToken>._))
            .MustHaveHappened(stepData.Length,Times.Exactly);
        A.CallTo(() => _checklistCreationService.CreateMissingChecklistItems(A<Guid>._,A<IEnumerable<ApplicationChecklistEntryTypeId>>._))
            .MustHaveHappened(stepData.Length,Times.Exactly);
        A.CallTo(() => _checklistCreationService.CreateInitialProcessSteps(A<Guid>._,A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>.That.Matches(entries => entries.Count() == 2)))
            .MustHaveHappened(stepData.Length,Times.Exactly);
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappened(stepData.Length,Times.Exactly);
        
        A.CallTo(() => _mockLogger.Log(A<LogLevel>.That.IsEqualTo(LogLevel.Information), A<Exception?>._, A<string>.That.StartsWith("Processed application"))).MustHaveHappened(stepData.Length,Times.Exactly);

        messages.Should().HaveSameCount(stepData);
        messages.Should().AllSatisfy(message => message.Should().ContainAll(Enum.GetNames<ApplicationChecklistEntryTypeId>()));
    }


    [Fact]
    public async Task ExecuteAsync_WithPendingApplications_WithSuppressedEntriesCreation_NoExecutableProcessSteps_CallsProcessAndSaveExpectedNumberOfTimes()
    {
        // Arrange
        var stepData = _fixture.CreateMany<int>(5)
            .Select(_ => _fixture
                .Build<(Guid ApplicationId, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)> Checklist, IEnumerable<ProcessStep> ProcessSteps)>()
                .With(x => x.Checklist, _fixture.CreateMany<ApplicationChecklistEntryTypeId>(Enum.GetValues<ApplicationChecklistEntryTypeId>().Length-1).Select(type => (type, _fixture.Create<ApplicationChecklistEntryStatusId>())))
                .With(x => x.ProcessSteps, _fixture.CreateMany<int>(5).Select(_ => _fixture.Build<ProcessStep>().With(x => x.ProcessStepStatusId, ProcessStepStatusId.DONE).Create()))
                .Create()).ToImmutableArray();

        A.CallTo(() => _applicationChecklistRepository.GetChecklistProcessStepData())
            .Returns(stepData.ToAsyncEnumerable());

        A.CallTo(() => _checklistCreationService.CreateMissingChecklistItems(A<Guid>._,A<IEnumerable<ApplicationChecklistEntryTypeId>>._))
            .Returns(Enumerable.Empty<(ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId)>());

        A.CallTo(() => _checklistProcessor.ProcessChecklist(A<Guid>._,A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._,A<IEnumerable<ProcessStep>>._,A<CancellationToken>._))
            .ReturnsLazily((Guid _, IEnumerable<(ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId)> _, IEnumerable<ProcessStep> processSteps, CancellationToken _) =>
                processSteps.Where(step => step.ProcessStepStatusId == ProcessStepStatusId.TODO).Select(step => (_fixture.Create<ApplicationChecklistEntryTypeId>(), _fixture.Create<ApplicationChecklistEntryStatusId>())).ToAsyncEnumerable());

        var messages = new List<string>();

        A.CallTo(() => _mockLogger.Log(A<LogLevel>.That.IsEqualTo(LogLevel.Information), A<Exception?>._, A<string>.That.StartsWith("Processed application")))
            .Invokes((LogLevel _, Exception? _, string message) => messages.Add(message));

        // Act
        await _service.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _checklistProcessor.ProcessChecklist(A<Guid>._,A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._,A<IEnumerable<ProcessStep>>._,A<CancellationToken>._))
            .MustHaveHappened(stepData.Length,Times.Exactly);
        A.CallTo(() => _checklistCreationService.CreateMissingChecklistItems(A<Guid>._,A<IEnumerable<ApplicationChecklistEntryTypeId>>._))
            .MustHaveHappened(stepData.Length,Times.Exactly);
        A.CallTo(() => _checklistCreationService.CreateInitialProcessSteps(A<Guid>._,A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>.That.IsEmpty()))
            .MustHaveHappened(stepData.Length,Times.Exactly);
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappened(stepData.Length,Times.Exactly);
        
        A.CallTo(() => _mockLogger.Log(A<LogLevel>.That.IsEqualTo(LogLevel.Information), A<Exception?>._, A<string>.That.StartsWith("Processed application"))).MustHaveHappened(stepData.Length,Times.Exactly);

        messages.Should().HaveSameCount(stepData);
        
        messages.Zip(stepData.Select(data => (data.ApplicationId, TypeIds: data.Checklist.Select(entry => entry.TypeId))))
            .Should()
            .AllSatisfy(zipItem =>
                zipItem.First.Should()
                    .Contain(zipItem.Second.ApplicationId.ToString())
                    .And.ContainAll(zipItem.Second.TypeIds.Select(typeId => Enum.GetName<ApplicationChecklistEntryTypeId>(typeId)))
                    .And.NotContainAny(Enum.GetValues<ApplicationChecklistEntryTypeId>().Except(zipItem.Second.TypeIds).Select(typeId => Enum.GetName<ApplicationChecklistEntryTypeId>(typeId)))
            );
    }
}
