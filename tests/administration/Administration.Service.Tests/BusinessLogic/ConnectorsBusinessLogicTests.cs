/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Models;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class ConnectorsBusinessLogicTests
{
    private static readonly Guid _validCompanyId = Guid.NewGuid();
    private static readonly string _validCompanyBpn = "CATENAXBPN123";
    private static readonly Guid _validHostId = Guid.NewGuid();
    private static readonly Guid _companyWithoutBpnId = Guid.NewGuid();
    private static readonly Guid _invalidCompanyId = Guid.NewGuid();
    private static readonly Guid _invalidHostId = Guid.NewGuid();
    private static readonly string _iamUserId = Guid.NewGuid().ToString();
    private static readonly string _technicalUserId = Guid.NewGuid().ToString();
    private static readonly string _accessToken = "validToken";
    private static readonly List<Connector> _connectors = new();
    private readonly ICountryRepository _countryRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IConnectorsRepository _connectorsRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly ISdFactoryService _sdFactoryService;
    private readonly IFixture _fixture;
    private readonly ConnectorsBusinessLogic _logic;
    private readonly IOptions<ConnectorsSettings> _options;
    private readonly ConnectorsSettings _settings;

    public ConnectorsBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());  

        _countryRepository = A.Fake<ICountryRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _connectorsRepository = A.Fake<IConnectorsRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _sdFactoryService = A.Fake<ISdFactoryService>();
        _options = A.Fake<IOptions<ConnectorsSettings>>();
        _settings = A.Fake<ConnectorsSettings>();
        _settings = new ConnectorsSettings
        {
            MaxPageSize = 15,
            SdFactoryUrl = "http://this-is-a-url.com",
        };

        SetupRepositoryMethods();

        A.CallTo(() => _options.Value).Returns(_settings);

        _logic = new ConnectorsBusinessLogic(_portalRepositories, _options, _sdFactoryService);
    }

    #region Create Connector
    
    [Fact]
    public async Task CreateConnectorAsync_WithValidInput_ReturnsCreatedConnectorData()
    {
        // Arrange
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", ConnectorStatusId.ACTIVE, "de");
        
        // Act
        var result = await _logic.CreateConnectorAsync(connectorInput, _accessToken, _iamUserId, CancellationToken.None).ConfigureAwait(false);
        
        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateConnectorAsync_WithInvalidLocation_ThrowsControllerArgumentException()
    {
        // Arrange
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", ConnectorStatusId.ACTIVE, "invalid");
        
        // Act
        async Task Act() => await _logic.CreateConnectorAsync(connectorInput, _accessToken, _iamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        exception.Message.Should().Be("Location invalid does not exist (Parameter 'location')");
    }
    
    #endregion
    
    #region CreateManagedConnectorAsync
    
    [Fact]
    public async Task CreateManagedConnectorAsync_WithValidInput_ReturnsCreatedConnectorData()
    {
        // Arrange
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", ConnectorStatusId.ACTIVE, "de", _validCompanyBpn);
        
        // Act
        var result = await _logic.CreateManagedConnectorAsync(connectorInput, _accessToken, _iamUserId, CancellationToken.None).ConfigureAwait(false);
        
        // Assert
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task CreateManagedConnectorAsync_WithTechnicalUser_ReturnsCreatedConnectorData()
    {
        // Arrange
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", ConnectorStatusId.ACTIVE, "de", _validCompanyBpn);
        
        // Act
        var result = await _logic.CreateManagedConnectorAsync(connectorInput, _accessToken, _technicalUserId, CancellationToken.None).ConfigureAwait(false);
        
        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithInvalidLocation_ThrowsControllerArgumentException()
    {
        // Arrange
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", ConnectorStatusId.ACTIVE, "invalid", _validCompanyBpn);
        
        // Act
        async Task Act() => await _logic.CreateManagedConnectorAsync(connectorInput, _accessToken, _iamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        exception.ParamName.Should().Be("location");
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithNotExistingBpn_ThrowsControllerArgumentException()
    {
        // Arrange
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", ConnectorStatusId.ACTIVE, "de",  "THISISNOTEXISTING");
        
        // Act
        async Task Act() => await _logic.CreateManagedConnectorAsync(connectorInput, _accessToken, _technicalUserId, CancellationToken.None).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.ParamName.Should().Be("providerBpn");
    }

    #endregion
    
    #region Setup

    private void SetupRepositoryMethods()
    {
        A.CallTo(() => _countryRepository.CheckCountryExistsByAlpha2CodeAsync(A<string>.That.Matches(x => x.Length == 2)))
            .Returns(true);
        A.CallTo(() => _countryRepository.CheckCountryExistsByAlpha2CodeAsync(A<string>.That.Not.Matches(x => x.Length == 2)))
            .Returns(false);

        A.CallTo(() => _companyRepository.GetCompanyBpnByIdAsync(A<Guid>.That.Matches(x => x == _validCompanyId)))
            .ReturnsLazily(() => _validCompanyBpn);
        A.CallTo(() => _companyRepository.GetCompanyBpnByIdAsync(A<Guid>.That.Not.Matches(x => x == _validCompanyId)))
            .ReturnsLazily(() => string.Empty);
        A.CallTo(() => _companyRepository.GetCompanyIdByBpnAsync(A<string>.That.Matches(x => x == _validCompanyBpn)))
            .ReturnsLazily(() => _validCompanyId);
        A.CallTo(() => _companyRepository.GetCompanyIdByBpnAsync(A<string>.That.Not.Matches(x => x == _validCompanyBpn)))
            .ReturnsLazily(() => Guid.Empty);
        
        A.CallTo(() =>
                _connectorsRepository.CreateConnector(A<string>._, A<string>._, A<string>._, A<Action<Connector>?>._))
            .Invokes(x =>
            {
                var name = x.Arguments.Get<string>("name")!;
                var location = x.Arguments.Get<string>("location")!;
                var connectorUrl = x.Arguments.Get<string>("connectorUrl")!;
                var action = x.Arguments.Get<Action<Connector?>>("setupOptionalFields");

                var connector = new Connector(Guid.NewGuid(), name, location, connectorUrl);
                action?.Invoke(connector);
                _connectors.Add(connector);
            });

        A.CallTo(() => _userRepository.GetOwnCompanyId(A<string>.That.Matches(x => x == _iamUserId)))
            .ReturnsLazily(() => _validCompanyId);
        A.CallTo(() => _userRepository.GetOwnCompanyId(A<string>.That.Not.Matches(x => x == _iamUserId)))
            .ReturnsLazily(() => Guid.Empty);

        A.CallTo(() => _userRepository.GetServiceAccountCompany(A<string>.That.Matches(x => x == _technicalUserId)))
            .ReturnsLazily(() => _validCompanyId);
        A.CallTo(() => _userRepository.GetServiceAccountCompany(A<string>.That.Not.Matches(x => x == _technicalUserId)))
            .ReturnsLazily(() => Guid.Empty);

        A.CallTo(() => _sdFactoryService.RegisterConnectorAsync(A<ConnectorRequestModel>._, A<string>.That.Matches(x => x == _accessToken), A<string>._, A<CancellationToken>._))
            .ReturnsLazily(Guid.NewGuid);
        A.CallTo(() =>
                _sdFactoryService.RegisterConnectorAsync(A<ConnectorRequestModel>._, A<string>.That.Not.Matches(x => x == _accessToken), A<string>._, A<CancellationToken>._))
            .Throws(() => new ServiceException("Access to SD factory failed with status code 401"));
        
        A.CallTo(() => _portalRepositories.GetInstance<ICountryRepository>()).Returns(_countryRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConnectorsRepository>()).Returns(_connectorsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
    }

    #endregion
}