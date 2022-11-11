/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class ServiceAccountBusinessLogicTests
{
    private const string ValidBpn = "BPNL00000003CRHK";
    private const string ClientId = "Cl1-CX-Registration";
    private static readonly Guid ValidCompanyId = Guid.NewGuid();
    private static readonly string ValidAdminId = Guid.NewGuid().ToString();
    private readonly IServiceAccountCreation _serviceAccountCreation;
    private readonly IUserRepository _userRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IFixture _fixture;

    public ServiceAccountBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());  

        _userRepository = A.Fake<IUserRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _serviceAccountCreation = A.Fake<IServiceAccountCreation>();

        _fixture.Inject(Options.Create(new ServiceAccountSettings
        {
            ClientId = ClientId
        }));
        
        SetupRepositories();
        SetupServices();
    }

    [Fact]
    public async Task CreateOwnCompanyServiceAccountAsync_WithValidInput_ReturnsCreatedConnectorData()
    {
        // Arrange
        var serviceAccountCreationInfos = new ServiceAccountCreationInfo("TheName", "Just a short description", IamClientAuthMethod.SECRET, new []
        {
            Guid.NewGuid()
        });
        var sut = _fixture.Create<ServiceAccountBusinessLogic>();

        // Act
        var result = await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos, ValidAdminId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IamClientAuthMethod.Should().Be(IamClientAuthMethod.SECRET);
    }
    
    [Fact]
    public async Task CreateOwnCompanyServiceAccountAsync_WithInvalidUser_NotFoundException()
    {
        // Arrange
        var serviceAccountCreationInfos = new ServiceAccountCreationInfo("TheName", "Just a short description", IamClientAuthMethod.SECRET, new []
        {
            Guid.NewGuid()
        });
        var invalidUserId = Guid.NewGuid().ToString();
        var sut = _fixture.Create<ServiceAccountBusinessLogic>();

        // Act
        async Task Act() => await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos, invalidUserId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(Act);
        exception.Message.Should().Be($"user {invalidUserId} is not associated with any company");
    }
    
    [Fact]
    public async Task CreateOwnCompanyServiceAccountAsync_WithEmptyName_ThrowsControllerArgumentException()
    {
        // Arrange
        var serviceAccountCreationInfos = new ServiceAccountCreationInfo(string.Empty, "Just a short description", IamClientAuthMethod.SECRET, new []
        {
            Guid.NewGuid()
        });
        var sut = _fixture.Create<ServiceAccountBusinessLogic>();

        // Act
        async Task Act() => await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos, ValidAdminId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        exception.Message.Should().Be("name must not be empty (Parameter 'name')");
        exception.ParamName.Should().Be("name");
    }
    
    [Fact]
    public async Task CreateConnectorAsync_WithInvalidIamClientAuthMethod_ThrowsControllerArgumentException()
    {
        // Arrange
        var serviceAccountCreationInfos = new ServiceAccountCreationInfo("TheName", "Just a short description", IamClientAuthMethod.JWT, new []
        {
            Guid.NewGuid()
        });
        var sut = _fixture.Create<ServiceAccountBusinessLogic>();

        // Act
        async Task Act() => await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos, ValidAdminId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        exception.Message.Should().Be("other authenticationType values than SECRET are not supported yet (Parameter 'authenticationType')");
        exception.ParamName.Should().Be("authenticationType");
    }
    
    #region Setup

    private void SetupRepositories()
    {
        A.CallTo(() => _userRepository.GetCompanyIdAndBpnForIamUserUntrackedAsync(A<string>.That.Matches(x => x == ValidAdminId)))
            .ReturnsLazily(() => new ValueTuple<Guid, string>(ValidCompanyId, ValidBpn));
        A.CallTo(() => _userRepository.GetCompanyIdAndBpnForIamUserUntrackedAsync(A<string>.That.Not.Matches(x => x == ValidAdminId)))
            .ReturnsLazily(() => new ValueTuple<Guid, string>());

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        _fixture.Inject(_portalRepositories);
    }

    private void SetupServices()
    {
        A.CallTo(() => _serviceAccountCreation.CreateServiceAccountAsync(A<string>._, A<string>._, A<IamClientAuthMethod>._, A<IEnumerable<Guid>>._, A<Guid>.That.Matches(x => x == ValidCompanyId), A<IEnumerable<string>>._))
            .ReturnsLazily(() => new ValueTuple<string, ServiceAccountData, Guid, List<UserRoleData>>(ClientId, new ServiceAccountData(ClientId, Guid.NewGuid().ToString(), new ClientAuthData(IamClientAuthMethod.SECRET)), Guid.NewGuid(), new List<UserRoleData>()));
        A.CallTo(() => _serviceAccountCreation.CreateServiceAccountAsync(A<string>._, A<string>._, A<IamClientAuthMethod>._, A<IEnumerable<Guid>>._, A<Guid>.That.Matches(x => x == ValidCompanyId), A<IEnumerable<string>>._))
            .ReturnsLazily(() => new ValueTuple<string, ServiceAccountData, Guid, List<UserRoleData>>());

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
    }

    #endregion
}