using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class NetworkBusinessLogicTests
{
    private const string Bpnl = "BPNL00000001TEST";
    private static Guid _existingExternalId = Guid.NewGuid();
    private static Guid _userRoleId = Guid.NewGuid();
    private static Guid _multiIdpCompanyId = Guid.NewGuid();
    private static Guid _idpId = Guid.NewGuid();

    private readonly IFixture _fixture;

    private readonly IdentityData _identity = new (Guid.NewGuid().ToString(), Guid.NewGuid(), IdentityTypeId.COMPANY_USER, Guid.NewGuid());
    private readonly IIdentityService _identityService;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly PartnerRegistrationSettings _settings;
    private readonly IOptions<PartnerRegistrationSettings> _options;

    private readonly IPortalRepositories _portalRepositories;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICompanyRolesRepository _companyRolesRepository;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly INetworkRepository _networkRepository;
    private readonly IIdentityProviderRepository _identityProviderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IUserBusinessPartnerRepository _userBusinessPartnerRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly NetworkBusinessLogic _sut;

    public NetworkBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _userProvisioningService = A.Fake<IUserProvisioningService>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _identityService = A.Fake<IIdentityService>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _companyRolesRepository = A.Fake<ICompanyRolesRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _networkRepository = A.Fake<INetworkRepository>();
        
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _userBusinessPartnerRepository = A.Fake<IUserBusinessPartnerRepository>();
        _countryRepository = A.Fake<ICountryRepository>();

        _settings = new PartnerRegistrationSettings
        {
            InitialRoles = new[] {new UserRoleConfig("cl1", Enumerable.Repeat<string>("Company Admin", 1))}
        };
        _options = A.Fake<IOptions<PartnerRegistrationSettings>>();
        
        A.CallTo(() => _options.Value).Returns(_settings);
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRolesRepository>()).Returns(_companyRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INetworkRepository>()).Returns(_networkRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserBusinessPartnerRepository>()).Returns(_userBusinessPartnerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICountryRepository>()).Returns(_countryRepository);

        _sut = new NetworkBusinessLogic(_portalRepositories, _identityService, _userProvisioningService, _options);
        SetupRepos();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("TEST00000012")]
    [InlineData("BPNL1234567899")]
    public async Task HandlePartnerRegistration_WithInvalidBpn_ThrowsControllerArgumentException(string? bpn)
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.Bpn, bpn)
            .Create();
        
        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("BPN must contain exactly 16 characters and must be prefixed with BPNL (Parameter 'Bpn')");
        ex.ParamName.Should().Be("Bpn");
    }
    
    [Fact]
    public async Task HandlePartnerRegistration_WithInvalidCompanyUserRole_ThrowsControllerArgumentException()
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.Bpn, Bpnl)
            .With(x => x.CompanyRoles, Enumerable.Empty<CompanyRoleId>())
            .Create();
        
        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("At least one company role must be selected (Parameter 'CompanyRoles')");
        ex.ParamName.Should().Be("CompanyRoles");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("test.abc")]
    [InlineData("tessadft@asds.de§")]
    public async Task HandlePartnerRegistration_WithInvalidEmail_ThrowsControllerArgumentException(string email)
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.Bpn, Bpnl)
            .With(x => x.UserDetails, Enumerable.Repeat(new UserDetailData(Enumerable.Empty<UserIdentityProviderLink>(), "Test", "test", email), 1))
            .Create();
        
        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("User must have a valid email address");
    }

    [Theory]
    [InlineData("")]
    [InlineData("name&with$special")]
    public async Task HandlePartnerRegistration_WithInvalidFirstnameEmail_ThrowsControllerArgumentException(string firstName)
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.Bpn, Bpnl)
            .With(x => x.UserDetails, Enumerable.Repeat(new UserDetailData(Enumerable.Empty<UserIdentityProviderLink>(), firstName, "test", "test@email.com"), 1))
            .Create();
        
        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("Firstname does not match expected format");
    }

    [Theory]
    [InlineData("")]
    [InlineData("name&with$special")]
    public async Task HandlePartnerRegistration_WithInvalidLastnameEmail_ThrowsControllerArgumentException(string lastname)
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.Bpn, Bpnl)
            .With(x => x.UserDetails, Enumerable.Repeat(new UserDetailData(Enumerable.Empty<UserIdentityProviderLink>(), "test", lastname, "test@email.com"), 1))
            .Create();
        
        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("Lastname does not match expected format");
    }

    [Fact]
    public async Task HandlePartnerRegistration_WithExistingExternalId_ThrowsControllerArgumentException()
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.Bpn, Bpnl)
            .With(x => x.UserDetails, Enumerable.Repeat(new UserDetailData(Enumerable.Empty<UserIdentityProviderLink>(), "test", "test", "test@email.com"), 1))
            .With(x => x.ExternalId, _existingExternalId)
            .Create();
        
        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"ExternalId {data.ExternalId} already exists (Parameter 'ExternalId')");
        ex.ParamName.Should().Be("ExternalId");
    }

    [Fact]
    public async Task HandlePartnerRegistration_WithInvalidCountryCode_ThrowsControllerArgumentException()
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.Bpn, Bpnl)
            .With(x => x.UserDetails, Enumerable.Repeat(new UserDetailData(Enumerable.Empty<UserIdentityProviderLink>(), "test", "test", "test@email.com"), 1))
            .With(x => x.CountryAlpha2Code, "XX")
            .Create();
        
        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"Location {data.CountryAlpha2Code} does not exist (Parameter 'CountryAlpha2Code')");
        ex.ParamName.Should().Be("CountryAlpha2Code");
    }

    [Fact]
    public async Task HandlePartnerRegistration_WithNoIdpIdSetAndMultipleIdps_ThrowsControllerArgumentException()
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.Bpn, Bpnl)
            .With(x => x.CountryAlpha2Code, "DE")
            .With(x => x.UserDetails, Enumerable.Repeat(new UserDetailData(Enumerable.Repeat(new UserIdentityProviderLink(null, "123", "test"), 1), "test", "test", "test@email.com"), 1))
            .Create();
        A.CallTo(() => _identityService.IdentityData).Returns(_identity with {CompanyId = _multiIdpCompanyId});

        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("Company has more than one identity provider linked, therefor identityProviderId must be set for all users (Parameter 'UserDetails')");
        ex.ParamName.Should().Be("UserDetails");
    }

    [Fact]
    public async Task HandlePartnerRegistration_WithNotExistingIdpIdSet_ThrowsControllerArgumentException()
    {
        // Arrange
        var notExistingIdpId = Guid.NewGuid();
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.Bpn, Bpnl)
            .With(x => x.CountryAlpha2Code, "DE")
            .With(x => x.UserDetails, Enumerable.Repeat(new UserDetailData(Enumerable.Repeat(new UserIdentityProviderLink(notExistingIdpId, "123", "test"), 1), "test", "test", "test@email.com"), 1))
            .Create();

        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Contain("Idps").And.Contain("do not exist");
    }

    [Fact]
    public async Task HandlePartnerRegistration_WithInvalidInitialRole_ThrowsConfigurationException()
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.Bpn, Bpnl)
            .With(x => x.CountryAlpha2Code, "DE")
            .With(x => x.UserDetails, Enumerable.Repeat(new UserDetailData(Enumerable.Repeat(new UserIdentityProviderLink(_idpId, "123", "test"), 1), "test", "test", "test@email.com"), 1))
            .Create();
        A.CallTo(() => _userProvisioningService.GetRoleDatas(A<IEnumerable<UserRoleConfig>>._))
            .Throws(new ControllerArgumentException($"invalid roles: clientId: 'cl1', roles: [Company Admin]"));

        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ConfigurationException>(Act);
        ex.Message.Should().Be("InitialRoles: invalid roles: clientId: 'cl1', roles: [Company Admin]");
    }

    [Fact]
    public async Task HandlePartnerRegistration_WithValidData_CallsExpected()
    {
        // Arrange
        var newCompanyId = Guid.NewGuid();
        var processId = Guid.NewGuid();
        var identityId = Guid.NewGuid();

        var addresses = new List<Address>();
        var companies = new List<Company>();
        var companyAssignedRoles = new List<CompanyAssignedRole>();
        var processes = new List<Process>();
        var processSteps = new List<ProcessStep>();
        var companyApplications = new List<CompanyApplication>();
        var networkRegistrations = new List<NetworkRegistration>();
        var identities = new List<Identity>();
        var companyUsers = new List<CompanyUser>();

        var data = new PartnerRegistrationData(
            Guid.NewGuid(),
            "Test N2N",
            Bpnl,
            "Munich",
            "Street",
            "DE",
            "BY",
            "5",
            "00001",
            Enumerable.Repeat(new IdentifierData(UniqueIdentifierId.VAT_ID, "DE123456789"), 1),
            Enumerable.Repeat(new UserDetailData(Enumerable.Repeat(new UserIdentityProviderLink(_idpId, "123", "ironman"), 1), "tony", "stark", "tony@stark.com"), 1),
            new [] { CompanyRoleId.APP_PROVIDER, CompanyRoleId.SERVICE_PROVIDER }
        );
        A.CallTo(() => _companyRepository.CreateAddress(A<string>._, A<string>._, A<string>._, A<Action<Address>>._))
            .Invokes((string city, string streetname, string countryAlpha2Code, Action<Address>? setOptionalParameters) =>
                {
                    var address = new Address(
                        Guid.NewGuid(),
                        city,
                        streetname,
                        countryAlpha2Code,
                        DateTimeOffset.UtcNow
                    );
                    setOptionalParameters?.Invoke(address);
                    addresses.Add(address);
                });
        A.CallTo(() => _companyRepository.CreateCompany(A<string>._, A<Action<Company>>._))
            .Invokes((string name, Action<Company>? setOptionalParameters) =>
            {
                var company = new Company(
                    Guid.NewGuid(),
                    name,
                    CompanyStatusId.PENDING,
                    DateTimeOffset.UtcNow
                );
                setOptionalParameters?.Invoke(company);
                companies.Add(company);
            })
            .Returns(new Company(newCompanyId, null!, default, default));
        A.CallTo(() => _companyRolesRepository.CreateCompanyAssignedRoles(newCompanyId, A<IEnumerable<CompanyRoleId>>._))
            .Invokes((Guid companyId, IEnumerable<CompanyRoleId> companyRoleIds) =>
            {
                companyAssignedRoles.AddRange(companyRoleIds.Select(x => new CompanyAssignedRole(companyId, x)));
            });
        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.PARTNER_REGISTRATION))
            .Invokes((ProcessTypeId processTypeId) =>
            {
                processes.Add(new Process(Guid.NewGuid(), processTypeId, Guid.NewGuid()));
            })
            .Returns(new Process(processId, default, default));

        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)>>._))
            .Invokes((IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> processStepData) =>
            {
                processSteps.AddRange(processStepData.Select(x => new ProcessStep(Guid.NewGuid(), x.ProcessStepTypeId, x.ProcessStepStatusId, x.ProcessId, DateTimeOffset.UtcNow)));
            });
        A.CallTo(() => _applicationRepository.CreateCompanyApplication(newCompanyId, CompanyApplicationStatusId.CREATED, CompanyApplicationTypeId.EXTERNAL, A<Action<CompanyApplication>>._))
            .Invokes((Guid companyId, CompanyApplicationStatusId companyApplicationStatusId, CompanyApplicationTypeId applicationTypeId, Action<CompanyApplication>? setOptionalFields) =>
            {
                var companyApplication = new CompanyApplication(
                    Guid.NewGuid(),
                    companyId,
                    companyApplicationStatusId,
                    applicationTypeId,
                    DateTimeOffset.UtcNow);
                setOptionalFields?.Invoke(companyApplication);
                companyApplications.Add(companyApplication);
            });
        A.CallTo(() => _networkRepository.CreateNetworkRegistration(data.ExternalId, newCompanyId, processId))
            .Invokes((Guid externalId, Guid companyId, Guid processId) =>
            {
                networkRegistrations.Add(new NetworkRegistration(Guid.NewGuid(), externalId, companyId, processId, DateTimeOffset.UtcNow));
            });
        A.CallTo(() => _userRepository.CreateIdentity(newCompanyId, UserStatusId.PENDING, IdentityTypeId.COMPANY_USER))
            .Invokes((Guid companyId, UserStatusId userStatusId, IdentityTypeId identityTypeId) =>
            {
                identities.Add(new Identity(Guid.NewGuid(), DateTimeOffset.UtcNow, companyId, userStatusId, identityTypeId));
            })
            .Returns(new Identity(identityId, DateTimeOffset.UtcNow, default, default, default));
        A.CallTo(() => _userRepository.CreateCompanyUser(identityId, A<string>._, A<string>._, A<string>._))
            .Invokes((Guid identityId, string? firstName, string? lastName, string email) =>
            {
                companyUsers.Add(new CompanyUser(identityId)
                {
                    Firstname = firstName,
                    Lastname = lastName,
                    Email = email
                });
            })
            .Returns(new CompanyUser(identityId));
        
        // Act
        await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);
        
        // Assert
        addresses.Should().ContainSingle().And.Satisfy(x => x.Region == data.Region && x.Zipcode == data.ZipCode);
        companies.Should().ContainSingle().And.Satisfy(x => x.Name == data.Name && x.CompanyStatusId == CompanyStatusId.PENDING);
        processes.Should().ContainSingle().And.Satisfy(x => x.ProcessTypeId == ProcessTypeId.PARTNER_REGISTRATION);
        processSteps.Should().ContainSingle().And.Satisfy(x => x.ProcessStepStatusId == ProcessStepStatusId.TODO && x.ProcessStepTypeId == ProcessStepTypeId.SYNCHRONIZE_USER);
        companyApplications.Should().ContainSingle().And.Satisfy(x => x.CompanyId == newCompanyId && x.ApplicationStatusId == CompanyApplicationStatusId.CREATED);
        identities.Should().ContainSingle().And.Satisfy(x => x.IdentityTypeId == IdentityTypeId.COMPANY_USER && x.UserStatusId == UserStatusId.PENDING);
        companyUsers.Should().ContainSingle().And.Satisfy(x => x.Email == "tony@stark.com");
        companyAssignedRoles.Should().HaveCount(2).And.Satisfy(
            x => x.CompanyRoleId == CompanyRoleId.APP_PROVIDER,
            x => x.CompanyRoleId == CompanyRoleId.SERVICE_PROVIDER);
        networkRegistrations.Should().ContainSingle().And.Satisfy(x => x.ExternalId == data.ExternalId && x.ProcessId == processId);

        A.CallTo(() => _userBusinessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(identityId, data.Bpn)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRolesRepository.CreateIdentityAssignedRole(identityId, _userRoleId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.AddCompanyUserAssignedIdentityProvider(identityId, _idpId, "123", "ironman")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }
    
    #region Setup
    
    private void SetupRepos()
    {        
        A.CallTo(() => _networkRepository.CheckExternalIdExists(_existingExternalId))
            .Returns(true);
        A.CallTo(() => _networkRepository.CheckExternalIdExists(A<Guid>.That.Not.Matches(x => x == _existingExternalId)))
            .Returns(false);

        A.CallTo(() => _countryRepository.CheckCountryExistsByAlpha2CodeAsync("XX"))
            .Returns(false);
        A.CallTo(() => _countryRepository.CheckCountryExistsByAlpha2CodeAsync(A<string>.That.Not.Matches(x => x == "XX")))
            .Returns(true);

        A.CallTo(() => _companyRepository.GetLinkedIdpIds(_multiIdpCompanyId))
            .Returns(_fixture.CreateMany<Guid>(2).ToAsyncEnumerable());
        A.CallTo(() => _companyRepository.GetLinkedIdpIds(_identity.CompanyId))
            .Returns(Enumerable.Repeat(_idpId, 1).ToAsyncEnumerable());
        
        A.CallTo(() => _identityProviderRepository.GetCompanyIdentityProviderCategoryDataUntracked(_identity.CompanyId))
            .Returns(Enumerable.Repeat(new ValueTuple<Guid, IdentityProviderCategoryId, string, IdentityProviderTypeId>(_idpId, IdentityProviderCategoryId.KEYCLOAK_OIDC, "test-alias", IdentityProviderTypeId.MANAGED), 1).ToAsyncEnumerable());

        A.CallTo(() => _userProvisioningService.GetRoleDatas(A<IEnumerable<UserRoleConfig>>._))
            .Returns(Enumerable.Repeat(new UserRoleData(_userRoleId, "cl1", "Company Admin"), 1).ToAsyncEnumerable());
    }

    #endregion
}
