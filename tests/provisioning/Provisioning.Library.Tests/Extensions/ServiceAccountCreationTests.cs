/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using ServiceAccountData = Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models.ServiceAccountData;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests.Extensions;

public class ServiceAccountCreationTests
{
    private const string Bpn = "CAXSDUMMYCATENAZZ";
    private readonly string _iamUserId = Guid.NewGuid().ToString();
    private readonly Guid _companyId = Guid.NewGuid();
    private readonly Guid _identityId = Guid.NewGuid();
    private readonly Guid _secondId = Guid.NewGuid();
    private readonly string _validClientId;
    private readonly Guid _validUserRoleId = Guid.NewGuid();
    private readonly string _dimClient;
    private readonly string _dimRoleText;
    private readonly Guid _dimUserRoleId = Guid.NewGuid();
    private readonly Guid _invalidUserRoleId = Guid.NewGuid();
    private readonly Guid _processId = Guid.NewGuid();
    private readonly Guid _processStepId = Guid.NewGuid();

    private readonly ITechnicalUserRepository _technicalUserRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IProcessStepRepository<ProcessTypeId, ProcessStepTypeId> _processStepRepository;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningDBAccess _provisioningDbAccess;
    private readonly IServiceAccountCreation _sut;

    public ServiceAccountCreationTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _validClientId = fixture.Create<string>();
        _dimClient = fixture.Create<string>();
        _dimRoleText = fixture.Create<string>();

        _technicalUserRepository = A.Fake<ITechnicalUserRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>();

        _provisioningManager = A.Fake<IProvisioningManager>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _provisioningDbAccess = A.Fake<IProvisioningDBAccess>();

        var settings = new ServiceAccountCreationSettings
        {
            ServiceAccountClientPrefix = "sa",
            DimUserRoles = [new UserRoleConfig(_dimClient, [_dimRoleText])]
        };

        A.CallTo(() => _portalRepositories.GetInstance<ITechnicalUserRepository>()).Returns(_technicalUserRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>()).Returns(_processStepRepository);

        _sut = new ServiceAccountCreation(_provisioningManager, _portalRepositories, _provisioningDbAccess, Options.Create(settings));
    }

    private void ServiceAccountCreationAction(TechnicalUser _) { }

    [Fact]
    public async Task CreateServiceAccountAsync_WithInvalidRole_ThrowsNotFoundException()
    {
        // Arrange
        var creationData = new ServiceAccountCreationInfo("testName", "abc", IamClientAuthMethod.SECRET, [_invalidUserRoleId]);
        Setup();

        // Act
        async Task Act() => await _sut.CreateServiceAccountAsync(creationData, _companyId, Enumerable.Empty<string>(), TechnicalUserTypeId.OWN, false, true, new ServiceAccountCreationProcessData(ProcessTypeId.DIM_TECHNICAL_USER, null), ServiceAccountCreationAction);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(ProvisioningServiceErrors.USER_NOT_VALID_USERROLEID.ToString());

        A.CallTo(() => _userRepository.CreateIdentity(A<Guid>._, A<UserStatusId>._, A<IdentityTypeId>._, A<Action<Identity>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _technicalUserRepository.CreateTechnicalUser(A<Guid>._, A<string>._, A<string>._, A<string>._, A<TechnicalUserTypeId>._, A<TechnicalUserKindId>._, A<Action<TechnicalUser>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _userRolesRepository.CreateIdentityAssignedRoleRange(A<IEnumerable<(Guid, Guid)>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.SetupCentralServiceAccountClientAsync(A<string>._, A<ClientConfigRolesData>._, A<bool>._))
            .MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(A<string>._, A<IEnumerable<string>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.AddProtocolMapperAsync(A<string>._))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(false, "testName")]
    [InlineData(true, "sa1-testName")]
    public async Task CreateServiceAccountAsync_WithValidData_ReturnsExpected(bool enhance, string serviceAccountName)
    {
        // Arrange
        var serviceAccounts = new List<TechnicalUser>();
        var identities = new List<Identity>();
        var creationData = new ServiceAccountCreationInfo("testName", "abc", IamClientAuthMethod.SECRET, [_validUserRoleId]);
        var bpns = new[]
        {
            Bpn
        };
        Setup(serviceAccounts, identities);

        // Act
        var result = await _sut.CreateServiceAccountAsync(creationData, _companyId, bpns, TechnicalUserTypeId.OWN, enhance, true, new ServiceAccountCreationProcessData(ProcessTypeId.DIM_TECHNICAL_USER, null), ServiceAccountCreationAction);

        // Assert

        result.ServiceAccounts.Should().ContainSingle()
            .Which.Should().Match<CreatedServiceAccountData>(x =>
                x.ClientId == "sa1" &&
                x.Description == "abc" &&
                x.UserRoleData.SequenceEqual(new[] { new UserRoleData(_validUserRoleId, _validClientId, "UserRole") }) &&
                x.Name == serviceAccountName &&
                x.ServiceAccountData != null &&
                x.ServiceAccountData.InternalClientId == "internal-sa1" &&
                x.ServiceAccountData.IamUserId == _iamUserId &&
                x.ServiceAccountData.AuthData.IamClientAuthMethod == IamClientAuthMethod.SECRET
            );

        A.CallTo(() => _userRepository.CreateIdentity(_companyId, UserStatusId.ACTIVE, IdentityTypeId.COMPANY_SERVICE_ACCOUNT, null))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _technicalUserRepository.CreateTechnicalUser(_identityId, "testName", "abc", "sa1", TechnicalUserTypeId.OWN, TechnicalUserKindId.INTERNAL, ServiceAccountCreationAction))
            .MustHaveHappenedOnceExactly();
        var expectedRolesIds = new[] { (_identityId, _validUserRoleId) };
        A.CallTo(() => _userRolesRepository.CreateIdentityAssignedRoleRange(A<IEnumerable<(Guid, Guid)>>.That.IsSameSequenceAs(expectedRolesIds)))
            .MustHaveHappenedOnceExactly();
        IEnumerable<string>? userRoles;
        A.CallTo(() => _provisioningManager.SetupCentralServiceAccountClientAsync(
            "sa1",
            A<ClientConfigRolesData>.That.Matches(x =>
                x.IamClientAuthMethod == IamClientAuthMethod.SECRET &&
                x.Name == serviceAccountName &&
                x.Description == "abc" &&
                x.ClientRoles.Count() == 1 &&
                x.ClientRoles.TryGetValue(_validClientId, out userRoles) &&
                userRoles.SequenceEqual(new[] { "UserRole" })),
            true))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(_iamUserId, bpns))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.AddProtocolMapperAsync("internal-sa1"))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _userRepository.CreateIdentity(A<Guid>._, UserStatusId.PENDING, A<IdentityTypeId>._, A<Action<Identity>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _technicalUserRepository.CreateTechnicalUser(A<Guid>._, A<string>._, A<string>._, A<string>._, A<TechnicalUserTypeId>._, TechnicalUserKindId.EXTERNAL, A<Action<TechnicalUser>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _userRolesRepository.CreateIdentityAssignedRoleRange(A<IEnumerable<(Guid, Guid)>>.That.Matches(x => x.Any(y => y.Item2 != _validUserRoleId))))
            .MustNotHaveHappened();
        A.CallTo(() => _processStepRepository.CreateProcess(A<ProcessTypeId>._))
            .MustNotHaveHappened();
        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._, A<ProcessStepStatusId>._, A<Guid>._))
            .MustNotHaveHappened();
        A.CallTo(() => _technicalUserRepository.CreateExternalTechnicalUserCreationData(A<Guid>._, A<Guid>._))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustNotHaveHappened();
        serviceAccounts.Should().ContainSingle().Which.Should().Match<TechnicalUser>(
            x => x.Name == "testName" &&
                 x.ClientClientId == "sa1" &&
                 x.TechnicalUserKindId == TechnicalUserKindId.INTERNAL);
        identities.Should().ContainSingle().Which.Should().Match<Identity>(
            x => x.CompanyId == _companyId &&
                 x.UserStatusId == UserStatusId.ACTIVE &&
                 x.IdentityTypeId == IdentityTypeId.COMPANY_SERVICE_ACCOUNT);
    }

    [Fact]
    public async Task CreateServiceAccountAsync_WithValidDimData_ReturnsExpected()
    {
        // Arrange
        var serviceAccounts = new List<TechnicalUser>();
        var identities = new List<Identity>();
        var creationData = new ServiceAccountCreationInfo("testName", "abc", IamClientAuthMethod.SECRET, [_dimUserRoleId]);
        var bpns = new[]
        {
            Bpn
        };
        Setup(serviceAccounts, identities);

        // Act
        var result = await _sut.CreateServiceAccountAsync(creationData, _companyId, bpns, TechnicalUserTypeId.OWN, false, true, new ServiceAccountCreationProcessData(ProcessTypeId.DIM_TECHNICAL_USER, null), ServiceAccountCreationAction);

        // Assert
        result.ServiceAccounts.Should().ContainSingle()
            .Which.Should().Match<CreatedServiceAccountData>(x =>
                x.ClientId == null &&
                x.Description == "abc" &&
                x.UserRoleData.SequenceEqual(new[] { new UserRoleData(_dimUserRoleId, _dimClient, _dimRoleText) }) &&
                x.Name == "dim-testName" &&
                x.ServiceAccountData == null &&
                x.Status == UserStatusId.PENDING
            );

        A.CallTo(() => _userRepository.CreateIdentity(A<Guid>._, UserStatusId.ACTIVE, A<IdentityTypeId>._, A<Action<Identity>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _technicalUserRepository.CreateTechnicalUser(A<Guid>._, A<string>._, A<string>._, A<string>._, A<TechnicalUserTypeId>._, TechnicalUserKindId.INTERNAL, A<Action<TechnicalUser>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _userRolesRepository.CreateIdentityAssignedRoleRange(A<IEnumerable<(Guid, Guid)>>.That.Matches(x => x.Any(y => y.Item2 != _dimUserRoleId))))
            .MustNotHaveHappened();

        A.CallTo(() => _provisioningManager.SetupCentralServiceAccountClientAsync(A<string>._, A<ClientConfigRolesData>._, A<bool>._))
            .MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(A<string>._, A<IEnumerable<string>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.AddProtocolMapperAsync(A<string>._))
            .MustNotHaveHappened();

        A.CallTo(() => _userRepository.CreateIdentity(_companyId, UserStatusId.PENDING, IdentityTypeId.COMPANY_SERVICE_ACCOUNT, null))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _technicalUserRepository.CreateTechnicalUser(_identityId, "dim-testName", "abc", null, TechnicalUserTypeId.OWN, TechnicalUserKindId.EXTERNAL, ServiceAccountCreationAction))
            .MustHaveHappenedOnceExactly();
        var expectedRolesIds = new[] { (_identityId, _dimUserRoleId) };
        A.CallTo(() => _userRolesRepository.CreateIdentityAssignedRoleRange(A<IEnumerable<(Guid, Guid)>>.That.IsSameSequenceAs(expectedRolesIds)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.DIM_TECHNICAL_USER))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER, ProcessStepStatusId.TODO, A<Guid>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _technicalUserRepository.CreateExternalTechnicalUserCreationData(_identityId, _processId))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustNotHaveHappened();

        serviceAccounts.Should().ContainSingle().Which.Should().Match<TechnicalUser>(
            x => x.Name == "dim-testName" &&
                 x.ClientClientId == null &&
                 x.TechnicalUserKindId == TechnicalUserKindId.EXTERNAL);
        identities.Should().ContainSingle().Which.Should().Match<Identity>(
            x => x.CompanyId == _companyId &&
                 x.UserStatusId == UserStatusId.PENDING &&
                 x.IdentityTypeId == IdentityTypeId.COMPANY_SERVICE_ACCOUNT);
    }

    [Fact]
    public async Task CreateServiceAccountAsync_WithValidDataPlus_ReturnsExpected()
    {
        // Arrange
        var serviceAccounts = new List<TechnicalUser>();
        var identities = new List<Identity>();
        var creationData = new ServiceAccountCreationInfo("testName", "abc", IamClientAuthMethod.SECRET, [_validUserRoleId, _dimUserRoleId]);
        var bpns = new[]
        {
            Bpn
        };
        Setup(serviceAccounts, identities);

        // Act
        var result = await _sut.CreateServiceAccountAsync(creationData, _companyId, bpns, TechnicalUserTypeId.OWN, false, true, new ServiceAccountCreationProcessData(ProcessTypeId.DIM_TECHNICAL_USER, null), ServiceAccountCreationAction);

        // Assert
        result.ServiceAccounts.Should().HaveCount(2)
            .And.Satisfy(
                x => x.ClientId == "sa1" &&
                     x.Description == "abc" &&
                     x.UserRoleData.SequenceEqual(new[] { new UserRoleData(_validUserRoleId, _validClientId, "UserRole") }) &&
                     x.Name == "testName" &&
                     x.ServiceAccountData != null &&
                     x.ServiceAccountData.InternalClientId == "internal-sa1" &&
                     x.ServiceAccountData.IamUserId == _iamUserId &&
                     x.ServiceAccountData.AuthData.IamClientAuthMethod == IamClientAuthMethod.SECRET,
                x => x.ClientId == null &&
                     x.Description == "abc" &&
                     x.UserRoleData.SequenceEqual(new[] { new UserRoleData(_dimUserRoleId, _dimClient, _dimRoleText) }) &&
                     x.Name == "dim-testName" &&
                     x.ServiceAccountData == null);

        A.CallTo(() => _userRepository.CreateIdentity(_companyId, UserStatusId.ACTIVE, IdentityTypeId.COMPANY_SERVICE_ACCOUNT, null))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _technicalUserRepository.CreateTechnicalUser(_identityId, "testName", "abc", "sa1", TechnicalUserTypeId.OWN, TechnicalUserKindId.INTERNAL, ServiceAccountCreationAction))
            .MustHaveHappenedOnceExactly();
        var expectedRolesIds = new[] { (_identityId, _validUserRoleId) };
        A.CallTo(() => _userRolesRepository.CreateIdentityAssignedRoleRange(A<IEnumerable<(Guid, Guid)>>.That.IsSameSequenceAs(expectedRolesIds)))
            .MustHaveHappenedOnceExactly();
        IEnumerable<string>? userRoles;
        A.CallTo(() => _provisioningManager.SetupCentralServiceAccountClientAsync(
            "sa1",
            A<ClientConfigRolesData>.That.Matches(x =>
                x.IamClientAuthMethod == IamClientAuthMethod.SECRET &&
                x.Name == "testName" &&
                x.Description == "abc" &&
                x.ClientRoles.Count() == 1 &&
                x.ClientRoles.TryGetValue(_validClientId, out userRoles) &&
                userRoles.SequenceEqual(new[] { "UserRole" })),
            true))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(_iamUserId, bpns))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.AddProtocolMapperAsync("internal-sa1"))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _userRepository.CreateIdentity(_companyId, UserStatusId.ACTIVE, IdentityTypeId.COMPANY_SERVICE_ACCOUNT, null))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _technicalUserRepository.CreateTechnicalUser(_secondId, "dim-testName", "abc", null, TechnicalUserTypeId.OWN, TechnicalUserKindId.EXTERNAL, ServiceAccountCreationAction))
            .MustHaveHappenedOnceExactly();
        var expectedDimRolesIds = new[] { (_secondId, _dimUserRoleId) };
        A.CallTo(() => _userRolesRepository.CreateIdentityAssignedRoleRange(A<IEnumerable<(Guid, Guid)>>.That.IsSameSequenceAs(expectedDimRolesIds)))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustNotHaveHappened();

        serviceAccounts.Should().HaveCount(2)
            .And.Satisfy(
                x => x.Name == "testName" && x.ClientClientId == "sa1" && x.TechnicalUserKindId == TechnicalUserKindId.INTERNAL,
                x => x.Name == "dim-testName" && x.ClientClientId == null && x.TechnicalUserKindId == TechnicalUserKindId.EXTERNAL
            );
        identities.Should().HaveCount(2)
            .And.AllSatisfy(x => x.Should().Match<Identity>(x => x.CompanyId == _companyId && x.IdentityTypeId == IdentityTypeId.COMPANY_SERVICE_ACCOUNT))
            .And.Satisfy(
                x => x.Id == _identityId && x.UserStatusId == UserStatusId.ACTIVE,
                x => x.Id == _secondId && x.UserStatusId == UserStatusId.PENDING
            );
    }

    #region Setup

    private void Setup(ICollection<TechnicalUser>? serviceAccounts = null, ICollection<Identity>? identities = null)
    {
        A.CallTo(() => _provisioningDbAccess.GetNextClientSequenceAsync())
            .Returns(1).Once();

        A.CallTo(() => _provisioningManager.SetupCentralServiceAccountClientAsync(A<string>._, A<ClientConfigRolesData>._, A<bool>._))
            .Returns(new ServiceAccountData("internal-sa1", _iamUserId, new ClientAuthData(IamClientAuthMethod.SECRET))).Once();

        A.CallTo(() => _userRepository.CreateIdentity(A<Guid>._, A<UserStatusId>._, A<IdentityTypeId>._, A<Action<Identity>>._))
            .ReturnsLazily((Guid companyId, UserStatusId userStatusId, IdentityTypeId identityTypeId, Action<Identity>? setOptionalFields) =>
            {
                var identity = new Identity(_identityId, DateTimeOffset.UtcNow, companyId, userStatusId, identityTypeId);
                setOptionalFields?.Invoke(identity);
                identities?.Add(identity);
                return identity;
            }).Once()
            .Then.ReturnsLazily((Guid companyId, UserStatusId userStatusId, IdentityTypeId identityTypeId, Action<Identity>? setOptionalFields) =>
            {
                var identity = new Identity(_secondId, DateTimeOffset.UtcNow, companyId, userStatusId, identityTypeId);
                setOptionalFields?.Invoke(identity);
                identities?.Add(identity);
                return identity;
            }).Once();

        A.CallTo(() => _technicalUserRepository.CreateTechnicalUser(A<Guid>._, A<string>._, A<string>._, A<string>._, A<TechnicalUserTypeId>._, A<TechnicalUserKindId>._, A<Action<TechnicalUser>>._))
            .ReturnsLazily((Guid identityId, string name, string description, string clientClientId, TechnicalUserTypeId technicalUserTypeId, TechnicalUserKindId technicalUserKindId, Action<TechnicalUser>? setOptionalParameters) =>
            {
                var sa = new TechnicalUser(
                    identityId,
                    Guid.NewGuid(),
                    name,
                    description,
                    technicalUserTypeId,
                    technicalUserKindId)
                {
                    ClientClientId = clientClientId
                };
                setOptionalParameters?.Invoke(sa);
                serviceAccounts?.Add(sa);
                return sa;
            });

        A.CallTo(() => _userRolesRepository.GetUserRoleDataUntrackedAsync(A<IEnumerable<Guid>>.That.Contains(_validUserRoleId)))
            .Returns(new[] { new UserRoleData(_validUserRoleId, _validClientId, "UserRole") }.ToAsyncEnumerable());
        A.CallTo(() => _userRolesRepository.GetUserRoleDataUntrackedAsync(A<IEnumerable<Guid>>.That.Contains(_dimUserRoleId)))
            .Returns(new[] { new UserRoleData(_dimUserRoleId, _dimClient, _dimRoleText) }.ToAsyncEnumerable());
        A.CallTo(() => _userRolesRepository.GetUserRoleDataUntrackedAsync(A<IEnumerable<Guid>>.That.Contains(_invalidUserRoleId)))
            .Returns(Enumerable.Empty<UserRoleData>().ToAsyncEnumerable());
        A.CallTo(() => _userRolesRepository.GetUserRoleDataUntrackedAsync(A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { _validUserRoleId, _dimUserRoleId })))
            .Returns(new UserRoleData[]
            {
                new(_validUserRoleId, _validClientId, "UserRole"),
                new(_dimUserRoleId, _dimClient, _dimRoleText)
            }.ToAsyncEnumerable());

        A.CallTo(() => _processStepRepository.CreateProcess(A<ProcessTypeId>._))
            .ReturnsLazily((ProcessTypeId processTypeId) => new Process<ProcessTypeId, ProcessStepTypeId>(_processId, processTypeId, Guid.NewGuid())).Once();
        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._, A<ProcessStepStatusId>._, A<Guid>._))
            .ReturnsLazily((ProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid processId) => new ProcessStep<ProcessTypeId, ProcessStepTypeId>(_processStepId, processStepTypeId, processStepStatusId, processId, DateTimeOffset.UtcNow)).Once();
    }

    #endregion
}
