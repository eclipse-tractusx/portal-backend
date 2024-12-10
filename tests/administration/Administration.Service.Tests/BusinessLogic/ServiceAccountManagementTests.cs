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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class ServiceAccountManagementTests
{
    private const string ClientId = "Cl1-CX-Registration";
    private static readonly Guid ValidServiceAccountId = Guid.NewGuid();
    private readonly IEnumerable<Guid> _userRoleIds = Enumerable.Repeat(Guid.NewGuid(), 1);
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IProcessStepRepository<ProcessTypeId, ProcessStepTypeId> _processStepRepository;
    private readonly ITechnicalUserRepository _serviceAccountRepository;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IFixture _fixture;
    private readonly ServiceAccountManagement _sut;

    public ServiceAccountManagementTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.ConfigureFixture();

        _provisioningManager = A.Fake<IProvisioningManager>();

        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _serviceAccountRepository = A.Fake<ITechnicalUserRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>();
        var portalRepositories = A.Fake<IPortalRepositories>();
        A.CallTo(() => portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        A.CallTo(() => portalRepositories.GetInstance<ITechnicalUserRepository>()).Returns(_serviceAccountRepository);
        A.CallTo(() => portalRepositories.GetInstance<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>()).Returns(_processStepRepository);

        _sut = new ServiceAccountManagement(_provisioningManager, portalRepositories);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public async Task DeleteOwnCompanyServiceAccountAsync_WithoutClient_CallsExpected(bool withClient, bool isDimServiceAccount)
    {
        // Arrange
        var identity = _fixture.Build<Identity>()
            .With(x => x.Id, ValidServiceAccountId)
            .With(x => x.UserStatusId, UserStatusId.ACTIVE)
            .Create();
        var processId = Guid.NewGuid();
        SetupDeleteOwnCompanyServiceAccount(isDimServiceAccount, identity);

        // Act
        await _sut.DeleteServiceAccount(ValidServiceAccountId, new DeleteServiceAccountData(_userRoleIds, withClient ? ClientId : null, isDimServiceAccount, false, processId));

        // Assert
        if (isDimServiceAccount)
        {
            A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId ProcessStepTypeId, Framework.Processes.Library.Enums.ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)>>.That.Matches(x => x.Count() == 1 && x.First().ProcessStepTypeId == ProcessStepTypeId.DELETE_DIM_TECHNICAL_USER && x.First().ProcessStepStatusId == ProcessStepStatusId.TODO && x.First().ProcessId == processId))).MustHaveHappenedOnceExactly();
            A.CallTo(() => _userRepository.AttachAndModifyIdentity(A<Guid>._, A<Action<Identity>>._, A<Action<Identity>>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _provisioningManager.DeleteCentralClientAsync(A<string>._)).MustNotHaveHappened();
            identity.UserStatusId.Should().Be(UserStatusId.PENDING_DELETION);
        }
        else if (withClient)
        {
            A.CallTo(() => _userRepository.AttachAndModifyIdentity(A<Guid>._, A<Action<Identity>>._, A<Action<Identity>>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId, ProcessStepStatusId, Guid)>>._)).MustNotHaveHappened();
            A.CallTo(() => _provisioningManager.DeleteCentralClientAsync(ClientId)).MustHaveHappenedOnceExactly();
            identity.UserStatusId.Should().Be(UserStatusId.DELETED);
        }

        var validServiceAccountUserRoleIds = _userRoleIds.Select(userRoleId => (ValidServiceAccountId, userRoleId));
        A.CallTo(() => _userRolesRepository.DeleteCompanyUserAssignedRoles(A<IEnumerable<(Guid, Guid)>>.That.IsSameSequenceAs(validServiceAccountUserRoleIds))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteServiceAccount_WithCreationProcessInProgress_ThrowsException()
    {
        // Arrange
        Task Act() => _sut.DeleteServiceAccount(ValidServiceAccountId, new DeleteServiceAccountData(_userRoleIds, ClientId, true, true, Guid.NewGuid()));

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("TECHNICAL_USER_CREATION_IN_PROGRESS");
    }

    private void SetupDeleteOwnCompanyServiceAccount(bool isDimServiceAccount, Identity? identity = null)
    {
        if (isDimServiceAccount)
        {
            A.CallTo(() => _serviceAccountRepository.GetProcessDataForTechnicalUserDeletionCallback(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
                .ReturnsLazily((Guid id, IEnumerable<ProcessStepTypeId>? processStepTypeIds) =>
                    (
                        ProcessTypeId.OFFER_SUBSCRIPTION,
                        new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(
                            new Process<ProcessTypeId, ProcessStepTypeId>(id, ProcessTypeId.OFFER_SUBSCRIPTION, Guid.NewGuid()),
                            processStepTypeIds?.Select(stepTypeId => new ProcessStep<ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), stepTypeId, ProcessStepStatusId.TODO, id, _fixture.Create<DateTimeOffset>())) ?? Enumerable.Empty<ProcessStep<ProcessTypeId, ProcessStepTypeId>>()),
                        Guid.NewGuid()));
        }

        if (identity != null)
        {
            A.CallTo(() => _userRepository.AttachAndModifyIdentity(ValidServiceAccountId, A<Action<Identity>>._, A<Action<Identity>>._))
                .Invokes((Guid _, Action<Identity>? _, Action<Identity> modify) =>
                {
                    modify.Invoke(identity);
                });
        }
    }
}
