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

            A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
            A.CallTo(() => _portalRepositories.GetInstance<IUserBusinessPartnerRepository>()).Returns(_businessPartnerRepository);
            A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_rolesRepository);
            A.CallTo(() => _options.Value).Returns(_settings);

            _logic = new RegistrationBusinessLogic(_portalRepositories, _options, _provisioningManager, _custodianService, _mailingService);
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
            Guid centralUserId = new Guid("6bc51706-9a30-4eb9-9e60-77fdd6d9cd6f");
            Guid userRoleId = new Guid("607818be-4978-41f4-bf63-fa8d2de51154");
            string userEntityId = "some entity id";
            string businessPartnerNumber = "CAXLSHAREDIDPZZ";
            string companyName = "Shared Idp Test";

            var company = _fixture.Build<Company>()
                .With(u => u.BusinessPartnerNumber, businessPartnerNumber)
                .With(u => u.Name, companyName)
                .Create();
            var companyApplication = _fixture.Build<CompanyApplication>()
                .With(u => u.Company, company)
                .Create();
            var clientId = "catenax-portal";
            var userRoleIds = new List<Guid>() { userRoleId };
            var roles = new List<string> { "IT Admin" };
            var clientRoleNames = new Dictionary<string, IEnumerable<string>>
                        {
                            { clientId, roles.AsEnumerable() }
                        };
            var companyInvitedUsers = new List<CompanyInvitedUserData>()
            {
                new(companyUserId1, userEntityId, Enumerable.Empty<string>(), Enumerable.Empty<Guid>()),
                new(companyUserId2, userEntityId, Enumerable.Repeat(businessPartnerNumber, 1), Enumerable.Repeat(userRoleId, 1)),
                new(companyUserId3, userEntityId, Enumerable.Empty<string>(), Enumerable.Empty<Guid>())
            }.ToAsyncEnumerable();

            var userRole = _fixture.Build<UserRoleData>()
                .With(x => x.UserRoleId, userRoleId)
                .CreateMany(1)
                .ToAsyncEnumerable();
            var companyUserAssignedRole = _fixture.Create<CompanyUserAssignedRole>();
            var companyUserAssignedBusinessPartner = _fixture.Create<CompanyUserAssignedBusinessPartner>();
            var bpns = new List<string> { businessPartnerNumber }.AsEnumerable();

            _settings.ApplicationApprovalInitialRoles = clientRoleNames;

            A.CallTo(() => _applicationRepository.GetCompanyAndApplicationForSubmittedApplication(id))
                .Returns(companyApplication);

            A.CallTo(() => _rolesRepository.GetUserRoleIdsUntrackedAsync(clientRoleNames))
                .Returns(userRoleIds.ToAsyncEnumerable());

            A.CallTo(() => _rolesRepository.GetUserRoleDataUntrackedAsync(clientRoleNames))
                .Returns(userRole);

            A.CallTo(() => _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(id))
                .Returns(companyInvitedUsers);

            A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(userEntityId.ToString(), A<Dictionary<string, IEnumerable<string>>>._))
                .ReturnsLazily(() => clientRoleNames);

            A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(centralUserId.ToString(), bpns))
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
