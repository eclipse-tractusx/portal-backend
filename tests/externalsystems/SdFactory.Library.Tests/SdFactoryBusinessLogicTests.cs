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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Tests;

public class SdFactoryBusinessLogicTests
{
    #region Initialization

    private const string CountryCode = "DE";
    private const string Region = "NW";
    private const string LegalName = "Legal Participant Company Name";
    private const string Bpn = "BPNL000000000009";
    private static readonly Guid ApplicationId = new("ac1cf001-7fbc-1f2f-817f-bce058020001");
    private readonly Process _process;
    private static readonly Guid CompanyId = new("b4697623-dd87-410d-abb8-6d4f4d87ab58");

    private static readonly IEnumerable<(UniqueIdentifierId Id, string Value)> UniqueIdentifiers = new List<(UniqueIdentifierId Id, string Value)>
    {
        new(UniqueIdentifierId.VAT_ID, "JUSTATEST")
    };

    private readonly IApplicationRepository _applicationRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IConnectorsRepository _connectorsRepository;
    private readonly IProcessStepRepository<ProcessTypeId, ProcessStepTypeId> _portalProcessStepRepository;
    private readonly ISdFactoryService _service;
    private readonly ICollection<Document> _documents;

    private readonly SdFactoryBusinessLogic _sut;
    private readonly IFixture _fixture;
    private readonly IApplicationChecklistService _checklistService;
    private readonly IOptions<SdFactorySettings> _options;
    private readonly IPortalRepositories _portalRepositories;

    public SdFactoryBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.ConfigureFixture();

        _process = _fixture.Create<Process>();

        _documents = new HashSet<Document>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _connectorsRepository = A.Fake<IConnectorsRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _portalProcessStepRepository = A.Fake<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>();
        _checklistService = A.Fake<IApplicationChecklistService>();
        _service = A.Fake<ISdFactoryService>();

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConnectorsRepository>()).Returns(_connectorsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>()).Returns(_portalProcessStepRepository);

        _options = Options.Create(new SdFactorySettings
        {
            SdFactoryUrl = "https://www.api.sdfactory.com",
            SdFactoryIssuerBpn = "BPNL00000003CRHK"
        });
        _sut = new SdFactoryBusinessLogic(_service, _portalRepositories, _checklistService, _options);
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
        await _sut.RegisterConnectorAsync(id, url, Bpn, CancellationToken.None);

        // Assert
        A.CallTo(() => _service.RegisterConnectorAsync(id, url, Bpn, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region StartSelfDescriptionRegistration

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task StartSelfDescriptionRegistration_WithValidData_CompanyIsUpdated(bool clearinghouseConnectDisabled)
    {
        // Arrange
        var checklist = ImmutableDictionary.CreateRange<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
            [
                new(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
                new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE),
                new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE)
            ]);
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        var entry = new ApplicationChecklistEntry(Guid.NewGuid(), ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(ApplicationId))
            .Returns((CompanyId, LegalName, Bpn, CountryCode, Region, UniqueIdentifiers));
        var sut = new SdFactoryBusinessLogic(_service, _portalRepositories, _checklistService, Options.Create(new SdFactorySettings
        {
            SdFactoryUrl = "https://www.api.sdfactory.com",
            SdFactoryIssuerBpn = "BPNL00000003CRHK",
            ClearinghouseConnectDisabled = clearinghouseConnectDisabled
        }));

        // Act
        var result = await sut.StartSelfDescriptionRegistration(context, CancellationToken.None);

        // Assert
        A.CallTo(() => _service.RegisterSelfDescriptionAsync(ApplicationId, LegalName, UniqueIdentifiers, CountryCode, Region, Bpn, A<CancellationToken>._))
            .MustHaveHappened(clearinghouseConnectDisabled ? 0 : 1, Times.Exactly);
        result.Should().NotBeNull();
        result.ModifyChecklistEntry.Should().NotBeNull();
        result.ModifyChecklistEntry!.Invoke(entry);
        result.ProcessMessage.Should().Be(clearinghouseConnectDisabled ? "Self description was skipped due to clearinghouse trigger is disabled" : null);
        entry.ApplicationChecklistEntryStatusId.Should().Be(clearinghouseConnectDisabled ? ApplicationChecklistEntryStatusId.SKIPPED : ApplicationChecklistEntryStatusId.IN_PROGRESS);
        result.ScheduleStepTypeIds.Should().ContainSingle().And.Match(x => clearinghouseConnectDisabled ? x.Single() == ProcessStepTypeId.ASSIGN_INITIAL_ROLES : x.Single() == ProcessStepTypeId.AWAIT_SELF_DESCRIPTION_LP_RESPONSE);
        result.SkipStepTypeIds.Should().BeNull();
        result.Modified.Should().BeTrue();
        result.StepStatusId.Should().Be(clearinghouseConnectDisabled ? ProcessStepStatusId.SKIPPED : ProcessStepStatusId.DONE);
    }

    [Fact]
    public async Task StartSelfDescriptionRegistration_WithNoApplication_ThrowsConflictException()
    {
        // Arrange
        var checklist = ImmutableDictionary.CreateRange<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
            [
                new(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
                new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE),
                new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE)
            ]);
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(ApplicationId))
            .Returns<(Guid, string, string?, string?, string?, IEnumerable<(UniqueIdentifierId, string)>)>(default);

        // Act
        async Task Act() => await _sut.StartSelfDescriptionRegistration(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"CompanyApplication {ApplicationId} is not in status SUBMITTED");
        A.CallTo(() => _service.RegisterSelfDescriptionAsync(ApplicationId, LegalName, UniqueIdentifiers, CountryCode, Region, Bpn, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task StartSelfDescriptionRegistration_WithBpnNotSet_ThrowsConflictException()
    {
        // Arrange
        var checklist = ImmutableDictionary.CreateRange<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
            [
                new(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
                new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE),
                new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE)
            ]);
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(ApplicationId))
            .Returns((CompanyId, LegalName, null, CountryCode, Region, Enumerable.Empty<(UniqueIdentifierId Id, string Value)>()));

        // Act
        async Task Act() => await _sut.StartSelfDescriptionRegistration(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"BusinessPartnerNumber (bpn) for CompanyApplications {ApplicationId} company {CompanyId} is empty");
        A.CallTo(() => _service.RegisterSelfDescriptionAsync(ApplicationId, LegalName, UniqueIdentifiers, CountryCode, Region, Bpn, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(CountryCode, null)]
    [InlineData(null, Region)]
    [InlineData(null, null)]
    public async Task StartSelfDescriptionRegistration_WithCountryOrRegionNotSet_ThrowsConflictException(string? countryCode, string? region)
    {
        // Arrange
        var checklist = ImmutableDictionary.CreateRange<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
            [
                new(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
                new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE),
                new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE),
            ]);
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(ApplicationId))
            .Returns((CompanyId, LegalName, Bpn, countryCode, region, Enumerable.Empty<(UniqueIdentifierId Id, string Value)>()));

        // Act
        async Task Act() => await _sut.StartSelfDescriptionRegistration(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"CountryCode or Region for CompanyApplications {ApplicationId} and Company {CompanyId} is empty. Expected value: DE-NW and Current value: {countryCode}-{region}");
        A.CallTo(() => _service.RegisterSelfDescriptionAsync(ApplicationId, LegalName, UniqueIdentifiers, CountryCode, Region, Bpn, A<CancellationToken>._))
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
        const string contentJson = "{\"@context\":[\"https://www.w3.org/2018/credentials/v1\",\"https://github.com/eclipse-tractusx/sd-factory/raw/clearing-house/src/main/resources/verifiablecredentials.jsonld/sd-document-v22.10.jsonld\",\"https://w3id.org/vc/status-list/2021/v1\"],\"type\":[\"VerifiableCredential\",\"LegalPerson\"],\"issuer\":\"did:sov:12345\",\"issuanceDate\":\"2023-02-18T23:03:16Z\",\"expirationDate\":\"2023-05-19T23:03:16Z\",\"credentialSubject\":{\"bpn\":\"BPNL000000000000\",\"registrationNumber\":[{\"type\":\"local\",\"value\":\"o12345678\"}],\"headquarterAddress\":{\"countryCode\":\"DE\"},\"type\":\"LegalPerson\",\"legalAddress\":{\"countryCode\":\"DE\"},\"id\":\"did:sov:12345\"},\"credentialStatus\":{\"id\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\",\"type\":\"StatusList2021Entry\",\"statusPurpose\":\"revocation\",\"statusListIndex\":\"58\",\"statusListCredential\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\"},\"proof\":{\"type\":\"Ed25519Signature2018\",\"created\":\"2023-02-18T23:03:18Z\",\"proofPurpose\":\"assertionMethod\",\"verificationMethod\":\"did:sov:12345#key-1\",\"jws\":\"test\"}}";
        var data = new SelfDescriptionResponseData(ApplicationId, SelfDescriptionStatus.Confirm, null, contentJson);
        SetupForProcessFinish(company, applicationChecklistEntry);

        // Act
        await _sut.ProcessFinishSelfDescriptionLpForApplication(data, company.Id, CancellationToken.None);

        // Assert
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count(y => y == ProcessStepTypeId.ASSIGN_INITIAL_ROLES) == 1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.CreateDocument($"SelfDescription_LegalPerson.json", A<byte[]>._, A<byte[]>._, MediaTypeId.JSON, DocumentTypeId.SELF_DESCRIPTION, A<long>._, A<Action<Document>?>._)).MustHaveHappenedOnceExactly();
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
        async Task Act() => await _sut.ProcessFinishSelfDescriptionLpForApplication(data, company.Id, CancellationToken.None);

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
        await _sut.ProcessFinishSelfDescriptionLpForApplication(data, company.Id, CancellationToken.None);

        // Assert
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, null)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.CreateDocument("SelfDescription_LegalPerson.json", A<byte[]>._, A<byte[]>._, MediaTypeId.JSON, DocumentTypeId.SELF_DESCRIPTION, A<long>._, A<Action<Document>?>._)).MustNotHaveHappened();
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
        async Task Act() => await _sut.ProcessFinishSelfDescriptionLpForApplication(data, company.Id, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Please provide a message");
    }

    [Fact]
    public async Task RegisterSelfDescriptionAsync_WithNoApplication_ThrowsConflictException()
    {
        // Arrange
        var checklist = ImmutableDictionary.CreateRange<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
            [
                new(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
                new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE),
                new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE)
            ]);
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(ApplicationId))
            .Returns<(Guid, string, string?, string?, string?, IEnumerable<(UniqueIdentifierId, string)>)>(default);

        // Act
        async Task Act() => await _sut.StartSelfDescriptionRegistration(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"CompanyApplication {ApplicationId} is not in status SUBMITTED");
    }

    [Fact]
    public async Task RegisterSelfDescriptionAsync_WithBpnNotSet_ThrowsConflictException()
    {
        // Arrange
        var checklist = ImmutableDictionary.CreateRange<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
            [
                new(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
                new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE),
                new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE)
            ]);
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(ApplicationId))
            .Returns((CompanyId, LegalName, null, CountryCode, Region, Enumerable.Empty<(UniqueIdentifierId Id, string Value)>()));

        // Act
        async Task Act() => await _sut.StartSelfDescriptionRegistration(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"BusinessPartnerNumber (bpn) for CompanyApplications {ApplicationId} company {CompanyId} is empty");
    }

    #endregion

    #region ProcessFinishSelfDescriptionLp

    [Fact]
    public async Task ProcessFinishSelfDescriptionLpForConnector_ConfirmWithValidDataAndWithoutProcess_CompanyIsUpdated()
    {
        // Arrange
        var connector = new Connector(Guid.NewGuid(), "con-air", "de", "https://one-url.com");
        const string contentJson = "{\"@context\":[\"https://www.w3.org/2018/credentials/v1\",\"https://github.com/eclipse-tractusx/sd-factory/raw/clearing-house/src/main/resources/verifiablecredentials.jsonld/sd-document-v22.10.jsonld\",\"https://w3id.org/vc/status-list/2021/v1\"],\"type\":[\"VerifiableCredential\",\"LegalPerson\"],\"issuer\":\"did:sov:12345\",\"issuanceDate\":\"2023-02-18T23:03:16Z\",\"expirationDate\":\"2023-05-19T23:03:16Z\",\"credentialSubject\":{\"bpn\":\"BPNL000000000000\",\"registrationNumber\":[{\"type\":\"local\",\"value\":\"o12345678\"}],\"headquarterAddress\":{\"countryCode\":\"DE\"},\"type\":\"LegalPerson\",\"legalAddress\":{\"countryCode\":\"DE\"},\"id\":\"did:sov:12345\"},\"credentialStatus\":{\"id\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\",\"type\":\"StatusList2021Entry\",\"statusPurpose\":\"revocation\",\"statusListIndex\":\"58\",\"statusListCredential\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\"},\"proof\":{\"type\":\"Ed25519Signature2018\",\"created\":\"2023-02-18T23:03:18Z\",\"proofPurpose\":\"assertionMethod\",\"verificationMethod\":\"did:sov:12345#key-1\",\"jws\":\"test\"}}";
        var data = new SelfDescriptionResponseData(connector.Id, SelfDescriptionStatus.Confirm, null, contentJson);
        var processId = Guid.NewGuid();
        var processStep = new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), ProcessStepTypeId.AWAIT_SELF_DESCRIPTION_CONNECTOR_RESPONSE, ProcessStepStatusId.TODO, processId, DateTimeOffset.UtcNow);
        var processSteps = new List<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>();
        SetupForProcessFinishForConnector(processId, connector, processStep, processSteps);
        A.CallTo(() => _companyRepository.GetProcessDataForCompanyIdId(A<Guid>._))
            .Returns<VerifyProcessData<ProcessTypeId, ProcessStepTypeId>?>(null);

        // Act
        await _sut.ProcessFinishSelfDescriptionLpForConnector(data, CancellationToken.None);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument("SelfDescription_Connector.json", A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, DocumentTypeId.SELF_DESCRIPTION, A<long>._, A<Action<Document>?>._)).MustHaveHappenedOnceExactly();

        _documents.Should().HaveCount(1);
        var document = _documents.Single();
        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(connector.Id, null, A<Action<Connector>>._))
            .MustHaveHappenedOnceExactly();
        document.DocumentName.Should().Be("SelfDescription_Connector.json");
        connector.SelfDescriptionDocumentId.Should().Be(document.Id);
    }

    [Fact]
    public async Task ProcessFinishSelfDescriptionLpForConnector_ConfirmWithValidData_CompanyIsUpdated()
    {
        // Arrange
        var connector = new Connector(Guid.NewGuid(), "con-air", "de", "https://one-url.com");
        const string contentJson = "{\"@context\":[\"https://www.w3.org/2018/credentials/v1\",\"https://github.com/eclipse-tractusx/sd-factory/raw/clearing-house/src/main/resources/verifiablecredentials.jsonld/sd-document-v22.10.jsonld\",\"https://w3id.org/vc/status-list/2021/v1\"],\"type\":[\"VerifiableCredential\",\"LegalPerson\"],\"issuer\":\"did:sov:12345\",\"issuanceDate\":\"2023-02-18T23:03:16Z\",\"expirationDate\":\"2023-05-19T23:03:16Z\",\"credentialSubject\":{\"bpn\":\"BPNL000000000000\",\"registrationNumber\":[{\"type\":\"local\",\"value\":\"o12345678\"}],\"headquarterAddress\":{\"countryCode\":\"DE\"},\"type\":\"LegalPerson\",\"legalAddress\":{\"countryCode\":\"DE\"},\"id\":\"did:sov:12345\"},\"credentialStatus\":{\"id\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\",\"type\":\"StatusList2021Entry\",\"statusPurpose\":\"revocation\",\"statusListIndex\":\"58\",\"statusListCredential\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\"},\"proof\":{\"type\":\"Ed25519Signature2018\",\"created\":\"2023-02-18T23:03:18Z\",\"proofPurpose\":\"assertionMethod\",\"verificationMethod\":\"did:sov:12345#key-1\",\"jws\":\"test\"}}";
        var data = new SelfDescriptionResponseData(connector.Id, SelfDescriptionStatus.Confirm, null, contentJson);
        var processId = Guid.NewGuid();
        var processStep = new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), ProcessStepTypeId.AWAIT_SELF_DESCRIPTION_CONNECTOR_RESPONSE, ProcessStepStatusId.TODO, processId, DateTimeOffset.UtcNow);
        var processSteps = new List<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>();
        SetupForProcessFinishForConnector(processId, connector, processStep, processSteps);

        // Act
        await _sut.ProcessFinishSelfDescriptionLpForConnector(data, CancellationToken.None);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument("SelfDescription_Connector.json", A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, DocumentTypeId.SELF_DESCRIPTION, A<long>._, A<Action<Document>?>._)).MustHaveHappenedOnceExactly();

        _documents.Should().HaveCount(1);
        var document = _documents.Single();
        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(connector.Id, null, A<Action<Connector>>._))
            .MustHaveHappenedOnceExactly();
        document.DocumentName.Should().Be("SelfDescription_Connector.json");
        connector.SelfDescriptionDocumentId.Should().Be(document.Id);
        processStep.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        processSteps.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessFinishSelfDescriptionLpForConnector_ConfirmWitNoDocument_ThrowsConflictException()
    {
        // Arrange
        var data = new SelfDescriptionResponseData(Guid.NewGuid(), SelfDescriptionStatus.Confirm, null, null);

        // Act
        async Task Act() => await _sut.ProcessFinishSelfDescriptionLpForConnector(data, CancellationToken.None);

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
        var processId = Guid.NewGuid();
        var processStep = new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), ProcessStepTypeId.AWAIT_SELF_DESCRIPTION_CONNECTOR_RESPONSE, ProcessStepStatusId.TODO, processId, DateTimeOffset.UtcNow);
        var processSteps = new List<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>();
        SetupForProcessFinishForConnector(processId, connector, processStep, processSteps);

        // Act
        await _sut.ProcessFinishSelfDescriptionLpForConnector(data, CancellationToken.None);

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
        async Task Act() => await _sut.ProcessFinishSelfDescriptionLpForConnector(data, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Please provide a message");
    }

    #endregion

    #region ProcessFinishSelfDescriptionLpForCompany

    [Fact]
    public async Task ProcessFinishSelfDescriptionLpForCompany_ConfirmWithValidDataAndNoProcess_CompanyIsUpdated()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var company = new Company(Guid.NewGuid(), "con-air", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow);
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, null, A<Action<Company>>._))
            .Invokes((Guid _, Action<Company>? _, Action<Company> modify) =>
            {
                modify(company);
            });
        A.CallTo(() =>
                _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, A<DocumentTypeId>._, A<long>._, A<Action<Document>?>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentTypeId, long documentSize, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, mediaTypeId, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentTypeId, documentSize);
                setupOptionalFields?.Invoke(document);
                _documents.Add(document);
            })
            .Returns(new Document(documentId, null!, null!, null!, default, default, default, default, default));
        A.CallTo(() => _companyRepository.GetProcessDataForCompanyIdId(company.Id))
            .Returns<VerifyProcessData<ProcessTypeId, ProcessStepTypeId>?>(null);

        const string contentJson = "{\"@context\":[\"https://www.w3.org/2018/credentials/v1\",\"https://github.com/eclipse-tractusx/sd-factory/raw/clearing-house/src/main/resources/verifiablecredentials.jsonld/sd-document-v22.10.jsonld\",\"https://w3id.org/vc/status-list/2021/v1\"],\"type\":[\"VerifiableCredential\",\"LegalPerson\"],\"issuer\":\"did:sov:12345\",\"issuanceDate\":\"2023-02-18T23:03:16Z\",\"expirationDate\":\"2023-05-19T23:03:16Z\",\"credentialSubject\":{\"bpn\":\"BPNL000000000000\",\"registrationNumber\":[{\"type\":\"local\",\"value\":\"o12345678\"}],\"headquarterAddress\":{\"countryCode\":\"DE\"},\"type\":\"LegalPerson\",\"legalAddress\":{\"countryCode\":\"DE\"},\"id\":\"did:sov:12345\"},\"credentialStatus\":{\"id\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\",\"type\":\"StatusList2021Entry\",\"statusPurpose\":\"revocation\",\"statusListIndex\":\"58\",\"statusListCredential\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\"},\"proof\":{\"type\":\"Ed25519Signature2018\",\"created\":\"2023-02-18T23:03:18Z\",\"proofPurpose\":\"assertionMethod\",\"verificationMethod\":\"did:sov:12345#key-1\",\"jws\":\"test\"}}";
        var data = new SelfDescriptionResponseData(company.Id, SelfDescriptionStatus.Confirm, null, contentJson);

        // Act
        await _sut.ProcessFinishSelfDescriptionLpForCompany(data, CancellationToken.None);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument("SelfDescription_LegalPerson.json", A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, DocumentTypeId.SELF_DESCRIPTION, A<long>._, A<Action<Document>?>._)).MustHaveHappenedOnceExactly();

        _documents.Should().HaveCount(1);
        var document = _documents.Single();
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(company.Id, null, A<Action<Company>>._))
            .MustHaveHappenedOnceExactly();
        document.DocumentName.Should().Be("SelfDescription_LegalPerson.json");
        company.SelfDescriptionDocumentId.Should().Be(document.Id);
    }

    [Fact]
    public async Task ProcessFinishSelfDescriptionLpForCompany_DeclineWithoutProcess_DoesNothing()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var company = new Company(Guid.NewGuid(), "con-air", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow);
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, null, A<Action<Company>>._))
            .Invokes((Guid _, Action<Company>? _, Action<Company> modify) =>
            {
                modify(company);
            });
        A.CallTo(() =>
                _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, A<DocumentTypeId>._, A<long>._, A<Action<Document>?>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentTypeId, long documentSize, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, mediaTypeId, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentTypeId, documentSize);
                setupOptionalFields?.Invoke(document);
                _documents.Add(document);
            })
            .Returns(new Document(documentId, null!, null!, null!, default, default, default, default, default));
        A.CallTo(() => _companyRepository.GetProcessDataForCompanyIdId(company.Id))
            .Returns<VerifyProcessData<ProcessTypeId, ProcessStepTypeId>?>(null);

        const string contentJson = "{\"@context\":[\"https://www.w3.org/2018/credentials/v1\",\"https://github.com/eclipse-tractusx/sd-factory/raw/clearing-house/src/main/resources/verifiablecredentials.jsonld/sd-document-v22.10.jsonld\",\"https://w3id.org/vc/status-list/2021/v1\"],\"type\":[\"VerifiableCredential\",\"LegalPerson\"],\"issuer\":\"did:sov:12345\",\"issuanceDate\":\"2023-02-18T23:03:16Z\",\"expirationDate\":\"2023-05-19T23:03:16Z\",\"credentialSubject\":{\"bpn\":\"BPNL000000000000\",\"registrationNumber\":[{\"type\":\"local\",\"value\":\"o12345678\"}],\"headquarterAddress\":{\"countryCode\":\"DE\"},\"type\":\"LegalPerson\",\"legalAddress\":{\"countryCode\":\"DE\"},\"id\":\"did:sov:12345\"},\"credentialStatus\":{\"id\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\",\"type\":\"StatusList2021Entry\",\"statusPurpose\":\"revocation\",\"statusListIndex\":\"58\",\"statusListCredential\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\"},\"proof\":{\"type\":\"Ed25519Signature2018\",\"created\":\"2023-02-18T23:03:18Z\",\"proofPurpose\":\"assertionMethod\",\"verificationMethod\":\"did:sov:12345#key-1\",\"jws\":\"test\"}}";
        var data = new SelfDescriptionResponseData(company.Id, SelfDescriptionStatus.Failed, null, contentJson);

        // Act
        await _sut.ProcessFinishSelfDescriptionLpForCompany(data, CancellationToken.None);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, DocumentTypeId.SELF_DESCRIPTION, A<long>._, A<Action<Document>?>._))
            .MustNotHaveHappened();
        _documents.Should().BeEmpty();
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(company.Id, null, A<Action<Company>>._))
            .MustNotHaveHappened();
        company.SelfDescriptionDocumentId.Should().BeNull();
    }

    [Fact]
    public async Task ProcessFinishSelfDescriptionLpForCompany_ConfirmWithValidDataAndExistingProcess_CompanyIsUpdatedAndProcessStepUpdated()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var company = new Company(Guid.NewGuid(), "con-air", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow);
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, null, A<Action<Company>>._))
            .Invokes((Guid _, Action<Company>? _, Action<Company> modify) =>
            {
                modify(company);
            });
        A.CallTo(() =>
                _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, A<DocumentTypeId>._, A<long>._, A<Action<Document>?>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentTypeId, long documentSize, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, mediaTypeId, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentTypeId, documentSize);
                setupOptionalFields?.Invoke(document);
                _documents.Add(document);
            })
            .Returns(new Document(documentId, null!, null!, null!, default, default, default, default, default));
        var process = new Process(Guid.NewGuid(), ProcessTypeId.SELF_DESCRIPTION_CREATION, Guid.NewGuid());
        var processStep = new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), ProcessStepTypeId.AWAIT_SELF_DESCRIPTION_COMPANY_RESPONSE, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow);
        var processSteps = new List<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>();

        A.CallTo(() => _companyRepository.GetProcessDataForCompanyIdId(A<Guid>._))
            .Returns(new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(process, Enumerable.Repeat(processStep, 1)));
        A.CallTo(() => _portalProcessStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<ValueTuple<Guid, Action<IProcessStep<ProcessStepTypeId>>?, Action<IProcessStep<ProcessStepTypeId>>>>>._))
            .Invokes((IEnumerable<(Guid ProcessStepId, Action<IProcessStep<ProcessStepTypeId>>? Initialize, Action<IProcessStep<ProcessStepTypeId>> Modify)> processStepIdsInitializeModifyData) =>
            {
                var modify = processStepIdsInitializeModifyData.SingleOrDefault(x => processStep.Id == x.ProcessStepId);
                if (modify == default)
                    return;

                modify.Initialize?.Invoke(processStep);
                modify.Modify.Invoke(processStep);
            });
        A.CallTo(() => _portalProcessStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId, ProcessStepStatusId, Guid)>>._))
            .Invokes((IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> ps) =>
            {
                processSteps.AddRange(ps.Select(x => new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), x.ProcessStepTypeId, x.ProcessStepStatusId, x.ProcessId, DateTimeOffset.UtcNow)));
            });

        const string contentJson = "{\"@context\":[\"https://www.w3.org/2018/credentials/v1\",\"https://github.com/eclipse-tractusx/sd-factory/raw/clearing-house/src/main/resources/verifiablecredentials.jsonld/sd-document-v22.10.jsonld\",\"https://w3id.org/vc/status-list/2021/v1\"],\"type\":[\"VerifiableCredential\",\"LegalPerson\"],\"issuer\":\"did:sov:12345\",\"issuanceDate\":\"2023-02-18T23:03:16Z\",\"expirationDate\":\"2023-05-19T23:03:16Z\",\"credentialSubject\":{\"bpn\":\"BPNL000000000000\",\"registrationNumber\":[{\"type\":\"local\",\"value\":\"o12345678\"}],\"headquarterAddress\":{\"countryCode\":\"DE\"},\"type\":\"LegalPerson\",\"legalAddress\":{\"countryCode\":\"DE\"},\"id\":\"did:sov:12345\"},\"credentialStatus\":{\"id\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\",\"type\":\"StatusList2021Entry\",\"statusPurpose\":\"revocation\",\"statusListIndex\":\"58\",\"statusListCredential\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\"},\"proof\":{\"type\":\"Ed25519Signature2018\",\"created\":\"2023-02-18T23:03:18Z\",\"proofPurpose\":\"assertionMethod\",\"verificationMethod\":\"did:sov:12345#key-1\",\"jws\":\"test\"}}";
        var data = new SelfDescriptionResponseData(company.Id, SelfDescriptionStatus.Confirm, null, contentJson);

        // Act
        await _sut.ProcessFinishSelfDescriptionLpForCompany(data, CancellationToken.None);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument("SelfDescription_LegalPerson.json", A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, DocumentTypeId.SELF_DESCRIPTION, A<long>._, A<Action<Document>?>._)).MustHaveHappenedOnceExactly();

        _documents.Should().HaveCount(1);
        var document = _documents.Single();
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(company.Id, null, A<Action<Company>>._))
            .MustHaveHappenedOnceExactly();
        processStep.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        processSteps.Should().BeEmpty();
        document.DocumentName.Should().Be("SelfDescription_LegalPerson.json");
        company.SelfDescriptionDocumentId.Should().Be(document.Id);
    }

    [Fact]
    public async Task ProcessFinishSelfDescriptionLpForCompany_DeclineWithProcess_CreatesRetrigger()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var company = new Company(Guid.NewGuid(), "con-air", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow);
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, null, A<Action<Company>>._))
            .Invokes((Guid _, Action<Company>? _, Action<Company> modify) =>
            {
                modify(company);
            });
        A.CallTo(() =>
                _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, A<DocumentTypeId>._, A<long>._, A<Action<Document>?>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentTypeId, long documentSize, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, mediaTypeId, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentTypeId, documentSize);
                setupOptionalFields?.Invoke(document);
                _documents.Add(document);
            })
            .Returns(new Document(documentId, null!, null!, null!, default, default, default, default, default));
        var process = new Process(Guid.NewGuid(), ProcessTypeId.SELF_DESCRIPTION_CREATION, Guid.NewGuid());
        var processStep = new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), ProcessStepTypeId.AWAIT_SELF_DESCRIPTION_COMPANY_RESPONSE, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow);
        var processSteps = new List<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>();

        A.CallTo(() => _companyRepository.GetProcessDataForCompanyIdId(A<Guid>._))
            .Returns(new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(process, Enumerable.Repeat(processStep, 1)));
        A.CallTo(() => _portalProcessStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<ValueTuple<Guid, Action<IProcessStep<ProcessStepTypeId>>?, Action<IProcessStep<ProcessStepTypeId>>>>>._))
            .Invokes((IEnumerable<(Guid ProcessStepId, Action<IProcessStep<ProcessStepTypeId>>? Initialize, Action<IProcessStep<ProcessStepTypeId>> Modify)> processStepIdsInitializeModifyData) =>
            {
                var modify = processStepIdsInitializeModifyData.SingleOrDefault(x => processStep.Id == x.ProcessStepId);
                if (modify == default)
                    return;

                modify.Initialize?.Invoke(processStep);
                modify.Modify.Invoke(processStep);
            });
        A.CallTo(() => _portalProcessStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId, ProcessStepStatusId, Guid)>>._))
            .Invokes((IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> ps) =>
            {
                processSteps.AddRange(ps.Select(x => new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), x.ProcessStepTypeId, x.ProcessStepStatusId, x.ProcessId, DateTimeOffset.UtcNow)));
            });

        const string contentJson = "{\"@context\":[\"https://www.w3.org/2018/credentials/v1\",\"https://github.com/eclipse-tractusx/sd-factory/raw/clearing-house/src/main/resources/verifiablecredentials.jsonld/sd-document-v22.10.jsonld\",\"https://w3id.org/vc/status-list/2021/v1\"],\"type\":[\"VerifiableCredential\",\"LegalPerson\"],\"issuer\":\"did:sov:12345\",\"issuanceDate\":\"2023-02-18T23:03:16Z\",\"expirationDate\":\"2023-05-19T23:03:16Z\",\"credentialSubject\":{\"bpn\":\"BPNL000000000000\",\"registrationNumber\":[{\"type\":\"local\",\"value\":\"o12345678\"}],\"headquarterAddress\":{\"countryCode\":\"DE\"},\"type\":\"LegalPerson\",\"legalAddress\":{\"countryCode\":\"DE\"},\"id\":\"did:sov:12345\"},\"credentialStatus\":{\"id\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\",\"type\":\"StatusList2021Entry\",\"statusPurpose\":\"revocation\",\"statusListIndex\":\"58\",\"statusListCredential\":\"https://managed-identity-wallets.int.demo.catena-x.net/api/credentials/status/123\"},\"proof\":{\"type\":\"Ed25519Signature2018\",\"created\":\"2023-02-18T23:03:18Z\",\"proofPurpose\":\"assertionMethod\",\"verificationMethod\":\"did:sov:12345#key-1\",\"jws\":\"test\"}}";
        var data = new SelfDescriptionResponseData(company.Id, SelfDescriptionStatus.Failed, null, contentJson);

        // Act
        await _sut.ProcessFinishSelfDescriptionLpForCompany(data, CancellationToken.None);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, DocumentTypeId.SELF_DESCRIPTION, A<long>._, A<Action<Document>?>._))
            .MustNotHaveHappened();
        _documents.Should().BeEmpty();
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(company.Id, null, A<Action<Company>>._))
            .MustNotHaveHappened();
        processStep.ProcessStepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        processSteps.Should().ContainSingle().And.Satisfy(ps =>
            ps.ProcessStepTypeId == ProcessStepTypeId.RETRIGGER_AWAIT_SELF_DESCRIPTION_COMPANY_RESPONSE &&
            ps.ProcessStepStatusId == ProcessStepStatusId.TODO);
        company.SelfDescriptionDocumentId.Should().BeNull();
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
                ProcessStepTypeId.AWAIT_SELF_DESCRIPTION_LP_RESPONSE,
                null,
                new[] { ProcessStepTypeId.START_SELF_DESCRIPTION_LP }))
            .Returns(new IApplicationChecklistService.ManualChecklistProcessStepData(ApplicationId, _process, Guid.NewGuid(),
                ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP,
                ImmutableDictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>.Empty,
                Enumerable.Empty<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>()));
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
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, A<DocumentTypeId>._, A<long>._, A<Action<Document>?>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentTypeId, long documentSize, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, mediaTypeId, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentTypeId, documentSize);
                setupOptionalFields?.Invoke(document);
                _documents.Add(document);
            })
            .Returns(new Document(documentId, null!, null!, null!, default, default, default, default, default));

        A.CallTo(() =>
                _companyRepository.AttachAndModifyCompany(A<Guid>._, null, A<Action<Company>>._))
            .Invokes((Guid _, Action<Company>? init, Action<Company> modify) =>
            {
                init?.Invoke(company);
                modify.Invoke(company);
            });
    }

    private void SetupForProcessFinishForConnector(Guid processId, Connector connector, ProcessStep<Process, ProcessTypeId, ProcessStepTypeId> processStep, List<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>> processSteps)
    {
        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(connector.Id, null, A<Action<Connector>>._))
            .Invokes((Guid _, Action<Connector>? initialize, Action<Connector> modify) =>
            {
                initialize?.Invoke(connector);
                modify.Invoke(connector);
            });
        var documentId = Guid.NewGuid();
        A.CallTo(() =>
                _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, A<DocumentTypeId>._, A<long>._, A<Action<Document>?>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentTypeId, long documentSize, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, mediaTypeId, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentTypeId, documentSize);
                setupOptionalFields?.Invoke(document);
                _documents.Add(document);
            })
            .Returns(new Document(documentId, null!, null!, null!, default, default, default, default, default));
        var process = new Process(processId, ProcessTypeId.SELF_DESCRIPTION_CREATION, Guid.NewGuid());

        A.CallTo(() => _connectorsRepository.GetProcessDataForConnectorId(A<Guid>._))
            .Returns(new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(process, Enumerable.Repeat(processStep, 1)));
        A.CallTo(() => _portalProcessStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<ValueTuple<Guid, Action<IProcessStep<ProcessStepTypeId>>?, Action<IProcessStep<ProcessStepTypeId>>>>>._))
            .Invokes((IEnumerable<(Guid ProcessStepId, Action<IProcessStep<ProcessStepTypeId>>? Initialize, Action<IProcessStep<ProcessStepTypeId>> Modify)> processStepIdsInitializeModifyData) =>
            {
                var modify = processStepIdsInitializeModifyData.SingleOrDefault(x => processStep.Id == x.ProcessStepId);
                if (modify == default)
                    return;

                modify.Initialize?.Invoke(processStep);
                modify.Modify.Invoke(processStep);
            });
        A.CallTo(() => _portalProcessStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId, ProcessStepStatusId, Guid)>>._))
            .Invokes((IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> ps) =>
            {
                processSteps.AddRange(ps.Select(x => new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), x.ProcessStepTypeId, x.ProcessStepStatusId, x.ProcessId, DateTimeOffset.UtcNow)));
            });
    }

    #endregion
}
