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
using Org.Eclipse.TractusX.Portal.Backend.ApplicationActivation.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.ApplicationActivation.Library.Tests;

public class ApplicationActivationTests
{
    private const string BusinessPartnerNumber = "CAXLSHAREDIDPZZ";
    private const string IdpAlias = "idp5";
    private const string ClientId = "catenax-portal";
    private const string CompanyName = "Shared Idp Test";
    private static readonly Guid Id = new("d90995fe-1241-4b8d-9f5c-f3909acc6383");
    private static readonly Guid IdWithoutBpn = new("d90995fe-1241-4b8d-9f5c-f3909acc6399");
    private static readonly Guid IdWithTypeExternal = new("8660f3d3-bf98-42ec-960d-692d4d794368");
    private static readonly Guid IdWithTypeExternalWithoutProcess = new("8660f3d3-bf98-42ec-960d-692d4d794369");
    private static readonly Guid CompanyUserId1 = new("857b93b1-8fcb-4141-81b0-ae81950d489e");
    private static readonly Guid CompanyUserId2 = new("857b93b1-8fcb-4141-81b0-ae81950d489f");
    private static readonly Guid CompanyUserId3 = new("857b93b1-8fcb-4141-81b0-ae81950d48af");
    private static readonly Guid UserRoleId = new("607818be-4978-41f4-bf63-fa8d2de51154");
    private static readonly Guid CompanyUserRoleId = new("607818be-4978-41f4-bf63-fa8d2de51154");
    private static readonly string CentralUserId1 = "6bc51706-9a30-4eb9-9e60-77fdd6d9cd6f";
    private static readonly string CentralUserId2 = "6bc51706-9a30-4eb9-9e60-77fdd6d9cd70";
    private static readonly string CentralUserId3 = "6bc51706-9a30-4eb9-9e60-77fdd6d9cd71";
    private static readonly Guid ProcessId = new("db9d99cd-51a3-4933-a1cf-dc1b836b53bb");

    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUserBusinessPartnerRepository _businessPartnerRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserRolesRepository _rolesRepository;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly IMailingProcessCreation _mailingProcessCreation;
    private readonly List<Notification> _notifications = new();
    private readonly List<Guid> _notifiedUserIds = new();
    private readonly INotificationService _notificationService;
    private readonly IProvisioningManager _provisioningManager;
    private readonly ApplicationActivationSettings _settings;
    private readonly ApplicationActivationService _sut;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICustodianService _custodianService;

    public ApplicationActivationTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.ConfigureFixture();

        _provisioningManager = A.Fake<IProvisioningManager>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _businessPartnerRepository = A.Fake<IUserBusinessPartnerRepository>();
        _mailingProcessCreation = A.Fake<IMailingProcessCreation>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _rolesRepository = A.Fake<IUserRolesRepository>();
        _notificationService = A.Fake<INotificationService>();
        _dateTimeProvider = A.Fake<IDateTimeProvider>();
        _custodianService = A.Fake<ICustodianService>();
        _settings = A.Fake<ApplicationActivationSettings>();
        _processStepRepository = A.Fake<IProcessStepRepository>();

        var options = A.Fake<IOptions<ApplicationActivationSettings>>();

        _settings.WelcomeNotificationTypeIds = new[]
        {
            NotificationTypeId.WELCOME,
            NotificationTypeId.WELCOME_USE_CASES,
            NotificationTypeId.WELCOME_APP_MARKETPLACE,
            NotificationTypeId.WELCOME_SERVICE_PROVIDER,
            NotificationTypeId.WELCOME_CONNECTOR_REGISTRATION
        };

        _settings.ClientToRemoveRolesOnActivation = new[]
        {
            "remove-id"
        };
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserBusinessPartnerRepository>()).Returns(_businessPartnerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_rolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);
        A.CallTo(() => options.Value).Returns(_settings);

        _sut = new ApplicationActivationService(_portalRepositories, _notificationService, _provisioningManager, _dateTimeProvider, _custodianService, _mailingProcessCreation, options);
    }

    #region HandleApplicationActivation

    [Fact]
    public async Task HandleApplicationActivation_OutsideConfiguredTime_DoesntActivateApplication()
    {
        //Arrange
        var companyUserAssignedRole = _fixture.Create<IdentityAssignedRole>();
        var companyUserAssignedBusinessPartner = _fixture.Create<CompanyUserAssignedBusinessPartner>();
        A.CallTo(() => _dateTimeProvider.Now).Returns(new DateTime(2022, 01, 01, 12, 0, 0));
        _settings.StartTime = TimeSpan.FromHours(4);
        _settings.EndTime = TimeSpan.FromHours(8);
        SetupFakes(Enumerable.Empty<UserRoleConfig>(), Enumerable.Empty<UserRoleData>(), companyUserAssignedRole, companyUserAssignedBusinessPartner);
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            Id,
            default,
            new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO}
            }.ToImmutableDictionary(),
            Enumerable.Empty<ProcessStepTypeId>());

        //Act
        await _sut.HandleApplicationActivation(context, CancellationToken.None).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForApprovalAsync(Id)).MustNotHaveHappened();
        A.CallTo(() => _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(Id)).MustNotHaveHappened();
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(CompanyUserId1, UserRoleId)).MustNotHaveHappened();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId1, BusinessPartnerNumber)).MustNotHaveHappened();
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(CompanyUserId2, UserRoleId)).MustNotHaveHappened();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId2, BusinessPartnerNumber)).MustNotHaveHappened();
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(CompanyUserId3, UserRoleId)).MustNotHaveHappened();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId3, BusinessPartnerNumber)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        _notifications.Should().BeEmpty();
        _notifiedUserIds.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleApplicationActivation_WithNotAllStepsInDone_ThrowsConflictException()
    {
        var roles = new[] { "Company Admin" };
        var clientRoleNames = new[]
        {
            new UserRoleConfig(ClientId, roles.AsEnumerable())
        };
        var userRoleData = new UserRoleData[] { new(UserRoleId, ClientId, "Company Admin") };

        var companyUserAssignedRole = _fixture.Create<IdentityAssignedRole>();
        var companyUserAssignedBusinessPartner = _fixture.Create<CompanyUserAssignedBusinessPartner>();
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(Id, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupFakes(clientRoleNames, userRoleData, companyUserAssignedRole, companyUserAssignedBusinessPartner);
        A.CallTo(() => _dateTimeProvider.Now).Returns(new DateTime(2022, 01, 01, 0, 0, 0));
        _settings.StartTime = TimeSpan.FromHours(22);
        _settings.EndTime = TimeSpan.FromHours(8);

        //Act
        async Task Act() => await _sut.HandleApplicationActivation(context, CancellationToken.None).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"cannot activate application {context.ApplicationId}. Checklist entries that are not in status DONE: [REGISTRATION_VERIFICATION, DONE],[BUSINESS_PARTNER_NUMBER, DONE],[IDENTITY_WALLET, DONE],[CLEARING_HOUSE, DONE],[SELF_DESCRIPTION_LP, TO_DO]");
    }

    [Fact]
    public async Task HandleApplicationActivation_WithMidnightRun_ApprovesRequestAndCreatesNotifications()
    {
        //Arrange
        var roles = new[] { "Company Admin" };
        var clientRoleNames = new[]
        {
            new UserRoleConfig(ClientId, roles.AsEnumerable())
        };
        var userRoleData = new UserRoleData[] { new(UserRoleId, ClientId, "Company Admin") };

        var companyUserAssignedRole = _fixture.Create<IdentityAssignedRole>();
        var companyUserAssignedBusinessPartner = _fixture.Create<CompanyUserAssignedBusinessPartner>();
        var companyApplication = _fixture.Build<CompanyApplication>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .Create();
        var company = _fixture.Build<Company>()
            .With(x => x.CompanyStatusId, CompanyStatusId.PENDING)
            .Create();
        SetupFakes(clientRoleNames, userRoleData, companyUserAssignedRole, companyUserAssignedBusinessPartner);
        SetupForDelete();
        A.CallTo(() => _dateTimeProvider.Now).Returns(new DateTime(2022, 01, 01, 0, 0, 0));
        _settings.StartTime = TimeSpan.FromHours(22);
        _settings.EndTime = TimeSpan.FromHours(8);
        A.CallTo(() =>
                _applicationRepository.AttachAndModifyCompanyApplication(A<Guid>._, A<Action<CompanyApplication>>._))
            .Invokes((Guid _, Action<CompanyApplication> setOptionalParameters) =>
            {
                setOptionalParameters.Invoke(companyApplication);
            });
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, null, A<Action<Company>>._))
            .Invokes((Guid _, Action<Company>? _, Action<Company> setOptionalParameters) =>
            {
                setOptionalParameters.Invoke(company);
            });
        A.CallTo(() => _custodianService.SetMembership(BusinessPartnerNumber, A<CancellationToken>._))
            .Returns("Membership Credential successfully created");
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            Id,
            default,
            new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO}
            }.ToImmutableDictionary(),
            Enumerable.Empty<ProcessStepTypeId>());

        //Act
        var result = await _sut.HandleApplicationActivation(context, CancellationToken.None).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForApprovalAsync(Id)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(Id)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(CompanyUserId1, UserRoleId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId1, BusinessPartnerNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(CompanyUserId2, UserRoleId)).MustNotHaveHappened();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId2, BusinessPartnerNumber)).MustNotHaveHappened();
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(CompanyUserId3, UserRoleId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId3, BusinessPartnerNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.GetUserRolesByClientId(A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "remove-id" }))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.GetUserWithUserRolesForApplicationId(A<Guid>._, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { CompanyUserRoleId }))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteClientRolesFromCentralUserAsync(CentralUserId1, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteClientRolesFromCentralUserAsync(CentralUserId2, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteClientRolesFromCentralUserAsync(CentralUserId3, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingProcessCreation.CreateMailProcess(A<string>._, "EmailRegistrationWelcomeTemplate", A<Dictionary<string, string>>._))
            .MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => _custodianService.SetMembership(BusinessPartnerNumber, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        _notifications.Should().HaveCount(5);
        _notifiedUserIds.Should().HaveCount(3)
            .And.ContainInOrder(CompanyUserId1, CompanyUserId2, CompanyUserId3);
        companyApplication.ApplicationStatusId.Should().Be(CompanyApplicationStatusId.CONFIRMED);
        company.CompanyStatusId.Should().Be(CompanyStatusId.ACTIVE);
        var entry = new ApplicationChecklistEntry(Guid.NewGuid(), ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO, default);
        result.ModifyChecklistEntry!.Invoke(entry);
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
        entry.Comment.Should().Be("Membership Credential successfully created");
        result.ScheduleStepTypeIds.Should().BeNull();
        result.SkipStepTypeIds.Should().HaveCount(Enum.GetValues<ProcessStepTypeId>().Length - 1);
        result.Modified.Should().BeTrue();
    }

    [Fact]
    public async Task HandleApplicationActivation_WithCompanyAdminUser_ApprovesRequestAndCreatesNotifications()
    {
        //Arrange
        var roles = new[] { "Company Admin" };
        var clientRoleNames = new[]
        {
            new UserRoleConfig(ClientId, roles.AsEnumerable())
        };
        var userRoleData = new UserRoleData[] { new(UserRoleId, ClientId, "Company Admin") };

        var companyUserAssignedRole = _fixture.Create<IdentityAssignedRole>();
        var companyUserAssignedBusinessPartner = _fixture.Create<CompanyUserAssignedBusinessPartner>();
        var companyApplication = _fixture.Build<CompanyApplication>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .Create();
        var company = _fixture.Build<Company>()
            .With(x => x.CompanyStatusId, CompanyStatusId.PENDING)
            .Create();
        SetupFakes(clientRoleNames, userRoleData, companyUserAssignedRole, companyUserAssignedBusinessPartner);
        SetupForDelete();
        A.CallTo(() =>
                _applicationRepository.AttachAndModifyCompanyApplication(A<Guid>._, A<Action<CompanyApplication>>._))
            .Invokes((Guid _, Action<CompanyApplication> setOptionalParameters) =>
            {
                setOptionalParameters.Invoke(companyApplication);
            });
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, null, A<Action<Company>>._))
            .Invokes((Guid _, Action<Company>? _, Action<Company> setOptionalParameters) =>
            {
                setOptionalParameters.Invoke(company);
            });
        A.CallTo(() => _custodianService.SetMembership(BusinessPartnerNumber, A<CancellationToken>._))
            .Returns("Membership Credential successfully created");
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            Id,
            default,
            new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO}
            }.ToImmutableDictionary(),
            Enumerable.Empty<ProcessStepTypeId>());

        //Act
        var result = await _sut.HandleApplicationActivation(context, CancellationToken.None).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForApprovalAsync(Id)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(Id)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(CompanyUserId1, UserRoleId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId1, BusinessPartnerNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(CompanyUserId2, UserRoleId)).MustNotHaveHappened();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId2, BusinessPartnerNumber)).MustNotHaveHappened();
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(CompanyUserId3, UserRoleId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId3, BusinessPartnerNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.GetUserRolesByClientId(A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "remove-id" }))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.GetUserWithUserRolesForApplicationId(A<Guid>._, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { CompanyUserRoleId }))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteClientRolesFromCentralUserAsync(CentralUserId1, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteClientRolesFromCentralUserAsync(CentralUserId2, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteClientRolesFromCentralUserAsync(CentralUserId3, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.DeleteCompanyUserAssignedRoles(A<IEnumerable<(Guid CompanyUserId, Guid UserRoleId)>>._)).MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => _mailingProcessCreation.CreateMailProcess(A<string>._, "EmailRegistrationWelcomeTemplate", A<Dictionary<string, string>>._))
            .MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => _custodianService.SetMembership(BusinessPartnerNumber, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        _notifications.Should().HaveCount(5);
        _notifiedUserIds.Should().HaveCount(3)
            .And.ContainInOrder(CompanyUserId1, CompanyUserId2, CompanyUserId3);
        companyApplication.ApplicationStatusId.Should().Be(CompanyApplicationStatusId.CONFIRMED);
        company.CompanyStatusId.Should().Be(CompanyStatusId.ACTIVE);
        var entry = new ApplicationChecklistEntry(Guid.NewGuid(), ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO, default);
        result.ModifyChecklistEntry!.Invoke(entry);
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
        entry.Comment.Should().Be("Membership Credential successfully created");
        result.ScheduleStepTypeIds.Should().BeNull();
        result.SkipStepTypeIds.Should().HaveCount(Enum.GetValues<ProcessStepTypeId>().Length - 1);
        result.Modified.Should().BeTrue();
    }

    [Fact]
    public async Task HandleApplicationActivation_WithoutNetworkRegistrationId_ThrowsConflictException()
    {
        //Arrange
        var roles = new[] { "Company Admin" };
        var clientRoleNames = new[]
        {
            new UserRoleConfig(ClientId, roles.AsEnumerable())
        };
        var userRoleData = new UserRoleData[] { new(UserRoleId, ClientId, "Company Admin") };

        var companyUserAssignedRole = _fixture.Create<IdentityAssignedRole>();
        var companyUserAssignedBusinessPartner = _fixture.Create<CompanyUserAssignedBusinessPartner>();
        SetupFakes(clientRoleNames, userRoleData, companyUserAssignedRole, companyUserAssignedBusinessPartner);
        SetupForDelete();

        A.CallTo(() => _custodianService.SetMembership(BusinessPartnerNumber, A<CancellationToken>._))
            .Returns("Membership Credential successfully created");

        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            IdWithTypeExternalWithoutProcess,
            default,
            new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO}
            }.ToImmutableDictionary(),
            Enumerable.Empty<ProcessStepTypeId>());

        //Act
        async Task Act() => await _sut.HandleApplicationActivation(context, CancellationToken.None).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("ProcessId should be set for external applications");
    }

    [Fact]
    public async Task HandleApplicationActivation_WithExternalApplication_ApprovesRequestAndCreatesNotifications()
    {
        //Arrange
        var roles = new[] { "Company Admin" };
        var clientRoleNames = new[]
        {
            new UserRoleConfig(ClientId, roles.AsEnumerable())
        };
        var userRoleData = new UserRoleData[] { new(UserRoleId, ClientId, "Company Admin") };

        var processSteps = new List<ProcessStep>();
        var companyUserAssignedRole = _fixture.Create<IdentityAssignedRole>();
        var companyUserAssignedBusinessPartner = _fixture.Create<CompanyUserAssignedBusinessPartner>();
        var companyApplication = _fixture.Build<CompanyApplication>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .Create();
        var company = _fixture.Build<Company>()
            .With(x => x.CompanyStatusId, CompanyStatusId.PENDING)
            .Create();
        SetupFakes(clientRoleNames, userRoleData, companyUserAssignedRole, companyUserAssignedBusinessPartner);
        SetupForDelete();
        A.CallTo(() =>
                _applicationRepository.AttachAndModifyCompanyApplication(A<Guid>._, A<Action<CompanyApplication>>._))
            .Invokes((Guid _, Action<CompanyApplication> setOptionalParameters) =>
            {
                setOptionalParameters.Invoke(companyApplication);
            });
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, null, A<Action<Company>>._))
            .Invokes((Guid _, Action<Company>? _, Action<Company> setOptionalParameters) =>
            {
                setOptionalParameters.Invoke(company);
            });
        A.CallTo(() => _custodianService.SetMembership(BusinessPartnerNumber, A<CancellationToken>._))
            .Returns("Membership Credential successfully created");
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)>>.That.Matches(x =>
                x.Count() == 1 &&
                x.Single().ProcessStepTypeId == ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED)))
            .Invokes((IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> steps) =>
            {
                processSteps.AddRange(steps.Select(x => new ProcessStep(Guid.NewGuid(), x.ProcessStepTypeId, x.ProcessStepStatusId, x.ProcessId, DateTimeOffset.UtcNow)));
            });

        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            IdWithTypeExternal,
            default,
            new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO}
            }.ToImmutableDictionary(),
            Enumerable.Empty<ProcessStepTypeId>());

        //Act
        var result = await _sut.HandleApplicationActivation(context, CancellationToken.None).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForApprovalAsync(IdWithTypeExternal)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(IdWithTypeExternal)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(CompanyUserId2, UserRoleId)).MustNotHaveHappened();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId2, BusinessPartnerNumber)).MustNotHaveHappened();
        A.CallTo(() => _rolesRepository.GetUserRolesByClientId(A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "remove-id" }))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.GetUserWithUserRolesForApplicationId(A<Guid>._, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { CompanyUserRoleId }))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteClientRolesFromCentralUserAsync(CentralUserId1, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteClientRolesFromCentralUserAsync(CentralUserId2, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteClientRolesFromCentralUserAsync(CentralUserId3, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.DeleteCompanyUserAssignedRoles(A<IEnumerable<(Guid CompanyUserId, Guid UserRoleId)>>._)).MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => _custodianService.SetMembership(BusinessPartnerNumber, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        processSteps.Should().ContainSingle().And.Satisfy(
            x => x.ProcessStepTypeId == ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED &&
                 x.ProcessStepStatusId == ProcessStepStatusId.TODO);
        _notifications.Should().HaveCount(5);
        _notifiedUserIds.Should().HaveCount(3)
            .And.ContainInOrder(CompanyUserId1, CompanyUserId2, CompanyUserId3);
        companyApplication.ApplicationStatusId.Should().Be(CompanyApplicationStatusId.CONFIRMED);
        company.CompanyStatusId.Should().Be(CompanyStatusId.ACTIVE);
        var entry = new ApplicationChecklistEntry(Guid.NewGuid(), ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO, default);
        result.ModifyChecklistEntry!.Invoke(entry);
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
        entry.Comment.Should().Be("Membership Credential successfully created");
        result.ScheduleStepTypeIds.Should().BeNull();
        result.SkipStepTypeIds.Should().HaveCount(Enum.GetValues<ProcessStepTypeId>().Length - 1);
        result.Modified.Should().BeTrue();
    }

    [Fact]
    public async Task HandleApplicationActivation_WithNotExistingRoles_ThrowsConfigurationException()
    {
        //Arrange
        var roles = new[] { "Company Admin" };
        var clientRoleNames = new[]
        {
            new UserRoleConfig(ClientId, roles.AsEnumerable())
        };
        var userRoleData = new UserRoleData[] { new(UserRoleId, ClientId, "Company Admin") };

        var companyUserAssignedRole = _fixture.Create<IdentityAssignedRole>();
        var companyUserAssignedBusinessPartner = _fixture.Create<CompanyUserAssignedBusinessPartner>();
        var companyApplication = _fixture.Build<CompanyApplication>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .Create();
        var company = _fixture.Build<Company>()
            .With(x => x.CompanyStatusId, CompanyStatusId.PENDING)
            .Create();
        SetupFakes(clientRoleNames, userRoleData, companyUserAssignedRole, companyUserAssignedBusinessPartner);
        SetupForDelete();

        _settings.ApplicationApprovalInitialRoles = new[]
        {
            new UserRoleConfig(ClientId, new []{ "Company Admin", "notexistingrole"})
        };
        ;
        A.CallTo(() =>
                _applicationRepository.AttachAndModifyCompanyApplication(A<Guid>._, A<Action<CompanyApplication>>._))
            .Invokes((Guid _, Action<CompanyApplication> setOptionalParameters) =>
            {
                setOptionalParameters.Invoke(companyApplication);
            });
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, null, A<Action<Company>>._))
            .Invokes((Guid _, Action<Company>? _, Action<Company> setOptionalParameters) =>
            {
                setOptionalParameters.Invoke(company);
            });
        A.CallTo(() => _custodianService.SetMembership(BusinessPartnerNumber, A<CancellationToken>._))
            .Returns("Membership Credential successfully created");
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            Id,
            default,
            new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO}
            }.ToImmutableDictionary(),
            Enumerable.Empty<ProcessStepTypeId>());

        //Act
        async Task Act() => await _sut.HandleApplicationActivation(context, CancellationToken.None).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ConfigurationException>(Act);
        ex.Message.Should().Be("invalid configuration, at least one of the configured roles does not exist in the database: client: catenax-portal, roles: [Company Admin, notexistingrole]");
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForApprovalAsync(Id)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(A<Guid>._)).MustNotHaveHappened();
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(A<Guid>._, A<Guid>._)).MustNotHaveHappened();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(A<Guid>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _rolesRepository.GetUserRolesByClientId(A<IEnumerable<string>>._)).MustNotHaveHappened();
        A.CallTo(() => _rolesRepository.GetUserWithUserRolesForApplicationId(A<Guid>._, A<IEnumerable<Guid>>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.DeleteClientRolesFromCentralUserAsync(A<string>._, A<IDictionary<string, IEnumerable<string>>>._)).MustNotHaveHappened();
        A.CallTo(() => _rolesRepository.DeleteCompanyUserAssignedRoles(A<IEnumerable<(Guid CompanyUserId, Guid UserRoleId)>>._)).MustNotHaveHappened();
        A.CallTo(() => _mailingProcessCreation.CreateMailProcess(A<string>._, "EmailRegistrationWelcomeTemplate", A<Dictionary<string, string>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _custodianService.SetMembership(BusinessPartnerNumber, A<CancellationToken>._)).MustNotHaveHappened();
        _notifications.Should().BeEmpty();
        _notifiedUserIds.Should().BeEmpty();
        companyApplication.ApplicationStatusId.Should().Be(CompanyApplicationStatusId.SUBMITTED);
        company.CompanyStatusId.Should().Be(CompanyStatusId.PENDING);
    }

    [Fact]
    public async Task HandleApplicationActivation_WithUnassignedRoles_ThrowsUnexpectedConditionException()
    {
        //Arrange
        var roles = new[] { "Company Admin" };
        var clientRoleNames = new[]
        {
            new UserRoleConfig(ClientId, roles.AsEnumerable())
        };
        var userRoleData = new UserRoleData[] { new(UserRoleId, ClientId, "Company Admin"), new(Guid.NewGuid(), ClientId, "IT Admin") };

        var companyUserAssignedRole = _fixture.Create<IdentityAssignedRole>();
        var companyUserAssignedBusinessPartner = _fixture.Create<CompanyUserAssignedBusinessPartner>();
        var companyApplication = _fixture.Build<CompanyApplication>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .Create();
        var company = _fixture.Build<Company>()
            .With(x => x.CompanyStatusId, CompanyStatusId.PENDING)
            .Create();
        SetupFakes(clientRoleNames, userRoleData, companyUserAssignedRole, companyUserAssignedBusinessPartner);
        SetupForDelete();

        _settings.ApplicationApprovalInitialRoles = new[]
        {
            new UserRoleConfig(ClientId, new []{ "Company Admin", "IT Admin"})
        };
        ;
        A.CallTo(() =>
                _applicationRepository.AttachAndModifyCompanyApplication(A<Guid>._, A<Action<CompanyApplication>>._))
            .Invokes((Guid _, Action<CompanyApplication> setOptionalParameters) =>
            {
                setOptionalParameters.Invoke(companyApplication);
            });
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, null, A<Action<Company>>._))
            .Invokes((Guid _, Action<Company>? _, Action<Company> setOptionalParameters) =>
            {
                setOptionalParameters.Invoke(company);
            });
        A.CallTo(() => _custodianService.SetMembership(BusinessPartnerNumber, A<CancellationToken>._))
            .Returns("Membership Credential successfully created");
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            Id,
            default,
            new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO}
            }.ToImmutableDictionary(),
            Enumerable.Empty<ProcessStepTypeId>());

        //Act
        async Task Act() => await _sut.HandleApplicationActivation(context, CancellationToken.None).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be("inconsistent data, roles not assigned in keycloak: client: catenax-portal, roles: [IT Admin], error: ");

        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForApprovalAsync(Id)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(Id)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(CompanyUserId1, UserRoleId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId1, BusinessPartnerNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(CompanyUserId2, UserRoleId)).MustNotHaveHappened();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId2, BusinessPartnerNumber)).MustNotHaveHappened();
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(CompanyUserId3, UserRoleId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId3, BusinessPartnerNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.GetUserRolesByClientId(A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "remove-id" }))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.GetUserWithUserRolesForApplicationId(A<Guid>._, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { CompanyUserRoleId }))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteClientRolesFromCentralUserAsync(CentralUserId1, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteClientRolesFromCentralUserAsync(CentralUserId2, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteClientRolesFromCentralUserAsync(CentralUserId3, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.DeleteCompanyUserAssignedRoles(A<IEnumerable<(Guid CompanyUserId, Guid UserRoleId)>>._)).MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => _mailingProcessCreation.CreateMailProcess(A<string>._, "EmailRegistrationWelcomeTemplate", A<Dictionary<string, string>>._))
            .MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => _custodianService.SetMembership(BusinessPartnerNumber, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        _notifications.Should().HaveCount(5);
        _notifiedUserIds.Should().HaveCount(3)
            .And.ContainInOrder(CompanyUserId1, CompanyUserId2, CompanyUserId3);
        companyApplication.ApplicationStatusId.Should().Be(CompanyApplicationStatusId.CONFIRMED);
        company.CompanyStatusId.Should().Be(CompanyStatusId.ACTIVE);
    }

    [Fact]
    public async Task HandleApplicationActivation_WithDefaultApplicationId_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.DONE},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(Guid.Empty, default, checklist, Enumerable.Empty<ProcessStepTypeId>());

        //Act
        async Task Action() => await _sut.HandleApplicationActivation(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Action);
        ex.Message.Should().Be($"BusinessPartnerNumber (bpn) for CompanyApplications {Guid.Empty} company {Guid.Empty} is empty");
    }

    [Fact]
    public async Task HandleApplicationActivation_WithoutCompanyApplication_ThrowsConflictException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            applicationId,
            default,
            new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO}
            }.ToImmutableDictionary(),
            Enumerable.Empty<ProcessStepTypeId>());

        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForApprovalAsync(applicationId))
            .Returns<(Guid, string, string?, IEnumerable<string>, CompanyApplicationTypeId, Guid?)>(default);

        //Act
        async Task Action() => await _sut.HandleApplicationActivation(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Action);
        ex.Message.Should().Be($"CompanyApplication {applicationId} is not in status SUBMITTED");
    }

    [Fact]
    public async Task HandleApplicationActivation_WithCompanyWithoutBPN_ThrowsConflictException()
    {
        //Act
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            IdWithoutBpn,
            default,
            new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO}
            }.ToImmutableDictionary(),
            Enumerable.Empty<ProcessStepTypeId>());

        async Task Action() => await _sut.HandleApplicationActivation(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Action);
        ex.Message.Should().Be($"BusinessPartnerNumber (bpn) for CompanyApplications {IdWithoutBpn} company {Guid.Empty} is empty");
    }

    #endregion

    #region Validate ApplicationActivationSettings

    [Fact]
    public void ApplicationActivationSettingsValidate_WithValidValues_ReturnsTrue()
    {
        //Arrange
        var settings = new ApplicationActivationSettings
        {
            StartTime = new TimeSpan(1, 0, 0),
            EndTime = new TimeSpan(12, 0, 0)
        };

        //Act
        var result = ApplicationActivationSettings.Validate(settings);

        //Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ApplicationActivationSettingsValidate_WithoutStartAndEndTime_ReturnsTrue()
    {
        //Arrange
        var settings = new ApplicationActivationSettings();

        //Act
        var result = ApplicationActivationSettings.Validate(settings);

        //Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ApplicationActivationSettingsValidate_WithOnlyStartTime_ReturnsFalse()
    {
        //Arrange
        var settings = new ApplicationActivationSettings
        {
            StartTime = new TimeSpan(1, 0, 0)
        };

        //Act
        var result = ApplicationActivationSettings.Validate(settings);

        //Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ApplicationActivationSettingsValidate_WithOnlyEndTime_ReturnsFalse()
    {
        //Arrange
        var settings = new ApplicationActivationSettings
        {
            EndTime = new TimeSpan(1, 0, 0)
        };

        //Act
        var result = ApplicationActivationSettings.Validate(settings);

        //Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ApplicationActivationSettingsValidate_WithEndTimeEarlierStartTime_ReturnsTrue()
    {
        //Arrange
        var settings = new ApplicationActivationSettings
        {
            StartTime = new TimeSpan(22, 0, 0),
            EndTime = new TimeSpan(6, 0, 0)
        };

        //Act
        var result = ApplicationActivationSettings.Validate(settings);

        //Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ApplicationActivationSettingsValidate_WithStartTimeMoreThanADay_ReturnsFalse()
    {
        //Arrange
        var settings = new ApplicationActivationSettings
        {
            StartTime = new TimeSpan(1, 5, 0, 0),
            EndTime = new TimeSpan(2, 0, 0)
        };

        //Act
        var result = ApplicationActivationSettings.Validate(settings);

        //Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ApplicationActivationSettingsValidate_WithEndTimeMoreThanADay_ReturnsFalse()
    {
        //Arrange
        var settings = new ApplicationActivationSettings
        {
            StartTime = new TimeSpan(1, 0, 0),
            EndTime = new TimeSpan(1, 2, 0, 0)
        };

        //Act
        var result = ApplicationActivationSettings.Validate(settings);

        //Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ApplicationActivationSettingsValidate_WithStartTimeMoreThan24Hours_ReturnsFalse()
    {
        //Arrange
        var settings = new ApplicationActivationSettings
        {
            StartTime = new TimeSpan(26, 0, 0),
            EndTime = new TimeSpan(2, 0, 0)
        };

        //Act
        var result = ApplicationActivationSettings.Validate(settings);

        //Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ApplicationActivationSettingsValidate_WithEndTimeMoreThan24Hours_ReturnsFalse()
    {
        //Arrange
        var settings = new ApplicationActivationSettings
        {
            StartTime = new TimeSpan(2, 0, 0),
            EndTime = new TimeSpan(25, 0, 0)
        };

        //Act
        var result = ApplicationActivationSettings.Validate(settings);

        //Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ApplicationActivationSettingsValidate_WithNegativeStartTime_ReturnsFalse()
    {
        //Arrange
        var settings = new ApplicationActivationSettings
        {
            StartTime = new TimeSpan(-2, 0, 0),
            EndTime = new TimeSpan(14, 0, 0)
        };

        //Act
        var result = ApplicationActivationSettings.Validate(settings);

        //Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ApplicationActivationSettingsValidate_WithNegativeEndTime_ReturnsFalse()
    {
        //Arrange
        var settings = new ApplicationActivationSettings
        {
            StartTime = new TimeSpan(14, 0, 0),
            EndTime = new TimeSpan(-2, 0, 0)
        };

        //Act
        var result = ApplicationActivationSettings.Validate(settings);

        //Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Setup

    private void SetupFakes(
        IEnumerable<UserRoleConfig> clientRoleNames,
        IEnumerable<UserRoleData> userRoleData,
        IdentityAssignedRole companyUserAssignedRole,
        CompanyUserAssignedBusinessPartner companyUserAssignedBusinessPartner)
    {
        var company = _fixture.Build<Company>()
            .With(u => u.BusinessPartnerNumber, BusinessPartnerNumber)
            .With(u => u.Name, CompanyName)
            .Create();

        var companyInvitedUsers = new CompanyInvitedUserData[]
        {
            new(CompanyUserId1, Enumerable.Empty<string>(), Enumerable.Empty<Guid>()),
            new(CompanyUserId2, Enumerable.Repeat(BusinessPartnerNumber, 1), Enumerable.Repeat(UserRoleId, 1)),
            new(CompanyUserId3, Enumerable.Empty<string>(), Enumerable.Empty<Guid>())
        }.ToAsyncEnumerable();
        var businessPartnerNumbers = new[] { BusinessPartnerNumber }.AsEnumerable();

        _settings.ApplicationApprovalInitialRoles = clientRoleNames;
        _settings.CompanyAdminRoles = new[]
        {
            new UserRoleConfig(ClientId, new[] { "Company Admin" }.AsEnumerable())
        };

        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForApprovalAsync(A<Guid>.That.Matches(x => x == Id)))
            .Returns((company.Id, company.Name, company.BusinessPartnerNumber, new[] { IdpAlias }, CompanyApplicationTypeId.INTERNAL, null));
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForApprovalAsync(A<Guid>.That.Matches(x => x == IdWithTypeExternal)))
            .Returns((company.Id, company.Name, company.BusinessPartnerNumber, new[] { IdpAlias }, CompanyApplicationTypeId.EXTERNAL, ProcessId));
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForApprovalAsync(A<Guid>.That.Matches(x => x == IdWithTypeExternalWithoutProcess)))
            .Returns((company.Id, company.Name, company.BusinessPartnerNumber, new[] { IdpAlias }, CompanyApplicationTypeId.EXTERNAL, null));
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForApprovalAsync(A<Guid>.That.Matches(x => x == IdWithoutBpn)))
            .Returns((IdWithoutBpn, null!, null, Enumerable.Empty<string>(), CompanyApplicationTypeId.INTERNAL, null));

        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForCreateWalletAsync(A<Guid>.That.Matches(x => x == Id)))
            .Returns((company.Id, company.Name, company.BusinessPartnerNumber));
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForCreateWalletAsync(A<Guid>.That.Matches(x => x == IdWithoutBpn)))
            .Returns((IdWithoutBpn, company.Name, null));

        var welcomeEmailData = new EmailData[]
        {
            new (CompanyUserId1, "Stan", "Lee", "stan@lee.com"),
            new (CompanyUserId2, "Tony", "Stark", "tony@stark.com"),
            new (CompanyUserId3, "Peter", "Parker", "peter@parker.com")
        };
        A.CallTo(() => _applicationRepository.GetEmailDataUntrackedAsync(Id))
            .Returns(welcomeEmailData.ToAsyncEnumerable());
        A.CallTo(() => _applicationRepository.GetEmailDataUntrackedAsync(A<Guid>.That.Not.Matches(x => x == Id)))
            .Returns(Enumerable.Empty<EmailData>().ToAsyncEnumerable());

        A.CallTo(() => _rolesRepository.GetUserRoleDataUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.Matches(x => x.First(y => y.ClientId == ClientId).UserRoleNames.First() == clientRoleNames.First(x => x.ClientId == ClientId).UserRoleNames.First())))
            .Returns(userRoleData.ToAsyncEnumerable());
        A.CallTo(() => _rolesRepository.GetUserRoleDataUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.Matches(x => x.First(y => y.ClientId == ClientId).UserRoleNames.First() == _settings.CompanyAdminRoles.First(x => x.ClientId == ClientId).UserRoleNames.First())))
            .Returns(new UserRoleData[] { new(UserRoleId, ClientId, "Company Admin") }.ToAsyncEnumerable());
        A.CallTo(() => _rolesRepository.GetUserRoleDataUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.Matches(x => x.First(y => y.ClientId == ClientId).UserRoleNames.Count() > 1)))
            .Returns(userRoleData.ToAsyncEnumerable());

        A.CallTo(() => _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(Id))
            .Returns(companyInvitedUsers);

        A.CallTo(() => _provisioningManager.GetUserByUserName(CompanyUserId1.ToString()))
            .Returns(CentralUserId1);
        A.CallTo(() => _provisioningManager.GetUserByUserName(CompanyUserId2.ToString()))
            .Returns(CentralUserId2);
        A.CallTo(() => _provisioningManager.GetUserByUserName(CompanyUserId3.ToString()))
            .Returns(CentralUserId3);

        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(CentralUserId1, A<IDictionary<string, IEnumerable<string>>>.That.Matches(x => x[ClientId].First() == clientRoleNames.First(x => x.ClientId == ClientId).UserRoleNames.First())))
            .Returns(clientRoleNames.Select(x => (x.ClientId, x.UserRoleNames, default(Exception?))).ToAsyncEnumerable());
        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(CentralUserId2, A<IDictionary<string, IEnumerable<string>>>.That.Matches(x => x[ClientId].First() == clientRoleNames.First(x => x.ClientId == ClientId).UserRoleNames.First())))
            .Returns(clientRoleNames.Select(x => (x.ClientId, x.UserRoleNames, default(Exception?))).ToAsyncEnumerable());
        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(CentralUserId3, A<IDictionary<string, IEnumerable<string>>>.That.Matches(x => x[ClientId].First() == clientRoleNames.First(x => x.ClientId == ClientId).UserRoleNames.First())))
            .Returns(clientRoleNames.Select(x => (x.ClientId, x.UserRoleNames, default(Exception?))).ToAsyncEnumerable());

        A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(CentralUserId1, businessPartnerNumbers))
            .Returns(Task.CompletedTask);
        A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(CentralUserId2, businessPartnerNumbers))
            .Returns(Task.CompletedTask);
        A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(CentralUserId3, businessPartnerNumbers))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(CompanyUserId1, CompanyUserRoleId))
            .Returns(companyUserAssignedRole);
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(CompanyUserId3, CompanyUserRoleId))
            .Returns(companyUserAssignedRole);

        A.CallTo(() =>
                _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId1, BusinessPartnerNumber))
            .Returns(companyUserAssignedBusinessPartner);
        A.CallTo(() =>
                _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId3, BusinessPartnerNumber))
            .Returns(companyUserAssignedBusinessPartner);

        A.CallTo(() => _portalRepositories.SaveAsync())
            .Returns(1);

        async IAsyncEnumerable<Guid> CreateNotificationsUserIds(IEnumerable<Guid> userIds)
        {
            foreach (var userId in userIds)
            {
                _notifiedUserIds.Add(userId);
                yield return userId;
                await Task.CompletedTask;
            }
        }

        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._))
            .ReturnsLazily((
                IEnumerable<UserRoleConfig> _,
                Guid? creatorId,
                IEnumerable<(string? content, NotificationTypeId notificationTypeId)> notifications,
                Guid _,
                bool? done) =>
            {
                _notifications.AddRange(
                    notifications.Select(notificationData =>
                        new Notification(Guid.NewGuid(), Guid.NewGuid(),
                            DateTimeOffset.UtcNow, notificationData.notificationTypeId, false)
                        {
                            CreatorUserId = creatorId,
                            Content = notificationData.content,
                            Done = done
                        }));

                return CreateNotificationsUserIds(new[] {
                    CompanyUserId1,
                    CompanyUserId2,
                    CompanyUserId3
                });
            });
    }

    private void SetupForDelete()
    {
        var userData = new (Guid CompanyUserId, IEnumerable<Guid> UserRoleIds)[]
        {
            new (CompanyUserId1, new [] {CompanyUserRoleId}),
            new (CompanyUserId2, new [] {CompanyUserRoleId}),
            new (CompanyUserId3, new [] {CompanyUserRoleId}),
        };
        var userRoles = new (string ClientClientId, IEnumerable<(Guid UserRoleId, string UserRoleText)>)[] {
            ( "remove-id", new [] { ( CompanyUserRoleId, "Company Admin" ) } )
        };

        A.CallTo(() => _rolesRepository.GetUserRolesByClientId(A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "remove-id" })))
            .Returns(userRoles.ToAsyncEnumerable());

        A.CallTo(() => _rolesRepository.GetUserWithUserRolesForApplicationId(A<Guid>._, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { CompanyUserRoleId })))
            .Returns(userData.ToAsyncEnumerable());
    }

    #endregion
}
