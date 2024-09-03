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
using Org.Eclipse.TractusX.Portal.Backend.ApplicationActivation.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;
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
    # region Initialization

    private const string BusinessPartnerNumber = "CAXLSHAREDIDPZZ";
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

    private static readonly ImmutableDictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId> Checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
        {
            { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
            { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
            { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
            { ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.DONE },
            { ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.DONE }
        }
        .ToImmutableDictionary();

    private readonly IFixture _fixture;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUserBusinessPartnerRepository _businessPartnerRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserRolesRepository _rolesRepository;
    private readonly IProcessStepRepository<ProcessTypeId, ProcessStepTypeId> _processStepRepository;
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
        var portalRepositories = A.Fake<IPortalRepositories>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _businessPartnerRepository = A.Fake<IUserBusinessPartnerRepository>();
        _mailingProcessCreation = A.Fake<IMailingProcessCreation>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _rolesRepository = A.Fake<IUserRolesRepository>();
        _notificationService = A.Fake<INotificationService>();
        _dateTimeProvider = A.Fake<IDateTimeProvider>();
        _custodianService = A.Fake<ICustodianService>();
        _settings = A.Fake<ApplicationActivationSettings>();
        _processStepRepository = A.Fake<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>();

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
        A.CallTo(() => portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => portalRepositories.GetInstance<IUserBusinessPartnerRepository>()).Returns(_businessPartnerRepository);
        A.CallTo(() => portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_rolesRepository);
        A.CallTo(() => portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => portalRepositories.GetInstance<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>()).Returns(_processStepRepository);
        A.CallTo(() => options.Value).Returns(_settings);

        _sut = new ApplicationActivationService(portalRepositories, _notificationService, _provisioningManager, _dateTimeProvider, _custodianService, _mailingProcessCreation, options);
    }

    #endregion

    #region START_APPLICATION_ACTIVATION

    [Fact]
    public async Task StartApplicationActivation_OutsideConfiguredTime_DoesntActivateApplication()
    {
        //Arrange
        A.CallTo(() => _dateTimeProvider.Now).Returns(new DateTime(2022, 01, 01, 12, 0, 0));
        _settings.StartTime = TimeSpan.FromHours(4);
        _settings.EndTime = TimeSpan.FromHours(8);
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            Id,
            default,
            new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO }
            }.ToImmutableDictionary(),
            Enumerable.Empty<ProcessStepTypeId>());

        //Act
        var result = await _sut.StartApplicationActivation(context, CancellationToken.None);

        //Assert
        result.Modified.Should().BeFalse();
        result.ModifyChecklistEntry.Should().BeNull();
        result.ProcessMessage.Should().BeNull();
        result.StepStatusId.Should().Be(ProcessStepStatusId.TODO);
        result.ScheduleStepTypeIds.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task StartApplicationActivation_WithNotAllStepsInDone_ThrowsConflictException()
    {
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO }
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(Id, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _dateTimeProvider.Now).Returns(new DateTime(2022, 01, 01, 0, 0, 0));
        _settings.StartTime = TimeSpan.FromHours(22);
        _settings.EndTime = TimeSpan.FromHours(8);
        Task Act() => _sut.StartApplicationActivation(context, CancellationToken.None);

        //Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        //Assert
        ex.Message.Should().Be($"cannot activate application {context.ApplicationId}. Checklist entries that are not in status DONE: [REGISTRATION_VERIFICATION, DONE],[BUSINESS_PARTNER_NUMBER, DONE],[IDENTITY_WALLET, DONE],[CLEARING_HOUSE, DONE],[SELF_DESCRIPTION_LP, TO_DO]");
    }

    [Theory]
    [InlineData(ApplicationChecklistEntryStatusId.DONE)]
    [InlineData(ApplicationChecklistEntryStatusId.SKIPPED)]
    public async Task StartApplicationActivation_WithApplicationActivationPossible_SetsChecklistToActive(ApplicationChecklistEntryStatusId sdFactoryStatusId)
    {
        var applicationChecklistEntry = new ApplicationChecklistEntry(Id, ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(Id, default, new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, sdFactoryStatusId }
            }
            .ToImmutableDictionary(), Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _dateTimeProvider.Now).Returns(new DateTime(2022, 01, 01, 0, 0, 0));
        _settings.StartTime = TimeSpan.FromHours(22);
        _settings.EndTime = TimeSpan.FromHours(8);

        //Act
        var result = await _sut.StartApplicationActivation(context, CancellationToken.None);

        //Assert
        result.Modified.Should().BeTrue();
        result.SkipStepTypeIds.Should().HaveCount(Enum.GetValues<ProcessStepTypeId>().Length - 1).And.NotContain(ProcessStepTypeId.START_APPLICATION_ACTIVATION);
        result.ModifyChecklistEntry?.Should().NotBeNull();
        result.StepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ScheduleStepTypeIds.Should().ContainSingle()
            .And.Satisfy(x => x == ProcessStepTypeId.ASSIGN_INITIAL_ROLES);
        result.ProcessMessage.Should().BeNull();
        result.ModifyChecklistEntry?.Invoke(applicationChecklistEntry);
        applicationChecklistEntry.ApplicationChecklistEntryStatusId.Should()
            .Be(ApplicationChecklistEntryStatusId.IN_PROGRESS);
    }

    #endregion

    #region ASSIGN_INITIAL_ROLES

    [Fact]
    public async Task AssignRoles_WithWrongRoleConfiguration_ThrowsConfigurationException()
    {
        A.CallTo(() => _rolesRepository.GetUserRoleDataUntrackedAsync(A<IEnumerable<UserRoleConfig>>._))
            .Returns(new UserRoleData[] { new(UserRoleId, ClientId, "Company Admin") }.ToAsyncEnumerable());

        _settings.ApplicationApprovalInitialRoles = new[]
        {
            new UserRoleConfig(ClientId, new[] { "Company Admin", "notexistingrole" })
        };
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(Id, default, Checklist, Enumerable.Empty<ProcessStepTypeId>());
        Task Act() => _sut.AssignRoles(context, CancellationToken.None);

        //Act
        var ex = await Assert.ThrowsAsync<ConfigurationException>(Act);

        //Assert
        ex.Message.Should().Be("invalid configuration, at least one of the configured roles does not exist in the database: client: catenax-portal, roles: [Company Admin, notexistingrole]");
    }

    [Fact]
    public async Task AssignRoles_WithUnassignedRoles_ThrowsUnexpectedConditionException()
    {
        //Arrange
        var roles = new[] { "Company Admin" };
        var clientRoleNames = new[]
        {
            new UserRoleConfig(ClientId, roles.AsEnumerable())
        };
        var userRoleData = new UserRoleData[] { new(UserRoleId, ClientId, "Company Admin"), new(Guid.NewGuid(), ClientId, "IT Admin") };
        A.CallTo(() => _rolesRepository.GetUserRoleDataUntrackedAsync(A<IEnumerable<UserRoleConfig>>._))
            .Returns(userRoleData.ToAsyncEnumerable());
        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(A<string>._, A<IDictionary<string, IEnumerable<string>>>._))
            .Returns(clientRoleNames.Select(x => (x.ClientId, x.UserRoleNames, default(Exception?))).ToAsyncEnumerable());
        A.CallTo(() => _applicationRepository.GetInvitedUsersWithoutInitialRoles(Id, A<IEnumerable<Guid>>._))
            .Returns(Enumerable.Repeat(new CompanyInvitedUserData(CompanyUserId1, Enumerable.Empty<Guid>()), 1).ToAsyncEnumerable());
        _settings.ApplicationApprovalInitialRoles = new[]
        {
            new UserRoleConfig(ClientId, new[] { "Company Admin", "IT Admin" })
        };
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            Id,
            default,
            Checklist,
            Enumerable.Empty<ProcessStepTypeId>());
        Task Act() => _sut.AssignRoles(context, CancellationToken.None);

        //Act
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        //Assert
        ex.Message.Should().Be("inconsistent data, roles not assigned in keycloak: client: catenax-portal, roles: [IT Admin], error: ");
    }

    [Fact]
    public async Task AssignRoles_WithMultipleRoles_AssignsRoles()
    {
        //Arrange
        var roles = new[] { "Company Admin" };
        var clientRoleNames = new[]
        {
            new UserRoleConfig(ClientId, roles.AsEnumerable())
        };
        var userRoleData = new UserRoleData[] { new(UserRoleId, ClientId, "Company Admin") };

        A.CallTo(() => _rolesRepository.GetUserRoleDataUntrackedAsync(A<IEnumerable<UserRoleConfig>>._))
            .Returns(userRoleData.ToAsyncEnumerable());
        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(A<string>._, A<IDictionary<string, IEnumerable<string>>>._))
            .Returns(clientRoleNames.Select(x => (x.ClientId, x.UserRoleNames, default(Exception?))).ToAsyncEnumerable());
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(A<Guid>._, A<Guid>._))
            .Returns(_fixture.Create<IdentityAssignedRole>());
        var companyInvitedUsers = new CompanyInvitedUserData[]
        {
            new(CompanyUserId1, Enumerable.Empty<Guid>()),
            new(CompanyUserId2, Enumerable.Empty<Guid>())
        }.ToAsyncEnumerable();
        A.CallTo(() => _applicationRepository.GetInvitedUsersWithoutInitialRoles(Id, A<IEnumerable<Guid>>._))
            .Returns(companyInvitedUsers);

        _settings.ApplicationApprovalInitialRoles = clientRoleNames;
        _settings.CompanyAdminRoles = new[]
        {
            new UserRoleConfig(ClientId, new[] { "Company Admin" }.AsEnumerable())
        };

        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            Id,
            default,
            Checklist,
            Enumerable.Empty<ProcessStepTypeId>());

        //Act
        var result = await _sut.AssignRoles(context, CancellationToken.None);

        //Assert
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRoleRange(A<IEnumerable<(Guid, Guid)>>.That.Matches(x => x.Count() == 1 && x.Single().Item1 == CompanyUserId1 && x.Single().Item2 == UserRoleId))).MustHaveHappenedOnceExactly();
        result.ScheduleStepTypeIds.Should().ContainSingle().And.Satisfy(x => x == ProcessStepTypeId.ASSIGN_INITIAL_ROLES);
        result.ModifyChecklistEntry.Should().BeNull();
        result.ProcessMessage.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
        result.Modified.Should().BeTrue();
    }

    [Fact]
    public async Task AssignRoles_WithValid_AssignsRoles()
    {
        //Arrange
        var clientRoleNames = new[]
        {
            new UserRoleConfig(ClientId, new[] { "Company Admin" })
        };
        A.CallTo(() => _rolesRepository.GetUserRoleDataUntrackedAsync(A<IEnumerable<UserRoleConfig>>._))
            .Returns(new UserRoleData[] { new(UserRoleId, ClientId, "Company Admin") }.ToAsyncEnumerable());
        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(A<string>._, A<IDictionary<string, IEnumerable<string>>>._))
            .Returns(clientRoleNames.Select(x => (x.ClientId, x.UserRoleNames, default(Exception?))).ToAsyncEnumerable());
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRole(A<Guid>._, A<Guid>._))
            .Returns(_fixture.Create<IdentityAssignedRole>());
        A.CallTo(() => _applicationRepository.GetInvitedUsersWithoutInitialRoles(Id, A<IEnumerable<Guid>>._))
            .Returns(Enumerable.Repeat(new CompanyInvitedUserData(CompanyUserId1, Enumerable.Empty<Guid>()), 1).ToAsyncEnumerable());

        _settings.ApplicationApprovalInitialRoles = clientRoleNames;
        _settings.CompanyAdminRoles = new[]
        {
            new UserRoleConfig(ClientId, new[] { "Company Admin" }.AsEnumerable())
        };

        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            Id,
            default,
            Checklist,
            Enumerable.Empty<ProcessStepTypeId>());

        //Act
        var result = await _sut.AssignRoles(context, CancellationToken.None);

        //Assert
        A.CallTo(() => _rolesRepository.CreateIdentityAssignedRoleRange(A<IEnumerable<(Guid, Guid)>>.That.Matches(x => x.Count() == 1 && x.Single().Item1 == CompanyUserId1 && x.Single().Item2 == UserRoleId))).MustHaveHappenedOnceExactly();
        result.ScheduleStepTypeIds.Should().ContainSingle().And.Satisfy(x => x == ProcessStepTypeId.ASSIGN_BPN_TO_USERS);
        result.ModifyChecklistEntry.Should().BeNull();
        result.ProcessMessage.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
        result.Modified.Should().BeTrue();
    }

    #endregion

    #region ASSIGN_BPN

    [Fact]
    public async Task AssignBpn_WithMissingBpn_ThrowsConflictException()
    {
        A.CallTo(() => _applicationRepository.GetBpnForApplicationIdAsync(Id)).Returns<string?>(null);
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(Id, default, Checklist, Enumerable.Empty<ProcessStepTypeId>());
        Task Act() => _sut.AssignBpn(context, CancellationToken.None);

        //Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        //Assert
        ex.Message.Should().Be("BusinessPartnerNumber must be set");
    }

    [Fact]
    public async Task AssignBpn_WithValid_AssignsBpnToUser()
    {
        //Arrange
        A.CallTo(() => _applicationRepository.GetBpnForApplicationIdAsync(Id)).Returns(BusinessPartnerNumber);
        A.CallTo(() => _applicationRepository.GetInvitedUserDataByApplicationWithoutBpn(Id))
            .Returns(Enumerable.Repeat(CompanyUserId1, 1).ToAsyncEnumerable());
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            Id,
            default,
            Checklist,
            Enumerable.Empty<ProcessStepTypeId>());

        //Act
        var result = await _sut.AssignBpn(context, CancellationToken.None);

        //Assert
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId1, BusinessPartnerNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(A<string>._, A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
        result.ScheduleStepTypeIds.Should().ContainSingle().And.Satisfy(x => x == ProcessStepTypeId.REMOVE_REGISTRATION_ROLES);
        result.StepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ModifyChecklistEntry.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
        result.ProcessMessage.Should().BeNull();
        result.Modified.Should().BeTrue();
    }

    [Fact]
    public async Task AssignBpn_WithMultipleUsers_AssignsBpnToUser()
    {
        //Arrange
        A.CallTo(() => _applicationRepository.GetBpnForApplicationIdAsync(Id)).Returns(BusinessPartnerNumber);
        A.CallTo(() => _applicationRepository.GetInvitedUserDataByApplicationWithoutBpn(Id))
            .Returns(new[] { CompanyUserId1, CompanyUserId3 }.ToAsyncEnumerable());
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            Id,
            default,
            Checklist,
            Enumerable.Empty<ProcessStepTypeId>());

        //Act
        var result = await _sut.AssignBpn(context, CancellationToken.None);

        //Assert
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId1, BusinessPartnerNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(A<string>._, A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
        result.ScheduleStepTypeIds.Should().ContainSingle().And.Satisfy(x => x == ProcessStepTypeId.ASSIGN_BPN_TO_USERS);
        result.StepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ModifyChecklistEntry.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
        result.ProcessMessage.Should().BeNull();
        result.Modified.Should().BeTrue();
    }

    #endregion

    #region REMOVE_REGISTRATION_ROLES

    [Fact]
    public async Task RemoveRegistrationRoles_WithMissingRoles_ThrowsUnexpectedConditionException()
    {
        A.CallTo(() => _rolesRepository.GetUsersWithUserRolesForApplicationId(Id, A<IEnumerable<string>>._))
            .Returns(Enumerable.Repeat(new ValueTuple<Guid, IEnumerable<(string, Guid, string)>>(Guid.NewGuid(), Enumerable.Empty<(string, Guid, string)>()), 1).ToAsyncEnumerable());

        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(Id, default, Checklist, Enumerable.Empty<ProcessStepTypeId>());
        Task Act() => _sut.RemoveRegistrationRoles(context, CancellationToken.None);

        //Act
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        //Assert
        ex.Message.Should().Be("userRoleIds should never be empty here");
    }

    [Fact]
    public async Task RemoveRegistrationRoles_WithMultipleUsers_ReturnsExpected()
    {
        //Arrange
        var userData = new (Guid CompanyUserId, IEnumerable<(string, Guid, string)> UserRoleIds)[]
        {
            new(CompanyUserId1, new[] { ("remove-id", CompanyUserRoleId, "remove") }),
            new(CompanyUserId2, new[] { ("remove-id", CompanyUserRoleId, "remove") })
        };
        A.CallTo(() => _rolesRepository.GetUsersWithUserRolesForApplicationId(A<Guid>._, A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "remove-id" })))
            .Returns(userData.ToAsyncEnumerable());
        SetupGetUserByUserName();

        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            Id,
            default,
            Checklist,
            Enumerable.Empty<ProcessStepTypeId>());

        //Act
        var result = await _sut.RemoveRegistrationRoles(context, CancellationToken.None);

        //Assert
        A.CallTo(() => _provisioningManager.DeleteClientRolesFromCentralUserAsync(CentralUserId1, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteClientRolesFromCentralUserAsync(CentralUserId2, A<IDictionary<string, IEnumerable<string>>>._)).MustNotHaveHappened();
        result.StepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ScheduleStepTypeIds.Should().ContainSingle()
            .And.Satisfy(x => x == ProcessStepTypeId.REMOVE_REGISTRATION_ROLES);
        result.ModifyChecklistEntry.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
        result.Modified.Should().BeTrue();
        result.ProcessMessage.Should().BeNull();
    }

    [Fact]
    public async Task RemoveRegistrationRoles_WithValid_ReturnsExpected()
    {
        //Arrange
        var userData = new (Guid CompanyUserId, IEnumerable<(string, Guid, string)> UserRoleIds)[]
        {
            new(CompanyUserId1, new[] { ("remove-id", CompanyUserRoleId, "remove") }),
        };
        A.CallTo(() => _rolesRepository.GetUsersWithUserRolesForApplicationId(A<Guid>._, A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "remove-id" })))
            .Returns(userData.ToAsyncEnumerable());
        SetupGetUserByUserName();

        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            Id,
            default,
            Checklist,
            Enumerable.Empty<ProcessStepTypeId>());

        //Act
        var result = await _sut.RemoveRegistrationRoles(context, CancellationToken.None);

        //Assert
        A.CallTo(() => _provisioningManager.DeleteClientRolesFromCentralUserAsync(CentralUserId1, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
        result.StepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ScheduleStepTypeIds.Should().ContainSingle()
            .And.Satisfy(x => x == ProcessStepTypeId.SET_THEME);
        result.ModifyChecklistEntry.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
        result.Modified.Should().BeTrue();
        result.ProcessMessage.Should().BeNull();
    }

    #endregion

    #region SET_THEME

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SetTheme_WithValid_ReturnsExpected(bool useDimWallet)
    {
        //Arrange
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            Id,
            default,
            Checklist,
            Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetSharedIdpAliasseForApplicationId(Id))
            .Returns(Enumerable.Repeat("idp1", 1).ToAsyncEnumerable());
        _settings.UseDimWallet = useDimWallet;

        //Act
        var result = await _sut.SetTheme(context, CancellationToken.None);

        //Assert
        var expectedProcessStepTypeId =
            useDimWallet ? ProcessStepTypeId.FINISH_APPLICATION_ACTIVATION : ProcessStepTypeId.SET_MEMBERSHIP;
        A.CallTo(() => _provisioningManager.UpdateSharedRealmTheme("idp1", _settings.LoginTheme)).MustHaveHappenedOnceExactly();
        result.StepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ScheduleStepTypeIds.Should().ContainSingle()
            .And.Satisfy(x => x == expectedProcessStepTypeId);
        result.ModifyChecklistEntry.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
        result.Modified.Should().BeTrue();
        result.ProcessMessage.Should().BeNull();
    }

    #endregion

    #region SET_MEMBERSHIP

    [Fact]
    public async Task SetMembership_WithMissingBpn_ThrowsConflictException()
    {
        A.CallTo(() => _applicationRepository.GetBpnForApplicationIdAsync(Id)).Returns<string?>(null);
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(Id, default, Checklist, Enumerable.Empty<ProcessStepTypeId>());
        Task Act() => _sut.SetMembership(context, CancellationToken.None);

        //Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        //Assert
        ex.Message.Should().Be("BusinessPartnerNumber must be set");
    }

    [Fact]
    public async Task SetMembership_WithValid_ReturnsExpected()
    {
        //Arrange
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            Id,
            default,
            Checklist,
            Enumerable.Empty<ProcessStepTypeId>());
        var applicationChecklistEntry = new ApplicationChecklistEntry(Id, ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        A.CallTo(() => _applicationRepository.GetBpnForApplicationIdAsync(Id))
            .Returns(BusinessPartnerNumber);
        A.CallTo(() => _custodianService.SetMembership(BusinessPartnerNumber, A<CancellationToken>._))
            .Returns("testMessage");

        //Act
        var result = await _sut.SetMembership(context, CancellationToken.None);

        //Assert
        result.StepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ScheduleStepTypeIds.Should().ContainSingle()
            .And.Satisfy(x => x == ProcessStepTypeId.FINISH_APPLICATION_ACTIVATION);
        result.ModifyChecklistEntry?.Should().NotBeNull();
        result.ModifyChecklistEntry?.Invoke(applicationChecklistEntry);
        applicationChecklistEntry.Comment.Should()
            .Be("testMessage");
        result.SkipStepTypeIds.Should().BeNull();
        result.Modified.Should().BeTrue();
        result.ProcessMessage.Should().BeNull();
    }

    #endregion

    #region FINISH_APPLICATION_ACTIVATION

    [Fact]
    public async Task SaveApplicationActivationToDatabase_WithCompanyAdminUser_ApprovesRequestAndCreatesNotifications()
    {
        //Arrange
        var companyApplication = _fixture.Build<CompanyApplication>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .Create();
        var company = _fixture.Build<Company>()
            .With(x => x.CompanyStatusId, CompanyStatusId.PENDING)
            .Create();
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForApprovalAsync(A<Guid>.That.Matches(x => x == Id)))
            .Returns((company.Id, company.Name, company.BusinessPartnerNumber, CompanyApplicationTypeId.INTERNAL, null));
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
        var welcomeEmailData = new EmailData[]
        {
            new(CompanyUserId1, "Stan", "Lee", "stan@lee.com"),
            new(CompanyUserId2, "Tony", "Stark", "tony@stark.com"),
            new(CompanyUserId3, "Peter", "Parker", "peter@parker.com")
        };
        A.CallTo(() => _applicationRepository.GetEmailDataUntrackedAsync(Id))
            .Returns(welcomeEmailData.ToAsyncEnumerable());
        SetupNotifications();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            Id,
            default,
            Checklist,
            Enumerable.Empty<ProcessStepTypeId>());

        //Act
        var result = await _sut.SaveApplicationActivationToDatabase(context, CancellationToken.None);

        //Assert
        A.CallTo(() => _mailingProcessCreation.CreateMailProcess(A<string>._, "EmailRegistrationWelcomeTemplate", A<IReadOnlyDictionary<string, string>>._))
            .MustHaveHappened(3, Times.Exactly);
        _notifications.Should().HaveCount(5);
        _notifiedUserIds.Should().HaveCount(3)
            .And.ContainInOrder(CompanyUserId1, CompanyUserId2, CompanyUserId3);
        companyApplication.ApplicationStatusId.Should().Be(CompanyApplicationStatusId.CONFIRMED);
        company.CompanyStatusId.Should().Be(CompanyStatusId.ACTIVE);
        var entry = new ApplicationChecklistEntry(Guid.NewGuid(), ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO, default);
        result.ModifyChecklistEntry!.Invoke(entry);
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
        result.ScheduleStepTypeIds.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
        result.Modified.Should().BeTrue();
    }

    [Fact]
    public async Task SaveApplicationActivationToDatabase_WithoutNetworkRegistrationId_ThrowsConflictException()
    {
        //Arrange
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForApprovalAsync(A<Guid>.That.Matches(x => x == IdWithTypeExternalWithoutProcess)))
            .Returns((Guid.NewGuid(), CompanyName, BusinessPartnerNumber, CompanyApplicationTypeId.EXTERNAL, null));

        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            IdWithTypeExternalWithoutProcess,
            default,
            Checklist,
            Enumerable.Empty<ProcessStepTypeId>());

        //Act
        async Task Act() => await _sut.SaveApplicationActivationToDatabase(context, CancellationToken.None);

        //Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("ProcessId should be set for external applications");
    }

    [Fact]
    public async Task SaveApplicationActivationToDatabase_WithExternalApplication_ApprovesRequestAndCreatesNotifications()
    {
        //Arrange
        var processSteps = new List<ProcessStep<ProcessTypeId, ProcessStepTypeId>>();
        var companyApplication = _fixture.Build<CompanyApplication>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .Create();
        var company = _fixture.Build<Company>()
            .With(x => x.CompanyStatusId, CompanyStatusId.PENDING)
            .Create();
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForApprovalAsync(A<Guid>.That.Matches(x => x == IdWithTypeExternal)))
            .Returns((company.Id, company.Name, company.BusinessPartnerNumber, CompanyApplicationTypeId.EXTERNAL, new(new(ProcessId, ProcessTypeId.PARTNER_REGISTRATION, Guid.NewGuid()), [])));
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
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)>>.That.Matches(x =>
                x.Count() == 1 &&
                x.Single().ProcessStepTypeId == ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED)))
            .Invokes((IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> steps) =>
            {
                processSteps.AddRange(steps.Select(x => new ProcessStep<ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), x.ProcessStepTypeId, x.ProcessStepStatusId, x.ProcessId, DateTimeOffset.UtcNow)));
            });
        SetupNotifications();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            IdWithTypeExternal,
            default,
            Checklist,
            Enumerable.Empty<ProcessStepTypeId>());

        //Act
        var result = await _sut.SaveApplicationActivationToDatabase(context, CancellationToken.None);

        //Assert
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
        result.ScheduleStepTypeIds.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
        result.Modified.Should().BeTrue();
    }

    [Fact]
    public async Task SaveApplicationActivationToDatabase_WithDefaultApplicationId_ThrowsConflictException()
    {
        // Arrange
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(Guid.Empty, default, Checklist, Enumerable.Empty<ProcessStepTypeId>());

        //Act
        async Task Action() => await _sut.SaveApplicationActivationToDatabase(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Action);
        ex.Message.Should().Be($"BusinessPartnerNumber (bpn) for CompanyApplications {Guid.Empty} company {Guid.Empty} is empty");
    }

    [Fact]
    public async Task SaveApplicationActivationToDatabase_WithoutCompanyApplication_ThrowsConflictException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            applicationId,
            default,
            Checklist,
            Enumerable.Empty<ProcessStepTypeId>());

        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForApprovalAsync(applicationId))
            .Returns<(Guid, string, string?, CompanyApplicationTypeId, VerifyProcessData<ProcessTypeId, ProcessStepTypeId>?)>(default);

        //Act
        async Task Action() => await _sut.SaveApplicationActivationToDatabase(context, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Action);
        ex.Message.Should().Be($"CompanyApplication {applicationId} is not in status SUBMITTED");
    }

    [Fact]
    public async Task SaveApplicationActivationToDatabase_WithCompanyWithoutBPN_ThrowsConflictException()
    {
        //Act
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            IdWithoutBpn,
            default,
            Checklist,
            Enumerable.Empty<ProcessStepTypeId>());

        async Task Action() => await _sut.SaveApplicationActivationToDatabase(context, CancellationToken.None);

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

    private void SetupNotifications()
    {
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

    private void SetupGetUserByUserName()
    {
        A.CallTo(() => _provisioningManager.GetUserByUserName(CompanyUserId1.ToString()))
            .Returns(CentralUserId1);
        A.CallTo(() => _provisioningManager.GetUserByUserName(CompanyUserId2.ToString()))
            .Returns(CentralUserId2);
        A.CallTo(() => _provisioningManager.GetUserByUserName(CompanyUserId3.ToString()))
            .Returns(CentralUserId3);
    }

    #endregion
}
