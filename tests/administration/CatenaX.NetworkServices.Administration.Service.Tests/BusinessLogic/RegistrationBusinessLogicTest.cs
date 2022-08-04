/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Custodian;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CatenaX.NetworkServices.Administration.Service.Tests.BusinessLogic
{
    public class RegistrationBusinessLogicTest
    {
        private static readonly Guid Id = new("d90995fe-1241-4b8d-9f5c-f3909acc6383");
        private static readonly string IamUserId = new Guid("4C1A6851-D4E7-4E10-A011-3732CD045E8A").ToString();
        private static readonly Guid CompanyUserId1 = new("857b93b1-8fcb-4141-81b0-ae81950d489e");
        private static readonly Guid CompanyUserId2 = new("857b93b1-8fcb-4141-81b0-ae81950d489f");
        private static readonly Guid CompanyUserId3 = new("857b93b1-8fcb-4141-81b0-ae81950d48af");
        private static readonly Guid CompanyUserRoleId = new("607818be-4978-41f4-bf63-fa8d2de51154");
        private static readonly Guid CentralUserId1 = new("6bc51706-9a30-4eb9-9e60-77fdd6d9cd6f");
        private static readonly Guid CentralUserId2 = new("6bc51706-9a30-4eb9-9e60-77fdd6d9cd70");
        private static readonly Guid CentralUserId3 = new("6bc51706-9a30-4eb9-9e60-77fdd6d9cd71");
        private static readonly Guid UserRoleId = new("607818be-4978-41f4-bf63-fa8d2de51154");
        private const string BusinessPartnerNumber = "CAXLSHAREDIDPZZ";
        private const string CompanyName = "Shared Idp Test";
        private const string ClientId = "catenax-portal";

        private readonly Guid _companyAdminRoleId = Guid.NewGuid();
        private readonly IProvisioningManager _provisioningManager;
        private readonly IPortalRepositories _portalRepositories;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IUserBusinessPartnerRepository _businessPartnerRepository;
        private readonly IUserRolesRepository _rolesRepository;
        private readonly ICustodianService _custodianService;
        private readonly IFixture _fixture;
        private readonly IRegistrationBusinessLogic _logic;
        private readonly RegistrationSettings _settings;
        private readonly INotificationRepository _notificationRepository;
        private readonly List<Notification> _notifications = new();

        public RegistrationBusinessLogicTest()
        {
            _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());  

            _provisioningManager = A.Fake<IProvisioningManager>();
            _portalRepositories = A.Fake<IPortalRepositories>();
            _applicationRepository = A.Fake<IApplicationRepository>();
            _businessPartnerRepository = A.Fake<IUserBusinessPartnerRepository>();
            _rolesRepository = A.Fake<IUserRolesRepository>();
            _custodianService = A.Fake<ICustodianService>();
            _settings = A.Fake<RegistrationSettings>();
            _notificationRepository = A.Fake<INotificationRepository>();
            
            var userRepository = A.Fake<IUserRepository>();
            var mailingService = A.Fake<IMailingService>();
            var options = A.Fake<IOptions<RegistrationSettings>>();

            _settings.WelcomeNotificationTypeIds = new List<NotificationTypeId>
            {
                NotificationTypeId.WELCOME,
                NotificationTypeId.WELCOME_USE_CASES,
                NotificationTypeId.WELCOME_APP_MARKETPLACE,
                NotificationTypeId.WELCOME_SERVICE_PROVIDER,
                NotificationTypeId.WELCOME_CONNECTOR_REGISTRATION
            };
            
            A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
            A.CallTo(() => _portalRepositories.GetInstance<IUserBusinessPartnerRepository>()).Returns(_businessPartnerRepository);
            A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_rolesRepository);
            A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
            A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(userRepository);
            A.CallTo(() => options.Value).Returns(_settings);

            A.CallTo(() => userRepository.GetCompanyUserIdForIamUserUntrackedAsync(IamUserId))
                .ReturnsLazily(Guid.NewGuid);

            _logic = new RegistrationBusinessLogic(_portalRepositories, options, _provisioningManager, _custodianService, mailingService);
        }

        [Fact]
        public async Task ApprovePartnerRequest_WithoutCompanyAdmin_ApprovesRequestDoesntCreateNotifications()
        {
            //Arrange
            var roles = new List<string> { "IT Admin" };
            var clientRoleNames = new Dictionary<string, IEnumerable<string>>
                        {
                            { ClientId, roles.AsEnumerable() }
                        };
            var userRoleData = new List<UserRoleData>() { new(UserRoleId, ClientId, "IT Admin") };

            var companyUserAssignedRole = _fixture.Create<CompanyUserAssignedRole>();
            var companyUserAssignedBusinessPartner = _fixture.Create<CompanyUserAssignedBusinessPartner>();

            SetupFakes(clientRoleNames, userRoleData, companyUserAssignedRole, companyUserAssignedBusinessPartner, false);

            //Act
            var result = await _logic.ApprovePartnerRequest(IamUserId, Id).ConfigureAwait(false);

            //Assert
            A.CallTo(() => _applicationRepository.GetCompanyAndApplicationForSubmittedApplication(Id)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(Id)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(CompanyUserId1, UserRoleId)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId1, BusinessPartnerNumber)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(CompanyUserId2, UserRoleId)).MustNotHaveHappened();
            A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId2, BusinessPartnerNumber)).MustNotHaveHappened();
            A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(CompanyUserId3, UserRoleId)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId3, BusinessPartnerNumber)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened(1, Times.OrMore);
            A.CallTo(() => _custodianService.CreateWallet(BusinessPartnerNumber, CompanyName)).MustHaveHappened(1, Times.OrMore);
            Assert.IsType<bool>(result);
            Assert.True(result);
            _notifications.Should().BeEmpty();
        }

        [Fact]
        public async Task ApprovePartnerRequest_WithCompanyAdminUser_ApprovesRequestAndCreatesNotifications()
        {
            //Arrange
            var roles = new List<string> { "Company Admin" };
            var clientRoleNames = new Dictionary<string, IEnumerable<string>>
                        {
                            { ClientId, roles.AsEnumerable() }
                        };
            var userRoleData = new List<UserRoleData>() { new(UserRoleId, ClientId, "Company Admin") };

            var companyUserAssignedRole = _fixture.Create<CompanyUserAssignedRole>();
            var companyUserAssignedBusinessPartner = _fixture.Create<CompanyUserAssignedBusinessPartner>();

            SetupFakes(clientRoleNames, userRoleData, companyUserAssignedRole, companyUserAssignedBusinessPartner, true);

            //Act
            var result = await _logic.ApprovePartnerRequest(IamUserId, Id).ConfigureAwait(false);

            //Assert
            A.CallTo(() => _applicationRepository.GetCompanyAndApplicationForSubmittedApplication(Id)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(Id)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(CompanyUserId1, UserRoleId)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId1, BusinessPartnerNumber)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(CompanyUserId2, UserRoleId)).MustNotHaveHappened();
            A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId2, BusinessPartnerNumber)).MustNotHaveHappened();
            A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(CompanyUserId3, UserRoleId)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId3, BusinessPartnerNumber)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened(1, Times.OrMore);
            A.CallTo(() => _custodianService.CreateWallet(BusinessPartnerNumber, CompanyName)).MustHaveHappened(1, Times.OrMore);
            Assert.IsType<bool>(result);
            Assert.True(result);
            _notifications.Should().HaveCount(5);
        }

        [Fact]
        public async Task ApprovePartnerRequest_WithDefaultApplicationId_ThrowsArgumentNullException()
        {
            //Act
            async Task Action() => await _logic.ApprovePartnerRequest(IamUserId, Guid.Empty);
            // Assert
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(Action);
            ex.ParamName.Should().Be("applicationId");
        }

        [Fact]
        public async Task ApprovePartnerRequest_WithoutCompanyApplication_ThrowsNotFoundException()
        {
            // Arrange
            var applicationId = Guid.NewGuid();
            A.CallTo(() => _applicationRepository.GetCompanyAndApplicationForSubmittedApplication(applicationId))
                .ReturnsLazily(() => (CompanyApplication?)null);

            //Act
            async Task Action() => await _logic.ApprovePartnerRequest(IamUserId, applicationId);
            // Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
            ex.Message.Should().Be($"CompanyApplication {applicationId} is not in status SUBMITTED");
        }

        [Fact]
        public async Task ApprovePartnerRequest_WithCompanyWithoutBPN_ThrowsArgumentException()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var applicationId = companyId;
            var companyApplication = new CompanyApplication(applicationId, companyId, CompanyApplicationStatusId.CREATED, DateTimeOffset.UtcNow)
            {
                Company = new Company(companyId, "test", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow)
            };
            A.CallTo(() => _applicationRepository.GetCompanyAndApplicationForSubmittedApplication(applicationId))
                .ReturnsLazily(() => companyApplication);

            //Act
            async Task Action() => await _logic.ApprovePartnerRequest(IamUserId, applicationId);
            // Assert
            var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
            ex.Message.Should().Be($"BusinessPartnerNumber (bpn) for CompanyApplications {applicationId} company {companyApplication.CompanyId} is empty (Parameter 'bpn')");
            ex.ParamName.Should().Be($"bpn");
        }

        #region Setup

        private void SetupFakes(
            IDictionary<string, IEnumerable<string>> clientRoleNames,
            IEnumerable<UserRoleData> userRoleData,
            CompanyUserAssignedRole companyUserAssignedRole,
            CompanyUserAssignedBusinessPartner companyUserAssignedBusinessPartner,
            bool withCompanyAdmin)
        {
            var company = _fixture.Build<Company>()
                .With(u => u.BusinessPartnerNumber, BusinessPartnerNumber)
                .With(u => u.Name, CompanyName)
                .Create();
            var companyApplication = _fixture.Build<CompanyApplication>()
                .With(u => u.Company, company)
                .Create();

            var companyInvitedUsers = new List<CompanyInvitedUserData>
            {
                new(CompanyUserId1, CentralUserId1.ToString(), Enumerable.Empty<string>(), Enumerable.Empty<Guid>()),
                new(CompanyUserId2, CentralUserId2.ToString(), Enumerable.Repeat(BusinessPartnerNumber, 1), Enumerable.Repeat(UserRoleId, 1)),
                new(CompanyUserId3, CentralUserId3.ToString(), Enumerable.Empty<string>(), Enumerable.Empty<Guid>())
            }.ToAsyncEnumerable();
            var businessPartnerNumbers = new List<string> { BusinessPartnerNumber }.AsEnumerable();

            _settings.ApplicationApprovalInitialRoles = clientRoleNames;
            _settings.CompanyAdminRoles = new Dictionary<string, IEnumerable<string>>
            {
                { ClientId, new List<string> { "Company Admin" }.AsEnumerable() }
            };

            A.CallTo(() => _applicationRepository.GetCompanyAndApplicationForSubmittedApplication(Id))
                .Returns(companyApplication);

            var welcomeEmailData = new List<WelcomeEmailData>();
            welcomeEmailData.AddRange(new WelcomeEmailData[]
            {
                new (CompanyUserId1, "Stan", "Lee", "stan@lee.com", company.Name, withCompanyAdmin),
                new (CompanyUserId2, "Tony", "Stark", "tony@stark.com", company.Name, false),
                new (CompanyUserId3, "Peter", "Parker", "peter@parker.com", company.Name, false)
            });
            A.CallTo(() => _applicationRepository.GetWelcomeEmailDataUntrackedAsync(Id, A<IEnumerable<Guid>>._))
                .Returns(welcomeEmailData.ToAsyncEnumerable());
            A.CallTo(() => _applicationRepository.GetWelcomeEmailDataUntrackedAsync(A<Guid>.That.Not.Matches(x => x == Id), A<IEnumerable<Guid>>._))
                .Returns(new List<WelcomeEmailData>().ToAsyncEnumerable());

            A.CallTo(() => _rolesRepository.GetUserRoleDataUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>.That.Matches(x => x[ClientId].First() == clientRoleNames[ClientId].First())))
                .Returns(userRoleData.ToAsyncEnumerable());

            A.CallTo(() => _rolesRepository.GetUserRoleDataUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>.That.Matches(x => x[ClientId].First() == _settings.CompanyAdminRoles[ClientId].First())))
                .Returns(new List<UserRoleData>() { new(UserRoleId, ClientId, "Company Admin") }.ToAsyncEnumerable());

            A.CallTo(() => _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(Id))
                .Returns(companyInvitedUsers);

            A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(CentralUserId1.ToString(), clientRoleNames))
                .Returns(Task.FromResult(clientRoleNames));
            A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(CentralUserId2.ToString(), clientRoleNames))
                .Returns(Task.FromResult(clientRoleNames));
            A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(CentralUserId3.ToString(), clientRoleNames))
                .Returns(Task.FromResult(clientRoleNames));

            A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(CentralUserId1.ToString(), businessPartnerNumbers))
                .Returns(Task.CompletedTask);
            A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(CentralUserId2.ToString(), businessPartnerNumbers))
                .Returns(Task.CompletedTask);
            A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(CentralUserId3.ToString(), businessPartnerNumbers))
                .Returns(Task.CompletedTask);

            A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(CompanyUserId1, CompanyUserRoleId))
                .Returns(companyUserAssignedRole);
            A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(CompanyUserId3, CompanyUserRoleId))
                .Returns(companyUserAssignedRole);

            A.CallTo(() =>
                    _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId1, BusinessPartnerNumber))
                .Returns(companyUserAssignedBusinessPartner);
            A.CallTo(() =>
                    _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId3, BusinessPartnerNumber))
                .Returns(companyUserAssignedBusinessPartner);

            A.CallTo(() => _portalRepositories.SaveAsync())
                .Returns(1);

            A.CallTo(() => _custodianService.CreateWallet(BusinessPartnerNumber, CompanyName))
                .Returns(Task.CompletedTask);
            
            A.CallTo(() => _notificationRepository.Create(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>?>._))
                .Invokes(x =>
                {
                    var receiverId = x.Arguments.Get<Guid>("receiverUserId");
                    var notificationTypeId = x.Arguments.Get<NotificationTypeId>("notificationTypeId");
                    var isRead = x.Arguments.Get<bool>("isRead");
                    var action = x.Arguments.Get<Action<Notification?>>("setOptionalParameter");

                    var notification = new Notification(Guid.NewGuid(), receiverId,
                        DateTimeOffset.UtcNow, notificationTypeId, isRead);
                    action?.Invoke(notification);
                    _notifications.Add(notification);
                });
        }

        #endregion
    }
}
