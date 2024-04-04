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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Tests;

public class BpdmBusinessLogicTests
{
    #region Initialization

    private static readonly Guid IdWithBpn = new("c244f79a-7faf-4c59-bb85-fbfdf72ce46f");
    private static readonly Guid IdWithSharingPending = new("920AF606-9581-4EAD-A7FD-78480F42D3A1");
    private static readonly Guid IdWithSharingError = new("9460ED6B-2DD3-4446-9B9D-9AE3640717F4");
    private static readonly Guid IdWithoutSharingProcessStarted = new("f167835a-9859-4ae4-8f1d-b5d682e2562c");
    private static readonly Guid IdWithStateCreated = new("bda6d1b5-042e-493a-894c-11f3a89c12b1");
    private static readonly Guid IdWithoutZipCode = new("beaa6de5-d411-4da8-850e-06047d3170be");
    private static readonly Guid ValidCompanyId = new("abf990f8-0c27-43dc-bbd0-b1bce964d8f4");
    private const string ValidCompanyName = "valid company";

    private readonly IFixture _fixture;
    private readonly IApplicationRepository _applicationRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IBpdmService _bpdmService;
    private readonly BpdmBusinessLogic _logic;
    private readonly IPortalRepositories _portalRepositories;

    public BpdmBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _bpdmService = A.Fake<IBpdmService>();
        var options = A.Fake<IOptions<BpdmServiceSettings>>();

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);

        _logic = new BpdmBusinessLogic(_portalRepositories, _bpdmService, options);
    }

    #endregion

    #region Trigger PushLegalEntity

    [Fact]
    public async Task PushLegalEntity_WithoutBpdmData_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithBpn, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithBpn)))
            .Returns((ValidCompanyId, null!));

        // Act
        Task Act() => _logic.PushLegalEntity(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be("BpdmData should never be null here");
    }

    [Fact]
    public async Task PushLegalEntity_WithoutExistingApplication_ThrowsNotFoundException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(applicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupFakesForTrigger();

        // Act
        Task Act() => _logic.PushLegalEntity(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Application {applicationId} does not exists.");
    }

    [Fact]
    public async Task PushLegalEntity_WithBpn_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithoutZipCode, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithoutZipCode)))
            .Returns((ValidCompanyId, new BpdmData(ValidCompanyName, null!, "Test", null!, null!, "Test", "test", null!, null!, Enumerable.Empty<(BpdmIdentifierId UniqueIdentifierId, string Value)>())));

        // Act
        Task Act() => _logic.PushLegalEntity(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("BusinessPartnerNumber is already set");
    }

    [Fact]
    public async Task PushLegalEntity_WithoutAlpha2Code_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithoutZipCode, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithoutZipCode)))
            .Returns((ValidCompanyId, new BpdmData(ValidCompanyName, null!, null!, null!, null!, "Test", "test", null!, null!, Enumerable.Empty<(BpdmIdentifierId UniqueIdentifierId, string Value)>())));

        // Act
        Task Act() => _logic.PushLegalEntity(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Alpha2Code must not be empty");
    }

    [Fact]
    public async Task PushLegalEntity_WithoutCity_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithoutZipCode, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithoutZipCode)))
            .Returns((ValidCompanyId, new BpdmData(ValidCompanyName, null!, null!, "DE", null!, null, "test", null!, null!, Enumerable.Empty<(BpdmIdentifierId UniqueIdentifierId, string Value)>())));

        // Act
        Task Act() => _logic.PushLegalEntity(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("City must not be empty");
    }

    [Fact]
    public async Task PushLegalEntity_WithoutStreetName_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithoutZipCode, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithoutZipCode)))
            .Returns((ValidCompanyId, new BpdmData(ValidCompanyName, null!, null!, "DE", null!, "TEST", null, null!, null!, Enumerable.Empty<(BpdmIdentifierId UniqueIdentifierId, string Value)>())));

        // Act
        Task Act() => _logic.PushLegalEntity(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("StreetName must not be empty");
    }

    [Fact]
    public async Task PushLegalEntity_WithValidData_CallsExpected()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithBpn, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithBpn, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupFakesForTrigger();

        // Act
        var result = await _logic.PushLegalEntity(context, CancellationToken.None);

        // Assert
        result.Modified.Should().BeTrue();
        result.ModifyChecklistEntry?.Invoke(entry);
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.IN_PROGRESS);
        A.CallTo(() => _bpdmService.PutInputLegalEntity(
                A<BpdmTransferData>.That.Matches(x => x.ZipCode == "50668" && x.CompanyName == ValidCompanyName),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region HandlePullLegalEntity

    [Fact]
    public async Task HandlePullLegalEntity_WithoutExistingApplication_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(applicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandlePullLegalEntity();

        // Act
        Task Act() => _logic.HandlePullLegalEntity(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be($"CompanyApplication {applicationId} does not exist");
    }

    [Fact]
    public async Task HandlePullLegalEntity_WithoutLegalEntity_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithStateCreated, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandlePullLegalEntity();

        // Act
        Task Act() => _logic.HandlePullLegalEntity(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be($"not found");
    }

    [Fact]
    public async Task HandlePullLegalEntity_WithLegalEntityWithoutBpn_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithoutZipCode, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandlePullLegalEntity();

        // Act
        var result = await _logic.HandlePullLegalEntity(context, CancellationToken.None);

        // Assert
        result.ModifyChecklistEntry.Should().BeNull();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
        result.Modified.Should().BeFalse();
    }

    [Fact]
    public async Task HandlePullLegalEntity_WithSharingStateError_ThrowsServiceException()
    {
        // Arrange
        var company = new Company(Guid.NewGuid(), "Test Company", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow)
        {
            BusinessPartnerNumber = "1"
        };
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithSharingError, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandlePullLegalEntity(company);

        // Act
        Task Act() => _logic.HandlePullLegalEntity(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be($"ErrorCode: Code 43, ErrorMessage: This is a test sharing state error");
    }

    [Fact]
    public async Task HandlePullLegalEntity_WithSharingProcessStartedNotSet_ReturnsExpected()
    {
        // Arrange
        var company = new Company(Guid.NewGuid(), "Test Company", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow)
        {
            BusinessPartnerNumber = "1"
        };
        var checklistEntry = _fixture.Build<ApplicationChecklistEntry>()
            .With(x => x.ApplicationChecklistEntryStatusId, ApplicationChecklistEntryStatusId.TO_DO)
            .Create();
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithoutSharingProcessStarted, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandlePullLegalEntity(company);

        // Act
        var result = await _logic.HandlePullLegalEntity(context, CancellationToken.None);

        // Assert
        result.ModifyChecklistEntry?.Invoke(checklistEntry);
        checklistEntry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.TO_DO);
        result.ScheduleStepTypeIds.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
        result.Modified.Should().BeFalse();
        result.ProcessMessage.Should().Be("SharingProcessStarted was not set");
    }

    [Fact]
    public async Task HandlePullLegalEntity_WithSharingTypePending_ReturnsExpected()
    {
        // Arrange
        var company = new Company(Guid.NewGuid(), "Test Company", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow)
        {
            BusinessPartnerNumber = "1"
        };
        var checklistEntry = _fixture.Build<ApplicationChecklistEntry>()
            .With(x => x.ApplicationChecklistEntryStatusId,
                ApplicationChecklistEntryStatusId.TO_DO).Create();
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithSharingPending, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandlePullLegalEntity(company);

        // Act
        var result = await _logic.HandlePullLegalEntity(context, CancellationToken.None);

        // Assert
        result.ModifyChecklistEntry?.Invoke(checklistEntry);
        checklistEntry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.TO_DO);
        result.ScheduleStepTypeIds.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
        result.Modified.Should().BeFalse();
    }

    [Theory]
    [InlineData(true, ProcessStepTypeId.CREATE_DIM_WALLET)]
    [InlineData(false, ProcessStepTypeId.CREATE_IDENTITY_WALLET)]
    public async Task HandlePullLegalEntity_WithValidData_ReturnsExpected(bool useDimWallet, ProcessStepTypeId processStepTypeId)
    {
        // Arrange
        var options = Options.Create(new BpdmServiceSettings
        {
            UseDimWallet = useDimWallet
        });
        var company = new Company(Guid.NewGuid(), "Test Company", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow)
        {
            BusinessPartnerNumber = "1"
        };
        var checklistEntry = _fixture.Build<ApplicationChecklistEntry>()
            .With(x => x.ApplicationChecklistEntryStatusId,
            ApplicationChecklistEntryStatusId.TO_DO).Create();
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithBpn, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandlePullLegalEntity(company);
        var logic = new BpdmBusinessLogic(_portalRepositories, _bpdmService, options);

        // Act
        var result = await logic.HandlePullLegalEntity(context, CancellationToken.None);

        // Assert
        result.ModifyChecklistEntry?.Invoke(checklistEntry);
        checklistEntry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
        result.ScheduleStepTypeIds.Should().ContainSingle().And.Subject.Single().Should().Be(processStepTypeId);
        result.SkipStepTypeIds.Should().ContainSingle(x => x == ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL);
        result.Modified.Should().BeTrue();
    }

    [Fact]
    public async Task HandlePullLegalEntity_WithValidDataAndRegistrationVerificationFailed_ReturnsExpected()
    {
        // Arrange
        var company = new Company(Guid.NewGuid(), "Test Company", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow)
        {
            BusinessPartnerNumber = "1"
        };
        var checklistEntry = _fixture.Build<ApplicationChecklistEntry>()
            .With(x => x.ApplicationChecklistEntryStatusId,
                ApplicationChecklistEntryStatusId.TO_DO).Create();
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.FAILED},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithBpn, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandlePullLegalEntity(company);

        // Act
        var result = await _logic.HandlePullLegalEntity(context, CancellationToken.None);

        // Assert
        result.ModifyChecklistEntry?.Invoke(checklistEntry);
        checklistEntry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
        result.ScheduleStepTypeIds.Should().BeNull();
        result.SkipStepTypeIds.Should().ContainSingle(x => x == ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL);
        result.Modified.Should().BeTrue();
    }

    #endregion

    #region Setup

    private void SetupFakesForTrigger()
    {
        var validData = _fixture.Build<BpdmData>()
            .With(x => x.ZipCode, "50668")
            .With(x => x.CompanyName, ValidCompanyName)
            .With(x => x.Alpha2Code, "DE")
            .With(x => x.City, "Test")
            .With(x => x.StreetName, "test")
            .With(x => x.BusinessPartnerNumber, default(string?))
            .Create();

        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithBpn)))
            .Returns((ValidCompanyId, validData));
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithStateCreated)))
            .Returns((ValidCompanyId, new BpdmData(ValidCompanyName, null!, null!, "DE", null!, "Test", "test", null!, null!, Enumerable.Empty<(BpdmIdentifierId UniqueIdentifierId, string Value)>())));
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithoutZipCode)))
            .Returns((ValidCompanyId, new BpdmData(ValidCompanyName, null!, null!, null!, null!, "Test", "test", null!, null!, Enumerable.Empty<(BpdmIdentifierId UniqueIdentifierId, string Value)>())));
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Not.Matches(x => x == IdWithStateCreated || x == IdWithBpn || x == IdWithoutZipCode)))
            .Returns<(Guid, BpdmData)>(default);

        A.CallTo(() => _bpdmService.PutInputLegalEntity(
                A<BpdmTransferData>.That.Matches(x => x.CompanyName == ValidCompanyName && x.ZipCode == "50668"),
                A<CancellationToken>._))
            .Returns(true);
    }

    private void SetupForHandlePullLegalEntity(Company? company = null)
    {
        var validData = _fixture.Build<BpdmData>()
            .With(x => x.ZipCode, "50668")
            .With(x => x.CompanyName, ValidCompanyName)
            .With(x => x.Alpha2Code, "DE")
            .With(x => x.City, "Test")
            .With(x => x.StreetName, "test")
            .With(x => x.BusinessPartnerNumber, default(string?))
            .Create();

        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithBpn || x == IdWithSharingError || x == IdWithSharingPending || x == IdWithoutSharingProcessStarted)))
            .Returns((ValidCompanyId, validData));
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithStateCreated)))
            .Returns((ValidCompanyId, new BpdmData(ValidCompanyName, null!, null!, "DE", null!, "Test", "test", null!, null!, Enumerable.Empty<(BpdmIdentifierId UniqueIdentifierId, string Value)>())));
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithoutZipCode)))
            .Returns((ValidCompanyId, new BpdmData(ValidCompanyName, null!, null!, null!, null!, "Test", "test", null!, null!, Enumerable.Empty<(BpdmIdentifierId UniqueIdentifierId, string Value)>())));
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Not.Matches(x => x == IdWithStateCreated || x == IdWithBpn || x == IdWithoutZipCode || x == IdWithSharingError || x == IdWithSharingPending || x == IdWithoutSharingProcessStarted)))
            .Returns<(Guid, BpdmData)>(default);

        A.CallTo(() => _bpdmService.FetchInputLegalEntity(A<string>.That.Matches(x => x == IdWithStateCreated.ToString()), A<CancellationToken>._))
            .ThrowsAsync(new ServiceException("not found", System.Net.HttpStatusCode.NotFound));
        A.CallTo(() => _bpdmService.FetchInputLegalEntity(A<string>.That.Matches(x => x == IdWithoutZipCode.ToString()), A<CancellationToken>._))
            .Returns(_fixture.Build<BpdmLegalEntityOutputData>().With(x => x.LegalEntity, default(BpdmLegelEntityData?)).Create());
        A.CallTo(() => _bpdmService.FetchInputLegalEntity(A<string>.That.Matches(x => x == IdWithBpn.ToString()), A<CancellationToken>._))
            .Returns(_fixture.Build<BpdmLegalEntityOutputData>().With(x => x.LegalEntity, new BpdmLegelEntityData("CAXSDUMMYCATENAZZ", null, null, null, Enumerable.Empty<BpdmProfileClassification>())).Create());
        A.CallTo(() => _bpdmService.GetSharingState(A<Guid>.That.Matches(x => x == IdWithBpn || x == IdWithStateCreated || x == IdWithoutZipCode || x == IdWithoutSharingProcessStarted), A<CancellationToken>._))
            .Returns(_fixture.Build<BpdmSharingState>()
                .With(x => x.SharingStateType, BpdmSharingStateType.Success)
                .With(x => x.SharingProcessStarted, DateTimeOffset.UtcNow)
                .Create());
        A.CallTo(() => _bpdmService.GetSharingState(IdWithSharingPending, A<CancellationToken>._))
            .Returns(_fixture.Build<BpdmSharingState>()
                .With(x => x.SharingStateType, BpdmSharingStateType.Pending)
                .With(x => x.SharingProcessStarted, DateTimeOffset.UtcNow)
                .Create());
        A.CallTo(() => _bpdmService.GetSharingState(IdWithSharingError, A<CancellationToken>._))
            .Returns(_fixture.Build<BpdmSharingState>()
                .With(x => x.SharingStateType, BpdmSharingStateType.Error)
                .With(x => x.SharingErrorMessage, "This is a test sharing state error")
                .With(x => x.SharingErrorCode, "Code 43")
                .With(x => x.SharingProcessStarted, DateTimeOffset.UtcNow)
                .Create());
        A.CallTo(() => _bpdmService.GetSharingState(IdWithoutSharingProcessStarted, A<CancellationToken>._))
            .Returns(_fixture.Build<BpdmSharingState>()
                .With(x => x.SharingProcessStarted, default(DateTimeOffset?))
                .Create());

        if (company != null)
        {
            A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, A<Action<Company>>._, A<Action<Company>>._))
                .Invokes((Guid _, Action<Company>? initialize, Action<Company> modify) =>
                {
                    initialize?.Invoke(company);
                    modify.Invoke(company);
                });
        }
    }

    #endregion
}
