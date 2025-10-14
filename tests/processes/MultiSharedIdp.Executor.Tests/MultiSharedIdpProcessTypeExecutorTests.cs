/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.MultiSharedIdp.Executor.Tests;

public class MultiSharedIdpProcessTypeExecutorTests
{
    private readonly Guid _sharedProcessId = Guid.NewGuid();
    private readonly Guid _ownProcessId = Guid.NewGuid();
    private readonly IFixture _fixture;
    private readonly MultiSharedIdpProcessTypeExecutor _executor;
    private readonly ISharedMultiKeycloakManager _sharedMultiKeycloakManager;

    public MultiSharedIdpProcessTypeExecutorTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _sharedMultiKeycloakManager = A.Fake<ISharedMultiKeycloakManager>();

        _executor = new MultiSharedIdpProcessTypeExecutor(_sharedMultiKeycloakManager);
    }

    #region GetProcessTypeId

    [Fact]
    public void GetProcessTypeId_ReturnsExpected()
    {
        // Act
        var result = _executor.GetProcessTypeId();

        // Assert
        result.Should().Be(ProcessTypeId.MULTI_SHARED_IDENTITY_PROVIDER);
    }

    #endregion

    #region IsLockRequested

    [Fact]
    public async Task IsLockRequested_ReturnsExpected()
    {
        // Act
        var result = await _executor.IsLockRequested(_fixture.Create<ProcessStepTypeId>());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsExecutableStepTypeId

    [Theory]
    [InlineData(ProcessStepTypeId.SYNC_MULTI_SHARED_IDP, true)]
    public void IsExecutableProcessStep_ReturnsExpected(ProcessStepTypeId processStepTypeId, bool executable)
    {
        // Act
        var result = _executor.IsExecutableStepTypeId(processStepTypeId);

        // Assert
        result.Should().Be(executable);
    }

    #endregion

    #region GetExecutableStepTypeIds

    [Fact]
    public void GetExecutableStepTypeIds_ReturnsExpected()
    {
        //Act
        var result = _executor.GetExecutableStepTypeIds();

        // Assert
        result.Should().HaveCount(1)
            .And.Satisfy(
                x => x == ProcessStepTypeId.SYNC_MULTI_SHARED_IDP);
    }

    #endregion

    #region InitializeProcess

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task InitializeProcess_ValidProcessId_ReturnsExpected(bool shared)
    {
        // Arrange
        var processId = shared ? _sharedProcessId : _ownProcessId;

        // Act
        var result = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>());

        // Assert
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    #endregion

    #region ExecuteProcessStep

    [Fact]
    public async Task ExecuteProcessStep_InitializeNotCalled_Throws()
    {
        // Arrange initialize
        var processId = Guid.NewGuid();

        // Act initialize
        var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>());

        // Assert initialize
        initializationResult.Should().NotBeNull();
        initializationResult.Modified.Should().BeFalse();
        initializationResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange execute
        var error = new NullReferenceException(_fixture.Create<string>());
        A.CallTo(() => _sharedMultiKeycloakManager.SyncMultiSharedIdpAsync())
            .Throws(error);

        // Act execute
        async Task Act() => await _executor.ExecuteProcessStep(ProcessStepTypeId.SYNC_MULTI_SHARED_IDP, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);
        var ex = await Assert.ThrowsAsync<NullReferenceException>(Act);

        // Assert execute
        ex.Message.Should().Be(error.Message);
    }

    [Theory]
    [InlineData(true, ProcessStepTypeId.SYNC_MULTI_SHARED_IDP, null, ProcessStepStatusId.DONE, null, true)]
    public async Task ExecuteProcessStep_WithValidTriggerData_CallsExpected(bool shared, ProcessStepTypeId processStepTypeId, ProcessStepTypeId? nextprocessStepTypeId, ProcessStepStatusId stepStatus, string? message, bool modified)
    {
        // Arrange
        var processId = shared ? _sharedProcessId : _ownProcessId;

        // Act InitializeProcess
        var initializeResult = await _executor.InitializeProcess(processId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert InitializeProcess
        initializeResult.Modified.Should().BeFalse();
        initializeResult.ScheduleStepTypeIds.Should().BeNull();

        // Act
        var result = await _executor.ExecuteProcessStep(processStepTypeId, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        result.Modified.Should().Be(modified);
        if (nextprocessStepTypeId == null)
        {
            result.ScheduleStepTypeIds.Should().BeNullOrEmpty();
        }
        else
        {
            result.ScheduleStepTypeIds.Should().ContainSingle()
                .Which.Should().Be(nextprocessStepTypeId);
        }

        result.ProcessStepStatusId.Should().Be(stepStatus);
        result.ProcessMessage.Should().Be(message);
        result.SkipStepTypeIds.Should().BeNull();
    }

    #endregion
}
