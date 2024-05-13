/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.DimUserCreationProcess.Executor;

namespace Org.Eclipse.TractusX.Portal.Backend.DimUserCreationProcess.Executor.Tests;

public class DimUserCreationProcessTypeExecutorTests
{
    private readonly Guid _dimServiceAccountId = Guid.NewGuid();
    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly IDimUserCreationProcessService _dimUserCreationProcessService;
    private readonly DimUserCreationProcessTypeExecutor _executor;
    private readonly IFixture _fixture;
    private readonly IEnumerable<ProcessStepTypeId> _executableSteps;

    public DimUserCreationProcessTypeExecutorTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var portalRepositories = A.Fake<IPortalRepositories>();
        _serviceAccountRepository = A.Fake<IServiceAccountRepository>();

        _dimUserCreationProcessService = A.Fake<IDimUserCreationProcessService>();

        A.CallTo(() => portalRepositories.GetInstance<IServiceAccountRepository>())
            .Returns(_serviceAccountRepository);

        _executor = new DimUserCreationProcessTypeExecutor(
            portalRepositories,
            _dimUserCreationProcessService);

        _executableSteps = new[] {
            ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER,
        };
    }

    #region InitializeProcess

    [Fact]
    public async Task InitializeProcess_WithExisting_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();

        A.CallTo(() => _serviceAccountRepository.GetDimServiceAccountIdForProcess(processId))
            .Returns(_dimServiceAccountId);

        // Act
        var result = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>());

        // Assert
        result.Should().NotBeNull();
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task InitializeProcess_WithNotExistingInvitation_Throws()
    {
        // Arrange
        var processId = Guid.NewGuid();

        A.CallTo(() => _serviceAccountRepository.GetDimServiceAccountIdForProcess(processId))
            .Returns(Guid.Empty);

        // Act
        async Task Act() => await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>());
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be($"process {processId} does not exist or is not associated with an dim service account");
    }

    #endregion

    #region ExecuteProcessStep

    [Fact]
    public async Task ExecuteProcessStep_InitializeNotCalled_Throws()
    {
        // Arrange
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();

        async Task Act() => await _executor.ExecuteProcessStep(processStepTypeId, processStepTypeIds, CancellationToken.None);

        // Act
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        // Assert
        result.Message.Should().Be("dimServiceAccountId should never be empty here");
    }

    [Fact]
    public async Task ExecuteProcessStep_ReturnsExpected()
    {
        // Arrange initialize
        var processId = Guid.NewGuid();

        A.CallTo(() => _serviceAccountRepository.GetDimServiceAccountIdForProcess(processId))
            .Returns(_dimServiceAccountId);

        // Act initialize
        var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>());

        // Assert initialize
        initializationResult.Should().NotBeNull();
        initializationResult.Modified.Should().BeFalse();
        initializationResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange execute
        SetupFakes();

        // Act execute
        var executionResult = await _executor.ExecuteProcessStep(ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert execute
        executionResult.Modified.Should().BeTrue();
        executionResult.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        executionResult.ScheduleStepTypeIds.Should().ContainSingle().And.Satisfy(x => x == ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE);
        executionResult.SkipStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteProcessStep_ThrowingTestException_ReturnsExpected()
    {
        // Arrange initialize
        var processId = Guid.NewGuid();
        var dimServiceAccountId = Guid.NewGuid();

        A.CallTo(() => _serviceAccountRepository.GetDimServiceAccountIdForProcess(processId))
            .Returns(dimServiceAccountId);

        // Act initialize
        var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>());

        // Assert initialize
        initializationResult.Should().NotBeNull();
        initializationResult.Modified.Should().BeFalse();
        initializationResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange execute
        var error = _fixture.Create<TestException>();
        A.CallTo(() => _dimUserCreationProcessService.CreateDimUser(processId, dimServiceAccountId, A<CancellationToken>._))
            .Throws(error);

        // Act execute
        var executionResult = await _executor.ExecuteProcessStep(ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert execute
        A.CallTo(() => _dimUserCreationProcessService.CreateDimUser(processId, dimServiceAccountId, CancellationToken.None))
            .MustHaveHappenedOnceExactly();

        executionResult.Modified.Should().BeTrue();
        executionResult.ProcessStepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        executionResult.ScheduleStepTypeIds.Should().ContainInOrder(ProcessStepTypeId.RETRIGGER_CREATE_DIM_TECHNICAL_USER);
        executionResult.SkipStepTypeIds.Should().BeNull();
        executionResult.ProcessMessage.Should().Be(error.Message);
    }

    [Fact]
    public async Task ExecuteProcessStep_ThrowingRecoverableServiceException_ReturnsExpected()
    {
        // Arrange initialize
        var processId = Guid.NewGuid();
        var dimServiceAccountId = Guid.NewGuid();

        A.CallTo(() => _serviceAccountRepository.GetDimServiceAccountIdForProcess(processId))
            .Returns(dimServiceAccountId);

        // Act initialize
        var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>());

        // Assert initialize
        initializationResult.Should().NotBeNull();
        initializationResult.Modified.Should().BeFalse();
        initializationResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange execute
        var error = new ServiceException(_fixture.Create<string>(), true);
        A.CallTo(() => _dimUserCreationProcessService.CreateDimUser(processId, dimServiceAccountId, A<CancellationToken>._))
            .Throws(error);

        // Act execute
        var executionResult = await _executor.ExecuteProcessStep(ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert execute
        A.CallTo(() => _dimUserCreationProcessService.CreateDimUser(processId, dimServiceAccountId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        executionResult.Modified.Should().BeTrue();
        executionResult.ProcessStepStatusId.Should().Be(ProcessStepStatusId.TODO);
        executionResult.ScheduleStepTypeIds.Should().BeNull();
        executionResult.SkipStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteProcessStep_ThrowingSystemException_Throws()
    {
        // Arrange initialize
        var processId = Guid.NewGuid();
        var dimServiceAccountId = Guid.NewGuid();

        A.CallTo(() => _serviceAccountRepository.GetDimServiceAccountIdForProcess(processId))
            .Returns(dimServiceAccountId);

        // Act initialize
        var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>());

        // Assert initialize
        initializationResult.Should().NotBeNull();
        initializationResult.Modified.Should().BeFalse();
        initializationResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange execute
        var error = new SystemException(_fixture.Create<string>());
        A.CallTo(() => _dimUserCreationProcessService.CreateDimUser(processId, dimServiceAccountId, CancellationToken.None))
            .Throws(error);

        // Act execute
        async Task Act() => await _executor.ExecuteProcessStep(ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);
        var ex = await Assert.ThrowsAsync<SystemException>(Act);

        // Assert execute
        ex.Message.Should().Be(error.Message);
    }

    #endregion

    #region GetProcessTypeId

    [Fact]
    public void GetProcessTypeId_ReturnsExpected()
    {
        // Act
        var result = _executor.GetProcessTypeId();

        // Assert
        result.Should().Be(ProcessTypeId.DIM_TECHNICAL_USER);
    }

    #endregion

    #region IsExecutableStepTypeId

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsExecutableProcessStep_ReturnsExpected(bool checklistHandlerReturnValue)
    {
        // Arrange
        var processStepTypeId = checklistHandlerReturnValue ? ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER : ProcessStepTypeId.START_AUTOSETUP;

        // Act
        var result = _executor.IsExecutableStepTypeId(processStepTypeId);

        // Assert
        result.Should().Be(checklistHandlerReturnValue);
    }

    #endregion

    #region IsLockRequested

    [Theory]
    [InlineData(ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER, false)]
    public async Task IsLockRequested_ReturnsExpected(ProcessStepTypeId stepTypeId, bool isLocked)
    {
        // Act
        var result = await _executor.IsLockRequested(stepTypeId);

        // Assert
        result.Should().Be(isLocked);
    }

    #endregion

    #region GetExecutableStepTypeIds

    [Fact]
    public void GetExecutableStepTypeIds_ReturnsExpected()
    {
        //Act
        var result = _executor.GetExecutableStepTypeIds();

        // Assert
        result.Should().HaveCount(_executableSteps.Count())
            .And.BeEquivalentTo(_executableSteps);
    }

    #endregion

    #region Setup

    private void SetupFakes()
    {
        A.CallTo(() => _dimUserCreationProcessService.CreateDimUser(A<Guid>._, _dimServiceAccountId, A<CancellationToken>._))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(Enumerable.Repeat(ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE, 1), ProcessStepStatusId.DONE, true, null));
    }

    #endregion
}
