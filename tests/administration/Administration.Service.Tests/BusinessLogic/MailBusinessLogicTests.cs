using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class MailBusinessLogicTests
{
    private readonly Guid UserId = Guid.NewGuid();
    private readonly IFixture _fixture;
    private readonly IMailBusinessLogic _sut;
    private readonly IMailingProcessCreation _mailingService;
    private readonly IUserRepository _userRepository;

    public MailBusinessLogicTests()
    {
        _fixture = new Fixture();
        var identity = A.Fake<IIdentityData>();
        A.CallTo(() => identity.IdentityId).Returns(UserId);

        _userRepository = A.Fake<IUserRepository>();
        var portalRepositories = A.Fake<IPortalRepositories>();
        _mailingService = A.Fake<IMailingProcessCreation>();

        A.CallTo(() => portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);

        _sut = new MailBusinessLogic(portalRepositories, _mailingService);
    }

    #region SendMail

    [Fact]
    public async Task SendMail_WithoutExistingUser_ThrowsNotFoundException()
    {
        // Arrange
        var data = _fixture.Build<MailData>().With(x => x.Requester, UserId).Create();
        A.CallTo(() => _userRepository.GetUserMailData(UserId)).Returns((false, null));
        async Task Act() => await _sut.SendMail(data).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be(AdministrationMailErrors.USER_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task SendMail_WithUserWithoutEmail_DoesntCallService()
    {
        // Arrange
        var data = _fixture.Build<MailData>().With(x => x.Requester, UserId).Create();
        A.CallTo(() => _userRepository.GetUserMailData(UserId)).Returns((true, null));

        // Act
        await _sut.SendMail(data);

        // Assert
        A.CallTo(() => _mailingService.CreateMailProcess(A<string>._, A<string>._, A<Dictionary<string, string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task SendMail_WithValid_CallsExpected()
    {
        // Arrange
        var data = new MailData(UserId, "testTemplate", new Dictionary<string, string>());
        A.CallTo(() => _userRepository.GetUserMailData(UserId)).Returns((true, "test@email.com"));

        // Act
        await _sut.SendMail(data);

        // Assert
        A.CallTo(() => _mailingService.CreateMailProcess("test@email.com", "testTemplate", A<Dictionary<string, string>>._)).MustHaveHappenedOnceExactly();
    }

    #endregion
}
