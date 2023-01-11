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

using System.Net;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.Bpdm;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.Bpdm.Models;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.Custodian;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.Tests;

public class ChecklistServiceTests
{
    private static readonly Guid IdWithoutBpn = new ("0a9bd7b1-e692-483e-8128-dbf52759c7a5");
    private static readonly Guid IdWithBpn = new ("c244f79a-7faf-4c59-bb85-fbfdf72ce46f");
    private static readonly Guid IdWithFailingCustodian = new ("bda6d1b5-042e-493a-894c-11f3a89c12b1");
    private static readonly Guid IdWithCustodianUnavailable = new ("beaa6de5-d411-4da8-850e-06047d3170be");
    private static readonly Guid NotExistingApplicationId = new ("1942e8d3-b545-4fbc-842c-01a694f84390");
    private static readonly Guid ActiveApplicationCompanyId = new("66c765dd-872d-46e0-aac1-f79330b55406");
    private static readonly string IamUserId = new Guid("4C1A6851-D4E7-4E10-A011-3732CD045E8A").ToString();
    private static readonly Guid CompanyId = new("95c4339e-e087-4cd2-a5b8-44d385e64630");
    private const string ValidBpn = "BPNL123698762345";
    private const string ValidCompanyName = "valid company";
    private const string AlreadyTakenBpn = "BPNL123698762666";

    private readonly IFixture _fixture;
    
    private readonly IApplicationRepository _applicationRepository;
    private readonly IApplicationChecklistRepository _applicationChecklistRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPortalRepositories _portalRepositories;
    
    private readonly IBpdmService _bpdmService;
    private readonly ICustodianService _custodianService;
    private readonly ChecklistService _service;

    public ChecklistServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _applicationRepository = A.Fake<IApplicationRepository>();
        _applicationChecklistRepository = A.Fake<IApplicationChecklistRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();

        _bpdmService = A.Fake<IBpdmService>();
        _custodianService = A.Fake<ICustodianService>();

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>()).Returns(_applicationChecklistRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);

        _service = new ChecklistService(_portalRepositories, _bpdmService, _custodianService, A.Fake<ILogger<IChecklistService>>());
    }
    
    #region TriggerBpnDataPush
    
    [Fact]
    public async Task TriggerBpnDataPush_WithValidData_CallsExpected()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithoutBpn, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        SetupFakesForTrigger(entry);

        // Act
        await _service.TriggerBpnDataPush(IdWithoutBpn, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _bpdmService.TriggerBpnDataPush(A<BpdmTransferData>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(IdWithoutBpn, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, A<Action<ApplicationChecklistEntry>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.IN_PROGRESS);
    }

    [Fact]
    public async Task TriggerBpnDataPush_WithValidDataAndFailingBpdmServiceCall_ThrowsExceptionAndDoesntUpdateEntry()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithoutBpn, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        A.CallTo(() => _bpdmService.TriggerBpnDataPush(A<BpdmTransferData>._, CancellationToken.None))
            .Throws(new ServiceException("Bpdm Service Call failed."));
        
        SetupFakesForTrigger(entry);

        // Act
        async Task Act() => await _service.TriggerBpnDataPush(IdWithoutBpn, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<ServiceException>(Act);
        A.CallTo(() => _bpdmService.TriggerBpnDataPush(A<BpdmTransferData>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(IdWithoutBpn, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, A<Action<ApplicationChecklistEntry>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.TO_DO);
    }

    [Fact]
    public async Task TriggerBpnDataPush_WithNotExistingApplication_ThrowsNotFoundException()
    {
        // Arrange
        SetupFakesForTrigger();

        // Act
        async Task Act() => await _service.TriggerBpnDataPush(NotExistingApplicationId, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Application {NotExistingApplicationId} does not exists.");
        A.CallTo(() => _bpdmService.TriggerBpnDataPush(A<BpdmTransferData>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TriggerBpnDataPush_WithNotSubmittedApplication_ThrowsArgumentException()
    {
        // Arrange
        var createdApplicationId = Guid.NewGuid();
        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(IamUserId, createdApplicationId))
            .ReturnsLazily(() => new BpdmData(CompanyApplicationStatusId.CREATED, null!, null!, null!, null!, null!, true));
        SetupFakesForTrigger();

        // Act
        async Task Act() => await _service.TriggerBpnDataPush(createdApplicationId, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(Act);
        ex.ParamName.Should().Be("applicationId");
        A.CallTo(() => _bpdmService.TriggerBpnDataPush(A<BpdmTransferData>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TriggerBpnDataPush_WithNotExistingUser_ThrowsArgumentException()
    {
        // Arrange
        SetupFakesForTrigger();

        // Act
        async Task Act() => await _service.TriggerBpnDataPush(IdWithoutBpn, Guid.NewGuid().ToString(), CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.ParamName.Should().Be("iamUserId");
        A.CallTo(() => _bpdmService.TriggerBpnDataPush(A<BpdmTransferData>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TriggerBpnDataPush_WithEmptyZipCode_ThrowsConflictException()
    {
        // Arrange
        var applicationWithoutZipId = _fixture.Create<Guid>();
        var data = _fixture.Build<BpdmData>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .With(x => x.IsUserInCompany, true)
            .With(x => x.ZipCode, (string?)null)
            .Create();
        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(IamUserId, applicationWithoutZipId))
            .ReturnsLazily(() => data);
        SetupFakesForTrigger();

        // Act
        async Task Act() => await _service.TriggerBpnDataPush(applicationWithoutZipId, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("ZipCode must not be empty");
        A.CallTo(() => _bpdmService.TriggerBpnDataPush(A<BpdmTransferData>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TriggerBpnDataPush_WithChecklistStatusAlreadyDone_ThrowsConflictException()
    {
        // Arrange
        SetupFakesForTrigger();

        // Act
        async Task Act() => await _service.TriggerBpnDataPush(IdWithBpn, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"{ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER} is not available as next step");
        A.CallTo(() => _bpdmService.TriggerBpnDataPush(A<BpdmTransferData>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    #endregion
    
    #region UpdateCompanyBpn
    
    [Fact]
    public async Task UpdateCompanyBpnAsync_WithInvalidBpn_ThrowsControllerArgumentException()
    {
        // Arrange
        var bpn = "123";

        // Act
        async Task Act() => await _service.UpdateCompanyBpn(IdWithBpn, bpn).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.ParamName.Should().Be("bpn");
        ex.Message.Should().Be("BPN must contain exactly 16 characters long. (Parameter 'bpn')");
    }
    
    [Fact]
    public async Task UpdateCompanyBpnAsync_WithInvalidBpnPrefix_ThrowsControllerArgumentException()
    {
        // Arrange
        var bpn = "BPXX123698762345";

        // Act
        async Task Act() => await _service.UpdateCompanyBpn(IdWithBpn, bpn).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.ParamName.Should().Be("bpn");
        ex.Message.Should().Be("businessPartnerNumbers must prefixed with BPNL (Parameter 'bpn')");
    }

    [Fact]
    public async Task UpdateCompanyBpnAsync_WithNotExistingApplication_ThrowsNotFoundException()
    {
        // Arrange
        SetupForUpdateCompanyBpn();
        
        // Act
        async Task Act() => await _service.UpdateCompanyBpn(NotExistingApplicationId, ValidBpn).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"application {NotExistingApplicationId} not found");
    }

    [Fact]
    public async Task UpdateCompanyBpnAsync_WithAlreadyTakenBpn_ThrowsConflictException()
    {
        // Arrange
        SetupForUpdateCompanyBpn();

        // Act
        async Task Act() => await _service.UpdateCompanyBpn(IdWithoutBpn, AlreadyTakenBpn).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("BusinessPartnerNumber is already assigned to a different company");
    }

    [Fact]
    public async Task UpdateCompanyBpnAsync_WithActiveCompanyForApplication_ThrowsConflictException()
    {
        // Arrange
        SetupForUpdateCompanyBpn();

        // Act
        async Task Act() => await _service.UpdateCompanyBpn(ActiveApplicationCompanyId, ValidBpn).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"application {ActiveApplicationCompanyId} for company {CompanyId} is not pending");
    }

    [Fact]
    public async Task UpdateCompanyBpnAsync_WithBpnAlreadySet_ThrowsConflictException()
    {
        // Arrange
        SetupForUpdateCompanyBpn();

        // Act
        async Task Act() => await _service.UpdateCompanyBpn(IdWithBpn, ValidBpn).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"BusinessPartnerNumber of company {CompanyId} has already been set.");
    }

    [Fact]
    public async Task UpdateCompanyBpnAsync_WithValidData_CallsExpected()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithoutBpn, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        SetupForUpdateCompanyBpn(entry);

        // Act
        await _service.UpdateCompanyBpn(IdWithoutBpn, ValidBpn).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(CompanyId, null, A<Action<Company>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(IdWithoutBpn, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, A<Action<ApplicationChecklistEntry>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
    }

    #endregion
    
    #region CreateWallet
    
    [Fact]
    public async Task CreateWalletAsync_WithApplicationNotSubmitted_ThrowsNotFoundException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        SetupForCreateWallet();

        // Act
        async Task Act() => await _service.CreateWalletAsync(applicationId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"CompanyApplication {applicationId} is not in status SUBMITTED");
    }

    [Fact]
    public async Task CreateWalletAsync_WithBpnNotSet_ThrowsControllerArgumentException()
    {
        // Arrange
        SetupForCreateWallet();

        // Act
        async Task Act() => await _service.CreateWalletAsync(IdWithoutBpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.ParamName.Should().Be("bpn");
    }

    [Fact]
    public async Task CreateWalletAsync_WithCustodianUnavailable_EntryIsUpdatedCorrectly()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithFailingCustodian, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        SetupForCreateWallet(entry);

        // Act
        var result = await _service.CreateWalletAsync(IdWithCustodianUnavailable, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _custodianService.CreateWalletAsync("custodiaNotAvailable", "a company", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(IdWithCustodianUnavailable, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, A<Action<ApplicationChecklistEntry>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.Comment.Should().Contain("Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.ServiceException: Failed");
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.TO_DO);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreateWalletAsync_WithFailingCustodianCall_EntryIsUpdatedCorrectly()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithFailingCustodian, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        SetupForCreateWallet(entry);

        // Act
        var result = await _service.CreateWalletAsync(IdWithFailingCustodian, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _custodianService.CreateWalletAsync("bpnNotValidForWallet", "a company", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(IdWithFailingCustodian, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, A<Action<ApplicationChecklistEntry>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.Comment.Should().Contain("Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.ServiceException: Failed");
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.FAILED);
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task CreateWalletAsync_WithValidData_CallsExpected()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithBpn, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        SetupForCreateWallet(entry);

        // Act
        var result = await _service.CreateWalletAsync(IdWithBpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _custodianService.CreateWalletAsync(ValidBpn, ValidCompanyName, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(IdWithBpn, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, A<Action<ApplicationChecklistEntry>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.Comment.Should().Be("It worked.");
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
        result.Should().BeTrue();
    }

    #endregion
    
    #region ProcessChecklist
    
    [Fact]
    public async Task ProcessChecklist_WithBpnNextStep_ExecutesNothing()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var checklist = new[]
        {
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.IN_PROGRESS),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO)
        };
        SetupForCreateWallet();

        // Act
        await _service.ProcessChecklist(applicationId, checklist, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task ProcessChecklist_WithIdentityWalletAlreadyInProgress_ExecutesNothing()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var checklist = new[]
        {
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.IN_PROGRESS),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO)
        };
        SetupForCreateWallet();

        // Act
        await _service.ProcessChecklist(applicationId, checklist, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task ProcessChecklist_WithCreateWalletFailing_UpdatesEntryExpected()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var checklist = new[]
        {
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO)
        };
        var entry = new ApplicationChecklistEntry(IdWithBpn, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        SetupForCreateWallet(entry);

        // Act
        await _service.ProcessChecklist(applicationId, checklist, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.Comment.Should().Contain($"CompanyApplication {applicationId} is not in status SUBMITTED");
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.FAILED);
    }

    [Fact]
    public async Task ProcessChecklist_WithFailingCustodianCall_EntryIsUpdatedCorrectly()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithFailingCustodian, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var checklist = new[]
        {
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO)
        };
        SetupForCreateWallet(entry);

        // Act
        await _service.ProcessChecklist(IdWithFailingCustodian, checklist, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _custodianService.CreateWalletAsync(A<string>._, A<string>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.Comment.Should().Contain("Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.ServiceException: Failed");
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.FAILED);
    }
    
    [Fact]
    public async Task ProcessChecklist_WithValidData_CallsExpected()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithBpn, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var checklist = new[]
        {
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO)
        };
        SetupForCreateWallet(entry);

        // Act
        await _service.ProcessChecklist(IdWithBpn, checklist, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _custodianService.CreateWalletAsync(ValidBpn, ValidCompanyName, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(IdWithBpn, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, A<Action<ApplicationChecklistEntry>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.Comment.Should().Be("It worked.");
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
    }

    #endregion
    
    #region Setup

    private void SetupFakesForTrigger(ApplicationChecklistEntry? applicationChecklistEntry = null)
    {
        if (applicationChecklistEntry != null)
        {
            SetupForUpdate(applicationChecklistEntry);
        }

        var validData = _fixture.Build<BpdmData>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .With(x => x.IsUserInCompany, true)
            .With(x => x.ZipCode, "50668")
            .Create();
        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(IamUserId, NotExistingApplicationId))
            .ReturnsLazily(() => (BpdmData?)null);

        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(A<string>.That.Not.Matches(x => x == IamUserId), IdWithoutBpn))
            .ReturnsLazily(() => new BpdmData(CompanyApplicationStatusId.SUBMITTED, null!, null!, null!, null!, null!, false));
        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(IamUserId, A<Guid>.That.Matches(x => x == IdWithoutBpn || x == IdWithBpn)))
            .ReturnsLazily(() => validData);

        A.CallTo(() => _applicationChecklistRepository.GetChecklistDataAsync(IdWithoutBpn))
            .ReturnsLazily(() => new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
            });

        A.CallTo(() => _applicationChecklistRepository.GetChecklistDataAsync(IdWithBpn))
            .ReturnsLazily(() => new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>());
    }

    private void SetupForUpdateCompanyBpn(ApplicationChecklistEntry? applicationChecklistEntry = null)
    {
        if (applicationChecklistEntry != null)
        {
            SetupForUpdate(applicationChecklistEntry);
        }

        A.CallTo(() => _userRepository.GetBpnForIamUserUntrackedAsync(IdWithoutBpn, ValidBpn))
            .ReturnsLazily(() => new List<ValueTuple<bool, bool, string?, Guid>>
            {
                new (true, true, null, CompanyId)
            }.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetBpnForIamUserUntrackedAsync(NotExistingApplicationId, ValidBpn))
            .ReturnsLazily(() => new List<ValueTuple<bool, bool, string?, Guid>>
            {
                new (false, true, ValidBpn, CompanyId)
            }.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetBpnForIamUserUntrackedAsync(IdWithoutBpn, AlreadyTakenBpn))
            .ReturnsLazily(() => new List<ValueTuple<bool, bool, string?, Guid>>
            {
                new (true, true, ValidBpn, CompanyId),
                new (false, true, AlreadyTakenBpn, Guid.NewGuid())
            }.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetBpnForIamUserUntrackedAsync(ActiveApplicationCompanyId, ValidBpn))
            .ReturnsLazily(() => new List<ValueTuple<bool, bool, string?, Guid>>
            {
                new (true, false, ValidBpn, CompanyId)
            }.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetBpnForIamUserUntrackedAsync(IdWithBpn, ValidBpn))
            .ReturnsLazily(() => new List<ValueTuple<bool, bool, string?, Guid>>
            {
                new (true, true, ValidBpn, CompanyId)
            }.ToAsyncEnumerable());
    }

    private void SetupForCreateWallet(ApplicationChecklistEntry? applicationChecklistEntry = null)
    {
        if (applicationChecklistEntry != null)
        {
            SetupForUpdate(applicationChecklistEntry);
        }

        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForSubmittedApplicationAsync(IdWithoutBpn))
            .ReturnsLazily(() => new ValueTuple<Guid, string, string?, string>(Guid.NewGuid(), ValidCompanyName, null, "DE"));
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForSubmittedApplicationAsync(IdWithBpn))
            .ReturnsLazily(() => new ValueTuple<Guid, string, string?, string>(CompanyId, ValidCompanyName, ValidBpn, "DE"));
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForSubmittedApplicationAsync(IdWithFailingCustodian))
            .ReturnsLazily(() => new ValueTuple<Guid, string, string?, string>(Guid.NewGuid(), "a company", "bpnNotValidForWallet", "DE"));
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForSubmittedApplicationAsync(IdWithCustodianUnavailable))
            .ReturnsLazily(() => new ValueTuple<Guid, string, string?, string>(Guid.NewGuid(), "a company", "custodiaNotAvailable", "DE"));
        
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForSubmittedApplicationAsync(A<Guid>.That.Not.Matches(x => x == IdWithBpn || x == IdWithoutBpn || x == IdWithFailingCustodian || x == IdWithCustodianUnavailable)))
            .ReturnsLazily(() => new ValueTuple<Guid, string, string?, string>());

        A.CallTo(() => _custodianService.CreateWalletAsync(ValidBpn, ValidCompanyName, CancellationToken.None))
            .ReturnsLazily(() => "It worked.");
        A.CallTo(() => _custodianService.CreateWalletAsync(A<string>.That.Matches(x => x == "bpnNotValidForWallet"), A<string>._, CancellationToken.None))
            .Throws(new ServiceException("Failed"));
        A.CallTo(() => _custodianService.CreateWalletAsync(A<string>.That.Matches(x => x == "custodiaNotAvailable"), A<string>._, CancellationToken.None))
            .Throws(new ServiceException("Failed", HttpStatusCode.ServiceUnavailable));
    }

    private void SetupForUpdate(ApplicationChecklistEntry applicationChecklistEntry)
    {
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._))
            .Invokes((Guid _, ApplicationChecklistEntryTypeId _, Action<ApplicationChecklistEntry> setFields) =>
            {
                applicationChecklistEntry.DateLastChanged = DateTimeOffset.UtcNow;
                setFields.Invoke(applicationChecklistEntry);
            });

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>()).Returns(_applicationChecklistRepository);
    }

    #endregion
}
