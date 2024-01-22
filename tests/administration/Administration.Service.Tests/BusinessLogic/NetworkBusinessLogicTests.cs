/********************************************************************************
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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Common;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class NetworkBusinessLogicTests
{
    private const string Bpn = "BPNL00000001TEST";
    private static readonly string ExistingExternalId = Guid.NewGuid().ToString();
    private static readonly Guid UserRoleId = Guid.NewGuid();
    private static readonly Guid MultiIdpCompanyId = Guid.NewGuid();
    private static readonly Guid NoIdpCompanyId = Guid.NewGuid();
    private static readonly Guid IdpId = Guid.NewGuid();
    private static readonly Guid NoAliasIdpCompanyId = Guid.NewGuid();

    private readonly IFixture _fixture;

    private readonly IIdentityData _identity;
    private readonly IIdentityService _identityService;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly INetworkRegistrationProcessHelper _networkRegistrationProcessHelper;

    private readonly IPortalRepositories _portalRepositories;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICompanyRolesRepository _companyRolesRepository;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly INetworkRepository _networkRepository;
    private readonly IIdentityProviderRepository _identityProviderRepository;
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
        _networkRegistrationProcessHelper = A.Fake<INetworkRegistrationProcessHelper>();

        _companyRepository = A.Fake<ICompanyRepository>();
        _companyRolesRepository = A.Fake<ICompanyRolesRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _networkRepository = A.Fake<INetworkRepository>();
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();
        _countryRepository = A.Fake<ICountryRepository>();
        _identity = A.Fake<IIdentityData>();

        var settings = new PartnerRegistrationSettings
        {
            InitialRoles = new[] { new UserRoleConfig("cl1", new[] { "Company Admin" }) }
        };
        var options = A.Fake<IOptions<PartnerRegistrationSettings>>();

        A.CallTo(() => options.Value).Returns(settings);
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRolesRepository>()).Returns(_companyRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INetworkRepository>()).Returns(_networkRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICountryRepository>()).Returns(_countryRepository);

        _sut = new NetworkBusinessLogic(_portalRepositories, _identityService, _userProvisioningService, _networkRegistrationProcessHelper, options);

        SetupRepos();
    }

    #region HandlePartnerRegistration

    [Theory]
    [InlineData("")]
    [InlineData("TEST00000012")]
    [InlineData("BPNL1234567899")]
    public async Task HandlePartnerRegistration_WithInvalidBusinessPartnerNumber_ThrowsControllerArgumentException(string? bpn)
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.BusinessPartnerNumber, bpn)
            .Create();

        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("BPN must contain exactly 16 characters and must be prefixed with BPNL (Parameter 'BusinessPartnerNumber')");
        ex.ParamName.Should().Be("BusinessPartnerNumber");
    }

    [Fact]
    public async Task HandlePartnerRegistration_WithoutExistingBusinessPartnerNumber_ThrowsControllerArgumentException()
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.CountryAlpha2Code, "DE")
            .With(x => x.BusinessPartnerNumber, "BPNL00000001FAIL")
            .Create();

        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"The Bpn {data.BusinessPartnerNumber} already exists (Parameter 'BusinessPartnerNumber')");
        ex.ParamName.Should().Be("BusinessPartnerNumber");
    }

    [Fact]
    public async Task HandlePartnerRegistration_WithInvalidCompanyUserRole_ThrowsControllerArgumentException()
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.BusinessPartnerNumber, Bpn)
            .With(x => x.CountryAlpha2Code, "DE")
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
    [InlineData("abc.example.com")]
    [InlineData("a@b@c@example.com")]
    public async Task HandlePartnerRegistration_WithInvalidEmail_ThrowsControllerArgumentException(string email)
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.BusinessPartnerNumber, Bpn)
            .With(x => x.CountryAlpha2Code, "DE")
            .With(x => x.UserDetails, new[] { new UserDetailData(null, Guid.NewGuid().ToString(), "test", "Test", "test", email) })
            .Create();

        // Act
        async Task Act() => await _sut.HandlePartnerRegistration(data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"Mail {email} must not be empty and have valid format");
    }

    [Theory]
    [InlineData("")]
    public async Task HandlePartnerRegistration_WithInvalidFirstnameEmail_ThrowsControllerArgumentException(string firstName)
    {
        // Arrange
        var data = _fixture.Build<PartnerRegistrationData>()
            .With(x => x.BusinessPartnerNumber, Bpn)
            .With(x => x.CountryAlpha2Code, "DE")
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
            .With(x => x.BusinessPartnerNumber, Bpn)
            .With(x => x.CountryAlpha2Code, "DE")
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
            .With(x => x.BusinessPartnerNumber, Bpn)
            .With(x => x.CountryAlpha2Code, "DE")
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
            .With(x => x.BusinessPartnerNumber, Bpn)
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
            .With(x => x.ExternalId, Guid.NewGuid().ToString())
            .With(x => x.BusinessPartnerNumber, Bpn)
            .With(x => x.CountryAlpha2Code, "DE")
            .With(x => x.UserDetails, new[] { new UserDetailData(null, "123", "test", "test", "test", "test@email.com") })
            .Create();
        A.CallTo(() => _identity.CompanyId).Returns(NoIdpCompanyId);

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
            .With(x => x.ExternalId, Guid.NewGuid().ToString())
            .With(x => x.BusinessPartnerNumber, Bpn)
            .With(x => x.CountryAlpha2Code, "DE")
            .With(x => x.UserDetails, new[] { new UserDetailData(null, "123", "test", "test", "test", "test@email.com") })
            .Create();
        A.CallTo(() => _identity.CompanyId).Returns(MultiIdpCompanyId);

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
            .With(x => x.ExternalId, Guid.NewGuid().ToString())
            .With(x => x.BusinessPartnerNumber, Bpn)
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
            .With(x => x.ExternalId, Guid.NewGuid().ToString())
            .With(x => x.BusinessPartnerNumber, Bpn)
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
            Guid.NewGuid().ToString(),
            "Test N2N",
            Bpn,
            "Munich",
            "Street",
            "DE",
            "BY",
            "5",
            "00001",
            new[] { new CompanyUniqueIdData(UniqueIdentifierId.VAT_ID, "DE123456789") },
            new[] { new UserDetailData(IdpId, "123", "ironman", "tony", "stark", "tony@stark.com") },
            new[] { CompanyRoleId.APP_PROVIDER, CompanyRoleId.SERVICE_PROVIDER }
        );
        A.CallTo(() => _companyRepository.CreateCompany(A<string>._, A<Action<Company>>._))
            .Returns(new Company(newCompanyId, null!, default, default));

        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.PARTNER_REGISTRATION))
            .Returns(new Process(processId, default, default));

        A.CallTo(() => _userProvisioningService.GetOrCreateCompanyUser(A<IUserRepository>._, A<string>._, A<UserCreationRoleDataIdpInfo>._, A<Guid>._, A<Guid>._, "BPNL00000001TEST"))
            .Throws(new UnexpectedConditionException("Test message"));

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
        var data = new PartnerRegistrationData(
            Guid.NewGuid().ToString(),
            "Test N2N",
            Bpn,
            "Munich",
            "Street",
            "DE",
            "BY",
            "5",
            "00001",
            new[] { new CompanyUniqueIdData(UniqueIdentifierId.VAT_ID, "DE123456789") },
            new[] { new UserDetailData(null, "123", "ironman", "tony", "stark", "tony@stark.com") },
            new[] { CompanyRoleId.APP_PROVIDER, CompanyRoleId.SERVICE_PROVIDER }
        );
        A.CallTo(() => _identity.CompanyId).Returns(NoAliasIdpCompanyId);

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
            Guid.NewGuid().ToString(),
            "Test N2N",
            Bpn,
            "Munich",
            "Street",
            "DE",
            "BY",
            "5",
            "00001",
            new[] { new CompanyUniqueIdData(UniqueIdentifierId.VAT_ID, "DE123456789") },
            Enumerable.Range(1, 10).Select(_ => _fixture.Build<UserDetailData>().With(x => x.IdentityProviderId, (Guid?)null).WithEmailPattern(x => x.Email).Create()).ToImmutableArray(),
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
        A.CallTo(() => _networkRepository.CreateNetworkRegistration(A<string>._, A<Guid>._, A<Guid>._, A<Guid>._, A<Guid>._))
            .Invokes((string externalId, Guid companyId, Guid pId, Guid ospId, Guid companyApplicationId) =>
            {
                networkRegistrations.Add(new NetworkRegistration(Guid.NewGuid(), externalId, companyId, pId, ospId, companyApplicationId, DateTimeOffset.UtcNow));
            });

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

        A.CallTo(() => _userProvisioningService.GetOrCreateCompanyUser(A<IUserRepository>._, "test-alias", A<UserCreationRoleDataIdpInfo>._, newCompanyId, IdpId, Bpn))
            .MustHaveHappened(10, Times.Exactly);
        A.CallTo(() => _identityProviderRepository.CreateCompanyIdentityProviders(A<IEnumerable<(Guid, Guid)>>.That.IsSameSequenceAs(new[] { new ValueTuple<Guid, Guid>(newCompanyId, IdpId) })))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(Bpn)]
    [InlineData((string?)null)]
    public async Task HandlePartnerRegistration_WithValidData_CallsExpected(string? BusinessPartnerNumberl)
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
            Guid.NewGuid().ToString(),
            "Test N2N",
            BusinessPartnerNumberl,
            "Munich",
            "Street",
            "DE",
            "BY",
            "5",
            "00001",
            new[] { new CompanyUniqueIdData(UniqueIdentifierId.VAT_ID, "DE123456789") },
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
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<ValueTuple<ProcessStepTypeId, ProcessStepStatusId, Guid>>>._))
            .Invokes((IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> processStepTypeStatus) =>
            {
                processSteps.AddRange(processStepTypeStatus.Select(x => new ProcessStep(Guid.NewGuid(), x.ProcessStepTypeId, x.ProcessStepStatusId, x.ProcessId, DateTimeOffset.UtcNow)).ToList());
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
        A.CallTo(() => _networkRepository.CreateNetworkRegistration(A<string>._, A<Guid>._, A<Guid>._, A<Guid>._, A<Guid>._))
            .Invokes((string externalId, Guid companyId, Guid pId, Guid ospId, Guid companyApplicationId) =>
            {
                networkRegistrations.Add(new NetworkRegistration(Guid.NewGuid(), externalId, companyId, pId, ospId, companyApplicationId, DateTimeOffset.UtcNow));
            });

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
        processSteps.Should().HaveCount(2).And.Satisfy(
            x => x.ProcessStepStatusId == ProcessStepStatusId.TODO && x.ProcessStepTypeId == ProcessStepTypeId.SYNCHRONIZE_USER,
            x => x.ProcessStepStatusId == ProcessStepStatusId.TODO && x.ProcessStepTypeId == ProcessStepTypeId.MANUAL_DECLINE_OSP);
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

        A.CallTo(() => _userProvisioningService.GetOrCreateCompanyUser(A<IUserRepository>._, "test-alias", A<UserCreationRoleDataIdpInfo>._, newCompanyId, IdpId, BusinessPartnerNumberl))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.CreateCompanyIdentityProviders(A<IEnumerable<(Guid, Guid)>>.That.IsSameSequenceAs(new[] { new ValueTuple<Guid, Guid>(newCompanyId, IdpId) })))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region RetriggerSynchronizeUser

    [Fact]
    public async Task RetriggerSynchronizeUser_CallsExpected()
    {
        // Arrange
        var externalId = Guid.NewGuid().ToString();
        const ProcessStepTypeId ProcessStepId = ProcessStepTypeId.RETRIGGER_SYNCHRONIZE_USER;

        // Act
        await _sut.RetriggerProcessStep(externalId, ProcessStepId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _networkRegistrationProcessHelper.TriggerProcessStep(externalId, ProcessStepId)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Setup

    private void SetupRepos()
    {
        A.CallTo(() => _networkRepository.CheckExternalIdExists(ExistingExternalId, A<Guid>.That.Matches(x => x == _identity.CompanyId || x == NoIdpCompanyId)))
            .Returns(true);
        A.CallTo(() => _networkRepository.CheckExternalIdExists(A<string>.That.Not.Matches(x => x == ExistingExternalId), A<Guid>.That.Matches(x => x == _identity.CompanyId || x == NoIdpCompanyId)))
            .Returns(false);

        A.CallTo(() => _companyRepository.CheckBpnExists(Bpn)).Returns(false);
        A.CallTo(() => _companyRepository.CheckBpnExists(A<string>.That.Not.Matches(x => x == Bpn))).Returns(true);

        A.CallTo(() => _countryRepository.CheckCountryExistsByAlpha2CodeAsync("XX"))
            .Returns(false);
        A.CallTo(() => _countryRepository.CheckCountryExistsByAlpha2CodeAsync(A<string>.That.Not.Matches(x => x == "XX")))
            .Returns(true);
        A.CallTo(() => _countryRepository.GetCountryAssignedIdentifiers("DE", A<IEnumerable<UniqueIdentifierId>>._))
            .Returns(new ValueTuple<bool, IEnumerable<UniqueIdentifierId>>(true, new[] { UniqueIdentifierId.VAT_ID, UniqueIdentifierId.LEI_CODE, UniqueIdentifierId.EORI }));
        A.CallTo(() => _countryRepository.GetCountryAssignedIdentifiers(A<string>.That.Not.Matches(x => x == "DE"), A<IEnumerable<UniqueIdentifierId>>._))
            .Returns(new ValueTuple<bool, IEnumerable<UniqueIdentifierId>>(false, Enumerable.Empty<UniqueIdentifierId>()));

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
