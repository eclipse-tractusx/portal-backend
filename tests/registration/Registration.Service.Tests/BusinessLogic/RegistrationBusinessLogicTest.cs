/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Bpn;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Collections.Immutable;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests.BusinessLogic;

public class RegistrationBusinessLogicTest
{
    private readonly IFixture _fixture;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRoleRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly ICompanyRolesRepository _companyRolesRepository;
    private readonly IConsentRepository _consentRepository;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly IApplicationChecklistCreationService _checklistService;
    private readonly string _iamUserId;
    private readonly IdentityData _identity;
    private readonly Guid _companyUserId;
    private readonly Guid _existingApplicationId;
    private readonly string _displayName;
    private readonly string _alpha2code;
    private readonly TestException _error;
    private readonly IOptions<RegistrationSettings> _options;
    private readonly IMailingService _mailingService;
    private readonly IStaticDataRepository _staticDataRepository;
    private readonly Func<UserCreationRoleDataIdpInfo, (Guid CompanyUserId, string UserName, string? Password, Exception? Error)> _processLine;

    public RegistrationBusinessLogicTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _userProvisioningService = A.Fake<IUserProvisioningService>();
        _mailingService = A.Fake<IMailingService>();
        _invitationRepository = A.Fake<IInvitationRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _userRoleRepository = A.Fake<IUserRolesRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _companyRolesRepository = A.Fake<ICompanyRolesRepository>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _countryRepository = A.Fake<ICountryRepository>();
        _consentRepository = A.Fake<IConsentRepository>();
        _checklistService = A.Fake<IApplicationChecklistCreationService>();
        _staticDataRepository = A.Fake<IStaticDataRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();

        var options = Options.Create(new RegistrationSettings
        {
            BasePortalAddress = "just a test",
            KeycloakClientID = "CatenaX",
        });
        _fixture.Inject(options);
        _fixture.Inject(A.Fake<IMailingService>());
        _fixture.Inject(A.Fake<IBpnAccess>());
        _fixture.Inject(A.Fake<ILogger<RegistrationBusinessLogic>>());

        _options = _fixture.Create<IOptions<RegistrationSettings>>();

        _iamUserId = _fixture.Create<string>();
        _companyUserId = _fixture.Create<Guid>();
        _existingApplicationId = _fixture.Create<Guid>();
        _displayName = _fixture.Create<string>();
        _alpha2code = "XY";
        _identity = _fixture.Build<IdentityData>()
            .With(x => x.UserEntityId, _iamUserId)
            .With(x => x.UserId, _companyUserId)
            .Create();
        _error = _fixture.Create<TestException>();

        _processLine = A.Fake<Func<UserCreationRoleDataIdpInfo, (Guid CompanyUserId, string UserName, string? Password, Exception? Error)>>();

        SetupRepositories();

        _fixture.Inject(_provisioningManager);
        _fixture.Inject(_userProvisioningService);
        _fixture.Inject(_portalRepositories);
    }

    #region GetClientRolesComposite

    [Fact]
    public async Task GetClientRolesCompositeAsync_GetsAllRoles()
    {
        //Arrange
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        // Act
        var result = sut.GetClientRolesCompositeAsync();
        await foreach (var item in result)
        {
            // Assert
            A.CallTo(() => _userRoleRepository.GetClientRolesCompositeAsync(A<string>._)).MustHaveHappenedOnceExactly();
            Assert.NotNull(item);
        }
    }

    #endregion

    #region GetCompanyByIdentifier

    [Fact]
    public async Task GetCompanyByIdentifierAsync_WithValidBpn_FetchesBusinessPartner()
    {
        //Arrange
        var bpnAccess = A.Fake<IBpnAccess>();
        var bpn = "THISBPNISVALID12";
        var token = "justatoken";
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            bpnAccess,
            null!,
            null!,
            null!,
            null!,
            null!);

        // Act
        var result = await sut.GetCompanyByIdentifierAsync(bpn, token, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        result.Should().NotBeNull();
        A.CallTo(() => bpnAccess.FetchBusinessPartner(bpn, token, CancellationToken.None)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompanyByIdentifierAsync_WithValidBpn_ThrowsArgumentException()
    {
        //Arrange
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        // Act
        async Task Act() => await sut.GetCompanyByIdentifierAsync("NotLongEnough", "justatoken", CancellationToken.None).ToListAsync().ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.ParamName.Should().Be("companyIdentifier");
    }

    #endregion

    #region GetCompanyBpdmDetailDataByBusinessPartnerNumber

    [Fact]
    public async Task GetCompanyBpdmDetailDataByBusinessPartnerNumber_WithValidBpn_ReturnsExpected()
    {
        //Arrange
        var bpnAccess = A.Fake<IBpnAccess>();
        var businessPartnerNumber = "THISBPNISVALID12";
        var token = _fixture.Create<string>();
        var country = "XY";

        var uniqueIdSeed = _fixture.CreateMany<(BpdmIdentifierId BpdmIdentifierId, UniqueIdentifierId UniqueIdentifierId, string Value)>(5).ToImmutableArray();
        var name = _fixture.Create<string>();
        var shortName = _fixture.Create<string>();
        var region = _fixture.Create<string>();
        var city = _fixture.Create<string>();
        var streetName = _fixture.Create<string>();
        var streetNumber = _fixture.Create<string>();
        var zipCode = _fixture.Create<string>();

        var bpdmIdentifiers = uniqueIdSeed.Select(x => ((string TechnicalKey, string Value))(x.BpdmIdentifierId.ToString(), x.Value));
        var validIdentifiers = uniqueIdSeed.Skip(2).Take(2).Select(x => (x.BpdmIdentifierId, x.UniqueIdentifierId));

        var legalEntity = _fixture.Build<Bpn.Model.BpdmLegalEntityDto>()
            .With(x => x.Bpn, businessPartnerNumber)
            .With(x => x.Names, new[] { _fixture.Build<Bpn.Model.BpdmNameDto>()
                    .With(x => x.Value, name)
                    .With(x => x.ShortName, shortName)
                    .Create() })
            .With(x => x.Identifiers, bpdmIdentifiers.Select(identifier => _fixture.Build<Bpn.Model.BpdmIdentifierDto>()
                    .With(x => x.Type, _fixture.Build<Bpn.Model.BpdmUrlDataDto>().With(x => x.TechnicalKey, identifier.TechnicalKey).Create())
                    .With(x => x.Value, identifier.Value)
                    .Create()))
            .Create();
        var bpdmAddress = _fixture.Build<Bpn.Model.BpdmLegalEntityAddressDto>()
            .With(x => x.LegalEntity, businessPartnerNumber)
            .With(x => x.LegalAddress, _fixture.Build<Bpn.Model.BpdmLegalAddressDto>()
                .With(x => x.Country, _fixture.Build<Bpn.Model.BpdmDataDto>().With(x => x.TechnicalKey, country).Create())
                .With(x => x.AdministrativeAreas, new[] { _fixture.Build<Bpn.Model.BpdmAdministrativeAreaDto>().With(x => x.Value, region).Create() })
                .With(x => x.PostCodes, new[] { _fixture.Build<Bpn.Model.BpdmPostCodeDto>().With(x => x.Value, zipCode).Create() })
                .With(x => x.Localities, new[] { _fixture.Build<Bpn.Model.BpdmLocalityDto>().With(x => x.Value, city).Create() })
                .With(x => x.Thoroughfares, new[] { _fixture.Build<Bpn.Model.BpdmThoroughfareDto>().With(x => x.Value, streetName).With(x => x.Number, streetNumber).Create() })
                .Create())
            .Create();
        A.CallTo(() => bpnAccess.FetchLegalEntityByBpn(businessPartnerNumber, token, A<CancellationToken>._))
            .Returns(legalEntity);
        A.CallTo(() => bpnAccess.FetchLegalEntityAddressByBpn(businessPartnerNumber, token, A<CancellationToken>._))
            .Returns(new[] { bpdmAddress }.ToAsyncEnumerable());
        A.CallTo(() => _staticDataRepository.GetCountryAssignedIdentifiers(A<IEnumerable<BpdmIdentifierId>>.That.Matches<IEnumerable<BpdmIdentifierId>>(ids => ids.SequenceEqual(uniqueIdSeed.Select(seed => seed.BpdmIdentifierId))), country))
            .Returns((true, validIdentifiers));

        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            bpnAccess,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        // Act
        var result = await sut.GetCompanyBpdmDetailDataByBusinessPartnerNumber(businessPartnerNumber, token, CancellationToken.None).ConfigureAwait(false);

        A.CallTo(() => bpnAccess.FetchLegalEntityByBpn(businessPartnerNumber, token, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => bpnAccess.FetchLegalEntityAddressByBpn(businessPartnerNumber, token, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _staticDataRepository.GetCountryAssignedIdentifiers(A<IEnumerable<BpdmIdentifierId>>.That.Matches<IEnumerable<BpdmIdentifierId>>(ids => ids.SequenceEqual(uniqueIdSeed.Select(seed => seed.BpdmIdentifierId))), country))
            .MustHaveHappenedOnceExactly();

        result.Should().NotBeNull();
        result.BusinessPartnerNumber.Should().Be(businessPartnerNumber);
        result.CountryAlpha2Code.Should().Be(country);

        var expectedUniqueIds = uniqueIdSeed.Skip(2).Take(2).Select(x => new CompanyUniqueIdData(x.UniqueIdentifierId, x.Value));
        result.UniqueIds.Should().HaveSameCount(expectedUniqueIds);
        result.UniqueIds.Should().ContainInOrder(expectedUniqueIds);

        result.Name.Should().Be(name);
        result.ShortName.Should().Be(shortName);
        result.Region.Should().Be(region);
        result.City.Should().Be(city);
        result.StreetName.Should().Be(streetName);
        result.StreetNumber.Should().Be(streetNumber);
        result.ZipCode.Should().Be(zipCode);
    }

    [Fact]
    public async Task GetCompanyBpdmDetailDataByBusinessPartnerNumber_WithValidBpn_ThrowsArgumentException()
    {
        //Arrange
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        // Act
        async Task Act() => await sut.GetCompanyBpdmDetailDataByBusinessPartnerNumber("NotLongEnough", "justatoken", CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.ParamName.Should().Be("businessPartnerNumber");
    }

    #endregion

    #region GetAllApplicationsForUserWithStatus

    [Fact]
    public async Task GetAllApplicationsForUserWithStatus_WithValidUser_GetsAllRoles()
    {
        //Arrange
        var userCompanyId = _fixture.Create<Guid>();
        var identity = _fixture.Build<IdentityData>()
            .With(x => x.CompanyId, userCompanyId)
            .Create();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);
        var resultList = new List<CompanyApplicationWithStatus>
        {
            new()
            {
                ApplicationId = _fixture.Create<Guid>(),
                ApplicationStatus = CompanyApplicationStatusId.VERIFY
            }
        };
        A.CallTo(() => _userRepository.GetApplicationsWithStatusUntrackedAsync(userCompanyId))
            .Returns(resultList.ToAsyncEnumerable());

        // Act
        var result = await sut.GetAllApplicationsForUserWithStatus(identity.CompanyId).ToListAsync().ConfigureAwait(false);
        result.Should().ContainSingle();
        result.Single().ApplicationStatus.Should().Be(CompanyApplicationStatusId.VERIFY);
    }

    #endregion

    #region GetCompanyWithAddress

    [Fact]
    public async Task GetCompanyWithAddressAsync_WithValidApplication_GetsData()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var data = _fixture.Build<CompanyApplicationDetailData>()
            .With(x => x.IsUserOfCompany, true)
            .Create();

        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, _identity.CompanyId, null))
            .Returns(data);

        // Act
        var result = await sut.GetCompanyDetailData(applicationId, _identity.CompanyId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.UniqueIds.Should().HaveSameCount(data.UniqueIds);
    }

    [Fact]
    public async Task GetCompanyWithAddressAsync_WithInvalidApplication_ThrowsNotFoundException()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var userId = _fixture.Create<string>();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, _identity.CompanyId, null))
            .Returns((CompanyApplicationDetailData?)null);

        // Act
        async Task Act() => await sut.GetCompanyDetailData(applicationId, _identity.CompanyId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"CompanyApplication {applicationId} not found");
    }

    [Fact]
    public async Task GetCompanyWithAddressAsync_WithInvalidUser_ThrowsForbiddenException()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, _identity.CompanyId, null))
            .Returns(_fixture.Build<CompanyApplicationDetailData>().With(x => x.IsUserOfCompany, false).Create());

        // Act
        async Task Act() => await sut.GetCompanyDetailData(applicationId, _identity.CompanyId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"The users company is not assigned with CompanyApplication {applicationId}");
    }

    #endregion

    #region SetCompanyWithAddress

    [Theory]
    [InlineData(null, null, null, null, new UniqueIdentifierId[] { }, new string[] { }, "Name")]
    [InlineData("filled", null, null, null, new UniqueIdentifierId[] { }, new string[] { }, "City")]
    [InlineData("filled", "filled", null, null, new UniqueIdentifierId[] { }, new string[] { }, "StreetName")]
    [InlineData("filled", "filled", "filled", "", new UniqueIdentifierId[] { }, new string[] { }, "CountryAlpha2Code")]
    [InlineData("filled", "filled", "filled", "XX", new UniqueIdentifierId[] { UniqueIdentifierId.VAT_ID, UniqueIdentifierId.LEI_CODE }, new string[] { "filled", "" }, "UniqueIds")]
    [InlineData("filled", "filled", "filled", "XX", new UniqueIdentifierId[] { UniqueIdentifierId.VAT_ID, UniqueIdentifierId.VAT_ID }, new string[] { "filled", "filled" }, "UniqueIds")]
    public async Task SetCompanyWithAddressAsync_WithMissingData_ThrowsArgumentException(string? name, string? city, string? streetName, string? countryCode, IEnumerable<UniqueIdentifierId> uniqueIdentifierIds, IEnumerable<string> values, string argumentName)
    {
        //Arrange
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        var uniqueIdData = uniqueIdentifierIds.Zip(values, (id, value) => new CompanyUniqueIdData(id, value));
        var companyData = new CompanyDetailData(Guid.NewGuid(), name!, city!, streetName!, countryCode!, null, null, null, null, null, null, null, uniqueIdData);

        // Act
        async Task Act() => await sut.SetCompanyDetailDataAsync(Guid.NewGuid(), companyData, _fixture.Create<Guid>()).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.ParamName.Should().Be(argumentName);
    }

    [Fact]
    public async Task SetCompanyWithAddressAsync_WithInvalidApplicationId_ThrowsNotFoundException()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        var companyData = new CompanyDetailData(companyId, "name", "munich", "main street", "de", null, null, null, null, null, null, null, Enumerable.Empty<CompanyUniqueIdData>());

        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, A<Guid>._, companyId))
            .ReturnsLazily(() => (CompanyApplicationDetailData?)null);

        // Act
        async Task Act() => await sut.SetCompanyDetailDataAsync(applicationId, companyData, _identity.CompanyId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"CompanyApplication {applicationId} for CompanyId {companyId} not found");
    }

    [Fact]
    public async Task SetCompanyWithAddressAsync_WithoutCompanyUserId_ThrowsForbiddenException()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var identity = _fixture.Build<IdentityData>()
            .With(x => x.CompanyId, companyId)
            .Create();
        var companyData = new CompanyDetailData(companyId, "name", "munich", "main street", "de", null, null, null, null, null, null, null, Enumerable.Empty<CompanyUniqueIdData>());

        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, A<Guid>._, companyId))
            .ReturnsLazily(() => _fixture.Build<CompanyApplicationDetailData>().With(x => x.IsUserOfCompany, false).Create());

        // Act
        async Task Act() => await sut.SetCompanyDetailDataAsync(applicationId, companyData, identity.CompanyId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Contain($" is not assigned with CompanyApplication {applicationId}");
    }

    [Fact]
    public async Task SetCompanyWithAddressAsync_ModifyCompany()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var identity = _fixture.Build<IdentityData>()
            .With(x => x.CompanyId, companyId)
            .Create();
        var companyData = _fixture.Build<CompanyDetailData>()
            .With(x => x.CompanyId, companyId)
            .With(x => x.CountryAlpha2Code, _alpha2code)
            .Create();

        var existingData = _fixture.Build<CompanyApplicationDetailData>()
            .With(x => x.IsUserOfCompany, true)
            .Create();

        Company? company = null;

        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, A<Guid>._, identity.CompanyId))
            .Returns(existingData);

        A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, A<Action<Company>>._, A<Action<Company>>._))
            .Invokes((Guid companyId, Action<Company>? initialize, Action<Company> modify) =>
            {
                company = new Company(companyId, null!, default, default);
                initialize?.Invoke(company);
                modify(company);
            });

        // Act
        await sut.SetCompanyDetailDataAsync(applicationId, companyData, identity.CompanyId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, A<Action<Company>>._, A<Action<Company>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();

        company.Should().NotBeNull();
        company.Should().Match<Company>(c =>
            c.Id == companyId &&
            c.Name == companyData.Name &&
            c.Shortname == companyData.ShortName &&
            c.BusinessPartnerNumber == companyData.BusinessPartnerNumber);
    }

    [Fact]
    public async Task SetCompanyWithAddressAsync_WithoutInitialCompanyAddress_CreatesAddress()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var addressId = Guid.NewGuid();

        var identity = _fixture.Build<IdentityData>()
            .With(x => x.CompanyId, companyId)
            .Create();
        var companyData = _fixture.Build<CompanyDetailData>()
            .With(x => x.CompanyId, companyId)
            .With(x => x.CountryAlpha2Code, _alpha2code)
            .Create();

        var existingData = _fixture.Build<CompanyApplicationDetailData>()
            .With(x => x.CompanyId, companyId)
            .With(x => x.AddressId, (Guid?)null)
            .With(x => x.City, (string)null!)
            .With(x => x.CountryAlpha2Code, (string)null!)
            .With(x => x.CountryNameDe, (string)null!)
            .With(x => x.Region, (string)null!)
            .With(x => x.Streetadditional, (string)null!)
            .With(x => x.Streetname, (string)null!)
            .With(x => x.Streetnumber, (string)null!)
            .With(x => x.Zipcode, (string)null!)
            .With(x => x.IsUserOfCompany, true)
            .Create();

        Company? company = null;
        Address? address = null;

        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, A<Guid>._, companyId))
            .Returns(existingData);

        A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, A<Action<Company>>._, A<Action<Company>>._))
            .Invokes((Guid companyId, Action<Company>? initialize, Action<Company> modify) =>
            {
                company = new Company(companyId, null!, default, default);
                initialize?.Invoke(company);
                modify(company);
            });

        A.CallTo(() => _companyRepository.CreateAddress(A<string>._, A<string>._, A<string>._, A<Action<Address>>._))
            .ReturnsLazily((string city, string streetName, string alpha2Code, Action<Address>? setParameters) =>
            {
                address = new Address(addressId, city, streetName, alpha2Code, default);
                setParameters?.Invoke(address);
                return address;
            });

        // Act
        await sut.SetCompanyDetailDataAsync(applicationId, companyData, identity.CompanyId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companyRepository.CreateAddress(A<string>._, A<string>._, A<string>._, A<Action<Address>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRepository.CreateAddress(companyData.City, companyData.StreetName, companyData.CountryAlpha2Code, A<Action<Address>>._))
            .MustHaveHappened();
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, A<Action<Company>>._, A<Action<Company>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(companyId, A<Action<Company>>._, A<Action<Company>>._))
            .MustHaveHappened();
        A.CallTo(() => _companyRepository.AttachAndModifyAddress(A<Guid>._, A<Action<Address>>._, A<Action<Address>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();

        company.Should().NotBeNull();
        address.Should().NotBeNull();

        company!.Id.Should().Be(companyId);
        company.AddressId.Should().Be(addressId);

        address.Should().Match<Address>(a =>
            a.Id == addressId &&
            a.City == companyData.City &&
            a.CountryAlpha2Code == companyData.CountryAlpha2Code &&
            a.Region == companyData.Region &&
            a.Streetadditional == companyData.StreetAdditional &&
            a.Streetname == companyData.StreetName &&
            a.Streetnumber == companyData.StreetNumber &&
            a.Zipcode == companyData.ZipCode);
    }

    [Fact]
    public async Task SetCompanyWithAddressAsync_WithInitialCompanyAddress_ModifyAddress()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var identity = _fixture.Build<IdentityData>()
            .With(x => x.CompanyId, companyId)
            .Create();
        var companyData = _fixture.Build<CompanyDetailData>()
            .With(x => x.CompanyId, companyId)
            .With(x => x.CountryAlpha2Code, _alpha2code)
            .Create();

        var existingData = _fixture.Build<CompanyApplicationDetailData>()
            .With(x => x.CompanyId, companyId)
            .With(x => x.IsUserOfCompany, true)
            .Create();

        Company? company = null;
        Address? address = null;

        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, A<Guid>._, identity.CompanyId))
            .Returns(existingData);

        A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, A<Action<Company>>._, A<Action<Company>>._))
            .Invokes((Guid companyId, Action<Company>? initialize, Action<Company> modify) =>
            {
                company = new Company(companyId, null!, default, default);
                initialize?.Invoke(company);
                modify(company);
            });

        A.CallTo(() => _companyRepository.AttachAndModifyAddress(existingData.AddressId!.Value, A<Action<Address>>._, A<Action<Address>>._))
            .Invokes((Guid addressId, Action<Address>? initialize, Action<Address> modify) =>
            {
                address = new Address(addressId, null!, null!, null!, default);
                initialize?.Invoke(address);
                modify(address);
            });

        // Act
        await sut.SetCompanyDetailDataAsync(applicationId, companyData, identity.CompanyId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companyRepository.CreateAddress(A<string>._, A<string>._, A<string>._, A<Action<Address>?>._))
            .MustNotHaveHappened();
        A.CallTo(() => _companyRepository.AttachAndModifyAddress(A<Guid>._, A<Action<Address>>._, A<Action<Address>>._!))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRepository.AttachAndModifyAddress(existingData.AddressId!.Value, A<Action<Address>>._, A<Action<Address>>._!))
            .MustHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();

        company.Should().NotBeNull();
        address.Should().NotBeNull();

        company!.Id.Should().Be(companyId);
        company.AddressId.Should().Be(existingData.AddressId!.Value);

        address.Should().Match<Address>(a =>
            a.Id == existingData.AddressId!.Value &&
            a.City == companyData.City &&
            a.CountryAlpha2Code == companyData.CountryAlpha2Code &&
            a.Region == companyData.Region &&
            a.Streetadditional == companyData.StreetAdditional &&
            a.Streetname == companyData.StreetName &&
            a.Streetnumber == companyData.StreetNumber &&
            a.Zipcode == companyData.ZipCode);
    }

    [Fact]
    public async Task SetCompanyWithAddressAsync_WithUniqueIdentifiers_CreateModifyDeleteExpected()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var identity = _fixture.Build<IdentityData>()
            .With(x => x.CompanyId, companyId)
            .Create();
        var uniqueIdentifiers = _fixture.CreateMany<UniqueIdentifierId>(4);

        var firstIdData = _fixture.Build<CompanyUniqueIdData>().With(x => x.UniqueIdentifierId, uniqueIdentifiers.First()).Create();       // shall not modify
        var secondIdData = _fixture.Build<CompanyUniqueIdData>().With(x => x.UniqueIdentifierId, uniqueIdentifiers.ElementAt(1)).Create(); // shall modify
        var thirdIdData = _fixture.Build<CompanyUniqueIdData>().With(x => x.UniqueIdentifierId, uniqueIdentifiers.ElementAt(2)).Create();  // shall create new

        var companyData = _fixture.Build<CompanyDetailData>()
            .With(x => x.CompanyId, companyId)
            .With(x => x.CountryAlpha2Code, _alpha2code)
            .With(x => x.UniqueIds, new[] { firstIdData, secondIdData, thirdIdData })
            .Create();

        var existingData = _fixture.Build<CompanyApplicationDetailData>()
            .With(x => x.UniqueIds, new[] {
                (firstIdData.UniqueIdentifierId, firstIdData.Value),            // shall be left unmodified
                (secondIdData.UniqueIdentifierId, _fixture.Create<string>()),   // shall be modified
                (uniqueIdentifiers.ElementAt(3), _fixture.Create<string>()) })  // shall be deleted
            .With(x => x.IsUserOfCompany, true)
            .Create();

        IEnumerable<(UniqueIdentifierId UniqueIdentifierId, string Value)>? initialIdentifiers = null;
        IEnumerable<(UniqueIdentifierId UniqueIdentifierId, string Value)>? modifiedIdentifiers = null;

        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, A<Guid>._, companyId))
            .Returns(existingData);

        A.CallTo(() => _companyRepository.CreateUpdateDeleteIdentifiers(A<Guid>._, A<IEnumerable<(UniqueIdentifierId, string)>>._, A<IEnumerable<(UniqueIdentifierId, string)>>._))
            .Invokes((Guid _, IEnumerable<(UniqueIdentifierId UniqueIdentifierId, string Value)> initial, IEnumerable<(UniqueIdentifierId UniqueIdentifierId, string Value)> modified) =>
            {
                initialIdentifiers = initial;
                modifiedIdentifiers = modified;
            });

        // Act
        await sut.SetCompanyDetailDataAsync(applicationId, companyData, identity.CompanyId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companyRepository.CreateUpdateDeleteIdentifiers(companyId, A<IEnumerable<(UniqueIdentifierId, string)>>._, A<IEnumerable<(UniqueIdentifierId, string)>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRepository.CreateUpdateDeleteIdentifiers(A<Guid>.That.Not.IsEqualTo(companyId), A<IEnumerable<(UniqueIdentifierId, string)>>._, A<IEnumerable<(UniqueIdentifierId, string)>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();

        initialIdentifiers.Should().NotBeNull();
        modifiedIdentifiers.Should().NotBeNull();
        initialIdentifiers.Should().ContainInOrder(existingData.UniqueIds);
        modifiedIdentifiers.Should().ContainInOrder((firstIdData.UniqueIdentifierId, firstIdData.Value), (secondIdData.UniqueIdentifierId, secondIdData.Value), (thirdIdData.UniqueIdentifierId, thirdIdData.Value));
    }

    [Fact]
    public async Task SetCompanyWithAddressAsync_WithInvalidCountryCode_Throws()
    {
        //Arrange
        var companyData = _fixture.Build<CompanyDetailData>()
            .With(x => x.CountryAlpha2Code, _alpha2code)
            .Create();

        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        A.CallTo(() => _countryRepository.GetCountryAssignedIdentifiers(A<string>._, A<IEnumerable<UniqueIdentifierId>>._))
            .Returns((false, null!));

        // Act
        var Act = () => sut.SetCompanyDetailDataAsync(Guid.NewGuid(), companyData, _fixture.Create<Guid>());

        //Assert
        var result = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"{_alpha2code} is not a valid country-code (Parameter 'UniqueIds')");
    }

    [Fact]
    public async Task SetCompanyWithAddressAsync_WithInvalidUniqueIdentifiers_Throws()
    {
        //Arrange
        var identifiers = _fixture.CreateMany<UniqueIdentifierId>(2);

        var companyData = _fixture.Build<CompanyDetailData>()
            .With(x => x.CountryAlpha2Code, _alpha2code)
            .With(x => x.UniqueIds, identifiers.Select(id => _fixture.Build<CompanyUniqueIdData>().With(x => x.UniqueIdentifierId, id).Create()))
            .Create();

        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        A.CallTo(() => _countryRepository.GetCountryAssignedIdentifiers(_alpha2code, A<IEnumerable<UniqueIdentifierId>>._))
            .Returns((true, new[] { identifiers.First() }));

        // Act
        var Act = () => sut.SetCompanyDetailDataAsync(Guid.NewGuid(), companyData, _fixture.Create<Guid>());

        //Assert
        var result = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"invalid uniqueIds for country {_alpha2code}: '{identifiers.ElementAt(1)}' (Parameter 'UniqueIds')");
    }

    #endregion

    #region SetOwnCompanyApplicationStatus

    [Fact]
    public async Task SetOwnCompanyApplicationStatusAsync_WithInvalidStatus_ThrowsControllerArgumentException()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        // Act
        async Task Act() => await sut.SetOwnCompanyApplicationStatusAsync(applicationId, 0, _fixture.Create<Guid>()).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be("status must not be null");
    }

    [Fact]
    public async Task SetOwnCompanyApplicationStatusAsync_WithInvalidApplication_ThrowsNotFoundException()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserDataAsync(A<Guid>._, A<Guid>._))
            .ReturnsLazily(() => new ValueTuple<bool, CompanyApplicationStatusId>());

        // Act
        async Task Act() => await sut.SetOwnCompanyApplicationStatusAsync(applicationId, CompanyApplicationStatusId.VERIFY, _fixture.Create<Guid>()).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"CompanyApplication {applicationId} not found");
    }

    [Fact]
    public async Task SetOwnCompanyApplicationStatusAsync_WithInvalidStatus_ThrowsArgumentException()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserDataAsync(A<Guid>._, A<Guid>._))
            .ReturnsLazily(() => new ValueTuple<bool, CompanyApplicationStatusId>(true, CompanyApplicationStatusId.CREATED));

        // Act
        async Task Act() => await sut.SetOwnCompanyApplicationStatusAsync(applicationId, CompanyApplicationStatusId.VERIFY, _fixture.Create<Guid>()).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(Act).ConfigureAwait(false);
        ex.Message.Should().Contain("invalid status update requested");
    }

    [Fact]
    public async Task SetOwnCompanyApplicationStatusAsync_WithValidData_SavesChanges()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserDataAsync(A<Guid>._, A<Guid>._))
            .ReturnsLazily(() => new ValueTuple<bool, CompanyApplicationStatusId>(true, CompanyApplicationStatusId.VERIFY));

        // Act
        await sut.SetOwnCompanyApplicationStatusAsync(applicationId, CompanyApplicationStatusId.SUBMITTED, _fixture.Create<Guid>()).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetCompanyRoles

    [Fact]
    public async Task GetCompanyRolesAsync_()
    {
        //Arrange
        var companyRolesRepository = A.Fake<ICompanyRolesRepository>();
        A.CallTo(() => companyRolesRepository.GetCompanyRolesAsync(A<string?>._))
            .Returns(_fixture.CreateMany<CompanyRolesDetails>(2).ToAsyncEnumerable());
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRolesRepository>())
            .Returns(companyRolesRepository);
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);
        // Act
        var result = await sut.GetCompanyRoles().ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetInvitedUser

    [Fact]
    public async Task Get_WhenThereAreInvitedUser_ShouldReturnInvitedUserWithRoles()
    {
        //Arrange
        var sut = _fixture.Create<RegistrationBusinessLogic>();

        //Act
        var result = sut.GetInvitedUsersAsync(_existingApplicationId);
        await foreach (var item in result)
        {
            //Assert
            A.CallTo(() => _invitationRepository.GetInvitedUserDetailsUntrackedAsync(_existingApplicationId)).MustHaveHappened(1, Times.OrMore);
            A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(A<string>._, A<string>._)).MustHaveHappened(1, Times.OrMore);
            Assert.NotNull(item);
            Assert.IsType<InvitedUser>(item);
        }
    }

    [Fact]
    public async Task GetInvitedUsersDetail_ThrowException_WhenIdIsNull()
    {
        //Arrange
        var sut = _fixture.Create<RegistrationBusinessLogic>();

        //Act
        async Task Action() => await sut.GetInvitedUsersAsync(Guid.Empty).ToListAsync().ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<Exception>(Action);
    }

    #endregion

    #region UploadDocument

    [Fact]
    public async Task UploadDocumentAsync_WithValidData_CreatesDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        var documents = new List<Document>();
        var settings = new RegistrationSettings
        {
            DocumentTypeIds = new[]{
                DocumentTypeId.CX_FRAME_CONTRACT,
                DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT
            }
        };
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, A<DocumentTypeId>._, A<Action<Document>?>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentTypeId, Action<Document>? action) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, mediaTypeId, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentTypeId);
                action?.Invoke(document);
                documents.Add(document);
            });

        var sut = new RegistrationBusinessLogic(
            Options.Create(settings),
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        // Act
        await sut.UploadDocumentAsync(_existingApplicationId, file, DocumentTypeId.CX_FRAME_CONTRACT, (_identity.UserId, _identity.CompanyId), CancellationToken.None);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        documents.Should().HaveCount(1);
    }

    [Fact]
    public async Task UploadDocumentAsync_WithJsonDocument_ThrowsException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.json", "application/json");
        var sut = _fixture.Create<RegistrationBusinessLogic>();

        // Act
        async Task Action() => await sut.UploadDocumentAsync(_existingApplicationId, file, DocumentTypeId.ADDITIONAL_DETAILS, (_identity.UserId, _identity.CompanyId), CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Action);
        ex.Message.Should().Be("Only .pdf files are allowed.");
    }

    [Fact]
    public async Task UploadDocumentAsync_WithEmptyTitle_ThrowsException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("this is just a test", string.Empty, "application/pdf");
        var sut = _fixture.Create<RegistrationBusinessLogic>();

        // Act
        async Task Action() => await sut.UploadDocumentAsync(_existingApplicationId, file, DocumentTypeId.ADDITIONAL_DETAILS, (_identity.UserId, _identity.CompanyId), CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.Message.Should().Be("File name is must not be null");
    }

    [Fact]
    public async Task UploadDocumentAsync_WithNotExistingApplicationId_ThrowsException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        var settings = new RegistrationSettings
        {
            DocumentTypeIds = new[]{
                DocumentTypeId.CX_FRAME_CONTRACT,
                DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT
            }
        };
        var sut = new RegistrationBusinessLogic(
            Options.Create(settings),
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);
        var notExistingId = Guid.NewGuid();

        // Act
        async Task Action() => await sut.UploadDocumentAsync(notExistingId, file, DocumentTypeId.CX_FRAME_CONTRACT, (_identity.UserId, _identity.CompanyId), CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Action);
        ex.Message.Should().Be($"The users company is not assigned with application {notExistingId}");
    }

    [Fact]
    public async Task UploadDocumentAsync_WithNotExistingIamUser_ThrowsException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        var settings = new RegistrationSettings
        {
            DocumentTypeIds = new[]{
                DocumentTypeId.CX_FRAME_CONTRACT,
                DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT
            }
        };
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>().IsValidApplicationForCompany(A<Guid>._, A<Guid>._))
            .Returns(false);

        var sut = new RegistrationBusinessLogic(
            Options.Create(settings),
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);
        var identity = _fixture.Create<IdentityData>();

        // Act
        async Task Action() => await sut.UploadDocumentAsync(_existingApplicationId, file, DocumentTypeId.CX_FRAME_CONTRACT, (_identity.UserId, _identity.CompanyId), CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Action);
        ex.Message.Should().Be($"The users company is not assigned with application {_existingApplicationId}");
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>().IsValidApplicationForCompany(_existingApplicationId, _identity.CompanyId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UploadDocumentAsync_WithInvalidDocumentTypeId_ThrowsException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        var settings = new RegistrationSettings
        {
            DocumentTypeIds = new[]{
                DocumentTypeId.CX_FRAME_CONTRACT,
                DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT
            }
        };
        var sut = new RegistrationBusinessLogic(
            Options.Create(settings),
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);
        // Act
        async Task Action() => await sut.UploadDocumentAsync(_existingApplicationId, file, DocumentTypeId.ADDITIONAL_DETAILS, (_identity.UserId, _identity.CompanyId), CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.Message.Should().Be($"documentType must be either: {string.Join(",", settings.DocumentTypeIds)}");
    }

    #endregion

    #region InviteNewUser

    [Fact]
    public async Task TestInviteNewUserAsyncSuccess()
    {
        SetupFakesForInvitation();

        var userCreationInfo = _fixture.Create<UserCreationInfoWithMessage>();

        var sut = new RegistrationBusinessLogic(
            _options,
            _mailingService,
            null!,
            _provisioningManager,
            _userProvisioningService,
            null!,
            _portalRepositories,
            null!);

        await sut.InviteNewUserAsync(_existingApplicationId, userCreationInfo, (_identity.UserId, _identity.CompanyId)).ConfigureAwait(false);

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._, A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._, A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => _applicationRepository.CreateInvitation(A<Guid>.That.IsEqualTo(_existingApplicationId), A<Guid>._)).MustHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>.That.IsEqualTo(userCreationInfo.eMail), A<IDictionary<string, string>>.That.Matches(x => x["companyName"] == _displayName), A<List<string>>._)).MustHaveHappened();
    }

    [Fact]
    public async Task TestInviteNewUserEmptyEmailThrows()
    {
        SetupFakesForInvitation();

        var userCreationInfo = _fixture.Build<UserCreationInfoWithMessage>()
            .With(x => x.firstName, _fixture.CreateName())
            .With(x => x.lastName, _fixture.CreateName())
            .With(x => x.eMail, "")
            .Create();

        var sut = new RegistrationBusinessLogic(
            _options,
            _mailingService,
            null!,
            _provisioningManager,
            _userProvisioningService,
            null!,
            _portalRepositories,
            null!);

        Task Act() => sut.InviteNewUserAsync(_existingApplicationId, userCreationInfo, (_identity.UserId, _identity.CompanyId));

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be("email must not be empty");

        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<List<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestInviteNewUserUserAlreadyExistsThrows()
    {
        SetupFakesForInvitation();

        A.CallTo(() => _userRepository.IsOwnCompanyUserWithEmailExisting(A<string>._, A<Guid>._)).Returns(true);

        var userCreationInfo = _fixture.Create<UserCreationInfoWithMessage>();

        var sut = new RegistrationBusinessLogic(
            _options,
            _mailingService,
            null!,
            _provisioningManager,
            _userProvisioningService,
            null!,
            _portalRepositories,
            null!);

        Task Act() => sut.InviteNewUserAsync(_existingApplicationId, userCreationInfo, (_identity.UserId, _identity.CompanyId));

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"user with email {userCreationInfo.eMail} does already exist");

        A.CallTo(() => _userRepository.IsOwnCompanyUserWithEmailExisting(userCreationInfo.eMail, _identity.CompanyId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<List<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestInviteNewUserAsyncCreationErrorThrows()
    {
        SetupFakesForInvitation();

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>._)).ReturnsLazily(
            (UserCreationRoleDataIdpInfo creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
                .With(x => x.UserName, creationInfo.UserName)
                .With(x => x.Error, _error)
                .Create());

        var userCreationInfo = _fixture.Create<UserCreationInfoWithMessage>();

        var sut = new RegistrationBusinessLogic(
            _options,
            _mailingService,
            null!,
            _provisioningManager,
            _userProvisioningService,
            null!,
            _portalRepositories,
            null!);

        Task Act() => sut.InviteNewUserAsync(_existingApplicationId, userCreationInfo, (_identity.UserId, _identity.CompanyId));

        var error = await Assert.ThrowsAsync<TestException>(Act).ConfigureAwait(false);
        error.Message.Should().Be(_error.Message);

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._, A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._, A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => _applicationRepository.CreateInvitation(A<Guid>.That.IsEqualTo(_existingApplicationId), A<Guid>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>.That.IsEqualTo(userCreationInfo.eMail), A<IDictionary<string, string>>._, A<List<string>>._)).MustNotHaveHappened();
    }

    #endregion

    #region GetUploadedDocuments

    [Fact]
    public async Task GetUploadedDocumentsAsync_ReturnsExpectedOutput()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var identity = _fixture.Create<IdentityData>();
        var uploadDocuments = _fixture.CreateMany<UploadDocuments>(3);

        A.CallTo(() => _documentRepository.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT, identity.UserId))
            .Returns((true, uploadDocuments));

        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);
        // Act
        var result = await sut.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT, identity.UserId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveSameCount(uploadDocuments);
        result.Should().ContainInOrder(uploadDocuments);
    }

    [Fact]
    public async Task GetUploadedDocumentsAsync_InvalidApplication_ThrowsNotFound()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var identity = _fixture.Create<IdentityData>();

        A.CallTo(() => _documentRepository.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT, identity.UserId))
            .Returns(((bool, IEnumerable<UploadDocuments>))default);

        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        Task Act() => sut.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT, identity.UserId);

        // Act
        var error = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

        // Assert
        error.Message.Should().Be($"application {applicationId} not found");
    }

    [Fact]
    public async Task GetUploadedDocumentsAsync_InvalidUser_ThrowsForbidden()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var identity = _fixture.Create<IdentityData>();

        A.CallTo(() => _documentRepository.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT, identity.UserId))
            .Returns((false, Enumerable.Empty<UploadDocuments>()));

        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        Task Act() => sut.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT, identity.UserId);

        // Act
        var error = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);

        // Assert
        error.Message.Should().Be($"The user is not associated with application {applicationId}");
    }

    #endregion

    #region SubmitRoleConsents

    [Fact]
    public async Task SubmitRoleConsentsAsync_WithNotExistingApplication_ThrowsNotFoundException()
    {
        // Arrange
        var notExistingId = _fixture.Create<Guid>();
        A.CallTo(() => _companyRolesRepository.GetCompanyRoleAgreementConsentDataAsync(notExistingId))
            .ReturnsLazily(() => (CompanyRoleAgreementConsentData?)null);
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRoleConsentAsync(notExistingId, _fixture.Create<CompanyRoleAgreementConsents>(), _identity.UserId, _identity.CompanyId)
                .ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"application {notExistingId} does not exist");
    }

    [Fact]
    public async Task SubmitRoleConsentsAsync_WithWrongCompanyUser_ThrowsForbiddenException()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var applicationStatusId = _fixture.Create<CompanyApplicationStatusId>();
        var data = new CompanyRoleAgreementConsentData(Guid.NewGuid(), applicationStatusId, _fixture.CreateMany<CompanyRoleId>(2), _fixture.CreateMany<ConsentData>(5));
        A.CallTo(() => _companyRolesRepository.GetCompanyRoleAgreementConsentDataAsync(applicationId))
            .ReturnsLazily(() => data);
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRoleConsentAsync(applicationId, _fixture.Create<CompanyRoleAgreementConsents>(), _identity.UserId, _identity.CompanyId)
            .ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"The users company is not assigned with CompanyApplication {applicationId}");
    }

    [Fact]
    public async Task SubmitRoleConsentsAsync_WithInvalidRoles_ThrowsControllerArgumentException()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var applicationStatusId = _fixture.Create<CompanyApplicationStatusId>();
        var data = new CompanyRoleAgreementConsentData(_identity.CompanyId, applicationStatusId, _fixture.CreateMany<CompanyRoleId>(2), _fixture.CreateMany<ConsentData>(5));
        var roleIds = new List<CompanyRoleId>
        {
            CompanyRoleId.APP_PROVIDER,
        };
        var companyRoleAssignedAgreements = new List<(CompanyRoleId CompanyRoleId, IEnumerable<Guid> AgreementIds)>
        {
            new ValueTuple<CompanyRoleId, IEnumerable<Guid>>(CompanyRoleId.APP_PROVIDER, _fixture.CreateMany<Guid>(5)),
        };
        A.CallTo(() => _companyRolesRepository.GetCompanyRoleAgreementConsentDataAsync(applicationId))
            .ReturnsLazily(() => data);
        A.CallTo(() => _companyRolesRepository.GetAgreementAssignedCompanyRolesUntrackedAsync(roleIds))
            .Returns(companyRoleAssignedAgreements.ToAsyncEnumerable());
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRoleConsentAsync(applicationId, _fixture.Create<CompanyRoleAgreementConsents>(), _identity.UserId, _identity.CompanyId)
            .ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Contain("invalid companyRole: ");
    }

    [Fact]
    public async Task SubmitRoleConsentsAsync_WithoutAllRolesConsentGiven_ThrowsControllerArgumentException()
    {
        // Arrange
        var consents = new CompanyRoleAgreementConsents(new[]
            {
                CompanyRoleId.APP_PROVIDER,
            },
            new[]
            {
                new AgreementConsentStatus(new("0a283850-5a73-4940-9215-e713d0e1c419"), ConsentStatusId.ACTIVE),
                new AgreementConsentStatus(new("e38da3a1-36f9-4002-9447-c55a38ac2a53"), ConsentStatusId.INACTIVE)
            });
        var applicationId = _fixture.Create<Guid>();
        var applicationStatusId = _fixture.Create<CompanyApplicationStatusId>();
        var agreementIds = new List<Guid>
        {
            new("0a283850-5a73-4940-9215-e713d0e1c419"),
            new ("e38da3a1-36f9-4002-9447-c55a38ac2a53")
        };
        var data = new CompanyRoleAgreementConsentData(_identity.CompanyId, applicationStatusId, new[] { CompanyRoleId.APP_PROVIDER }, new List<ConsentData>());
        var companyRoleAssignedAgreements = new List<(CompanyRoleId CompanyRoleId, IEnumerable<Guid> AgreementIds)>
        {
            new ValueTuple<CompanyRoleId, IEnumerable<Guid>>(CompanyRoleId.APP_PROVIDER, agreementIds)
        };
        A.CallTo(() => _companyRolesRepository.GetCompanyRoleAgreementConsentDataAsync(applicationId))
            .ReturnsLazily(() => data);
        A.CallTo(() => _companyRolesRepository.GetAgreementAssignedCompanyRolesUntrackedAsync(A<IEnumerable<CompanyRoleId>>._))
            .Returns(companyRoleAssignedAgreements.ToAsyncEnumerable());

        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRoleConsentAsync(applicationId, consents, _identity.UserId, _identity.CompanyId)
            .ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("consent must be given to all CompanyRole assigned agreements");
    }

    [Fact]
    public async Task SubmitRoleConsentsAsync_WithValidData_CallsExpected()
    {
        var agreementId_1 = _fixture.Create<Guid>();
        var agreementId_2 = _fixture.Create<Guid>();
        var agreementId_3 = _fixture.Create<Guid>();

        var consentId = _fixture.Create<Guid>();

        IEnumerable<CompanyRoleId>? removedCompanyRoleIds = null;

        // Arrange
        var consents = new CompanyRoleAgreementConsents(new[]
            {
                CompanyRoleId.APP_PROVIDER,
                CompanyRoleId.ACTIVE_PARTICIPANT
            },
            new[]
            {
                new AgreementConsentStatus(agreementId_1, ConsentStatusId.ACTIVE),
                new AgreementConsentStatus(agreementId_2, ConsentStatusId.ACTIVE)
            });
        var applicationId = _fixture.Create<Guid>();
        var applicationStatusId = CompanyApplicationStatusId.INVITE_USER;
        var data = new CompanyRoleAgreementConsentData(
            _identity.CompanyId,
            applicationStatusId,
            new[]
            {
                CompanyRoleId.APP_PROVIDER,
                CompanyRoleId.SERVICE_PROVIDER,
            },
            new[] {
                new ConsentData(consentId, ConsentStatusId.INACTIVE, agreementId_1)
            });
        var companyRoleAssignedAgreements = new List<(CompanyRoleId CompanyRoleId, IEnumerable<Guid> AgreementIds)>
        {
            new ValueTuple<CompanyRoleId, IEnumerable<Guid>>(CompanyRoleId.APP_PROVIDER, new [] { agreementId_1, agreementId_2 }),
            new ValueTuple<CompanyRoleId, IEnumerable<Guid>>(CompanyRoleId.ACTIVE_PARTICIPANT, new [] { agreementId_1 }),
            new ValueTuple<CompanyRoleId, IEnumerable<Guid>>(CompanyRoleId.SERVICE_PROVIDER, new [] { agreementId_1, agreementId_3 }),
        };
        A.CallTo(() => _companyRolesRepository.GetCompanyRoleAgreementConsentDataAsync(applicationId))
            .Returns(data);
        A.CallTo(() => _companyRolesRepository.GetAgreementAssignedCompanyRolesUntrackedAsync(A<IEnumerable<CompanyRoleId>>._))
            .Returns(companyRoleAssignedAgreements.ToAsyncEnumerable());
        A.CallTo(() => _consentRepository.AttachAndModifiesConsents(A<IEnumerable<Guid>>._, A<Action<Consent>>._))
            .Invokes((IEnumerable<Guid> consentIds, Action<Consent> setOptionalParameter) =>
            {
                var consents = consentIds.Select(x => new Consent(x, Guid.Empty, Guid.Empty, Guid.Empty, default, default));
                foreach (var consent in consents)
                {
                    setOptionalParameter.Invoke(consent);
                }
            });
        A.CallTo(() => _applicationRepository.AttachAndModifyCompanyApplication(A<Guid>._, A<Action<CompanyApplication>>._))
            .Invokes((Guid companyApplicationId, Action<CompanyApplication> setOptionalParameters) =>
            {
                var companyApplication = new CompanyApplication(companyApplicationId, Guid.Empty, default!, default!);
                setOptionalParameters.Invoke(companyApplication);
            });
        A.CallTo(() => _companyRolesRepository.RemoveCompanyAssignedRoles(_identity.CompanyId, A<IEnumerable<CompanyRoleId>>._))
            .Invokes((Guid _, IEnumerable<CompanyRoleId> companyRoleIds) =>
            {
                removedCompanyRoleIds = companyRoleIds;
            });

        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        await sut.SubmitRoleConsentAsync(applicationId, consents, _identity.UserId, _identity.CompanyId).ConfigureAwait(false);

        // Arrange
        A.CallTo(() => _consentRepository.AttachAndModifiesConsents(A<IEnumerable<Guid>>._, A<Action<Consent>>._)).MustHaveHappened(2, Times.Exactly);
        A.CallTo(() => _consentRepository.CreateConsent(A<Guid>._, A<Guid>._, A<Guid>._, A<ConsentStatusId>._, A<Action<Consent>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRolesRepository.CreateCompanyAssignedRole(_identity.CompanyId, CompanyRoleId.ACTIVE_PARTICIPANT)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRolesRepository.CreateCompanyAssignedRole(A<Guid>.That.Not.IsEqualTo(_identity.CompanyId), A<CompanyRoleId>._)).MustNotHaveHappened();
        A.CallTo(() => _companyRolesRepository.CreateCompanyAssignedRole(A<Guid>._, A<CompanyRoleId>.That.Not.IsEqualTo(CompanyRoleId.ACTIVE_PARTICIPANT))).MustNotHaveHappened();
        A.CallTo(() => _companyRolesRepository.RemoveCompanyAssignedRoles(_identity.CompanyId, A<IEnumerable<CompanyRoleId>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRolesRepository.RemoveCompanyAssignedRoles(A<Guid>.That.Not.IsEqualTo(_identity.CompanyId), A<IEnumerable<CompanyRoleId>>._)).MustNotHaveHappened();
        removedCompanyRoleIds.Should().NotBeNull();
        removedCompanyRoleIds.Should().ContainSingle(x => x == CompanyRoleId.SERVICE_PROVIDER);
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region SubmitRegistrationAsync

    [Fact]
    public async Task SubmitRegistrationAsync_WithNotExistingApplication_ThrowsNotFoundException()
    {
        // Arrange
        var notExistingId = _fixture.Create<Guid>();
        var settings = new RegistrationSettings
        {
            SubmitDocumentTypeIds = new[]{
                DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT
            }
        };
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(A<Guid>._, A<Guid>._, A<IEnumerable<DocumentTypeId>>._))
            .Returns((CompanyApplicationUserEmailData?)null);
        var sut = new RegistrationBusinessLogic(Options.Create(settings), _mailingService, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRegistrationAsync(notExistingId, _identity.UserId)
            .ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"application {notExistingId} does not exist");
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(notExistingId, _identity.UserId, A<IEnumerable<DocumentTypeId>>.That.IsSameSequenceAs(new[] { DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SubmitRegistrationAsync_WithDocumentId_Success()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var documents = new DocumentStatusData[] {
            new(Guid.NewGuid(),DocumentStatusId.PENDING),
            new(Guid.NewGuid(),DocumentStatusId.INACTIVE)
        };
        var checklist = _fixture.CreateMany<ApplicationChecklistEntryTypeId>(3).Select(x => (x, ApplicationChecklistEntryStatusId.TO_DO)).ToImmutableArray();
        var stepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(3).ToImmutableArray();
        var uniqueIds = _fixture.CreateMany<UniqueIdentifierId>(3).ToImmutableArray();
        var companyRoleIds = _fixture.CreateMany<CompanyRoleId>(3).ToImmutableArray();
        var agreementConsents = new List<(Guid AgreementId, ConsentStatusId ConsentStatusId)>
        {
            new ValueTuple<Guid, ConsentStatusId>(Guid.NewGuid(), ConsentStatusId.ACTIVE),
        };
        var companyData = new CompanyData("Test Company", Guid.NewGuid(), "Strabe Street", "Munich", "Germany", uniqueIds, companyRoleIds);
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(A<Guid>._, A<Guid>._, A<IEnumerable<DocumentTypeId>>._))
            .Returns(new CompanyApplicationUserEmailData(CompanyApplicationStatusId.VERIFY, true, "test@mail.de", documents, companyData, agreementConsents));

        A.CallTo(() => _checklistService.CreateInitialChecklistAsync(applicationId))
            .Returns(checklist);

        A.CallTo(() => _checklistService.GetInitialProcessStepTypeIds(A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>.That.IsSameSequenceAs(checklist)))
            .Returns(stepTypeIds);

        var utcNow = DateTimeOffset.UtcNow;

        Process? process = null;

        A.CallTo(() => _processStepRepository.CreateProcess(A<ProcessTypeId>._))
            .ReturnsLazily((ProcessTypeId processTypeId) =>
            {
                process = new Process(Guid.NewGuid(), processTypeId, Guid.NewGuid());
                return process;
            });

        CompanyApplication? application = null;

        A.CallTo(() => _applicationRepository.AttachAndModifyCompanyApplication(A<Guid>._, A<Action<CompanyApplication>>._))
            .Invokes((Guid applicationId, Action<CompanyApplication> setOptionalParameters) =>
            {
                application = new CompanyApplication(applicationId, Guid.Empty, default, default);
                setOptionalParameters(application);
            });

        IEnumerable<ProcessStep>? processSteps = null;

        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId, ProcessStepStatusId, Guid)>>._))
            .ReturnsLazily((IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> processStepTypeStatus) =>
            {
                processSteps = processStepTypeStatus.Select(x => new ProcessStep(Guid.NewGuid(), x.ProcessStepTypeId, x.ProcessStepStatusId, x.ProcessId, utcNow)).ToImmutableArray();
                return processSteps;
            });
        var settings = new RegistrationSettings
        {
            SubmitDocumentTypeIds = new[]{
                DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT
            }
        };
        var sut = new RegistrationBusinessLogic(Options.Create(settings), _mailingService, null!, null!, null!, null!, _portalRepositories, _checklistService);

        // Act
        await sut.SubmitRegistrationAsync(applicationId, _identity.UserId);

        // Assert
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, _identity.UserId, A<IEnumerable<DocumentTypeId>>.That.IsSameSequenceAs(new[] { DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT })))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _documentRepository.AttachAndModifyDocument(A<Guid>._, A<Action<Document>>._, A<Action<Document>>._)).MustHaveHappened(2, Times.Exactly);

        A.CallTo(() => _checklistService.CreateInitialChecklistAsync(applicationId))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _checklistService.GetInitialProcessStepTypeIds(A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>.That.IsSameSequenceAs(checklist)))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _processStepRepository.CreateProcess(A<ProcessTypeId>._))
            .MustHaveHappenedOnceExactly();

        process.Should().NotBeNull();
        process!.ProcessTypeId.Should().Be(ProcessTypeId.APPLICATION_CHECKLIST);

        A.CallTo(() => _applicationRepository.AttachAndModifyCompanyApplication(A<Guid>._, A<Action<CompanyApplication>>._))
            .MustHaveHappenedOnceExactly();

        application.Should().NotBeNull();
        application!.ChecklistProcessId.Should().Be(process!.Id);
        application.ApplicationStatusId.Should().Be(CompanyApplicationStatusId.SUBMITTED);

        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId, ProcessStepStatusId, Guid)>>._))
            .MustHaveHappenedOnceExactly();

        processSteps.Should().NotBeNull()
            .And.HaveCount(stepTypeIds.Length)
            .And.AllSatisfy(x =>
                {
                    x.ProcessId.Should().Be(process.Id);
                    x.ProcessStepStatusId.Should().Be(ProcessStepStatusId.TODO);
                })
            .And.Satisfy(
                x => x.ProcessStepTypeId == stepTypeIds[0],
                x => x.ProcessStepTypeId == stepTypeIds[1],
                x => x.ProcessStepTypeId == stepTypeIds[2]
            );

        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(CompanyApplicationStatusId.CREATED)]
    [InlineData(CompanyApplicationStatusId.ADD_COMPANY_DATA)]
    [InlineData(CompanyApplicationStatusId.INVITE_USER)]
    [InlineData(CompanyApplicationStatusId.SELECT_COMPANY_ROLE)]
    [InlineData(CompanyApplicationStatusId.UPLOAD_DOCUMENTS)]
    public async Task SubmitRegistrationAsync_InvalidStatus_ThrowsForbiddenException(CompanyApplicationStatusId statusId)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationId = _fixture.Create<Guid>();
        var uniqueIds = _fixture.CreateMany<UniqueIdentifierId>(3).ToImmutableArray();
        var companyRoleIds = _fixture.CreateMany<CompanyRoleId>(3).ToImmutableArray();
        var documents = new DocumentStatusData[] {
            new(Guid.NewGuid(),DocumentStatusId.PENDING),
            new(Guid.NewGuid(),DocumentStatusId.INACTIVE)
        };
        var agreementConsents = new List<(Guid AgreementId, ConsentStatusId ConsentStatusId)>
        {
            new ValueTuple<Guid, ConsentStatusId>(Guid.NewGuid(), ConsentStatusId.ACTIVE),
        };
        var settings = new RegistrationSettings
        {
            SubmitDocumentTypeIds = new[]{
                DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT
            }
        };
        var companyData = new CompanyData("Test Company", Guid.NewGuid(), "Strabe Street", "Munich", "Germany", uniqueIds, companyRoleIds);
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(A<Guid>._, A<Guid>._, A<IEnumerable<DocumentTypeId>>._))
            .Returns(new CompanyApplicationUserEmailData(statusId, true, _fixture.Create<string>(), documents, companyData, agreementConsents));
        var sut = new RegistrationBusinessLogic(Options.Create(settings), _mailingService, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRegistrationAsync(applicationId, userId)
            .ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be("Application status is not fitting to the pre-requisite");
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, userId, A<IEnumerable<DocumentTypeId>>.That.IsSameSequenceAs(new[] { DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT })))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(CompanyApplicationStatusId.SUBMITTED)]
    [InlineData(CompanyApplicationStatusId.CONFIRMED)]
    [InlineData(CompanyApplicationStatusId.DECLINED)]
    public async Task SubmitRegistrationAsync_AlreadyClosed_ThrowsForbiddenException(CompanyApplicationStatusId statusId)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationId = _fixture.Create<Guid>();
        var uniqueIds = _fixture.CreateMany<UniqueIdentifierId>(3).ToImmutableArray();
        var companyRoleIds = _fixture.CreateMany<CompanyRoleId>(3).ToImmutableArray();
        var documents = new DocumentStatusData[] {
            new(Guid.NewGuid(),DocumentStatusId.PENDING),
            new(Guid.NewGuid(),DocumentStatusId.INACTIVE)
        };
        var agreementConsents = new List<(Guid AgreementId, ConsentStatusId ConsentStatusId)>
        {
            new ValueTuple<Guid, ConsentStatusId>(Guid.NewGuid(), ConsentStatusId.ACTIVE),
        };
        var companyData = new CompanyData("Test Company", Guid.NewGuid(), "Strabe Street", "Munich", "Germany", uniqueIds, companyRoleIds);
        var settings = new RegistrationSettings
        {
            SubmitDocumentTypeIds = new[]{
                DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT
            }
        };
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(A<Guid>._, A<Guid>._, A<IEnumerable<DocumentTypeId>>._))
            .Returns(new CompanyApplicationUserEmailData(statusId, true, _fixture.Create<string>(), documents, companyData, agreementConsents));
        var sut = new RegistrationBusinessLogic(Options.Create(settings), _mailingService, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRegistrationAsync(applicationId, userId)
            .ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be("Application is already closed");
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, userId, A<IEnumerable<DocumentTypeId>>.That.IsSameSequenceAs(new[] { DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SubmitRegistrationAsync_WithNotExistingCompanyUser_ThrowsForbiddenException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationId = _fixture.Create<Guid>();
        var uniqueIds = _fixture.CreateMany<UniqueIdentifierId>(3).ToImmutableArray();
        var companyRoleIds = _fixture.CreateMany<CompanyRoleId>(3).ToImmutableArray();
        var agreementConsents = new List<(Guid AgreementId, ConsentStatusId ConsentStatusId)>
        {
            new ValueTuple<Guid, ConsentStatusId>(Guid.NewGuid(), ConsentStatusId.ACTIVE),
        };
        var companyData = new CompanyData("Test Company", Guid.NewGuid(), "Strabe Street", "Munich", "Germany", uniqueIds, companyRoleIds);
        var settings = new RegistrationSettings
        {
            SubmitDocumentTypeIds = new[]{
                DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT
            }
        };
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(A<Guid>._, A<Guid>._, A<IEnumerable<DocumentTypeId>>._))
            .Returns(new CompanyApplicationUserEmailData(CompanyApplicationStatusId.VERIFY, false, null, null!, companyData, agreementConsents));
        var sut = new RegistrationBusinessLogic(Options.Create(settings), _mailingService, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRegistrationAsync(applicationId, userId)
            .ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"userId {userId} is not associated with CompanyApplication {applicationId}");
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, userId, A<IEnumerable<DocumentTypeId>>.That.IsSameSequenceAs(new[] { DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SubmitRegistrationAsync_WithNotExistingStreetName_ThrowsConflictException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationId = _fixture.Create<Guid>();
        var uniqueIds = _fixture.CreateMany<UniqueIdentifierId>(3).ToImmutableArray();
        var companyRoleIds = _fixture.CreateMany<CompanyRoleId>(3).ToImmutableArray();
        var agreementConsents = new List<(Guid AgreementId, ConsentStatusId ConsentStatusId)>
        {
            new ValueTuple<Guid, ConsentStatusId>(Guid.NewGuid(), ConsentStatusId.ACTIVE),
        };
        var companyData = new CompanyData("Test Company", Guid.NewGuid(), string.Empty, "Munich", "Germany", uniqueIds, companyRoleIds);
        var settings = new RegistrationSettings
        {
            SubmitDocumentTypeIds = new[]{
                DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT
            }
        };
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, userId, A<IEnumerable<DocumentTypeId>>._))
            .Returns(new CompanyApplicationUserEmailData(CompanyApplicationStatusId.VERIFY, true, _fixture.Create<string>(), Enumerable.Empty<DocumentStatusData>(), companyData, agreementConsents));
        var sut = new RegistrationBusinessLogic(Options.Create(settings), _mailingService, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRegistrationAsync(applicationId, userId)
            .ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Street Name must not be empty");
    }

    [Fact]
    public async Task SubmitRegistrationAsync_WithNotExistingAddressId_ThrowsConflictException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationId = _fixture.Create<Guid>();
        var uniqueIds = _fixture.CreateMany<UniqueIdentifierId>(3).ToImmutableArray();
        var companyRoleIds = _fixture.CreateMany<CompanyRoleId>(3).ToImmutableArray();
        var agreementConsents = new List<(Guid AgreementId, ConsentStatusId ConsentStatusId)>
        {
            new ValueTuple<Guid, ConsentStatusId>(Guid.NewGuid(), ConsentStatusId.ACTIVE),
        };
        var companyData = new CompanyData("Test Company", null, "Strabe Street", "Munich", "Germany", uniqueIds, companyRoleIds);
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, userId, A<IEnumerable<DocumentTypeId>>._))
            .ReturnsLazily(() => new CompanyApplicationUserEmailData(CompanyApplicationStatusId.VERIFY, true, _fixture.Create<string>(), Enumerable.Empty<DocumentStatusData>(), companyData, agreementConsents));
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), _mailingService, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRegistrationAsync(applicationId, userId)
            .ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Address must not be empty");
    }

    [Fact]
    public async Task SubmitRegistrationAsync_WithNotExistingCompanyName_ThrowsConflictException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationId = _fixture.Create<Guid>();
        var uniqueIds = _fixture.CreateMany<UniqueIdentifierId>(3).ToImmutableArray();
        var companyRoleIds = _fixture.CreateMany<CompanyRoleId>(3).ToImmutableArray();
        var agreementConsents = new List<(Guid AgreementId, ConsentStatusId ConsentStatusId)>
        {
            new ValueTuple<Guid, ConsentStatusId>(Guid.NewGuid(), ConsentStatusId.ACTIVE),
        };
        var companyData = new CompanyData(string.Empty, Guid.NewGuid(), "Strabe Street", "Munich", "Germany", uniqueIds, companyRoleIds);
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, userId, A<IEnumerable<DocumentTypeId>>._))
            .Returns(new CompanyApplicationUserEmailData(CompanyApplicationStatusId.VERIFY, true, _fixture.Create<string>(), Enumerable.Empty<DocumentStatusData>(), companyData, agreementConsents));
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), _mailingService, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRegistrationAsync(applicationId, userId)
            .ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Company Name must not be empty");
    }

    [Fact]
    public async Task SubmitRegistrationAsync_WithNotExistingUniqueId_ThrowsConflictException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationId = _fixture.Create<Guid>();
        var companyRoleIds = _fixture.CreateMany<CompanyRoleId>(3).ToImmutableArray();
        var agreementConsents = new List<(Guid AgreementId, ConsentStatusId ConsentStatusId)>
         {
             new ValueTuple<Guid, ConsentStatusId>(Guid.NewGuid(), ConsentStatusId.ACTIVE),
         };
        var uniqueIdentifierData = Enumerable.Empty<UniqueIdentifierId>();
        var companyData = new CompanyData("Test Company", Guid.NewGuid(), "Strabe Street", "Munich", "Germany", uniqueIdentifierData, companyRoleIds);
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, userId, A<IEnumerable<DocumentTypeId>>._))
            .ReturnsLazily(() => new CompanyApplicationUserEmailData(CompanyApplicationStatusId.VERIFY, true, _fixture.Create<string>(), Enumerable.Empty<DocumentStatusData>(), companyData, agreementConsents));
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), _mailingService, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRegistrationAsync(applicationId, userId)
            .ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Company Identifiers [{string.Join(", ", uniqueIdentifierData)}] must not be empty");
    }

    [Fact]
    public async Task SubmitRegistrationAsync_WithNotExistingCompanyRoleId_ThrowsConflictException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationId = _fixture.Create<Guid>();
        var uniqueIds = _fixture.CreateMany<UniqueIdentifierId>(3).ToImmutableArray();
        var agreementConsents = new List<(Guid AgreementId, ConsentStatusId ConsentStatusId)>
         {
             new ValueTuple<Guid, ConsentStatusId>(Guid.NewGuid(), ConsentStatusId.ACTIVE),
         };
        var companyRoleIdData = Enumerable.Empty<CompanyRoleId>();
        var companyData = new CompanyData("Test Company", Guid.NewGuid(), "Strabe Street", "Munich", "Germany", uniqueIds, companyRoleIdData);
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, userId, A<IEnumerable<DocumentTypeId>>._))
            .ReturnsLazily(() => new CompanyApplicationUserEmailData(CompanyApplicationStatusId.VERIFY, true, _fixture.Create<string>(), Enumerable.Empty<DocumentStatusData>(), companyData, agreementConsents));
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), _mailingService, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRegistrationAsync(applicationId, userId)
            .ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Company assigned role [{string.Join(", ", companyRoleIdData)}] must not be empty");
    }

    [Fact]
    public async Task SubmitRegistrationAsync_WithNotExistingAgreementandConsent_ThrowsConflictException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationId = _fixture.Create<Guid>();
        var uniqueIds = _fixture.CreateMany<UniqueIdentifierId>(3).ToImmutableArray();
        var companyRoleIds = _fixture.CreateMany<CompanyRoleId>(3).ToImmutableArray();
        var agreementConsents = Enumerable.Empty<(Guid, ConsentStatusId)>();
        var companyData = new CompanyData("Test Company", Guid.NewGuid(), "Strabe Street", "Munich", "Germany", uniqueIds, companyRoleIds);
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, userId, A<IEnumerable<DocumentTypeId>>._))
            .ReturnsLazily(() => new CompanyApplicationUserEmailData(CompanyApplicationStatusId.VERIFY, true, _fixture.Create<string>(), Enumerable.Empty<DocumentStatusData>(), companyData, agreementConsents));
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), _mailingService, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRegistrationAsync(applicationId, userId)
            .ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Agreement and Consent must not be empty");
    }

    [Fact]
    public async Task SubmitRegistrationAsync_WithNotExistingCity_ThrowsConflictException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationId = _fixture.Create<Guid>();
        var uniqueIds = _fixture.CreateMany<UniqueIdentifierId>(3).ToImmutableArray();
        var companyRoleIds = _fixture.CreateMany<CompanyRoleId>(3).ToImmutableArray();
        var agreementConsents = new List<(Guid AgreementId, ConsentStatusId ConsentStatusId)>
        {
            new ValueTuple<Guid, ConsentStatusId>(Guid.NewGuid(), ConsentStatusId.ACTIVE),
        };
        var companyData = new CompanyData("Test Company", Guid.NewGuid(), "Strabe Street", string.Empty, "Germany", uniqueIds, companyRoleIds);
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, userId, A<IEnumerable<DocumentTypeId>>._))
            .ReturnsLazily(() => new CompanyApplicationUserEmailData(CompanyApplicationStatusId.VERIFY, true, _fixture.Create<string>(), Enumerable.Empty<DocumentStatusData>(), companyData, agreementConsents));
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), _mailingService, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRegistrationAsync(applicationId, userId)
            .ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("City must not be empty");
    }

    [Fact]
    public async Task SubmitRegistrationAsync_WithNotExistingCountry_ThrowsConflictException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationId = _fixture.Create<Guid>();
        var uniqueIds = _fixture.CreateMany<UniqueIdentifierId>(3).ToImmutableArray();
        var companyRoleIds = _fixture.CreateMany<CompanyRoleId>(3).ToImmutableArray();
        var agreementConsents = new List<(Guid AgreementId, ConsentStatusId ConsentStatusId)>
         {
             new ValueTuple<Guid, ConsentStatusId>(Guid.NewGuid(), ConsentStatusId.ACTIVE),
         };

        var companyData = new CompanyData("Test Company", Guid.NewGuid(), "Strabe Street", "Munich", string.Empty, uniqueIds, companyRoleIds);
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, userId, A<IEnumerable<DocumentTypeId>>._))
            .ReturnsLazily(() => new CompanyApplicationUserEmailData(CompanyApplicationStatusId.VERIFY, true, _fixture.Create<string>(), Enumerable.Empty<DocumentStatusData>(), companyData, agreementConsents));
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), _mailingService, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRegistrationAsync(applicationId, userId)
            .ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Country must not be empty");
    }

    [Fact]
    public async Task SubmitRegistrationAsync_WithUserEmail_SendsMail()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var uniqueIds = _fixture.CreateMany<UniqueIdentifierId>(3).ToImmutableArray();
        var companyRoleIds = _fixture.CreateMany<CompanyRoleId>(3).ToImmutableArray();
        var agreementConsents = new List<(Guid AgreementId, ConsentStatusId ConsentStatusId)>
        {
            new ValueTuple<Guid, ConsentStatusId>(Guid.NewGuid(), ConsentStatusId.ACTIVE),
        };
        IEnumerable<DocumentStatusData> documents = new DocumentStatusData[]{
            new(
                Guid.NewGuid(),DocumentStatusId.INACTIVE
            )};
        var companyData = new CompanyData("Test Company", Guid.NewGuid(), "Strabe Street", "Munich", "Germany", uniqueIds, companyRoleIds);
        var settings = new RegistrationSettings
        {
            SubmitDocumentTypeIds = new[]{
                DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT
            }
        };
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(A<Guid>._, A<Guid>._, A<IEnumerable<DocumentTypeId>>._))
            .Returns(new CompanyApplicationUserEmailData(CompanyApplicationStatusId.VERIFY, true, "test@mail.de", documents, companyData, agreementConsents));
        var sut = new RegistrationBusinessLogic(Options.Create(settings), _mailingService, null!, null!, null!, null!, _portalRepositories, _checklistService);

        // Act
        var result = await sut.SubmitRegistrationAsync(applicationId, _identity.UserId)
            .ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, _identity.UserId, A<IEnumerable<DocumentTypeId>>.That.IsSameSequenceAs(new[] { DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT })))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _checklistService.CreateInitialChecklistAsync(applicationId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>._)).MustHaveHappened();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SubmitRegistrationAsync_WithoutUserEmail_DoesntSendMail()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var uniqueIds = _fixture.CreateMany<UniqueIdentifierId>(3).ToImmutableArray();
        var companyRoleIds = _fixture.CreateMany<CompanyRoleId>(3).ToImmutableArray();
        var agreementConsents = new List<(Guid AgreementId, ConsentStatusId ConsentStatusId)>
        {
            new ValueTuple<Guid, ConsentStatusId>(Guid.NewGuid(), ConsentStatusId.ACTIVE),
        };
        IEnumerable<DocumentStatusData> documents = new DocumentStatusData[]{
            new(
                Guid.NewGuid(),DocumentStatusId.PENDING
            )};
        var companyData = new CompanyData("Test Company", Guid.NewGuid(), "Strabe Street", "Munich", "Germany", uniqueIds, companyRoleIds);
        var settings = new RegistrationSettings
        {
            SubmitDocumentTypeIds = new[]{
                DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT
            }
        };
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(A<Guid>._, A<Guid>._, A<IEnumerable<DocumentTypeId>>._))
            .Returns(new CompanyApplicationUserEmailData(CompanyApplicationStatusId.VERIFY, true, null, documents, companyData, agreementConsents));
        var sut = new RegistrationBusinessLogic(Options.Create(settings), _mailingService, null!, null!, null!, A.Fake<ILogger<RegistrationBusinessLogic>>(), _portalRepositories, _checklistService);

        // Act
        var result = await sut.SubmitRegistrationAsync(applicationId, _identity.UserId)
            .ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, _identity.UserId, A<IEnumerable<DocumentTypeId>>.That.IsSameSequenceAs(new[] { DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT })))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _checklistService.CreateInitialChecklistAsync(applicationId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>._)).MustNotHaveHappened();
        result.Should().BeTrue();
    }

    #endregion

    #region GetCompanyIdentifiers

    [Fact]
    public async Task GetCompanyIdentifiers_ReturnsExpectedOutput()
    {
        // Arrange
        var uniqueIdentifierData = _fixture.CreateMany<UniqueIdentifierId>();

        A.CallTo(() => _staticDataRepository.GetCompanyIdentifiers(A<string>._))
            .Returns((uniqueIdentifierData, true));

        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);
        // Act
        var result = await sut.GetCompanyIdentifiers(_fixture.Create<string>()).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        foreach (var item in result)
        {
            A.CallTo(() => _staticDataRepository.GetCompanyIdentifiers(A<string>._)).MustHaveHappenedOnceExactly();
            Assert.NotNull(item);
            Assert.IsType<UniqueIdentifierData?>(item);
        }
    }

    [Fact]
    public async Task GetCompanyIdentifiers_InvalidCountry_Throws()
    {
        // Arrange
        A.CallTo(() => _staticDataRepository.GetCompanyIdentifiers(A<string>._))
            .Returns(((IEnumerable<UniqueIdentifierId> IdentifierIds, bool IsValidCountry))default);

        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        var countryCode = _fixture.Create<string>();

        // Act
        var Act = () => sut.GetCompanyIdentifiers(countryCode);

        // Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"invalid country code {countryCode}");
    }

    #endregion

    #region GetRegistrationDataAsync

    [Fact]
    public async Task GetRegistrationDataAsync_ReturnsExpected()
    {
        // Arrange
        var data = _fixture.Create<RegistrationData>();
        A.CallTo(() => _applicationRepository.GetRegistrationDataUntrackedAsync(_existingApplicationId, _identity.CompanyId, A<IEnumerable<DocumentTypeId>>._))
            .Returns((true, true, data));

        var sut = new RegistrationBusinessLogic(_options, null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        var result = await sut.GetRegistrationDataAsync(_existingApplicationId, _identity.CompanyId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<CompanyRegistrationData>();
        result.Should().Match<CompanyRegistrationData>(x =>
            x.CompanyId == data.CompanyId &&
            x.Name == data.Name &&
            x.BusinessPartnerNumber == data.BusinessPartnerNumber &&
            x.ShortName == data.ShortName &&
            x.City == data.City &&
            x.Region == data.Region &&
            x.StreetAdditional == data.StreetAdditional &&
            x.StreetName == data.StreetName &&
            x.StreetNumber == data.StreetNumber &&
            x.ZipCode == data.ZipCode &&
            x.CountryAlpha2Code == data.CountryAlpha2Code &&
            x.CountryDe == data.CountryDe);
        result.CompanyRoleIds.Should().HaveSameCount(data.CompanyRoleIds);
        result.CompanyRoleIds.Should().ContainInOrder(data.CompanyRoleIds);
        result.AgreementConsentStatuses.Should().HaveSameCount(data.AgreementConsentStatuses);
        result.AgreementConsentStatuses.Zip(data.AgreementConsentStatuses).Should().AllSatisfy(x =>
            x.Should().Match<(AgreementConsentStatusForRegistrationData First, (Guid AgreementId, ConsentStatusId ConsentStatusId) Second)>(x =>
                x.First.AgreementId == x.Second.AgreementId && x.First.ConsentStatusId == x.Second.ConsentStatusId));
        result.Documents.Should().HaveSameCount(data.DocumentNames);
        result.Documents.Zip(data.DocumentNames).Should().AllSatisfy(x =>
            x.Should().Match<(RegistrationDocumentNames First, string Second)>(x =>
                x.First.DocumentName == x.Second));
        result.UniqueIds.Should().HaveSameCount(data.Identifiers);
        result.UniqueIds.Zip(data.Identifiers).Should().AllSatisfy(x =>
            x.Should().Match<(CompanyUniqueIdData First, (UniqueIdentifierId UniqueIdentifierId, string Value) Second)>(x =>
                x.First.UniqueIdentifierId == x.Second.UniqueIdentifierId && x.First.Value == x.Second.Value));
    }

    [Fact]
    public async Task GetRegistrationDataAsync_WithInvalidApplicationId_Throws()
    {
        // Arrange
        var data = _fixture.Create<RegistrationData>();
        var applicationId = Guid.NewGuid();
        A.CallTo(() => _applicationRepository.GetRegistrationDataUntrackedAsync(A<Guid>._, _identity.CompanyId, A<IEnumerable<DocumentTypeId>>._))
            .Returns((false, false, data));

        var sut = new RegistrationBusinessLogic(_options, null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        var Act = () => sut.GetRegistrationDataAsync(applicationId, _identity.CompanyId);

        // Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"application {applicationId} does not exist");
    }

    [Fact]
    public async Task GetRegistrationDataAsync_WithInvalidUser_Throws()
    {
        // Arrange
        var data = _fixture.Create<RegistrationData>();
        var applicationId = Guid.NewGuid();
        A.CallTo(() => _applicationRepository.GetRegistrationDataUntrackedAsync(A<Guid>._, _identity.CompanyId, A<IEnumerable<DocumentTypeId>>._))
            .Returns((true, false, data));

        var sut = new RegistrationBusinessLogic(_options, null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        var Act = () => sut.GetRegistrationDataAsync(applicationId, _identity.CompanyId);

        // Assert
        var result = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"The users company is not assigned with CompanyApplication {applicationId}");
    }

    [Fact]
    public async Task GetRegistrationDataAsync_WithNullData_Throws()
    {
        var applicationId = Guid.NewGuid();

        // Arrange
        A.CallTo(() => _applicationRepository.GetRegistrationDataUntrackedAsync(A<Guid>._, _identity.CompanyId, A<IEnumerable<DocumentTypeId>>._))
            .Returns((true, true, null));

        var sut = new RegistrationBusinessLogic(_options, null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        var Act = () => sut.GetRegistrationDataAsync(applicationId, _identity.CompanyId);

        // Assert
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"registrationData should never be null for application {applicationId}");
    }

    #endregion

    [Fact]
    public async Task GetRegistrationDocumentAsync_ReturnsExpectedResult()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var content = new byte[7];
        A.CallTo(() => _documentRepository.GetDocumentAsync(documentId, A<IEnumerable<DocumentTypeId>>._))
            .ReturnsLazily(() => new ValueTuple<byte[], string, bool, MediaTypeId>(content, "test.json", true, MediaTypeId.JSON));
        var sut = new RegistrationBusinessLogic(_options, null!, null!, null!, null!, null!, _portalRepositories, null!);

        //Act
        var result = await sut.GetRegistrationDocumentAsync(documentId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _documentRepository.GetDocumentAsync(documentId, A<IEnumerable<DocumentTypeId>>._)).MustHaveHappenedOnceExactly();
        result.Should().NotBeNull();
        result.fileName.Should().Be("test.json");
    }

    [Fact]
    public async Task GetRegistrationDocumentAsync_WithInvalidDocumentTypeId_ThrowsNotFoundException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var content = new byte[7];
        A.CallTo(() => _documentRepository.GetDocumentAsync(documentId, A<IEnumerable<DocumentTypeId>>._))
            .ReturnsLazily(() => new ValueTuple<byte[], string, bool, MediaTypeId>(content, "test.json", false, MediaTypeId.JSON));
        var sut = new RegistrationBusinessLogic(_options, null!, null!, null!, null!, null!, _portalRepositories, null!);

        //Act
        var Act = () => sut.GetRegistrationDocumentAsync(documentId);

        // Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"document {documentId} does not exist.");
    }

    [Fact]
    public async Task GetRegistrationDocumentAsync_WithInvalidDocumentId_ThrowsNotFoundException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var content = new byte[7];
        A.CallTo(() => _documentRepository.GetDocumentAsync(documentId, A<IEnumerable<DocumentTypeId>>._))
            .ReturnsLazily(() => new ValueTuple<byte[], string, bool, MediaTypeId>());
        var sut = new RegistrationBusinessLogic(_options, null!, null!, null!, null!, null!, _portalRepositories, null!);

        //Act
        var Act = () => sut.GetRegistrationDocumentAsync(documentId);

        // Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"document {documentId} does not exist.");
    }

    #region GetDocumentAsync

    [Fact]
    public async Task GetDocumentAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var content = new byte[7];
        A.CallTo(() => _documentRepository.GetDocumentIdWithCompanyUserCheckAsync(documentId, _identity.UserId))
            .ReturnsLazily(() => new ValueTuple<Guid, bool>(documentId, true));
        A.CallTo(() => _documentRepository.GetDocumentByIdAsync(documentId))
            .ReturnsLazily(() => new Document(documentId, content, content, "test.pdf", MediaTypeId.PDF, DateTimeOffset.UtcNow, DocumentStatusId.LOCKED, DocumentTypeId.APP_CONTRACT));
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        var result = await sut.GetDocumentContentAsync(documentId, _identity.UserId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be("test.pdf");
        result.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task GetDocumentAsync_WithoutDocument_ThrowsNotFoundException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var content = new byte[7];
        A.CallTo(() => _documentRepository.GetDocumentIdWithCompanyUserCheckAsync(documentId, _identity.UserId))
            .ReturnsLazily(() => new ValueTuple<Guid, bool>(Guid.Empty, false));
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.GetDocumentContentAsync(documentId, _identity.UserId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"document {documentId} does not exist.");
    }

    [Fact]
    public async Task GetDocumentAsync_WithWrongUser_ThrowsForbiddenException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        A.CallTo(() => _documentRepository.GetDocumentIdWithCompanyUserCheckAsync(documentId, _identity.UserId))
            .ReturnsLazily(() => new ValueTuple<Guid, bool>(documentId, false));
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.GetDocumentContentAsync(documentId, _identity.UserId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"The user is not permitted to access document {documentId}.");
    }

    #endregion

    #region Setup  

    private void SetupRepositories()
    {
        var invitedUserRole = _fixture.CreateMany<string>(2).AsEnumerable();
        var invitedUser = _fixture.CreateMany<InvitedUserDetail>(1).ToAsyncEnumerable();

        A.CallTo(() => _invitationRepository.GetInvitedUserDetailsUntrackedAsync(_existingApplicationId))
            .Returns(invitedUser);
        A.CallTo(() => _invitationRepository.GetInvitedUserDetailsUntrackedAsync(Guid.Empty)).Throws(new Exception());

        A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(A<string>._, A<string>._))
            .Returns(invitedUserRole);
        A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(string.Empty, string.Empty)).Throws(new Exception());

        A.CallTo(() => _applicationRepository.IsValidApplicationForCompany(_existingApplicationId, _identity.CompanyId))
            .Returns(true);
        A.CallTo(() => _applicationRepository.IsValidApplicationForCompany(_existingApplicationId, A<Guid>.That.Not.Matches(x => x == _identity.CompanyId)))
            .Returns(false);
        A.CallTo(() => _applicationRepository.IsValidApplicationForCompany(
                A<Guid>.That.Not.Matches(x => x == _existingApplicationId), _identity.CompanyId))
            .Returns(false);
        A.CallTo(() => _applicationRepository.IsValidApplicationForCompany(
                A<Guid>.That.Not.Matches(x => x == _existingApplicationId), A<Guid>.That.Not.Matches(x => x == _identity.CompanyId)))
            .Returns(false);

        A.CallTo(() => _countryRepository.GetCountryAssignedIdentifiers(A<string>._, A<IEnumerable<UniqueIdentifierId>>._))
            .ReturnsLazily((string alpha2Code, IEnumerable<UniqueIdentifierId> identifiers) => (true, identifiers));

        A.CallTo(() => _companyRepository.CreateAddress(A<string>._, A<string>._, A<string>._, A<Action<Address>>._))
            .ReturnsLazily((string city, string streetName, string alpha2Code, Action<Address>? setParameters) =>
            {
                var address = new Address(Guid.NewGuid(), city, streetName, alpha2Code, _fixture.Create<DateTimeOffset>());
                setParameters?.Invoke(address);
                return address;
            });

        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>())
            .Returns(_documentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IInvitationRepository>())
            .Returns(_invitationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>())
            .Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRolesRepository>())
            .Returns(_companyRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>())
            .Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICountryRepository>())
            .Returns(_countryRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentRepository>())
            .Returns(_consentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IStaticDataRepository>())
            .Returns(_staticDataRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>())
            .Returns(_processStepRepository);
    }

    private void SetupFakesForInvitation()
    {
        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._, A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._, A<CancellationToken>._))
            .ReturnsLazily((CompanyNameIdpAliasData companyNameIdpAliasData, IAsyncEnumerable<UserCreationRoleDataIdpInfo> userCreationInfos, CancellationToken cancellationToken) =>
                userCreationInfos.Select(userCreationInfo => _processLine(userCreationInfo)));

        A.CallTo(() => _userProvisioningService.GetRoleDatas(A<IDictionary<string, IEnumerable<string>>>._))
            .ReturnsLazily((IDictionary<string, IEnumerable<string>> clientRoles) =>
                clientRoles.SelectMany(r => r.Value.Select(role => _fixture.Build<UserRoleData>().With(x => x.UserRoleText, role).Create())).ToAsyncEnumerable());

        A.CallTo(() => _userProvisioningService.GetIdentityProviderDisplayName(A<string>._)).Returns(_displayName);

        A.CallTo(() => _userProvisioningService.GetCompanyNameSharedIdpAliasData(A<Guid>._, A<Guid?>._)).Returns(
            (
                _fixture.Create<CompanyNameIdpAliasData>(),
                _fixture.Create<string>()
            ));

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>._)).ReturnsLazily(
            (UserCreationRoleDataIdpInfo creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
                .With(x => x.UserName, creationInfo.UserName)
                .With(x => x.Error, (Exception?)null)
                .Create());
    }

    #endregion

    [Serializable]
    public class TestException : Exception
    {
        public TestException() { }
        public TestException(string message) : base(message) { }
        public TestException(string message, Exception inner) : base(message, inner) { }
        protected TestException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
