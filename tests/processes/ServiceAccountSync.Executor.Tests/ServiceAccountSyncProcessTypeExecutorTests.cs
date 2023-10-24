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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ServiceACcountSync.Executor;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.ServiceAccountSync.Executor.Tests;

public class ServiceAccountSyncProcessTypeExecutorTests
{
    private readonly IUserRepository _userRepository;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IFixture _fixture;
    private readonly ServiceAccountSyncProcessTypeExecutor _executor;

    public ServiceAccountSyncProcessTypeExecutorTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var portalRepositories = A.Fake<IPortalRepositories>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _userRepository = A.Fake<IUserRepository>();

        A.CallTo(() => portalRepositories.GetInstance<IUserRepository>())
            .Returns(_userRepository);

        _executor = new ServiceAccountSyncProcessTypeExecutor(portalRepositories, _provisioningManager);
    }

    #region InitializeProcess

    [Fact]
    public async Task InitializeProcess_ValidProcessId_ReturnsExpected()
    {
        // Arrange
        var result = await _executor.InitializeProcess(Guid.NewGuid(), _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    #endregion

    #region ExecuteProcessStep

    [Fact]
    public async Task ExecuteProcessStep_WithUnrecoverableServiceException_Throws()
    {
        // Arrange
        const ProcessStepTypeId processStepTypeId = ProcessStepTypeId.SYNCHRONIZE_SERVICE_ACCOUNTS;
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();
        A.CallTo(() => _provisioningManager.GetServiceAccountUserId(A<string>._))
            .ThrowsAsync(new ServiceException("test"));
        A.CallTo(() => _userRepository.GetNextServiceAccountsWithoutUserEntityId())
            .Returns(Enumerable.Repeat(new ValueTuple<Guid, string>(Guid.NewGuid(), "sa1"), 1).ToAsyncEnumerable());

        // Act
        var result = await _executor.ExecuteProcessStep(processStepTypeId, processStepTypeIds, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeTrue();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        result.ProcessMessage.Should().Be("test");
    }

    [Fact]
    public async Task ExecuteProcessStep_WithConflictException_Throws()
    {
        // Arrange
        const ProcessStepTypeId processStepTypeId = ProcessStepTypeId.SYNCHRONIZE_SERVICE_ACCOUNTS;
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();
        A.CallTo(() => _provisioningManager.GetServiceAccountUserId(A<string>._))
            .ThrowsAsync(new ConflictException("test"));
        A.CallTo(() => _userRepository.GetNextServiceAccountsWithoutUserEntityId())
            .Returns(Enumerable.Repeat(new ValueTuple<Guid, string>(Guid.NewGuid(), "sa1"), 1).ToAsyncEnumerable());

        // Act
        var result = await _executor.ExecuteProcessStep(processStepTypeId, processStepTypeIds, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeTrue();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        result.ProcessMessage.Should().Be("test");
    }

    [Fact]
    public async Task ExecuteProcessStep_WithRecoverableServiceException_Throws()
    {
        // Arrange
        const ProcessStepTypeId processStepTypeId = ProcessStepTypeId.SYNCHRONIZE_SERVICE_ACCOUNTS;
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();
        A.CallTo(() => _provisioningManager.GetServiceAccountUserId(A<string>._))
            .ThrowsAsync(new ServiceException("test", true));
        A.CallTo(() => _userRepository.GetNextServiceAccountsWithoutUserEntityId())
            .Returns(Enumerable.Repeat(new ValueTuple<Guid, string>(Guid.NewGuid(), "sa1"), 1).ToAsyncEnumerable());

        // Act
        var result = await _executor.ExecuteProcessStep(processStepTypeId, processStepTypeIds, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeTrue();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.TODO);
    }

    [Fact]
    public async Task ExecuteProcessStep_WithNoIdentity_ExecutesExpected()
    {
        // Arrange
        var identity = new Identity(Guid.NewGuid(), DateTimeOffset.Now, Guid.NewGuid(), UserStatusId.ACTIVE, IdentityTypeId.COMPANY_SERVICE_ACCOUNT);
        var userEntityId = Guid.NewGuid().ToString();
        A.CallTo(() => _userRepository.GetNextServiceAccountsWithoutUserEntityId())
            .Returns(Enumerable.Empty<(Guid, string)>().ToAsyncEnumerable());

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId.SYNCHRONIZE_SERVICE_ACCOUNTS, new[] { ProcessStepTypeId.SYNCHRONIZE_SERVICE_ACCOUNTS }, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ProcessMessage.Should().Be("no serviceAccounts to synchronize found");

        A.CallTo(() => _userRepository.GetNextServiceAccountsWithoutUserEntityId())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.GetServiceAccountUserId(A<string>._))
            .MustNotHaveHappened();
        A.CallTo(() => _userRepository.AttachAndModifyIdentity(A<Guid>._, A<Action<Identity>>._, A<Action<Identity>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteProcessStep_WithSingleIdentity_ExecutesExpected()
    {
        // Arrange
        var identity = new Identity(Guid.NewGuid(), DateTimeOffset.Now, Guid.NewGuid(), UserStatusId.ACTIVE, IdentityTypeId.COMPANY_SERVICE_ACCOUNT);
        var userEntityId = Guid.NewGuid().ToString();
        A.CallTo(() => _userRepository.GetNextServiceAccountsWithoutUserEntityId())
            .Returns(new[]
            {
                (identity.Id, "sa1"),
            }.ToAsyncEnumerable());
        A.CallTo(() => _provisioningManager.GetServiceAccountUserId(A<string>._))
            .Returns(userEntityId);
        A.CallTo(() => _userRepository.AttachAndModifyIdentity(A<Guid>._, A<Action<Identity>>._, A<Action<Identity>>._))
            .Invokes((Guid id, Action<Identity>? initialize, Action<Identity> setOptionalFields) =>
            {
                initialize?.Invoke(identity);
                setOptionalFields.Invoke(identity);
            });

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId.SYNCHRONIZE_SERVICE_ACCOUNTS, new[] { ProcessStepTypeId.SYNCHRONIZE_SERVICE_ACCOUNTS }, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeTrue();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ProcessMessage.Should().Be($"synchronized serviceAccountId {identity.Id}, clientClientId sa1 with userEntityId {userEntityId}");
        identity.UserEntityId.Should().Be(userEntityId);

        A.CallTo(() => _userRepository.GetNextServiceAccountsWithoutUserEntityId())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.GetServiceAccountUserId("sa1"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.AttachAndModifyIdentity(identity.Id, A<Action<Identity>>._, A<Action<Identity>>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteProcessStep_WithMultipleIdentities_ExecutesExpected()
    {
        // Arrange
        var identity = new Identity(Guid.NewGuid(), DateTimeOffset.Now, Guid.NewGuid(), UserStatusId.ACTIVE, IdentityTypeId.COMPANY_SERVICE_ACCOUNT);
        var userEntityId = Guid.NewGuid().ToString();
        var identity2Id = Guid.NewGuid();
        A.CallTo(() => _userRepository.GetNextServiceAccountsWithoutUserEntityId())
            .Returns(new[]
            {
                (identity.Id, "sa1"),
                (identity2Id, "sa2")
            }.ToAsyncEnumerable());
        A.CallTo(() => _provisioningManager.GetServiceAccountUserId(A<string>._))
            .Returns(userEntityId);
        A.CallTo(() => _userRepository.AttachAndModifyIdentity(A<Guid>._, A<Action<Identity>>._, A<Action<Identity>>._))
            .Invokes((Guid id, Action<Identity>? initialize, Action<Identity> setOptionalFields) =>
            {
                initialize?.Invoke(identity);
                setOptionalFields.Invoke(identity);
            });

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId.SYNCHRONIZE_SERVICE_ACCOUNTS, new[] { ProcessStepTypeId.SYNCHRONIZE_SERVICE_ACCOUNTS }, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeTrue();
        result.ScheduleStepTypeIds.Should().ContainSingle(x => x == ProcessStepTypeId.SYNCHRONIZE_SERVICE_ACCOUNTS);
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ProcessMessage.Should().Be($"synchronized serviceAccountId {identity.Id}, clientClientId sa1 with userEntityId {userEntityId}");
        identity.UserEntityId.Should().Be(userEntityId);

        A.CallTo(() => _userRepository.GetNextServiceAccountsWithoutUserEntityId())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.GetServiceAccountUserId("sa1"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.GetServiceAccountUserId("sa2"))
            .MustNotHaveHappened();
        A.CallTo(() => _userRepository.AttachAndModifyIdentity(identity.Id, A<Action<Identity>>._, A<Action<Identity>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.AttachAndModifyIdentity(identity2Id, A<Action<Identity>>._, A<Action<Identity>>._))
           .MustNotHaveHappened();
    }

    #endregion

    #region GetProcessTypeId

    [Fact]
    public void GetProcessTypeId_ReturnsExpected()
    {
        // Act
        var result = _executor.GetProcessTypeId();

        // Assert
        result.Should().Be(ProcessTypeId.SERVICE_ACCOUNT_SYNC);
    }

    #endregion

    #region GetProcessTypeId

    [Fact]
    public async Task IsLockRequested_ReturnsExpected()
    {
        // Act
        var result = await _executor.IsLockRequested(_fixture.Create<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsExecutableStepTypeId

    [Theory]
    [InlineData(ProcessStepTypeId.SYNCHRONIZE_SERVICE_ACCOUNTS, true)]
    [InlineData(ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION, false)]
    [InlineData(ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION, false)]
    [InlineData(ProcessStepTypeId.ACTIVATE_SUBSCRIPTION, false)]
    [InlineData(ProcessStepTypeId.TRIGGER_PROVIDER_CALLBACK, false)]
    [InlineData(ProcessStepTypeId.SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION, false)]
    [InlineData(ProcessStepTypeId.START_AUTOSETUP, false)]
    [InlineData(ProcessStepTypeId.END_CLEARING_HOUSE, false)]
    [InlineData(ProcessStepTypeId.START_CLEARING_HOUSE, false)]
    [InlineData(ProcessStepTypeId.DECLINE_APPLICATION, false)]
    [InlineData(ProcessStepTypeId.CREATE_IDENTITY_WALLET, false)]
    [InlineData(ProcessStepTypeId.TRIGGER_ACTIVATE_SUBSCRIPTION, false)]
    public void IsExecutableProcessStep_ReturnsExpected(ProcessStepTypeId processStepTypeId, bool expectedResult)
    {
        // Act
        var result = _executor.IsExecutableStepTypeId(processStepTypeId);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region GetExecutableStepTypeIds

    [Fact]
    public void GetExecutableStepTypeIds_ReturnsExpected()
    {
        //Act
        var result = _executor.GetExecutableStepTypeIds();

        // Assert
        result.Should().HaveCount(1).And.Satisfy(x => x == ProcessStepTypeId.SYNCHRONIZE_SERVICE_ACCOUNTS);
    }

    #endregion
}
