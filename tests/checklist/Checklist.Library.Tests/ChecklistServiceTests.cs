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

using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;
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
    private static readonly Guid NotExistingApplicationId = new ("1942e8d3-b545-4fbc-842c-01a694f84390");
    private static readonly Guid ActiveApplicationCompanyId = new("66c765dd-872d-46e0-aac1-f79330b55406");
    private static readonly string IamUserId = new Guid("4C1A6851-D4E7-4E10-A011-3732CD045E8A").ToString();
    private static readonly Guid CompanyId = new("95c4339e-e087-4cd2-a5b8-44d385e64630");
    private const string ValidBpn = "BPNL123698762345";
    private const string AlreadyTakenBpn = "BPNL123698762666";
    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IBpdmService _bpdmService;
    private readonly IApplicationChecklistRepository _applicationChecklistRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserRepository _userRepository;
    private readonly ChecklistService _service;
    private readonly ICustodianService _custodianService;

    public ChecklistServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _bpdmService = A.Fake<IBpdmService>();
        _custodianService = A.Fake<ICustodianService>();
        
        _applicationChecklistRepository = A.Fake<IApplicationChecklistRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _userRepository = A.Fake<IUserRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>()).Returns(_applicationChecklistRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);

        _service = new ChecklistService(_portalRepositories, _bpdmService, _custodianService);
    }
    
    #region TriggerBpnDataPush
    
    [Fact]
    public async Task TriggerBpnDataPush_WithValidData_CallsExpected()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithoutBpn, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        SetupFakesForTrigger(entry);

        // Act
        await _service.TriggerBpnDataPush(IdWithoutBpn, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _bpdmService.TriggerBpnDataPush(A<BpdmTransferData>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(IdWithoutBpn, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, A<Action<ApplicationChecklistEntry>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.StatusId.Should().Be(ChecklistEntryStatusId.IN_PROGRESS);
    }

    [Fact]
    public async Task TriggerBpnDataPush_WithValidDataAndFailingBpdmServiceCall_ThrowsExceptionAndDoesntUpdateEntry()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithoutBpn, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        A.CallTo(() => _bpdmService.TriggerBpnDataPush(A<BpdmTransferData>._, CancellationToken.None))
            .Throws(new ServiceException("Bpdm Service Call failed."));
        
        SetupFakesForTrigger(entry);

        // Act
        async Task Act() => await _service.TriggerBpnDataPush(IdWithoutBpn, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<ServiceException>(Act);
        A.CallTo(() => _bpdmService.TriggerBpnDataPush(A<BpdmTransferData>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(IdWithoutBpn, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, A<Action<ApplicationChecklistEntry>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        entry.StatusId.Should().Be(ChecklistEntryStatusId.TO_DO);
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
        ex.Message.Should().Be($"{ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER} is not available as next step");
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
        var entry = new ApplicationChecklistEntry(IdWithoutBpn, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        SetupForUpdateCompanyBpn(entry);

        // Act
        await _service.UpdateCompanyBpn(IdWithoutBpn, ValidBpn).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(CompanyId, A<Action<Company>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(IdWithoutBpn, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, A<Action<ApplicationChecklistEntry>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.StatusId.Should().Be(ChecklistEntryStatusId.DONE);
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
            .ReturnsLazily(() => new Dictionary<ChecklistEntryTypeId, ChecklistEntryStatusId>
            {
                {ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ChecklistEntryStatusId.TO_DO},
            });

        A.CallTo(() => _applicationChecklistRepository.GetChecklistDataAsync(IdWithBpn))
            .ReturnsLazily(() => new Dictionary<ChecklistEntryTypeId, ChecklistEntryStatusId>());
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

    private void SetupForUpdate(ApplicationChecklistEntry applicationChecklistEntry)
    {
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, A<Action<ApplicationChecklistEntry>>._))
            .Invokes((Guid _, ChecklistEntryTypeId _, Action<ApplicationChecklistEntry> setFields) =>
            {
                applicationChecklistEntry.DateLastChanged = DateTimeOffset.UtcNow;
                setFields.Invoke(applicationChecklistEntry);
            });

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>()).Returns(_applicationChecklistRepository);
    }

    #endregion
}
