/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class NetworkBusinessLogicTests
{
    private const string Bpnl = "BPNL00000001TEST";
    private static readonly Guid ExistingExternalId = Guid.NewGuid();
    private static readonly Guid UserRoleId = Guid.NewGuid();
    private static readonly Guid MultiIdpCompanyId = Guid.NewGuid();
    private static readonly Guid NoIdpCompanyId = Guid.NewGuid();
    private static readonly Guid IdpId = Guid.NewGuid();
    private static readonly Guid NoAliasIdpCompanyId = Guid.NewGuid();

    private readonly IFixture _fixture;

    private readonly IdentityData _identity = new(Guid.NewGuid().ToString(), Guid.NewGuid(), IdentityTypeId.COMPANY_USER, Guid.NewGuid());
    private readonly IIdentityService _identityService;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly INetworkRegistrationProcessHelper _networkRegistrationProcessHelper;
    private readonly IMailingService _mailingService;

    private readonly IPortalRepositories _portalRepositories;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICompanyRolesRepository _companyRolesRepository;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly INetworkRepository _networkRepository;
    private readonly IIdentityProviderRepository _identityProviderRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly NetworkBusinessLogic _sut;
    private readonly PartnerRegistrationSettings _settings;
    private readonly IConsentRepository _consentRepository;

    public NetworkBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _userProvisioningService = A.Fake<IUserProvisioningService>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _identityService = A.Fake<IIdentityService>();
        _networkRegistrationProcessHelper = A.Fake<INetworkRegistrationProcessHelper>();
        _mailingService = A.Fake<IMailingService>();

        _companyRepository = A.Fake<ICompanyRepository>();
        _companyRolesRepository = A.Fake<ICompanyRolesRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _networkRepository = A.Fake<INetworkRepository>();
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();
        _countryRepository = A.Fake<ICountryRepository>();
        _consentRepository = A.Fake<IConsentRepository>();

        _settings = new PartnerRegistrationSettings
        {
            InitialRoles = new[] { new UserRoleConfig("cl1", new[] { "Company Admin" }) }
        };
        var options = A.Fake<IOptions<PartnerRegistrationSettings>>();

        A.CallTo(() => options.Value).Returns(_settings);
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentRepository>()).Returns(_consentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRolesRepository>()).Returns(_companyRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INetworkRepository>()).Returns(_networkRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICountryRepository>()).Returns(_countryRepository);

        _sut = new NetworkBusinessLogic(_portalRepositories, _identityService, _userProvisioningService, _networkRegistrationProcessHelper, _mailingService, options);

        SetupRepos();
    }

    #region HandlePartnerRegistration

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
    public async Task HandlePartnerRegistration_WithoutExistingBpn_ThrowsControllerArgumentException()
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.Bpn, "BPNL00000001FAIL")
            .Create();

        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"The Bpn {data.Bpn} already exists (Parameter 'Bpn')");
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
    public async Task HandlePartnerRegistration_WithInvalidEmail_ThrowsControllerArgumentException(string email)
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.Bpn, Bpnl)
            .With(x => x.UserDetails, new[] { new UserDetailData(null, Guid.NewGuid().ToString(), "test", "Test", "test", email) })
            .Create();

        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("User must have a valid email address");
    }

    [Theory]
    [InlineData("")]
    public async Task HandlePartnerRegistration_WithInvalidFirstnameEmail_ThrowsControllerArgumentException(string firstName)
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.Bpn, Bpnl)
            .With(x => x.UserDetails, new[] { new UserDetailData(null, Guid.NewGuid().ToString(), "test", firstName, "test", "test@email.com") })
            .Create();

        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("Firstname does not match expected format");
    }

    [Theory]
    [InlineData("")]
    public async Task HandlePartnerRegistration_WithInvalidLastnameEmail_ThrowsControllerArgumentException(string lastname)
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.Bpn, Bpnl)
            .With(x => x.UserDetails, new[] { new UserDetailData(null, Guid.NewGuid().ToString(), "test", "test", lastname, "test@email.com") })
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
            .With(x => x.UserDetails, new[] { new UserDetailData(null, Guid.NewGuid().ToString(), "test", "test", "test", "test@email.com") })
            .With(x => x.ExternalId, ExistingExternalId)
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
            .With(x => x.UserDetails, new[] { new UserDetailData(null, Guid.NewGuid().ToString(), "test", "test", "test", "test@email.com") })
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
    public async Task HandlePartnerRegistration_WithNoIdpIdSetAndNoManagedIdps_ThrowsConflictException()
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.Bpn, Bpnl)
            .With(x => x.CountryAlpha2Code, "DE")
            .With(x => x.UserDetails, new[] { new UserDetailData(null, "123", "test", "test", "test", "test@email.com") })
            .Create();
        A.CallTo(() => _identityService.IdentityData).Returns(_identity with { CompanyId = NoIdpCompanyId });

        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"company {NoIdpCompanyId} has no managed identityProvider");
    }

    [Fact]
    public async Task HandlePartnerRegistration_WithNoIdpIdSetAndMultipleManagedIdps_ThrowsControllerArgumentException()
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.Bpn, Bpnl)
            .With(x => x.CountryAlpha2Code, "DE")
            .With(x => x.UserDetails, new[] { new UserDetailData(null, "123", "test", "test", "test", "test@email.com") })
            .Create();
        A.CallTo(() => _identityService.IdentityData).Returns(_identity with { CompanyId = MultiIdpCompanyId });

        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"Company {MultiIdpCompanyId} has more than one identity provider linked, therefore identityProviderId must be set for all users (Parameter 'UserDetails')");
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
            .With(x => x.UserDetails, new[] { new UserDetailData(notExistingIdpId, "123", "test", "test", "test", "test@email.com") })
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
            .With(x => x.UserDetails, new[] { new UserDetailData(IdpId, "123", "test", "test", "test", "test@email.com") })
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
    public async Task HandlePartnerRegistration_WithUserCreationThrowsException_ThrowsServiceException()
    {
        // Arrange
        var newCompanyId = Guid.NewGuid();
        var processId = Guid.NewGuid();

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
            new[] { new IdentifierData(UniqueIdentifierId.VAT_ID, "DE123456789") },
            new[] { new UserDetailData(IdpId, "123", "ironman", "tony", "stark", "tony@stark.com") },
            new[] { CompanyRoleId.APP_PROVIDER, CompanyRoleId.SERVICE_PROVIDER }
        );
        A.CallTo(() => _companyRepository.CreateCompany(A<string>._, A<Action<Company>>._))
            .Returns(new Company(newCompanyId, null!, default, default));

        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.PARTNER_REGISTRATION))
            .Returns(new Process(processId, default, default));

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._, A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._, A<CancellationToken>._))
            .Returns(new[] { (Guid.Empty, "", (string?)null, (Exception?)new UnexpectedConditionException("Test")) }.ToAsyncEnumerable());

        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);
        ex.Message.Should().Contain("Errors occured while saving the users: ");
    }

    [Fact]
    public async Task HandlePartnerRegistration_WithSingleIdpWithoutAlias_ThrowsServiceException()
    {
        // Arrange
        var newCompanyId = Guid.NewGuid();
        var processId = Guid.NewGuid();

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
            new[] { new IdentifierData(UniqueIdentifierId.VAT_ID, "DE123456789") },
            new[] { new UserDetailData(null, "123", "ironman", "tony", "stark", "tony@stark.com") },
            new[] { CompanyRoleId.APP_PROVIDER, CompanyRoleId.SERVICE_PROVIDER }
        );
        A.CallTo(() => _identityService.IdentityData)
            .Returns(_identity with { CompanyId = NoAliasIdpCompanyId });

        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        ex.Message.Should().Contain($"identityProvider {IdpId} has no alias");
    }

    [Fact]
    public async Task HandlePartnerRegistration_WithIdpNotSetAndOnlyOneIdp_CallsExpected()
    {
        // Arrange
        var newCompanyId = Guid.NewGuid();
        var newProcessId = Guid.NewGuid();
        var newApplicationId = Guid.NewGuid();

        var addresses = new List<Address>();
        var companies = new List<Company>();
        var companyAssignedRoles = new List<CompanyAssignedRole>();
        var processes = new List<Process>();
        var processSteps = new List<ProcessStep>();
        var companyApplications = new List<CompanyApplication>();
        var networkRegistrations = new List<NetworkRegistration>();

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
            new[] { new IdentifierData(UniqueIdentifierId.VAT_ID, "DE123456789") },
            new[] { new UserDetailData(null, "123", "ironman", "tony", "stark", "tony@stark.com") },
            new[] { CompanyRoleId.APP_PROVIDER, CompanyRoleId.SERVICE_PROVIDER }
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
            .ReturnsLazily((string name, Action<Company>? setOptionalParameters) =>
            {
                var company = new Company(
                    newCompanyId,
                    name,
                    CompanyStatusId.PENDING,
                    DateTimeOffset.UtcNow
                );
                setOptionalParameters?.Invoke(company);
                companies.Add(company);
                return company;
            });
        A.CallTo(() => _companyRolesRepository.CreateCompanyAssignedRoles(newCompanyId, A<IEnumerable<CompanyRoleId>>._))
            .Invokes((Guid companyId, IEnumerable<CompanyRoleId> companyRoleIds) =>
            {
                companyAssignedRoles.AddRange(companyRoleIds.Select(x => new CompanyAssignedRole(companyId, x)));
            });
        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.PARTNER_REGISTRATION))
            .ReturnsLazily((ProcessTypeId processTypeId) =>
            {
                var process = new Process(newProcessId, processTypeId, Guid.NewGuid());
                processes.Add(process);
                return process;
            });
        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._, A<ProcessStepStatusId>._, A<Guid>._))
            .Invokes((ProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid processId) =>
            {
                processSteps.Add(new ProcessStep(Guid.NewGuid(), processStepTypeId, processStepStatusId, processId, DateTimeOffset.UtcNow));
            });
        A.CallTo(() => _applicationRepository.CreateCompanyApplication(A<Guid>._, A<CompanyApplicationStatusId>._, A<CompanyApplicationTypeId>._, A<Action<CompanyApplication>>._))
            .ReturnsLazily((Guid companyId, CompanyApplicationStatusId companyApplicationStatusId, CompanyApplicationTypeId applicationTypeId, Action<CompanyApplication>? setOptionalFields) =>
            {
                var companyApplication = new CompanyApplication(
                    newApplicationId,
                    companyId,
                    companyApplicationStatusId,
                    applicationTypeId,
                    DateTimeOffset.UtcNow);
                setOptionalFields?.Invoke(companyApplication);
                companyApplications.Add(companyApplication);
                return companyApplication;
            });
        A.CallTo(() => _networkRepository.CreateNetworkRegistration(A<Guid>._, A<Guid>._, A<Guid>._, A<Guid>._, A<Guid>._))
            .Invokes((Guid externalId, Guid companyId, Guid pId, Guid ospId, Guid companyApplicationId) =>
            {
                networkRegistrations.Add(new NetworkRegistration(Guid.NewGuid(), externalId, companyId, pId, ospId, companyApplicationId, DateTimeOffset.UtcNow));
            });
        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._, A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._, A<CancellationToken>._))
            .Returns(new[] { (Guid.NewGuid(), "ironman", (string?)"testpw", (Exception?)null) }.ToAsyncEnumerable());

        // Act
        await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);

        // Assert
        addresses.Should().ContainSingle()
            .Which.Should().Match<Address>(x =>
                x.Region == data.Region &&
                x.Zipcode == data.ZipCode);
        companies.Should().ContainSingle()
            .Which.Should().Match<Company>(x =>
                x.Name == data.Name &&
                x.CompanyStatusId == CompanyStatusId.PENDING);
        processes.Should().ContainSingle()
            .Which.Should().Match<Process>(
                x => x.ProcessTypeId == ProcessTypeId.PARTNER_REGISTRATION);
        processSteps.Should().ContainSingle()
            .Which.Should().Match<ProcessStep>(x =>
                x.ProcessStepStatusId == ProcessStepStatusId.TODO &&
                x.ProcessStepTypeId == ProcessStepTypeId.SYNCHRONIZE_USER);
        companyApplications.Should().ContainSingle()
            .Which.Should().Match<CompanyApplication>(x =>
                x.CompanyId == newCompanyId &&
                x.ApplicationStatusId == CompanyApplicationStatusId.CREATED);
        companyAssignedRoles.Should().HaveCount(2).And.Satisfy(
            x => x.CompanyRoleId == CompanyRoleId.APP_PROVIDER,
            x => x.CompanyRoleId == CompanyRoleId.SERVICE_PROVIDER);
        networkRegistrations.Should().ContainSingle()
            .Which.Should().Match<NetworkRegistration>(x =>
                x.ExternalId == data.ExternalId &&
                x.ProcessId == newProcessId &&
                x.ApplicationId == newApplicationId);

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._, A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.CreateCompanyIdentityProviders(A<IEnumerable<(Guid, Guid)>>.That.IsSameSequenceAs(new[] { new ValueTuple<Guid, Guid>(newCompanyId, IdpId) })))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.Matches(x => x.Count() == 1 && x.Single() == "OspWelcomeMail")))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task HandlePartnerRegistration_WithValidData_CallsExpected()
    {
        // Arrange
        var newCompanyId = Guid.NewGuid();
        var newProcessId = Guid.NewGuid();
        var newApplicationId = Guid.NewGuid();

        var addresses = new List<Address>();
        var companies = new List<Company>();
        var companyAssignedRoles = new List<CompanyAssignedRole>();
        var processes = new List<Process>();
        var processSteps = new List<ProcessStep>();
        var companyApplications = new List<CompanyApplication>();
        var networkRegistrations = new List<NetworkRegistration>();

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
            new[] { new IdentifierData(UniqueIdentifierId.VAT_ID, "DE123456789") },
            new[] { new UserDetailData(IdpId, "123", "ironman", "tony", "stark", "tony@stark.com") },
            new[] { CompanyRoleId.APP_PROVIDER, CompanyRoleId.SERVICE_PROVIDER }
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
            .ReturnsLazily((string name, Action<Company>? setOptionalParameters) =>
            {
                var company = new Company(
                    newCompanyId,
                    name,
                    CompanyStatusId.PENDING,
                    DateTimeOffset.UtcNow
                );
                setOptionalParameters?.Invoke(company);
                companies.Add(company);
                return company;
            });
        A.CallTo(() => _companyRolesRepository.CreateCompanyAssignedRoles(A<Guid>._, A<IEnumerable<CompanyRoleId>>._))
            .Invokes((Guid companyId, IEnumerable<CompanyRoleId> companyRoleIds) =>
            {
                companyAssignedRoles.AddRange(companyRoleIds.Select(x => new CompanyAssignedRole(companyId, x)));
            });
        A.CallTo(() => _processStepRepository.CreateProcess(A<ProcessTypeId>._))
            .ReturnsLazily((ProcessTypeId processTypeId) =>
            {
                var process = new Process(newProcessId, processTypeId, Guid.NewGuid());
                processes.Add(process);
                return process;
            });
        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._, A<ProcessStepStatusId>._, A<Guid>._))
            .Invokes((ProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid processId) =>
            {
                processSteps.Add(new ProcessStep(Guid.NewGuid(), processStepTypeId, processStepStatusId, processId, DateTimeOffset.UtcNow));
            });
        A.CallTo(() => _applicationRepository.CreateCompanyApplication(A<Guid>._, A<CompanyApplicationStatusId>._, A<CompanyApplicationTypeId>._, A<Action<CompanyApplication>>._))
            .ReturnsLazily((Guid companyId, CompanyApplicationStatusId companyApplicationStatusId, CompanyApplicationTypeId applicationTypeId, Action<CompanyApplication>? setOptionalFields) =>
            {
                var companyApplication = new CompanyApplication(
                    newApplicationId,
                    companyId,
                    companyApplicationStatusId,
                    applicationTypeId,
                    DateTimeOffset.UtcNow);
                setOptionalFields?.Invoke(companyApplication);
                companyApplications.Add(companyApplication);
                return companyApplication;
            });
        A.CallTo(() => _networkRepository.CreateNetworkRegistration(A<Guid>._, A<Guid>._, A<Guid>._, A<Guid>._, A<Guid>._))
            .Invokes((Guid externalId, Guid companyId, Guid pId, Guid ospId, Guid companyApplicationId) =>
            {
                networkRegistrations.Add(new NetworkRegistration(Guid.NewGuid(), externalId, companyId, pId, ospId, companyApplicationId, DateTimeOffset.UtcNow));
            });
        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._, A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._, A<CancellationToken>._))
            .Returns(new[] { (Guid.NewGuid(), "ironman", (string?)"testpw", (Exception?)null) }.ToAsyncEnumerable());

        // Act
        await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);

        // Assert
        addresses.Should().ContainSingle()
            .Which.Should().Match<Address>(x =>
                x.Region == data.Region &&
                x.Zipcode == data.ZipCode);
        companies.Should().ContainSingle()
            .Which.Should().Match<Company>(x =>
                x.Name == data.Name &&
                x.CompanyStatusId == CompanyStatusId.PENDING);
        processes.Should().ContainSingle()
            .Which.Should().Match<Process>(x =>
                x.ProcessTypeId == ProcessTypeId.PARTNER_REGISTRATION);
        processSteps.Should().ContainSingle()
            .Which.Should().Match<ProcessStep>(x =>
                x.ProcessStepStatusId == ProcessStepStatusId.TODO &&
                x.ProcessStepTypeId == ProcessStepTypeId.SYNCHRONIZE_USER);
        companyApplications.Should().ContainSingle()
            .Which.Should().Match<CompanyApplication>(x =>
                x.CompanyId == newCompanyId &&
                x.ApplicationStatusId == CompanyApplicationStatusId.CREATED);
        companyAssignedRoles.Should().HaveCount(2).And.Satisfy(
            x => x.CompanyRoleId == CompanyRoleId.APP_PROVIDER,
            x => x.CompanyRoleId == CompanyRoleId.SERVICE_PROVIDER);
        networkRegistrations.Should().ContainSingle()
            .Which.Should().Match<NetworkRegistration>(x =>
                x.ExternalId == data.ExternalId &&
                x.ProcessId == newProcessId &&
                x.ApplicationId == newApplicationId);

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._, A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.CreateCompanyIdentityProviders(A<IEnumerable<(Guid, Guid)>>.That.IsSameSequenceAs(new[] { new ValueTuple<Guid, Guid>(newCompanyId, IdpId) })))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.Matches(x => x.Count() == 1 && x.Single() == "OspWelcomeMail")))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region RetriggerSynchronizeUser

    [Fact]
    public async Task RetriggerSynchronizeUser_CallsExpected()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        const ProcessStepTypeId ProcessStepId = ProcessStepTypeId.RETRIGGER_SYNCHRONIZE_USER;

        // Act
        await _sut.RetriggerProcessStep(externalId, ProcessStepId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _networkRegistrationProcessHelper.TriggerProcessStep(externalId, ProcessStepId)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Submit

    [Fact]
    public async Task Submit_WithNotExistingSubmitData_ThrowsNotFoundException()
    {
        // Arrange
        var data = _fixture.Create<PartnerSubmitData>();
        A.CallTo(() => _networkRepository.GetSubmitData(_identity.CompanyId, _identity.UserId, A<IEnumerable<Guid>>._))
            .Returns(new ValueTuple<bool, IEnumerable<ValueTuple<Guid, CompanyApplicationStatusId, string?>>, bool, IEnumerable<ValueTuple<CompanyRoleId, IEnumerable<Guid>>>, Guid?>());

        // Act
        async Task Act() => await _sut.Submit(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Company {_identity.CompanyId} not found");
    }

    [Fact]
    public async Task Submit_WithUserNotInRole_ThrowsForbiddenException()
    {
        // Arrange
        var data = _fixture.Create<PartnerSubmitData>();
        A.CallTo(() => _networkRepository.GetSubmitData(_identity.CompanyId, _identity.UserId, A<IEnumerable<Guid>>._))
            .Returns((true, Enumerable.Empty<ValueTuple<Guid, CompanyApplicationStatusId, string?>>(), false, Enumerable.Empty<ValueTuple<CompanyRoleId, IEnumerable<Guid>>>(), null));

        // Act
        async Task Act() => await _sut.Submit(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"User must be in role {string.Join(",", _settings.InitialRoles.SelectMany(x => x.UserRoleNames))}");
    }

    [Fact]
    public async Task Submit_WithoutCompanyApplications_ThrowsConflictException()
    {
        // Arrange
        var data = _fixture.Create<PartnerSubmitData>();
        A.CallTo(() => _networkRepository.GetSubmitData(_identity.CompanyId, _identity.UserId, A<IEnumerable<Guid>>._))
            .Returns((true, Enumerable.Empty<ValueTuple<Guid, CompanyApplicationStatusId, string?>>(), true, Enumerable.Empty<ValueTuple<CompanyRoleId, IEnumerable<Guid>>>(), null));

        // Act
        async Task Act() => await _sut.Submit(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Company {_identity.CompanyId} has no or more than one application");
    }

    [Fact]
    public async Task Submit_WithMultipleCompanyApplications_ThrowsConflictException()
    {
        // Arrange
        var data = _fixture.Create<PartnerSubmitData>();
        A.CallTo(() => _networkRepository.GetSubmitData(_identity.CompanyId, _identity.UserId, A<IEnumerable<Guid>>._))
            .Returns((true, _fixture.CreateMany<ValueTuple<Guid, CompanyApplicationStatusId, string?>>(2), true, Enumerable.Empty<ValueTuple<CompanyRoleId, IEnumerable<Guid>>>(), null));

        // Act
        async Task Act() => await _sut.Submit(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Company {_identity.CompanyId} has no or more than one application");
    }

    [Fact]
    public async Task Submit_WithWrongApplicationStatus_ThrowsConflictException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var data = _fixture.Create<PartnerSubmitData>();
        A.CallTo(() => _networkRepository.GetSubmitData(_identity.CompanyId, _identity.UserId, A<IEnumerable<Guid>>._))
            .Returns((true, Enumerable.Repeat<ValueTuple<Guid, CompanyApplicationStatusId, string?>>((applicationId, CompanyApplicationStatusId.VERIFY, null), 1), true, Enumerable.Empty<ValueTuple<CompanyRoleId, IEnumerable<Guid>>>(), Guid.NewGuid()));

        // Act
        async Task Act() => await _sut.Submit(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Application {applicationId} is not in state CREATED");
    }

    [Fact]
    public async Task Submit_WithOneMissingAgreement_ThrowsConflictException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var agreementId = Guid.NewGuid();
        var notExistingAgreementId = Guid.NewGuid();
        var data = new PartnerSubmitData(
            new[] { CompanyRoleId.APP_PROVIDER },
            new[] { new AgreementConsentData(agreementId, ConsentStatusId.ACTIVE) });
        var companyRoleIds = new ValueTuple<CompanyRoleId, IEnumerable<Guid>>[]
        {
            (CompanyRoleId.APP_PROVIDER, new [] {agreementId, notExistingAgreementId})
        };
        A.CallTo(() => _networkRepository.GetSubmitData(_identity.CompanyId, _identity.UserId, A<IEnumerable<Guid>>._))
            .Returns((true, Enumerable.Repeat<ValueTuple<Guid, CompanyApplicationStatusId, string?>>((applicationId, CompanyApplicationStatusId.CREATED, null), 1), true, companyRoleIds, Guid.NewGuid()));

        // Act
        async Task Act() => await _sut.Submit(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("All Agreements for the company roles must be agreed to");
    }

    [Fact]
    public async Task Submit_WithOneInactiveAgreement_ThrowsConflictException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var agreementId = Guid.NewGuid();
        var inactiveAgreementId = Guid.NewGuid();
        var data = new PartnerSubmitData(
            new[] { CompanyRoleId.APP_PROVIDER },
            new[]
            {
                new AgreementConsentData(agreementId, ConsentStatusId.ACTIVE),
                new AgreementConsentData(inactiveAgreementId, ConsentStatusId.INACTIVE),
            });
        var companyRoleIds = new ValueTuple<CompanyRoleId, IEnumerable<Guid>>[]
        {
            (CompanyRoleId.APP_PROVIDER, new [] {agreementId, inactiveAgreementId})
        };
        A.CallTo(() => _networkRepository.GetSubmitData(_identity.CompanyId, _identity.UserId, A<IEnumerable<Guid>>._))
            .Returns((true, Enumerable.Repeat<ValueTuple<Guid, CompanyApplicationStatusId, string?>>((applicationId, CompanyApplicationStatusId.CREATED, null), 1), true, companyRoleIds, Guid.NewGuid()));

        // Act
        async Task Act() => await _sut.Submit(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("All agreements must be agreed to");
    }

    [Fact]
    public async Task Submit_WithoutProcessId_ThrowsConflictException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var agreementId = Guid.NewGuid();
        var agreementId1 = Guid.NewGuid();
        var data = new PartnerSubmitData(
            new[] { CompanyRoleId.APP_PROVIDER },
            new[]
            {
                new AgreementConsentData(agreementId, ConsentStatusId.ACTIVE),
                new AgreementConsentData(agreementId1, ConsentStatusId.ACTIVE),
            });
        var companyRoleIds = new ValueTuple<CompanyRoleId, IEnumerable<Guid>>[]
        {
            (CompanyRoleId.APP_PROVIDER, new [] {agreementId, agreementId1})
        };
        A.CallTo(() => _networkRepository.GetSubmitData(_identity.CompanyId, _identity.UserId, A<IEnumerable<Guid>>._))
            .Returns((true, Enumerable.Repeat<ValueTuple<Guid, CompanyApplicationStatusId, string?>>((applicationId, CompanyApplicationStatusId.CREATED, "https://callback.url"), 1), true, companyRoleIds, null));
        // Act
        async Task Act() => await _sut.Submit(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("There must be an process");
    }

    [Fact]
    public async Task Submit_WithValidData_CallsExpected()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var agreementId = Guid.NewGuid();
        var agreementId1 = Guid.NewGuid();
        var processSteps = new List<ProcessStep>();
        var application = new CompanyApplication(applicationId, _identity.CompanyId, CompanyApplicationStatusId.CREATED, CompanyApplicationTypeId.EXTERNAL, DateTimeOffset.UtcNow);

        var data = new PartnerSubmitData(
            new[] { CompanyRoleId.APP_PROVIDER },
            new[]
            {
                new AgreementConsentData(agreementId, ConsentStatusId.ACTIVE),
                new AgreementConsentData(agreementId1, ConsentStatusId.ACTIVE),
            });
        var companyRoleIds = new ValueTuple<CompanyRoleId, IEnumerable<Guid>>[]
        {
            (CompanyRoleId.APP_PROVIDER, new [] {agreementId, agreementId1})
        };
        A.CallTo(() => _networkRepository.GetSubmitData(_identity.CompanyId, _identity.UserId, A<IEnumerable<Guid>>._))
            .Returns((true, Enumerable.Repeat<ValueTuple<Guid, CompanyApplicationStatusId, string?>>((applicationId, CompanyApplicationStatusId.CREATED, "https://callback.url"), 1), true, companyRoleIds, Guid.NewGuid()));
        A.CallTo(() => _applicationRepository.AttachAndModifyCompanyApplication(applicationId, A<Action<CompanyApplication>>._))
            .Invokes((Guid _, Action<CompanyApplication> setOptionalFields) =>
            {
                setOptionalFields.Invoke(application);
            });
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)>>.That.Matches(x =>
                    x.Count() == 1 &&
                    x.Single().ProcessStepTypeId == ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED)))
            .Invokes((IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> steps) =>
                {
                    processSteps.AddRange(steps.Select(x => new ProcessStep(Guid.NewGuid(), x.ProcessStepTypeId, x.ProcessStepStatusId, x.ProcessId, DateTimeOffset.UtcNow)));
                });

        // Act
        await _sut.Submit(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        application.ApplicationStatusId.Should().Be(CompanyApplicationStatusId.SUBMITTED);
        A.CallTo(() => _consentRepository.CreateConsent(agreementId, _identity.CompanyId, _identity.UserId, ConsentStatusId.ACTIVE, A<Action<Consent>?>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _consentRepository.CreateConsent(agreementId1, _identity.CompanyId, _identity.UserId, ConsentStatusId.ACTIVE, A<Action<Consent>?>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappenedOnceExactly();
        processSteps.Should().ContainSingle().And.Satisfy(x =>
            x.ProcessStepTypeId == ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED &&
            x.ProcessStepStatusId == ProcessStepStatusId.TODO);
    }

    #endregion

    #region Setup

    private void SetupRepos()
    {
        A.CallTo(() => _networkRepository.CheckExternalIdExists(ExistingExternalId, A<Guid>.That.Matches(x => x == _identity.CompanyId || x == NoIdpCompanyId)))
            .Returns(true);
        A.CallTo(() => _networkRepository.CheckExternalIdExists(A<Guid>.That.Not.Matches(x => x == ExistingExternalId), A<Guid>.That.Matches(x => x == _identity.CompanyId || x == NoIdpCompanyId)))
            .Returns(false);

        A.CallTo(() => _companyRepository.CheckBpnExists(Bpnl)).Returns(false);
        A.CallTo(() => _companyRepository.CheckBpnExists(A<string>.That.Not.Matches(x => x == Bpnl))).Returns(true);

        A.CallTo(() => _countryRepository.CheckCountryExistsByAlpha2CodeAsync("XX"))
            .Returns(false);
        A.CallTo(() => _countryRepository.CheckCountryExistsByAlpha2CodeAsync(A<string>.That.Not.Matches(x => x == "XX")))
            .Returns(true);

        A.CallTo(() => _companyRepository.GetCompanyNameUntrackedAsync(A<Guid>.That.Matches(x => x == _identity.CompanyId || x == NoIdpCompanyId)))
            .Returns((true, "testCompany"));
        A.CallTo(() => _companyRepository.GetCompanyNameUntrackedAsync(A<Guid>.That.Not.Matches(x => x == _identity.CompanyId)))
            .Returns((false, ""));

        A.CallTo(() => _identityProviderRepository.GetSingleManagedIdentityProviderAliasDataUntracked(_identity.CompanyId))
            .Returns((IdpId, (string?)"test-alias"));

        A.CallTo(() => _identityProviderRepository.GetSingleManagedIdentityProviderAliasDataUntracked(NoAliasIdpCompanyId))
            .Returns((IdpId, (string?)null));

        A.CallTo(() => _identityProviderRepository.GetSingleManagedIdentityProviderAliasDataUntracked(NoIdpCompanyId))
            .Returns(((Guid, string?))default);

        A.CallTo(() => _identityProviderRepository.GetSingleManagedIdentityProviderAliasDataUntracked(MultiIdpCompanyId))
            .Throws(new InvalidOperationException("Sequence contains more than one element."));

        A.CallTo(() => _identityProviderRepository.GetManagedIdentityProviderAliasDataUntracked(A<Guid>.That.Matches(x => x == _identity.CompanyId || x == NoIdpCompanyId), A<IEnumerable<Guid>>._))
            .Returns(new[] { (IdpId, (string?)"test-alias") }.ToAsyncEnumerable());

        A.CallTo(() => _userProvisioningService.GetRoleDatas(A<IEnumerable<UserRoleConfig>>._))
            .Returns(new[] { new UserRoleData(UserRoleId, "cl1", "Company Admin") }.ToAsyncEnumerable());
    }

    #endregion
}
