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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Worker.Library.Tests;

public class ProcessExecutionServiceTests
{
    private readonly IProcessStepRepository _processStepRepository;

    private readonly IPortalRepositories _portalRepositories;
    private readonly IProcessExecutor _processExecutor;
    private readonly IMockLogger<ProcessExecutionService> _mockLogger;
    private readonly ILogger<ProcessExecutionService> _logger;
    private readonly ProcessExecutionService _service;
    private readonly IFixture _fixture;

    public ProcessExecutionServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b =>_fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _processStepRepository = A.Fake<IProcessStepRepository>();
        _processExecutor = A.Fake<IProcessExecutor>();

        _mockLogger = A.Fake<IMockLogger<ProcessExecutionService>>();
        _logger = new MockLogger<ProcessExecutionService>(_mockLogger);

        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>())
            .Returns(_processStepRepository);

        var serviceProvider = A.Fake<IServiceProvider>();
        A.CallTo(() => serviceProvider.GetService(typeof(IPortalRepositories))).Returns(_portalRepositories);
        A.CallTo(() => serviceProvider.GetService(typeof(IProcessExecutor))).Returns(_processExecutor);
        var serviceScope = A.Fake<IServiceScope>();
        A.CallTo(() => serviceScope.ServiceProvider).Returns(serviceProvider);
        var serviceScopeFactory = A.Fake<IServiceScopeFactory>();
        A.CallTo(() => serviceScopeFactory.CreateScope()).Returns(serviceScope);
        A.CallTo(() => serviceProvider.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactory);

        _service = new ProcessExecutionService(serviceScopeFactory, _logger);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoPendingItems_NoServiceCall()
    {
        // Arrange
        A.CallTo(() => _processStepRepository.GetActiveProcesses(A<IEnumerable<ProcessTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(Array.Empty<(Guid ProcessId, ProcessTypeId)>().ToAsyncEnumerable());

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._,A<ProcessTypeId>._,A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WithPendingItems_CallsProcessExpectedNumberOfTimes()
    {
        // Arrange
        var processData = _fixture.CreateMany<(Guid ProcessId, ProcessTypeId)>().ToImmutableArray();
        A.CallTo(() => _processStepRepository.GetActiveProcesses(A<IEnumerable<ProcessTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(processData.ToAsyncEnumerable());

        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._,A<ProcessTypeId>._,A<CancellationToken>._))
            .Returns(Enumerable.Repeat<bool>(true, 2).ToAsyncEnumerable());

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._,A<ProcessTypeId>._,A<CancellationToken>._))
            .MustHaveHappened(processData.Length,Times.Exactly);
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappened(processData.Length * 2,Times.Exactly);
        A.CallTo(() => _portalRepositories.Clear())
            .MustHaveHappened(processData.Length * 2,Times.Exactly);
    }

    [Fact]
    public async Task ExecuteAsync_ExecuteProcess_Returns_Unmodified()
    {
        // Arrange
        var processData = _fixture.CreateMany<(Guid ProcessId, ProcessTypeId)>().ToImmutableArray();
        A.CallTo(() => _processStepRepository.GetActiveProcesses(A<IEnumerable<ProcessTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(processData.ToAsyncEnumerable());

        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._,A<ProcessTypeId>._,A<CancellationToken>._))
            .Returns(Enumerable.Repeat<bool>(false, 2).ToAsyncEnumerable());

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._,A<ProcessTypeId>._,A<CancellationToken>._))
            .MustHaveHappened(processData.Length,Times.Exactly);
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.Clear())
            .MustHaveHappened(processData.Length * 2,Times.Exactly);
    }

    [Fact]
    public async Task ExecuteAsync_WithException_LogsError()
    {
        // Arrange
        var processData = _fixture.CreateMany<(Guid ProcessId, ProcessTypeId ProcessTypeId)>().ToImmutableArray();
        var error = new Exception("Only a test");
        A.CallTo(() => _processStepRepository.GetActiveProcesses(A<IEnumerable<ProcessTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(processData.ToAsyncEnumerable());
        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._,A<ProcessTypeId>._,A<CancellationToken>._))
            .Throws(() => new Exception("Only a test"));

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        Environment.ExitCode.Should().Be(0);
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, A<Exception>.That.Matches(e => e.Message == error.Message), $"error processing process {processData[0].ProcessId} type {processData[0].ProcessTypeId}: {error.Message}")).MustHaveHappened();
        A.CallTo(() => _mockLogger.Log(LogLevel.Error, A<Exception>._, A<string>._)).MustNotHaveHappened();        
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WithSystemException_Exits()
    {
        // Arrange
        var processData = _fixture.CreateMany<(Guid ProcessId, ProcessTypeId)>().ToImmutableArray();
        var error = new SystemException("unrecoverable failure");
        A.CallTo(() => _processStepRepository.GetActiveProcesses(A<IEnumerable<ProcessTypeId>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(processData.ToAsyncEnumerable());
        A.CallTo(() => _processExecutor.ExecuteProcess(A<Guid>._,A<ProcessTypeId>._,A<CancellationToken>._))
            .Throws(() => new SystemException("unrecoverable failure"));

        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        Environment.ExitCode.Should().Be(1);
        A.CallTo(() => _mockLogger.Log(LogLevel.Error, A<Exception>.That.Matches(e => e.Message == error.Message), $"processing failed with following Exception {error.Message}")).MustHaveHappened();
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, A<Exception>._, A<string>._)).MustNotHaveHappened();        
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }
}
