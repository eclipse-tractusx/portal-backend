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
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.BusinessLogic;
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
    private static readonly Guid NotExistingApplicationId = new ("9f0cfd0d-c512-438e-a07e-3198bce873bf");
    private static readonly string IamUserId = new Guid("4C1A6851-D4E7-4E10-A011-3732CD045E8A").ToString();
    private static readonly Guid CompanyId = new("95c4339e-e087-4cd2-a5b8-44d385e64630");
    private const string ValidBpn = "BPNL123698762345";
    private const string ValidCompanyName = "valid company";

    private readonly IFixture _fixture;
    
    private readonly IApplicationRepository _applicationRepository;
    private readonly IApplicationChecklistRepository _applicationChecklistRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPortalRepositories _portalRepositories;
    
    private readonly IBpdmBusinessLogic _bpdmBusinessLogic;
    private readonly ICustodianBusinessLogic _custodianBusinessLogic;
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

        _bpdmBusinessLogic = A.Fake<IBpdmBusinessLogic>();
        _custodianBusinessLogic = A.Fake<ICustodianBusinessLogic>();

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>()).Returns(_applicationChecklistRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);

        _service = new ChecklistService(_portalRepositories, _bpdmBusinessLogic, _custodianBusinessLogic, A.Fake<ILogger<IChecklistService>>());
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
        A.CallTo(() => _bpdmBusinessLogic.TriggerBpnDataPush(IdWithoutBpn, IamUserId, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(IdWithoutBpn, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, A<Action<ApplicationChecklistEntry>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.IN_PROGRESS);
    }

    [Fact]
    public async Task TriggerBpnDataPush_WithValidDataAndFailingBpdmServiceCall_ThrowsExceptionAndDoesntUpdateEntry()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithoutBpn, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        A.CallTo(() => _bpdmBusinessLogic.TriggerBpnDataPush(IdWithoutBpn, IamUserId, CancellationToken.None))
            .Throws(new ServiceException("Bpdm Service Call failed."));
        
        SetupFakesForTrigger(entry);

        // Act
        async Task Act() => await _service.TriggerBpnDataPush(IdWithoutBpn, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<ServiceException>(Act);
        A.CallTo(() => _bpdmBusinessLogic.TriggerBpnDataPush(IdWithoutBpn, IamUserId, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(IdWithoutBpn, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, A<Action<ApplicationChecklistEntry>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.TO_DO);
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
        A.CallTo(() => _bpdmBusinessLogic.TriggerBpnDataPush(IdWithBpn, IamUserId, A<CancellationToken>._)).MustNotHaveHappened();
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
        A.CallTo(() => _custodianBusinessLogic.CreateWalletAsync(A<Guid>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        entry.Comment.Should().Contain("Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.ServiceException: Failed");
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.FAILED);
    }
    
    [Fact]
    public async Task ProcessChecklist_WithCustodianUnavailable_EntryIsUpdatedCorrectly()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithCustodianUnavailable, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
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
        await _service.ProcessChecklist(IdWithCustodianUnavailable, checklist, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _custodianBusinessLogic.CreateWalletAsync(A<Guid>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, A<ApplicationChecklistEntryTypeId>._, A<Action<ApplicationChecklistEntry>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        entry.Comment.Should().Contain("Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.ServiceException: Failed");
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.TO_DO);
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
        A.CallTo(() => _custodianBusinessLogic.CreateWalletAsync(IdWithBpn, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(IdWithBpn, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, A<Action<ApplicationChecklistEntry>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
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
            .ReturnsLazily(() => new List<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>
            {
                new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO),
            }.ToAsyncEnumerable());

        A.CallTo(() => _applicationChecklistRepository.GetChecklistDataAsync(IdWithBpn))
            .ReturnsLazily(() => new List<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>().ToAsyncEnumerable());
    }

    private void SetupForCreateWallet(ApplicationChecklistEntry? applicationChecklistEntry = null)
    {
        if (applicationChecklistEntry != null)
        {
            SetupForUpdate(applicationChecklistEntry);
        }

        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForCreateWalletAsync(IdWithoutBpn))
            .Returns(new ValueTuple<Guid, string, string?>(Guid.NewGuid(), ValidCompanyName, null));
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForCreateWalletAsync(IdWithBpn))
            .Returns(new ValueTuple<Guid, string, string?>(CompanyId, ValidCompanyName, ValidBpn));
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForCreateWalletAsync(IdWithFailingCustodian))
            .Returns(new ValueTuple<Guid, string, string?>(Guid.NewGuid(), "a company", "bpnNotValidForWallet"));
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForCreateWalletAsync(IdWithCustodianUnavailable))
            .Returns(new ValueTuple<Guid, string, string?>(Guid.NewGuid(), "a company", "custodiaNotAvailable"));

        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForCreateWalletAsync(A<Guid>.That.Not.Matches(x => x == IdWithBpn || x == IdWithoutBpn || x == IdWithFailingCustodian || x == IdWithCustodianUnavailable)))
            .Returns(((Guid, string, string))default);

        A.CallTo(() => _custodianBusinessLogic.CreateWalletAsync(IdWithBpn, CancellationToken.None))
            .ReturnsLazily(() => "It worked.");
        A.CallTo(() => _custodianBusinessLogic.CreateWalletAsync(IdWithFailingCustodian, CancellationToken.None))
            .Throws(new ServiceException("Failed"));
        A.CallTo(() => _custodianBusinessLogic.CreateWalletAsync(IdWithCustodianUnavailable, CancellationToken.None))
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
