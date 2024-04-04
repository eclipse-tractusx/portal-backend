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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ProcessIdentity;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Worker.Library.Tests;

public class ProcessExecutionServiceTests
{
    private readonly IProcessStepRepository _processStepRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProcessExecutor _processExecutor;
    private readonly IMockLogger<ProcessExecutionService> _mockLogger;
    private readonly ProcessExecutionService _service;
    private readonly IFixture _fixture;
    private readonly IProcessIdentityDataDetermination _processIdentityDataDetermination;

    public ProcessExecutionServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var dateTimeProvider = A.Fake<IDateTimeProvider>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _processStepRepository = A.Fake<IProcessStepRepository>();
        _processExecutor = A.Fake<IProcessExecutor>();
        _processIdentityDataDetermination = A.Fake<IProcessIdentityDataDetermination>();

        _mockLogger = A.Fake<IMockLogger<ProcessExecutionService>>();
        ILogger<ProcessExecutionService> logger = new MockLogger<ProcessExecutionService>(_mockLogger);

        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>())
            .Returns(_processStepRepository);

        var settings = _fixture.Create<ProcessExecutionServiceSettings>();

        var options = Options.Create(settings);
        var serviceProvider = A.Fake<IServiceProvider>();
        A.CallTo(() => serviceProvider.GetService(typeof(IPortalRepositories))).Returns(_portalRepositories);
        A.CallTo(() => serviceProvider.GetService(typeof(IProcessExecutor))).Returns(_processExecutor);
        A.CallTo(() => serviceProvider.GetService(typeof(IProcessIdentityDataDetermination))).Returns(_processIdentityDataDetermination);
        var serviceScope = A.Fake<IServiceScope>();
        A.CallTo(() => serviceScope.ServiceProvider).Returns(serviceProvider);
        var serviceScopeFactory = A.Fake<IServiceScopeFactory>();
        A.CallTo(() => serviceScopeFactory.CreateScope()).Returns(serviceScope);
        A.CallTo(() => serviceProvider.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactory);

        _service = new ProcessExecutionService(serviceScopeFactory, dateTimeProvider, options, logger);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoPendingItems_NoServiceCall()
    {
        // Arrange
        A.CallTo(() => _processStepRepository.GetActiveProcesses(A<IEnumerable<ProcessTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._, A<DateTimeOffset>._))
            .Returns(Array.Empty<Process>().ToAsyncEnumerable());

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _processIdentityDataDetermination.GetIdentityData()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._, A<ProcessTypeId>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WithPendingItems_CallsProcessExpectedNumberOfTimes()
    {
        // Arrange
        var processData = _fixture.CreateMany<Guid>().Select(x => new Process(x, ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid())).ToImmutableArray();
        A.CallTo(() => _processStepRepository.GetActiveProcesses(A<IEnumerable<ProcessTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._, A<DateTimeOffset>._))
            .Returns(processData.ToAsyncEnumerable());

        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._, A<ProcessTypeId>._, A<CancellationToken>._))
            .Returns(Enumerable.Repeat(IProcessExecutor.ProcessExecutionResult.SaveRequested, 2).ToAsyncEnumerable());

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._, A<ProcessTypeId>._, A<CancellationToken>._))
            .MustHaveHappened(processData.Length, Times.Exactly);
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappened(processData.Length * 2, Times.Exactly);
        A.CallTo(() => _portalRepositories.Clear())
            .MustHaveHappened(processData.Length * 2, Times.Exactly);
    }

    [Fact]
    public async Task ExecuteAsync_ExecuteProcess_Returns_Unmodified()
    {
        // Arrange
        var processData = _fixture.CreateMany<Guid>().Select(x => new Process(x, ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid())).ToImmutableArray();
        A.CallTo(() => _processStepRepository.GetActiveProcesses(A<IEnumerable<ProcessTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._, A<DateTimeOffset>._))
            .Returns(processData.ToAsyncEnumerable());

        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._, A<ProcessTypeId>._, A<CancellationToken>._))
            .Returns(Enumerable.Repeat(IProcessExecutor.ProcessExecutionResult.Unmodified, 2).ToAsyncEnumerable());

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._, A<ProcessTypeId>._, A<CancellationToken>._))
            .MustHaveHappened(processData.Length, Times.Exactly);
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.Clear())
            .MustHaveHappened(processData.Length * 2, Times.Exactly);
    }

    [Fact]
    public async Task ExecuteAsync_ExecuteProcess_Returns_RequestLock()
    {
        // Arrange
        var processId = Guid.NewGuid();
        var processVersion = Guid.NewGuid();
        var process = new Process(processId, ProcessTypeId.APPLICATION_CHECKLIST, processVersion);
        var processData = new[] { process };
        A.CallTo(() => _processStepRepository.GetActiveProcesses(A<IEnumerable<ProcessTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._, A<DateTimeOffset>._))
            .Returns(processData.ToAsyncEnumerable());

        A.CallTo(() => _processExecutor.ExecuteProcess(processId, A<ProcessTypeId>._, A<CancellationToken>._))
            .Returns(new[] { IProcessExecutor.ProcessExecutionResult.LockRequested, IProcessExecutor.ProcessExecutionResult.SaveRequested }.ToAsyncEnumerable());

        var changeHistory = new List<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>();

        A.CallTo(() => _portalRepositories.SaveAsync())
            .ReturnsLazily(() =>
            {
                changeHistory.Add((process.Version, process.LockExpiryDate, true));
                return 1;
            });

        A.CallTo(() => _portalRepositories.Clear())
            .Invokes(() =>
            {
                changeHistory.Add((process.Version, process.LockExpiryDate, false));
            });

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._, A<ProcessTypeId>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => _portalRepositories.Clear())
            .MustHaveHappened(3, Times.Exactly);
        changeHistory.Should().HaveCount(6)
            .And.SatisfyRespectively(
                first => first.Should().Match<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Version != processVersion && x.LockExpiryTime != null && x.Save),
                second => second.Should().Match<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Version == changeHistory[0].Version && x.LockExpiryTime == changeHistory[0].LockExpiryTime && !x.Save),
                third => third.Should().Match<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Version == changeHistory[1].Version && x.LockExpiryTime == changeHistory[1].LockExpiryTime && x.Save),
                forth => forth.Should().Match<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Version == changeHistory[2].Version && x.LockExpiryTime == changeHistory[2].LockExpiryTime && !x.Save),
                fifth => fifth.Should().Match<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Version != changeHistory[3].Version && x.LockExpiryTime == null && x.Save),
                sixth => sixth.Should().Match<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Version == changeHistory[4].Version && x.LockExpiryTime == null && !x.Save)
            );
    }

    [Fact]
    public async Task ExecuteAsync_ExecuteProcess_Returns_RequestLockTwice()
    {
        // Arrange
        var processId = Guid.NewGuid();
        var processVersion = Guid.NewGuid();
        var process = new Process(processId, ProcessTypeId.APPLICATION_CHECKLIST, processVersion);
        var processData = new[] { process };
        A.CallTo(() => _processStepRepository.GetActiveProcesses(A<IEnumerable<ProcessTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._, A<DateTimeOffset>._))
            .Returns(processData.ToAsyncEnumerable());

        A.CallTo(() => _processExecutor.ExecuteProcess(processId, A<ProcessTypeId>._, A<CancellationToken>._))
            .Returns(new[] { IProcessExecutor.ProcessExecutionResult.LockRequested, IProcessExecutor.ProcessExecutionResult.LockRequested, IProcessExecutor.ProcessExecutionResult.SaveRequested }.ToAsyncEnumerable());

        var changeHistory = new List<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>();

        A.CallTo(() => _portalRepositories.SaveAsync())
            .ReturnsLazily(() =>
            {
                changeHistory.Add((process.Version, process.LockExpiryDate, true));
                return 1;
            });

        A.CallTo(() => _portalRepositories.Clear())
            .Invokes(() =>
            {
                changeHistory.Add((process.Version, process.LockExpiryDate, false));
            });

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._, A<ProcessTypeId>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => _portalRepositories.Clear())
            .MustHaveHappened(4, Times.Exactly);
        changeHistory.Should().HaveCount(7)
            .And.SatisfyRespectively(
                first => first.Should().Match<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Version != processVersion && x.LockExpiryTime != null && x.Save),
                second => second.Should().Match<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Version == changeHistory[0].Version && x.LockExpiryTime == changeHistory[0].LockExpiryTime && !x.Save),
                third => third.Should().Match<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Version == changeHistory[1].Version && x.LockExpiryTime == changeHistory[1].LockExpiryTime && !x.Save),
                forth => forth.Should().Match<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Version == changeHistory[2].Version && x.LockExpiryTime == changeHistory[2].LockExpiryTime && x.Save),
                fifth => fifth.Should().Match<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Version == changeHistory[3].Version && x.LockExpiryTime == changeHistory[3].LockExpiryTime && !x.Save),
                sixth => sixth.Should().Match<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Version != changeHistory[4].Version && x.LockExpiryTime == null && x.Save),
                seventh => seventh.Should().Match<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Version == changeHistory[5].Version && x.LockExpiryTime == null && !x.Save)
            );
    }

    [Fact]
    public async Task ExecuteAsync_ExecuteProcess_Returns_RequestLockThenThrows()
    {
        // Arrange
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var firstVersion = Guid.NewGuid();
        var secondVersion = Guid.NewGuid();
        var firstProcess = new Process(firstId, ProcessTypeId.APPLICATION_CHECKLIST, firstVersion);
        var secondProcess = new Process(secondId, ProcessTypeId.APPLICATION_CHECKLIST, secondVersion);

        var processData = new[] { firstProcess, secondProcess };
        A.CallTo(() => _processStepRepository.GetActiveProcesses(A<IEnumerable<ProcessTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._, A<DateTimeOffset>._))
            .Returns(processData.ToAsyncEnumerable());

        IEnumerable<IProcessExecutor.ProcessExecutionResult> ThrowingEnumerable()
        {
            yield return IProcessExecutor.ProcessExecutionResult.LockRequested;
            throw new Exception("normal error");
        }

        Process? process = null;

        A.CallTo(() => _processExecutor.ExecuteProcess(firstId, A<ProcessTypeId>._, A<CancellationToken>._))
            .ReturnsLazily((Guid Id, ProcessTypeId _, CancellationToken _) =>
            {
                process = firstProcess;
                return ThrowingEnumerable().ToAsyncEnumerable();
            });

        A.CallTo(() => _processExecutor.ExecuteProcess(secondId, A<ProcessTypeId>._, A<CancellationToken>._))
            .ReturnsLazily((Guid Id, ProcessTypeId _, CancellationToken _) =>
            {
                process = secondProcess;
                return new[] { IProcessExecutor.ProcessExecutionResult.SaveRequested }.ToAsyncEnumerable();
            });

        var changeHistory = new List<(Guid Id, Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>();

        A.CallTo(() => _portalRepositories.SaveAsync())
            .ReturnsLazily(() =>
            {
                changeHistory.Add(process == null
                    ? (Guid.Empty, Guid.Empty, null, true)
                    : (process.Id, process.Version, process.LockExpiryDate, true));
                return 1;
            });

        A.CallTo(() => _portalRepositories.Clear())
            .Invokes(() =>
            {
                changeHistory.Add(process == null
                    ? (Guid.Empty, Guid.Empty, null, false)
                    : (process.Id, process.Version, process.LockExpiryDate, false));
            });

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _processExecutor.ExecuteProcess(firstId, A<ProcessTypeId>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _processExecutor.ExecuteProcess(secondId, A<ProcessTypeId>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappenedTwiceExactly();
        A.CallTo(() => _portalRepositories.Clear())
            .MustHaveHappened(3, Times.Exactly);
        changeHistory.Should().SatisfyRespectively(
                first => first.Should().Match<(Guid Id, Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Id == firstId && x.Version != firstVersion && x.LockExpiryTime != null && x.Save),
                second => second.Should().Match<(Guid Id, Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Id == firstId && x.Version == changeHistory[0].Version && x.LockExpiryTime == changeHistory[0].LockExpiryTime && !x.Save),
                second => second.Should().Match<(Guid Id, Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Id == firstId && x.Version == changeHistory[1].Version && x.LockExpiryTime == changeHistory[0].LockExpiryTime && !x.Save),
                third => third.Should().Match<(Guid Id, Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Id == secondId && x.Version != secondVersion && x.LockExpiryTime == null && x.Save),
                forth => forth.Should().Match<(Guid Id, Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Id == secondId && x.Version == changeHistory[3].Version && x.LockExpiryTime == null && !x.Save)
            );
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, A<Exception>.That.Matches(e => e != null && e.Message == "normal error"), A<string>.That.StartsWith("error processing process"))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_ExecuteProcess_Returns_UnmodifiedSafeRequested()
    {
        // Arrange
        var firstVersion = Guid.NewGuid();
        var process = new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, firstVersion);
        var processData = new[] { process };
        A.CallTo(() => _processStepRepository.GetActiveProcesses(A<IEnumerable<ProcessTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._, A<DateTimeOffset>._))
            .Returns(processData.ToAsyncEnumerable());

        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._, A<ProcessTypeId>._, A<CancellationToken>._))
            .Returns(new[] { IProcessExecutor.ProcessExecutionResult.Unmodified, IProcessExecutor.ProcessExecutionResult.SaveRequested }.ToAsyncEnumerable());

        var changeHistory = new List<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>();

        A.CallTo(() => _portalRepositories.SaveAsync())
            .ReturnsLazily(() =>
            {
                changeHistory.Add((process.Version, process.LockExpiryDate, true));
                return 1;
            });

        A.CallTo(() => _portalRepositories.Clear())
            .Invokes(() =>
            {
                changeHistory.Add((process.Version, process.LockExpiryDate, false));
            });

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._, A<ProcessTypeId>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.Clear())
            .MustHaveHappenedTwiceExactly();
        changeHistory.Should().HaveCount(3)
            .And.SatisfyRespectively(
                first => first.Should().Match<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Version == firstVersion && x.LockExpiryTime == null && !x.Save),
                second => second.Should().Match<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Version != changeHistory[0].Version && x.LockExpiryTime == null && x.Save),
                third => third.Should().Match<(Guid Version, DateTimeOffset? LockExpiryTime, bool Save)>(x => x.Version == changeHistory[1].Version && x.LockExpiryTime == null && !x.Save)
            );
    }

    [Fact]
    public async Task ExecuteAsync_ExecuteProcess_Returns_RequestLock_SaveAsyncThrows()
    {
        // Arrange
        var firstProcessId = Guid.NewGuid();
        var firstVersion = Guid.NewGuid();
        var firstProcess = new Process(firstProcessId, ProcessTypeId.APPLICATION_CHECKLIST, firstVersion);
        var secondProcessId = Guid.NewGuid();
        var secondVersion = Guid.NewGuid();
        var secondProcess = new Process(secondProcessId, ProcessTypeId.APPLICATION_CHECKLIST, secondVersion);
        var thirdProcessId = Guid.NewGuid();
        var thirdVersion = Guid.NewGuid();
        var thirdProcess = new Process(thirdProcessId, ProcessTypeId.APPLICATION_CHECKLIST, thirdVersion);
        var processData = new[] { firstProcess, secondProcess, thirdProcess };
        var error = new Exception("save conflict error");
        A.CallTo(() => _processStepRepository.GetActiveProcesses(A<IEnumerable<ProcessTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._, A<DateTimeOffset>._))
            .Returns(processData.ToAsyncEnumerable());

        Process? process = null;

        A.CallTo(() => _processExecutor.ExecuteProcess(firstProcessId, A<ProcessTypeId>._, A<CancellationToken>._))
            .ReturnsLazily((Guid _, ProcessTypeId _, CancellationToken _) =>
            {
                process = firstProcess;
                return new[] { IProcessExecutor.ProcessExecutionResult.LockRequested, IProcessExecutor.ProcessExecutionResult.SaveRequested }.ToAsyncEnumerable();
            });

        A.CallTo(() => _processExecutor.ExecuteProcess(secondProcessId, A<ProcessTypeId>._, A<CancellationToken>._))
            .ReturnsLazily((Guid _, ProcessTypeId _, CancellationToken _) =>
            {
                process = secondProcess;
                return new[] { IProcessExecutor.ProcessExecutionResult.SaveRequested }.ToAsyncEnumerable();
            });

        A.CallTo(() => _processExecutor.ExecuteProcess(thirdProcessId, A<ProcessTypeId>._, A<CancellationToken>._))
            .ReturnsLazily((Guid _, ProcessTypeId _, CancellationToken _) =>
            {
                process = thirdProcess;
                return new[] { IProcessExecutor.ProcessExecutionResult.Unmodified }.ToAsyncEnumerable();
            });

        var changeHistory = new List<(Guid Id, Guid Version, DateTimeOffset? LockExpiryTime)>();

        A.CallTo(() => _portalRepositories.SaveAsync())
            .Throws(error);

        A.CallTo(() => _portalRepositories.Clear())
            .Invokes(() =>
            {
                changeHistory.Add(
                    process == null
                        ? (Guid.Empty, Guid.Empty, null)
                        : (process.Id, process.Version, process.LockExpiryDate));
            });

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._, A<ProcessTypeId>._, A<CancellationToken>._))
            .MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappenedTwiceExactly();
        A.CallTo(() => _portalRepositories.Clear())
            .MustHaveHappened(3, Times.Exactly);
        changeHistory.Should().HaveCount(3)
            .And.SatisfyRespectively(
                first => first.Should().Match<(Guid Id, Guid Version, DateTimeOffset? LockExpiryTime)>(x => x.Id == firstProcessId && x.Version != firstVersion && x.LockExpiryTime != null),
                second => second.Should().Match<(Guid Id, Guid Version, DateTimeOffset? LockExpiryTime)>(x => x.Id == secondProcessId && x.Version != secondVersion && x.LockExpiryTime == null),
                third => third.Should().Match<(Guid Id, Guid Version, DateTimeOffset? LockExpiryTime)>(x => x.Id == thirdProcessId && x.Version == thirdVersion && x.LockExpiryTime == null)
            );
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, A<Exception>.That.Matches(e => e != null && e.Message == error.Message), A<string>.That.StartsWith($"error processing process {firstProcessId}")))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, A<Exception>.That.Matches(e => e != null && e.Message == error.Message), A<string>.That.StartsWith($"error processing process {secondProcessId}")))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, A<Exception>.That.Matches(e => e != null && e.Message == error.Message), A<string>.That.StartsWith($"error processing process {thirdProcessId}")))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_IgnoresLockedProcesses_LogsInformation()
    {
        var lockExpiryDate = _fixture.Create<DateTimeOffset>();
        // Arrange
        var processData = _fixture.CreateMany<Guid>().Select(x => new Process(x, ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid()) { LockExpiryDate = lockExpiryDate }).ToImmutableArray();
        A.CallTo(() => _processStepRepository.GetActiveProcesses(A<IEnumerable<ProcessTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._, A<DateTimeOffset>._))
            .Returns(processData.ToAsyncEnumerable());

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, A<Exception>.That.IsNull(), A<string>.That.StartsWith("skipping locked process")))
            .MustHaveHappened(processData.Length, Times.Exactly);
        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._, A<ProcessTypeId>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.Clear())
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WithException_LogsError()
    {
        // Arrange
        var processData = _fixture.CreateMany<Guid>().Select(x => new Process(x, ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid())).ToImmutableArray();
        var error = new Exception("Only a test");
        A.CallTo(() => _processStepRepository.GetActiveProcesses(A<IEnumerable<ProcessTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._, A<DateTimeOffset>._))
            .Returns(processData.ToAsyncEnumerable());
        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._, A<ProcessTypeId>._, A<CancellationToken>._))
            .Throws(error);

        Environment.ExitCode = 0;

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        Environment.ExitCode.Should().Be(0);
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, A<Exception>.That.IsNull(), A<string>.That.Matches(x => x.StartsWith("start processing process")))).MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, A<Exception>.That.Matches(e => e != null && e.Message == error.Message), A<string>.That.StartsWith("error processing process"))).MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, A<Exception>.That.IsNull(), A<string>.That.Matches(x => x.StartsWith("finished processing process")))).MustNotHaveHappened();
        A.CallTo(() => _mockLogger.Log(LogLevel.Error, A<Exception>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WithSystemException_Exits()
    {
        // Arrange
        var processData = _fixture.CreateMany<Guid>().Select(x => new Process(x, ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid())).ToImmutableArray();
        var error = new SystemException("unrecoverable failure");
        A.CallTo(() => _processStepRepository.GetActiveProcesses(A<IEnumerable<ProcessTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._, A<DateTimeOffset>._))
            .Returns(processData.ToAsyncEnumerable());
        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._, A<ProcessTypeId>._, A<CancellationToken>._))
            .Throws(error);

        Environment.ExitCode = 0;

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        Environment.ExitCode.Should().Be(1);
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, A<Exception>.That.IsNull(), A<string>.That.Matches(x => x.StartsWith("start processing process")))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, A<Exception>.That.IsNotNull(), A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, A<Exception>.That.IsNull(), A<string>.That.Matches(x => x.StartsWith("finished processing process")))).MustNotHaveHappened();
        A.CallTo(() => _mockLogger.Log(LogLevel.Error, A<Exception>.That.Matches(e => e != null && e.Message == error.Message), $"processing failed with following Exception {error.Message}")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }
}
