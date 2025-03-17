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
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Tests;

public class ClearinghouseBusinessLogicTests
{
    private static readonly Guid IdWithoutCountryCodeVatId = new("0a9bd7b1-e692-483e-8128-dbf52759c7a5");
    private static readonly Guid IdWithoutBpn = new("0a9bd7b1-e692-483e-8128-dbf52759c7a5");
    private static readonly Guid IdWithApplicationCreated = new("7a8f5cb6-6ad2-4b88-a765-ff1888fcedbe");
    private static readonly Guid IdWithCustodianUnavailable = new("beaa6de5-d411-4da8-850e-06047d3170be");

    private static readonly Guid IdWithBpn = new("c244f79a-7faf-4c59-bb85-fbfdf72ce46f");
    private const string ValidBpn = "BPNL123698762345";
    private readonly IFixture _fixture;

    private readonly IApplicationRepository _applicationRepository;
    private readonly IPortalRepositories _portalRepositories;

    private readonly ClearinghouseBusinessLogic _logic;
    private readonly IClearinghouseService _clearinghouseService;
    private readonly IApplicationChecklistService _checklistService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IApplicationChecklistRepository _applicationChecklistRepository;

    public ClearinghouseBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _applicationRepository = A.Fake<IApplicationRepository>();
        _applicationChecklistRepository = A.Fake<IApplicationChecklistRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _clearinghouseService = A.Fake<IClearinghouseService>();
        _checklistService = A.Fake<IApplicationChecklistService>();

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>()).Returns(_applicationChecklistRepository);

        _dateTimeProvider = A.Fake<IDateTimeProvider>();
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(DateTimeOffset.UtcNow);

        _logic = new ClearinghouseBusinessLogic(_portalRepositories, _clearinghouseService, _checklistService, _dateTimeProvider, Options.Create(new ClearinghouseSettings
        {
            CallbackUrl = "https://api.com",
            UseDimWallet = false
        }));
    }

    #region HandleStartClearingHouse

    [Theory]
    [InlineData(ProcessStepTypeId.MANUAL_VERIFY_REGISTRATION)]
    [InlineData(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH)]
    [InlineData(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL)]
    [InlineData(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL)]
    [InlineData(ProcessStepTypeId.CREATE_IDENTITY_WALLET)]
    [InlineData(ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET)]
    [InlineData(ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE)]
    [InlineData(ProcessStepTypeId.AWAIT_CLEARING_HOUSE_RESPONSE)]
    [InlineData(ProcessStepTypeId.START_SELF_DESCRIPTION_LP)]
    [InlineData(ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP)]
    [InlineData(ProcessStepTypeId.ASSIGN_INITIAL_ROLES)]
    [InlineData(ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH)]
    [InlineData(ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL)]
    [InlineData(ProcessStepTypeId.MANUAL_TRIGGER_OVERRIDE_CLEARING_HOUSE)]
    [InlineData(ProcessStepTypeId.AWAIT_SELF_DESCRIPTION_LP_RESPONSE)]
    public async Task HandleStartClearingHouse_ForInvalidProcessStepTypeId_ThrowsUnexpectedCondition(ProcessStepTypeId stepTypeId)
    {
        // Arrange
        var checklist = _fixture.Create<Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>>().ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(Guid.NewGuid(), stepTypeId, checklist, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        async Task Act() => await _logic.HandleClearinghouse(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be($"HandleClearingHouse called for unexpected processStepTypeId {stepTypeId}. Expected START_CLEARING_HOUSE or START_OVERRIDE_CLEARING_HOUSE");
    }

    [Fact]
    public async Task HandleStartClearingHouse_WithNotExistingApplication_ThrowsConflictException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();

        var checklist = ImmutableDictionary.CreateRange<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>([
            new(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
            new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE),
            new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE),
            new(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO)
        ]);

        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(applicationId, ProcessStepTypeId.START_CLEARING_HOUSE, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandleStartClearingHouse();

        // Act
        async Task Act() => await _logic.HandleClearinghouse(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Application {context.ApplicationId} does not exists.");
    }

    [Fact]
    public async Task HandleStartClearingHouse_WithCreatedApplication_ThrowsConflictException()
    {
        // Arrange
        var checklist = ImmutableDictionary.CreateRange<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>([
            new(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
            new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE),
            new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE),
            new(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO)
        ]);

        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithApplicationCreated, ProcessStepTypeId.START_CLEARING_HOUSE, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandleStartClearingHouse();

        // Act
        async Task Act() => await _logic.HandleClearinghouse(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"CompanyApplication {IdWithApplicationCreated} is not in status SUBMITTED");
    }

    [Fact]
    public async Task HandleStartClearingHouse_WithBpnNull_ThrowsConflictException()
    {
        // Arrange
        var checklist = ImmutableDictionary.CreateRange<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>([
            new(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
            new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE),
            new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE),
            new(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO)
        ]);

        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithoutBpn, ProcessStepTypeId.START_CLEARING_HOUSE, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandleStartClearingHouse();

        // Act
        async Task Act() => await _logic.HandleClearinghouse(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("BusinessPartnerNumber is null");
    }

    [Theory]
    [InlineData(UniqueIdentifierId.VAT_ID, "DE123456789")]
    [InlineData(UniqueIdentifierId.VAT_ID, "123456789")]
    [InlineData(UniqueIdentifierId.COMMERCIAL_REG_NUMBER, "HRB123456")]
    public async Task HandleStartClearingHouse_WithoutContryCodeVatId_ThrowsConflictException(UniqueIdentifierId uniqueIdentifierId, string vatId)
    {
        // Arrange
        var checklist = ImmutableDictionary.CreateRange<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>([
            new(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
            new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE),
            new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE),
            new(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO)
        ]);
        var addressData = _fixture.Build<ClearinghouseAddressData>()
            .With(x => x.CountryAlpha2Code, "DE")
            .Create();
        var dataWithoutCountryCodeVatId = _fixture.Build<ClearinghouseData>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .With(x => x.Address, addressData)
            .With(x => x.Bpn, ValidBpn)
            .With(x => x.Identifiers, [new CompanyUniqueIdentifier(uniqueIdentifierId, vatId)])
            .Create();

        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithoutCountryCodeVatId, ProcessStepTypeId.START_CLEARING_HOUSE, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetClearinghouseDataForApplicationId(IdWithoutCountryCodeVatId))
            .Returns(dataWithoutCountryCodeVatId);

        var hearders = ImmutableDictionary.CreateRange<string, string>([new("Business-Partner-Number", ValidBpn)]);
        var transferData = new ClearinghouseTransferData(
            new LegalEntity(
                dataWithoutCountryCodeVatId.Name,
                new LegalAddress(
                    dataWithoutCountryCodeVatId.Address!.CountryAlpha2Code,
                    string.Format("{0}-{1}", dataWithoutCountryCodeVatId.Address.CountryAlpha2Code, dataWithoutCountryCodeVatId.Address.Region),
                    dataWithoutCountryCodeVatId.Address.City,
                    dataWithoutCountryCodeVatId.Address.Zipcode,
                    string.IsNullOrEmpty(dataWithoutCountryCodeVatId.Address.Streetnumber)
                        ? dataWithoutCountryCodeVatId.Address.Streetname
                        : string.Format("{0} {1}", dataWithoutCountryCodeVatId.Address.Streetname, dataWithoutCountryCodeVatId.Address.Streetnumber)),
                dataWithoutCountryCodeVatId.Identifiers.Select(ci =>
                    new UniqueIdData(
                        ci.UniqueIdentifierId.GetUniqueIdentifierId(),
                        ci.Value.GetUniqueIdentifierValue(ci.UniqueIdentifierId, dataWithoutCountryCodeVatId.Address.CountryAlpha2Code))
                    )),
            ValidationModes.LEGAL_NAME,
            new CallBack("https://www.test.de", hearders)
        );
        A.CallTo(() => _clearinghouseService.TriggerCompanyDataPost(transferData, A<CancellationToken>._));

        // Act
        var result = await _logic.HandleClearinghouse(context, CancellationToken.None);

        // Assert
        result.ModifyChecklistEntry.Should().NotBeNull();
        if (uniqueIdentifierId == UniqueIdentifierId.VAT_ID)
        {
            transferData.LegalEntity.Identifiers.Should().ContainSingle().And.Satisfy(
                p => p.Type == "schema:vatID" && p.Value == "DE123456789");
        }
        else if (uniqueIdentifierId == UniqueIdentifierId.COMMERCIAL_REG_NUMBER)
        {
            transferData.LegalEntity.Identifiers.Should().ContainSingle().And.Satisfy(
                p => p.Type == "schema:taxID" && p.Value == "HRB123456");
        }
    }

    [Theory]
    [InlineData(ProcessStepTypeId.START_CLEARING_HOUSE, ApplicationChecklistEntryStatusId.IN_PROGRESS, ProcessStepTypeId.AWAIT_CLEARING_HOUSE_RESPONSE)]
    [InlineData(ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE, ApplicationChecklistEntryStatusId.IN_PROGRESS, ProcessStepTypeId.AWAIT_CLEARING_HOUSE_RESPONSE)]
    public async Task HandleStartClearingHouse_WithValidData_CallsExpected(ProcessStepTypeId stepTypeId, ApplicationChecklistEntryStatusId statusId, ProcessStepTypeId expectedProcessTypeId)
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(Guid.NewGuid(), ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);

        var checklist = ImmutableDictionary.CreateRange<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>([
            new(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
            new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE),
            new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE),
            new(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO)
        ]);

        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithBpn, stepTypeId, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandleStartClearingHouse();

        // Act
        var result = await _logic.HandleClearinghouse(context, CancellationToken.None);

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
        var reasonMessage = _fixture.Build<Reason>()
            .With(x => x.DetailMessage, default(string?))
            .Create();

        var validationUnits = _fixture.Build<ValidationUnits>()
            .With(x => x.Status, ClearinghouseResponseStatus.VALID)
            .With(x => x.Reason, reasonMessage)
            .CreateMany(2);

        var data = _fixture.Build<ClearinghouseResponseData>()
            .With(x => x.ValidationUnits, validationUnits)
            .Create();

        SetupForProcessClearinghouseResponse(entry);

        // Act
        await _logic.ProcessEndClearinghouse(IdWithBpn, data, CancellationToken.None);

        // Assert
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count(y => y == ProcessStepTypeId.START_SELF_DESCRIPTION_LP) == 1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        entry.Comment.Should().Be("VALID");
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
    }

    [Fact]
    public async Task ProcessClearinghouseResponseAsync_WithDecline_UpdatesEntry()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithBpn, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var reasonMessage = _fixture.Build<Reason>()
            .With(x => x.DetailMessage, "Comment about the error")
            .Create();

        var validationUnits = _fixture.Build<ValidationUnits>()
            .With(x => x.Status, ClearinghouseResponseStatus.INVALID)
            .With(x => x.Reason, reasonMessage)
            .CreateMany(2);

        var data = _fixture.Build<ClearinghouseResponseData>()
            .With(x => x.ValidationUnits, validationUnits)
            .Create();
        SetupForProcessClearinghouseResponse(entry);

        // Act
        await _logic.ProcessEndClearinghouse(IdWithBpn, data, CancellationToken.None);

        // Assert
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count(y => y == ProcessStepTypeId.MANUAL_TRIGGER_OVERRIDE_CLEARING_HOUSE) == 1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        entry.Comment.Should().Be("Comment about the error, Comment about the error");
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.FAILED);
    }

    #endregion

    #region CheckEndClearinghouseProcesses

    [Fact]
    public async Task CheckEndClearinghouseProcesses_WithEntry_CreatesProcessStep()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithBpn, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        A.CallTo(() => _applicationChecklistRepository.GetApplicationsForClearinghouseRetrigger(A<DateTimeOffset>._))
            .Returns(Enumerable.Repeat(IdWithBpn, 1).ToAsyncEnumerable());
        SetupForCheckEndClearinghouseProcesses(Enumerable.Repeat(entry, 1));

        // Act
        await _logic.CheckEndClearinghouseProcesses(CancellationToken.None);

        // Assert
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count(y => y == ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE) == 1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.Comment.Should().Be("Reset to retrigger clearinghouse");
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.TO_DO);
    }

    #endregion

    #region Setup

    private void SetupForHandleStartClearingHouse()
    {
        var addressData = _fixture.Build<ClearinghouseAddressData>()
            .Create();
        var clearinghouseDataWithoutBpn = _fixture.Build<ClearinghouseData>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .With(x => x.Address, addressData)
            .With(x => x.Bpn, default(string?))
            .Create();
        var clearinghouseData = _fixture.Build<ClearinghouseData>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .With(x => x.Address, addressData)
            .With(x => x.Bpn, ValidBpn)
            .Create();
        var chDataWithApplicationCreated = _fixture.Build<ClearinghouseData>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.CREATED)
            .Create();

        A.CallTo(() => _applicationRepository.GetClearinghouseDataForApplicationId(IdWithoutBpn))
            .Returns(clearinghouseDataWithoutBpn);
        A.CallTo(() => _applicationRepository.GetClearinghouseDataForApplicationId(IdWithBpn))
            .Returns(clearinghouseData);
        A.CallTo(() => _applicationRepository.GetClearinghouseDataForApplicationId(IdWithApplicationCreated))
            .Returns(chDataWithApplicationCreated);
        A.CallTo(() => _applicationRepository.GetClearinghouseDataForApplicationId(A<Guid>.That.Not.Matches(x => x == IdWithoutBpn || x == IdWithBpn || x == IdWithApplicationCreated || x == IdWithCustodianUnavailable)))
            .Returns<ClearinghouseData?>(null);
    }

    private void SetupForProcessClearinghouseResponse(ApplicationChecklistEntry applicationChecklistEntry)
    {
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Invokes((IApplicationChecklistService.ManualChecklistProcessStepData _, Action<ApplicationChecklistEntry> initialApplicationChecklistEntry, Action<ApplicationChecklistEntry> modifyApplicationChecklistEntry, IEnumerable<ProcessStepTypeId> _) =>
            {
                applicationChecklistEntry.DateLastChanged = DateTimeOffset.UtcNow;
                modifyApplicationChecklistEntry.Invoke(applicationChecklistEntry);
            });

        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(
                IdWithBpn,
                ApplicationChecklistEntryTypeId.CLEARING_HOUSE,
                A<IEnumerable<ApplicationChecklistEntryStatusId>>._,
                ProcessStepTypeId.AWAIT_CLEARING_HOUSE_RESPONSE,
                A<IEnumerable<ApplicationChecklistEntryTypeId>?>._,
                A<IEnumerable<ProcessStepTypeId>?>._))
            .Returns(new IApplicationChecklistService.ManualChecklistProcessStepData(Guid.Empty, new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid()), Guid.Empty, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ImmutableDictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>.Empty, new List<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>()));
    }

    private void SetupForCheckEndClearinghouseProcesses(IEnumerable<ApplicationChecklistEntry> applicationChecklistEntries)
    {
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Invokes((IApplicationChecklistService.ManualChecklistProcessStepData data, Action<ApplicationChecklistEntry>? _, Action<ApplicationChecklistEntry> modifyApplicationChecklistEntry, IEnumerable<ProcessStepTypeId> _) =>
            {
                var entry = applicationChecklistEntries.SingleOrDefault(x => x.ApplicationId == data.ApplicationId);
                if (entry == null)
                    return;

                entry.DateLastChanged = DateTimeOffset.UtcNow;
                modifyApplicationChecklistEntry(entry);
            });

        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(
                A<Guid>._,
                ApplicationChecklistEntryTypeId.CLEARING_HOUSE,
                A<IEnumerable<ApplicationChecklistEntryStatusId>>._,
                ProcessStepTypeId.AWAIT_CLEARING_HOUSE_RESPONSE,
                A<IEnumerable<ApplicationChecklistEntryTypeId>?>._,
                A<IEnumerable<ProcessStepTypeId>?>._))
            .ReturnsLazily((Guid id,
                ApplicationChecklistEntryTypeId _,
                IEnumerable<ApplicationChecklistEntryStatusId> _,
                ProcessStepTypeId _,
                IEnumerable<ApplicationChecklistEntryTypeId>? _,
                IEnumerable<ProcessStepTypeId>? _) => new IApplicationChecklistService.ManualChecklistProcessStepData(
                id,
                new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid()),
                Guid.Empty,
                ApplicationChecklistEntryTypeId.CLEARING_HOUSE,
                ImmutableDictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>.Empty,
                []));
    }

    #endregion
}
