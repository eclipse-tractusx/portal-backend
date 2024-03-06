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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Tests;

public class SdFactoryBusinessLogicTests
{
    #region Initialization

    private const string CountryCode = "DE";
    private const string Bpn = "BPNL000000000009";
    private static readonly Guid ApplicationId = new("ac1cf001-7fbc-1f2f-817f-bce058020001");
    private static readonly Guid CompanyUserId = new("ac1cf001-7fbc-1f2f-817f-bce058020002");
    private readonly Process _process;
    private static readonly Guid CompanyId = new("b4697623-dd87-410d-abb8-6d4f4d87ab58");
    private static readonly IEnumerable<(UniqueIdentifierId Id, string Value)> UniqueIdentifiers = new List<(UniqueIdentifierId Id, string Value)>
    {
        new (UniqueIdentifierId.VAT_ID, "JUSTATEST")
    };

    private readonly IApplicationRepository _applicationRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IConnectorsRepository _connectorsRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly ISdFactoryService _service;
    private readonly ICollection<Document> _documents;

    private readonly SdFactoryBusinessLogic _sut;
    private readonly IFixture _fixture;
    private readonly IApplicationChecklistService _checklistService;

    public SdFactoryBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _process = _fixture.Create<Process>();

        _documents = new HashSet<Document>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _connectorsRepository = A.Fake<IConnectorsRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _checklistService = A.Fake<IApplicationChecklistService>();
        _service = A.Fake<ISdFactoryService>();

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConnectorsRepository>()).Returns(_connectorsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);

        _sut = new SdFactoryBusinessLogic(_service, _portalRepositories, _checklistService);
    }

    #endregion

    #region Register Connector

    [Fact]
    public async Task RegisterConnectorAsync_ExpectedServiceCallIsMade()
    {
        // Arrange
        const string url = "https://connect-tor.com";
        var id = Guid.NewGuid();

        // Act
        await _sut.RegisterConnectorAsync(id, url, Bpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _service.RegisterConnectorAsync(id, url, Bpn, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region StartSelfDescriptionRegistration

    [Fact]
    public async Task StartSelfDescriptionRegistration_WithValidData_CompanyIsUpdated()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        var entry = new ApplicationChecklistEntry(Guid.NewGuid(), ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(ApplicationId))
            .Returns((CompanyId, Bpn, CountryCode, UniqueIdentifiers));

        // Act
        var result = await _sut.StartSelfDescriptionRegistration(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _service.RegisterSelfDescriptionAsync(ApplicationId, UniqueIdentifiers, CountryCode, Bpn, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        result.Should().NotBeNull();
        result.ModifyChecklistEntry.Should().NotBeNull();
        result.ModifyChecklistEntry!.Invoke(entry);
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.IN_PROGRESS);
        result.ScheduleStepTypeIds.Should().ContainSingle().And.Match(x => x.Single() == ProcessStepTypeId.FINISH_SELF_DESCRIPTION_LP);
        result.SkipStepTypeIds.Should().BeNull();
        result.Modified.Should().BeTrue();
    }

    [Fact]
    public async Task StartSelfDescriptionRegistration_WithNoApplication_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(ApplicationId))
            .Returns<(Guid, string?, string, IEnumerable<(UniqueIdentifierId, string)>)>(default);

        // Act
        async Task Act() => await _sut.StartSelfDescriptionRegistration(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"CompanyApplication {ApplicationId} is not in status SUBMITTED");
        A.CallTo(() => _service.RegisterSelfDescriptionAsync(ApplicationId, UniqueIdentifiers, CountryCode, Bpn, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task StartSelfDescriptionRegistration_WithBpnNotSet_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(ApplicationId))
            .Returns((CompanyId, null, CountryCode, Enumerable.Empty<(UniqueIdentifierId Id, string Value)>()));

        // Act
        async Task Act() => await _sut.StartSelfDescriptionRegistration(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"BusinessPartnerNumber (bpn) for CompanyApplications {ApplicationId} company {CompanyId} is empty");
        A.CallTo(() => _service.RegisterSelfDescriptionAsync(ApplicationId, UniqueIdentifiers, CountryCode, Bpn, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    #endregion

    #region ProcessFinishSelfDescriptionLp

    [Fact]
    public async Task ProcessFinishSelfDescriptionLp_ConfirmWithValidData_CompanyIsUpdated()
    {
        // Arrange
        var applicationChecklistEntry = new ApplicationChecklistEntry(Guid.NewGuid(), ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var company = _fixture.Build<Company>()
            .With(x => x.SelfDescriptionDocumentId, default(Guid?))
            .Create();
        const string contentJson = "{\"@context\":[\"https://www.w3.org/2018/credentials/v1\",\"https://github.com/catenax-ng/tx-sd-factory/raw/clearing-house/src/main/resources/verifiablecredentials.jsonld/sd-document-v22.10.jsonld\",\"https://w3id.org/vc/status-list/2021/v1\"],\"type\":[\"VerifiableCredential\",\"LegalPerson\"],\"issuer\":\"did:sov:12345\",\"issuanceDate\":\"2023-02-18T23:03:16Z\",\"expirationDate\":\"2023-05-19T23:03:16Z\",\"credentialSubject\":{\"bpn\":\"BPNL000000000000\",\"registrationNumber\":[{\"type\":\"local\",\"value\":\"o12345678\"}],\"headquarterAddress\":{\"countryCode\":\"DE\"},\"type\":\"LegalPerson\",\"legalAddress\":{\"countryCode\":\"DE\"},\"id\":\"did:sov:12345\"},\"credentialStatus\":{\"id\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\",\"type\":\"StatusList2021Entry\",\"statusPurpose\":\"revocation\",\"statusListIndex\":\"58\",\"statusListCredential\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\"},\"proof\":{\"type\":\"Ed25519Signature2018\",\"created\":\"2023-02-18T23:03:18Z\",\"proofPurpose\":\"assertionMethod\",\"verificationMethod\":\"did:sov:12345#key-1\",\"jws\":\"test\"}}";
        var data = new SelfDescriptionResponseData(ApplicationId, SelfDescriptionStatus.Confirm, null, contentJson);
        SetupForProcessFinish(company, applicationChecklistEntry);

        // Act
        await _sut.ProcessFinishSelfDescriptionLpForApplication(data, company.Id, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count(y => y == ProcessStepTypeId.ACTIVATE_APPLICATION) == 1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.CreateDocument($"SelfDescription_LegalPerson.json", A<byte[]>._, A<byte[]>._, MediaTypeId.JSON, DocumentTypeId.SELF_DESCRIPTION, A<Action<Document>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(company.Id, null, A<Action<Company>>._)).MustHaveHappenedOnceExactly();

        applicationChecklistEntry.Comment.Should().BeNull();
        applicationChecklistEntry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);

        _documents.Should().HaveCount(1);
        var document = _documents.Single();
        document.DocumentName.Should().Be($"SelfDescription_LegalPerson.json");
        company.SelfDescriptionDocumentId.Should().Be(document.Id);
    }

    [Fact]
    public async Task ProcessFinishSelfDescriptionLp_ConfirmWitNoDocument_ThrowsConflictException()
    {
        // Arrange
        var company = _fixture.Build<Company>()
            .With(x => x.SelfDescriptionDocumentId, default(Guid?))
            .Create();
        var data = new SelfDescriptionResponseData(ApplicationId, SelfDescriptionStatus.Confirm, null, null);

        // Act
        async Task Act() => await _sut.ProcessFinishSelfDescriptionLpForApplication(data, company.Id, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Please provide a selfDescriptionDocument");
    }

    [Fact]
    public async Task ProcessFinishSelfDescriptionLp_FailedWithValidData_CompanyIsUpdated()
    {
        // Arrange
        var applicationChecklistEntry = new ApplicationChecklistEntry(Guid.NewGuid(), ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var company = _fixture.Build<Company>()
            .With(x => x.SelfDescriptionDocumentId, default(Guid?))
            .Create();
        var data = new SelfDescriptionResponseData(ApplicationId, SelfDescriptionStatus.Failed, "test message", null);
        SetupForProcessFinish(company, applicationChecklistEntry);

        // Act
        await _sut.ProcessFinishSelfDescriptionLpForApplication(data, company.Id, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, null)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.CreateDocument("SelfDescription_LegalPerson.json", A<byte[]>._, A<byte[]>._, MediaTypeId.JSON, DocumentTypeId.SELF_DESCRIPTION, A<Action<Document>?>._)).MustNotHaveHappened();
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(company.Id, null, A<Action<Company>>._)).MustNotHaveHappened();

        applicationChecklistEntry.Comment.Should().Be("test message");
        applicationChecklistEntry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.FAILED);

        _documents.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessFinishSelfDescriptionLp_FailedWithoutMessage_ThrowsConflictException()
    {
        // Arrange
        var company = _fixture.Build<Company>()
            .With(x => x.SelfDescriptionDocumentId, default(Guid?))
            .Create();
        var data = new SelfDescriptionResponseData(ApplicationId, SelfDescriptionStatus.Failed, null, null);

        // Act
        async Task Act() => await _sut.ProcessFinishSelfDescriptionLpForApplication(data, company.Id, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Please provide a messsage");
    }

    [Fact]
    public async Task RegisterSelfDescriptionAsync_WithNoApplication_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(ApplicationId))
            .Returns<(Guid, string?, string, IEnumerable<(UniqueIdentifierId, string)>)>(default);

        // Act
        async Task Act() => await _sut.StartSelfDescriptionRegistration(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"CompanyApplication {ApplicationId} is not in status SUBMITTED");
    }

    [Fact]
    public async Task RegisterSelfDescriptionAsync_WithBpnNotSet_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(ApplicationId))
            .Returns((CompanyId, null, CountryCode, Enumerable.Empty<(UniqueIdentifierId Id, string Value)>()));

        // Act
        async Task Act() => await _sut.StartSelfDescriptionRegistration(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"BusinessPartnerNumber (bpn) for CompanyApplications {ApplicationId} company {CompanyId} is empty");
    }

    #endregion

    #region ProcessFinishSelfDescriptionLp

    [Fact]
    public async Task ProcessFinishSelfDescriptionLpForConnector_ConfirmWithValidData_CompanyIsUpdated()
    {
        // Arrange
        var connector = new Connector(Guid.NewGuid(), "con-air", "de", "https://one-url.com");
        const string contentJson = "{\"@context\":[\"https://www.w3.org/2018/credentials/v1\",\"https://github.com/catenax-ng/tx-sd-factory/raw/clearing-house/src/main/resources/verifiablecredentials.jsonld/sd-document-v22.10.jsonld\",\"https://w3id.org/vc/status-list/2021/v1\"],\"type\":[\"VerifiableCredential\",\"LegalPerson\"],\"issuer\":\"did:sov:12345\",\"issuanceDate\":\"2023-02-18T23:03:16Z\",\"expirationDate\":\"2023-05-19T23:03:16Z\",\"credentialSubject\":{\"bpn\":\"BPNL000000000000\",\"registrationNumber\":[{\"type\":\"local\",\"value\":\"o12345678\"}],\"headquarterAddress\":{\"countryCode\":\"DE\"},\"type\":\"LegalPerson\",\"legalAddress\":{\"countryCode\":\"DE\"},\"id\":\"did:sov:12345\"},\"credentialStatus\":{\"id\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\",\"type\":\"StatusList2021Entry\",\"statusPurpose\":\"revocation\",\"statusListIndex\":\"58\",\"statusListCredential\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\"},\"proof\":{\"type\":\"Ed25519Signature2018\",\"created\":\"2023-02-18T23:03:18Z\",\"proofPurpose\":\"assertionMethod\",\"verificationMethod\":\"did:sov:12345#key-1\",\"jws\":\"test\"}}";
        var data = new SelfDescriptionResponseData(connector.Id, SelfDescriptionStatus.Confirm, null, contentJson);
        SetupForProcessFinishForConnector(connector);

        // Act
        await _sut.ProcessFinishSelfDescriptionLpForConnector(data, CompanyUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument("SelfDescription_Connector.json", A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, DocumentTypeId.SELF_DESCRIPTION, A<Action<Document>?>._)).MustHaveHappenedOnceExactly();

        _documents.Should().HaveCount(1);
        var document = _documents.Single();
        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(connector.Id, null, A<Action<Connector>>._))
            .MustHaveHappenedOnceExactly();
        document.DocumentName.Should().Be("SelfDescription_Connector.json");
        connector.SelfDescriptionDocumentId.Should().Be(document.Id);
    }

    [Fact]
    public async Task ProcessFinishSelfDescriptionLpForConnector_ConfirmWitNoDocument_ThrowsConflictException()
    {
        // Arrange
        var data = new SelfDescriptionResponseData(Guid.NewGuid(), SelfDescriptionStatus.Confirm, null, null);

        // Act
        async Task Act() => await _sut.ProcessFinishSelfDescriptionLpForConnector(data, CompanyUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Please provide a selfDescriptionDocument");
    }

    [Fact]
    public async Task ProcessFinishSelfDescriptionLpForConnector_FailedWithValidData_ConnectorIsUpdated()
    {
        // Arrange
        var connector = new Connector(Guid.NewGuid(), "con-air", "de", "https://one-url.com");
        var data = new SelfDescriptionResponseData(connector.Id, SelfDescriptionStatus.Failed, "test message", null);
        SetupForProcessFinishForConnector(connector);

        // Act
        await _sut.ProcessFinishSelfDescriptionLpForConnector(data, CompanyUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(connector.Id, null, A<Action<Connector>>._))
            .MustHaveHappenedOnceExactly();
        connector.SelfDescriptionMessage.Should().Be("test message");
        _documents.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessFinishSelfDescriptionLpForConnector_FailedWithoutMessage_ThrowsConflictException()
    {
        // Arrange
        var data = new SelfDescriptionResponseData(ApplicationId, SelfDescriptionStatus.Failed, null, null);

        // Act
        async Task Act() => await _sut.ProcessFinishSelfDescriptionLpForConnector(data, CompanyUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Please provide a messsage");
    }

    #endregion

    #region GetSdUniqueIdentifierValue

    [Theory]
    [InlineData(UniqueIdentifierId.COMMERCIAL_REG_NUMBER, "local")]
    [InlineData(UniqueIdentifierId.VAT_ID, "vatID")]
    [InlineData(UniqueIdentifierId.LEI_CODE, "leiCode")]
    [InlineData(UniqueIdentifierId.VIES, "EUID")]
    [InlineData(UniqueIdentifierId.EORI, "EORI")]
    public void GetSdUniqueIdentifierValue_WithIdentifier_ReturnsExpected(UniqueIdentifierId uiId, string expectedValue)
    {
        // Act
        var result = uiId.GetSdUniqueIdentifierValue();

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void GetSdUniqueIdentifierValue_WithNotYetConfiguredValue_ThrowsArgumentOutOfRangeException()
    {
        // Assert
        const UniqueIdentifierId id = default;

        // Act
        string Act() => id.GetSdUniqueIdentifierValue();

        // Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>((Func<string>)Act);
        ex.ParamName.Should().Be("uniqueIdentifierId");
    }

    #endregion

    #region Setup

    private void SetupForProcessFinish(Company company, ApplicationChecklistEntry applicationChecklistEntry)
    {
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(CompanyId, null, A<Action<Company>>._))
            .Invokes((Guid _, Action<Company>? initialize, Action<Company> modify) =>
            {
                initialize?.Invoke(company);
                modify.Invoke(company);
            });
        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(ApplicationId,
                ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP,
                new[] { ApplicationChecklistEntryStatusId.IN_PROGRESS },
                ProcessStepTypeId.FINISH_SELF_DESCRIPTION_LP,
                null,
                new[] { ProcessStepTypeId.START_SELF_DESCRIPTION_LP }))
            .Returns(new IApplicationChecklistService.ManualChecklistProcessStepData(ApplicationId, _process, Guid.NewGuid(),
                ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP,
                ImmutableDictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>.Empty,
                Enumerable.Empty<ProcessStep>()));
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(
                A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._,
                A<IEnumerable<ProcessStepTypeId>>._))
            .Invokes((IApplicationChecklistService.ManualChecklistProcessStepData _, Action<ApplicationChecklistEntry> initailApplicationChecklistEntry,
                Action<ApplicationChecklistEntry> modifyApplicationChecklistEntry, IEnumerable<ProcessStepTypeId> _) =>
            {
                applicationChecklistEntry.DateLastChanged = DateTimeOffset.UtcNow;
                modifyApplicationChecklistEntry.Invoke(applicationChecklistEntry);
            });
        var documentId = Guid.NewGuid();
        A.CallTo(() =>
                _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, A<DocumentTypeId>._,
                    A<Action<Document>?>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentTypeId, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, mediaTypeId, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentTypeId);
                setupOptionalFields?.Invoke(document);
                _documents.Add(document);
            })
            .Returns(new Document(documentId, null!, null!, null!, default, default, default, default));

        A.CallTo(() =>
                _companyRepository.AttachAndModifyCompany(A<Guid>._, null, A<Action<Company>>._))
            .Invokes((Guid _, Action<Company>? init, Action<Company> modify) =>
            {
                init?.Invoke(company);
                modify.Invoke(company);
            });
    }

    private void SetupForProcessFinishForConnector(Connector connector)
    {
        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(connector.Id, null, A<Action<Connector>>._))
            .Invokes((Guid _, Action<Connector>? initialize, Action<Connector> modify) =>
            {
                initialize?.Invoke(connector);
                modify.Invoke(connector);
            });
        var documentId = Guid.NewGuid();
        A.CallTo(() =>
                _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, A<DocumentTypeId>._, A<Action<Document>?>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentTypeId, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, mediaTypeId, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentTypeId);
                setupOptionalFields?.Invoke(document);
                _documents.Add(document);
            })
            .Returns(new Document(documentId, null!, null!, null!, default, default, default, default));
    }

    #endregion
}
