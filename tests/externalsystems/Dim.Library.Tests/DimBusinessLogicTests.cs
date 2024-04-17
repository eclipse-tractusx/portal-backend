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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Collections.Immutable;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Tests;

public class DimBusinessLogicTests
{
    #region Initialization

    private const string BPN = "BPNL0000000000XX";
    private static readonly Guid ApplicationId = Guid.NewGuid();

    private readonly IFixture _fixture;
    private readonly IApplicationRepository _applicationRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IDimService _dimService;
    private readonly IDimBusinessLogic _logic;
    private readonly IOptions<DimSettings> _options;
    private readonly IApplicationChecklistService _checklistService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly byte[] _encryptionKey;

    public DimBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.ConfigureFixture();

        var portalRepository = A.Fake<IPortalRepositories>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _dimService = A.Fake<IDimService>();
        _dateTimeProvider = A.Fake<IDateTimeProvider>();
        _checklistService = A.Fake<IApplicationChecklistService>();
        _encryptionKey = _fixture.CreateMany<byte>(32).ToArray();
        _options = Options.Create(new DimSettings
        {
            DidDocumentBaseLocation = "https://example.org/did",
            EncryptionConfigIndex = 1,
            EncryptionConfigs = new[] { new EncryptionModeConfig() { Index = 1, EncryptionKey = Convert.ToHexString(_encryptionKey), CipherMode = System.Security.Cryptography.CipherMode.CBC, PaddingMode = System.Security.Cryptography.PaddingMode.PKCS7 } },
        });

        A.CallTo(() => portalRepository.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => portalRepository.GetInstance<ICompanyRepository>()).Returns(_companyRepository);

        _logic = new DimBusinessLogic(portalRepository, _dimService, _checklistService, _dateTimeProvider, _options);
    }

    #endregion

    #region CreateDimWalletAsync

    [Fact]
    public async Task CreateDimWalletAsync_WithBpnProcessInTodo_DoesNothing()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _logic.CreateDimWalletAsync(context, CancellationToken.None);

        // Assert
        result.StepStatusId.Should().Be(ProcessStepStatusId.TODO);
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task CreateDimWalletAsync_WithRegistrationProcessInTodo_DoesNothing()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.TO_DO},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _logic.CreateDimWalletAsync(context, CancellationToken.None);

        // Assert
        result.StepStatusId.Should().Be(ProcessStepStatusId.TODO);
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task CreateDimWalletAsync_WithRegistrationProcessInFailed_SkipsProcess()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.FAILED },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _logic.CreateDimWalletAsync(context, CancellationToken.None);

        // Assert
        result.StepStatusId.Should().Be(ProcessStepStatusId.SKIPPED);
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task CreateDimWalletAsync_WithBpnProcessInFailed_SkipsProcess()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.FAILED },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _logic.CreateDimWalletAsync(context, CancellationToken.None);

        // Assert
        result.StepStatusId.Should().Be(ProcessStepStatusId.SKIPPED);
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task CreateDimWalletAsync_WithNotExistingApplication_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForCreateWalletAsync(ApplicationId))
            .Returns(new ValueTuple<Guid, string, string?>());
        async Task Act() => await _logic.CreateDimWalletAsync(context, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be($"CompanyApplication {ApplicationId} is not in status SUBMITTED");
    }

    [Fact]
    public async Task CreateDimWalletAsync_WithEmptyBpn_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        var companyId = Guid.NewGuid();
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForCreateWalletAsync(ApplicationId))
            .Returns(new ValueTuple<Guid, string, string?>(companyId, "Test Corp", null));
        async Task Act() => await _logic.CreateDimWalletAsync(context, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be($"BusinessPartnerNumber (bpn) for CompanyApplications {ApplicationId} company {companyId} is empty");
    }

    [Fact]
    public async Task CreateDimWalletAsync_WithValid_CallsExpected()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var companyName = "Test Corp";
        var company = new Company(companyId, companyName, CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow);
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForCreateWalletAsync(ApplicationId))
            .Returns(new ValueTuple<Guid, string, string?>(companyId, companyName, BPN));
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(companyId, A<Action<Company>>._, A<Action<Company>>._))
            .Invokes((Guid _, Action<Company>? initialize, Action<Company> modify) =>
            {
                initialize?.Invoke(company);
                modify(company);
            });

        // Act
        var result = await _logic.CreateDimWalletAsync(context, CancellationToken.None);

        // Assert
        A.CallTo(() => _dimService.CreateWalletAsync(companyName, BPN, A<string>.That.Contains($"{BPN}/did.json"), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(companyId, A<Action<Company>>._, A<Action<Company>>._))
            .MustHaveHappenedOnceExactly();
        company.DidDocumentLocation.Should().NotBeNull();
        result.StepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ScheduleStepTypeIds.Should().ContainSingle(x => x == ProcessStepTypeId.AWAIT_DIM_RESPONSE);
    }

    #endregion

    #region ProcessDimResponse

    [Fact]
    public async Task ProcessDimResponse_NoCompanyForBpn_ThrowsNotFoundException()
    {
        // Arrange
        var data = _fixture.Create<DimWalletData>();
        A.CallTo(() => _companyRepository.GetCompanyIdByBpn(BPN))
            .Returns(new ValueTuple<bool, Guid, IEnumerable<Guid>>(false, default, Enumerable.Empty<Guid>()));
        async Task Act() => await _logic.ProcessDimResponse(BPN, data, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        A.CallTo(() => _companyRepository.CreateWalletData(A<Guid>._, A<string>._, A<JsonDocument>._, A<string>._, A<byte[]>._, A<byte[]?>._, A<int>._, A<string>._))
            .MustNotHaveHappened();
        ex.Message.Should().Be($"No company found for bpn {BPN}");
    }

    [Fact]
    public async Task ProcessDimResponse_WithMultipleSubmittedApplications_ThrowsConflictException()
    {
        // Arrange
        var data = _fixture.Create<DimWalletData>();
        A.CallTo(() => _companyRepository.GetCompanyIdByBpn(BPN))
            .Returns(new ValueTuple<bool, Guid, IEnumerable<Guid>>(true, default, _fixture.CreateMany<Guid>(2)));
        async Task Act() => await _logic.ProcessDimResponse(BPN, data, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        A.CallTo(() => _companyRepository.CreateWalletData(A<Guid>._, A<string>._, A<JsonDocument>._, A<string>._, A<byte[]>._, A<byte[]?>._, A<int>._, A<string>._))
            .MustNotHaveHappened();
        ex.Message.Should().Be($"There must be exactly one company application in state {CompanyApplicationStatusId.SUBMITTED}");
    }

    [Fact]
    public async Task ProcessDimResponse_WithDidSchemaInvalid_CallsExpected()
    {
        // Arrange
        var context = new IApplicationChecklistService.ManualChecklistProcessStepData(ApplicationId, _fixture.Create<Process>(), Guid.NewGuid(), ApplicationChecklistEntryTypeId.IDENTITY_WALLET, new Dictionary<ApplicationChecklistEntryTypeId, ValueTuple<ApplicationChecklistEntryStatusId, string?>>()
        {
            { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, new(ApplicationChecklistEntryStatusId.TO_DO, string.Empty) }
        }.ToImmutableDictionary(), Enumerable.Empty<ProcessStep>());
        var didDocument = JsonDocument.Parse("{\n  \"@context\": [\n    \"https://www.w3.org/ns/did/v1\",\n    \"https://w3id.org/security/suites/ed25519-2020/v1\"\n  ],\n  \"id\": \"did:web:example.com:did:BPNL0000000000XX\",\n  \"verificationMethod\": [\n     {\n         \"id\": [\"did:web:example.com:did:BPNL0000000000XX#key-0\"],\n         \"type\": \"JsonWebKey2020\",\n         \"publicKeyJwk\": {\n            \"kty\": \"JsonWebKey2020\",\n            \"crv\": \"Ed25519\",\n            \"x\": \"3534354354353\"\n         }\n     }\n   ],\n   \"services\": [\n     {\n         \"id\": [\"did:web:example.com:did:BPNL0000000000XX#key-0\"],\n         \"type\": \"CredentialStore\",\n         \"serviceEndpoint\": \"test.org:123\"\n     }\n  ]\n}");
        var data = _fixture.Build<DimWalletData>()
            .With(x => x.DidDocument, didDocument)
            .With(x => x.Did, "did:web:test.com:BPNL0000000000XX")
            .Create();
        var companyId = Guid.NewGuid();
        A.CallTo(() => _companyRepository.GetCompanyIdByBpn(BPN))
            .Returns(new ValueTuple<bool, Guid, IEnumerable<Guid>>(true, companyId, Enumerable.Repeat(ApplicationId, 1)));
        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(ApplicationId, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, A<IEnumerable<ApplicationChecklistEntryStatusId>>._, ProcessStepTypeId.AWAIT_DIM_RESPONSE, A<IEnumerable<ApplicationChecklistEntryTypeId>?>._, A<IEnumerable<ProcessStepTypeId>?>._))
            .Returns(context);

        // Act
        await _logic.ProcessDimResponse(BPN, data, CancellationToken.None);

        // Assert
        A.CallTo(() => _companyRepository.CreateWalletData(A<Guid>._, A<string>._, A<JsonDocument>._, A<string>._, A<byte[]>._, A<byte[]?>._, A<int>._, A<string>._))
            .MustNotHaveHappened();
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(context, null, A<Action<ApplicationChecklistEntry>>._, null))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ProcessDimResponse_WithFailingSchemaValidation_CallsExpected()
    {
        // Arrange
        var context = new IApplicationChecklistService.ManualChecklistProcessStepData(ApplicationId, _fixture.Create<Process>(), Guid.NewGuid(), ApplicationChecklistEntryTypeId.IDENTITY_WALLET, new Dictionary<ApplicationChecklistEntryTypeId, ValueTuple<ApplicationChecklistEntryStatusId, string?>>()
        {
            { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, new (ApplicationChecklistEntryStatusId.TO_DO, string.Empty )}
        }.ToImmutableDictionary(), Enumerable.Empty<ProcessStep>());
        var didDocument = JsonDocument.Parse("{\n  \"@context\": [\n    \"https://www.w3.org/ns/did/v1\",\n    \"https://w3id.org/security/suites/ed25519-2020/v1\"\n  ],\n  \"id\": \"did:web:example.com:did:BPNL0000000000XX\",\n  \"verificationMethod\": [\n     {\n         \"id\": [\"did:web:example.com:did:BPNL0000000000XX#key-0\"],\n         \"publicKeyJwk\": {\n            \"kty\": \"JsonWebKey2020\",\n            \"crv\": \"Ed25519\",\n            \"x\": \"3534354354353\"\n         }\n     }\n   ],\n   \"services\": [\n     {\n         \"id\": [\"did:web:example.com:did:BPNL0000000000XX#key-0\"],\n         \"serviceEndpoint\": \"test.org:123\"\n     }\n  ]\n}");
        var data = _fixture.Build<DimWalletData>().With(x => x.DidDocument, didDocument).Create();
        var companyId = Guid.NewGuid();
        A.CallTo(() => _companyRepository.GetCompanyIdByBpn(BPN))
            .Returns(new ValueTuple<bool, Guid, IEnumerable<Guid>>(true, companyId, Enumerable.Repeat(ApplicationId, 1)));
        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(ApplicationId, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, A<IEnumerable<ApplicationChecklistEntryStatusId>>._, ProcessStepTypeId.AWAIT_DIM_RESPONSE, A<IEnumerable<ApplicationChecklistEntryTypeId>?>._, A<IEnumerable<ProcessStepTypeId>?>._))
            .Returns(context);

        // Act
        await _logic.ProcessDimResponse(BPN, data, CancellationToken.None);

        // Assert
        A.CallTo(() => _companyRepository.CreateWalletData(A<Guid>._, A<string>._, A<JsonDocument>._, A<string>._, A<byte[]>._, A<byte[]?>._, A<int>._, A<string>._))
            .MustNotHaveHappened();
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(context, null, A<Action<ApplicationChecklistEntry>>._, null))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ProcessDimResponse_WithValid_CallsExpected()
    {
        // Arrange
        var context = new IApplicationChecklistService.ManualChecklistProcessStepData(
            ApplicationId,
            _fixture.Create<Process>(),
            Guid.NewGuid(),
            ApplicationChecklistEntryTypeId.IDENTITY_WALLET,
            ImmutableDictionary.CreateRange(new[] { KeyValuePair.Create<ApplicationChecklistEntryTypeId, ValueTuple<ApplicationChecklistEntryStatusId, string?>>(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, new(ApplicationChecklistEntryStatusId.TO_DO, string.Empty)) }),
            Enumerable.Empty<ProcessStep>());

        const string jsonData = """
                                        {
                                            "@context": [
                                                "https://www.w3.org/ns/did/v1",
                                                "https://w3id.org/security/suites/ed25519-2020/v1"
                                            ],
                                            "id": "did:web:example.com:did:BPNL0000000000XX",
                                            "verificationMethod": [
                                                {
                                                    "id": [
                                                        "did:web:example.com:did:BPNL0000000000XX#key-0"
                                                    ],
                                                    "type": "JsonWebKey2020",
                                                    "publicKeyJwk": {
                                                        "kty": "JsonWebKey2020",
                                                        "crv": "Ed25519",
                                                        "x": "3534354354353"
                                                    }
                                                }
                                            ],
                                            "services": [
                                                {
                                                    "id": [
                                                        "did:web:example.com:did:BPNL0000000000XX#key-0"
                                                    ],
                                                    "type": "CredentialStore",
                                                    "serviceEndpoint": "https://example.com/svc"
                                                }
                                            ]
                                        }
                                """;
        var didDocument = JsonDocument.Parse(jsonData);
        var data = _fixture.Build<DimWalletData>()
            .With(x => x.DidDocument, didDocument)
            .With(x => x.Did, "did:web:example.org:did:BPNL0000000000XX")
            .Create();
        var companyId = Guid.NewGuid();
        A.CallTo(() => _companyRepository.GetCompanyIdByBpn(BPN))
            .Returns(new ValueTuple<bool, Guid, IEnumerable<Guid>>(true, companyId, Enumerable.Repeat(ApplicationId, 1)));
        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(ApplicationId, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, A<IEnumerable<ApplicationChecklistEntryStatusId>>._, ProcessStepTypeId.AWAIT_DIM_RESPONSE, A<IEnumerable<ApplicationChecklistEntryTypeId>?>._, A<IEnumerable<ProcessStepTypeId>?>._))
            .Returns(context);
        byte[]? encrypted = null;
        byte[]? iv = null;

        A.CallTo(() => _companyRepository.CreateWalletData(A<Guid>._, A<string>._, A<JsonDocument>._, A<string>._, A<byte[]>._, A<byte[]?>._, A<int>._, A<string>._))
            .Invokes((Guid _, string _, JsonDocument _, string _, byte[] clientSecret, byte[]? initializationVector, int _, string _) =>
            {
                encrypted = clientSecret;
                iv = initializationVector;
            });

        // Act
        await _logic.ProcessDimResponse(BPN, data, CancellationToken.None);

        // Assert
        A.CallTo(() => _companyRepository.CreateWalletData(companyId, data.Did, didDocument, data.AuthenticationDetails.ClientId, A<byte[]>._, A<byte[]?>._, 1, data.AuthenticationDetails.AuthenticationServiceUrl))
            .MustHaveHappenedOnceExactly();
        encrypted.Should().NotBeNull();
        var decrypted = CryptoHelper.Decrypt(encrypted!, iv, _encryptionKey, System.Security.Cryptography.CipherMode.CBC, System.Security.Cryptography.PaddingMode.PKCS7);
        decrypted.Should().Be(data.AuthenticationDetails.ClientSecret);

        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(context, null, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>>.That.IsSameSequenceAs(new[] { ProcessStepTypeId.VALIDATE_DID_DOCUMENT })))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region ValidateDidDocument

    [Fact]
    public async Task ValidateDidDocument_WithProcessInTodo_ProcessFails()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _logic.ValidateDidDocument(context, CancellationToken.None);

        // Assert
        result.StepStatusId.Should().Be(ProcessStepStatusId.FAILED);
    }

    [Fact]
    public async Task ValidateDidDocument_WithoutApplication_ThrowsNotFoundException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.IN_PROGRESS },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetDidApplicationId(ApplicationId))
            .Returns((false, null, Enumerable.Empty<DateTimeOffset>()));
        async Task Act() => await _logic.ValidateDidDocument(context, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be($"CompanyApplication {ApplicationId} does not exist");
    }

    [Fact]
    public async Task ValidateDidDocument_WitEmptyDid_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.IN_PROGRESS },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetDidApplicationId(ApplicationId))
            .Returns((true, null, Enumerable.Empty<DateTimeOffset>()));
        async Task Act() => await _logic.ValidateDidDocument(context, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("There must be a did set");
    }

    [Fact]
    public async Task ValidateDidDocument_WithoutProcess_ThrowsConflictException()
    {
        // Arrange
        const string did = "did:web:123";
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.IN_PROGRESS },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetDidApplicationId(ApplicationId))
            .Returns((true, did, Enumerable.Empty<DateTimeOffset>()));
        A.CallTo(() => _dimService.ValidateDid(did, A<CancellationToken>._)).Returns(false);
        async Task Act() => await _logic.ValidateDidDocument(context, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be($"There must be excatly on active {ProcessStepTypeId.VALIDATE_DID_DOCUMENT}");
    }

    [Fact]
    public async Task ValidateDidDocument_WithInvalidDid_ProcessStaysInTodo()
    {
        // Arrange
        const string did = "did:web:123";
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
        {
            { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.IN_PROGRESS },
        }.ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        var now = DateTimeOffset.UtcNow;
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _applicationRepository.GetDidApplicationId(ApplicationId))
            .Returns((true, did, Enumerable.Repeat(now, 1)));
        A.CallTo(() => _dimService.ValidateDid(did, A<CancellationToken>._)).Returns(false);

        // Act
        var result = await _logic.ValidateDidDocument(context, CancellationToken.None);

        // Assert
        result.StepStatusId.Should().Be(ProcessStepStatusId.TODO);
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessMessage.Should().BeNull();
    }

    [Fact]
    public async Task ValidateDidDocument_WithCreatedOutsideMaxTime_ProcessFailed()
    {
        // Arrange
        const string did = "did:web:123";
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
        {
            { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.IN_PROGRESS },
        }.ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        var now = DateTimeOffset.UtcNow;
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _applicationRepository.GetDidApplicationId(ApplicationId))
            .Returns((true, did, Enumerable.Repeat(now.AddDays(-8), 1)));
        A.CallTo(() => _dimService.ValidateDid(did, A<CancellationToken>._)).Returns(false);

        // Act
        var result = await _logic.ValidateDidDocument(context, CancellationToken.None);

        // Assert
        result.StepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        result.ScheduleStepTypeIds.Should().ContainSingle(x => x == ProcessStepTypeId.RETRIGGER_VALIDATE_DID_DOCUMENT);
        result.ProcessMessage.Should().Be("The validation was aborted");
    }

    [Fact]
    public async Task ValidateDidDocument_WithValidDid_ProcessDone()
    {
        // Arrange
        const string did = "did:web:123";
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
        {
            { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.IN_PROGRESS },
        }.ToImmutableDictionary();
        var now = DateTimeOffset.Now;
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetDidApplicationId(ApplicationId))
            .Returns((true, did, Enumerable.Repeat(now, 1)));
        A.CallTo(() => _dimService.ValidateDid(did, A<CancellationToken>._)).Returns(true);

        // Act
        var result = await _logic.ValidateDidDocument(context, CancellationToken.None);

        // Assert
        result.StepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ScheduleStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.TRANSMIT_BPN_DID);
    }

    #endregion
}
