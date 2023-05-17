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
using Org.Eclipse.TractusX.Portal.Backend.Daps.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class ConnectorsBusinessLogicTests
{
    private const string ValidCompanyBpn = "CATENAXBPN123";
    private const string CompanyBpnWithoutSdDocument = "NoSdDocument123";
    private static readonly Guid CompanyUserId = new("ac1cf001-7fbc-1f2f-817f-bce058020002");
    private static readonly Guid ServiceAccountUserId = new("ac1cf001-7fbc-1f2f-817f-bce058020003");
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
    private readonly IServiceAccountRepository _serviceAccountRepository;

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
        _serviceAccountRepository = A.Fake<IServiceAccountRepository>();
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
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", file, ServiceAccountUserId);
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._, A<IFormFile>._, A<CancellationToken>._))
            .Returns(new DapsResponse("12345"));

        // Act
        var result = await _logic.CreateConnectorAsync(connectorInput, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().NotBeEmpty();
        _connectors.Should().HaveCount(1);
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._, A<IFormFile>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateConnectorAsync_WithInvalidTechnicalUser_ThrowsControllerArgumentException()
    {
        // Arrange
        var saId = Guid.NewGuid();
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pem", "application/x-pem-file");
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", file, saId);
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._, A<IFormFile>._, A<CancellationToken>._))
            .Returns(new DapsResponse("12345"));

        // Act
        async Task Act() => await _logic.CreateConnectorAsync(connectorInput, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"Technical User {saId} is not assigned to company {ValidCompanyId} or is not active (Parameter 'technicalUserId')");
        ex.ParamName.Should().Be("technicalUserId");
    }

    [Fact]
    public async Task CreateConnectorAsync_WithClientIdNull_DoesntSaveData()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pem", "application/x-pem-file");
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", file, null);
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._, A<IFormFile>._, A<CancellationToken>._))
            .Returns((DapsResponse?)null);

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
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", file, null);

        // Act
        async Task Act() => await _logic.CreateConnectorAsync(connectorInput, IamUserWithoutSdDocumentId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        exception.Message.Should().Be($"provider company {CompanyIdWithoutSdDocument} has no self description document");
    }

    [Fact]
    public async Task CreateConnectorAsync_WithNotExistingUser_ThrowsConflictException()
    {
        // Arrange
        var iamUserId = Guid.NewGuid().ToString();
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pem", "application/x-pem-file");
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", file, null);

        // Act
        async Task Act() => await _logic.CreateConnectorAsync(connectorInput, iamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(Act);
        exception.Message.Should().Be($"No company found for user {iamUserId}");
    }

    [Fact]
    public async Task CreateConnectorAsync_WithInvalidLocation_ThrowsControllerArgumentException()
    {
        // Arrange
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "invalid", null, null);

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
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", null, null);

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
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", file, null);
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
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", file, null);

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
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", ValidCompanyBpn, file, ServiceAccountUserId);
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._, A<IFormFile>._, A<CancellationToken>._))
            .Returns(new DapsResponse("12345"));

        // Act
        var result = await _logic.CreateManagedConnectorAsync(connectorInput, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().NotBeEmpty();
        _connectors.Should().HaveCount(1);
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._, A<IFormFile>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithInvalidTechnicalUser_ThrowsControllerArgumentException()
    {
        // Arrange
        var saId = Guid.NewGuid();
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pdf", "application/x-pem-file");
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", ValidCompanyBpn, file, saId);
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._, A<IFormFile>._, A<CancellationToken>._))
            .Returns(new DapsResponse("12345"));

        // Act
        async Task Act() => await _logic.CreateManagedConnectorAsync(connectorInput, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"Technical User {saId} is not assigned to company {ValidCompanyId} or is not active (Parameter 'technicalUserId')");
        ex.ParamName.Should().Be("technicalUserId");
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithoutExisting_ThrowsUnexpectedException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pdf", "application/x-pem-file");
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", CompanyBpnWithoutSdDocument, file, null);

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
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", ValidCompanyBpn, file, null);
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._, A<IFormFile>._, A<CancellationToken>._))
            .Returns(new DapsResponse("12345"));

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
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "invalid", ValidCompanyBpn, null, null);

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
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de",  "THISISNOTEXISTING", null, null);

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
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", "THISISNOTEXISTING", file, null);

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
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._, A<IFormFile>._, A<CancellationToken>._))
            .Returns(new DapsResponse("12345"));

        // Act
        await _logic.TriggerDapsAsync(ExistingConnectorId, file, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._, A<IFormFile>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task TriggerDaps_WithFailingDapsCall_ThrowsConflictException()
    {
        // Arrange
        _connectors.Add(new Connector(ExistingConnectorId, "test", "de", "https://www.api.connector.com"));
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pem", "application/x-pem-file");
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._, A<IFormFile>._, A<CancellationToken>._))
            .Returns((DapsResponse?)null);

        // Act
        async Task Act() => await _logic.TriggerDapsAsync(ExistingConnectorId, file, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Client Id should be set here");
        A.CallTo(() => _dapsService.EnableDapsAuthAsync(A<string>._, A<string>._, A<string>._, A<IFormFile>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task TriggerDaps_WithNotExistingConnector_ThrowsNotFoundException()
    {
        // Arrange
        var notExistingConnectorId = Guid.NewGuid();
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pem", "application/x-pem-file");

        // Act
        async Task Act() => await _logic.TriggerDapsAsync(notExistingConnectorId,  file, IamUserId, CancellationToken.None).ConfigureAwait(false);

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
        async Task Act() => await _logic.TriggerDapsAsync(ExistingConnectorId,  file, Guid.NewGuid().ToString(), CancellationToken.None).ConfigureAwait(false);

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
            .Returns((connectorId, (Guid?)null));

        // Act
        await _logic.ProcessClearinghouseSelfDescription(data, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _sdFactoryBusinessLogic.ProcessFinishSelfDescriptionLpForConnector(data, CompanyUserId, A<CancellationToken>._))
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
            .Returns(((Guid,Guid?))default);

        // Act
        async Task Act() => await _logic.ProcessClearinghouseSelfDescription(data, IamUserId, CancellationToken.None).ConfigureAwait(false);

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
            .Returns((connectorId, Guid.NewGuid()));

        // Act
        async Task Act() => await _logic.ProcessClearinghouseSelfDescription(data, IamUserId, CancellationToken.None).ConfigureAwait(false);

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
        const DocumentStatusId documentStatusId = DocumentStatusId.LOCKED;
        var connectorId = Guid.NewGuid();
        var connector = new Connector(connectorId, null!, null!, null!);
        var selfDescriptionDocumentId = Guid.NewGuid();
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(connectorId))
            .Returns((true, "123", selfDescriptionDocumentId, documentStatusId, ConnectorStatusId.ACTIVE));

        A.CallTo(() => _documentRepository.AttachAndModifyDocument(A<Guid>._,A<Action<Document>>._,A<Action<Document>>._))
            .Invokes((Guid DocId, Action<Document>? initialize, Action<Document> modify)
                => {
                        var document = new Document(DocId, null!, null!, null!, default, default, default, default);
                        initialize?.Invoke(document);
                        modify(document);
                    });
        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(connectorId, A<Action<Connector>>._, A<Action<Connector>>._))
            .Invokes((Guid _, Action<Connector>? initialize, Action<Connector> setOptionalFields) =>
            {
                initialize?.Invoke(connector);
                setOptionalFields.Invoke(connector);
            });

        // Act
        await _logic.DeleteConnectorAsync(connectorId, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        connector.StatusId.Should().Be(ConnectorStatusId.INACTIVE);
        A.CallTo(() => _documentRepository.AttachAndModifyDocument(selfDescriptionDocumentId, A<Action<Document>>._, A<Action<Document>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _connectorsRepository.DeleteConnectorClientDetails(connectorId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dapsService.DeleteDapsClient("123", A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteConnectorAsync_WithOutDocumentId_ExpectedCalls()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var connector = new Connector(connectorId, null!, null!, null!);
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(connectorId))
            .Returns((true, "12345", null, null, ConnectorStatusId.ACTIVE));
        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(connectorId, A<Action<Connector>>._, A<Action<Connector>>._))
            .Invokes((Guid _, Action<Connector>? initialize, Action<Connector> setOptionalFields) =>
            {
                initialize?.Invoke(connector);
                setOptionalFields.Invoke(connector);
            });

        // Act
        await _logic.DeleteConnectorAsync(connectorId, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        connector.StatusId.Should().Be(ConnectorStatusId.INACTIVE);
        A.CallTo(() => _documentRepository.AttachAndModifyDocument(A<Guid>._, A<Action<Document>>._, A<Action<Document>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteConnectorAsync_WithInactiveConnector_ThrowsConflictException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(connectorId))
            .Returns((true, "1234", null, null, ConnectorStatusId.INACTIVE));

        // Act
        async Task Act() => await _logic.DeleteConnectorAsync(connectorId, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("INACTIVE Connector can not be deleted");
    }

    [Fact]
    public async Task DeleteConnectorAsync_WithEmptyDapsClientId_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(connectorId))
            .Returns((true, null, null, null, ConnectorStatusId.ACTIVE));

        // Act
        async Task Act() => await _logic.DeleteConnectorAsync(connectorId, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("DapsClientId must be set");
    }

    [Fact]
    public async Task DeleteConnectorAsync_ThrowsNotFoundException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(connectorId))
            .Returns(((bool,string?,Guid?,DocumentStatusId?, ConnectorStatusId))default);

        // Act
        async Task Act() => await _logic.DeleteConnectorAsync(connectorId, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Connector {connectorId} does not exist");
    }

    #endregion

    #region GetManagedConnectorForIamUserAsync

    [Theory]
    [InlineData(0,10,5,1,0,5)]
    [InlineData(1,10,5,1,1,0)]
    [InlineData(0,10,20,2,0,10)]
    [InlineData(1,10,20,2,1,10)]
    [InlineData(1,15,20,2,1,5)]
    public async Task GetManagedConnectorForIamUserAsync_GetExpected(int page, int size, int numberOfElements, int numberOfPages, int resultPage, int resultPageSize)
    {
        // Arrange
        var data = _fixture.CreateMany<ManagedConnectorData>(numberOfElements).ToImmutableArray();

        A.CallTo(() => _connectorsRepository.GetManagedConnectorsForIamUser(IamUserId))
            .Returns((int skip, int take) => Task.FromResult((Pagination.Source<ManagedConnectorData>?)new Pagination.Source<ManagedConnectorData>(data.Length, data.Skip(skip).Take(take))));

        // Act
        var result  = await _logic.GetManagedConnectorForIamUserAsync(IamUserId, page, size);
        
        // Assert
        result.Should().NotBeNull();
        result.Meta.NumberOfElements.Should().Be(numberOfElements);
        result.Meta.NumberOfPages.Should().Be(numberOfPages);
        result.Meta.Page.Should().Be(resultPage);
        result.Meta.PageSize.Should().Be(resultPageSize);
        result.Content.Should().HaveCount(resultPageSize);
    }

    [Fact]
    public async Task GetManagedConnectorForIamUserAsync_EmptyResult_GetExpected()
    {
        // Arrange
        A.CallTo(() => _connectorsRepository.GetManagedConnectorsForIamUser(IamUserId))
            .Returns((int _, int _) => Task.FromResult((Pagination.Source<ManagedConnectorData>?)null));

        // Act
        var result  = await _logic.GetManagedConnectorForIamUserAsync(IamUserId, 0, 10);
        
        // Assert
        result.Should().NotBeNull();
        result.Meta.NumberOfElements.Should().Be(0);
        result.Meta.NumberOfPages.Should().Be(0);
        result.Meta.Page.Should().Be(0);
        result.Meta.PageSize.Should().Be(0);
        result.Content.Should().BeEmpty();
    }

    #endregion

    #region UpdateConnectorUrl

    [Fact]
    public async Task UpdateConnectorUrl_WithoutConnector_ThrowsNotFoundException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        A.CallTo(() => _connectorsRepository.GetConnectorUpdateInformation(connectorId, IamUserId))
            .Returns((ConnectorUpdateInformation?)null);

        // Act
        async Task Act() => await _logic.UpdateConnectorUrl(connectorId, new ConnectorUpdateRequest("https://test.de"), IamUserId, CancellationToken.None).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Connector {connectorId} does not exists");
    }

    [Fact]
    public async Task UpdateConnectorUrl_WithSameUrlAsStored_ReturnsWithoutDoing()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var data = _fixture.Build<ConnectorUpdateInformation>()
            .With(x => x.ConnectorUrl, "https://test.de")
            .Create();
        A.CallTo(() => _connectorsRepository.GetConnectorUpdateInformation(connectorId, IamUserId))
            .Returns(data);

        // Act
        await _logic.UpdateConnectorUrl(connectorId, new ConnectorUpdateRequest("https://test.de"), IamUserId, CancellationToken.None).ConfigureAwait(false);
        
        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _dapsService.UpdateDapsConnectorUrl(A<string>._,A<string>._,A<string>._,A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateConnectorUrl_WithUserNotOfHostCompany_ThrowsForbiddenException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var data = _fixture.Build<ConnectorUpdateInformation>()
            .With(x => x.ConnectorUrl, "https://old.de")
            .With(x => x.IsUserOfHostCompany, false)
            .Create();
        A.CallTo(() => _connectorsRepository.GetConnectorUpdateInformation(connectorId, IamUserId))
            .Returns(data);

        // Act
        async Task Act() => await _logic.UpdateConnectorUrl(connectorId, new ConnectorUpdateRequest("https://new.de"), IamUserId, CancellationToken.None).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"User {IamUserId} is no user of the connectors host company");
    }

    [Fact]
    public async Task UpdateConnectorUrl_WithInactiveConnector_ThrowsConflictException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var data = _fixture.Build<ConnectorUpdateInformation>()
            .With(x => x.ConnectorUrl, "https://old.de")
            .With(x => x.IsUserOfHostCompany, true)
            .With(x => x.Status, ConnectorStatusId.INACTIVE)
            .Create();
        A.CallTo(() => _connectorsRepository.GetConnectorUpdateInformation(connectorId, IamUserId))
            .Returns(data);

        // Act
        async Task Act() => await _logic.UpdateConnectorUrl(connectorId, new ConnectorUpdateRequest("https://new.de"), IamUserId, CancellationToken.None).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Connector {connectorId} is in state {ConnectorStatusId.INACTIVE}");
    }

    [Fact]
    public async Task UpdateConnectorUrl_WithoutDapsClientId_ThrowsConflictException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var data = _fixture.Build<ConnectorUpdateInformation>()
            .With(x => x.ConnectorUrl, "https://old.de")
            .With(x => x.IsUserOfHostCompany, true)
            .With(x => x.Status, ConnectorStatusId.ACTIVE)
            .With(x => x.DapsClientId, (string?)null)
            .Create();
        A.CallTo(() => _connectorsRepository.GetConnectorUpdateInformation(connectorId, IamUserId))
            .Returns(data);

        // Act
        async Task Act() => await _logic.UpdateConnectorUrl(connectorId, new ConnectorUpdateRequest("https://new.de"), IamUserId, CancellationToken.None).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Connector {connectorId} has no client id");
    }

    [Fact]
    public async Task UpdateConnectorUrl_WithBpnNotSet_ThrowsConflictException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var data = _fixture.Build<ConnectorUpdateInformation>()
            .With(x => x.ConnectorUrl, "https://old.de")
            .With(x => x.IsUserOfHostCompany, true)
            .With(x => x.Status, ConnectorStatusId.ACTIVE)
            .With(x => x.DapsClientId, "1234")
            .With(x => x.Type, ConnectorTypeId.CONNECTOR_AS_A_SERVICE)
            .With(x => x.Bpn, (string?)null)
            .Create();
        A.CallTo(() => _connectorsRepository.GetConnectorUpdateInformation(connectorId, IamUserId))
            .Returns(data);

        // Act
        async Task Act() => await _logic.UpdateConnectorUrl(connectorId, new ConnectorUpdateRequest("https://new.de"), IamUserId, CancellationToken.None).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("The business partner number must be set here");
    }

    [Fact]
    public async Task UpdateConnectorUrl_WithCompanyBpnNotSet_ThrowsConflictException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var data = _fixture.Build<ConnectorUpdateInformation>()
            .With(x => x.ConnectorUrl, "https://old.de")
            .With(x => x.IsUserOfHostCompany, true)
            .With(x => x.Status, ConnectorStatusId.ACTIVE)
            .With(x => x.DapsClientId, "1234")
            .With(x => x.Type, ConnectorTypeId.COMPANY_CONNECTOR)
            .With(x => x.Bpn, "BPNL123456789")
            .Create();
        A.CallTo(() => _connectorsRepository.GetConnectorUpdateInformation(connectorId, IamUserId))
            .Returns(data);
        A.CallTo(() => _userRepository.GetCompanyBpnForIamUserAsync(IamUserId))
            .Returns((string?)null);

        // Act
        async Task Act() => await _logic.UpdateConnectorUrl(connectorId, new ConnectorUpdateRequest("https://new.de"), IamUserId, CancellationToken.None).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("The business partner number must be set here");
    }

    [Fact]
    public async Task UpdateConnectorUrl_WithValidData_CallsExpected()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var connector = _fixture.Build<Connector>()
            .With(x => x.ConnectorUrl, "https://old.de")
            .Create();
        var data = _fixture.Build<ConnectorUpdateInformation>()
            .With(x => x.ConnectorUrl, "https://old.de")
            .With(x => x.IsUserOfHostCompany, true)
            .With(x => x.Status, ConnectorStatusId.ACTIVE)
            .With(x => x.DapsClientId, "1234")
            .With(x => x.Type, ConnectorTypeId.CONNECTOR_AS_A_SERVICE)
            .With(x => x.Bpn, "BPNL123456789")
            .Create();
        A.CallTo(() => _connectorsRepository.GetConnectorUpdateInformation(connectorId, IamUserId))
            .Returns(data);
        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(connectorId, null, A<Action<Connector>>._))
            .Invokes((Guid _, Action<Connector>? initialize, Action<Connector> setOptionalProperties) =>
            {
                initialize?.Invoke(connector);
                setOptionalProperties.Invoke(connector);
            });

        // Act
        await _logic.UpdateConnectorUrl(connectorId, new ConnectorUpdateRequest("https://new.de"), IamUserId, CancellationToken.None).ConfigureAwait(false);
        
        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(connectorId, null, A<Action<Connector>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dapsService.UpdateDapsConnectorUrl("1234", "https://new.de", A<string>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        connector.ConnectorUrl.Should().Be("https://new.de");
    }

    #endregion
    
    [Fact]
    public async Task GetCompanyConnectorEndPoint_WithValidData_ReturnsExpectedResult()
    {
        //Arrange
        var bpns = new[]
        {
            "BPNL00000002CRHL",
            "BPNL00000003CRHL",
            "BPNL00000004CRHL",
            "BPNL00000003CRHK"
        };
        A.CallTo(() => _connectorsRepository.GetConnectorEndPointDataAsync(bpns))
            .Returns(new [] {
                (BusinessPartnerNumber: "BPNL00000003CRHL", ConnectorEndPoint: "www.googlr0.com"),
                (BusinessPartnerNumber: "BPNL00000003CRHL", ConnectorEndPoint: "www.googlr1.com"),
                (BusinessPartnerNumber: "BPNL00000004CRHL", ConnectorEndPoint: "www.googlr2.com"),
                (BusinessPartnerNumber: "BPNL00000004CRHL", ConnectorEndPoint: "www.googlr3.com"),
                (BusinessPartnerNumber: "BPNL00000004CRHL", ConnectorEndPoint: "www.googlr4.com"),
                (BusinessPartnerNumber: "BPNL00000002CRHL", ConnectorEndPoint: "www.googlr5.com")
            }.ToAsyncEnumerable());

        //Act
        var result = await _logic.GetCompanyConnectorEndPointAsync(bpns).ToListAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _connectorsRepository.GetConnectorEndPointDataAsync(bpns)).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(3).And.Satisfy(
            x => x.Bpn == "BPNL00000002CRHL" && x.ConnectorEndpoint.Count() == 1 && x.ConnectorEndpoint.Contains("www.googlr5.com"),
            x => x.Bpn == "BPNL00000003CRHL" && x.ConnectorEndpoint.Count() == 2 && x.ConnectorEndpoint.Contains("www.googlr0.com") && x.ConnectorEndpoint.Contains("www.googlr1.com"),
            x => x.Bpn == "BPNL00000004CRHL" && x.ConnectorEndpoint.Count() == 3 && x.ConnectorEndpoint.Contains("www.googlr2.com") && x.ConnectorEndpoint.Contains("www.googlr3.com") && x.ConnectorEndpoint.Contains("www.googlr4.com")
        );
    }

    [Fact]
    public async Task GetCompanyConnectorEndPoint_WithInValidBpn_ThrowsArgumentException()
    {
        //Arrange
        var bpns = new[]
        {
            "CAXLBOSCHZZ"
        };
        A.CallTo(() => _connectorsRepository.GetConnectorEndPointDataAsync(bpns))
            .Returns(new [] { (BusinessPartnerNumber: "CAXLBOSCHZZ", ConnectorEndPoint: "www.googlr.com") }.ToAsyncEnumerable());

         // Act
        async Task Act() => await _logic.GetCompanyConnectorEndPointAsync(bpns).ToListAsync().ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"Incorrect BPN [{bpns[0]}] attribute value");
    }

    #region Setup

    private void SetupRepositoryMethods()
    {
        A.CallTo(() => _countryRepository.CheckCountryExistsByAlpha2CodeAsync(A<string>.That.Matches(x => x.Length == 2)))
            .Returns(true);
        A.CallTo(() => _countryRepository.CheckCountryExistsByAlpha2CodeAsync(A<string>.That.Not.Matches(x => x.Length == 2)))
            .Returns(false);

        A.CallTo(() => _companyRepository.GetCompanyBpnAndSelfDescriptionDocumentByIdAsync(ValidCompanyId))
            .Returns((ValidCompanyBpn, Guid.NewGuid()));
        A.CallTo(() => _companyRepository.GetCompanyBpnAndSelfDescriptionDocumentByIdAsync(CompanyIdWithoutSdDocument))
            .Returns((ValidCompanyBpn, null));
        A.CallTo(() => _companyRepository.GetCompanyBpnAndSelfDescriptionDocumentByIdAsync(A<Guid>.That.Not.Matches(x => x == ValidCompanyId || x == CompanyIdWithoutSdDocument)))
            .Returns((null,null));
        A.CallTo(() => _companyRepository.GetCompanyIdAndSelfDescriptionDocumentByBpnAsync(ValidCompanyBpn))
            .Returns((ValidCompanyId, Guid.NewGuid()));
        A.CallTo(() => _companyRepository.GetCompanyIdAndSelfDescriptionDocumentByBpnAsync(CompanyBpnWithoutSdDocument))
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

        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(A<Guid>._, A<Action<Connector>?>._, A<Action<Connector>>.That.IsNotNull()))
            .Invokes((Guid connectorId, Action<Connector>? initialize, Action<Connector> setOptionalParameters) =>
            {
                var connector = _connectors.First(x => x.Id == connectorId);
                initialize?.Invoke(connector);
                setOptionalParameters.Invoke(connector);
            });

        A.CallTo(() => _connectorsRepository.GetConnectorInformationByIdForIamUser(ExistingConnectorId, IamUserId))
            .Returns((_fixture.Create<ConnectorInformationData>(), true));
        A.CallTo(() => _connectorsRepository.GetConnectorInformationByIdForIamUser(A<Guid>.That.Not.Matches(x => x == ExistingConnectorId), IamUserId))
            .Returns(((ConnectorInformationData, bool))default);
        A.CallTo(() => _connectorsRepository.GetConnectorInformationByIdForIamUser(ExistingConnectorId, A<string>.That.Not.Matches(x => x == IamUserId)))
            .Returns((_fixture.Create<ConnectorInformationData>(), false));

        A.CallTo(() => _userRepository.GetOwnCompanyId(IamUserId))
            .Returns(ValidCompanyId);
        A.CallTo(() => _userRepository.GetOwnCompanyId(IamUserWithoutSdDocumentId))
            .Returns(CompanyIdWithoutSdDocument);
        A.CallTo(() => _userRepository.GetOwnCompanyId(UserWithoutBpn))
            .Returns(CompanyWithoutBpnId);

        A.CallTo(() => _companyRepository.GetCompanyIdAndUserIdForUserOrTechnicalUser(IamUserId))
            .Returns((ValidCompanyId, CompanyUserId));
        A.CallTo(() => _companyRepository.GetCompanyIdAndUserIdForUserOrTechnicalUser(A<string>.That.Matches(x => x == UserWithoutBpn)))
            .Returns((CompanyWithoutBpnId, CompanyUserId));
        A.CallTo(() => _companyRepository.GetCompanyIdAndUserIdForUserOrTechnicalUser(A<string>.That.Matches(x => x == IamUserWithoutSdDocumentId)))
            .Returns((CompanyIdWithoutSdDocument, CompanyUserId));
        A.CallTo(() => _companyRepository.GetCompanyIdAndUserIdForUserOrTechnicalUser(A<string>.That.Matches(x => x == TechnicalUserId)))
            .Returns((ValidCompanyId, Guid.Empty));
        A.CallTo(() => _companyRepository.GetCompanyIdAndUserIdForUserOrTechnicalUser(A<string>.That.Not.Matches(x => x == TechnicalUserId || x == IamUserId || x == UserWithoutBpn || x == IamUserWithoutSdDocumentId)))
            .Returns(new ValueTuple<Guid, Guid>());

        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(IamUserId))
            .Returns(CompanyUserId);

        A.CallTo(() => _serviceAccountRepository.CheckActiveServiceAccountExistsForCompanyAsync(ServiceAccountUserId, ValidCompanyId))
            .Returns(true);

        A.CallTo(() => _sdFactoryBusinessLogic.RegisterConnectorAsync(A<Guid>._, A<string>._, A<string>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _portalRepositories.GetInstance<IServiceAccountRepository>()).Returns(_serviceAccountRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICountryRepository>()).Returns(_countryRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConnectorsRepository>()).Returns(_connectorsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
    }

    #endregion
}