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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests.BusinessLogic;

public class NetworkBusinessLogicTests
{
    private const string Bpnl = "BPNL00000001TEST";
    private static readonly string ExistingExternalId = Guid.NewGuid().ToString();
    private static readonly Guid UserRoleId = Guid.NewGuid();
    private static readonly Guid MultiIdpCompanyId = Guid.NewGuid();
    private static readonly Guid NoIdpCompanyId = Guid.NewGuid();
    private static readonly Guid IdpId = Guid.NewGuid();
    private static readonly Guid NoAliasIdpCompanyId = Guid.NewGuid();

    private readonly IFixture _fixture;

    private readonly IdentityData _identity = new(Guid.NewGuid().ToString(), Guid.NewGuid(), IdentityTypeId.COMPANY_USER, Guid.NewGuid());
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IApplicationChecklistCreationService _checklistService;

    private readonly IPortalRepositories _portalRepositories;
    private readonly ICompanyRepository _companyRepository;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly INetworkRepository _networkRepository;
    private readonly IIdentityProviderRepository _identityProviderRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly NetworkBusinessLogic _sut;
    private readonly IConsentRepository _consentRepository;

    public NetworkBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _userProvisioningService = A.Fake<IUserProvisioningService>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _checklistService = A.Fake<IApplicationChecklistCreationService>();

        _companyRepository = A.Fake<ICompanyRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _networkRepository = A.Fake<INetworkRepository>();
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();
        _countryRepository = A.Fake<ICountryRepository>();
        _consentRepository = A.Fake<IConsentRepository>();

        var identityService = A.Fake<IIdentityService>();
        A.CallTo(() => identityService.IdentityData).Returns(_identity);
        A.CallTo(() => identityService.IdentityId).Returns(_identity.UserId);

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentRepository>()).Returns(_consentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INetworkRepository>()).Returns(_networkRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICountryRepository>()).Returns(_countryRepository);

        _sut = new NetworkBusinessLogic(_portalRepositories, identityService, _checklistService);

        SetupRepos();
    }

    #region Submit

    [Fact]
    public async Task Submit_WithNotExistingSubmitData_ThrowsNotFoundException()
    {
        // Arrange
        var data = _fixture.Create<PartnerSubmitData>();
        A.CallTo(() => _networkRepository.GetSubmitData(_identity.CompanyId))
            .Returns(new ValueTuple<bool, IEnumerable<ValueTuple<Guid, CompanyApplicationStatusId, string?>>, IEnumerable<ValueTuple<CompanyRoleId, IEnumerable<Guid>>>, Guid?>());

        // Act
        async Task Act() => await _sut.Submit(data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Company {_identity.CompanyId} not found");
    }

    [Fact]
    public async Task Submit_WithoutCompanyApplications_ThrowsConflictException()
    {
        // Arrange
        var data = _fixture.Create<PartnerSubmitData>();
        A.CallTo(() => _networkRepository.GetSubmitData(_identity.CompanyId))
            .Returns((true, Enumerable.Empty<ValueTuple<Guid, CompanyApplicationStatusId, string?>>(), Enumerable.Empty<ValueTuple<CompanyRoleId, IEnumerable<Guid>>>(), null));

        // Act
        async Task Act() => await _sut.Submit(data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Company {_identity.CompanyId} has no or more than one application");
    }

    [Fact]
    public async Task Submit_WithMultipleCompanyApplications_ThrowsConflictException()
    {
        // Arrange
        var data = _fixture.Create<PartnerSubmitData>();
        A.CallTo(() => _networkRepository.GetSubmitData(_identity.CompanyId))
            .Returns((true, _fixture.CreateMany<ValueTuple<Guid, CompanyApplicationStatusId, string?>>(2), Enumerable.Empty<ValueTuple<CompanyRoleId, IEnumerable<Guid>>>(), null));

        // Act
        async Task Act() => await _sut.Submit(data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Company {_identity.CompanyId} has no or more than one application");
    }

    [Fact]
    public async Task Submit_WithWrongApplicationStatus_ThrowsConflictException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var data = _fixture.Create<PartnerSubmitData>();
        A.CallTo(() => _networkRepository.GetSubmitData(_identity.CompanyId))
            .Returns((true, Enumerable.Repeat<ValueTuple<Guid, CompanyApplicationStatusId, string?>>((applicationId, CompanyApplicationStatusId.VERIFY, null), 1), Enumerable.Empty<ValueTuple<CompanyRoleId, IEnumerable<Guid>>>(), Guid.NewGuid()));

        // Act
        async Task Act() => await _sut.Submit(data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Application {applicationId} is not in state CREATED");
    }

    [Fact]
    public async Task Submit_WithOneMissingAgreement_ThrowsConflictException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var agreementId = Guid.NewGuid();
        var notExistingAgreementId = Guid.NewGuid();
        var data = new PartnerSubmitData(
            new[] { CompanyRoleId.APP_PROVIDER },
            new[] { new AgreementConsentData(agreementId, ConsentStatusId.ACTIVE) });
        var companyRoleIds = new ValueTuple<CompanyRoleId, IEnumerable<Guid>>[]
        {
            (CompanyRoleId.APP_PROVIDER, new [] {agreementId, notExistingAgreementId})
        };
        A.CallTo(() => _networkRepository.GetSubmitData(_identity.CompanyId))
            .Returns((true, Enumerable.Repeat<ValueTuple<Guid, CompanyApplicationStatusId, string?>>((applicationId, CompanyApplicationStatusId.CREATED, null), 1), companyRoleIds, Guid.NewGuid()));

        // Act
        async Task Act() => await _sut.Submit(data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"All Agreements for the company roles must be agreed to, missing agreementIds: {notExistingAgreementId} (Parameter 'Agreements')");
    }

    [Fact]
    public async Task Submit_WithOneInactiveAgreement_ThrowsConflictException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var agreementId = Guid.NewGuid();
        var inactiveAgreementId = Guid.NewGuid();
        var data = new PartnerSubmitData(
            new[] { CompanyRoleId.APP_PROVIDER },
            new[]
            {
                new AgreementConsentData(agreementId, ConsentStatusId.ACTIVE),
                new AgreementConsentData(inactiveAgreementId, ConsentStatusId.INACTIVE),
            });
        var companyRoleIds = new ValueTuple<CompanyRoleId, IEnumerable<Guid>>[]
        {
            (CompanyRoleId.APP_PROVIDER, new [] {agreementId, inactiveAgreementId})
        };
        A.CallTo(() => _networkRepository.GetSubmitData(_identity.CompanyId))
            .Returns((true, Enumerable.Repeat<ValueTuple<Guid, CompanyApplicationStatusId, string?>>((applicationId, CompanyApplicationStatusId.CREATED, null), 1), companyRoleIds, Guid.NewGuid()));

        // Act
        async Task Act() => await _sut.Submit(data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"All agreements must be agreed to. Agreements that are not active: {inactiveAgreementId} (Parameter 'Agreements')");
    }

    [Fact]
    public async Task Submit_WithoutProcessId_ThrowsConflictException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var agreementId = Guid.NewGuid();
        var agreementId1 = Guid.NewGuid();
        var data = new PartnerSubmitData(
            new[] { CompanyRoleId.APP_PROVIDER },
            new[]
            {
                new AgreementConsentData(agreementId, ConsentStatusId.ACTIVE),
                new AgreementConsentData(agreementId1, ConsentStatusId.ACTIVE),
            });
        var companyRoleIds = new ValueTuple<CompanyRoleId, IEnumerable<Guid>>[]
        {
            (CompanyRoleId.APP_PROVIDER, new [] {agreementId, agreementId1})
        };
        A.CallTo(() => _networkRepository.GetSubmitData(_identity.CompanyId))
            .Returns((true, Enumerable.Repeat<ValueTuple<Guid, CompanyApplicationStatusId, string?>>((applicationId, CompanyApplicationStatusId.CREATED, "https://callback.url"), 1), companyRoleIds, null));

        // Act
        async Task Act() => await _sut.Submit(data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("There must be an process");
    }

    [Fact]
    public async Task Submit_WithValidData_CallsExpected()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var agreementId = Guid.NewGuid();
        var agreementId1 = Guid.NewGuid();
        var processId = Guid.NewGuid();
        var submitProcessId = Guid.NewGuid();
        var processSteps = new List<ProcessStep>();
        var application = new CompanyApplication(applicationId, _identity.CompanyId, CompanyApplicationStatusId.CREATED, CompanyApplicationTypeId.EXTERNAL, DateTimeOffset.UtcNow);

        var data = new PartnerSubmitData(
            new[] { CompanyRoleId.APP_PROVIDER },
            new[]
            {
                new AgreementConsentData(agreementId, ConsentStatusId.ACTIVE),
                new AgreementConsentData(agreementId1, ConsentStatusId.ACTIVE),
            });
        var companyRoleIds = new ValueTuple<CompanyRoleId, IEnumerable<Guid>>[]
        {
            (CompanyRoleId.APP_PROVIDER, new [] {agreementId, agreementId1})
        };
        A.CallTo(() => _networkRepository.GetSubmitData(_identity.CompanyId))
            .Returns((true, Enumerable.Repeat<ValueTuple<Guid, CompanyApplicationStatusId, string?>>((applicationId, CompanyApplicationStatusId.CREATED, "https://callback.url"), 1), companyRoleIds, submitProcessId));
        A.CallTo(() => _applicationRepository.AttachAndModifyCompanyApplication(applicationId, A<Action<CompanyApplication>>._))
            .Invokes((Guid _, Action<CompanyApplication> setOptionalFields) =>
            {
                setOptionalFields.Invoke(application);
            });
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)>>._))
            .Invokes((IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> steps) =>
                {
                    processSteps.AddRange(steps.Select(x => new ProcessStep(Guid.NewGuid(), x.ProcessStepTypeId, x.ProcessStepStatusId, x.ProcessId, DateTimeOffset.UtcNow)));
                });
        var consents = new List<Consent>();
        var now = DateTimeOffset.UtcNow;
        A.CallTo(() => _consentRepository.CreateConsents(A<IEnumerable<(Guid, Guid, Guid, ConsentStatusId)>>._))
            .Invokes((IEnumerable<(Guid AgreementId, Guid CompanyId, Guid CompanyUserId, ConsentStatusId ConsentStatusId)> agreementConsents) =>
            {
                foreach (var x in agreementConsents)
                {
                    consents.Add(new Consent(Guid.NewGuid(), x.AgreementId, x.CompanyId, x.CompanyUserId, x.ConsentStatusId, now));
                }
            });
        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.APPLICATION_CHECKLIST))
            .ReturnsLazily((ProcessTypeId processTypeId) => new Process(processId, processTypeId, Guid.NewGuid()));
        A.CallTo(() => _checklistService.CreateInitialChecklistAsync(applicationId))
            .Returns(new[]
            {
                (ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.TO_DO),
                (ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE),
                (ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO),
                (ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO),
                (ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
                (ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO),
            });
        A.CallTo(() => _checklistService.GetInitialProcessStepTypeIds(A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._))
            .Returns(new[] { ProcessStepTypeId.VERIFY_REGISTRATION, ProcessStepTypeId.DECLINE_APPLICATION });

        // Act
        await _sut.Submit(data).ConfigureAwait(false);

        // Assert
        application.ApplicationStatusId.Should().Be(CompanyApplicationStatusId.SUBMITTED);
        A.CallTo(() => _consentRepository.CreateConsents(A<IEnumerable<(Guid, Guid, Guid, ConsentStatusId)>>._))
            .MustHaveHappenedOnceExactly();
        consents.Should().HaveCount(2)
            .And.AllSatisfy(x => x.Should().Match<Consent>(x =>
                x.CompanyId == _identity.CompanyId &&
                x.CompanyUserId == _identity.UserId &&
                x.ConsentStatusId == ConsentStatusId.ACTIVE))
            .And.Satisfy(
                x => x.AgreementId == agreementId,
                x => x.AgreementId == agreementId1);
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)>>._))
            .MustHaveHappenedOnceExactly();
        processSteps.Should().Satisfy(
            x => x.ProcessId == processId && x.ProcessStepTypeId == ProcessStepTypeId.VERIFY_REGISTRATION && x.ProcessStepStatusId == ProcessStepStatusId.TODO,
            x => x.ProcessId == processId && x.ProcessStepTypeId == ProcessStepTypeId.DECLINE_APPLICATION && x.ProcessStepStatusId == ProcessStepStatusId.TODO,
            x => x.ProcessId == submitProcessId && x.ProcessStepTypeId == ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED && x.ProcessStepStatusId == ProcessStepStatusId.TODO);
    }

    #endregion

    #region Setup

    private void SetupRepos()
    {
        A.CallTo(() => _networkRepository.CheckExternalIdExists(ExistingExternalId, A<Guid>.That.Matches(x => x == _identity.CompanyId || x == NoIdpCompanyId)))
            .Returns(true);
        A.CallTo(() => _networkRepository.CheckExternalIdExists(A<string>.That.Not.Matches(x => x == ExistingExternalId), A<Guid>.That.Matches(x => x == _identity.CompanyId || x == NoIdpCompanyId)))
            .Returns(false);

        A.CallTo(() => _companyRepository.CheckBpnExists(Bpnl)).Returns(false);
        A.CallTo(() => _companyRepository.CheckBpnExists(A<string>.That.Not.Matches(x => x == Bpnl))).Returns(true);

        A.CallTo(() => _countryRepository.CheckCountryExistsByAlpha2CodeAsync("XX"))
            .Returns(false);
        A.CallTo(() => _countryRepository.CheckCountryExistsByAlpha2CodeAsync(A<string>.That.Not.Matches(x => x == "XX")))
            .Returns(true);

        A.CallTo(() => _companyRepository.GetCompanyNameUntrackedAsync(A<Guid>.That.Matches(x => x == _identity.CompanyId || x == NoIdpCompanyId)))
            .Returns((true, "testCompany"));
        A.CallTo(() => _companyRepository.GetCompanyNameUntrackedAsync(A<Guid>.That.Not.Matches(x => x == _identity.CompanyId)))
            .Returns((false, ""));

        A.CallTo(() => _identityProviderRepository.GetSingleManagedIdentityProviderAliasDataUntracked(_identity.CompanyId))
            .Returns((IdpId, (string?)"test-alias"));

        A.CallTo(() => _identityProviderRepository.GetSingleManagedIdentityProviderAliasDataUntracked(NoAliasIdpCompanyId))
            .Returns((IdpId, (string?)null));

        A.CallTo(() => _identityProviderRepository.GetSingleManagedIdentityProviderAliasDataUntracked(NoIdpCompanyId))
            .Returns(((Guid, string?))default);

        A.CallTo(() => _identityProviderRepository.GetSingleManagedIdentityProviderAliasDataUntracked(MultiIdpCompanyId))
            .Throws(new InvalidOperationException("Sequence contains more than one element."));

        A.CallTo(() => _identityProviderRepository.GetManagedIdentityProviderAliasDataUntracked(A<Guid>.That.Matches(x => x == _identity.CompanyId || x == NoIdpCompanyId), A<IEnumerable<Guid>>._))
            .Returns(new[] { (IdpId, (string?)"test-alias") }.ToAsyncEnumerable());

        A.CallTo(() => _userProvisioningService.GetRoleDatas(A<IEnumerable<UserRoleConfig>>._))
            .Returns(new[] { new UserRoleData(UserRoleId, "cl1", "Company Admin") }.ToAsyncEnumerable());
    }

    #endregion
}
