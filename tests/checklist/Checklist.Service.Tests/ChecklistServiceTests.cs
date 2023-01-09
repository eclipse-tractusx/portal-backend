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

using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Service;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Service.Bpdm;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Service.Bpdm.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Framework.Checklist.Tests;

public class ChecklistServiceTests
{
    private static readonly Guid ApplicationWithoutBpnId = new ("0a9bd7b1-e692-483e-8128-dbf52759c7a5");
    private static readonly Guid ApplicationWithBpnId = new ("c244f79a-7faf-4c59-bb85-fbfdf72ce46f");
    private static readonly Guid NotExistingApplicationId = new ("1942e8d3-b545-4fbc-842c-01a694f84390");
    private static readonly string IamUserId = new Guid("4C1A6851-D4E7-4E10-A011-3732CD045E8A").ToString();
    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IBpdmService _bpdmService;
    private readonly IApplicationRepository _applicationRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IApplicationChecklistRepository _applicationChecklistRepository;
    private readonly ChecklistService _service;

    public ChecklistServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _bpdmService = A.Fake<IBpdmService>();
        
        _applicationRepository = A.Fake<IApplicationRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _applicationChecklistRepository = A.Fake<IApplicationChecklistRepository>();

        _service = new ChecklistService(_portalRepositories, _bpdmService);
    }
    
    #region CreateInitialChecklistAsync

    [Fact]
    public async Task CreateInitialChecklistAsync_WithBpnSet_CreatesExpectedResult()
    {
        // Arrange
        SetupFakesForCreate();
        
        // Act
        await _service.CreateInitialChecklistAsync(ApplicationWithBpnId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.CreateChecklistForApplication(
            ApplicationWithBpnId,
            A<IEnumerable<(ChecklistEntryTypeId TypeId, ChecklistEntryStatusId StatusId)>>
                .That
                .Matches(x => 
                    x.Count(y => y.TypeId == ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER && y.StatusId == ChecklistEntryStatusId.DONE) == 1)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateInitialChecklistAsync_WithoutBpnSet_CreatesExpectedResult()
    {
        // Arrange
        SetupFakesForCreate();
        
        // Act
        await _service.CreateInitialChecklistAsync(ApplicationWithoutBpnId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.CreateChecklistForApplication(
                ApplicationWithoutBpnId,
                A<IEnumerable<(ChecklistEntryTypeId TypeId, ChecklistEntryStatusId StatusId)>>
                    .That
                    .Matches(x => x.All(y => y.StatusId == ChecklistEntryStatusId.TO_DO))))
            .MustHaveHappenedOnceExactly();
    }

    #endregion
    
    #region UpdateBpnStatus
    
    [Theory]
    [InlineData(ChecklistEntryStatusId.IN_PROGRESS)]
    [InlineData(ChecklistEntryStatusId.DONE)]
    [InlineData(ChecklistEntryStatusId.FAILED)]
    public async Task UpdateBpnStatus_WithStatus_SetsExpectedStatus(ChecklistEntryStatusId statusId)
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(ApplicationWithoutBpnId, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        SetupFakesForUpdate(entry);
        
        // Act
        await _service.UpdateBpnStatusAsync(ApplicationWithoutBpnId, statusId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.StatusId = statusId;
    }
    
    #endregion

    #region TriggerBpnDataPush
    
    [Fact]
    public async Task TriggerBpnDataPush_WithValidData_CallsExpected()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(ApplicationWithoutBpnId, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        SetupFakesForTrigger(entry);

        // Act
        await _service.TriggerBpnDataPush(ApplicationWithoutBpnId, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _bpdmService.TriggerBpnDataPush(A<BpdmTransferData>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(ApplicationWithoutBpnId, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, A<Action<ApplicationChecklistEntry>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.StatusId.Should().Be(ChecklistEntryStatusId.IN_PROGRESS);
    }

    [Fact]
    public async Task TriggerBpnDataPush_WithValidDataAndFailingBpdmServiceCall_ThrowsExceptionAndDoesntUpdateEntry()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(ApplicationWithoutBpnId, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        A.CallTo(() => _bpdmService.TriggerBpnDataPush(A<BpdmTransferData>._, CancellationToken.None))
            .Throws(new ServiceException("Bpdm Service Call failed."));
        
        SetupFakesForTrigger(entry);

        // Act
        async Task Act() => await _service.TriggerBpnDataPush(ApplicationWithoutBpnId, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<ServiceException>(Act);
        A.CallTo(() => _bpdmService.TriggerBpnDataPush(A<BpdmTransferData>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(ApplicationWithoutBpnId, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, A<Action<ApplicationChecklistEntry>>._)).MustNotHaveHappened();
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
        async Task Act() => await _service.TriggerBpnDataPush(ApplicationWithoutBpnId, Guid.NewGuid().ToString(), CancellationToken.None).ConfigureAwait(false);

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
        async Task Act() => await _service.TriggerBpnDataPush(ApplicationWithBpnId, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"{ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER} is not available as next step");
        A.CallTo(() => _bpdmService.TriggerBpnDataPush(A<BpdmTransferData>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    #endregion
    
    #region Setup

    private void SetupFakesForCreate()
    {
        A.CallTo(() => _applicationRepository.GetBpnForApplicationIdAsync(ApplicationWithBpnId)).ReturnsLazily(() => "testbpn");
        A.CallTo(() => _applicationRepository.GetBpnForApplicationIdAsync(ApplicationWithoutBpnId)).ReturnsLazily(() => (string?)null);

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>()).Returns(_applicationChecklistRepository);
    }

    private void SetupFakesForUpdate(ApplicationChecklistEntry applicationChecklistEntry)
    {
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, A<Action<ApplicationChecklistEntry>>._))
            .Invokes((Guid _, ChecklistEntryTypeId _, Action<ApplicationChecklistEntry> setFields) =>
                {
                    applicationChecklistEntry.DateLastChanged = DateTimeOffset.UtcNow;
                    setFields.Invoke(applicationChecklistEntry);
                });

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>()).Returns(_applicationChecklistRepository);
    }

    private void SetupFakesForTrigger(ApplicationChecklistEntry? applicationChecklistEntry = null)
    {
        if (applicationChecklistEntry != null)
        {
            SetupFakesForUpdate(applicationChecklistEntry);
        }

        var validData = _fixture.Build<BpdmData>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .With(x => x.IsUserInCompany, true)
            .With(x => x.ZipCode, "50668")
            .Create();
        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(IamUserId, NotExistingApplicationId))
            .ReturnsLazily(() => (BpdmData?)null);

        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(A<string>.That.Not.Matches(x => x == IamUserId), ApplicationWithoutBpnId))
            .ReturnsLazily(() => new BpdmData(CompanyApplicationStatusId.SUBMITTED, null!, null!, null!, null!, null!, false));
        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(IamUserId, A<Guid>.That.Matches(x => x == ApplicationWithoutBpnId || x == ApplicationWithBpnId)))
            .ReturnsLazily(() => validData);

        A.CallTo(() => _applicationChecklistRepository.GetChecklistDataAsync(ApplicationWithoutBpnId))
            .ReturnsLazily(() => new Dictionary<ChecklistEntryTypeId, ChecklistEntryStatusId>
            {
                {ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ChecklistEntryStatusId.TO_DO},
            });

        A.CallTo(() => _applicationChecklistRepository.GetChecklistDataAsync(ApplicationWithBpnId))
            .ReturnsLazily(() => new Dictionary<ChecklistEntryTypeId, ChecklistEntryStatusId>());

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>()).Returns(_applicationChecklistRepository);
    }

    #endregion
}
