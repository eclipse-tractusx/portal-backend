using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class BringYourOwnWalletBusinessLogicTests
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly ICompanyRepository _companyRepository;
    private readonly IOptions<BringYourOwnWalletSettings> _options;
    private readonly BringYourOwnWalletBusinessLogic _sut;
    private readonly Guid _companyId = Guid.NewGuid();
    private readonly Guid _applicationId = Guid.NewGuid();
    private readonly Guid _excludedRoleId = Guid.NewGuid();

    public BringYourOwnWalletBusinessLogicTests()
    {
        _portalRepositories = A.Fake<IPortalRepositories>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _options = Options.Create(new BringYourOwnWalletSettings
        {
            NonApplicableUserRoles = new List<Guid> { _excludedRoleId }
        });
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        _sut = new BringYourOwnWalletBusinessLogic(_portalRepositories, _options);
    }

    [Fact]
    public async Task IsUserRoleAuthorizedForBYOW_ReturnsTrue_WhenNotBYOW()
    {
        // Arrange
        A.CallTo(() => _companyRepository.GetApplicationIdByCompanyId(_companyId)).Returns(_applicationId);
        A.CallTo(() => _companyRepository.IsBringYourOwnWallet(_applicationId)).Returns(false);

        // Act
        var result = await _sut.IsUserRoleAuthorizedForBYOW(_companyId, new[] { _excludedRoleId });

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsUserRoleAuthorizedForBYOW_ReturnsFalse_WhenBYOWAndRoleIsExcluded()
    {
        // Arrange
        A.CallTo(() => _companyRepository.GetApplicationIdByCompanyId(_companyId)).Returns(_applicationId);
        A.CallTo(() => _companyRepository.IsBringYourOwnWallet(_applicationId)).Returns(true);

        // Act
        var result = await _sut.IsUserRoleAuthorizedForBYOW(_companyId, new[] { _excludedRoleId });

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsUserRoleAuthorizedForBYOW_ReturnsTrue_WhenBYOWAndRoleIsNotExcluded()
    {
        // Arrange
        var otherRoleId = Guid.NewGuid();
        A.CallTo(() => _companyRepository.GetApplicationIdByCompanyId(_companyId)).Returns(_applicationId);
        A.CallTo(() => _companyRepository.IsBringYourOwnWallet(_applicationId)).Returns(true);

        // Act
        var result = await _sut.IsUserRoleAuthorizedForBYOW(_companyId, new[] { otherRoleId });

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetExcludedUserRoles_ReturnsConfiguredRoles()
    {
        // Act
        var result = _sut.GetExcludedUserRoles();

        // Assert
        result.Should().ContainSingle().Which.Should().Be(_excludedRoleId);
    }

    [Fact]
    public async Task IsBringYourOwnWallet_ReturnsExpectedValue()
    {
        // Arrange
        A.CallTo(() => _companyRepository.GetApplicationIdByCompanyId(_companyId)).Returns(_applicationId);
        A.CallTo(() => _companyRepository.IsBringYourOwnWallet(_applicationId)).Returns(true);

        // Act
        var result = await _sut.IsBringYourOwnWallet(_companyId);

        // Assert
        result.Should().BeTrue();
    }
}
