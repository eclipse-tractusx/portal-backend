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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Invitation.Executor.Tests;

public class InvitationProcessTypeExecutorTests
{
    private readonly Guid _invitationId = Guid.NewGuid();
    private readonly ICompanyInvitationRepository _companyInvitationRepository;
    private readonly IInvitationProcessService _invitationProcessService;
    private readonly InvitationProcessTypeExecutor _executor;
    private readonly IFixture _fixture;
    private readonly IEnumerable<ProcessStepTypeId> _executableSteps;

    public InvitationProcessTypeExecutorTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var portalRepositories = A.Fake<IPortalRepositories>();
        _companyInvitationRepository = A.Fake<ICompanyInvitationRepository>();

        _invitationProcessService = A.Fake<IInvitationProcessService>();

        A.CallTo(() => portalRepositories.GetInstance<ICompanyInvitationRepository>())
            .Returns(_companyInvitationRepository);

        _executor = new InvitationProcessTypeExecutor(
            portalRepositories,
            _invitationProcessService);

        _executableSteps = new[] {
            ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP,
            ProcessStepTypeId.INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT,
            ProcessStepTypeId.INVITATION_UPDATE_CENTRAL_IDP_URLS,
            ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER,
            ProcessStepTypeId.INVITATION_CREATE_SHARED_REALM_IDP_CLIENT,
            ProcessStepTypeId.INVITATION_ENABLE_CENTRAL_IDP,
            ProcessStepTypeId.INVITATION_CREATE_DATABASE_IDP,
            ProcessStepTypeId.INVITATION_CREATE_USER,
            ProcessStepTypeId.INVITATION_SEND_MAIL
        };
    }

    #region InitializeProcess

    [Fact]
    public async Task InitializeProcess_WithExisting_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();

        A.CallTo(() => _companyInvitationRepository.GetCompanyInvitationForProcessId(processId))
            .Returns(_invitationId);

        // Act
        var result = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

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

        A.CallTo(() => _companyInvitationRepository.GetCompanyInvitationForProcessId(processId))
            .Returns(Guid.Empty);

        // Act
        async Task Act() => await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be($"process {processId} does not exist or is not associated with an company invitation");
    }

    #endregion

    #region ExecuteProcessStep

    [Fact]
    public async Task ExecuteProcessStep_InitializeNotCalled_Throws()
    {
        // Arrange
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();

        async Task Act() => await _executor.ExecuteProcessStep(processStepTypeId, processStepTypeIds, CancellationToken.None).ConfigureAwait(false);

        // Act
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);

        // Assert
        result.Message.Should().Be("companyInvitationId should never be empty here");
    }

    [Theory]
    [InlineData(ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP, ProcessStepTypeId.INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT)]
    [InlineData(ProcessStepTypeId.INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT, ProcessStepTypeId.INVITATION_UPDATE_CENTRAL_IDP_URLS)]
    [InlineData(ProcessStepTypeId.INVITATION_UPDATE_CENTRAL_IDP_URLS, ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER)]
    [InlineData(ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER, ProcessStepTypeId.INVITATION_CREATE_SHARED_REALM_IDP_CLIENT)]
    [InlineData(ProcessStepTypeId.INVITATION_CREATE_SHARED_REALM_IDP_CLIENT, ProcessStepTypeId.INVITATION_ENABLE_CENTRAL_IDP)]
    [InlineData(ProcessStepTypeId.INVITATION_ENABLE_CENTRAL_IDP, ProcessStepTypeId.INVITATION_CREATE_DATABASE_IDP)]
    [InlineData(ProcessStepTypeId.INVITATION_CREATE_DATABASE_IDP, ProcessStepTypeId.INVITATION_CREATE_USER)]
    [InlineData(ProcessStepTypeId.INVITATION_CREATE_USER, ProcessStepTypeId.INVITATION_SEND_MAIL)]
    [InlineData(ProcessStepTypeId.INVITATION_SEND_MAIL, null)]
    public async Task ExecuteProcessStep_ReturnsExpected(ProcessStepTypeId processStepTypeId, ProcessStepTypeId? expectedResult)
    {
        // Arrange initialize
        var processId = Guid.NewGuid();

        A.CallTo(() => _companyInvitationRepository.GetCompanyInvitationForProcessId(processId))
            .Returns(_invitationId);

        // Act initialize
        var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert initialize
        initializationResult.Should().NotBeNull();
        initializationResult.Modified.Should().BeFalse();
        initializationResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange execute
        SetupFakes();

        // Act execute
        var executionResult = await _executor.ExecuteProcessStep(processStepTypeId, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None).ConfigureAwait(false);

        // Assert execute
        executionResult.Modified.Should().BeTrue();
        executionResult.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        if (expectedResult == null)
        {
            executionResult.ScheduleStepTypeIds.Should().BeNull();
        }
        else
        {
            executionResult.ScheduleStepTypeIds.Should().ContainSingle().And.Satisfy(x => x == expectedResult);
        }

        executionResult.SkipStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteProcessStep_ThrowingTestException_ReturnsExpected()
    {
        // Arrange initialize
        var processId = Guid.NewGuid();
        var invitationId = Guid.NewGuid();

        A.CallTo(() => _companyInvitationRepository.GetCompanyInvitationForProcessId(processId))
            .Returns(invitationId);

        // Act initialize
        var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert initialize
        initializationResult.Should().NotBeNull();
        initializationResult.Modified.Should().BeFalse();
        initializationResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange execute
        var error = _fixture.Create<TestException>();
        A.CallTo(() => _invitationProcessService.CreateCentralIdp(invitationId))
            .Throws(error);

        // Act execute
        var executionResult = await _executor.ExecuteProcessStep(ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None).ConfigureAwait(false);

        // Assert execute
        A.CallTo(() => _invitationProcessService.CreateCentralIdp(invitationId))
            .MustHaveHappenedOnceExactly();

        executionResult.Modified.Should().BeTrue();
        executionResult.ProcessStepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        executionResult.ScheduleStepTypeIds.Should().ContainInOrder(ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_CENTRAL_IDP);
        executionResult.SkipStepTypeIds.Should().BeNull();
        executionResult.ProcessMessage.Should().Be(error.Message);
    }

    [Fact]
    public async Task ExecuteProcessStep_ThrowingRecoverableServiceException_ReturnsExpected()
    {
        // Arrange initialize
        var processId = Guid.NewGuid();
        var invitationId = Guid.NewGuid();

        A.CallTo(() => _companyInvitationRepository.GetCompanyInvitationForProcessId(processId))
            .Returns(invitationId);

        // Act initialize
        var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert initialize
        initializationResult.Should().NotBeNull();
        initializationResult.Modified.Should().BeFalse();
        initializationResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange execute
        var error = new ServiceException(_fixture.Create<string>(), true);
        A.CallTo(() => _invitationProcessService.CreateCentralIdp(invitationId))
            .Throws(error);

        // Act execute
        var executionResult = await _executor.ExecuteProcessStep(ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None).ConfigureAwait(false);

        // Assert execute
        A.CallTo(() => _invitationProcessService.CreateCentralIdp(invitationId))
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
        var invitationId = Guid.NewGuid();

        A.CallTo(() => _companyInvitationRepository.GetCompanyInvitationForProcessId(processId))
            .Returns(invitationId);

        // Act initialize
        var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert initialize
        initializationResult.Should().NotBeNull();
        initializationResult.Modified.Should().BeFalse();
        initializationResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange execute
        var error = new SystemException(_fixture.Create<string>());
        A.CallTo(() => _invitationProcessService.CreateCentralIdp(invitationId))
            .Throws(error);

        // Act execute
        async Task Act() => await _executor.ExecuteProcessStep(ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None).ConfigureAwait(false);
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
        result.Should().Be(ProcessTypeId.INVITATION);
    }

    #endregion

    #region IsExecutableStepTypeId

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsExecutableProcessStep_ReturnsExpected(bool checklistHandlerReturnValue)
    {
        // Arrange
        var processStepTypeId = checklistHandlerReturnValue ? ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP : ProcessStepTypeId.START_AUTOSETUP;

        // Act
        var result = _executor.IsExecutableStepTypeId(processStepTypeId);

        // Assert
        result.Should().Be(checklistHandlerReturnValue);
    }

    #endregion

    #region IsLockRequested

    [Theory]
    [InlineData(ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP, false)]
    [InlineData(ProcessStepTypeId.INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT, false)]
    [InlineData(ProcessStepTypeId.INVITATION_UPDATE_CENTRAL_IDP_URLS, false)]
    [InlineData(ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER, false)]
    [InlineData(ProcessStepTypeId.INVITATION_CREATE_SHARED_REALM_IDP_CLIENT, false)]
    [InlineData(ProcessStepTypeId.INVITATION_ENABLE_CENTRAL_IDP, false)]
    [InlineData(ProcessStepTypeId.INVITATION_CREATE_DATABASE_IDP, false)]
    [InlineData(ProcessStepTypeId.INVITATION_CREATE_USER, false)]
    [InlineData(ProcessStepTypeId.INVITATION_SEND_MAIL, false)]
    public async Task IsLockRequested_ReturnsExpected(ProcessStepTypeId stepTypeId, bool isLocked)
    {
        // Act
        var result = await _executor.IsLockRequested(stepTypeId).ConfigureAwait(false);

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
        A.CallTo(() => _invitationProcessService.CreateCentralIdp(_invitationId))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(Enumerable.Repeat(ProcessStepTypeId.INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT, 1), ProcessStepStatusId.DONE, true, null));
        A.CallTo(() => _invitationProcessService.CreateSharedIdpServiceAccount(_invitationId))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(Enumerable.Repeat(ProcessStepTypeId.INVITATION_UPDATE_CENTRAL_IDP_URLS, 1), ProcessStepStatusId.DONE, true, null));
        A.CallTo(() => _invitationProcessService.UpdateCentralIdpUrl(_invitationId))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(Enumerable.Repeat(ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER, 1), ProcessStepStatusId.DONE, true, null));
        A.CallTo(() => _invitationProcessService.CreateCentralIdpOrgMapper(_invitationId))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(Enumerable.Repeat(ProcessStepTypeId.INVITATION_CREATE_SHARED_REALM_IDP_CLIENT, 1), ProcessStepStatusId.DONE, true, null));
        A.CallTo(() => _invitationProcessService.CreateSharedIdpRealmIdpClient(_invitationId))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(Enumerable.Repeat(ProcessStepTypeId.INVITATION_ENABLE_CENTRAL_IDP, 1), ProcessStepStatusId.DONE, true, null));
        A.CallTo(() => _invitationProcessService.EnableCentralIdp(_invitationId))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(Enumerable.Repeat(ProcessStepTypeId.INVITATION_CREATE_DATABASE_IDP, 1), ProcessStepStatusId.DONE, true, null));
        A.CallTo(() => _invitationProcessService.CreateIdpDatabase(_invitationId))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(Enumerable.Repeat(ProcessStepTypeId.INVITATION_CREATE_USER, 1), ProcessStepStatusId.DONE, true, null));
        A.CallTo(() => _invitationProcessService.CreateUser(_invitationId, A<CancellationToken>._))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(Enumerable.Repeat(ProcessStepTypeId.INVITATION_SEND_MAIL, 1), ProcessStepStatusId.DONE, true, null));
        A.CallTo(() => _invitationProcessService.SendMail(_invitationId))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(null, ProcessStepStatusId.DONE, true, null));
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
