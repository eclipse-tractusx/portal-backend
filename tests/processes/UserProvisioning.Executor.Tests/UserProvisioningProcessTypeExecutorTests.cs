/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.UserProvisioning.Executor.Tests;

public class UserProvisioningProcessTypeExecutorTests
{
    private readonly Guid _processId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IUserRepository _userRepository;
    private readonly IFixture _fixture;
    private readonly UserProvisioningProcessTypeExecutor _executor;

    public UserProvisioningProcessTypeExecutorTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _userRepository = A.Fake<IUserRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>())
            .Returns(_userRepository);

        _executor = new UserProvisioningProcessTypeExecutor(_portalRepositories, _provisioningManager);
        SetupFakes();
    }

    #region GetProcessTypeId

    [Fact]
    public void GetProcessTypeId_ReturnsExpected()
    {
        // Act
        var result = _executor.GetProcessTypeId();

        // Assert
        result.Should().Be(ProcessTypeId.USER_PROVISIONING);
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
    [InlineData(ProcessStepTypeId.DELETE_CENTRAL_USER, true)]
    [InlineData(ProcessStepTypeId.RETRIGGER_DELETE_CENTRAL_USER, false)]
    [InlineData(ProcessStepTypeId.DELETE_COMPANYUSER_ASSIGNED_PROCESS, true)]
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
        result.Should().HaveCount(2)
            .And.Satisfy(
                x => x == ProcessStepTypeId.DELETE_CENTRAL_USER,
                x => x == ProcessStepTypeId.DELETE_COMPANYUSER_ASSIGNED_PROCESS);
    }

    #endregion

    #region InitializeProcess

    [Fact]
    public async Task InitializeProcess_ValidUserId_ReturnsExpected()
    {

        // Act
        var result = await _executor.InitializeProcess(_processId, _fixture.CreateMany<ProcessStepTypeId>());

        // Assert
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    #endregion

    #region DeleteCentralUser

    [Fact]
    public async Task DeleteCentralUser_ReturnsExpected()
    {
        // Arrange
        var keycloakUserId = _fixture.Create<string>();
        A.CallTo(() => _provisioningManager.GetUserByUserName(A<string>._))
            .Returns(keycloakUserId);

        // Act InitializeProcess
        await _executor.InitializeProcess(_processId, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId.DELETE_CENTRAL_USER, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ScheduleStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.DELETE_COMPANYUSER_ASSIGNED_PROCESS);
        result.Modified.Should().BeFalse();

        A.CallTo(() => _provisioningManager.GetUserByUserName(_userId.ToString()))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteCentralRealmUserAsync(keycloakUserId))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region DeleteCompanyUserAssignedProcess

    [Fact]
    public async Task DeleteCompanyUserAssignedProcess_ReturnsExpected()
    {
        // Act InitializeProcess
        await _executor.InitializeProcess(_processId, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId.DELETE_COMPANYUSER_ASSIGNED_PROCESS, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ScheduleStepTypeIds.Should().BeNullOrEmpty();
        result.Modified.Should().BeTrue();

        A.CallTo(() => _userRepository.DeleteCompanyUserAssignedProcess(_userId, _processId))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region  SetUp
    private void SetupFakes()
    {
        A.CallTo(() => _userRepository.GetCompanyUserIdForProcessIdAsync(A<Guid>.That.Not.IsEqualTo(_processId)))
            .Returns(Guid.Empty);
        A.CallTo(() => _userRepository.GetCompanyUserIdForProcessIdAsync(_processId))
            .Returns(_userId);
    }

    #endregion

}
