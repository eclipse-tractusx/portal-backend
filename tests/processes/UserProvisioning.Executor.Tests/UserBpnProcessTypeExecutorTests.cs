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

using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.UserProvisioning.Executor.Bpn;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.UserProvisioning.Executor.Tests;

public class UserBpnProcessTypeExecutorTests
{
    private const string Bpnl = "BPNL0000001TEST";
    private readonly Guid _processId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly IProvisioningManager _provisioningManager;
    private readonly IUserBusinessPartnerRepository _userBusinessPartnerRepository;
    private readonly IFixture _fixture;
    private readonly UserBpnProcessTypeExecutor _executor;
    private readonly IBpdmAccessService _bpdmAccessService;

    public UserBpnProcessTypeExecutorTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var portalRepositories = A.Fake<IPortalRepositories>();
        _userBusinessPartnerRepository = A.Fake<IUserBusinessPartnerRepository>();
        A.CallTo(() => portalRepositories.GetInstance<IUserBusinessPartnerRepository>())
            .Returns(_userBusinessPartnerRepository);

        _provisioningManager = A.Fake<IProvisioningManager>();
        _bpdmAccessService = A.Fake<IBpdmAccessService>();

        _executor = new UserBpnProcessTypeExecutor(portalRepositories, _provisioningManager, _bpdmAccessService);
        SetupFakes();
    }

    #region GetProcessTypeId

    [Fact]
    public void GetProcessTypeId_ReturnsExpected()
    {
        // Act
        var result = _executor.GetProcessTypeId();

        // Assert
        result.Should().Be(ProcessTypeId.USER_BPN);
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
    [InlineData(ProcessStepTypeId.DELETE_BPN_FROM_IDENTITY, true)]
    [InlineData(ProcessStepTypeId.DELETE_BPN_FROM_CENTRAL_USER, true)]
    [InlineData(ProcessStepTypeId.CHECK_LEGAL_ENTITY_DATA, true)]
    [InlineData(ProcessStepTypeId.ADD_BPN_TO_IDENTITY, true)]
    [InlineData(ProcessStepTypeId.CLEANUP_USER_BPN, true)]
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
        result.Should().HaveCount(5)
            .And.Satisfy(
                x => x == ProcessStepTypeId.DELETE_BPN_FROM_CENTRAL_USER,
                x => x == ProcessStepTypeId.DELETE_BPN_FROM_IDENTITY,
                x => x == ProcessStepTypeId.CHECK_LEGAL_ENTITY_DATA,
                x => x == ProcessStepTypeId.ADD_BPN_TO_IDENTITY,
                x => x == ProcessStepTypeId.CLEANUP_USER_BPN);
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

    #region DeleteBpnFromCentralUser

    [Fact]
    public async Task DeleteBpnFromCentralUser_WithUserIdNull_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _provisioningManager.GetUserByUserName(A<string>._))
            .Returns<string?>(null);

        // Act InitializeProcess
        await _executor.InitializeProcess(_processId, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId.DELETE_BPN_FROM_CENTRAL_USER, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.SKIPPED);
        result.ScheduleStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.DELETE_BPN_FROM_IDENTITY);
        result.ProcessMessage.Should().Be($"User {_userId} not found by username");
        result.Modified.Should().BeFalse();

        A.CallTo(() => _provisioningManager.GetUserByUserName(_userId.ToString()))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteCentralUserBusinessPartnerNumberAsync(A<string>._, Bpnl))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task DeleteBpnFromCentralUser_WithKeycloakEntityNotFoundException_ReturnsExpected()
    {
        // Arrange
        var keycloakUserId = _fixture.Create<string>();
        A.CallTo(() => _provisioningManager.GetUserByUserName(A<string>._))
            .Returns(keycloakUserId);
        A.CallTo(() => _provisioningManager.DeleteCentralUserBusinessPartnerNumberAsync(keycloakUserId, Bpnl))
            .Throws(new KeycloakEntityNotFoundException("Test message"));

        // Act InitializeProcess
        await _executor.InitializeProcess(_processId, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId.DELETE_BPN_FROM_CENTRAL_USER, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.SKIPPED);
        result.ScheduleStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.DELETE_BPN_FROM_IDENTITY);
        result.ProcessMessage.Should().Be($"User {keycloakUserId} not found");
        result.Modified.Should().BeFalse();

        A.CallTo(() => _provisioningManager.GetUserByUserName(_userId.ToString()))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteCentralUserBusinessPartnerNumberAsync(keycloakUserId, Bpnl))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteBpnFromCentralUser_WithValid_ReturnsExpected()
    {
        // Arrange
        var keycloakUserId = _fixture.Create<string>();
        A.CallTo(() => _provisioningManager.GetUserByUserName(A<string>._))
            .Returns(keycloakUserId);

        // Act InitializeProcess
        await _executor.InitializeProcess(_processId, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId.DELETE_BPN_FROM_CENTRAL_USER, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ScheduleStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.DELETE_BPN_FROM_IDENTITY);
        result.Modified.Should().BeFalse();

        A.CallTo(() => _provisioningManager.GetUserByUserName(_userId.ToString()))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteCentralUserBusinessPartnerNumberAsync(keycloakUserId, Bpnl))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region DeleteBpnFromIdentity

    [Fact]
    public async Task DeleteBpnFromIdentity_ReturnsExpected()
    {
        // Act InitializeProcess
        await _executor.InitializeProcess(_processId, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId.DELETE_BPN_FROM_IDENTITY, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ScheduleStepTypeIds.Should().BeNullOrEmpty();
        result.Modified.Should().BeTrue();

        A.CallTo(() => _userBusinessPartnerRepository.DeleteCompanyUserAssignedBusinessPartner(_userId, Bpnl))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region CheckLegalEntityData

    [Fact]
    public async Task CheckLegalEntityData_WithNotMatchingBpn_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _bpdmAccessService.FetchLegalEntityByBpn(Bpnl, A<CancellationToken>._))
            .Returns(_fixture.Build<BpdmLegalEntityDto>().With(x => x.Bpn, "NOTMATCHINGBPN").Create());

        // Act InitializeProcess
        await _executor.InitializeProcess(_processId, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId.CHECK_LEGAL_ENTITY_DATA, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        result.ScheduleStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.CLEANUP_USER_BPN);
        result.ProcessMessage.Should().Be($"Bpdm {Bpnl} did return incorrect bpn legal-entity-data");
        result.Modified.Should().BeFalse();
    }

    [Fact]
    public async Task CheckLegalEntityData_WithBpdmThrowingException_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _bpdmAccessService.FetchLegalEntityByBpn(Bpnl, A<CancellationToken>._))
            .Throws(new ArgumentException("Test"));

        // Act InitializeProcess
        await _executor.InitializeProcess(_processId, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId.CHECK_LEGAL_ENTITY_DATA, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.TODO);
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessMessage.Should().Be("Test");
        result.Modified.Should().BeFalse();
    }

    [Fact]
    public async Task CheckLegalEntityData_WithValid_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _bpdmAccessService.FetchLegalEntityByBpn(Bpnl, A<CancellationToken>._))
            .Returns(_fixture.Build<BpdmLegalEntityDto>().With(x => x.Bpn, Bpnl).Create());

        // Act InitializeProcess
        await _executor.InitializeProcess(_processId, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId.CHECK_LEGAL_ENTITY_DATA, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ScheduleStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.ADD_BPN_TO_IDENTITY);
        result.Modified.Should().BeFalse();
    }

    #endregion

    #region AddBpnFromCentralUser

    [Fact]
    public async Task AddBpnFromCentralUser_WithValid_ReturnsExpected()
    {
        // Arrange
        var keycloakUserId = _fixture.Create<string>();
        A.CallTo(() => _provisioningManager.GetUserByUserName(A<string>._))
            .Returns(keycloakUserId);

        // Act InitializeProcess
        await _executor.InitializeProcess(_processId, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId.ADD_BPN_TO_IDENTITY, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ScheduleStepTypeIds.Should().BeNull();
        result.Modified.Should().BeFalse();

        A.CallTo(() => _provisioningManager.GetUserByUserName(_userId.ToString()))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(keycloakUserId, A<IEnumerable<string>>.That.Contains(Bpnl)))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Setup

    private void SetupFakes()
    {
        A.CallTo(() => _userBusinessPartnerRepository.GetForProcessIdAsync(A<Guid>.That.Not.IsEqualTo(_processId)))
            .Returns(new ValueTuple<Guid, string?>());
        A.CallTo(() => _userBusinessPartnerRepository.GetForProcessIdAsync(_processId))
            .Returns((_userId, Bpnl));
    }

    #endregion
}
