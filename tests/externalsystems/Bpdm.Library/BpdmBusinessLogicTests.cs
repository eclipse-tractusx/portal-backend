﻿/********************************************************************************
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

using System.Collections.Immutable;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Tests;

public class BpdmBusinessLogicTests
{
    #region Initialization

    private static readonly Guid IdWithBpn = new ("c244f79a-7faf-4c59-bb85-fbfdf72ce46f");
    private static readonly Guid IdWithStateCreated = new ("bda6d1b5-042e-493a-894c-11f3a89c12b1");
    private static readonly Guid IdWithoutZipCode = new ("beaa6de5-d411-4da8-850e-06047d3170be");
    private static readonly Guid ValidCompanyId = new ("abf990f8-0c27-43dc-bbd0-b1bce964d8f4");
    private const string ValidCompanyName = "valid company";

    private readonly IFixture _fixture;
    private readonly IApplicationRepository _applicationRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IBpdmService _bpdmService;
    private readonly BpdmBusinessLogic _logic;

    public BpdmBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var portalRepository = A.Fake<IPortalRepositories>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _bpdmService = A.Fake<IBpdmService>();

        A.CallTo(() => portalRepository.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => portalRepository.GetInstance<ICompanyRepository>()).Returns(_companyRepository);

        _logic = new BpdmBusinessLogic(portalRepository, _bpdmService);
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
        var context = new IChecklistService.WorkerChecklistProcessStepData(IdWithBpn, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithBpn)))
            .ReturnsLazily(() => (ValidCompanyId, null!));

        // Act
        async Task Act() => await _logic.PushLegalEntity(context, CancellationToken.None).ConfigureAwait(false);

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
        var context = new IChecklistService.WorkerChecklistProcessStepData(applicationId, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupFakesForTrigger();

        // Act
        async Task Act() => await _logic.PushLegalEntity(context, CancellationToken.None).ConfigureAwait(false);

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
        var context = new IChecklistService.WorkerChecklistProcessStepData(IdWithoutZipCode, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithoutZipCode)))
            .ReturnsLazily(() => (ValidCompanyId, new BpdmData(ValidCompanyName, null!, "Test", null!, null!, "Test", "test", null!, null!, new List<(BpdmIdentifierId UniqueIdentifierId, string Value)>())));

        // Act
        async Task Act() => await _logic.PushLegalEntity(context, CancellationToken.None).ConfigureAwait(false);

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
        var context = new IChecklistService.WorkerChecklistProcessStepData(IdWithoutZipCode, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithoutZipCode)))
            .ReturnsLazily(() => (ValidCompanyId, new BpdmData(ValidCompanyName, null!, null!, null!, null!, "Test", "test", null!, null!, new List<(BpdmIdentifierId UniqueIdentifierId, string Value)>())));

        // Act
        async Task Act() => await _logic.PushLegalEntity(context, CancellationToken.None).ConfigureAwait(false);

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
        var context = new IChecklistService.WorkerChecklistProcessStepData(IdWithoutZipCode, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithoutZipCode)))
            .ReturnsLazily(() => (ValidCompanyId, new BpdmData(ValidCompanyName, null!, null!, "DE", null!, null, "test", null!, null!, new List<(BpdmIdentifierId UniqueIdentifierId, string Value)>())));

        // Act
        async Task Act() => await _logic.PushLegalEntity(context, CancellationToken.None).ConfigureAwait(false);

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
        var context = new IChecklistService.WorkerChecklistProcessStepData(IdWithoutZipCode, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithoutZipCode)))
            .ReturnsLazily(() => (ValidCompanyId, new BpdmData(ValidCompanyName, null!, null!, "DE", null!, "TEST", null, null!, null!, new List<(BpdmIdentifierId UniqueIdentifierId, string Value)>())));

        // Act
        async Task Act() => await _logic.PushLegalEntity(context, CancellationToken.None).ConfigureAwait(false);

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
        var context = new IChecklistService.WorkerChecklistProcessStepData(IdWithBpn, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupFakesForTrigger();

        // Act
        var result = await _logic.PushLegalEntity(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Item3.Should().BeTrue();
        result.Item1?.Invoke(entry);
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
        var context = new IChecklistService.WorkerChecklistProcessStepData(applicationId, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandlePullLegalEntity();

        // Act
        async Task Act() => await _logic.HandlePullLegalEntity(context, CancellationToken.None).ConfigureAwait(false);

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
        var context = new IChecklistService.WorkerChecklistProcessStepData(IdWithStateCreated, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandlePullLegalEntity();

        // Act
        async Task Act() => await _logic.HandlePullLegalEntity(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"legal-entity not found in bpdm for application {context.ApplicationId}");
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
        var context = new IChecklistService.WorkerChecklistProcessStepData(IdWithoutZipCode, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandlePullLegalEntity();

        // Act
        var result = await _logic.HandlePullLegalEntity(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Item1.Should().BeNull();
        result.Item2.Should().BeNull();
        result.Item3.Should().BeFalse();
    }

    [Fact]
    public async Task HandlePullLegalEntity_WithValidData_ReturnsExpected()
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
        var context = new IChecklistService.WorkerChecklistProcessStepData(IdWithBpn, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandlePullLegalEntity(company);

        // Act
        var result = await _logic.HandlePullLegalEntity(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Item1?.Invoke(checklistEntry);
        checklistEntry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
        result.Item2.Should().ContainSingle().And.Subject.Single().Should().Be(ProcessStepTypeId.CREATE_IDENTITY_WALLET);
        result.Item3.Should().BeTrue();
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
        var context = new IChecklistService.WorkerChecklistProcessStepData(IdWithBpn, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForHandlePullLegalEntity(company);

        // Act
        var result = await _logic.HandlePullLegalEntity(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Item1?.Invoke(checklistEntry);
        checklistEntry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
        result.Item2.Should().BeNull();
        result.Item3.Should().BeTrue();
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
            .With(x => x.BusinessPartnerNumber, (string?)null)
            .Create();

        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithBpn)))
            .ReturnsLazily(() => (ValidCompanyId, validData));
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithStateCreated)))
            .ReturnsLazily(() => (ValidCompanyId, new BpdmData(ValidCompanyName, null!, null!, "DE", null!, "Test", "test", null!, null!, new List<(BpdmIdentifierId UniqueIdentifierId, string Value)>())));
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithoutZipCode)))
            .ReturnsLazily(() => (ValidCompanyId, new BpdmData(ValidCompanyName, null!, null!, null!, null!, "Test", "test", null!, null!, new List<(BpdmIdentifierId UniqueIdentifierId, string Value)>())));
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Not.Matches(x => x == IdWithStateCreated || x == IdWithBpn || x == IdWithoutZipCode)))
            .ReturnsLazily(() => new ValueTuple<Guid, BpdmData>());

        A.CallTo(() => _bpdmService.PutInputLegalEntity(
                A<BpdmTransferData>.That.Matches(x => x.CompanyName == ValidCompanyName && x.ZipCode == "50668"),
                A<CancellationToken>._))
            .ReturnsLazily(() => true);
    }

    private void SetupForHandlePullLegalEntity(Company? company = null)
    {
        var validData = _fixture.Build<BpdmData>()
            .With(x => x.ZipCode, "50668")
            .With(x => x.CompanyName, ValidCompanyName)
            .With(x => x.Alpha2Code, "DE")
            .With(x => x.City, "Test")
            .With(x => x.StreetName, "test")
            .With(x => x.BusinessPartnerNumber, (string?)null)
            .Create();

        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithBpn)))
            .ReturnsLazily(() => (ValidCompanyId, validData));
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithStateCreated)))
            .ReturnsLazily(() => (ValidCompanyId, new BpdmData(ValidCompanyName, null!, null!, "DE", null!, "Test", "test", null!, null!, new List<(BpdmIdentifierId UniqueIdentifierId, string Value)>())));
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Matches(x => x == IdWithoutZipCode)))
            .ReturnsLazily(() => (ValidCompanyId, new BpdmData(ValidCompanyName, null!, null!, null!, null!, "Test", "test", null!, null!, new List<(BpdmIdentifierId UniqueIdentifierId, string Value)>())));
        A.CallTo(() => _applicationRepository.GetBpdmDataForApplicationAsync(A<Guid>.That.Not.Matches(x => x == IdWithStateCreated || x == IdWithBpn || x == IdWithoutZipCode)))
            .ReturnsLazily(() => new ValueTuple<Guid, BpdmData>());

        A.CallTo(() => _bpdmService.FetchInputLegalEntity(A<string>.That.Matches(x => x == IdWithStateCreated.ToString()), A<CancellationToken>._))
            .ReturnsLazily(() => (BpdmLegalEntityData?) null);
        A.CallTo(() => _bpdmService.FetchInputLegalEntity(A<string>.That.Matches(x => x == IdWithoutZipCode.ToString()), A<CancellationToken>._))
            .ReturnsLazily(() => _fixture.Build<BpdmLegalEntityData>().With(x => x.Bpn, (string?)null).Create());
        A.CallTo(() => _bpdmService.FetchInputLegalEntity(A<string>.That.Matches(x => x == IdWithBpn.ToString()), A<CancellationToken>._))
            .ReturnsLazily(() => _fixture.Build<BpdmLegalEntityData>().With(x => x.Bpn, "CAXSDUMMYCATENAZZ").Create());

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
