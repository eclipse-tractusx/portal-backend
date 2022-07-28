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

using AutoFixture;
using FakeItEasy;
using AutoFixture.AutoFakeItEasy;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.Administration.Service.Custodian;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using CatenaX.NetworkServices.Framework.Notifications;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.Administration.Service.Tests
{
    public class RegistrationBusinessLogicTest
    {

        private readonly IProvisioningManager _provisioningManager;
        private readonly IPortalRepositories _portalRepositories;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IUserBusinessPartnerRepository _businessPartnerRepository;
        private readonly IUserRolesRepository _rolesRepository;
        private readonly IMailingService _mailingService;
        private readonly ICustodianService _custodianService;
        private readonly IFixture _fixture;
        private readonly IRegistrationBusinessLogic _logic;
        private readonly IOptions<RegistrationSettings> _options;
        private readonly RegistrationSettings _settings;
<<<<<<< HEAD
        private readonly INotificationService _notificationService;
=======
        private readonly NotificationService _notificationService;
>>>>>>> f9d526c (CPLP-1247 add welcome notifications)

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
            _mailingService = A.Fake<IMailingService>();
            _custodianService = A.Fake<ICustodianService>();
            _options = A.Fake<IOptions<RegistrationSettings>>();
            _settings = A.Fake<RegistrationSettings>();
<<<<<<< HEAD
            _notificationService = A.Fake<INotificationService>();
=======
            _notificationService = A.Fake<NotificationService>();
>>>>>>> f9d526c (CPLP-1247 add welcome notifications)

            A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
            A.CallTo(() => _portalRepositories.GetInstance<IUserBusinessPartnerRepository>()).Returns(_businessPartnerRepository);
            A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_rolesRepository);
            A.CallTo(() => _options.Value).Returns(_settings);

            _logic = new RegistrationBusinessLogic(_portalRepositories, _options, _provisioningManager, _custodianService, _mailingService, _notificationService);
        }
        [Fact]
        public async Task Test3()
        {
            //Arrange
            Guid id = new Guid("d90995fe-1241-4b8d-9f5c-f3909acc6383");
            Guid companyUserId1 = new Guid("857b93b1-8fcb-4141-81b0-ae81950d489e");
            Guid companyUserId2 = new Guid("857b93b1-8fcb-4141-81b0-ae81950d489f");
            Guid companyUserId3 = new Guid("857b93b1-8fcb-4141-81b0-ae81950d48af");
            Guid companyUserRoleId = new Guid("607818be-4978-41f4-bf63-fa8d2de51154");
            Guid centralUserId1 = new Guid("6bc51706-9a30-4eb9-9e60-77fdd6d9cd6f");
            Guid centralUserId2 = new Guid("6bc51706-9a30-4eb9-9e60-77fdd6d9cd70");
            Guid centralUserId3 = new Guid("6bc51706-9a30-4eb9-9e60-77fdd6d9cd71");
            Guid userRoleId = new Guid("607818be-4978-41f4-bf63-fa8d2de51154");
            string businessPartnerNumber = "CAXLSHAREDIDPZZ";
            string companyName = "Shared Idp Test";

            var company = _fixture.Build<Company>()
                .With(u => u.BusinessPartnerNumber, businessPartnerNumber)
                .With(u => u.Name, companyName)
                .Create();
            var companyApplication = _fixture.Build<CompanyApplication>()
                .With(u => u.Company, company)
                .Create();
            string clientId = "catenax-portal";
            List<string> roles = new List<string> { "IT Admin" };
            var clientRoleNames = new Dictionary<string, IEnumerable<string>>
                        {
                            { clientId, roles.AsEnumerable() }
                        };
            List<UserRoleData> userRoleData = new List<UserRoleData>() { new UserRoleData(userRoleId, clientId, "IT Admin") };
            var companyInvitedUsers = new List<CompanyInvitedUserData>()
            {
                new CompanyInvitedUserData(companyUserId1, centralUserId1.ToString(), Enumerable.Empty<string>(), Enumerable.Empty<Guid>()),
                new CompanyInvitedUserData(companyUserId2, centralUserId2.ToString(), Enumerable.Repeat(businessPartnerNumber, 1), Enumerable.Repeat(userRoleId, 1)),
                new CompanyInvitedUserData(companyUserId3, centralUserId3.ToString(), Enumerable.Empty<string>(), Enumerable.Empty<Guid>())
            }.ToAsyncEnumerable();

            var companyUserAssignedRole = _fixture.Create<CompanyUserAssignedRole>();
            var companyUserAssignedBusinessPartner = _fixture.Create<CompanyUserAssignedBusinessPartner>();
            var bpns = new List<string> { businessPartnerNumber }.AsEnumerable();

            _settings.ApplicationApprovalInitialRoles = clientRoleNames;

            A.CallTo(() => _applicationRepository.GetCompanyAndApplicationForSubmittedApplication(id))
                .Returns(companyApplication);

            A.CallTo(() => _rolesRepository.GetUserRoleDataUntrackedAsync(clientRoleNames))
                .Returns(userRoleData.ToAsyncEnumerable());

            A.CallTo(() => _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(id))
                .Returns(companyInvitedUsers);

            A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(centralUserId1.ToString(), clientRoleNames))
                .Returns(Task.FromResult((IDictionary<string,IEnumerable<string>>)clientRoleNames));

            A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(centralUserId2.ToString(), clientRoleNames))
                .Returns(Task.FromResult((IDictionary<string,IEnumerable<string>>)clientRoleNames));

            A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(centralUserId3.ToString(), clientRoleNames))
                .Returns(Task.FromResult((IDictionary<string,IEnumerable<string>>)clientRoleNames));

            A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(centralUserId1.ToString(), bpns))
                .Returns(Task.CompletedTask);

            A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(centralUserId2.ToString(), bpns))
                .Returns(Task.CompletedTask);

            A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(centralUserId3.ToString(), bpns))
                .Returns(Task.CompletedTask);

            A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(companyUserId1, companyUserRoleId))
                .Returns(companyUserAssignedRole);

            A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(companyUserId3, companyUserRoleId))
                .Returns(companyUserAssignedRole);

            A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(companyUserId1, businessPartnerNumber))
                .Returns(companyUserAssignedBusinessPartner);

            A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(companyUserId3, businessPartnerNumber))
                .Returns(companyUserAssignedBusinessPartner);

            A.CallTo(() => _portalRepositories.SaveAsync())
                .Returns(1);

            A.CallTo(() => _custodianService.CreateWallet(businessPartnerNumber, companyName))
                .Returns(Task.CompletedTask);

            A.CallTo(() => _notificationService.CreateWelcomeNotificationsForCompanyAsync(company.Id))
                .ReturnsLazily(() => Task.CompletedTask);

            //Act
            var result = await _logic.ApprovePartnerRequest(id).ConfigureAwait(false);
            //Assert
            A.CallTo(() => _applicationRepository.GetCompanyAndApplicationForSubmittedApplication(id)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(id)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(companyUserId1, userRoleId)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(companyUserId1, businessPartnerNumber)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(companyUserId2, userRoleId)).MustNotHaveHappened();
            A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(companyUserId2, businessPartnerNumber)).MustNotHaveHappened();
            A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(companyUserId3, userRoleId)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(companyUserId3, businessPartnerNumber)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened(1, Times.OrMore);
            A.CallTo(() => _custodianService.CreateWallet(businessPartnerNumber, companyName)).MustHaveHappened(1, Times.OrMore);
            Assert.IsType<bool>(result);
            Assert.True(result);
        }
    }
}
