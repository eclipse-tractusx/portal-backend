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

using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Tests;

public class ClearinghouseBusinessLogicTests
{
    private static readonly Guid IdWithoutBpn = new ("0a9bd7b1-e692-483e-8128-dbf52759c7a5");
    private static readonly Guid IdWithApplicationCreated = new ("7a8f5cb6-6ad2-4b88-a765-ff1888fcedbe");
    private static readonly Guid IdWithCustodianUnavailable = new ("beaa6de5-d411-4da8-850e-06047d3170be");

    private static readonly Guid IdWithBpn = new ("c244f79a-7faf-4c59-bb85-fbfdf72ce46f");
    private const string ValidBpn = "BPNL123698762345";
    private const string ValidDid = "thisisavaliddid";
    private const string FailingBpn = "FAILINGBPN";

    private readonly IFixture _fixture;
    
    private readonly IApplicationRepository _applicationRepository;
    private readonly IApplicationChecklistRepository _applicationChecklistRepository;
    private readonly IPortalRepositories _portalRepositories;
    
    private readonly ClearinghouseBusinessLogic _logic;
    private readonly IClearinghouseService _clearinghouseService;

    public ClearinghouseBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _applicationRepository = A.Fake<IApplicationRepository>();
        _applicationChecklistRepository = A.Fake<IApplicationChecklistRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _clearinghouseService = A.Fake<IClearinghouseService>();

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>()).Returns(_applicationChecklistRepository);

        _logic = new ClearinghouseBusinessLogic(_portalRepositories, _clearinghouseService);
    }
    
    #region ProcessClearinghouseResponse

    [Fact]
    public async Task ProcessClearinghouseResponseAsync_WithoutCompanyForBpn_ThrowsNotFoundException()
    {
        // Arrange
        var bpn = "fakebpn";
        var data = _fixture.Build<ClearinghouseResponseData>()
            .Create();
        SetupForProcessClearinghouseResponse();

        // Act
        async Task Act() => await _logic.ProcessClearinghouseResponseAsync(bpn, data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"No companyApplication for BPN {bpn} is not in status SUBMITTED");
    }

    [Fact]
    public async Task ProcessClearinghouseResponseAsync_WithClearinghouseInTodo_ThrowsConflictException()
    {
        // Arrange
        var data = _fixture.Build<ClearinghouseResponseData>()
            .Create();
        SetupForProcessClearinghouseResponse();

        // Act
        async Task Act() => await _logic.ProcessClearinghouseResponseAsync(FailingBpn, data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Checklist Item {ApplicationChecklistEntryTypeId.CLEARING_HOUSE} is not in status {ApplicationChecklistEntryStatusId.IN_PROGRESS}");
    }
    
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
        await _logic.ProcessClearinghouseResponseAsync(ValidBpn, data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(IdWithBpn, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, A<Action<ApplicationChecklistEntry>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
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
        await _logic.ProcessClearinghouseResponseAsync(ValidBpn, data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(IdWithBpn, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, A<Action<ApplicationChecklistEntry>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.Comment.Should().Be("Comment about the error");
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.FAILED);
    }

    #endregion
    
    #region TriggerCompanyDataPost

    [Fact]
    public async Task TriggerCompanyDataPost_WithNotExistingApplication_ThrowsConflictException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        SetupForTrigger();

        // Act
        async Task Act() => await _logic.TriggerCompanyDataPost(applicationId, ValidDid, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Application {applicationId} does not exists.");
    }

    [Fact]
    public async Task TriggerCompanyDataPost_WithCreatedApplication_ThrowsConflictException()
    {
        // Arrange
        SetupForTrigger();

        // Act
        async Task Act() => await _logic.TriggerCompanyDataPost(IdWithApplicationCreated, ValidDid, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"CompanyApplication {IdWithApplicationCreated} is not in status SUBMITTED");
    }

    [Fact]
    public async Task TriggerCompanyDataPost_WithBpnNull_ThrowsConflictException()
    {
        // Arrange
        SetupForTrigger();

        // Act
        async Task Act() => await _logic.TriggerCompanyDataPost(IdWithoutBpn, ValidDid, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("BusinessPartnerNumber is null");
    }

    [Fact]
    public async Task TriggerCompanyDataPost_WithValidData_CallsExpected()
    {
        // Arrange
        SetupForTrigger();

        // Act
        await _logic.TriggerCompanyDataPost(IdWithBpn, ValidDid, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _clearinghouseService.TriggerCompanyDataPost(A<ClearinghouseTransferData>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion
    
    #region Setup
    
    private void SetupForProcessClearinghouseResponse(ApplicationChecklistEntry? applicationChecklistEntry = null)
    {
        if (applicationChecklistEntry != null)
        {
            SetupForUpdate(applicationChecklistEntry);
        }

        A.CallTo(() => _applicationRepository.GetSubmittedIdAndClearinghouseChecklistStatusByBpn(ValidBpn))
            .ReturnsLazily(() => new ValueTuple<Guid, ApplicationChecklistEntryStatusId>(IdWithBpn, ApplicationChecklistEntryStatusId.IN_PROGRESS));
        A.CallTo(() => _applicationRepository.GetSubmittedIdAndClearinghouseChecklistStatusByBpn(FailingBpn))
            .ReturnsLazily(() => new ValueTuple<Guid, ApplicationChecklistEntryStatusId>(IdWithBpn, ApplicationChecklistEntryStatusId.TO_DO));
        A.CallTo(() => _applicationRepository.GetSubmittedIdAndClearinghouseChecklistStatusByBpn(A<string>.That.Not.Matches(x => x == ValidBpn || x == FailingBpn)))
            .ReturnsLazily(() => new ValueTuple<Guid, ApplicationChecklistEntryStatusId>());
    }

    private void SetupForTrigger()
    {
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
