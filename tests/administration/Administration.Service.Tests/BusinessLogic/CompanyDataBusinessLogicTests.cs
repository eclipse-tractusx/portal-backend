/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class CompanyDataBusinessLogicTests
{
    private static readonly Guid _validDocumentId = Guid.NewGuid();

    private readonly IIdentityData _identity;
    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IConsentRepository _consentRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICompanyRolesRepository _companyRolesRepository;
    private readonly IProcessStepRepository<ProcessTypeId, ProcessStepTypeId> _processStepRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly ICompanyCertificateRepository _companyCertificateRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly DateTimeOffset _now;
    private readonly CompanyDataBusinessLogic _sut;

    public CompanyDataBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.ConfigureFixture();

        _portalRepositories = A.Fake<IPortalRepositories>();
        _consentRepository = A.Fake<IConsentRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _companyRolesRepository = A.Fake<ICompanyRolesRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _languageRepository = A.Fake<ILanguageRepository>();
        _companyCertificateRepository = A.Fake<ICompanyCertificateRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>();
        var issuerComponentBusinessLogic = A.Fake<IIssuerComponentBusinessLogic>();

        _now = _fixture.Create<DateTimeOffset>();
        _dateTimeProvider = A.Fake<IDateTimeProvider>();
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(_now);

        var identityService = A.Fake<IIdentityService>();
        _identity = A.Fake<IIdentityData>();

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentRepository>()).Returns(_consentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRolesRepository>()).Returns(_companyRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ILanguageRepository>()).Returns(_languageRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyCertificateRepository>()).Returns(_companyCertificateRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>()).Returns(_processStepRepository);

        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        A.CallTo(() => identityService.IdentityData).Returns(_identity);

        var options = Options.Create(new CompanyDataSettings { MaxPageSize = 20, CompanyCertificateMediaTypes = new[] { MediaTypeId.PDF }, DecentralIdentityManagementAuthUrl = "https://example.org/test", IssuerDid = "did:web:test", BpnDidResolverUrl = "https://example.org/bdrs" });
        _sut = new CompanyDataBusinessLogic(_portalRepositories, _dateTimeProvider, identityService, issuerComponentBusinessLogic, options);
    }

    #region GetOwnCompanyDetails

    [Fact]
    public async Task GetOwnCompanyDetailsAsync_ExpectedResults()
    {
        // Arrange
        var companyAddressDetailData = _fixture.Create<CompanyAddressDetailData>();
        A.CallTo(() => _companyRepository.GetCompanyDetailsAsync(_identity.CompanyId))
            .Returns(companyAddressDetailData);

        // Act
        var result = await _sut.GetCompanyDetailsAsync();

        // Assert
        result.Should().NotBeNull();
        result.CompanyId.Should().Be(companyAddressDetailData.CompanyId);
    }

    [Fact]
    public async Task GetOwnCompanyDetailsAsync_ThrowsConflictException()
    {
        // Arrange
        A.CallTo(() => _companyRepository.GetCompanyDetailsAsync(_identity.CompanyId))
            .Returns<CompanyAddressDetailData?>(null);

        // Act
        async Task Act() => await _sut.GetCompanyDetailsAsync();

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"company {_identity.CompanyId} is not a valid company");
    }

    #endregion

    #region GetCompanyRoleAndConsentAgreementDetails

    [Fact]
    public async Task GetCompanyRoleAndConsentAgreementDetails_CallsExpected()
    {
        // Arrange
        const string languageShortName = "en";
        var companyId = Guid.NewGuid();
        var companyRoleConsentData = new[] {
            _fixture.Create<CompanyRoleConsentData>(),
            new CompanyRoleConsentData(
                _fixture.Create<CompanyRoleId>(),
                _fixture.Create<string>(),
                true,
                new ConsentAgreementData [] {
                    new (Guid.NewGuid(), _fixture.Create<string>(), Guid.NewGuid(), 0, _fixture.Create<string>(), true),
                    new (Guid.NewGuid(), _fixture.Create<string>(), Guid.NewGuid(), ConsentStatusId.ACTIVE, _fixture.Create<string>(), true),
                    new (Guid.NewGuid(), _fixture.Create<string>(), Guid.NewGuid(), ConsentStatusId.INACTIVE, _fixture.Create<string>(), false),
                }),
            _fixture.Create<CompanyRoleConsentData>(),
        };

        A.CallTo(() => _identity.CompanyId).Returns(companyId);
        A.CallTo(() => _companyRepository.GetCompanyStatusDataAsync(A<Guid>._))
            .Returns((true, true));

        A.CallTo(() => _languageRepository.IsValidLanguageCode(A<string>._))
            .Returns(true);

        A.CallTo(() => _companyRepository.GetCompanyRoleAndConsentAgreementDataAsync(A<Guid>._, A<string>._))
            .Returns(companyRoleConsentData.ToAsyncEnumerable());

        // Act
        var result = await _sut.GetCompanyRoleAndConsentAgreementDetailsAsync(languageShortName).ToListAsync();

        // Assert
        result.Should().NotBeNull()
            .And.HaveSameCount(companyRoleConsentData);

        result.Zip(companyRoleConsentData).Should()
            .AllSatisfy(x => x.Should().Match<(CompanyRoleConsentViewData Result, CompanyRoleConsentData Mock)>(
                z =>
                z.Result.CompanyRoleId == z.Mock.CompanyRoleId &&
                z.Result.RoleDescription == z.Mock.RoleDescription &&
                z.Result.CompanyRolesActive == z.Mock.CompanyRolesActive &&
                z.Result.Agreements.SequenceEqual(z.Mock.Agreements.Select(a => new ConsentAgreementViewData(a.AgreementId, a.AgreementName, a.DocumentId, a.ConsentStatus == 0 ? null : a.ConsentStatus, a.AgreementLink, a.Mandatory)))));

        A.CallTo(() => _companyRepository.GetCompanyStatusDataAsync(companyId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _languageRepository.IsValidLanguageCode(languageShortName)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRepository.GetCompanyRoleAndConsentAgreementDataAsync(companyId, languageShortName)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompanyRoleAndConsentAgreementDetails_ThrowsNotFoundException()
    {
        // Arrange
        const string languageShortName = "en";
        var companyId = Guid.NewGuid();
        A.CallTo(() => _identity.CompanyId).Returns(companyId);

        A.CallTo(() => _companyRepository.GetCompanyStatusDataAsync(A<Guid>._))
            .Returns((false, false));

        A.CallTo(() => _languageRepository.IsValidLanguageCode(languageShortName))
            .Returns(true);

        // Act
        async Task Act() => await _sut.GetCompanyRoleAndConsentAgreementDetailsAsync(languageShortName).ToListAsync();

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"company {companyId} does not exist");
        A.CallTo(() => _companyRepository.GetCompanyStatusDataAsync(companyId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompanyRoleAndConsentAgreementDetails_ThrowsConflictException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        const string languageShortName = "en";
        A.CallTo(() => _identity.CompanyId).Returns(companyId);

        A.CallTo(() => _companyRepository.GetCompanyStatusDataAsync(A<Guid>._))
            .Returns((false, true));

        A.CallTo(() => _languageRepository.IsValidLanguageCode(languageShortName))
            .Returns(true);

        // Act
        async Task Act() => await _sut.GetCompanyRoleAndConsentAgreementDetailsAsync(languageShortName).ToListAsync();

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Company Status is Incorrect");
        A.CallTo(() => _companyRepository.GetCompanyStatusDataAsync(companyId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompanyRoleAndConsentAgreementDetails_Throws()
    {
        // Arrange
        const string languageShortName = "eng";

        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        A.CallTo(() => _languageRepository.IsValidLanguageCode(languageShortName))
            .Returns(false);

        // Act
        async Task Act() => await _sut.GetCompanyRoleAndConsentAgreementDetailsAsync(languageShortName).ToListAsync();

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"language {languageShortName} is not a valid languagecode");

    }

    #endregion

    #region CreateCompanyRoleAndConsentAgreementDetails

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_ReturnsExpected()
    {
        // Arrange
        var companyId = _identity.CompanyId;
        var companyUserId = _identity.IdentityId;
        var agreementId1 = _fixture.Create<Guid>();
        var agreementId2 = _fixture.Create<Guid>();
        var agreementId3 = _fixture.Create<Guid>();
        var agreementId4 = _fixture.Create<Guid>();
        var agreementId5 = _fixture.Create<Guid>();
        var agreementId6 = _fixture.Create<Guid>();
        var agreementId7 = _fixture.Create<Guid>();

        var utcNow = DateTimeOffset.UtcNow;

        var consentStatusDetails = new ConsentStatusDetails[] {
            new (_fixture.Create<Guid>(), agreementId1, ConsentStatusId.INACTIVE),
            new (_fixture.Create<Guid>(), agreementId5, ConsentStatusId.ACTIVE),
        };
        var companyRoleConsentDetails = new CompanyRoleConsentDetails[] {
            new (CompanyRoleId.ACTIVE_PARTICIPANT,
                new ConsentDetails[] {
                    new (agreementId1, ConsentStatusId.ACTIVE),
                    new (agreementId2, ConsentStatusId.ACTIVE),
                    new (agreementId3, ConsentStatusId.ACTIVE),
                }),
            new (CompanyRoleId.APP_PROVIDER,
                new ConsentDetails[] {
                    new (agreementId3, ConsentStatusId.ACTIVE),
                    new (agreementId4, ConsentStatusId.ACTIVE),
                    new (agreementId5, ConsentStatusId.ACTIVE),
                }),
            new (CompanyRoleId.SERVICE_PROVIDER,
                new ConsentDetails[] {
                    new (agreementId6, ConsentStatusId.INACTIVE),
                    new (agreementId7, ConsentStatusId.INACTIVE),
                }),
        };

        var agreementData = new (AgreementStatusData, CompanyRoleId)[]{
            new (new AgreementStatusData(agreementId1, AgreementStatusId.ACTIVE), CompanyRoleId.ACTIVE_PARTICIPANT),
            new (new AgreementStatusData(agreementId2, AgreementStatusId.INACTIVE), CompanyRoleId.ACTIVE_PARTICIPANT),
            new (new AgreementStatusData(agreementId3, AgreementStatusId.ACTIVE), CompanyRoleId.ACTIVE_PARTICIPANT),
            new (new AgreementStatusData(agreementId3, AgreementStatusId.ACTIVE), CompanyRoleId.APP_PROVIDER),
            new (new AgreementStatusData(agreementId4, AgreementStatusId.ACTIVE), CompanyRoleId.APP_PROVIDER),
            new (new AgreementStatusData(agreementId5, AgreementStatusId.ACTIVE), CompanyRoleId.APP_PROVIDER),
            new (new AgreementStatusData(agreementId6, AgreementStatusId.ACTIVE), CompanyRoleId.SERVICE_PROVIDER),
            new (new AgreementStatusData(agreementId7, AgreementStatusId.ACTIVE), CompanyRoleId.SERVICE_PROVIDER),
        }.ToAsyncEnumerable();

        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(A<Guid>._, A<IEnumerable<CompanyRoleId>>._))
            .Returns((true, true, new[] { CompanyRoleId.SERVICE_PROVIDER }, consentStatusDetails));

        A.CallTo(() => _companyRepository.GetAgreementAssignedRolesDataAsync(A<IEnumerable<CompanyRoleId>>._))
            .Returns(agreementData);

        A.CallTo(() => _consentRepository.AddAttachAndModifyConsents(A<IEnumerable<ConsentStatusDetails>>._, A<IEnumerable<(Guid, ConsentStatusId)>>._, A<Guid>._, A<Guid>._, A<DateTimeOffset>._))
            .Returns(new Consent[] {
                new(_fixture.Create<Guid>(), agreementId1, companyId, companyUserId, ConsentStatusId.ACTIVE, utcNow),
                new(_fixture.Create<Guid>(), agreementId2, companyId, companyUserId, ConsentStatusId.ACTIVE, utcNow),
                new(_fixture.Create<Guid>(), agreementId3, companyId, companyUserId, ConsentStatusId.ACTIVE, utcNow),
                new(_fixture.Create<Guid>(), agreementId4, companyId, companyUserId, ConsentStatusId.ACTIVE, utcNow),
                new(_fixture.Create<Guid>(), agreementId5, companyId, companyUserId, ConsentStatusId.ACTIVE, utcNow),
                new(_fixture.Create<Guid>(), agreementId6, companyId, companyUserId, ConsentStatusId.INACTIVE, utcNow),
                new(_fixture.Create<Guid>(), agreementId7, companyId, companyUserId, ConsentStatusId.INACTIVE, utcNow)
            });

        // Act
        await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails);

        // Assert
        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(companyId, A<IEnumerable<CompanyRoleId>>._)).MustHaveHappenedOnceExactly();

        A.CallTo(() => _companyRolesRepository.CreateCompanyAssignedRoles(
                companyId,
                A<IEnumerable<CompanyRoleId>>.That.Matches(x =>
                    x.Count() == 2 &&
                    x.Contains(CompanyRoleId.ACTIVE_PARTICIPANT) &&
                    x.Contains(CompanyRoleId.APP_PROVIDER))))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _consentRepository.AddAttachAndModifyConsents(
                consentStatusDetails,
                A<IEnumerable<(Guid, ConsentStatusId)>>.That.Matches(x =>
                    x.Count() == 6 &&
                    x.Contains(new(agreementId1, ConsentStatusId.ACTIVE)) &&
                    x.Contains(new(agreementId3, ConsentStatusId.ACTIVE)) &&
                    x.Contains(new(agreementId4, ConsentStatusId.ACTIVE)) &&
                    x.Contains(new(agreementId5, ConsentStatusId.ACTIVE)) &&
                    x.Contains(new(agreementId6, ConsentStatusId.INACTIVE)) &&
                    x.Contains(new(agreementId7, ConsentStatusId.INACTIVE))),
                companyId,
                companyUserId,
                A<DateTimeOffset>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _companyRolesRepository.RemoveCompanyAssignedRoles(
                companyId,
                A<IEnumerable<CompanyRoleId>>.That.Matches(x =>
                    x.Count() == 1 &&
                    x.Contains(CompanyRoleId.SERVICE_PROVIDER))))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_CompanyStatus_ThrowsConflictException()
    {
        // Arrange
        var companyRoleConsentDetails = _fixture.CreateMany<CompanyRoleConsentDetails>(2);

        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(A<Guid>._, A<IEnumerable<CompanyRoleId>>._))
            .Returns((true, false, null, null));

        // Act
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Company Status is Incorrect");
        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(_identity.CompanyId, A<IEnumerable<CompanyRoleId>>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_MissingAgreementAssignedRole_ThrowsControllerArgumentException()
    {
        // Arrange
        var agreementId1 = _fixture.Create<Guid>();
        var agreementId2 = _fixture.Create<Guid>();
        var agreementId3 = _fixture.Create<Guid>();
        var agreementId4 = _fixture.Create<Guid>();
        var agreementId5 = _fixture.Create<Guid>();

        var consentStatusDetails = new ConsentStatusDetails[] {
            new (_fixture.Create<Guid>(), agreementId1, ConsentStatusId.INACTIVE),
            new (_fixture.Create<Guid>(), agreementId5, ConsentStatusId.ACTIVE),
        };
        var companyRoleConsentDetails = new CompanyRoleConsentDetails[] {
            new (CompanyRoleId.ACTIVE_PARTICIPANT,
                new ConsentDetails[] {
                    new (agreementId1, ConsentStatusId.ACTIVE),
                    new (agreementId2, ConsentStatusId.INACTIVE),
                }),
            new (CompanyRoleId.APP_PROVIDER,
                new ConsentDetails[] {
                    new (agreementId4, ConsentStatusId.ACTIVE),
                    new (agreementId5, ConsentStatusId.ACTIVE),
                }),
        };

        var agreementData = new (AgreementStatusData, CompanyRoleId)[]{
            new (new AgreementStatusData(agreementId1, AgreementStatusId.ACTIVE), CompanyRoleId.ACTIVE_PARTICIPANT),
            new (new AgreementStatusData(agreementId2, AgreementStatusId.ACTIVE), CompanyRoleId.ACTIVE_PARTICIPANT),
            new (new AgreementStatusData(agreementId3, AgreementStatusId.ACTIVE), CompanyRoleId.ACTIVE_PARTICIPANT),
            new (new AgreementStatusData(agreementId3, AgreementStatusId.ACTIVE), CompanyRoleId.APP_PROVIDER),
            new (new AgreementStatusData(agreementId4, AgreementStatusId.ACTIVE), CompanyRoleId.APP_PROVIDER),
            new (new AgreementStatusData(agreementId5, AgreementStatusId.ACTIVE), CompanyRoleId.APP_PROVIDER),
        }.ToAsyncEnumerable();

        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(A<Guid>._, A<IEnumerable<CompanyRoleId>>._))
            .Returns((true, true, Enumerable.Empty<CompanyRoleId>(), consentStatusDetails));

        A.CallTo(() => _companyRepository.GetAgreementAssignedRolesDataAsync(A<IEnumerable<CompanyRoleId>>._))
            .Returns(agreementData);

        // Act
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"All agreements need to get signed as Active or InActive. Missing consents: [ACTIVE_PARTICIPANT: [{agreementId2}, {agreementId3}], APP_PROVIDER: [{agreementId3}]]");
        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(_identity.CompanyId, A<IEnumerable<CompanyRoleId>>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_ExtraAgreementAssignedRole_ThrowsControllerArgumentException()
    {
        // Arrange
        var agreementId1 = _fixture.Create<Guid>();
        var agreementId2 = _fixture.Create<Guid>();
        var agreementId3 = _fixture.Create<Guid>();
        var agreementId4 = _fixture.Create<Guid>();
        var agreementId5 = _fixture.Create<Guid>();

        var consentStatusDetails = new ConsentStatusDetails[] {
            new (_fixture.Create<Guid>(), agreementId1, ConsentStatusId.INACTIVE),
            new (_fixture.Create<Guid>(), agreementId5, ConsentStatusId.ACTIVE),
        };
        var companyRoleConsentDetails = new CompanyRoleConsentDetails[] {
            new (CompanyRoleId.ACTIVE_PARTICIPANT,
                new ConsentDetails[] {
                    new (agreementId1, ConsentStatusId.ACTIVE),
                    new (agreementId2, ConsentStatusId.ACTIVE),
                    new (agreementId3, ConsentStatusId.ACTIVE),
                    new (agreementId4, ConsentStatusId.ACTIVE),
                }),
            new (CompanyRoleId.APP_PROVIDER,
                new ConsentDetails[] {
                    new (agreementId1, ConsentStatusId.INACTIVE),
                    new (agreementId2, ConsentStatusId.INACTIVE),
                    new (agreementId3, ConsentStatusId.ACTIVE),
                    new (agreementId4, ConsentStatusId.ACTIVE),
                    new (agreementId5, ConsentStatusId.ACTIVE),
                }),
        };

        var agreementData = new (AgreementStatusData, CompanyRoleId)[]{
            new (new AgreementStatusData(agreementId1, AgreementStatusId.ACTIVE), CompanyRoleId.ACTIVE_PARTICIPANT),
            new (new AgreementStatusData(agreementId2, AgreementStatusId.ACTIVE), CompanyRoleId.ACTIVE_PARTICIPANT),
            new (new AgreementStatusData(agreementId3, AgreementStatusId.ACTIVE), CompanyRoleId.ACTIVE_PARTICIPANT),
            new (new AgreementStatusData(agreementId3, AgreementStatusId.ACTIVE), CompanyRoleId.APP_PROVIDER),
            new (new AgreementStatusData(agreementId4, AgreementStatusId.ACTIVE), CompanyRoleId.APP_PROVIDER),
            new (new AgreementStatusData(agreementId5, AgreementStatusId.ACTIVE), CompanyRoleId.APP_PROVIDER),
        }.ToAsyncEnumerable();

        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(A<Guid>._, A<IEnumerable<CompanyRoleId>>._))
            .Returns((true, true, Enumerable.Empty<CompanyRoleId>(), consentStatusDetails));

        A.CallTo(() => _companyRepository.GetAgreementAssignedRolesDataAsync(A<IEnumerable<CompanyRoleId>>._))
            .Returns(agreementData);

        // Act
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"Agreements not associated with requested companyRoles: [ACTIVE_PARTICIPANT: [{agreementId4}], APP_PROVIDER: [{agreementId1}, {agreementId2}]]");
        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(_identity.CompanyId, A<IEnumerable<CompanyRoleId>>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_AllUnassignedCompanyRoles_ThrowsConflictException()
    {
        // Arrange
        var agreementId1 = _fixture.Create<Guid>();
        var agreementId2 = _fixture.Create<Guid>();
        var consentStatusDetails = _fixture.CreateMany<ConsentStatusDetails>();

        var companyRoleConsentDetails = new CompanyRoleConsentDetails[] {
            new (CompanyRoleId.ACTIVE_PARTICIPANT,
                new ConsentDetails[] {
                    new (agreementId1, ConsentStatusId.INACTIVE),
                    new (agreementId2, ConsentStatusId.INACTIVE)
                })};

        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(A<Guid>._, A<IEnumerable<CompanyRoleId>>._))
            .Returns((true, true, Enumerable.Empty<CompanyRoleId>(), consentStatusDetails));

        // Act
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Company can't unassign from all roles, Atleast one Company role need to signed as active");
        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(_identity.CompanyId, A<IEnumerable<CompanyRoleId>>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_ThrowsConflictException()
    {
        // Arrange
        var companyRoleConsentDetails = _fixture.CreateMany<CompanyRoleConsentDetails>(2);
        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(A<Guid>._, A<IEnumerable<CompanyRoleId>>._))
            .Returns<(bool, bool, IEnumerable<CompanyRoleId>?, IEnumerable<ConsentStatusDetails>?)>(default);

        // Act
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"company {_identity.CompanyId} does not exist");
        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(_identity.CompanyId, A<IEnumerable<CompanyRoleId>>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var companyRoleConsentDetails = _fixture.CreateMany<CompanyRoleConsentDetails>(2);
        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(A<Guid>._, A<IEnumerable<CompanyRoleId>>._))
            .Returns((true, true, null!, null!));

        // Act
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be($"neither CompanyRoleIds nor ConsentStatusDetails should ever be null here");
        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(_identity.CompanyId, A<IEnumerable<CompanyRoleId>>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_AgreementAssignedrole_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var companyRoleConsentDetails = _fixture.CreateMany<CompanyRoleConsentDetails>(2);
        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(A<Guid>._, A<IEnumerable<CompanyRoleId>>._))
            .Returns((true, true, Enumerable.Empty<CompanyRoleId>(), null!));

        // Act
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be($"neither CompanyRoleIds nor ConsentStatusDetails should ever be null here");
        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(_identity.CompanyId, A<IEnumerable<CompanyRoleId>>._)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region CompanyAssigendUseCaseDetails

    [Fact]
    public async Task GetCompanyAssigendUseCaseDetailsAsync_ResturnsExpected()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var companyAssignedUseCaseData = _fixture.CreateMany<CompanyAssignedUseCaseData>(2).ToAsyncEnumerable();
        A.CallTo(() => _identity.CompanyId).Returns(companyId);
        A.CallTo(() => _companyRepository.GetCompanyAssigendUseCaseDetailsAsync(A<Guid>._))
            .Returns(companyAssignedUseCaseData);

        // Act
        var result = await _sut.GetCompanyAssigendUseCaseDetailsAsync().ToListAsync();

        // Assert
        result.Should().NotBeNull();

        A.CallTo(() => _companyRepository.GetCompanyAssigendUseCaseDetailsAsync(companyId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyAssignedUseCaseDetailsAsync_NoContent_ReturnsExpected()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        A.CallTo(() => _identity.CompanyId).Returns(companyId);
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(A<Guid>._, A<Guid>._))
            .Returns((false, true, true));

        // Act
        var result = await _sut.CreateCompanyAssignedUseCaseDetailsAsync(useCaseId);

        // Assert
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(companyId, useCaseId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRepository.CreateCompanyAssignedUseCase(companyId, useCaseId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCompanyAssignedUseCaseDetailsAsync_AlreadyReported_ReturnsExpected()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        A.CallTo(() => _identity.CompanyId).Returns(companyId);
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(A<Guid>._, A<Guid>._))
            .Returns((true, true, true));

        // Act
        var result = await _sut.CreateCompanyAssignedUseCaseDetailsAsync(useCaseId);

        // Assert
        result.Should().BeFalse();
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(companyId, useCaseId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRepository.CreateCompanyAssignedUseCase(companyId, useCaseId)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();

    }

    [Fact]
    public async Task CreateCompanyAssignedUseCaseDetailsAsync_ThrowsConflictException()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        A.CallTo(() => _identity.CompanyId).Returns(companyId);
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(A<Guid>._, A<Guid>._))
            .Returns((false, false, true));

        // Act
        async Task Act() => await _sut.CreateCompanyAssignedUseCaseDetailsAsync(useCaseId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Company Status is Incorrect");
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(companyId, useCaseId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RemoveCompanyAssignedUseCaseDetailsAsync_ReturnsExpected()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        A.CallTo(() => _identity.CompanyId).Returns(companyId);
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(A<Guid>._, A<Guid>._))
            .Returns((true, true, true));

        // Act
        await _sut.RemoveCompanyAssignedUseCaseDetailsAsync(useCaseId);

        // Assert
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(companyId, useCaseId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRepository.RemoveCompanyAssignedUseCase(companyId, useCaseId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RemoveCompanyAssignedUseCaseDetailsAsync_companyStatus_ThrowsConflictException()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        A.CallTo(() => _identity.CompanyId).Returns(companyId);
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(A<Guid>._, A<Guid>._))
            .Returns((true, false, true));

        // Act
        async Task Act() => await _sut.RemoveCompanyAssignedUseCaseDetailsAsync(useCaseId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Company Status is Incorrect");
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(companyId, useCaseId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RemoveCompanyAssignedUseCaseDetailsAsync_useCaseId_ThrowsConflictException()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        A.CallTo(() => _identity.CompanyId).Returns(companyId);
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(A<Guid>._, A<Guid>._))
            .Returns((false, true, true));

        // Act
        async Task Act() => await _sut.RemoveCompanyAssignedUseCaseDetailsAsync(useCaseId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"UseCaseId {useCaseId} is not available");
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(companyId, useCaseId)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region CompanyCertificate

    [Fact]
    public async Task CreateCompanyCertificate_WithInvalidDocumentContentType_ThrowsUnsupportedMediaTypeException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PNG.MapToMediaType());
        var data = new CompanyCertificateCreationData(CompanyCertificateTypeId.IATF, file, null, null, _now.AddMicroseconds(-1), _now.AddMicroseconds(1), null);

        // Act
        async Task Act() => await _sut.CreateCompanyCertificate(data, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Act);
        ex.Message.Should().Be($"Document type not supported. File must match contentTypes :{MediaTypeId.PDF.MapToMediaType()}");
    }

    [Fact]
    public async Task CheckCompanyCertificate_WithValidCall_CreatesExpected()
    {
        // Arrange
        SetupCreateCompanyCertificate();
        var validFrom = _now.AddMicroseconds(-1);
        var validTill = _now.AddMicroseconds(1);
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var externalCertificateNumber = "2345678";
        var sites = new[] { "BPNS00000003CRHK" };
        var data = new CompanyCertificateCreationData(CompanyCertificateTypeId.IATF, file, externalCertificateNumber, sites, validFrom, validTill, "Accenture");
        var documentId = Guid.NewGuid();
        var documents = new List<Document>();
        var companyCertificates = new List<CompanyCertificate>();

        A.CallTo(() => _companyCertificateRepository.CreateCompanyCertificate(_identity.CompanyId, A<CompanyCertificateTypeId>._, A<CompanyCertificateStatusId>._, A<Guid>._, A<Action<CompanyCertificate>>._))
            .Invokes((Guid companyId, CompanyCertificateTypeId companyCertificateTypeId, CompanyCertificateStatusId companyCertificateStatusId, Guid docId, Action<CompanyCertificate>? setOptionalFields) =>
            {
                var companyCertificateData = new CompanyCertificate(Guid.NewGuid(), companyCertificateTypeId, companyCertificateStatusId, companyId, docId);
                setOptionalFields?.Invoke(companyCertificateData);
                companyCertificates.Add(companyCertificateData);
            });
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, MediaTypeId.PDF, DocumentTypeId.COMPANY_CERTIFICATE, A<Action<Document>>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentTypeId, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, mediaTypeId, DateTimeOffset.UtcNow, DocumentStatusId.LOCKED, documentTypeId);
                setupOptionalFields?.Invoke(document);
                documents.Add(document);
            })
            .Returns(new Document(documentId, null!, null!, null!, default, default, default, default));

        // Act
        await _sut.CreateCompanyCertificate(data, CancellationToken.None);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, MediaTypeId.PDF, DocumentTypeId.COMPANY_CERTIFICATE, A<Action<Document>>._))
            .MustHaveHappenedOnceExactly();
        documents.Should().ContainSingle();
        var document = documents.Single();
        document.DocumentTypeId.Should().Be(DocumentTypeId.COMPANY_CERTIFICATE);
        document.DocumentStatusId.Should().Be(DocumentStatusId.LOCKED);
        A.CallTo(() => _companyCertificateRepository.CreateCompanyCertificate(_identity.CompanyId, CompanyCertificateTypeId.IATF, CompanyCertificateStatusId.ACTIVE, document.Id, A<Action<CompanyCertificate>>._))
            .MustHaveHappenedOnceExactly();
        companyCertificates.Should().ContainSingle();
        var detail = companyCertificates.Single();
        detail.CompanyCertificateStatusId.Should().Be(CompanyCertificateStatusId.ACTIVE);
        detail.DocumentId.Should().Be(document.Id);
        detail.CompanyCertificateTypeId.Should().Be(CompanyCertificateTypeId.IATF);
    }

    [Fact]
    public async Task CheckCompanyCertificateType_WithInvalidCall_ThrowsControllerArgumentException()
    {
        // Arrange
        SetupCreateCompanyCertificate();

        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var data = new CompanyCertificateCreationData(CompanyCertificateTypeId.IATF, file, null, null, _now.AddMicroseconds(-1), _now.AddMicroseconds(1), null);

        A.CallTo(() => _companyCertificateRepository.CheckCompanyCertificateType(CompanyCertificateTypeId.IATF))
        .Returns(false);

        // Act
        async Task Act() => await _sut.CreateCompanyCertificate(data, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"{CompanyCertificateTypeId.IATF} is not assigned to a certificate");
    }

    [Fact]
    public async Task CheckCompanyCertificateType_WithInvalidCall_ThrowsControllerArgumentExceptionForExternalCertificateNumber()
    {
        // Arrange
        SetupCreateCompanyCertificate();
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var externalCertificateNumber = "E4567@";
        var data = new CompanyCertificateCreationData(CompanyCertificateTypeId.IATF, file, externalCertificateNumber, null, _now.AddMicroseconds(-1), _now.AddMicroseconds(1), null);

        A.CallTo(() => _companyCertificateRepository.CheckCompanyCertificateType(CompanyCertificateTypeId.IATF))
        .Returns(false);

        // Act
        async Task Act() => await _sut.CreateCompanyCertificate(data, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"ExternalCertificateNumber must be alphanumeric and length should not be greater than 36");
    }

    [Fact]
    public async Task CheckCompanyCertificateType_WithInvalidCall_ThrowsControllerBPNS()
    {
        // Arrange
        SetupCreateCompanyCertificate();
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var externalCertificateNumber = "2345678";
        var sites = new[] { "BPNL00000003CRHK" };
        var data = new CompanyCertificateCreationData(CompanyCertificateTypeId.IATF, file, externalCertificateNumber, sites, _now.AddMicroseconds(-1), _now.AddMicroseconds(1), null);

        A.CallTo(() => _companyCertificateRepository.CheckCompanyCertificateType(CompanyCertificateTypeId.IATF))
        .Returns(false);

        // Act
        async Task Act() => await _sut.CreateCompanyCertificate(data, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"BPN must contain exactly 16 characters and must be prefixed with BPNS");
    }

    [Fact]
    public async Task CheckCompanyCertificateType_WithInvalidCall_ThrowsControllerForValidFromDate()
    {
        // Arrange
        SetupCreateCompanyCertificate();
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var externalCertificateNumber = "2345678";
        var sites = new[] { "BPNS00000003CRHK" };
        var data = new CompanyCertificateCreationData(CompanyCertificateTypeId.IATF, file, externalCertificateNumber, sites, _now.AddMicroseconds(1), _now.AddMicroseconds(2), null);

        A.CallTo(() => _companyCertificateRepository.CheckCompanyCertificateType(CompanyCertificateTypeId.IATF))
        .Returns(false);

        // Act
        async Task Act() => await _sut.CreateCompanyCertificate(data, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"ValidFrom date should not be greater than current date");
    }

    [Fact]
    public async Task CheckCompanyCertificateType_WithInvalidCall_ThrowsControllerForValidTillDate()
    {
        // Arrange
        SetupCreateCompanyCertificate();
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var externalCertificateNumber = "2345678";
        var sites = new[] { "BPNS00000003CRHK" };
        var data = new CompanyCertificateCreationData(CompanyCertificateTypeId.IATF, file, externalCertificateNumber, sites, _now.AddMicroseconds(-1), _now.AddMicroseconds(-1), null);

        A.CallTo(() => _companyCertificateRepository.CheckCompanyCertificateType(CompanyCertificateTypeId.IATF))
        .Returns(false);

        // Act
        async Task Act() => await _sut.CreateCompanyCertificate(data, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"ValidTill date should be greater than current date");
    }

    [Fact]
    public async Task CheckCompanyCertificateType_WithInvalidCall_ThrowsControllerForIssuer()
    {
        // Arrange
        SetupCreateCompanyCertificate();
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var externalCertificateNumber = "2345678";
        var sites = new[] { "BPNS00000003CRHK" };
        var data = new CompanyCertificateCreationData(CompanyCertificateTypeId.IATF, file, externalCertificateNumber, sites, _now.AddMicroseconds(-1), _now.AddMicroseconds(1), " +ACC");

        A.CallTo(() => _companyCertificateRepository.CheckCompanyCertificateType(CompanyCertificateTypeId.IATF))
        .Returns(false);

        // Act
        async Task Act() => await _sut.CreateCompanyCertificate(data, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(ValidationExpressionErrors.INCORRECT_COMPANY_NAME.ToString());
    }

    #endregion

    #region GetCompanyCertificateWithBpnNumber

    [Fact]
    public async Task GetCompanyCertificateWithNullOrEmptyBpn_ReturnsExpected()
    {
        // Act
        async Task Act() => await _sut.GetCompanyCertificatesByBpn(string.Empty).ToListAsync();

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        error.Message.Should().StartWith("businessPartnerNumber must not be empty");
    }

    [Fact]
    public async Task GetCompanyCertificateWithNoCompanyId_ReturnsExpected()
    {
        // Arrange
        var companyId = Guid.Empty;
        var businessPartnerNumber = "BPNL07800HZ01644";

        A.CallTo(() => _companyCertificateRepository.GetCompanyIdByBpn(businessPartnerNumber))
            .Returns(companyId);

        // Act
        async Task Act() => await _sut.GetCompanyCertificatesByBpn(businessPartnerNumber).ToListAsync();

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        error.Message.Should().StartWith($"company does not exist for {businessPartnerNumber}");
    }

    [Fact]
    public async Task GetCompanyCertificateWithBpnNumber_WithValidRequest_ReturnsExpected()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var data = _fixture.Build<CompanyCertificateBpnData>()
            .With(x => x.CompanyCertificateStatus, CompanyCertificateStatusId.ACTIVE)
            .With(x => x.CompanyCertificateType, CompanyCertificateTypeId.ISO_9001)
            .With(x => x.DocumentId, Guid.NewGuid())
            .With(x => x.ValidFrom, DateTime.UtcNow)
            .With(x => x.ValidTill, DateTime.UtcNow)
            .CreateMany(5).ToAsyncEnumerable();
        A.CallTo(() => _companyCertificateRepository.GetCompanyIdByBpn("BPNL07800HZ01643"))
            .Returns(companyId);
        A.CallTo(() => _companyCertificateRepository.GetCompanyCertificateData(companyId))
           .Returns(data);

        // Act
        var result = await _sut.GetCompanyCertificatesByBpn("BPNL07800HZ01643").ToListAsync();

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetCompanyCertificateWithBpnNumber_WithEmptyResult_ReturnsExpected()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        A.CallTo(() => _companyCertificateRepository.GetCompanyIdByBpn("BPNL07800HZ01643"))
            .Returns(companyId);

        // Act
        var result = await _sut.GetCompanyCertificatesByBpn("BPNL07800HZ01643").ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetAllCompanyCertificates

    [Theory]
    [InlineData(10, 0, 7, 7)]
    [InlineData(10, 1, 7, 3)]
    [InlineData(10, 0, 15, 10)]
    public async Task GetAllCompanyCertificatesAsync_GetsExpectedEntries(int num, int page, int requested, int expected)
    {
        // Arrange
        SetupPagination(num);

        // Act
        var result = await _sut.GetAllCompanyCertificatesAsync(page, requested, null, null, null);

        // Assert
        result.Content.Should().HaveCount(expected);
    }

    #endregion

    #region GetCompanyCertificateDocumentByCompanyId

    [Fact]
    public async Task GetCompanyCertificateDocumentByCompanyIdAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        SetupFakesForGetDocument();

        // Act
        var result = await _sut.GetCompanyCertificateDocumentByCompanyIdAsync(_validDocumentId);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be("test.pdf");
        result.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task GetCompanyCertificateDocumentByCompanyIdAsync_WithNotExistingDocument_ThrowsNotFoundException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        SetupFakesForGetDocument();

        // Act
        async Task Act() => await _sut.GetCompanyCertificateDocumentByCompanyIdAsync(documentId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Company certificate document {documentId} does not exist");
    }

    #endregion

    #region GetCompanyCertificateDocuments

    [Fact]
    public async Task GetCompanyCertificateDocumentAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        SetupFakesForGetDocument();

        // Act
        var result = await _sut.GetCompanyCertificateDocumentAsync(_validDocumentId);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be("test.pdf");
        result.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task GetCompanyCertificateDocumentAsync_WithNotExistingDocument_ThrowsNotFoundException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        SetupFakesForGetDocument();

        // Act
        async Task Act() => await _sut.GetCompanyCertificateDocumentAsync(documentId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Company certificate document {documentId} does not exist");
    }

    [Fact]
    public async Task GetCompanyCertificateDocumentAsync_WithDocumentStatusIsNotLocked_ThrowsNotFoundException()
    {
        // Arrange
        var documentId = new Guid("aaf53459-c36b-408e-a805-0b406ce9751d");
        SetupFakesForGetDocument();

        // Act
        async Task Act() => await _sut.GetCompanyCertificateDocumentAsync(documentId);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"Document {documentId} status is not locked");
    }

    #endregion

    #region GetCompaniesWithMissingSdDocument

    [Fact]
    public async Task GetCompaniesWithMissingSdDocument_WithMoreData_ReturnsExpected()
    {
        // Arrange
        SetupFakesForGetMissingSdDocCompanies(15);

        // Act
        var result = await _sut.GetCompaniesWithMissingSdDocument(0, 10);

        // Assert
        result.Should().NotBeNull();
        result.Content.Count().Should().Be(10);
    }

    [Fact]
    public async Task GetCompaniesWithMissingSdDocument_WithLessData_ReturnsExpected()
    {
        // Arrange
        SetupFakesForGetMissingSdDocCompanies(7);

        // Act
        var result = await _sut.GetCompaniesWithMissingSdDocument(0, 10);

        // Assert
        result.Should().NotBeNull();
        result.Content.Count().Should().Be(7);
    }

    #endregion

    #region DeleteCompanyCertificates

    [Fact]
    public async Task DeleteCompanyCertificateAsync_WithDocumentNotExisting_ThrowsNotFoundException()
    {
        // Arrange
        A.CallTo(() => _companyCertificateRepository.GetCompanyCertificateDocumentDetailsForIdUntrackedAsync(Guid.NewGuid(), _identity.CompanyId))
            .Returns((Guid.NewGuid(), DocumentStatusId.LOCKED, new[] { Guid.NewGuid() }.AsEnumerable(), false));

        // Act
        async Task Act() => await _sut.DeleteCompanyCertificateAsync(Guid.NewGuid());

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Document is not existing");
    }

    [Fact]
    public async Task DeleteCompanyCertificateAsync_WithDifferentCompanyIdNotExisting_ThrowsNotFoundException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        A.CallTo(() => _companyCertificateRepository.GetCompanyCertificateDocumentDetailsForIdUntrackedAsync(documentId, _identity.CompanyId))
            .Returns((documentId, DocumentStatusId.LOCKED, new[] { Guid.NewGuid() }.AsEnumerable(), false));

        // Act
        async Task Act() => await _sut.DeleteCompanyCertificateAsync(documentId);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"User is not allowed to delete this document");
    }

    [Fact]
    public async Task DeleteCompanyCertificateAsync_WithHavingMoreThanOneComapnyCertificate_ConflictException()
    {
        // Arrange
        var documentId = new Guid("aaf53459-c36b-408e-a805-0b406ce9751f");
        A.CallTo(() => _companyCertificateRepository.GetCompanyCertificateDocumentDetailsForIdUntrackedAsync(documentId, _identity.CompanyId))
            .Returns((documentId, DocumentStatusId.LOCKED, new[] { new Guid("9f5b9934-4014-4099-91e9-7b1aee696c10"), Guid.NewGuid() }.AsEnumerable(), true));

        // Act
        async Task Act() => await _sut.DeleteCompanyCertificateAsync(documentId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"There must not be multiple active certificates for document {documentId}");
    }

    [Fact]
    public async Task DeleteCompanyCertificateAsync_WithExpectedResult()
    {
        //Arrange        
        var documentId = new Guid("aaf53459-c36b-408e-a805-0b406ce9751f");
        A.CallTo(() => _companyCertificateRepository.GetCompanyCertificateDocumentDetailsForIdUntrackedAsync(documentId, _identity.CompanyId))
           .Returns((documentId, DocumentStatusId.LOCKED, new[] { new Guid("9f5b9934-4014-4099-91e9-7b1aee696c10") }.AsEnumerable(), true));

        //Act
        await _sut.DeleteCompanyCertificateAsync(documentId);

        //Assert
        A.CallTo(() => _companyCertificateRepository.AttachAndModifyCompanyCertificateDetails(A<Guid>._, null, A<Action<CompanyCertificate>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyCertificateRepository.AttachAndModifyCompanyCertificateDocumentDetails(documentId, null, A<Action<Document>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened(1, Times.OrMore);

    }

    #endregion

    #region TriggerSelfDescriptionCreation

    [Fact]
    public async Task TriggerSelfDescriptionCreation_WithMissingSdDocsForConnectorAndCompany_CallsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();
        var processes = new List<Process<ProcessTypeId, ProcessStepTypeId>>();
        var processSteps = new List<ProcessStep<ProcessTypeId, ProcessStepTypeId>>();
        A.CallTo(() => _processStepRepository.CreateProcess(A<ProcessTypeId>._))
            .Invokes((ProcessTypeId processTypeId) =>
            {
                processes.Add(new Process<ProcessTypeId, ProcessStepTypeId>(processId, processTypeId, Guid.NewGuid()));
            })
            .Returns(new Process<ProcessTypeId, ProcessStepTypeId>(processId, default, default));
        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._, A<ProcessStepStatusId>._, processId))
            .Invokes((ProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid _) =>
            {
                processSteps.Add(new ProcessStep<ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), processStepTypeId, processStepStatusId, processId, DateTimeOffset.UtcNow));
            });
        A.CallTo(() => _companyRepository.GetCompanyIdsWithMissingSelfDescription())
            .Returns(new[] { Guid.NewGuid(), Guid.NewGuid() }.ToAsyncEnumerable());

        // Act
        await _sut.TriggerSelfDescriptionCreation();

        // Assert
        processes.Should().NotBeNull()
            .And.HaveCount(2).And.Satisfy(
                p => p.ProcessTypeId == ProcessTypeId.SELF_DESCRIPTION_CREATION,
                    p => p.ProcessTypeId == ProcessTypeId.SELF_DESCRIPTION_CREATION);
        processSteps.Should().NotBeNull().And.HaveCount(2)
            .And.Satisfy(
                p => p.ProcessStepTypeId == ProcessStepTypeId.SELF_DESCRIPTION_COMPANY_CREATION,
                p => p.ProcessStepTypeId == ProcessStepTypeId.SELF_DESCRIPTION_COMPANY_CREATION);
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, A<Action<Company>>._, A<Action<Company>>._))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task TriggerSelfDescriptionCreation_WithoutMissingSdDocsForCompany_CallsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();
        var processes = new List<Process<ProcessTypeId, ProcessStepTypeId>>();
        var processSteps = new List<ProcessStep<ProcessTypeId, ProcessStepTypeId>>();
        A.CallTo(() => _processStepRepository.CreateProcess(A<ProcessTypeId>._))
            .Invokes((ProcessTypeId processTypeId) =>
            {
                processes.Add(new Process<ProcessTypeId, ProcessStepTypeId>(processId, processTypeId, Guid.NewGuid()));
            })
            .Returns(new Process<ProcessTypeId, ProcessStepTypeId>(processId, default, default));
        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._, A<ProcessStepStatusId>._, processId))
            .Invokes((ProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid _) =>
            {
                processSteps.Add(new ProcessStep<ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), processStepTypeId, processStepStatusId, processId, DateTimeOffset.UtcNow));
            });
        A.CallTo(() => _companyRepository.GetCompanyIdsWithMissingSelfDescription())
            .Returns(Enumerable.Empty<Guid>().ToAsyncEnumerable());

        // Act
        await _sut.TriggerSelfDescriptionCreation();

        // Assert
        processes.Should().BeEmpty();
        processSteps.Should().BeEmpty();
    }

    #endregion

    #region GetDimServiceUrls

    [Fact]
    public async Task GetDimServiceUrls_WithValid_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _companyRepository.GetDimServiceUrls(A<Guid>._))
            .Returns(new ValueTuple<string?, string?, string?>("BPNL00012345677", "did:web:test.org:123234345", "https://example.org/service"));

        // Act
        var result = await _sut.GetDimServiceUrls();

        // Assert
        A.CallTo(() => _companyRepository.GetDimServiceUrls(_identity.CompanyId))
            .MustHaveHappenedOnceExactly();
        result.DecentralIdentityManagementAuthUrl.Should().Be("https://example.org/service/oauth/token");
        result.DecentralIdentityManagementServiceUrl.Should().Be("https://example.org/test");
    }

    [Fact]
    public async Task GetDimServiceUrls_WithoutBpn_ThrowsConflictException()
    {
        // Arrange
        A.CallTo(() => _companyRepository.GetDimServiceUrls(A<Guid>._))
            .Returns(new ValueTuple<string?, string?, string?>(null, null, null));

        // Act
        var result = await _sut.GetDimServiceUrls();

        // Assert
        result.Bpnl.Should().BeNull();
    }

    [Fact]
    public async Task GetDimServiceUrls_WithoutDid_ThrowsConflictException()
    {
        // Arrange
        A.CallTo(() => _companyRepository.GetDimServiceUrls(A<Guid>._))
            .Returns(new ValueTuple<string?, string?, string?>("BPNL00012345677", null, null));

        // Act
        var result = await _sut.GetDimServiceUrls();

        // Assert
        result.HolderDid.Should().BeNull();
    }

    [Fact]
    public async Task GetDimServiceUrls_WithoutWalletUrl_ThrowsConflictException()
    {
        // Arrange
        A.CallTo(() => _companyRepository.GetDimServiceUrls(A<Guid>._))
            .Returns(new ValueTuple<string?, string?, string?>("BPNL00012345677", "did:web:test.org:123234345", null));

        // Act
        var result = await _sut.GetDimServiceUrls();

        // Assert
        result.DecentralIdentityManagementAuthUrl.Should().BeNull();
    }

    #endregion

    #region Setup

    private void SetupCreateCompanyCertificate()
    {
        A.CallTo(() => _companyCertificateRepository.CheckCompanyCertificateType(CompanyCertificateTypeId.IATF))
            .Returns(true);
        A.CallTo(() => _companyCertificateRepository.CheckCompanyCertificateType(A<CompanyCertificateTypeId>.That.Matches(x => x != CompanyCertificateTypeId.IATF)))
            .Returns(false);
    }

    private void SetupPagination(int count = 5)
    {
        var companyCertificateDetailData = _fixture.CreateMany<CompanyCertificateData>(count);
        var paginationResult = (int skip, int take) => Task.FromResult(new Pagination.Source<CompanyCertificateData>(companyCertificateDetailData.Count(), companyCertificateDetailData.Skip(skip).Take(take)));

        A.CallTo(() => _companyCertificateRepository.GetActiveCompanyCertificatePaginationSource(A<CertificateSorting?>._, A<CompanyCertificateStatusId?>._, A<CompanyCertificateTypeId?>._, A<Guid>._))
            .Returns(paginationResult);

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyCertificateRepository>()).Returns(_companyCertificateRepository);
    }

    private void SetupFakesForGetDocument()
    {
        var content = new byte[7];
        A.CallTo(() => _companyCertificateRepository.GetCompanyCertificateDocumentByCompanyIdDataAsync(_validDocumentId, _identity.CompanyId, DocumentTypeId.COMPANY_CERTIFICATE))
            .ReturnsLazily(() => new ValueTuple<byte[], string, MediaTypeId, bool>(content, "test.pdf", MediaTypeId.PDF, true));
        A.CallTo(() => _companyCertificateRepository.GetCompanyCertificateDocumentDataAsync(_validDocumentId, DocumentTypeId.COMPANY_CERTIFICATE))
            .ReturnsLazily(() => new ValueTuple<byte[], string, MediaTypeId, bool, bool>(content, "test.pdf", MediaTypeId.PDF, true, true));
        A.CallTo(() => _companyCertificateRepository.GetCompanyCertificateDocumentDataAsync(new Guid("aaf53459-c36b-408e-a805-0b406ce9751d"), DocumentTypeId.COMPANY_CERTIFICATE))
            .ReturnsLazily(() => new ValueTuple<byte[], string, MediaTypeId, bool, bool>(content, "test1.pdf", MediaTypeId.PDF, true, false));
    }

    private void SetupFakesForGetMissingSdDocCompanies(int count = 5)
    {
        var companyMissingSdDocumentData = _fixture.CreateMany<CompanyMissingSdDocumentData>(count);
        var paginationResult = (int skip, int take) => Task.FromResult(new Pagination.Source<CompanyMissingSdDocumentData>(companyMissingSdDocumentData.Count(), companyMissingSdDocumentData.Skip(skip).Take(take)));

        A.CallTo(() => _companyRepository.GetCompaniesWithMissingSdDocument())!
            .Returns(paginationResult);
    }

    #endregion
}
