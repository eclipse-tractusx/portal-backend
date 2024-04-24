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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using System.Collections.Immutable;
using System.Security.Cryptography;

namespace Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.Tests;

public class IssuerComponentBusinessLogicTests
{
    private static readonly Guid IdWithBpn = new("c244f79a-7faf-4c59-bb85-fbfdf72ce46f");
    private const string ValidBpn = "BPNL123698762345";

    private readonly IApplicationRepository _applicationRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IPortalRepositories _portalRepositories;

    private readonly IIssuerComponentService _issuerComponentService;
    private readonly IApplicationChecklistService _checklistService;
    private readonly IIssuerComponentBusinessLogic _sut;
    private readonly IOptions<IssuerComponentSettings> _options;
    private readonly IFixture _fixture;

    public IssuerComponentBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _applicationRepository = A.Fake<IApplicationRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _issuerComponentService = A.Fake<IIssuerComponentService>();
        _checklistService = A.Fake<IApplicationChecklistService>();

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);

        _options = Options.Create(new IssuerComponentSettings
        {
            Password = "passWord",
            Scope = "test",
            Username = "user@name",
            BaseAddress = "https://base.address.com",
            ClientId = "CatenaX",
            ClientSecret = "pass@Secret",
            GrantType = "cred",
            TokenAddress = "https://key.cloak.com",
            CallbackBaseUrl = "https://example.org/callback",
            EncryptionConfigIndex = 0,
            EncryptionConfigs = new EncryptionModeConfig[]
            {
                new()
                {
                    Index = 0,
                    EncryptionKey = "5892b7e151628aed2a6abf715892b7e151628aed2a62b7e151628aed2a6abf71",
                    CipherMode = CipherMode.CBC,
                    PaddingMode = PaddingMode.PKCS7
                },
            }
        });
        _sut = new IssuerComponentBusinessLogic(_portalRepositories, _issuerComponentService, _checklistService, _options);
    }

    #region CreateBpnlCredential

    [Fact]
    public async Task CreateBpnlCredential_WithValid_CallsExpected()
    {
        // Arrange
        var cryptoConfig = _options.Value.EncryptionConfigs.First();
        var (secret, vector) = CryptoHelper.Encrypt("test123", Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL, ApplicationChecklistEntryStatusId.TO_DO}
            }
            .ToImmutableDictionary();
        var entry = new ApplicationChecklistEntry(IdWithBpn, ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithBpn, ProcessStepTypeId.REQUEST_BPN_CREDENTIAL, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpnlCredentialIformationByApplicationId(IdWithBpn))
            .Returns(new ValueTuple<bool, string?, string?, WalletInformation?>(true, "did:123:testabc", ValidBpn, new WalletInformation("cl1", secret, vector, 0, "https://example.com/wallet")));

        // Act
        var result = await _sut.CreateBpnlCredential(context, CancellationToken.None);

        // Assert
        A.CallTo(() => _issuerComponentService.CreateBpnlCredential(A<CreateBpnCredentialRequest>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        result.StepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ScheduleStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.STORED_BPN_CREDENTIAL);
        result.Modified.Should().BeTrue();
        result.SkipStepTypeIds.Should().BeNull();
        result.ModifyChecklistEntry.Should().NotBeNull();
        result.ModifyChecklistEntry!(entry);
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.IN_PROGRESS);
    }

    [Fact]
    public async Task CreateBpnlCredential_WithApplicationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL, ApplicationChecklistEntryStatusId.TO_DO }
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithBpn, ProcessStepTypeId.REQUEST_BPN_CREDENTIAL, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpnlCredentialIformationByApplicationId(IdWithBpn))
            .Returns(new ValueTuple<bool, string?, string?, WalletInformation?>(false, null, null, null));
        async Task Act() => await _sut.CreateBpnlCredential(context, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        A.CallTo(() => _issuerComponentService.CreateBpnlCredential(A<CreateBpnCredentialRequest>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        ex.Message.Should().Be($"CompanyApplication {IdWithBpn} does not exist");
    }

    [Fact]
    public async Task CreateBpnlCredential_WithHolderNotSet_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL, ApplicationChecklistEntryStatusId.TO_DO }
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithBpn, ProcessStepTypeId.REQUEST_BPN_CREDENTIAL, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpnlCredentialIformationByApplicationId(IdWithBpn))
            .Returns(new ValueTuple<bool, string?, string?, WalletInformation?>(true, null, null, null));
        async Task Act() => await _sut.CreateBpnlCredential(context, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        A.CallTo(() => _issuerComponentService.CreateBpnlCredential(A<CreateBpnCredentialRequest>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        ex.Message.Should().Be("The holder must be set");
    }

    [Fact]
    public async Task CreateBpnlCredential_WithBpnNotSet_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL, ApplicationChecklistEntryStatusId.TO_DO }
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithBpn, ProcessStepTypeId.REQUEST_BPN_CREDENTIAL, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpnlCredentialIformationByApplicationId(IdWithBpn))
            .Returns(new ValueTuple<bool, string?, string?, WalletInformation?>(true, "test123", null, null));
        async Task Act() => await _sut.CreateBpnlCredential(context, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        A.CallTo(() => _issuerComponentService.CreateBpnlCredential(A<CreateBpnCredentialRequest>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        ex.Message.Should().Be("The bpn must be set");
    }

    [Fact]
    public async Task CreateBpnlCredential_WithWalletInformationNotSet_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL, ApplicationChecklistEntryStatusId.TO_DO }
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithBpn, ProcessStepTypeId.REQUEST_BPN_CREDENTIAL, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpnlCredentialIformationByApplicationId(IdWithBpn))
            .Returns(new ValueTuple<bool, string?, string?, WalletInformation?>(true, "test123", ValidBpn, null));
        async Task Act() => await _sut.CreateBpnlCredential(context, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        A.CallTo(() => _issuerComponentService.CreateBpnlCredential(A<CreateBpnCredentialRequest>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        ex.Message.Should().Be("The wallet information must be set");
    }

    #endregion

    #region StoreBpnlCredential

    [Fact]
    public async Task StoreBpnlCredential_WithSuccessful_UpdatesEntry()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithBpn, ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var data = new IssuerResponseData(ValidBpn, IssuerResponseStatus.SUCCESSFUL, null);
        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(
                IdWithBpn,
                ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL,
                A<IEnumerable<ApplicationChecklistEntryStatusId>>._,
                ProcessStepTypeId.STORED_BPN_CREDENTIAL,
                A<IEnumerable<ApplicationChecklistEntryTypeId>?>._,
                A<IEnumerable<ProcessStepTypeId>?>._))
            .Returns(new IApplicationChecklistService.ManualChecklistProcessStepData(Guid.Empty, new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid()), Guid.Empty, ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL, ImmutableDictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>.Empty, new List<ProcessStep>()));
        SetupForProcessIssuerComponentResponse(entry);

        // Act
        await _sut.StoreBpnlCredentialResponse(IdWithBpn, data);

        // Assert
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count(y => y == ProcessStepTypeId.REQUEST_MEMBERSHIP_CREDENTIAL) == 1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        entry.Comment.Should().BeNull();
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
    }

    [Fact]
    public async Task StoreBpnlCredential_WithUnsuccessful_UpdatesEntry()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithBpn, ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var data = new IssuerResponseData(ValidBpn, IssuerResponseStatus.UNSUCCESSFUL, "Comment about the error");
        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(
                IdWithBpn,
                ApplicationChecklistEntryTypeId.MEMBERSHIP_CREDENTIAL,
                A<IEnumerable<ApplicationChecklistEntryStatusId>>._,
                ProcessStepTypeId.STORED_MEMBERSHIP_CREDENTIAL,
                A<IEnumerable<ApplicationChecklistEntryTypeId>?>._,
                A<IEnumerable<ProcessStepTypeId>?>._))
            .Returns(new IApplicationChecklistService.ManualChecklistProcessStepData(Guid.Empty, new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid()), Guid.Empty, ApplicationChecklistEntryTypeId.MEMBERSHIP_CREDENTIAL, ImmutableDictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>.Empty, new List<ProcessStep>()));
        SetupForProcessIssuerComponentResponse(entry);

        // Act
        await _sut.StoreBpnlCredentialResponse(IdWithBpn, data);

        // Assert
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>>.That.IsNull())).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        entry.Comment.Should().Be("Comment about the error");
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.FAILED);
    }

    #endregion

    #region CreateMembershipCredential

    [Fact]
    public async Task CreateMembershipCredential_WithValid_CallsExpected()
    {
        // Arrange
        var cryptoConfig = _options.Value.EncryptionConfigs.First();
        var (secret, vector) = CryptoHelper.Encrypt("test123", Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL, ApplicationChecklistEntryStatusId.TO_DO}
            }
            .ToImmutableDictionary();
        var entry = new ApplicationChecklistEntry(IdWithBpn, ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithBpn, ProcessStepTypeId.REQUEST_BPN_CREDENTIAL, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpnlCredentialIformationByApplicationId(IdWithBpn))
            .Returns(new ValueTuple<bool, string?, string?, WalletInformation?>(true, "did:123:testabc", ValidBpn, new WalletInformation("cl1", secret, vector, 0, "https://example.com/wallet")));

        // Act
        var result = await _sut.CreateMembershipCredential(context, CancellationToken.None);

        // Assert
        A.CallTo(() => _issuerComponentService.CreateMembershipCredential(A<CreateMembershipCredentialRequest>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        result.StepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ScheduleStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.STORED_MEMBERSHIP_CREDENTIAL);
        result.Modified.Should().BeTrue();
        result.SkipStepTypeIds.Should().BeNull();
        result.ModifyChecklistEntry.Should().NotBeNull();
        result.ModifyChecklistEntry!(entry);
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.IN_PROGRESS);
    }

    [Fact]
    public async Task CreateMembershipCredential_WithApplicationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL, ApplicationChecklistEntryStatusId.TO_DO }
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithBpn, ProcessStepTypeId.REQUEST_BPN_CREDENTIAL, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpnlCredentialIformationByApplicationId(IdWithBpn))
            .Returns(new ValueTuple<bool, string?, string?, WalletInformation?>(false, null, null, null));
        async Task Act() => await _sut.CreateBpnlCredential(context, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        A.CallTo(() => _issuerComponentService.CreateMembershipCredential(A<CreateMembershipCredentialRequest>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        ex.Message.Should().Be($"CompanyApplication {IdWithBpn} does not exist");
    }

    [Fact]
    public async Task CreateMembershipCredential_WithHolderNotSet_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL, ApplicationChecklistEntryStatusId.TO_DO }
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithBpn, ProcessStepTypeId.REQUEST_BPN_CREDENTIAL, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpnlCredentialIformationByApplicationId(IdWithBpn))
            .Returns(new ValueTuple<bool, string?, string?, WalletInformation?>(true, null, null, null));
        async Task Act() => await _sut.CreateBpnlCredential(context, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        A.CallTo(() => _issuerComponentService.CreateMembershipCredential(A<CreateMembershipCredentialRequest>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        ex.Message.Should().Be("The holder must be set");
    }

    [Fact]
    public async Task CreateMembershipCredential_WithBpnNotSet_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL, ApplicationChecklistEntryStatusId.TO_DO }
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithBpn, ProcessStepTypeId.REQUEST_BPN_CREDENTIAL, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpnlCredentialIformationByApplicationId(IdWithBpn))
            .Returns(new ValueTuple<bool, string?, string?, WalletInformation?>(true, "test123", null, null));
        async Task Act() => await _sut.CreateMembershipCredential(context, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        A.CallTo(() => _issuerComponentService.CreateMembershipCredential(A<CreateMembershipCredentialRequest>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        ex.Message.Should().Be("The bpn must be set");
    }

    [Fact]
    public async Task CreateMembershipCredential_WithWalletInformationNotSet_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL, ApplicationChecklistEntryStatusId.TO_DO }
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithBpn, ProcessStepTypeId.REQUEST_BPN_CREDENTIAL, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpnlCredentialIformationByApplicationId(IdWithBpn))
            .Returns(new ValueTuple<bool, string?, string?, WalletInformation?>(true, "test123", ValidBpn, null));
        async Task Act() => await _sut.CreateMembershipCredential(context, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        A.CallTo(() => _issuerComponentService.CreateMembershipCredential(A<CreateMembershipCredentialRequest>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        ex.Message.Should().Be("The wallet information must be set");
    }

    #endregion

    #region StoreMembershipCredential

    [Fact]
    public async Task StoreMembershipCredential_WithSuccessful_UpdatesEntry()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithBpn, ApplicationChecklistEntryTypeId.MEMBERSHIP_CREDENTIAL, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var data = new IssuerResponseData(ValidBpn, IssuerResponseStatus.SUCCESSFUL, null);
        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(
                IdWithBpn,
                ApplicationChecklistEntryTypeId.MEMBERSHIP_CREDENTIAL,
                A<IEnumerable<ApplicationChecklistEntryStatusId>>._,
                ProcessStepTypeId.STORED_MEMBERSHIP_CREDENTIAL,
                A<IEnumerable<ApplicationChecklistEntryTypeId>?>._,
                A<IEnumerable<ProcessStepTypeId>?>._))
            .Returns(new IApplicationChecklistService.ManualChecklistProcessStepData(Guid.Empty, new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid()), Guid.Empty, ApplicationChecklistEntryTypeId.MEMBERSHIP_CREDENTIAL, ImmutableDictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>.Empty, new List<ProcessStep>()));
        SetupForProcessIssuerComponentResponse(entry);

        // Act
        await _sut.StoreMembershipCredentialResponse(IdWithBpn, data);

        // Assert
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count(y => y == ProcessStepTypeId.START_CLEARING_HOUSE) == 1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        entry.Comment.Should().BeNull();
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
    }

    [Fact]
    public async Task StoreMembershipCredential_WithUnsuccessful_UpdatesEntry()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithBpn, ApplicationChecklistEntryTypeId.MEMBERSHIP_CREDENTIAL, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var data = new IssuerResponseData(ValidBpn, IssuerResponseStatus.UNSUCCESSFUL, "Comment about the error");
        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(
                IdWithBpn,
                ApplicationChecklistEntryTypeId.MEMBERSHIP_CREDENTIAL,
                A<IEnumerable<ApplicationChecklistEntryStatusId>>._,
                ProcessStepTypeId.STORED_MEMBERSHIP_CREDENTIAL,
                A<IEnumerable<ApplicationChecklistEntryTypeId>?>._,
                A<IEnumerable<ProcessStepTypeId>?>._))
            .Returns(new IApplicationChecklistService.ManualChecklistProcessStepData(Guid.Empty, new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid()), Guid.Empty, ApplicationChecklistEntryTypeId.MEMBERSHIP_CREDENTIAL, ImmutableDictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>.Empty, new List<ProcessStep>()));
        SetupForProcessIssuerComponentResponse(entry);

        // Act
        await _sut.StoreMembershipCredentialResponse(IdWithBpn, data);

        // Assert
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>>.That.IsNull())).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        entry.Comment.Should().Be("Comment about the error");
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.FAILED);
    }

    #endregion

    #region CreateFrameworkCredential

    [Fact]
    public async Task CreateFrameworkCredential_WithValid_CallsExpected()
    {
        // Arrange
        var credentialId = Guid.NewGuid();
        var identityId = Guid.NewGuid();
        var useCaseFrameworkVersionId = Guid.NewGuid();
        var cryptoConfig = _options.Value.EncryptionConfigs.First();
        var (secret, vector) = CryptoHelper.Encrypt("test123", Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);
        A.CallTo(() => _companyRepository.GetWalletData(identityId))
            .Returns(new ValueTuple<string?, string?, WalletInformation?>("did:123:testabc", ValidBpn, new WalletInformation("cl1", secret, vector, 0, "https://example.com/wallet"))
            );
        A.CallTo(() => _issuerComponentService.CreateFrameworkCredential(A<CreateFrameworkCredentialRequest>._, A<CancellationToken>._))
            .Returns(credentialId);

        // Act
        var result = await _sut.CreateFrameworkCredentialData(useCaseFrameworkVersionId, UseCaseFrameworkId.TRACEABILITY_CREDENTIAL, identityId, CancellationToken.None);

        // Assert
        A.CallTo(() => _issuerComponentService.CreateFrameworkCredential(A<CreateFrameworkCredentialRequest>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        result.Should().Be(credentialId);
    }

    [Fact]
    public async Task CreateFrameworkCredentialData_WithHolderNotSet_ThrowsConflictException()
    {
        // Arrange
        var useCaseFrameworkVersionId = Guid.NewGuid();
        var identityId = Guid.NewGuid();
        A.CallTo(() => _companyRepository.GetWalletData(identityId))
            .Returns(new ValueTuple<string?, string?, WalletInformation?>(null, null, null));
        async Task Act() => await _sut.CreateFrameworkCredentialData(useCaseFrameworkVersionId, UseCaseFrameworkId.TRACEABILITY_CREDENTIAL, identityId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        A.CallTo(() => _issuerComponentService.CreateFrameworkCredential(A<CreateFrameworkCredentialRequest>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        ex.Message.Should().Be($"The holder must be set");
    }

    [Fact]
    public async Task CreateFrameworkCredential_WithBusinessPartnerNumberNotSet_ThrowsConflictException()
    {
        // Arrange
        var useCaseFrameworkVersionId = Guid.NewGuid();
        var identityId = Guid.NewGuid();
        A.CallTo(() => _companyRepository.GetWalletData(identityId))
            .Returns(new ValueTuple<string?, string?, WalletInformation?>("test", null, null));
        async Task Act() => await _sut.CreateFrameworkCredentialData(useCaseFrameworkVersionId, UseCaseFrameworkId.TRACEABILITY_CREDENTIAL, identityId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        A.CallTo(() => _issuerComponentService.CreateFrameworkCredential(A<CreateFrameworkCredentialRequest>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        ex.Message.Should().Be("The bpn must be set");
    }

    [Fact]
    public async Task CreateFrameworkCredential_WithWalletInformationNotSet_ThrowsConflictException()
    {
        // Arrange
        var useCaseFrameworkVersionId = Guid.NewGuid();
        var identityId = Guid.NewGuid();
        A.CallTo(() => _companyRepository.GetWalletData(identityId))
            .Returns(new ValueTuple<string?, string?, WalletInformation?>("test", "BPNL0000001Test", null));
        async Task Act() => await _sut.CreateFrameworkCredentialData(useCaseFrameworkVersionId, UseCaseFrameworkId.TRACEABILITY_CREDENTIAL, identityId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        A.CallTo(() => _issuerComponentService.CreateFrameworkCredential(A<CreateFrameworkCredentialRequest>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        ex.Message.Should().Be("The wallet information must be set");
    }

    #endregion

    #region Setup

    private void SetupForProcessIssuerComponentResponse(ApplicationChecklistEntry applicationChecklistEntry)
    {
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Invokes((IApplicationChecklistService.ManualChecklistProcessStepData _, Action<ApplicationChecklistEntry> initialApplicationChecklistEntry, Action<ApplicationChecklistEntry> modifyApplicationChecklistEntry, IEnumerable<ProcessStepTypeId> _) =>
            {
                applicationChecklistEntry.DateLastChanged = DateTimeOffset.UtcNow;
                modifyApplicationChecklistEntry.Invoke(applicationChecklistEntry);
            });
    }

    #endregion
}
