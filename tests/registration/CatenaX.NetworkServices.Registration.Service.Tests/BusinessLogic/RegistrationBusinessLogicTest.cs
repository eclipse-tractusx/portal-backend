using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using Microsoft.Extensions.Logging;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.Registration.Service.BPN;
using CatenaX.NetworkServices.Registration.Service.RegistrationAccess;
using CatenaX.NetworkServices.Registration.Service.Custodian;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.Registration.Service.BusinessLogic;
using CatenaX.NetworkServices.Provisioning.Library;
using Microsoft.Extensions.Options;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Registration.Service.Model;

namespace CatenaX.NetworkServices.Registration.Service.Tests
{
    public class RegistrationBusinessLogicTest
    {
        private readonly IFixture _fixture;
        private readonly IRegistrationDBAccess _dbAccess;
        private readonly IMailingService _mailingService;
        private readonly IBPNAccess _bpnAccess;
        private readonly ICustodianService _custodianService;
        private readonly IProvisioningManager _provisioningManager;
        private readonly IPortalBackendDBAccess _portalDBAccess;
        private readonly ILogger<RegistrationBusinessLogic> _logger;
        private readonly IOptions<RegistrationSettings> _settings;
        private readonly IRegistrationBusinessLogic _logic;
        public RegistrationBusinessLogicTest()
        {
            _fixture = new Fixture();
            _dbAccess = A.Fake<IRegistrationDBAccess>();
            _mailingService = A.Fake<IMailingService>();
            _bpnAccess = A.Fake<IBPNAccess>();
            _custodianService = A.Fake<ICustodianService>();
            _provisioningManager = A.Fake<IProvisioningManager>();
            _portalDBAccess = A.Fake<IPortalBackendDBAccess>();
            _logger = A.Fake<ILogger<RegistrationBusinessLogic>>();
            _settings = A.Fake<IOptions<RegistrationSettings>>();
            this._logic = new RegistrationBusinessLogic(_settings, _dbAccess, _mailingService, _bpnAccess, _custodianService, _provisioningManager, _portalDBAccess, _logger);
        }

        [Fact]
        public async Task Get_WhenThereAreInvitedUser_ShouldReturnInvitedUserWithRoles()
        {
            //Arrange
            Guid id = new Guid("7eab8e16-8298-4b41-953b-515745423658");
            var invitedUserRole = _fixture.CreateMany<UserRole>(2).AsEnumerable();
            var invitedUser = _fixture.CreateMany<InvitedUsers>(1).ToAsyncEnumerable();
            A.CallTo(() => _portalDBAccess.GetInvitedUsersDetail(id))
                .Returns(invitedUser);
            A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(A<string>._, A<string>._))
                .Returns(invitedUserRole);
            //Act
            var result = await this._logic.GetInvitedUsersDetail(id);

            //Assert
            A.CallTo(() => _portalDBAccess.GetInvitedUsersDetail(id)).MustHaveHappened(1, Times.OrMore);
            A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(A<string>._, A<string>._)).MustHaveHappened(1, Times.OrMore);
            Assert.NotNull(result);
            Assert.IsType<InvitedUserRoleMapper>(result);
        }

        [Fact]
        public async Task GetInvitedUsersDetail_ThrowException_WhenIdIsNull()
        {
            //Arrange
            Guid id = new Guid("7eab8e16-8298-4b41-953b-515745423658");
            var invitedUserRole = _fixture.CreateMany<UserRole>(2).AsEnumerable();
            var invitedUser = _fixture.CreateMany<InvitedUsers>(1).ToAsyncEnumerable();
            A.CallTo(() => _portalDBAccess.GetInvitedUsersDetail(id))
                .Returns(invitedUser);
            A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(A<string>._, A<string>._))
                .Returns(invitedUserRole);
            //Act
            var result = await this._logic.GetInvitedUsersDetail(Guid.Empty);

            //Assert
            A.CallTo(() => _portalDBAccess.GetInvitedUsersDetail(Guid.Empty)).Throws(new Exception());
            A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(string.Empty, string.Empty)).Throws(new Exception());

        }

    }
}