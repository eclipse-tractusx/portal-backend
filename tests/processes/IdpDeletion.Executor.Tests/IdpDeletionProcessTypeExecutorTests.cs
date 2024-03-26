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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.IdpDeletion.Executor.Tests;

public class IdpDeletionProcessTypeExecutorTests
{
    private readonly Guid _processId = Guid.NewGuid();
    private readonly Guid _companyId = Guid.NewGuid();
    private readonly Guid _companyUserId = Guid.NewGuid();
    private readonly IdpData _idpData;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IIdentityProviderRepository _identityProviderRepository;
    private readonly IFixture _fixture;
    private readonly IdpDeletionProcessTypeExecutor _executor;

    public IdpDeletionProcessTypeExecutorTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();
        _idpData = _fixture.Create<IdpData>();

        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>())
            .Returns(_identityProviderRepository);

        _executor = new IdpDeletionProcessTypeExecutor(_portalRepositories, _provisioningManager);
        SetupFakes();
    }

    #region GetProcessTypeId

    [Fact]
    public void GetProcessTypeId_ReturnsExpected()
    {
        // Act
        var result = _executor.GetProcessTypeId();

        // Assert
        result.Should().Be(ProcessTypeId.IDP_DELETION);
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
    [InlineData(ProcessStepTypeId.TRIGGER_DELETE_IDP_SHARED_REALM)]
    [InlineData(ProcessStepTypeId.TRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT)]
    [InlineData(ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_LINKED_USERS)]
    [InlineData(ProcessStepTypeId.TRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER)]
    [InlineData(ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_PROVIDER)]
    [InlineData(ProcessStepTypeId.RETRIGGER_DELETE_IDP_SHARED_REALM)]
    [InlineData(ProcessStepTypeId.RETRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT)]
    [InlineData(ProcessStepTypeId.RETRIGGER_DELETE_IDENTITY_LINKED_USERS)]
    [InlineData(ProcessStepTypeId.RETRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER)]
    [InlineData(ProcessStepTypeId.RETRIGGER_DELETE_IDENTITY_PROVIDER)]
    public void IsExecutableProcessStep_ReturnsExpected(ProcessStepTypeId processStepTypeId)
    {
        // Act
        var result = _executor.IsExecutableStepTypeId(processStepTypeId);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region GetExecutableStepTypeIds

    [Fact]
    public void GetExecutableStepTypeIds_ReturnsExpected()
    {
        //Act
        var result = _executor.GetExecutableStepTypeIds();

        // Assert
        result.Should().HaveCount(10)
            .And.Satisfy(
                x => x == ProcessStepTypeId.TRIGGER_DELETE_IDP_SHARED_REALM,
                x => x == ProcessStepTypeId.TRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT,
                x => x == ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_LINKED_USERS,
                x => x == ProcessStepTypeId.TRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER,
                x => x == ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_PROVIDER,
                x => x == ProcessStepTypeId.RETRIGGER_DELETE_IDP_SHARED_REALM,
                x => x == ProcessStepTypeId.RETRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT,
                x => x == ProcessStepTypeId.RETRIGGER_DELETE_IDENTITY_LINKED_USERS,
                x => x == ProcessStepTypeId.RETRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER,
                x => x == ProcessStepTypeId.RETRIGGER_DELETE_IDENTITY_PROVIDER);
    }

    #endregion

    #region InitializeProcess

    [Fact]
    public async Task InitializeProcess_ValidProcessId_ReturnsExpected()
    {
        // Arrange
        var result = await _executor.InitializeProcess(_processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    #endregion

    #region ExecuteProcessStep

    [Theory]
    [InlineData(ProcessStepTypeId.TRIGGER_DELETE_IDP_SHARED_REALM, ProcessStepTypeId.TRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT)]
    [InlineData(ProcessStepTypeId.TRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT, ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_LINKED_USERS)]
    [InlineData(ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_LINKED_USERS, ProcessStepTypeId.TRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER)]
    [InlineData(ProcessStepTypeId.TRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER, ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_PROVIDER)]
    [InlineData(ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_PROVIDER, null)]
    public async Task ExecuteProcessStep_WithValidTriggerData_CallsExpected(ProcessStepTypeId processStepTypeId, ProcessStepTypeId? nextprocessStepTypeId)
    {
        // Act InitializeProcess
        var initializeResult = await _executor.InitializeProcess(_processId, Enumerable.Empty<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert InitializeProcess
        initializeResult.Modified.Should().BeFalse();
        initializeResult.ScheduleStepTypeIds.Should().BeNull();

        // Act
        var result = await _executor.ExecuteProcessStep(processStepTypeId, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeFalse();
        if (nextprocessStepTypeId == null)
        {
            result.ScheduleStepTypeIds.Should().BeNull();
        }
        else
        {
            result.ScheduleStepTypeIds.Should().HaveCount(1).And.Satisfy(x => x == nextprocessStepTypeId);
        }
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ProcessMessage.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
    }

    #endregion

    #region  SetUp
    private void SetupFakes()
    {
        A.CallTo(() => _identityProviderRepository.GetIdentityProviderDataForProcessIdAsync(_processId))
            .Returns((_idpData, _companyId, _companyUserId));
        A.CallTo(() => _provisioningManager.TriggerDeleteSharedRealmAsync(_idpData.IamAlias))
            .Returns((new[] { ProcessStepTypeId.TRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT }, ProcessStepStatusId.DONE, false, null));
        A.CallTo(() => _provisioningManager.TriggerDeleteIdpSharedServiceAccount(_idpData.IamAlias))
            .Returns((new[] { ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_LINKED_USERS }, ProcessStepStatusId.DONE, false, null));
    }

    #endregion

}
