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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Tests;

public class ClearinghouseBusinessLogicTests
{
    private static readonly Guid IdWithoutBpn = new ("0a9bd7b1-e692-483e-8128-dbf52759c7a5");
    private static readonly Guid IdWithApplicationCreated = new ("7a8f5cb6-6ad2-4b88-a765-ff1888fcedbe");
    private static readonly Guid IdWithCustodianUnavailable = new ("beaa6de5-d411-4da8-850e-06047d3170be");

    private static readonly Guid IdWithBpn = new ("c244f79a-7faf-4c59-bb85-fbfdf72ce46f");
    private const string ValidBpn = "BPNL123698762345";
    private const string ValidDid = "thisisavaliddid";

    private readonly IFixture _fixture;
    
    private readonly IApplicationRepository _applicationRepository;
    private readonly IPortalRepositories _portalRepositories;
    
    private readonly ClearinghouseBusinessLogic _logic;
    private readonly IClearinghouseService _clearinghouseService;
    private readonly IApplicationChecklistService _checklistService;
    private readonly ICustodianBusinessLogic _custodianBusinessLogic;

    public ClearinghouseBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _applicationRepository = A.Fake<IApplicationRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _clearinghouseService = A.Fake<IClearinghouseService>();
        _custodianBusinessLogic = A.Fake<ICustodianBusinessLogic>();
        _checklistService = A.Fake<IApplicationChecklistService>();

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);

        _logic = new ClearinghouseBusinessLogic(_portalRepositories, _clearinghouseService, _custodianBusinessLogic, _checklistService, Options.Create(new ClearinghouseSettings
        {
            CallbackUrl = "https://api.com"
        }));
    }
    
    #region HandleStartClearingHouse

    [Theory]
    [InlineData(ProcessStepTypeId.VERIFY_REGISTRATION)]
    [InlineData(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH)]
    [InlineData(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL)]
    [InlineData(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL)]
    [InlineData(ProcessStepTypeId.CREATE_IDENTITY_WALLET)]
    [InlineData(ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET)]
    [InlineData(ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE)]
    [InlineData(ProcessStepTypeId.END_CLEARING_HOUSE)]
    [InlineData(ProcessStepTypeId.START_SELF_DESCRIPTION_LP)]
    [InlineData(ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP)]
    [InlineData(ProcessStepTypeId.ACTIVATE_APPLICATION)]
    [InlineData(ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH)]
    [InlineData(ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL)]
    [InlineData(ProcessStepTypeId.OVERRIDE_BUSINESS_PARTNER_NUMBER)]
    [InlineData(ProcessStepTypeId.TRIGGER_OVERRIDE_CLEARING_HOUSE)]
    [InlineData(ProcessStepTypeId.FINISH_SELF_DESCRIPTION_LP)]

    public async Task HandleStartClearingHouse_ForInvalidProcessStepTypeId_ThrowsUnexpectedCondition(ProcessStepTypeId stepTypeId)
    {
        // Arrange
        var checklist = _fixture.Create<Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>>().ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(Guid.NewGuid(), stepTypeId, checklist, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        async Task Act() => await _logic.HandleClearinghouse(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be($"HandleClearingHouse called for unexpected processStepTypeId {stepTypeId}. Expected START_CLEARING_HOUSE or START_OVERRIDE_CLEARING_HOUSE");
    }

    [Fact]
    public async Task HandleStartClearingHouse_WithNotExistingApplication_ThrowsConflictException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(applicationId, ProcessStepTypeId.START_CLEARING_HOUSE, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandleStartClearingHouse();

        // Act
        async Task Act() => await _logic.HandleClearinghouse(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Decentralized Identifier for application {context.ApplicationId} is not set");
    }

    [Fact]
    public async Task HandleStartClearingHouse_WithCreatedApplication_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithApplicationCreated, ProcessStepTypeId.START_CLEARING_HOUSE, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandleStartClearingHouse();

        // Act
        async Task Act() => await _logic.HandleClearinghouse(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"CompanyApplication {IdWithApplicationCreated} is not in status SUBMITTED");
    }

    [Fact]
    public async Task HandleStartClearingHouse_WithBpnNull_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithoutBpn, ProcessStepTypeId.START_CLEARING_HOUSE, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandleStartClearingHouse();

        // Act
        async Task Act() => await _logic.HandleClearinghouse(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("BusinessPartnerNumber is null");
    }

    [Theory]
    [InlineData(ProcessStepTypeId.START_CLEARING_HOUSE, ApplicationChecklistEntryStatusId.IN_PROGRESS, ProcessStepTypeId.END_CLEARING_HOUSE)]
    [InlineData(ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE, ApplicationChecklistEntryStatusId.IN_PROGRESS, ProcessStepTypeId.END_CLEARING_HOUSE)]
    public async Task HandleStartClearingHouse_WithValidData_CallsExpected(ProcessStepTypeId stepTypeId, ApplicationChecklistEntryStatusId statusId, ProcessStepTypeId expectedProcessTypeId)
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(Guid.NewGuid(), ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithBpn, stepTypeId, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandleStartClearingHouse();

        // Act
        var result = await _logic.HandleClearinghouse(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.ModifyChecklistEntry.Should().NotBeNull();
        result.ModifyChecklistEntry!.Invoke(entry);
        entry.ApplicationChecklistEntryStatusId.Should().Be(statusId);
        A.CallTo(() => _clearinghouseService.TriggerCompanyDataPost(A<ClearinghouseTransferData>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        result.ScheduleStepTypeIds.Should().HaveCount(1);
        result.ScheduleStepTypeIds.Should().Contain(expectedProcessTypeId);
        result.SkipStepTypeIds.Should().BeNull();
        result.Modified.Should().BeTrue();
    }

    #endregion
    
    #region ProcessClearinghouseResponse

    [Fact]
    public async Task ProcessClearinghouseResponseAsync_WithConfirmation_UpdatesEntry()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithBpn, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var data = _fixture.Build<ClearinghouseResponseData>()
            .With(x => x.Status, ClearinghouseResponseStatus.CONFIRM)
            .With(x => x.Message, (string?)null)
            .Create();
        SetupForProcessClearinghouseResponse(entry);

        // Act
        await _logic.ProcessEndClearinghouse(IdWithBpn, data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count(y => y == ProcessStepTypeId.START_SELF_DESCRIPTION_LP) == 1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        entry.Comment.Should().BeNull();
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
    }

    [Fact]
    public async Task ProcessClearinghouseResponseAsync_WithDecline_UpdatesEntry()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithBpn, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var data = _fixture.Build<ClearinghouseResponseData>()
            .With(x => x.Status, ClearinghouseResponseStatus.DECLINE)
            .With(x => x.Message, "Comment about the error")
            .Create();
        SetupForProcessClearinghouseResponse(entry);

        // Act
        await _logic.ProcessEndClearinghouse(IdWithBpn, data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count(y => y == ProcessStepTypeId.TRIGGER_OVERRIDE_CLEARING_HOUSE) == 1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        entry.Comment.Should().Be("Comment about the error");
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.FAILED);
    }

    #endregion
    
    #region Setup
    
    private void SetupForHandleStartClearingHouse()
    {
        A.CallTo(() => _custodianBusinessLogic.GetWalletByBpnAsync(A<Guid>.That.Matches(x => x == IdWithoutBpn || x == IdWithBpn || x == IdWithApplicationCreated), A<CancellationToken>._))
            .ReturnsLazily(() => new WalletData("Name", ValidBpn, ValidDid, DateTime.UtcNow, false, null));
        A.CallTo(() => _custodianBusinessLogic.GetWalletByBpnAsync(IdWithCustodianUnavailable, A<CancellationToken>._))
            .ReturnsLazily(() => (WalletData?)null);
        A.CallTo(() => _custodianBusinessLogic.GetWalletByBpnAsync(A<Guid>.That.Not.Matches(x => x == IdWithoutBpn || x == IdWithBpn || x == IdWithApplicationCreated || x== IdWithCustodianUnavailable), A<CancellationToken>._))
            .ReturnsLazily(() => new WalletData("Name", ValidBpn, null, DateTime.UtcNow, false, null));

        var participantDetailsWithoutBpn = _fixture.Build<ParticipantDetails>()
            .With(x => x.Bpn, (string?)null)
            .Create();
        var clearinghouseDataWithoutBpn = _fixture.Build<ClearinghouseData>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .With(x => x.ParticipantDetails, participantDetailsWithoutBpn)
            .Create();
        var participantDetails = _fixture.Build<ParticipantDetails>()
            .With(x => x.Bpn, ValidBpn)
            .Create();
        var clearinghouseData = _fixture.Build<ClearinghouseData>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .With(x => x.ParticipantDetails, participantDetails)
            .Create();
        var chDataWithApplicationCreated = _fixture.Build<ClearinghouseData>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.CREATED)
            .Create();

        A.CallTo(() => _applicationRepository.GetClearinghouseDataForApplicationId(IdWithoutBpn))
            .ReturnsLazily(() => clearinghouseDataWithoutBpn);
        A.CallTo(() => _applicationRepository.GetClearinghouseDataForApplicationId(IdWithBpn))
            .ReturnsLazily(() => clearinghouseData);
        A.CallTo(() => _applicationRepository.GetClearinghouseDataForApplicationId(IdWithApplicationCreated))
            .ReturnsLazily(() => chDataWithApplicationCreated);
        A.CallTo(() => _applicationRepository.GetClearinghouseDataForApplicationId(A<Guid>.That.Not.Matches(x => x == IdWithoutBpn || x == IdWithBpn || x == IdWithApplicationCreated || x == IdWithCustodianUnavailable)))
            .ReturnsLazily(() => (ClearinghouseData?)null);
    }

    private void SetupForProcessClearinghouseResponse(ApplicationChecklistEntry applicationChecklistEntry)
    {
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Invokes((IApplicationChecklistService.ManualChecklistProcessStepData _, Action<ApplicationChecklistEntry> modifyApplicationChecklistEntry, IEnumerable<ProcessStepTypeId> _) =>
            {
                applicationChecklistEntry.DateLastChanged = DateTimeOffset.UtcNow;
                modifyApplicationChecklistEntry.Invoke(applicationChecklistEntry);
            });

        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(
                IdWithBpn, 
                ApplicationChecklistEntryTypeId.CLEARING_HOUSE, 
                A<IEnumerable<ApplicationChecklistEntryStatusId>>._, 
                ProcessStepTypeId.END_CLEARING_HOUSE, 
                A<IEnumerable<ApplicationChecklistEntryTypeId>?>._,
                A<IEnumerable<ProcessStepTypeId>?>._))
            .Returns(new IApplicationChecklistService.ManualChecklistProcessStepData(Guid.Empty, new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid()), Guid.Empty, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ImmutableDictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>.Empty, new List<ProcessStep>()));
    }

    #endregion
}
