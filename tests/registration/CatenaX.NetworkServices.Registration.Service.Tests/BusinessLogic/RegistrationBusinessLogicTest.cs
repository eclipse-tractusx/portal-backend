using AutoFixture;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Registration.Service.BPN;
using CatenaX.NetworkServices.Registration.Service.BusinessLogic;
using CatenaX.NetworkServices.Registration.Service.Model;
using CatenaX.NetworkServices.Registration.Service.RegistrationAccess;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CatenaX.NetworkServices.Registration.Service.Tests;

public class RegistrationBusinessLogicTest
{
    private readonly IFixture _fixture;
    private readonly IRegistrationDBAccess _dbAccess;
    private readonly IMailingService _mailingService;
    private readonly IBPNAccess _bpnAccess;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IPortalBackendDBAccess _portalDBAccess;
    private readonly ILogger<RegistrationBusinessLogic> _logger;
    private readonly IOptions<RegistrationSettings> _settings;
    private readonly IRegistrationBusinessLogic _logic;
    private readonly IPortalRepositories _portalRepositories;
    public RegistrationBusinessLogicTest()
    {
        _fixture = new Fixture();
        _dbAccess = A.Fake<IRegistrationDBAccess>();
        _mailingService = A.Fake<IMailingService>();
        _bpnAccess = A.Fake<IBPNAccess>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _portalDBAccess = A.Fake<IPortalBackendDBAccess>();
        _logger = A.Fake<ILogger<RegistrationBusinessLogic>>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _settings = A.Fake<IOptions<RegistrationSettings>>();
        this._logic = new RegistrationBusinessLogic(_settings, _dbAccess, _mailingService, _bpnAccess, _provisioningManager, _portalDBAccess, _logger, _portalRepositories);
    }

    [Fact]
    public async Task Get_WhenThereAreInvitedUser_ShouldReturnInvitedUserWithRoles()
    {
        //Arrange
        Guid id = new Guid("7eab8e16-8298-4b41-953b-515745423658");
        var invitedUserRole = _fixture.CreateMany<string>(2).AsEnumerable();
        var invitedUser = _fixture.CreateMany<InvitedUserDetail>(1).ToAsyncEnumerable<InvitedUserDetail>();
        A.CallTo(() => _portalDBAccess.GetInvitedUserDetailsUntrackedAsync(id))
            .Returns(invitedUser);
        A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(A<string>._, A<string>._))
            .Returns(invitedUserRole);
        //Act
        var result = this._logic.GetInvitedUsersAsync(id);
        await foreach (var item in result)
        {
            //Assert
            A.CallTo(() => _portalDBAccess.GetInvitedUserDetailsUntrackedAsync(id)).MustHaveHappened(1, Times.OrMore);
            A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(A<string>._, A<string>._)).MustHaveHappened(1, Times.OrMore);
            Assert.NotNull(item);
            Assert.IsType<InvitedUser>(item);
        }
    }

    [Fact]
    public async Task GetInvitedUsersDetail_ThrowException_WhenIdIsNull()
    {
        //Arrange
        Guid id = new Guid("7eab8e16-8298-4b41-953b-515745423658");
        var invitedUserRole = _fixture.CreateMany<string>(2).AsEnumerable();
        var invitedUser = _fixture.CreateMany<InvitedUserDetail>(1).ToAsyncEnumerable<InvitedUserDetail>();
        A.CallTo(() => _portalDBAccess.GetInvitedUserDetailsUntrackedAsync(id))
            .Returns(invitedUser);
        A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(A<string>._, A<string>._))
            .Returns(invitedUserRole);
        //Act
        var result = this._logic.GetInvitedUsersAsync(Guid.Empty);
        await foreach (var item in result)
        {
            //Assert
            A.CallTo(() => _portalDBAccess.GetInvitedUserDetailsUntrackedAsync(Guid.Empty)).Throws(new Exception());
            A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(string.Empty, string.Empty)).Throws(new Exception());
        }
    }
}
