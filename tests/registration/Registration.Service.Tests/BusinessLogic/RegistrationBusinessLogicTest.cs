/********************************************************************************
 * Copyright (c) 2021,2022 Microsoft and BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
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
    private static readonly Guid IdWithoutBpn = new ("0a9bd7b1-e692-483e-8128-dbf52759c7a5");
    private static readonly Guid IdWithBpn = new ("c244f79a-7faf-4c59-bb85-fbfdf72ce46f");
    private static readonly Guid IdWithStateCreated = new ("148c0a07-2e1f-4dce-bfe0-4e3d1825c266");
    private static readonly Guid IdWithChecklistEntryInProgress = new ("9b288a8d-1d2f-4b86-be97-da40420dc8e4");
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
    private readonly IChecklistCreationService _checklistService;
    private readonly string _iamUserId;
    private readonly Guid _companyUserId;
    private readonly Guid _existingApplicationId;
    private readonly string _displayName;
    private readonly string _alpha2code;
    private readonly TestException _error;
    private readonly IOptions<RegistrationSettings> _options;
    private readonly IMailingService _mailingService;
    private readonly IStaticDataRepository _staticDataRepository;
    private readonly Func<UserCreationRoleDataIdpInfo,(Guid CompanyUserId, string UserName, string? Password, Exception? Error)> _processLine;

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
        _checklistService = A.Fake<IChecklistCreationService>();
        _staticDataRepository = A.Fake<IStaticDataRepository>();

        var options = Options.Create(new RegistrationSettings
        {
            BasePortalAddress = "just a test",
            KeyCloakClientID = "CatenaX"
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
        _error = _fixture.Create<TestException>();

        _processLine = A.Fake<Func<UserCreationRoleDataIdpInfo,(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>>();

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
            .With(x => x.Names, new [] { _fixture.Build<Bpn.Model.BpdmNameDto>()
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
                .With(x => x.AdministrativeAreas, new [] { _fixture.Build<Bpn.Model.BpdmAdministrativeAreaDto>().With(x => x.Value, region).Create() })
                .With(x => x.PostCodes, new [] { _fixture.Build<Bpn.Model.BpdmPostCodeDto>().With(x => x.Value, zipCode).Create() })
                .With(x => x.Localities, new [] { _fixture.Build<Bpn.Model.BpdmLocalityDto>().With(x => x.Value, city).Create() })
                .With(x => x.Thoroughfares, new [] { _fixture.Build<Bpn.Model.BpdmThoroughfareDto>().With(x => x.Value, streetName).With(x => x.Number, streetNumber).Create()})
                .Create())
            .Create();                    
        A.CallTo(() => bpnAccess.FetchLegalEntityByBpn(businessPartnerNumber, token, A<CancellationToken>._))
            .Returns(legalEntity);
        A.CallTo(() => bpnAccess.FetchLegalEntityAddressByBpn(businessPartnerNumber, token, A<CancellationToken>._))
            .Returns(new [] { bpdmAddress }.ToAsyncEnumerable());
        A.CallTo(() => _staticDataRepository.GetCountryAssignedIdentifiers(A<IEnumerable<BpdmIdentifierId>>.That.Matches<IEnumerable<BpdmIdentifierId>>(ids => ids.SequenceEqual(uniqueIdSeed.Select(seed => seed.BpdmIdentifierId))), country))
            .Returns(validIdentifiers.ToAsyncEnumerable());

        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            bpnAccess,
            null!,
            null!,
            null!,
            _portalRepositories);

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
        var resultList = new List<CompanyApplicationWithStatus>
        {
            new()
            {
                ApplicationId = _fixture.Create<Guid>(),
                ApplicationStatus = CompanyApplicationStatusId.VERIFY
            }
        };
        A.CallTo(() => _userRepository.GetApplicationsWithStatusUntrackedAsync(userId))
            .Returns(resultList.ToAsyncEnumerable());

        // Act
        var result = await sut.GetAllApplicationsForUserWithStatus(userId).ToListAsync().ConfigureAwait(false);
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
        var userId = _fixture.Create<string>();
        var data = _fixture.Create<CompanyApplicationDetailData>();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);
        
        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, userId, null))
            .Returns(data);

        // Act
        var result = await sut.GetCompanyDetailData(applicationId, userId).ConfigureAwait(false);
        
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
        
        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, userId, null))
            .Returns((CompanyApplicationDetailData?)null);

        // Act
        async Task Act() => await sut.GetCompanyDetailData(applicationId, userId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"CompanyApplication {applicationId} not found");
    }

    [Fact]
    public async Task GetCompanyWithAddressAsync_WithInvalidUser_ThrowsForbiddenException()
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
        
        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, userId, null))
            .Returns(_fixture.Build<CompanyApplicationDetailData>().With(x => x.CompanyUserId, Guid.Empty).Create());

        // Act
        async Task Act() => await sut.GetCompanyDetailData(applicationId, userId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"iamUserId {userId} is not assigned with CompanyApplication {applicationId}");
    }

    #endregion
    
    #region SetCompanyWithAddress
    
    [Theory]
    [InlineData(null, null, null, null, new UniqueIdentifierId[]{}, new string[]{}, "Name")]
    [InlineData("filled", null, null, null, new UniqueIdentifierId[]{}, new string[]{}, "City")]
    [InlineData("filled", "filled", null, null, new UniqueIdentifierId[]{}, new string[]{}, "StreetName")]
    [InlineData("filled", "filled", "filled", "", new UniqueIdentifierId[]{}, new string[]{}, "CountryAlpha2Code")]
    [InlineData("filled", "filled", "filled", "XX", new UniqueIdentifierId[]{ UniqueIdentifierId.VAT_ID, UniqueIdentifierId.LEI_CODE }, new string[]{ "filled", "" }, "UniqueIds")]
    [InlineData("filled", "filled", "filled", "XX", new UniqueIdentifierId[]{ UniqueIdentifierId.VAT_ID, UniqueIdentifierId.VAT_ID }, new string[]{ "filled", "filled" }, "UniqueIds")]
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
        async Task Act() => await sut.SetCompanyDetailDataAsync(Guid.NewGuid(), companyData, string.Empty).ConfigureAwait(false);

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

        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, A<string>._, companyId))
            .ReturnsLazily(() => (CompanyApplicationDetailData?)null);
        
        // Act
        async Task Act() => await sut.SetCompanyDetailDataAsync(applicationId, companyData, string.Empty).ConfigureAwait(false);

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

        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, A<string>._, companyId))
            .ReturnsLazily(() => _fixture.Build<CompanyApplicationDetailData>().With(x => x.CompanyUserId, Guid.Empty).Create());
        
        // Act
        async Task Act() => await sut.SetCompanyDetailDataAsync(applicationId, companyData, string.Empty).ConfigureAwait(false);

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

        var companyData = _fixture.Build<CompanyDetailData>()
            .With(x => x.CompanyId, companyId)
            .With(x => x.CountryAlpha2Code, _alpha2code)
            .Create();

        var existingData = _fixture.Build<CompanyApplicationDetailData>()
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

        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, A<string>._, companyId))
            .Returns(existingData);

        A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, A<Action<Company>>._, A<Action<Company>>._))
            .Invokes((Guid companyId, Action<Company>? initialize, Action<Company> modify) =>
            {
                company = new Company(companyId, null!, default, default);
                initialize?.Invoke(company);
                modify(company);
            });

        // Act
        await sut.SetCompanyDetailDataAsync(applicationId, companyData, string.Empty).ConfigureAwait(false);

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

        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, A<string>._, companyId))
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
        await sut.SetCompanyDetailDataAsync(applicationId, companyData, string.Empty).ConfigureAwait(false);

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

        var companyData = _fixture.Build<CompanyDetailData>()
            .With(x => x.CompanyId, companyId)
            .With(x => x.CountryAlpha2Code, _alpha2code)
            .Create();

        var existingData = _fixture.Build<CompanyApplicationDetailData>()
            .With(x => x.CompanyId, companyId)
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

        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, A<string>._, companyData.CompanyId))
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
        await sut.SetCompanyDetailDataAsync(applicationId, companyData, string.Empty).ConfigureAwait(false);

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

        var uniqueIdentifiers = _fixture.CreateMany<UniqueIdentifierId>(4);

        var firstIdData = _fixture.Build<CompanyUniqueIdData>().With(x => x.UniqueIdentifierId, uniqueIdentifiers.First()).Create();       // shall not modify
        var secondIdData = _fixture.Build<CompanyUniqueIdData>().With(x => x.UniqueIdentifierId, uniqueIdentifiers.ElementAt(1)).Create(); // shall modify
        var thirdIdData = _fixture.Build<CompanyUniqueIdData>().With(x => x.UniqueIdentifierId, uniqueIdentifiers.ElementAt(2)).Create();  // shall create new

        var companyData = _fixture.Build<CompanyDetailData>()
            .With(x => x.CompanyId, companyId)
            .With(x => x.CountryAlpha2Code, _alpha2code)
            .With(x => x.UniqueIds, new [] { firstIdData, secondIdData, thirdIdData })
            .Create();

        var existingData = _fixture.Build<CompanyApplicationDetailData>()
            .With(x => x.UniqueIds, new [] {
                (firstIdData.UniqueIdentifierId, firstIdData.Value),            // shall be left unmodified
                (secondIdData.UniqueIdentifierId, _fixture.Create<string>()),   // shall be modified
                (uniqueIdentifiers.ElementAt(3), _fixture.Create<string>()) })  // shall be deleted
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

        A.CallTo(() => _applicationRepository.GetCompanyApplicationDetailDataAsync(applicationId, A<string>._, companyId))
            .Returns(existingData);

        A.CallTo(() => _companyRepository.CreateUpdateDeleteIdentifiers(A<Guid>._, A<IEnumerable<(UniqueIdentifierId,string)>>._, A<IEnumerable<(UniqueIdentifierId,string)>>._))
            .Invokes((Guid companyId, IEnumerable<(UniqueIdentifierId UniqueIdentifierId, string Value)> initial, IEnumerable<(UniqueIdentifierId UniqueIdentifierId, string Value)> modified) => {
                initialIdentifiers = initial;
                modifiedIdentifiers = modified;
            });

        // Act
        await sut.SetCompanyDetailDataAsync(applicationId, companyData, string.Empty).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companyRepository.CreateUpdateDeleteIdentifiers(companyId, A<IEnumerable<(UniqueIdentifierId,string)>>._, A<IEnumerable<(UniqueIdentifierId,string)>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRepository.CreateUpdateDeleteIdentifiers(A<Guid>.That.Not.IsEqualTo(companyId), A<IEnumerable<(UniqueIdentifierId,string)>>._, A<IEnumerable<(UniqueIdentifierId,string)>>._)).MustNotHaveHappened();
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
            .Returns((false,null!));

        // Act
        var Act = () => sut.SetCompanyDetailDataAsync(Guid.NewGuid(), companyData, string.Empty);

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
            .Returns((true, new [] { identifiers.First() }));

        // Act
        var Act = () => sut.SetCompanyDetailDataAsync(Guid.NewGuid(), companyData, string.Empty);

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
        async Task Act() => await sut.SetOwnCompanyApplicationStatusAsync(applicationId, 0, _fixture.Create<string>()).ConfigureAwait(false);
        
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
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserDataAsync(A<Guid>._, A<string>._))
            .ReturnsLazily(() => (CompanyApplicationUserData?) null);
        
        // Act
        async Task Act() => await sut.SetOwnCompanyApplicationStatusAsync(applicationId, CompanyApplicationStatusId.VERIFY, _fixture.Create<string>()).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"CompanyApplication {applicationId} not found");
    }

    [Fact]
    public async Task SetOwnCompanyApplicationStatusAsync_WithoutAssignedUser_ThrowsForbiddenException()
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
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserDataAsync(A<Guid>._, A<string>._))
            .ReturnsLazily(() => new CompanyApplicationUserData(_fixture.Create<CompanyApplication>()) );
        
        // Act
        async Task Act() => await sut.SetOwnCompanyApplicationStatusAsync(applicationId, CompanyApplicationStatusId.VERIFY, _fixture.Create<string>()).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        ex.Message.Should().Contain($"is not associated with application {applicationId}");
    }

    [Fact]
    public async Task SetOwnCompanyApplicationStatusAsync_WithInvalidStatus_ThrowsArgumentException()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var companyApplication = _fixture.Build<CompanyApplication>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.CREATED)
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
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserDataAsync(A<Guid>._, A<string>._))
            .ReturnsLazily(() => new CompanyApplicationUserData(companyApplication)
            {
                CompanyUserId = _fixture.Create<Guid>()
            });
        
        // Act
        async Task Act() => await sut.SetOwnCompanyApplicationStatusAsync(applicationId, CompanyApplicationStatusId.VERIFY, _fixture.Create<string>()).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(Act).ConfigureAwait(false);
        ex.Message.Should().Contain("invalid status update requested");
    }

    [Fact]
    public async Task SetOwnCompanyApplicationStatusAsync_WithValidData_SavesChanges()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var companyApplication = _fixture.Build<CompanyApplication>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.VERIFY)
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
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserDataAsync(A<Guid>._, A<string>._))
            .ReturnsLazily(() => new CompanyApplicationUserData(companyApplication)
            {
                CompanyUserId = _fixture.Create<Guid>()
            });
        
        // Act
        await sut.SetOwnCompanyApplicationStatusAsync(applicationId, CompanyApplicationStatusId.SUBMITTED, _fixture.Create<string>()).ConfigureAwait(false);
        
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
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<DocumentTypeId>._,A<Action<Document>?>._))
            .Invokes(x =>
            {
                var documentName = x.Arguments.Get<string>("documentName")!;
                var documentContent = x.Arguments.Get<byte[]>("documentContent")!;
                var hash = x.Arguments.Get<byte[]>("hash")!;
                var documentTypeId = x.Arguments.Get<DocumentTypeId>("documentType")!;
                var action = x.Arguments.Get<Action<Document?>>("setupOptionalFields");

                var document = new Document(documentId, documentContent, hash, documentName, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentTypeId);
                action?.Invoke(document);
                documents.Add(document);
            });
        var sut = _fixture.Create<RegistrationBusinessLogic>();

        // Act
        await sut.UploadDocumentAsync(_existingApplicationId, file, DocumentTypeId.ADDITIONAL_DETAILS, _iamUserId, CancellationToken.None);

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
        async Task Action() => await sut.UploadDocumentAsync(_existingApplicationId, file, DocumentTypeId.ADDITIONAL_DETAILS, _iamUserId, CancellationToken.None).ConfigureAwait(false);

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
        async Task Action() => await sut.UploadDocumentAsync(_existingApplicationId, file, DocumentTypeId.ADDITIONAL_DETAILS, _iamUserId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.Message.Should().Be("File name is must not be null");
    }

    [Fact]
    public async Task UploadDocumentAsync_WithNotExistingApplicationId_ThrowsException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        var sut = _fixture.Create<RegistrationBusinessLogic>();
        var notExistingId = Guid.NewGuid();

        // Act
        async Task Action() => await sut.UploadDocumentAsync(notExistingId, file, DocumentTypeId.ADDITIONAL_DETAILS, _iamUserId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Action);
        ex.Message.Should().Be($"iamUserId {_iamUserId} is not assigned with CompanyApplication {notExistingId}");
    }

    [Fact]
    public async Task UploadDocumentAsync_WithNotExistingIamUser_ThrowsException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        var sut = _fixture.Create<RegistrationBusinessLogic>();
        var notExistingId = Guid.NewGuid();

        // Act
        async Task Action() => await sut.UploadDocumentAsync(_existingApplicationId, file, DocumentTypeId.ADDITIONAL_DETAILS, notExistingId.ToString(), CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Action);
        ex.Message.Should().Be($"iamUserId {notExistingId} is not assigned with CompanyApplication {_existingApplicationId}");
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

        await sut.InviteNewUserAsync(_existingApplicationId, userCreationInfo, _iamUserId).ConfigureAwait(false);

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._, A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._, A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => _applicationRepository.CreateInvitation(A<Guid>.That.IsEqualTo(_existingApplicationId),A<Guid>._)).MustHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>.That.IsEqualTo(userCreationInfo.eMail), A<IDictionary<string,string>>.That.Matches(x => x["companyName"] == _displayName), A<List<string>>._)).MustHaveHappened();
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

        Task Act() => sut.InviteNewUserAsync(_existingApplicationId, userCreationInfo, _iamUserId);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be("email must not be empty");

        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string,string>>._, A<List<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestInviteNewUserUserAlreadyExistsThrows()
    {
        SetupFakesForInvitation();

        A.CallTo(() => _userRepository.IsOwnCompanyUserWithEmailExisting(A<string>._,A<string>._)).Returns(true);

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

        Task Act() => sut.InviteNewUserAsync(_existingApplicationId, userCreationInfo, _iamUserId);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"user with email {userCreationInfo.eMail} does already exist");

        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string,string>>._, A<List<string>>._)).MustNotHaveHappened();
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

        Task Act() => sut.InviteNewUserAsync(_existingApplicationId, userCreationInfo, _iamUserId);

        var error = await Assert.ThrowsAsync<TestException>(Act).ConfigureAwait(false);
        error.Message.Should().Be(_error.Message);

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._, A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._, A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => _applicationRepository.CreateInvitation(A<Guid>.That.IsEqualTo(_existingApplicationId),A<Guid>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>.That.IsEqualTo(userCreationInfo.eMail), A<IDictionary<string,string>>._, A<List<string>>._)).MustNotHaveHappened();
    }

    #endregion

    #region GetUploadedDocuments

    [Fact]
    public async Task GetUploadedDocumentsAsync_ReturnsExpectedOutput()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var iamUserId = _fixture.Create<string>();
        var uploadDocuments = _fixture.CreateMany<UploadDocuments>(3);

        A.CallTo(() => _documentRepository.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT, iamUserId))
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
        var result = await sut.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT, iamUserId).ConfigureAwait(false);

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
        var iamUserId = _fixture.Create<string>();
        var uploadDocuments = _fixture.CreateMany<UploadDocuments>(3);

        A.CallTo(() => _documentRepository.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT, iamUserId))
            .Returns(((bool,IEnumerable<UploadDocuments>))default);

        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories,
            null!);

        Task Act() => sut.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT, iamUserId);

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
        var iamUserId = _fixture.Create<string>();
        var uploadDocuments = _fixture.CreateMany<UploadDocuments>(3);

        A.CallTo(() => _documentRepository.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT, iamUserId))
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

        Task Act() => sut.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT, iamUserId);

        // Act
        var error = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);

        // Assert
        error.Message.Should().Be($"user {iamUserId} is not associated with application {applicationId}");
    }

    #endregion

    #region SubmitRoleConsents

    [Fact]
    public async Task SubmitRoleConsentsAsync_WithNotExistingApplication_ThrowsNotFoundException()
    {
        // Arrange
        var notExistingId = _fixture.Create<Guid>();
        A.CallTo(() => _companyRolesRepository.GetCompanyRoleAgreementConsentDataAsync(notExistingId, _iamUserId))
            .ReturnsLazily(() => (CompanyRoleAgreementConsentData?) null);
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRoleConsentAsync(notExistingId, _fixture.Create<CompanyRoleAgreementConsents>(), _iamUserId)
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
        var data = new CompanyRoleAgreementConsentData(Guid.Empty, Guid.NewGuid(), applicationStatusId, _fixture.CreateMany<CompanyRoleId>(2), _fixture.CreateMany<ConsentData>(5));
        A.CallTo(() => _companyRolesRepository.GetCompanyRoleAgreementConsentDataAsync(applicationId, _iamUserId))
            .ReturnsLazily(() => data);
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRoleConsentAsync(applicationId, _fixture.Create<CompanyRoleAgreementConsents>(), _iamUserId)
            .ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"iamUserId {_iamUserId} is not assigned with CompanyApplication {applicationId}");
    }

    [Fact]
    public async Task SubmitRoleConsentsAsync_WithInvalidRoles_ThrowsControllerArgumentException()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var applicationStatusId = _fixture.Create<CompanyApplicationStatusId>();
        var data = new CompanyRoleAgreementConsentData(Guid.NewGuid(), Guid.NewGuid(), applicationStatusId, _fixture.CreateMany<CompanyRoleId>(2), _fixture.CreateMany<ConsentData>(5));
        var roleIds = new List<CompanyRoleId>
        {
            CompanyRoleId.APP_PROVIDER,
        };
        var companyRoleAssignedAgreements = new List<(CompanyRoleId CompanyRoleId, IEnumerable<Guid> AgreementIds)>
        {
            new ValueTuple<CompanyRoleId, IEnumerable<Guid>>(CompanyRoleId.APP_PROVIDER, _fixture.CreateMany<Guid>(5)),
        };
        A.CallTo(() => _companyRolesRepository.GetCompanyRoleAgreementConsentDataAsync(applicationId, _iamUserId))
            .ReturnsLazily(() => data);
        A.CallTo(() => _companyRolesRepository.GetAgreementAssignedCompanyRolesUntrackedAsync(roleIds))
            .Returns(companyRoleAssignedAgreements.ToAsyncEnumerable());
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRoleConsentAsync(applicationId, _fixture.Create<CompanyRoleAgreementConsents>(), _iamUserId)
            .ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Contain("invalid companyRole: ");
    }

    [Fact]
    public async Task SubmitRoleConsentsAsync_WithoutAllRolesConsentGiven_ThrowsControllerArgumentException()
    {
        // Arrange
        var consents = new CompanyRoleAgreementConsents(new []
            {
                CompanyRoleId.APP_PROVIDER,
            },
            new []
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
        var companyId = Guid.NewGuid();
        var data = new CompanyRoleAgreementConsentData(Guid.NewGuid(), companyId, applicationStatusId, new []{ CompanyRoleId.APP_PROVIDER }, new List<ConsentData>());
        var companyRoleAssignedAgreements = new List<(CompanyRoleId CompanyRoleId, IEnumerable<Guid> AgreementIds)>
        {
            new ValueTuple<CompanyRoleId, IEnumerable<Guid>>(CompanyRoleId.APP_PROVIDER, agreementIds)
        };
        A.CallTo(() => _companyRolesRepository.GetCompanyRoleAgreementConsentDataAsync(applicationId, _iamUserId))
            .ReturnsLazily(() => data);
        A.CallTo(() => _companyRolesRepository.GetAgreementAssignedCompanyRolesUntrackedAsync(A<IEnumerable<CompanyRoleId>>._))
            .Returns(companyRoleAssignedAgreements.ToAsyncEnumerable());

        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRoleConsentAsync(applicationId, consents, _iamUserId)
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
        var consents = new CompanyRoleAgreementConsents(new []
            {
                CompanyRoleId.APP_PROVIDER,
                CompanyRoleId.ACTIVE_PARTICIPANT
            },
            new []
            {
                new AgreementConsentStatus(agreementId_1, ConsentStatusId.ACTIVE),
                new AgreementConsentStatus(agreementId_2, ConsentStatusId.ACTIVE)
            });
        var applicationId = _fixture.Create<Guid>();
        var applicationStatusId =  CompanyApplicationStatusId.INVITE_USER;
        var agreementIds = new List<Guid>
        {
            agreementId_1,
            agreementId_2
        };
        var companyId = Guid.NewGuid();
        var data = new CompanyRoleAgreementConsentData(
            Guid.NewGuid(), 
            companyId, 
            applicationStatusId,
            new []
            {
                CompanyRoleId.APP_PROVIDER,
                CompanyRoleId.SERVICE_PROVIDER,
            },
            new [] {
                new ConsentData(consentId, ConsentStatusId.INACTIVE, agreementId_1)
            });
        var companyRoleAssignedAgreements = new List<(CompanyRoleId CompanyRoleId, IEnumerable<Guid> AgreementIds)>
        {
            new ValueTuple<CompanyRoleId, IEnumerable<Guid>>(CompanyRoleId.APP_PROVIDER, new [] { agreementId_1, agreementId_2 }),
            new ValueTuple<CompanyRoleId, IEnumerable<Guid>>(CompanyRoleId.ACTIVE_PARTICIPANT, new [] { agreementId_1 }),
            new ValueTuple<CompanyRoleId, IEnumerable<Guid>>(CompanyRoleId.SERVICE_PROVIDER, new [] { agreementId_1, agreementId_3 }),
        };
        A.CallTo(() => _companyRolesRepository.GetCompanyRoleAgreementConsentDataAsync(applicationId, _iamUserId))
            .Returns(data);
        A.CallTo(() => _companyRolesRepository.GetAgreementAssignedCompanyRolesUntrackedAsync(A<IEnumerable<CompanyRoleId>>._))
            .Returns(companyRoleAssignedAgreements.ToAsyncEnumerable());
        A.CallTo(() => _consentRepository.AttachAndModifiesConsents(A<IEnumerable<Guid>>._, A<Action<Consent>>._))
            .Invokes((IEnumerable<Guid> consentIds, Action<Consent> setOptionalParameter) =>
            {
                var consents = consentIds.Select(x => new Consent(x));
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
        A.CallTo(() => _companyRolesRepository.RemoveCompanyAssignedRoles(companyId, A<IEnumerable<CompanyRoleId>>._))
            .Invokes((Guid _, IEnumerable<CompanyRoleId> companyRoleIds) =>
            {
                removedCompanyRoleIds = companyRoleIds;
            });

        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        await sut.SubmitRoleConsentAsync(applicationId, consents, _iamUserId).ConfigureAwait(false);

        // Arrange
        A.CallTo(() => _consentRepository.AttachAndModifiesConsents(A<IEnumerable<Guid>>._, A<Action<Consent>>._)).MustHaveHappened(2, Times.Exactly);
        A.CallTo(() => _consentRepository.CreateConsent(A<Guid>._, A<Guid>._, A<Guid>._, A<ConsentStatusId>._, A<Action<Consent>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRolesRepository.CreateCompanyAssignedRole(companyId, CompanyRoleId.ACTIVE_PARTICIPANT)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRolesRepository.CreateCompanyAssignedRole(A<Guid>.That.Not.IsEqualTo(companyId), A<CompanyRoleId>._)).MustNotHaveHappened();
        A.CallTo(() => _companyRolesRepository.CreateCompanyAssignedRole(A<Guid>._, A<CompanyRoleId>.That.Not.IsEqualTo(CompanyRoleId.ACTIVE_PARTICIPANT))).MustNotHaveHappened();
        A.CallTo(() => _companyRolesRepository.RemoveCompanyAssignedRoles(companyId, A<IEnumerable<CompanyRoleId>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRolesRepository.RemoveCompanyAssignedRoles(A<Guid>.That.Not.IsEqualTo(companyId), A<IEnumerable<CompanyRoleId>>._)).MustNotHaveHappened();
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
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(notExistingId, _iamUserId))
            .ReturnsLazily(() => (CompanyApplicationUserEmailData?) null);
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), _mailingService, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRegistrationAsync(notExistingId, _iamUserId)
            .ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"application {notExistingId} does not exist");
    }

    
    [Fact]
    public async Task SubmitRegistrationAsync_WithDocumentId()
    {
        // Arrange
        var applicationid = _fixture.Create<Guid>();
        IEnumerable<DocumentStatusData> document = new DocumentStatusData[]{
            new(Guid.NewGuid(),DocumentStatusId.PENDING),
            new(Guid.NewGuid(),DocumentStatusId.INACTIVE)
        };
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationid, _iamUserId))
            .ReturnsLazily(() => new CompanyApplicationUserEmailData(CompanyApplicationStatusId.VERIFY,Guid.NewGuid(),"test@mail.de",document));
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), _mailingService, null!, null!, null!, null!, _portalRepositories, _checklistService);

        // Act
        await sut.SubmitRegistrationAsync(applicationid, _iamUserId);
        // Arrange
        A.CallTo(() => _documentRepository.AttachAndModifyDocument(A<Guid>._, A<Action<Document>>._, A<Action<Document>>._)).MustHaveHappened(2, Times.Exactly);
    }
    
    [Fact]
    public async Task SubmitRegistrationAsync_WithNotExistingCompanyUser_ThrowsForbiddenException()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, Guid.Empty.ToString()))
            .ReturnsLazily(() => new CompanyApplicationUserEmailData(CompanyApplicationStatusId.VERIFY, Guid.Empty, null,null!));
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), _mailingService, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        async Task Act() => await sut.SubmitRegistrationAsync(applicationId, Guid.Empty.ToString())
            .ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"iamUserId {Guid.Empty.ToString()} is not assigned with CompanyApplication {applicationId}");
    }

    [Fact]
    public async Task SubmitRegistrationAsync_WithUserEmail_SendsMail()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        IEnumerable<DocumentStatusData> document =  new DocumentStatusData[]{
            new(
                Guid.NewGuid(),DocumentStatusId.INACTIVE
            )};
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, _iamUserId))
            .ReturnsLazily(() => new CompanyApplicationUserEmailData(CompanyApplicationStatusId.VERIFY, Guid.NewGuid(), "test@mail.de",document));
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), _mailingService, null!, null!, null!, null!, _portalRepositories, _checklistService);

        // Act
        var result = await sut.SubmitRegistrationAsync(applicationId, _iamUserId)
            .ConfigureAwait(false);

        // Arrange
        A.CallTo(() => _checklistService.CreateInitialChecklistAsync(applicationId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>._)).MustHaveHappened();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SubmitRegistrationAsync_WithoutUserEmail_DoesntSendMail()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        IEnumerable<DocumentStatusData> document =  new DocumentStatusData[]{
            new(
                Guid.NewGuid(),DocumentStatusId.PENDING
            )};
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, _iamUserId))
            .ReturnsLazily(() => new CompanyApplicationUserEmailData(CompanyApplicationStatusId.VERIFY, Guid.NewGuid(), null, document));
        var sut = new RegistrationBusinessLogic(Options.Create(new RegistrationSettings()), _mailingService, null!, null!, null!, A.Fake<ILogger<RegistrationBusinessLogic>>(), _portalRepositories, _checklistService);

        // Act
        var result = await sut.SubmitRegistrationAsync(applicationId, _iamUserId)
            .ConfigureAwait(false);

        // Arrange
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
            .Returns((uniqueIdentifierData,true));
 
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
        A.CallTo(() => _applicationRepository.GetRegistrationDataUntrackedAsync(_existingApplicationId, _iamUserId, A<IEnumerable<DocumentTypeId>>._))
            .Returns((true, true, data));
            
        var sut = new RegistrationBusinessLogic(_options, null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        var result = await sut.GetRegistrationDataAsync(_existingApplicationId, _iamUserId).ConfigureAwait(false);

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
            x.Should().Match<(AgreementConsentStatusForRegistrationData First,(Guid AgreementId,ConsentStatusId ConsentStatusId) Second)>(x =>
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
        A.CallTo(() => _applicationRepository.GetRegistrationDataUntrackedAsync(A<Guid>._, _iamUserId, A<IEnumerable<DocumentTypeId>>._))
            .Returns((false, false, data));
            
        var sut = new RegistrationBusinessLogic(_options, null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        var Act = () => sut.GetRegistrationDataAsync(applicationId, _iamUserId);

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
        var iamUserId = _fixture.Create<string>();
        A.CallTo(() => _applicationRepository.GetRegistrationDataUntrackedAsync(A<Guid>._, A<string>._, A<IEnumerable<DocumentTypeId>>._))
            .Returns((true, false, data));
            
        var sut = new RegistrationBusinessLogic(_options, null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        var Act = () => sut.GetRegistrationDataAsync(applicationId, iamUserId);

        // Assert
        var result = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"iamUserId {iamUserId} is not assigned with CompanyApplication {applicationId}");
    }

    [Fact]
    public async Task GetRegistrationDataAsync_WithNullData_Throws()
    {
        var applicationId = Guid.NewGuid();
        var iamUserId = _fixture.Create<string>();
        // Arrange
        A.CallTo(() => _applicationRepository.GetRegistrationDataUntrackedAsync(A<Guid>._, A<string>._, A<IEnumerable<DocumentTypeId>>._))
            .Returns((true, true, null));
            
        var sut = new RegistrationBusinessLogic(_options, null!, null!, null!, null!, null!, _portalRepositories, null!);

        // Act
        var Act = () => sut.GetRegistrationDataAsync(applicationId, iamUserId);

        // Assert
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"registrationData should never be null for application {applicationId}");
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

        A.CallTo(() => _userRepository.GetCompanyUserIdForUserApplicationUntrackedAsync(
                _existingApplicationId, _iamUserId))
            .Returns(_companyUserId);
        A.CallTo(() => _userRepository.GetCompanyUserIdForUserApplicationUntrackedAsync(
                _existingApplicationId, A<string>.That.Not.Matches(x => x == _iamUserId)))
            .Returns(Guid.Empty);
        A.CallTo(() => _userRepository.GetCompanyUserIdForUserApplicationUntrackedAsync(
                A<Guid>.That.Not.Matches(x => x == _existingApplicationId), _iamUserId))
            .Returns(Guid.Empty);
        A.CallTo(() => _userRepository.GetCompanyUserIdForUserApplicationUntrackedAsync(
                A<Guid>.That.Not.Matches(x => x == _existingApplicationId), A<string>.That.Not.Matches(x => x == _iamUserId)))
            .Returns(Guid.Empty);

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
    }

    private void SetupFakesForInvitation()
    {
        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._,A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._,A<CancellationToken>._))
            .ReturnsLazily((CompanyNameIdpAliasData companyNameIdpAliasData, IAsyncEnumerable<UserCreationRoleDataIdpInfo> userCreationInfos, CancellationToken cancellationToken) =>
                userCreationInfos.Select(userCreationInfo => _processLine(userCreationInfo)));

        A.CallTo(() => _userProvisioningService.GetRoleDatas(A<IDictionary<string,IEnumerable<string>>>._))
            .ReturnsLazily((IDictionary<string,IEnumerable<string>> clientRoles) =>
                clientRoles.SelectMany(r => r.Value.Select(role => _fixture.Build<UserRoleData>().With(x => x.UserRoleText, role).Create())).ToAsyncEnumerable());

        A.CallTo(() => _userProvisioningService.GetIdentityProviderDisplayName(A<string>._)).Returns(_displayName);

        A.CallTo(() => _userProvisioningService.GetCompanyNameSharedIdpAliasData(A<string>._,A<Guid?>._)).Returns(
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
