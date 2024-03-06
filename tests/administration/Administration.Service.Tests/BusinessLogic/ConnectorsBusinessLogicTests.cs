/********************************************************************************
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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using System.Collections.Immutable;
using ConnectorData = Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models.ConnectorData;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class ConnectorsBusinessLogicTests
{
    private const string ValidCompanyBpn = "CATENAXBPN123";
    private static readonly Guid CompanyUserId = new("ac1cf001-7fbc-1f2f-817f-bce058020002");
    private static readonly Guid ServiceAccountUserId = new("ac1cf001-7fbc-1f2f-817f-bce058020003");
    private static readonly Guid ValidCompanyId = Guid.NewGuid();
    private static readonly Guid CompanyIdWithoutSdDocument = Guid.NewGuid();
    private static readonly Guid ExistingConnectorId = Guid.NewGuid();
    private static readonly Guid CompanyWithoutBpnId = Guid.NewGuid();
    private readonly Guid _validOfferSubscriptionId = Guid.NewGuid();
    private readonly IIdentityData _identity;
    private readonly IFixture _fixture;
    private readonly List<Connector> _connectors;
    private readonly ICountryRepository _countryRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionRepository;
    private readonly IConnectorsRepository _connectorsRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly ISdFactoryBusinessLogic _sdFactoryBusinessLogic;
    private readonly ConnectorsBusinessLogic _logic;
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
        _offerSubscriptionRepository = A.Fake<IOfferSubscriptionsRepository>();
        var identityService = A.Fake<IIdentityService>();
        _identity = A.Fake<IIdentityData>();
        _connectors = new List<Connector>();
        var options = A.Fake<IOptions<ConnectorsSettings>>();
        var settings = new ConnectorsSettings
        {
            MaxPageSize = 15,
            ValidCertificationContentTypes = new[]
            {
                "application/x-pem-file",
                "application/x-x509-ca-cert",
                "application/pkix-cert"
            }
        };
        _documentRepository = A.Fake<IDocumentRepository>();
        SetupRepositoryMethods();

        A.CallTo(() => options.Value).Returns(settings);
        A.CallTo(() => identityService.IdentityData).Returns(_identity);
        var logger = A.Fake<ILogger<ConnectorsBusinessLogic>>();

        SetupIdentity();

        _logic = new ConnectorsBusinessLogic(_portalRepositories, options, _sdFactoryBusinessLogic, identityService, logger);
    }

    #region GetAllCompanyConnectorDatas

    [Theory]
    [InlineData(0, 10, 5, 1, 0, 5)]
    [InlineData(1, 10, 5, 1, 1, 0)]
    [InlineData(0, 10, 20, 2, 0, 10)]
    [InlineData(1, 10, 20, 2, 1, 10)]
    [InlineData(1, 15, 20, 2, 1, 5)]
    public async Task GetAllCompanyConnectorDatas_WithValidData_ReturnsExpected(int page, int size, int numberOfElements, int numberOfPages, int resultPage, int resultPageSize)
    {
        var data = _fixture.CreateMany<ConnectorData>(numberOfElements).ToImmutableArray();

        A.CallTo(() => _connectorsRepository.GetAllCompanyConnectorsForCompanyId(A<Guid>._))
            .Returns((int skip, int take) => Task.FromResult<Pagination.Source<ConnectorData>?>(new(data.Length, data.Skip(skip).Take(take))));

        // Act
        var result = await _logic.GetAllCompanyConnectorDatas(page, size);

        // Assert
        A.CallTo(() => _connectorsRepository.GetAllCompanyConnectorsForCompanyId(_identity.CompanyId)).MustHaveHappenedOnceExactly();
        result.Should().NotBeNull();
        result.Meta.NumberOfElements.Should().Be(numberOfElements);
        result.Meta.NumberOfPages.Should().Be(numberOfPages);
        result.Meta.Page.Should().Be(resultPage);
        result.Meta.PageSize.Should().Be(resultPageSize);
        result.Content.Should().HaveCount(resultPageSize);
    }

    #endregion

    #region Create Connector

    [Fact]
    public async Task CreateConnectorAsync_WithValidInput_ReturnsCreatedConnectorData()
    {
        // Arrange
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", ServiceAccountUserId);

        // Act
        var result = await _logic.CreateConnectorAsync(connectorInput, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().NotBeEmpty();
        _connectors.Should().HaveCount(1);
        A.CallTo(() => _connectorsRepository.CreateConnectorAssignedSubscriptions(A<Guid>._, A<Guid>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task CreateConnectorAsync_WithInvalidTechnicalUser_ThrowsControllerArgumentException()
    {
        // Arrange
        var saId = Guid.NewGuid();
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", saId);

        // Act
        async Task Act() => await _logic.CreateConnectorAsync(connectorInput, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_ARGUMENT_TECH_USER_NOT_ACTIVE.ToString());
        ex.Parameters.Should().NotBeNull().And.Satisfy(
           x => x.Name == "technicalUserId"
           &&
           x.Value == saId.ToString(),
           y => y.Name == "companyId"
           &&
           y.Value == _identity.CompanyId.ToString()
           );
    }

    [Fact]
    public async Task CreateConnectorAsync_WithClientIdNull_DoesntSaveData()
    {
        // Arrange
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", null);

        // Act
        var result = await _logic.CreateConnectorAsync(connectorInput, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().NotBeEmpty();
        _connectors.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateConnectorAsync_WithoutSelfDescriptionDocument_ThrowsUnexpectedException()
    {
        // Arrange
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", null);
        A.CallTo(() => _identity.CompanyId).Returns(CompanyIdWithoutSdDocument);

        // Act
        async Task Act() => await _logic.CreateConnectorAsync(connectorInput, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        exception.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_UNEXPECTED_NO_DESCRIPTION.ToString());
    }

    [Fact]
    public async Task CreateConnectorAsync_WithInvalidLocation_ThrowsControllerArgumentException()
    {
        // Arrange
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "invalid", null);

        // Act
        async Task Act() => await _logic.CreateConnectorAsync(connectorInput, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        exception.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_ARGUMENT_LOCATION_NOT_EXIST.ToString());
    }

    [Fact]
    public async Task CreateConnectorAsync_WithCompanyWithoutBpn_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", null);
        A.CallTo(() => _identity.CompanyId).Returns(CompanyWithoutBpnId);

        // Act
        async Task Act() => await _logic.CreateConnectorAsync(connectorInput, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        exception.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_UNEXPECTED_NO_BPN_ASSIGNED.ToString());
    }

    [Fact]
    public async Task CreateConnectorAsync_WithFailingDapsService_ReturnsCreatedConnectorData()
    {
        // Arrange
        var connectorInput = new ConnectorInputModel("connectorName", "https://test.de", "de", null);

        // Act
        var result = await _logic.CreateConnectorAsync(connectorInput, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().NotBeEmpty();
        _connectors.Should().HaveCount(1);
        A.CallTo(() => _connectorsRepository.CreateConnectorAssignedSubscriptions(A<Guid>._, A<Guid>._)).MustNotHaveHappened();
    }

    #endregion

    #region CreateManagedConnectorAsync

    [Fact]
    public async Task CreateManagedConnectorAsync_WithValidInput_ReturnsCreatedConnectorData()
    {
        // Arrange
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", _validOfferSubscriptionId, ServiceAccountUserId);

        // Act
        var result = await _logic.CreateManagedConnectorAsync(connectorInput, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().NotBeEmpty();
        _connectors.Should().HaveCount(1);
        A.CallTo(() => _connectorsRepository.CreateConnectorAssignedSubscriptions(A<Guid>._, _validOfferSubscriptionId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithTechnicalUser_ReturnsCreatedConnectorData()
    {
        // Arrange
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", _validOfferSubscriptionId, null);

        SetupTechnicalIdentity();

        // Act
        var result = await _logic.CreateManagedConnectorAsync(connectorInput, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().NotBeEmpty();
        _connectors.Should().HaveCount(1);
        A.CallTo(() => _connectorsRepository.CreateConnectorAssignedSubscriptions(A<Guid>._, _validOfferSubscriptionId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithInvalidLocation_ThrowsControllerArgumentException()
    {
        // Arrange
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "invalid", _validOfferSubscriptionId, null);

        // Act
        async Task Act() => await _logic.CreateManagedConnectorAsync(connectorInput, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        exception.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_ARGUMENT_LOCATION_NOT_EXIST.ToString());
        exception.Parameters.Should().NotBeNull().And.Satisfy(
            x => x.Name == "location"
            &&
            x.Value == "invalid");
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithNotExistingSubscription_ThrowsNotFoundException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionRepository.CheckOfferSubscriptionWithOfferProvider(subscriptionId, ValidCompanyId))
            .Returns((false, default, default, default, default, default, default));
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", subscriptionId, null);

        SetupTechnicalIdentity();

        // Act
        async Task Act() => await _logic.CreateManagedConnectorAsync(connectorInput, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_NOT_OFFERSUBSCRIPTION_EXIST.ToString());
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithCallerNotOfferProvider_ThrowsForbiddenException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionRepository.CheckOfferSubscriptionWithOfferProvider(subscriptionId, ValidCompanyId))
            .Returns((true, false, default, default, default, default, default));
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", subscriptionId, null);

        SetupTechnicalIdentity();

        // Act
        async Task Act() => await _logic.CreateManagedConnectorAsync(connectorInput, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_NOT_PROVIDER_COMPANY_OFFER.ToString());
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithOfferAlreadyLinked_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionRepository.CheckOfferSubscriptionWithOfferProvider(subscriptionId, ValidCompanyId))
            .Returns((true, true, true, default, default, default, default));
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", subscriptionId, null);

        SetupTechnicalIdentity();

        // Act
        async Task Act() => await _logic.CreateManagedConnectorAsync(connectorInput, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_CONFLICT_OFFERSUBSCRIPTION_LINKED.ToString());
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithInactiveSubscription_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionRepository.CheckOfferSubscriptionWithOfferProvider(subscriptionId, ValidCompanyId))
            .Returns((true, true, false, OfferSubscriptionStatusId.INACTIVE, default, default, default));
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", subscriptionId, null);

        SetupTechnicalIdentity();

        // Act
        async Task Act() => await _logic.CreateManagedConnectorAsync(connectorInput, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_CONFLICT_STATUS_ACTIVE_OR_PENDING.ToString());
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithoutExistingSelfDescriptionDocument_ThrowsUnexpectedException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionRepository.CheckOfferSubscriptionWithOfferProvider(subscriptionId, A<Guid>.That.Matches(x => x == ValidCompanyId)))
            .Returns((true, true, false, OfferSubscriptionStatusId.ACTIVE, null, ValidCompanyId, ValidCompanyBpn));
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", subscriptionId, null);

        // Act
        async Task Act() => await _logic.CreateManagedConnectorAsync(connectorInput, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(Act);
        exception.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_CONFLICT_NO_DESCRIPTION.ToString());
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithSubscribingCompanyWithoutBpn_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionRepository.CheckOfferSubscriptionWithOfferProvider(subscriptionId, ValidCompanyId))
            .Returns((true, true, false, OfferSubscriptionStatusId.ACTIVE, Guid.NewGuid(), companyId, null));
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", subscriptionId, null);

        SetupTechnicalIdentity();

        // Act
        async Task Act() => await _logic.CreateManagedConnectorAsync(connectorInput, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_CONFLICT_SET_BPN.ToString());
    }

    [Fact]
    public async Task CreateManagedConnectorAsync_WithInvalidTechnicalUser_ThrowsControllerArgumentException()
    {
        // Arrange
        var saId = Guid.NewGuid();
        var connectorInput = new ManagedConnectorInputModel("connectorName", "https://test.de", "de", _validOfferSubscriptionId, saId);

        // Act
        async Task Act() => await _logic.CreateManagedConnectorAsync(connectorInput, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_ARGUMENT_TECH_USER_NOT_ACTIVE.ToString());
        ex.Parameters.Should().NotBeNull().And.Satisfy(
          x => x.Name == "technicalUserId"
          &&
          x.Value == saId.ToString(),
          y => y.Name == "companyId"
          &&
          y.Value == _identity.CompanyId.ToString()
          );
    }

    #endregion

    #region ProcessClearinghouseSelfDescription

    [Fact]
    public async Task ProcessClearinghouseSelfDescription_WithValidData_CallsExpected()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var data = new SelfDescriptionResponseData(connectorId, SelfDescriptionStatus.Confirm, null, "{ \"test\": true }");
        A.CallTo(() => _connectorsRepository.GetConnectorDataById(A<Guid>._))
            .Returns((connectorId, null));

        // Act
        await _logic.ProcessClearinghouseSelfDescription(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _connectorsRepository.GetConnectorDataById(connectorId)).MustHaveHappenedOnceExactly();
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
        A.CallTo(() => _connectorsRepository.GetConnectorDataById(A<Guid>._))
            .Returns<(Guid, Guid?)>(default);

        // Act
        async Task Act() => await _logic.ProcessClearinghouseSelfDescription(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_NOT_EXIST.ToString());
        A.CallTo(() => _connectorsRepository.GetConnectorDataById(connectorId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ProcessClearinghouseSelfDescription_WithExistingSelfDescriptionDocument_ThrowsConflictException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var data = new SelfDescriptionResponseData(connectorId, SelfDescriptionStatus.Confirm, null, "{ \"test\": true }");
        A.CallTo(() => _connectorsRepository.GetConnectorDataById(A<Guid>._))
            .Returns((connectorId, Guid.NewGuid()));

        // Act
        async Task Act() => await _logic.ProcessClearinghouseSelfDescription(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_CONFLICT_ALREADY_ASSIGNED.ToString());
        A.CallTo(() => _connectorsRepository.GetConnectorDataById(connectorId)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region DeleteConnector

    [Fact]
    public async Task DeleteConnectorAsync_WithDocumentId_AndActiveUser_ExpectedCalls()
    {
        // Arrange
        const DocumentStatusId DocumentStatusId = DocumentStatusId.LOCKED;
        var connectorId = Guid.NewGuid();
        var connector = new Connector(connectorId, null!, null!, null!);
        var selfDescriptionDocumentId = Guid.NewGuid();
        var connectorOfferSubscriptions = new[] {
            new ConnectorOfferSubscription(_fixture.Create<Guid>(), OfferSubscriptionStatusId.PENDING),
            new ConnectorOfferSubscription(_fixture.Create<Guid>(), OfferSubscriptionStatusId.PENDING),
        };
        var userId = Guid.NewGuid();
        Identity? identity = null;
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(A<Guid>._, _identity.CompanyId))
            .Returns(new DeleteConnectorData(true, selfDescriptionDocumentId, DocumentStatusId, ConnectorStatusId.ACTIVE, connectorOfferSubscriptions, UserStatusId.ACTIVE, userId));

        A.CallTo(() => _documentRepository.AttachAndModifyDocument(A<Guid>._, A<Action<Document>>._, A<Action<Document>>._))
            .Invokes((Guid docId, Action<Document>? initialize, Action<Document> modify)
                =>
            {
                var document = new Document(docId, null!, null!, null!, default, default, default, default);
                initialize?.Invoke(document);
                modify(document);
            });
        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(A<Guid>._, A<Action<Connector>>._, A<Action<Connector>>._))
            .Invokes((Guid _, Action<Connector>? initialize, Action<Connector> setOptionalFields) =>
            {
                initialize?.Invoke(connector);
                setOptionalFields.Invoke(connector);
            });
        A.CallTo(() => _userRepository.AttachAndModifyIdentity(A<Guid>._, A<Action<Identity>>._, A<Action<Identity>>._))
            .Invokes((Guid id, Action<Identity>? initialize, Action<Identity> modify) =>
            {
                identity = new Identity(id, default, Guid.Empty, default, default);
                initialize?.Invoke(identity);
                modify.Invoke(identity);
            });
        // Act
        await _logic.DeleteConnectorAsync(connectorId).ConfigureAwait(false);

        // Assert
        connector.StatusId.Should().Be(ConnectorStatusId.INACTIVE);
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(connectorId, _identity.CompanyId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.AttachAndModifyIdentity(userId, A<Action<Identity>>._, A<Action<Identity>>._)).MustHaveHappenedOnceExactly();
        identity.Should().NotBeNull().And.Match<Identity>(x => x.Id == userId && x.UserStatusId == UserStatusId.INACTIVE);
        A.CallTo(() => _documentRepository.AttachAndModifyDocument(selfDescriptionDocumentId, A<Action<Document>>._, A<Action<Document>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(connectorId, A<Action<Connector>>._, A<Action<Connector>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _connectorsRepository.DeleteConnectorAssignedSubscriptions(connectorId, A<IEnumerable<Guid>>.That.Matches(x => x.Count() == 2))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(UserStatusId.INACTIVE, "0762ce2b-4842-41cd-a786-aa1bfe7061a3")]
    [InlineData(null, null)]
    public async Task DeleteConnectorAsync_WithDocumentId_WithInactiveOrNoUser_ExpectedCalls(UserStatusId? statusId, string? id)
    {
        // Arrange
        const DocumentStatusId DocumentStatusId = DocumentStatusId.LOCKED;
        var connectorId = Guid.NewGuid();
        var connector = new Connector(connectorId, null!, null!, null!);
        var selfDescriptionDocumentId = Guid.NewGuid();
        var connectorOfferSubscriptions = new[] {
            new ConnectorOfferSubscription(_fixture.Create<Guid>(), OfferSubscriptionStatusId.PENDING),
            new ConnectorOfferSubscription(_fixture.Create<Guid>(), OfferSubscriptionStatusId.PENDING),
        };
        var userId = id == null ? default(Guid?) : new Guid(id);
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(A<Guid>._, _identity.CompanyId))
            .Returns(new DeleteConnectorData(true, selfDescriptionDocumentId, DocumentStatusId, ConnectorStatusId.ACTIVE, connectorOfferSubscriptions, statusId, userId));

        A.CallTo(() => _documentRepository.AttachAndModifyDocument(A<Guid>._, A<Action<Document>>._, A<Action<Document>>._))
            .Invokes((Guid docId, Action<Document>? initialize, Action<Document> modify)
                =>
            {
                var document = new Document(docId, null!, null!, null!, default, default, default, default);
                initialize?.Invoke(document);
                modify(document);
            });
        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(A<Guid>._, A<Action<Connector>>._, A<Action<Connector>>._))
            .Invokes((Guid _, Action<Connector>? initialize, Action<Connector> setOptionalFields) =>
            {
                initialize?.Invoke(connector);
                setOptionalFields.Invoke(connector);
            });
        // Act
        await _logic.DeleteConnectorAsync(connectorId).ConfigureAwait(false);

        // Assert
        connector.StatusId.Should().Be(ConnectorStatusId.INACTIVE);
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(connectorId, _identity.CompanyId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.AttachAndModifyIdentity(A<Guid>._, A<Action<Identity>>._, A<Action<Identity>>._)).MustNotHaveHappened();
        A.CallTo(() => _documentRepository.AttachAndModifyDocument(selfDescriptionDocumentId, A<Action<Document>>._, A<Action<Document>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(connectorId, A<Action<Connector>>._, A<Action<Connector>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _connectorsRepository.DeleteConnectorAssignedSubscriptions(connectorId, A<IEnumerable<Guid>>.That.Matches(x => x.Count() == 2))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteConnectorAsync_WithPendingAndWithoutDocumentId_ExpectedCalls()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var connectorOfferSubscriptions = new[] {
            new ConnectorOfferSubscription(_fixture.Create<Guid>(), OfferSubscriptionStatusId.PENDING),
            new ConnectorOfferSubscription(_fixture.Create<Guid>(), OfferSubscriptionStatusId.PENDING),
        };
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(A<Guid>._, _identity.CompanyId))
            .Returns(new DeleteConnectorData(true, null, null, ConnectorStatusId.PENDING, connectorOfferSubscriptions, UserStatusId.ACTIVE, Guid.NewGuid()));

        // Act
        await _logic.DeleteConnectorAsync(connectorId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(connectorId, _identity.CompanyId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _connectorsRepository.DeleteConnector(connectorId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _connectorsRepository.DeleteConnectorAssignedSubscriptions(connectorId, A<IEnumerable<Guid>>.That.Matches(x => x.Count() == 2))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteConnectorAsync_WithPendingAndDocumentId_ExpectedCalls()
    {
        // Arrange
        const DocumentStatusId DocumentStatusId = DocumentStatusId.LOCKED;
        var connectorId = Guid.NewGuid();
        var selfDescriptionDocumentId = Guid.NewGuid();
        var connectorOfferSubscriptions = new[] {
            new ConnectorOfferSubscription(_fixture.Create<Guid>(), OfferSubscriptionStatusId.PENDING),
            new ConnectorOfferSubscription(_fixture.Create<Guid>(), OfferSubscriptionStatusId.PENDING),
        };
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(A<Guid>._, _identity.CompanyId))
            .Returns(new DeleteConnectorData(true, selfDescriptionDocumentId, DocumentStatusId, ConnectorStatusId.PENDING, connectorOfferSubscriptions, UserStatusId.ACTIVE, Guid.NewGuid()));

        // Act
        await _logic.DeleteConnectorAsync(connectorId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(connectorId, _identity.CompanyId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.RemoveDocument(selfDescriptionDocumentId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _connectorsRepository.DeleteConnector(connectorId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _connectorsRepository.DeleteConnectorAssignedSubscriptions(connectorId, A<IEnumerable<Guid>>.That.Matches(x => x.Count() == 2))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteConnectorAsync_WithoutAssignedOfferSubscriptions_ExpectedCalls()
    {
        // Arrange
        const DocumentStatusId DocumentStatusId = DocumentStatusId.LOCKED;
        var connectorId = Guid.NewGuid();
        var selfDescriptionDocumentId = Guid.NewGuid();
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(A<Guid>._, _identity.CompanyId))
            .Returns(new DeleteConnectorData(true, selfDescriptionDocumentId, DocumentStatusId, ConnectorStatusId.PENDING, Enumerable.Empty<ConnectorOfferSubscription>(), UserStatusId.ACTIVE, Guid.NewGuid()));

        // Act
        await _logic.DeleteConnectorAsync(connectorId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(connectorId, _identity.CompanyId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.RemoveDocument(selfDescriptionDocumentId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _connectorsRepository.DeleteConnector(connectorId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _connectorsRepository.DeleteConnectorAssignedSubscriptions(connectorId, A<IEnumerable<Guid>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteConnectorAsync_WithOutDocumentId_ExpectedCalls()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(connectorId, _identity.CompanyId))
            .Returns(new DeleteConnectorData(true, null, null, ConnectorStatusId.ACTIVE, Enumerable.Empty<ConnectorOfferSubscription>(), UserStatusId.ACTIVE, Guid.NewGuid()));

        // Act
        async Task Act() => await _logic.DeleteConnectorAsync(connectorId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_CONFLICT_DELETION_DECLINED.ToString());
    }

    [Fact]
    public async Task DeleteConnectorAsync_WithInactiveConnector_ThrowsConflictException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(connectorId, _identity.CompanyId))
            .Returns(new DeleteConnectorData(true, null, null, ConnectorStatusId.ACTIVE, Enumerable.Empty<ConnectorOfferSubscription>(), UserStatusId.ACTIVE, Guid.NewGuid()));

        // Act
        async Task Act() => await _logic.DeleteConnectorAsync(connectorId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_CONFLICT_DELETION_DECLINED.ToString());
    }

    [Fact]
    public async Task DeleteConnectorAsync_ThrowsNotFoundException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(connectorId, _identity.CompanyId))
            .Returns(default(DeleteConnectorData));

        // Act
        async Task Act() => await _logic.DeleteConnectorAsync(connectorId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task DeleteConnectorAsync_ThrowsForbiddenException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(connectorId, _identity.CompanyId))
            .Returns(new DeleteConnectorData(false, null, null, default, Enumerable.Empty<ConnectorOfferSubscription>(), UserStatusId.ACTIVE, Guid.NewGuid()));

        // Act
        async Task Act() => await _logic.DeleteConnectorAsync(connectorId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_NOT_PROVIDER_COMPANY_NOR_HOST.ToString());
    }

    [Fact]
    public async Task DeleteConnectorAsync_WithPendingAndWithoutDocumentId_ThrowsForbiddenException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var offerSubscriptionId1 = _fixture.Create<Guid>();
        var offerSubscriptionId2 = _fixture.Create<Guid>();
        var offerSubscriptionId3 = _fixture.Create<Guid>();
        var connectorOfferSubscriptions = new[] {
            new ConnectorOfferSubscription(offerSubscriptionId1, OfferSubscriptionStatusId.ACTIVE),
            new ConnectorOfferSubscription(offerSubscriptionId2, OfferSubscriptionStatusId.ACTIVE),
            new ConnectorOfferSubscription(offerSubscriptionId3, OfferSubscriptionStatusId.PENDING),
        };
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(A<Guid>._, _identity.CompanyId))
            .Returns(new DeleteConnectorData(true, null, null, ConnectorStatusId.PENDING, connectorOfferSubscriptions, UserStatusId.ACTIVE, Guid.NewGuid()));

        // Act
        async Task Act() => await _logic.DeleteConnectorAsync(connectorId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_DELETION_FAILED_OFFER_SUBSCRIPTION.ToString());
    }

    [Fact]
    public async Task DeleteConnectorAsync_WithPendingAndDocumentId_ThrowsForbiddenException()
    {
        // Arrange
        const DocumentStatusId DocumentStatusId = DocumentStatusId.LOCKED;
        var connectorId = Guid.NewGuid();
        var selfDescriptionDocumentId = Guid.NewGuid();
        var offerSubscriptionId1 = _fixture.Create<Guid>();
        var offerSubscriptionId2 = _fixture.Create<Guid>();
        var offerSubscriptionId3 = _fixture.Create<Guid>();
        var connectorOfferSubscriptions = new[] {
            new ConnectorOfferSubscription(offerSubscriptionId1, OfferSubscriptionStatusId.ACTIVE),
            new ConnectorOfferSubscription(offerSubscriptionId2, OfferSubscriptionStatusId.ACTIVE),
            new ConnectorOfferSubscription(offerSubscriptionId3, OfferSubscriptionStatusId.PENDING),
        };
        A.CallTo(() => _connectorsRepository.GetConnectorDeleteDataAsync(A<Guid>._, _identity.CompanyId))
            .Returns(new DeleteConnectorData(true, selfDescriptionDocumentId, DocumentStatusId, ConnectorStatusId.PENDING, connectorOfferSubscriptions, UserStatusId.ACTIVE, Guid.NewGuid()));

        // Act
        async Task Act() => await _logic.DeleteConnectorAsync(connectorId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_DELETION_FAILED_OFFER_SUBSCRIPTION.ToString());
    }

    #endregion

    #region GetManagedConnectorForIamUserAsync

    [Theory]
    [InlineData(0, 10, 5, 1, 0, 5)]
    [InlineData(1, 10, 5, 1, 1, 0)]
    [InlineData(0, 10, 20, 2, 0, 10)]
    [InlineData(1, 10, 20, 2, 1, 10)]
    [InlineData(1, 15, 20, 2, 1, 5)]
    public async Task GetManagedConnectorForIamUserAsync_GetExpected(int page, int size, int numberOfElements, int numberOfPages, int resultPage, int resultPageSize)
    {
        // Arrange
        var data = _fixture.CreateMany<ManagedConnectorData>(numberOfElements).ToImmutableArray();

        A.CallTo(() => _connectorsRepository.GetManagedConnectorsForCompany(A<Guid>._))
            .Returns((int skip, int take) => Task.FromResult<Pagination.Source<ManagedConnectorData>?>(new(data.Length, data.Skip(skip).Take(take))));

        // Act
        var result = await _logic.GetManagedConnectorForCompany(page, size);

        // Assert
        A.CallTo(() => _connectorsRepository.GetManagedConnectorsForCompany(_identity.CompanyId)).MustHaveHappenedOnceExactly();
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
        A.CallTo(() => _connectorsRepository.GetManagedConnectorsForCompany(A<Guid>._))
            .Returns((int _, int _) => Task.FromResult<Pagination.Source<ManagedConnectorData>?>(null));

        // Act
        var result = await _logic.GetManagedConnectorForCompany(0, 10);

        // Assert
        A.CallTo(() => _connectorsRepository.GetManagedConnectorsForCompany(_identity.CompanyId)).MustHaveHappenedOnceExactly();
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
        A.CallTo(() => _connectorsRepository.GetConnectorUpdateInformation(connectorId, _identity.CompanyId))
            .Returns<ConnectorUpdateInformation?>(null);

        // Act
        async Task Act() => await _logic.UpdateConnectorUrl(connectorId, new ConnectorUpdateRequest("https://test.de")).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task UpdateConnectorUrl_WithSameUrlAsStored_ReturnsWithoutDoing()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var data = _fixture.Build<ConnectorUpdateInformation>()
            .With(x => x.ConnectorUrl, "https://test.de")
            .Create();
        A.CallTo(() => _connectorsRepository.GetConnectorUpdateInformation(connectorId, _identity.CompanyId))
            .Returns(data);

        // Act
        await _logic.UpdateConnectorUrl(connectorId, new ConnectorUpdateRequest("https://test.de")).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateConnectorUrl_WithUserNotOfHostCompany_ThrowsForbiddenException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var data = _fixture.Build<ConnectorUpdateInformation>()
            .With(x => x.ConnectorUrl, "https://old.de")
            .With(x => x.IsHostCompany, false)
            .Create();
        A.CallTo(() => _connectorsRepository.GetConnectorUpdateInformation(connectorId, _identity.CompanyId))
            .Returns(data);

        // Act
        async Task Act() => await _logic.UpdateConnectorUrl(connectorId, new ConnectorUpdateRequest("https://new.de")).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_NOT_HOST_COMPANY.ToString());
    }

    [Fact]
    public async Task UpdateConnectorUrl_WithInactiveConnector_ThrowsConflictException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var data = _fixture.Build<ConnectorUpdateInformation>()
            .With(x => x.ConnectorUrl, "https://old.de")
            .With(x => x.IsHostCompany, true)
            .With(x => x.Status, ConnectorStatusId.INACTIVE)
            .Create();
        A.CallTo(() => _connectorsRepository.GetConnectorUpdateInformation(connectorId, _identity.CompanyId))
            .Returns(data);

        // Act
        async Task Act() => await _logic.UpdateConnectorUrl(connectorId, new ConnectorUpdateRequest("https://new.de")).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_CONFLICT_INACTIVE_STATE.ToString());
    }

    [Fact]
    public async Task UpdateConnectorUrl_WithBpnNotSet_ThrowsConflictException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var data = _fixture.Build<ConnectorUpdateInformation>()
            .With(x => x.ConnectorUrl, "https://old.de")
            .With(x => x.IsHostCompany, true)
            .With(x => x.Status, ConnectorStatusId.ACTIVE)
            .With(x => x.Type, ConnectorTypeId.CONNECTOR_AS_A_SERVICE)
            .With(x => x.Bpn, default(string?))
            .Create();
        A.CallTo(() => _connectorsRepository.GetConnectorUpdateInformation(connectorId, _identity.CompanyId))
            .Returns(data);

        // Act
        async Task Act() => await _logic.UpdateConnectorUrl(connectorId, new ConnectorUpdateRequest("https://new.de")).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_CONFLICT_SET_BPN.ToString());
    }

    [Fact]
    public async Task UpdateConnectorUrl_WithCompanyBpnNotSet_ThrowsConflictException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var data = _fixture.Build<ConnectorUpdateInformation>()
            .With(x => x.ConnectorUrl, "https://old.de")
            .With(x => x.IsHostCompany, true)
            .With(x => x.Status, ConnectorStatusId.ACTIVE)
            .With(x => x.Type, ConnectorTypeId.COMPANY_CONNECTOR)
            .With(x => x.Bpn, "BPNL123456789")
            .Create();
        A.CallTo(() => _connectorsRepository.GetConnectorUpdateInformation(connectorId, _identity.CompanyId))
            .Returns(data);
        A.CallTo(() => _userRepository.GetCompanyBpnForIamUserAsync(_identity.IdentityId))
            .Returns<string?>(null);

        // Act
        async Task Act() => await _logic.UpdateConnectorUrl(connectorId, new ConnectorUpdateRequest("https://new.de")).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_CONFLICT_SET_BPN.ToString());
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
            .With(x => x.IsHostCompany, true)
            .With(x => x.Status, ConnectorStatusId.ACTIVE)
            .With(x => x.Type, ConnectorTypeId.CONNECTOR_AS_A_SERVICE)
            .With(x => x.Bpn, "BPNL123456789")
            .Create();
        A.CallTo(() => _connectorsRepository.GetConnectorUpdateInformation(connectorId, _identity.CompanyId))
            .Returns(data);
        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(connectorId, null, A<Action<Connector>>._))
            .Invokes((Guid _, Action<Connector>? initialize, Action<Connector> setOptionalProperties) =>
            {
                initialize?.Invoke(connector);
                setOptionalProperties.Invoke(connector);
            });

        // Act
        await _logic.UpdateConnectorUrl(connectorId, new ConnectorUpdateRequest("https://new.de")).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(connectorId, null, A<Action<Connector>>._)).MustHaveHappenedOnceExactly();
        connector.ConnectorUrl.Should().Be("https://new.de");
    }

    #endregion

    #region GetCompanyConnectorEndPointAsync

    [Fact]
    public async Task GetCompanyConnectorEndPoint_WithValidData_ReturnsExpectedResult()
    {
        //Arrange
        var bpns = new[]
        {
            "bpnL00000002CRHL",
            "BPNL00000003CRHL",
            "BPNL00000004CRHL",
            "BPNL00000003CRHK"
        };
        A.CallTo(() => _connectorsRepository.GetConnectorEndPointDataAsync(A<IEnumerable<string>>._))
            .Returns(new[] {
                (BusinessPartnerNumber: "BPNL00000002CRHL", ConnectorEndPoint: "www.googlr5.com"),
                (BusinessPartnerNumber: "BPNL00000003CRHL", ConnectorEndPoint: "www.googlr0.com"),
                (BusinessPartnerNumber: "BPNL00000003CRHL", ConnectorEndPoint: "www.googlr1.com"),
                (BusinessPartnerNumber: "BPNL00000004CRHL", ConnectorEndPoint: "www.googlr2.com"),
                (BusinessPartnerNumber: "BPNL00000004CRHL", ConnectorEndPoint: "www.googlr3.com"),
                (BusinessPartnerNumber: "BPNL00000004CRHL", ConnectorEndPoint: "www.googlr4.com")
            }.ToAsyncEnumerable());

        //Act
        var result = await _logic.GetCompanyConnectorEndPointAsync(bpns).ToListAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _connectorsRepository.GetConnectorEndPointDataAsync(A<IEnumerable<string>>.That.IsSameSequenceAs(bpns.Select(x => x.ToUpper())))).MustHaveHappenedOnceExactly();
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
            .Returns(new[] { (BusinessPartnerNumber: "CAXLBOSCHZZ", ConnectorEndPoint: "www.googlr.com") }.ToAsyncEnumerable());

        // Act
        async Task Act() => await _logic.GetCompanyConnectorEndPointAsync(bpns).ToListAsync().ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_ARGUMENT_INCORRECT_BPN.ToString());
    }

    #endregion

    #region GetConnectorOfferSubscriptionData

    [Fact]
    public async Task GetConnectorOfferSubscriptionData_ReturnsList()
    {
        // Arrange
        var data = _fixture.CreateMany<OfferSubscriptionConnectorData>(5);
        A.CallTo(() => _offerSubscriptionRepository.GetConnectorOfferSubscriptionData(null, _identity.CompanyId))
            .Returns(data.ToAsyncEnumerable());

        // Act
        var result = await _logic.GetConnectorOfferSubscriptionData(null).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(data.Count());
    }

    #endregion

    #region GetCompanyConnectorData

    [Fact]
    public async Task GetCompanyConnectorData_WithInvalid_ThrowsForbiddenException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var connectorData = _fixture.Create<ConnectorData>();
        A.CallTo(() => _connectorsRepository.GetConnectorByIdForCompany(connectorId, _identity.CompanyId))
            .Returns((connectorData, false));

        // Act
        async Task Act() => await _logic.GetCompanyConnectorData(connectorId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_NOT_PROVIDER_COMPANY.ToString());
    }

    [Fact]
    public async Task GetCompanyConnectorData_WithNotExisting_ThrowsNotFoundException()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        A.CallTo(() => _connectorsRepository.GetConnectorByIdForCompany(connectorId, _identity.CompanyId))
            .Returns<(ConnectorData, bool)>(default);

        // Act
        async Task Act() => await _logic.GetCompanyConnectorData(connectorId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(AdministrationConnectorErrors.CONNECTOR_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task GetCompanyConnectorData_WithValid_ReturnsExpected()
    {
        // Arrange
        var connectorId = Guid.NewGuid();
        var connectorData = _fixture.Build<ConnectorData>()
            .With(x => x.Name, "Test Connector")
            .Create();
        A.CallTo(() => _connectorsRepository.GetConnectorByIdForCompany(connectorId, _identity.CompanyId))
            .Returns((connectorData, true));

        // Act
        var result = await _logic.GetCompanyConnectorData(connectorId).ConfigureAwait(false);

        // Assert
        result.Name.Should().Be("Test Connector");
    }

    #endregion

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
            .Returns((null, null));
        A.CallTo(() => _offerSubscriptionRepository.CheckOfferSubscriptionWithOfferProvider(_validOfferSubscriptionId, ValidCompanyId))
            .Returns((true, true, false, OfferSubscriptionStatusId.ACTIVE, Guid.NewGuid(), ValidCompanyId, ValidCompanyBpn));

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

        A.CallTo(() => _connectorsRepository.GetConnectorInformationByIdForIamUser(ExistingConnectorId, _identity.CompanyId))
            .Returns((_fixture.Create<ConnectorInformationData>(), true));
        A.CallTo(() => _connectorsRepository.GetConnectorInformationByIdForIamUser(A<Guid>.That.Not.Matches(x => x == ExistingConnectorId), _identity.CompanyId))
            .Returns<(ConnectorInformationData, bool)>(default);
        A.CallTo(() => _connectorsRepository.GetConnectorInformationByIdForIamUser(ExistingConnectorId, A<Guid>.That.Not.Matches(x => x == _identity.CompanyId)))
            .Returns((_fixture.Create<ConnectorInformationData>(), false));

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
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionRepository);
    }

    private void SetupIdentity()
    {
        A.CallTo(() => _identity.IdentityId).Returns(CompanyUserId);
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(ValidCompanyId);
    }

    private void SetupTechnicalIdentity()
    {
        A.CallTo(() => _identity.IdentityId).Returns(ServiceAccountUserId);
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_SERVICE_ACCOUNT);
    }

    #endregion
}
