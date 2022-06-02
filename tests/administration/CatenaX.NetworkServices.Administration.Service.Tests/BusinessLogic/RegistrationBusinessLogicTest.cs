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
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.Administration.Service.Custodian;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.Extensions.Configuration;

namespace CatenaX.NetworkServices.Administration.Service.Tests
{
    public class RegistrationBusinessLogicTest
    {

        private readonly IProvisioningManager _provisioningManager;
        private readonly IPortalBackendDBAccess _portalDBAccess;
        private readonly IMailingService _mailingService;
        private readonly ICustodianService _custodianService;
        private readonly IFixture _fixture;
        private readonly IRegistrationBusinessLogic _logic;
        private readonly IOptions<RegistrationSettings> _settings;
        public RegistrationBusinessLogicTest()
        {
            _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
            _provisioningManager = A.Fake<IProvisioningManager>();
            _portalDBAccess = A.Fake<IPortalBackendDBAccess>();
            _mailingService = A.Fake<IMailingService>();
            _custodianService = A.Fake<ICustodianService>();
            _settings = A.Fake<IOptions<RegistrationSettings>>();
            _logic = new RegistrationBusinessLogic(_portalDBAccess, _settings, _provisioningManager, _custodianService, _mailingService);
        }
        [Fact]
        public async Task Test3()
        {
            //Arrange
            Guid id = new Guid("d90995fe-1241-4b8d-9f5c-f3909acc6383");
            Guid companyUserId = new Guid("857b93b1-8fcb-4141-81b0-ae81950d489e");
            Guid companyUserRoleId = new Guid("607818be-4978-41f4-bf63-fa8d2de51154");
            Guid centralUserId = new Guid("6bc51706-9a30-4eb9-9e60-77fdd6d9cd6f");

            //recursive relationship: Company relates to BusinessPartner relates to ParentBusinessPartner relates to Company
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());            

            var company = _fixture.Build<Company>()
                .With(u => u.BusinessPartnerNumber, "CAXLSHAREDIDPZZ")
                .With(u => u.Name, "Shared Idp Test")
                .Create();
            var companyApplication = _fixture.Build<CompanyApplication>()
                .With(u => u.Company, company)
                .Create();
            string clientId = "catenax-portal";
            List<Guid> userRoleIds = new List<Guid>();
            userRoleIds.Add(new Guid("607818be-4978-41f4-bf63-fa8d2de51154"));
            List<string> roles = new List<string> { "IT Admin" };
            var clientRoleNames = new Dictionary<string, IEnumerable<string>>
                        {
                            { clientId, roles.AsEnumerable() }
                        };
            var companyInvitedUser = _fixture.CreateMany<CompanyInvitedUser>(3).ToAsyncEnumerable();
            var companyUserAssignedRole = _fixture.Create<CompanyUserAssignedRole>();
            List<string> bpnList = new List<string> { "CAXLSHAREDIDPZZ" };
            var bpns = bpnList.AsEnumerable();
            A.CallTo(() => _portalDBAccess.GetCompanyAndApplicationForSubmittedApplication(id))
                   .Returns(companyApplication);
            A.CallTo(() => _portalDBAccess.GetUserRoleIdsUntrackedAsync(clientRoleNames))
                   .Returns(userRoleIds.ToAsyncEnumerable());
            A.CallTo(() => _portalDBAccess.GetInvitedUsersByApplicationIdUntrackedAsync(id))
            .Returns(companyInvitedUser);
            A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(centralUserId.ToString(), clientRoleNames))
           .Returns(Task.CompletedTask);
            A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(centralUserId.ToString(), bpns))
           .Returns(Task.CompletedTask);
            A.CallTo(() => _portalDBAccess.CreateCompanyUserAssignedRole(companyUserId, companyUserRoleId))
                   .Returns(companyUserAssignedRole);
            A.CallTo(() => _portalDBAccess.SaveAsync())
                  .Returns(1);
            A.CallTo(() => _custodianService.CreateWallet("CAXLSHAREDIDPZZ", "Shared Idp Test"))
                  .Returns(Task.CompletedTask);

            //Act
            var result = await _logic.ApprovePartnerRequest(id).ConfigureAwait(false);
            //Assert
            A.CallTo(() => _portalDBAccess.GetCompanyAndApplicationForSubmittedApplication(id)).MustHaveHappened(1, Times.OrMore);
            A.CallTo(() => _portalDBAccess.GetInvitedUsersByApplicationIdUntrackedAsync(id)).MustHaveHappened(1, Times.OrMore);
            A.CallTo(() => _portalDBAccess.SaveAsync()).MustHaveHappened(1, Times.OrMore);
            A.CallTo(() => _custodianService.CreateWallet("CAXLSHAREDIDPZZ", "Shared Idp Test")).MustHaveHappened(1, Times.OrMore);
            Assert.IsType<bool>(result);
            Assert.True(result);
        }
    }
}