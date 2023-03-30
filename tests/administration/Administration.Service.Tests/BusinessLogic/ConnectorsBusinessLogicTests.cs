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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Daps.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class ConnectorsBusinessLogicTests
{
    private const string ValidCompanyBpn = "CATENAXBPN123";
    private const string CompanyBpnWithoutSdDocument = "NoSdDocument123";
    private const string AccessToken = "validToken";
    private static readonly Guid ValidCompanyId = Guid.NewGuid();
    private static readonly Guid CompanyIdWithoutSdDocument = Guid.NewGuid();
    private static readonly Guid ExistingConnectorId = Guid.NewGuid();
    private static readonly Guid CompanyWithoutBpnId = Guid.NewGuid();
    private static readonly string IamUserId = Guid.NewGuid().ToString();
    private static readonly string IamUserWithoutSdDocumentId = Guid.NewGuid().ToString();
    private static readonly string UserWithoutBpn = Guid.NewGuid().ToString();
    private static readonly string TechnicalUserId = Guid.NewGuid().ToString();
    private readonly IFixture _fixture;
    private readonly List<Connector> _connectors;
    private readonly ICountryRepository _countryRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IConnectorsRepository _connectorsRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly ISdFactoryBusinessLogic _sdFactoryBusinessLogic;
    private readonly ConnectorsBusinessLogic _logic;
    private readonly IDapsService _dapsService;
    private readonly ConnectorsSettings _settings;
    private readonly IDocumentRepository _documentRepository;

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
        _sdFactoryBusinessLogic = A.Fake<ISdFactoryBusinessLogic>();
        _dapsService = A.Fake<IDapsService>();
        _connectors = new List<Connector>();
        var options = A.Fake<IOptions<ConnectorsSettings>>();
        _settings = new ConnectorsSettings
        {
            MaxPageSize = 15,
            ValidCertificationContentTypes = new []
            {
                "application/x-pem-file",
                "application/x-x509-ca-cert",
                "application/pkix-cert"
            }
        };
        _documentRepository = A.Fake<IDocumentRepository>();
        SetupRepositoryMethods();

        A.CallTo(() => options.Value).Returns(_settings);

        _logic = new ConnectorsBusinessLogic(_portalRepositories, options, _sdFactoryBusinessLogic, _dapsService);
    }

    #region Create Connector

    [Fact]
    public async Task CreateConnectorAsync_WithValidInput_ReturnsCreatedConnectorData()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pem", "application/x-pem-file");
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", file);

        // Act
        var result = await _logic.CreateConnectorAsync(connectorInput, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().NotBeEmpty();
        _connectors.Should().HaveCount(1);
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._, A<IFormFile>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateConnectorAsync_WithoutSelfDescriptionDocument_ThrowsUnexpectedException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pem", "application/x-pem-file");
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", file);

        // Act
        async Task Act() => await _logic.CreateConnectorAsync(connectorInput, IamUserWithoutSdDocumentId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        exception.Message.Should().Be($"provider company {CompanyIdWithoutSdDocument} has no self description document");
    }

    [Fact]
    public async Task CreateConnectorAsync_WithInvalidLocation_ThrowsControllerArgumentException()
    {
        // Arrange
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "invalid", null);

        // Act
        async Task Act() => await _logic.CreateConnectorAsync(connectorInput, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        exception.Message.Should().Be("Location invalid does not exist (Parameter 'location')");
    }

    [Fact]
    public async Task CreateConnectorAsync_WithCompanyWithoutBpn_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", null);

        // Act
        async Task Act() => await _logic.CreateConnectorAsync(connectorInput, UserWithoutBpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        exception.Message.Should().Be($"provider company {CompanyWithoutBpnId} has no businessPartnerNumber assigned");
    }

    [Fact]
    public async Task CreateConnectorAsync_WithFailingDapsService_ReturnsCreatedConnectorData()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pem", "application/x-pem-file");
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", file);
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._,
            A<IFormFile>._, A<CancellationToken>._)).Throws(new ServiceException("Service failed"));

        // Act
        var result = await _logic.CreateConnectorAsync(connectorInput, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().NotBeEmpty();
        _connectors.Should().HaveCount(1);
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._, A<IFormFile>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateConnectorAsync_WithWrongFileType_ThrowsUnsupportedMediaTypeException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pdf", "application/pdf");
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", file);

        // Act
        async Task Act() => await _logic.CreateConnectorAsync(connectorInput, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Act);
        exception.Message.Should().Be($"Only {string.Join(",", _settings.ValidCertificationContentTypes)} files are allowed.");
    }

    #endregion

    #region CreateManagedConnectorAsync

    [Fact]
    public async Task CreateManagedConnectorAsync_WithValidInput_ReturnsCreatedConnectorData()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pdf", "application/x-pem-file");
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", ValidCompanyBpn, file);

        // Act
        var result = await _logic.CreateManagedConnectorAsync(connectorInput, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().NotBeEmpty();
        _connectors.Should().HaveCount(1);
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._, A<IFormFile>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithoutExisting_ThrowsUnexpectedException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pdf", "application/x-pem-file");
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", CompanyBpnWithoutSdDocument, file);

        // Act
        async Task Act() => await _logic.CreateManagedConnectorAsync(connectorInput, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        exception.Message.Should().Be($"provider company {ValidCompanyId} has no self description document");
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithTechnicalUser_ReturnsCreatedConnectorData()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("this is just random content", "cert.pem", "application/x-pem-file");
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", ValidCompanyBpn, file);

        // Act
        var result = await _logic.CreateManagedConnectorAsync(connectorInput, TechnicalUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().NotBeEmpty();
        _connectors.Should().HaveCount(1);
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._, A<IFormFile>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithInvalidLocation_ThrowsControllerArgumentException()
    {
        // Arrange
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "invalid", ValidCompanyBpn, null);

        // Act
        async Task Act() => await _logic.CreateManagedConnectorAsync(connectorInput, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        exception.ParamName.Should().Be("location");
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithNotExistingBpn_ThrowsControllerArgumentException()
    {
        // Arrange
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de",  "THISISNOTEXISTING", null);

        // Act
        async Task Act() => await _logic.CreateManagedConnectorAsync(connectorInput, TechnicalUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.ParamName.Should().Be("providerBpn");
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithWrongFileType_ThrowsUnsupportedMediaTypeException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pdf", "application/pdf");
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", "THISISNOTEXISTING", file);

        // Act
        async Task Act() => await _logic.CreateManagedConnectorAsync(connectorInput, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Act);
        exception.Message.Should().Be($"Only {string.Join(",", _settings.ValidCertificationContentTypes)} files are allowed.");
    }

    #endregion

    #region TriggerDaps

    [Fact]
    public async Task TriggerDaps_WithValidInput_CallsDaps()
    {
        // Arrange
        _connectors.Add(new Connector(ExistingConnectorId, "test", "de", "https://www.api.connector.com"));
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pem", "application/x-pem-file");

        // Act
        await _logic.TriggerDapsAsync(ExistingConnectorId, file, AccessToken, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._, A<IFormFile>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task TriggerDaps_WithNotExistingConnector_ThrowsNotFoundException()
    {
        // Arrange
        var notExistingConnectorId = Guid.NewGuid();
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pem", "application/x-pem-file");

        // Act
        async Task Act() => await _logic.TriggerDapsAsync(notExistingConnectorId,  file, AccessToken, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(Act);
        exception.Message.Should().Be($"Connector {notExistingConnectorId} does not exists");
    }

    [Fact]
    public async Task TriggerDaps_WithWrongUser_ThrowsForbiddenException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pem", "application/x-pem-file");

        // Act
        async Task Act() => await _logic.TriggerDapsAsync(ExistingConnectorId,  file, AccessToken, Guid.NewGuid().ToString(), CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(Act);
        exception.Message.Should().Be("User is not provider of the connector");
    }

    #endregion

    #region ProcessClearinghouseSelfDescription

    [Fact]
    public async Task ProcessClearinghouseSelfDescription_WithValidData_CallsExpected()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var data = new SelfDescriptionResponseData(connectorId, SelfDescriptionStatus.Confirm, null, "{ \"test\": true }");
        A.CallTo(() => _connectorsRepository.GetConnectorDataById(connectorId))
            .ReturnsLazily(() => new ValueTuple<Guid, Guid?>(connectorId, null));

        // Act
        await _logic.ProcessClearinghouseSelfDescription(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _sdFactoryBusinessLogic.ProcessFinishSelfDescriptionLpForConnector(data, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ProcessClearinghouseSelfDescription_WithNotExistingApplication_ThrowsNotFoundException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var data = new SelfDescriptionResponseData(connectorId, SelfDescriptionStatus.Confirm, null, "{ \"test\": true }");
        A.CallTo(() => _connectorsRepository.GetConnectorDataById(connectorId))
            .ReturnsLazily(() => new ValueTuple<Guid, Guid?>());

        // Act
        async Task Act() => await _logic.ProcessClearinghouseSelfDescription(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Connector {data.ExternalId} does not exist");
    }

    [Fact]
    public async Task ProcessClearinghouseSelfDescription_WithExistingSelfDescriptionDocument_ThrowsConflictException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var data = new SelfDescriptionResponseData(connectorId, SelfDescriptionStatus.Confirm, null, "{ \"test\": true }");
        A.CallTo(() => _connectorsRepository.GetConnectorDataById(connectorId))
            .ReturnsLazily(() => new ValueTuple<Guid, Guid?>(connectorId, Guid.NewGuid()));

        // Act
        async Task Act() => await _logic.ProcessClearinghouseSelfDescription(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Connector {data.ExternalId} already has a document assigned");
    }

    #endregion

    #region DeleteConnector

    [Fact]
    public async Task DeleteConnectorAsync_WithDocumentId_ExpectedCalls()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var selfDescriptionDocumentId = Guid.NewGuid();
        var documentStatusId = DocumentStatusId.LOCKED;
        A.CallTo(() => _connectorsRepository.GetSelfDescriptionDocumentDataAsync(connectorId))
            .Returns((true, selfDescriptionDocumentId, documentStatusId));

        A.CallTo(() => _documentRepository.AttachAndModifyDocument(A<Guid>._,A<Action<Document>>._,A<Action<Document>>._))
            .Invokes((Guid DocId, Action<Document>? initialize, Action<Document> modify)
                => {
                        var document = new Document(DocId, null!, null!, null!, default, default, default, default);
                        initialize?.Invoke(document);
                        modify(document);
                    });
        // Act
        await _logic.DeleteConnectorAsync(connectorId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _documentRepository.AttachAndModifyDocument(selfDescriptionDocumentId, A<Action<Document>>._, A<Action<Document>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _connectorsRepository.DeleteConnector(connectorId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteConnectorAsync_WithOutDocumentId_ExpectedCalls()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        A.CallTo(() => _connectorsRepository.GetSelfDescriptionDocumentDataAsync(connectorId))
            .Returns((true, null, null));

        // Act
        await _logic.DeleteConnectorAsync(connectorId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _documentRepository.AttachAndModifyDocument(A<Guid>._, A<Action<Document>>._, A<Action<Document>>._)).MustNotHaveHappened();
        A.CallTo(() => _connectorsRepository.DeleteConnector(connectorId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteConnectorAsync_ThrowsNotFoundException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        A.CallTo(() => _connectorsRepository.GetSelfDescriptionDocumentDataAsync(connectorId))
            .Returns(((bool,Guid?,DocumentStatusId?))default);

        // Act
        async Task Act() => await _logic.DeleteConnectorAsync(connectorId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Connector {connectorId} does not exist");
    }

    #endregion

    #region Setup

    private void SetupRepositoryMethods()
    {
        A.CallTo(() => _countryRepository.CheckCountryExistsByAlpha2CodeAsync(A<string>.That.Matches(x => x.Length == 2)))
            .Returns(true);
        A.CallTo(() => _countryRepository.CheckCountryExistsByAlpha2CodeAsync(A<string>.That.Not.Matches(x => x.Length == 2)))
            .Returns(false);

        A.CallTo(() => _companyRepository.GetCompanyBpnAndSelfDescriptionDocumentByIdAsync(A<Guid>.That.Matches(x => x == ValidCompanyId)))
            .Returns((ValidCompanyBpn, Guid.NewGuid()));
        A.CallTo(() => _companyRepository.GetCompanyBpnAndSelfDescriptionDocumentByIdAsync(A<Guid>.That.Matches(x => x == CompanyIdWithoutSdDocument)))
            .Returns((ValidCompanyBpn, null));
        A.CallTo(() => _companyRepository.GetCompanyBpnAndSelfDescriptionDocumentByIdAsync(A<Guid>.That.Not.Matches(x => x == ValidCompanyId || x == CompanyIdWithoutSdDocument)))
            .Returns((null,null));
        A.CallTo(() => _companyRepository.GetCompanyIdAndSelfDescriptionDocumentByBpnAsync(A<string>.That.Matches(x => x == ValidCompanyBpn)))
            .Returns((ValidCompanyId, Guid.NewGuid()));
        A.CallTo(() => _companyRepository.GetCompanyIdAndSelfDescriptionDocumentByBpnAsync(A<string>.That.Matches(x => x == CompanyBpnWithoutSdDocument)))
            .Returns((ValidCompanyId, null));
        A.CallTo(() => _companyRepository.GetCompanyIdAndSelfDescriptionDocumentByBpnAsync(A<string>.That.Not.Matches(x => x == ValidCompanyBpn || x == CompanyBpnWithoutSdDocument)))
            .Returns((default,null));

        A.CallTo(() => _connectorsRepository.CreateConnector(A<string>._, A<string>._, A<string>._, A<Action<Connector>?>._))
            .Invokes((string name, string location, string connectorUrl, Action<Connector>? setupOptionalFields) =>
            {
                var connector = new Connector(Guid.NewGuid(), name, location, connectorUrl);
                setupOptionalFields?.Invoke(connector);
                _connectors.Add(connector);
            })
            .Returns(new Connector(Guid.NewGuid(), null!, null!, null!));

        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(A<Guid>._, A<Action<Connector>>._))
            .Invokes((Guid connectorId, Action<Connector> setOptionalParameters) =>
            {
                var connector = _connectors.First(x => x.Id == connectorId);
                setOptionalParameters.Invoke(connector);
            });

        A.CallTo(() => _connectorsRepository.GetConnectorInformationByIdForIamUser(ExistingConnectorId, IamUserId))
            .Returns((_fixture.Create<ConnectorInformationData>(), true));
        A.CallTo(() => _connectorsRepository.GetConnectorInformationByIdForIamUser(A<Guid>.That.Not.Matches(x => x == ExistingConnectorId), IamUserId))
            .Returns(((ConnectorInformationData, bool))default);
        A.CallTo(() => _connectorsRepository.GetConnectorInformationByIdForIamUser(ExistingConnectorId, A<string>.That.Not.Matches(x => x == IamUserId)))
            .Returns((_fixture.Create<ConnectorInformationData>(), false));

        A.CallTo(() => _userRepository.GetOwnCompanyId(A<string>.That.Matches(x => x == IamUserId)))
            .Returns(ValidCompanyId);
        A.CallTo(() => _userRepository.GetOwnCompanyId(A<string>.That.Matches(x => x == IamUserWithoutSdDocumentId)))
            .Returns(CompanyIdWithoutSdDocument);
        A.CallTo(() => _userRepository.GetOwnCompanyId(A<string>.That.Matches(x => x == UserWithoutBpn)))
            .Returns(CompanyWithoutBpnId);

        A.CallTo(() => _userRepository.GetServiceAccountCompany(A<string>.That.Matches(x => x == TechnicalUserId)))
            .Returns(ValidCompanyId);
        A.CallTo(() => _userRepository.GetServiceAccountCompany(A<string>.That.Not.Matches(x => x == TechnicalUserId)))
            .Returns(Guid.Empty);

        A.CallTo(() => _sdFactoryBusinessLogic.RegisterConnectorAsync(A<Guid>._, A<string>._, A<string>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _portalRepositories.GetInstance<ICountryRepository>()).Returns(_countryRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConnectorsRepository>()).Returns(_connectorsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
    }

    #endregion
}