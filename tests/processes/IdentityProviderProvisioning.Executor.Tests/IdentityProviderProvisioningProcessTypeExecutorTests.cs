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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.IdentityProviderProvisioning.Executor.Tests;

public class IdentityProviderProvisioningProcessTypeExecutorTests
{
    private readonly Guid _sharedProcessId = Guid.NewGuid();
    private readonly Guid _ownProcessId = Guid.NewGuid();
    private readonly IdpData _sharedIdpData;
    private readonly IdpData _ownIdpData;
    private readonly IIdentityProviderRepository _identityProviderRepository;
    private readonly IFixture _fixture;
    private readonly IdentityProviderProvisioningProcessTypeExecutor _executor;

    public IdentityProviderProvisioningProcessTypeExecutorTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var portalRepositories = A.Fake<IPortalRepositories>();
        var provisioningManager = A.Fake<IProvisioningManager>();
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();

        _sharedIdpData = new IdpData(Guid.NewGuid(), "sharedIdp", IdentityProviderTypeId.SHARED);

        _ownIdpData = new IdpData(Guid.NewGuid(), "ownIdp", IdentityProviderTypeId.OWN);

        A.CallTo(() => portalRepositories.GetInstance<IIdentityProviderRepository>())
            .Returns(_identityProviderRepository);

        _executor = new IdentityProviderProvisioningProcessTypeExecutor(portalRepositories, provisioningManager);
        SetupFakes();
    }

    #region GetProcessTypeId

    [Fact]
    public void GetProcessTypeId_ReturnsExpected()
    {
        // Act
        var result = _executor.GetProcessTypeId();

        // Assert
        result.Should().Be(ProcessTypeId.IDENTITYPROVIDER_PROVISIONING);
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
    [InlineData(ProcessStepTypeId.DELETE_IDP_SHARED_REALM, true)]
    [InlineData(ProcessStepTypeId.DELETE_IDP_SHARED_SERVICEACCOUNT, true)]
    [InlineData(ProcessStepTypeId.DELETE_CENTRAL_IDENTITY_PROVIDER, true)]
    [InlineData(ProcessStepTypeId.DELETE_IDENTITY_PROVIDER, true)]
    [InlineData(ProcessStepTypeId.RETRIGGER_DELETE_IDP_SHARED_REALM, false)]
    [InlineData(ProcessStepTypeId.RETRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT, false)]
    [InlineData(ProcessStepTypeId.RETRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER, false)]
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
        result.Should().HaveCount(4)
            .And.Satisfy(
                x => x == ProcessStepTypeId.DELETE_IDP_SHARED_REALM,
                x => x == ProcessStepTypeId.DELETE_IDP_SHARED_SERVICEACCOUNT,
                x => x == ProcessStepTypeId.DELETE_CENTRAL_IDENTITY_PROVIDER,
                x => x == ProcessStepTypeId.DELETE_IDENTITY_PROVIDER);
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

    [Theory]
    [InlineData(true, ProcessStepTypeId.DELETE_IDP_SHARED_REALM, ProcessStepTypeId.DELETE_IDP_SHARED_SERVICEACCOUNT, ProcessStepStatusId.DONE, null, false)]
    [InlineData(true, ProcessStepTypeId.DELETE_IDP_SHARED_SERVICEACCOUNT, ProcessStepTypeId.DELETE_CENTRAL_IDENTITY_PROVIDER, ProcessStepStatusId.DONE, null, false)]
    [InlineData(true, ProcessStepTypeId.DELETE_CENTRAL_IDENTITY_PROVIDER, ProcessStepTypeId.DELETE_IDENTITY_PROVIDER, ProcessStepStatusId.DONE, null, false)]
    [InlineData(true, ProcessStepTypeId.DELETE_IDENTITY_PROVIDER, null, ProcessStepStatusId.DONE, null, true)]
    [InlineData(false, ProcessStepTypeId.DELETE_IDP_SHARED_REALM, ProcessStepTypeId.DELETE_CENTRAL_IDENTITY_PROVIDER, ProcessStepStatusId.SKIPPED, "IdentityProvider ownIdp is not a shared idp", false)]
    [InlineData(false, ProcessStepTypeId.DELETE_IDP_SHARED_SERVICEACCOUNT, ProcessStepTypeId.DELETE_CENTRAL_IDENTITY_PROVIDER, ProcessStepStatusId.SKIPPED, "IdentityProvider ownIdp is not a shared idp", false)]
    [InlineData(false, ProcessStepTypeId.DELETE_CENTRAL_IDENTITY_PROVIDER, ProcessStepTypeId.DELETE_IDENTITY_PROVIDER, ProcessStepStatusId.DONE, null, false)]
    [InlineData(false, ProcessStepTypeId.DELETE_IDENTITY_PROVIDER, null, ProcessStepStatusId.DONE, null, true)]
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

    #region SetUp

    private void SetupFakes()
    {
        A.CallTo(() => _identityProviderRepository.GetIdentityProviderDataForProcessIdAsync(_sharedProcessId))
            .Returns(_sharedIdpData);
        A.CallTo(() => _identityProviderRepository.GetIdentityProviderDataForProcessIdAsync(_ownProcessId))
            .Returns(_ownIdpData);
    }

    #endregion
}
