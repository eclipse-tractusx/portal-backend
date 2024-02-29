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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class CompanyDataBusinessLogicTests
{
    private readonly IIdentityData _identity;
    private readonly Guid _traceabilityExternalTypeDetailId = Guid.NewGuid();
    private readonly Guid _validCredentialId = Guid.NewGuid();
    private static readonly Guid ValidDocumentId = Guid.NewGuid();
    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IConsentRepository _consentRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ICompanyRolesRepository _companyRolesRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly ICompanySsiDetailsRepository _companySsiDetailsRepository;
    private readonly ICompanyCertificateRepository _companyCertificateRepository;
    private readonly IMailingService _mailingService;
    private readonly ICustodianService _custodianService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly CompanyDataBusinessLogic _sut;
    private readonly IIdentityService _identityService;

    public CompanyDataBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _consentRepository = A.Fake<IConsentRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _companyRolesRepository = A.Fake<ICompanyRolesRepository>();
        _companySsiDetailsRepository = A.Fake<ICompanySsiDetailsRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _languageRepository = A.Fake<ILanguageRepository>();
        _notificationRepository = A.Fake<INotificationRepository>();
        _companySsiDetailsRepository = A.Fake<ICompanySsiDetailsRepository>();
        _companyCertificateRepository = A.Fake<ICompanyCertificateRepository>();

        _mailingService = A.Fake<IMailingService>();
        _custodianService = A.Fake<ICustodianService>();
        _dateTimeProvider = A.Fake<IDateTimeProvider>();
        _identityService = A.Fake<IIdentityService>();
        _identity = A.Fake<IIdentityData>();

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentRepository>()).Returns(_consentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRolesRepository>()).Returns(_companyRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanySsiDetailsRepository>()).Returns(_companySsiDetailsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ILanguageRepository>()).Returns(_languageRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyCertificateRepository>()).Returns(_companyCertificateRepository);

        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

        var options = Options.Create(new CompanyDataSettings { MaxPageSize = 20, UseCaseParticipationMediaTypes = new[] { MediaTypeId.PDF }, SsiCertificateMediaTypes = new[] { MediaTypeId.PDF }, CompanyCertificateMediaTypes = new[] { MediaTypeId.PDF } });
        _sut = new CompanyDataBusinessLogic(_portalRepositories, _mailingService, _custodianService, _dateTimeProvider, _identityService, options);
    }

    #region GetOwnCompanyDetails

    [Fact]
    public async Task GetOwnCompanyDetailsAsync_ExpectedResults()
    {
        // Arrange
        var companyAddressDetailData = _fixture.Create<CompanyAddressDetailData>();
        A.CallTo(() => _companyRepository.GetCompanyDetailsAsync(_identity.CompanyId))
            .ReturnsLazily(() => companyAddressDetailData);

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
            .ReturnsLazily(() => (CompanyAddressDetailData?)null);

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
                    new (Guid.NewGuid(), _fixture.Create<string>(), Guid.NewGuid(), 0, _fixture.Create<string>()),
                    new (Guid.NewGuid(), _fixture.Create<string>(), Guid.NewGuid(), ConsentStatusId.ACTIVE, _fixture.Create<string>()),
                    new (Guid.NewGuid(), _fixture.Create<string>(), Guid.NewGuid(), ConsentStatusId.INACTIVE, _fixture.Create<string>()),
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
        var result = await _sut.GetCompanyRoleAndConsentAgreementDetailsAsync(languageShortName).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull()
            .And.HaveSameCount(companyRoleConsentData);

        result.Zip(companyRoleConsentData).Should()
            .AllSatisfy(x => x.Should().Match<(CompanyRoleConsentViewData Result, CompanyRoleConsentData Mock)>(
                z =>
                z.Result.CompanyRoleId == z.Mock.CompanyRoleId &&
                z.Result.RoleDescription == z.Mock.RoleDescription &&
                z.Result.CompanyRolesActive == z.Mock.CompanyRolesActive &&
                z.Result.Agreements.SequenceEqual(z.Mock.Agreements.Select(a => new ConsentAgreementViewData(a.AgreementId, a.AgreementName, a.DocumentId, a.ConsentStatus == 0 ? null : a.ConsentStatus, a.AgreementLink)))));

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
        async Task Act() => await _sut.GetCompanyRoleAndConsentAgreementDetailsAsync(languageShortName).ToListAsync().ConfigureAwait(false);

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
        async Task Act() => await _sut.GetCompanyRoleAndConsentAgreementDetailsAsync(languageShortName).ToListAsync().ConfigureAwait(false);

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
        async Task Act() => await _sut.GetCompanyRoleAndConsentAgreementDetailsAsync(languageShortName).ToListAsync().ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"language {languageShortName} is not a valid languagecode");

    }

    #endregion

    #region  CreateCompanyRoleAndConsentAgreementDetails

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
        await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails).ConfigureAwait(false);

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
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails).ConfigureAwait(false);

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

        var utcNow = DateTimeOffset.UtcNow;

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
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails).ConfigureAwait(false);

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
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails).ConfigureAwait(false);

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
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails).ConfigureAwait(false);

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
            .Returns(((bool, bool, IEnumerable<CompanyRoleId>?, IEnumerable<ConsentStatusDetails>?))default);

        // Act
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails).ConfigureAwait(false);

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
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails).ConfigureAwait(false);

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
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails).ConfigureAwait(false);

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
        var result = await _sut.GetCompanyAssigendUseCaseDetailsAsync().ToListAsync().ConfigureAwait(false);

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
        var result = await _sut.CreateCompanyAssignedUseCaseDetailsAsync(useCaseId).ConfigureAwait(false);

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
        var result = await _sut.CreateCompanyAssignedUseCaseDetailsAsync(useCaseId).ConfigureAwait(false);

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
        async Task Act() => await _sut.CreateCompanyAssignedUseCaseDetailsAsync(useCaseId).ConfigureAwait(false);

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
        await _sut.RemoveCompanyAssignedUseCaseDetailsAsync(useCaseId).ConfigureAwait(false);

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
        async Task Act() => await _sut.RemoveCompanyAssignedUseCaseDetailsAsync(useCaseId).ConfigureAwait(false);

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
        async Task Act() => await _sut.RemoveCompanyAssignedUseCaseDetailsAsync(useCaseId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"UseCaseId {useCaseId} is not available");
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(companyId, useCaseId)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetUseCaseParticipationAsync

    [Fact]
    public async Task GetUseCaseParticipationAsync_WithValidRequest_ReturnsExpected()
    {
        // Arrange
        var verifiedCredentials = _fixture.Build<CompanySsiExternalTypeDetailTransferData>()
            .With(x => x.SsiDetailData, _fixture.CreateMany<CompanySsiDetailTransferData>(1))
            .CreateMany(5);
        A.CallTo(() => _companySsiDetailsRepository.GetUseCaseParticipationForCompany(_identity.CompanyId, "en"))
            .Returns(_fixture.Build<UseCaseParticipationTransferData>().With(x => x.VerifiedCredentials, verifiedCredentials).CreateMany(5).ToAsyncEnumerable());

        // Act
        var result = await _sut.GetUseCaseParticipationAsync("en").ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetUseCaseParticipationAsync_WithMultipleSsiDetailData_ReturnsExpected()
    {
        // Arrange
        var verifiedCredentials = _fixture.Build<CompanySsiExternalTypeDetailTransferData>()
            .With(x => x.SsiDetailData, _fixture.CreateMany<CompanySsiDetailTransferData>(2))
            .CreateMany(4);
        A.CallTo(() => _companySsiDetailsRepository.GetUseCaseParticipationForCompany(_identity.CompanyId, "en"))
            .Returns(_fixture.Build<UseCaseParticipationTransferData>().With(x => x.VerifiedCredentials, verifiedCredentials).CreateMany(5).ToAsyncEnumerable());

        // Act
        var Act = () => _sut.GetUseCaseParticipationAsync("en");

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be("There should only be one pending or active ssi detail be assigned");
    }

    #endregion

    #region GetSsiCertificates

    [Fact]
    public async Task GetSsiCertificates_WithValidRequest_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _companySsiDetailsRepository.GetSsiCertificates(_identity.CompanyId))
            .Returns(_fixture.Build<SsiCertificateTransferData>().With(x => x.SsiDetailData, _fixture.CreateMany<CompanySsiDetailTransferData>(1)).CreateMany(5).ToAsyncEnumerable());

        // Act
        var result = await _sut.GetSsiCertificatesAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(5);
    }

    #endregion

    #region CreateUseCaseParticipation

    [Fact]
    public async Task CreateUseCaseParticipation_WithInvalidDocumentContentType_ThrowsUnsupportedMediaTypeException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PNG.MapToMediaType());
        var data = new UseCaseParticipationCreationData(_traceabilityExternalTypeDetailId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, file);

        // Act
        async Task Act() => await _sut.CreateUseCaseParticipation(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Act);
        ex.Message.Should().Be($"Document type not supported. File must match contentTypes :{MediaTypeId.PDF.MapToMediaType()}");
    }

    [Fact]
    public async Task CreateUseCaseParticipation_WithNotExistingDetailId_ThrowsControllerArgumentException()
    {
        // Arrange
        SetupCreateUseCaseParticipation();
        var verifiedCredentialExternalTypeDetailId = Guid.NewGuid();
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var data = new UseCaseParticipationCreationData(verifiedCredentialExternalTypeDetailId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, file);

        // Act
        async Task Act() => await _sut.CreateUseCaseParticipation(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"VerifiedCredentialExternalTypeDetail {verifiedCredentialExternalTypeDetailId} does not exist");
    }

    [Fact]
    public async Task CreateUseCaseParticipation_WithRequestAlreadyExisting_ThrowsControllerArgumentException()
    {
        // Arrange
        SetupCreateUseCaseParticipation();
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var data = new UseCaseParticipationCreationData(_traceabilityExternalTypeDetailId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, file);

        A.CallTo(() => _companySsiDetailsRepository.CheckSsiDetailsExistsForCompany(_identity.CompanyId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, VerifiedCredentialTypeKindId.USE_CASE, _traceabilityExternalTypeDetailId))
            .Returns(true);

        // Act
        async Task Act() => await _sut.CreateUseCaseParticipation(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("Credential request already existing");
    }

    [Fact]
    public async Task CreateUseCaseParticipation_WithValidCall_CreatesExpected()
    {
        // Arrange
        SetupCreateUseCaseParticipation();
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var data = new UseCaseParticipationCreationData(_traceabilityExternalTypeDetailId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, file);
        var documentId = Guid.NewGuid();
        var documents = new List<Document>();
        var ssiDetails = new List<CompanySsiDetail>();

        A.CallTo(() => _companySsiDetailsRepository.CheckSsiDetailsExistsForCompany(_identity.CompanyId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, VerifiedCredentialTypeKindId.USE_CASE, _traceabilityExternalTypeDetailId))
            .Returns(false);
        A.CallTo(() => _companySsiDetailsRepository.CreateSsiDetails(_identity.CompanyId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, A<Guid>._, CompanySsiDetailStatusId.PENDING, _identity.IdentityId, A<Action<CompanySsiDetail>>._))
            .Invokes((Guid companyId, VerifiedCredentialTypeId verifiedCredentialTypeId, Guid docId, CompanySsiDetailStatusId companySsiDetailStatusId, Guid userId, Action<CompanySsiDetail>? setOptionalFields) =>
            {
                var ssiDetail = new CompanySsiDetail(Guid.NewGuid(), companyId, verifiedCredentialTypeId, companySsiDetailStatusId, docId, userId, DateTimeOffset.UtcNow);
                setOptionalFields?.Invoke(ssiDetail);
                ssiDetails.Add(ssiDetail);
            });
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, MediaTypeId.PDF, DocumentTypeId.PRESENTATION, A<Action<Document>>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentTypeId, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, mediaTypeId, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentTypeId);
                setupOptionalFields?.Invoke(document);
                documents.Add(document);
            })
            .Returns(new Document(documentId, null!, null!, null!, default, default, default, default));

        // Act
        await _sut.CreateUseCaseParticipation(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, MediaTypeId.PDF, DocumentTypeId.PRESENTATION, A<Action<Document>>._))
            .MustHaveHappenedOnceExactly();
        documents.Should().ContainSingle();
        var document = documents.Single();
        document.DocumentTypeId.Should().Be(DocumentTypeId.PRESENTATION);
        document.CompanyUserId.Should().Be(_identity.IdentityId);
        document.DocumentStatusId.Should().Be(DocumentStatusId.PENDING);
        A.CallTo(() => _companySsiDetailsRepository.CreateSsiDetails(_identity.CompanyId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, document.Id, CompanySsiDetailStatusId.PENDING, _identity.IdentityId, A<Action<CompanySsiDetail>>._))
            .MustHaveHappenedOnceExactly();
        ssiDetails.Should().ContainSingle();
        var detail = ssiDetails.Single();
        detail.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.PENDING);
        detail.DocumentId.Should().Be(document.Id);
        detail.VerifiedCredentialTypeId.Should().Be(VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK);
        detail.ExpiryDate.Should().Be(null);
        detail.VerifiedCredentialExternalTypeUseCaseDetailId.Should().Be(_traceabilityExternalTypeDetailId);
    }

    #endregion

    #region CreateSsiCertificate

    [Fact]
    public async Task CreateSsiCertificate_WithInvalidDocumentContentType_ThrowsUnsupportedMediaTypeException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PNG.MapToMediaType());
        var data = new SsiCertificateCreationData(VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, file);

        // Act
        async Task Act() => await _sut.CreateSsiCertificate(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Act);
        ex.Message.Should().Be($"Document type not supported. File must match contentTypes :{MediaTypeId.PDF.MapToMediaType()}");
    }

    [Fact]
    public async Task CreateSsiCertificate_WithWrongVerifiedCredentialType_ThrowsControllerArgumentException()
    {
        // Arrange
        SetupCreateSsiCertificate();
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var data = new SsiCertificateCreationData(VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, file);

        // Act
        async Task Act() => await _sut.CreateSsiCertificate(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"{VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK} is not assigned to a certificate");
    }

    [Fact]
    public async Task CheckSsiDetailsExistsForCompany_WithRequestAlreadyExisting_ThrowsControllerArgumentException()
    {
        // Arrange
        SetupCreateSsiCertificate();
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var data = new SsiCertificateCreationData(VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, file);

        A.CallTo(() => _companySsiDetailsRepository.CheckSsiDetailsExistsForCompany(_identity.CompanyId, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, VerifiedCredentialTypeKindId.CERTIFICATE, null))
            .Returns(true);

        // Act
        async Task Act() => await _sut.CreateSsiCertificate(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("Credential request already existing");
    }

    [Fact]
    public async Task CreateSsiCertificate_WithValidCall_CreatesExpected()
    {
        // Arrange
        SetupCreateSsiCertificate();
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var data = new SsiCertificateCreationData(VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, file);
        var documentId = Guid.NewGuid();
        var documents = new List<Document>();
        var ssiDetails = new List<CompanySsiDetail>();

        A.CallTo(() => _companySsiDetailsRepository.CheckSsiDetailsExistsForCompany(_identity.CompanyId, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, VerifiedCredentialTypeKindId.CERTIFICATE, null))
            .Returns(false);
        A.CallTo(() => _companySsiDetailsRepository.CreateSsiDetails(_identity.CompanyId, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, A<Guid>._, CompanySsiDetailStatusId.PENDING, _identity.IdentityId, A<Action<CompanySsiDetail>>._))
            .Invokes((Guid companyId, VerifiedCredentialTypeId verifiedCredentialTypeId, Guid docId, CompanySsiDetailStatusId companySsiDetailStatusId, Guid userId, Action<CompanySsiDetail>? setOptionalFields) =>
            {
                var ssiDetail = new CompanySsiDetail(Guid.NewGuid(), companyId, verifiedCredentialTypeId, companySsiDetailStatusId, docId, userId, DateTimeOffset.UtcNow);
                setOptionalFields?.Invoke(ssiDetail);
                ssiDetails.Add(ssiDetail);
            });
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, MediaTypeId.PDF, DocumentTypeId.PRESENTATION, A<Action<Document>>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentTypeId, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, mediaTypeId, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentTypeId);
                setupOptionalFields?.Invoke(document);
                documents.Add(document);
            })
            .Returns(new Document(documentId, null!, null!, null!, default, default, default, default));

        // Act
        await _sut.CreateSsiCertificate(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, MediaTypeId.PDF, DocumentTypeId.PRESENTATION, A<Action<Document>>._))
            .MustHaveHappenedOnceExactly();
        documents.Should().ContainSingle();
        var document = documents.Single();
        document.DocumentTypeId.Should().Be(DocumentTypeId.PRESENTATION);
        document.CompanyUserId.Should().Be(_identity.IdentityId);
        document.DocumentStatusId.Should().Be(DocumentStatusId.PENDING);
        A.CallTo(() => _companySsiDetailsRepository.CreateSsiDetails(_identity.CompanyId, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, document.Id, CompanySsiDetailStatusId.PENDING, _identity.IdentityId, A<Action<CompanySsiDetail>>._))
            .MustHaveHappenedOnceExactly();
        ssiDetails.Should().ContainSingle();
        var detail = ssiDetails.Single();
        detail.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.PENDING);
        detail.DocumentId.Should().Be(document.Id);
        detail.VerifiedCredentialTypeId.Should().Be(VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE);
        detail.ExpiryDate.Should().Be(null);
        detail.VerifiedCredentialExternalTypeUseCaseDetailId.Should().BeNull();
    }

    #endregion

    #region CompanyCertificate

    [Fact]
    public async Task CreateCompanyCertificate_WithInvalidDocumentContentType_ThrowsUnsupportedMediaTypeException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PNG.MapToMediaType());
        var data = new CompanyCertificateCreationData(CompanyCertificateTypeId.IATF, file, DateTime.UtcNow);

        // Act
        async Task Act() => await _sut.CreateCompanyCertificate(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Act);
        ex.Message.Should().Be($"Document type not supported. File must match contentTypes :{MediaTypeId.PDF.MapToMediaType()}");
    }

    [Fact]
    public async Task CheckCompanyCertificate_WithValidCall_CreatesExpected()
    {
        // Arrange
        SetupCreateCompanyCertificate();
        var expiryDate = DateTime.UtcNow;
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var data = new CompanyCertificateCreationData(CompanyCertificateTypeId.IATF, file, expiryDate);
        var documentId = Guid.NewGuid();
        var documents = new List<Document>();
        var companyCertificates = new List<CompanyCertificate>();

        A.CallTo(() => _companyCertificateRepository.CreateCompanyCertificate(_identity.CompanyId, CompanyCertificateTypeId.IATF, A<Guid>._, A<Action<CompanyCertificate>>._))
            .Invokes((Guid companyId, CompanyCertificateTypeId companyCertificateTypeId, Guid docId, Action<CompanyCertificate>? setOptionalFields) =>
            {
                var companyCertificateData = new CompanyCertificate(Guid.NewGuid(), DateTime.UtcNow, companyCertificateTypeId, CompanyCertificateStatusId.ACTIVE, companyId, docId);
                setOptionalFields?.Invoke(companyCertificateData);
                companyCertificates.Add(companyCertificateData);
            });
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, MediaTypeId.PDF, DocumentTypeId.COMPANY_CERTIFICATE, A<Action<Document>>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentTypeId, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, mediaTypeId, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentTypeId);
                setupOptionalFields?.Invoke(document);
                documents.Add(document);
            })
            .Returns(new Document(documentId, null!, null!, null!, default, default, default, default));

        // Act
        await _sut.CreateCompanyCertificate(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, MediaTypeId.PDF, DocumentTypeId.COMPANY_CERTIFICATE, A<Action<Document>>._))
            .MustHaveHappenedOnceExactly();
        documents.Should().ContainSingle();
        var document = documents.Single();
        document.DocumentTypeId.Should().Be(DocumentTypeId.COMPANY_CERTIFICATE);
        document.DocumentStatusId.Should().Be(DocumentStatusId.PENDING);
        A.CallTo(() => _companyCertificateRepository.CreateCompanyCertificate(_identity.CompanyId, CompanyCertificateTypeId.IATF, document.Id, A<Action<CompanyCertificate>>._))
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
        var data = new CompanyCertificateCreationData(CompanyCertificateTypeId.IATF, file, DateTime.UtcNow);

        A.CallTo(() => _companyCertificateRepository.CheckCompanyCertificateType(CompanyCertificateTypeId.IATF))
        .Returns(false);

        // Act
        async Task Act() => await _sut.CreateCompanyCertificate(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"{CompanyCertificateTypeId.IATF} is not assigned to a certificate");
    }
    #endregion

    #region GetCompanyCertificateWithBpnNumber

    [Fact]
    public async Task GetCompanyCertificateWithNullOrEmptyBpn_ReturnsExpected()
    {
        // Act
        async Task Act() => await _sut.GetCompanyCertificatesByBpn(string.Empty).ToListAsync().ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
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
        async Task Act() => await _sut.GetCompanyCertificatesByBpn(businessPartnerNumber).ToListAsync().ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
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
        var result = await _sut.GetCompanyCertificatesByBpn("BPNL07800HZ01643").ToListAsync().ConfigureAwait(false);

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
        var result = await _sut.GetCompanyCertificatesByBpn("BPNL07800HZ01643").ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetCredentials

    [Fact]
    public async Task GetCredentials_WithFilter_ReturnsList()
    {
        // Arrange
        var companySsiDetailStatusId = _fixture.Create<CompanySsiDetailStatusId>();
        var credentialTypeId = _fixture.Create<VerifiedCredentialTypeId>();
        var companyName = _fixture.Create<string>();
        var verificationCredentialType = _fixture.Build<VerifiedCredentialType>()
            .With(x => x.VerifiedCredentialTypeAssignedUseCase, new VerifiedCredentialTypeAssignedUseCase(VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, Guid.NewGuid())
            {
                UseCase = new UseCase(Guid.NewGuid(), "test", "name")
            })
            .Create();
        var data = _fixture.Build<CompanySsiDetail>()
            .With(x => x.VerifiedCredentialType, verificationCredentialType)
            .With(x => x.Document, new Document(Guid.NewGuid(), null!, null!, "test-doc.pdf", MediaTypeId.PDF, default, default, default))
            .CreateMany(3);
        var credentials = new AsyncEnumerableStub<CompanySsiDetail>(data);
        A.CallTo(() => _companySsiDetailsRepository.GetAllCredentialDetails(A<CompanySsiDetailStatusId>._, A<VerifiedCredentialTypeId?>._, A<string?>._))
            .Returns(credentials.AsQueryable());

        // Act
        var result = await _sut.GetCredentials(0, 15, companySsiDetailStatusId, credentialTypeId, companyName, CompanySsiDetailSorting.CompanyAsc).ConfigureAwait(false);

        // Assert
        result.Content.Should().HaveCount(credentials.Count());
        A.CallTo(() => _companySsiDetailsRepository.GetAllCredentialDetails(companySsiDetailStatusId, credentialTypeId, companyName))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region ApproveCredential

    [Fact]
    public async Task ApproveCredential_WithoutExistingSsiDetail_ThrowsNotFoundException()
    {
        // Arrange
        var notExistingId = Guid.NewGuid();
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(notExistingId))
            .Returns(new ValueTuple<bool, SsiApprovalData>());
        async Task Act() => await _sut.ApproveCredential(notExistingId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be($"CompanySsiDetail {notExistingId} does not exists");
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "CredentialApproval" })))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(CompanySsiDetailStatusId.ACTIVE)]
    [InlineData(CompanySsiDetailStatusId.INACTIVE)]
    public async Task ApproveCredential_WithStatusNotPending_ThrowsConflictException(CompanySsiDetailStatusId statusId)
    {
        // Arrange
        var alreadyActiveId = Guid.NewGuid();
        var approvalData = _fixture.Build<SsiApprovalData>()
            .With(x => x.Status, statusId)
            .Create();
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(alreadyActiveId))
            .Returns(new ValueTuple<bool, SsiApprovalData>(true, approvalData));
        async Task Act() => await _sut.ApproveCredential(alreadyActiveId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be($"Credential {alreadyActiveId} must be {CompanySsiDetailStatusId.PENDING}");
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "CredentialApproval" })))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task ApproveCredential_WithBpnNotSetActiveSsiDetail_ThrowsConflictException()
    {
        // Arrange
        var alreadyActiveId = Guid.NewGuid();
        var approvalData = _fixture.Build<SsiApprovalData>()
            .With(x => x.Status, CompanySsiDetailStatusId.PENDING)
            .With(x => x.Bpn, (string?)null)
            .With(x => x.CompanyName, "Test Company")
            .Create();
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(alreadyActiveId))
            .Returns(new ValueTuple<bool, SsiApprovalData>(true, approvalData));
        async Task Act() => await _sut.ApproveCredential(alreadyActiveId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be($"Bpn should be set for company {approvalData.CompanyName}");
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "CredentialApproval" })))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task ApproveCredential_WithoutExternalDetailSet_ThrowsConflictException()
    {
        // Arrange
        var alreadyActiveId = Guid.NewGuid();
        var approvalData = _fixture.Build<SsiApprovalData>()
            .With(x => x.Status, CompanySsiDetailStatusId.PENDING)
            .With(x => x.Bpn, "test")
            .With(x => x.CompanyName, "Test Company")
            .With(x => x.Kind, VerifiedCredentialTypeKindId.USE_CASE)
            .With(x => x.UseCaseDetailData, (UseCaseDetailData?)null)
            .Create();
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(alreadyActiveId))
            .Returns(new ValueTuple<bool, SsiApprovalData>(true, approvalData));
        async Task Act() => await _sut.ApproveCredential(alreadyActiveId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be("The VerifiedCredentialExternalTypeUseCaseDetail must be set");
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "CredentialApproval" })))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(VerifiedCredentialTypeKindId.USE_CASE, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK)]
    [InlineData(VerifiedCredentialTypeKindId.CERTIFICATE, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE)]
    public async Task ApproveCredential_WithValidRequest_ReturnsExpected(VerifiedCredentialTypeKindId kindId, VerifiedCredentialTypeId typeId)
    {
        // Arrange
        const string recipientMail = "test@mail.com";
        var now = DateTimeOffset.UtcNow;
        var requesterId = Guid.NewGuid();
        var notifications = new List<Notification>();
        UseCaseDetailData? useCaseData = null;
        if (kindId == VerifiedCredentialTypeKindId.USE_CASE)
        {
            useCaseData = new UseCaseDetailData(
                VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL,
                "test",
                "1.0.0"
            );
        }

        var bpn = "BPNL00000001TEST";
        var data = new SsiApprovalData(
            CompanySsiDetailStatusId.PENDING,
            typeId,
            kindId,
            "Stark Industries",
            bpn,
            null,
            useCaseData,
            new SsiRequesterData(
                requesterId,
                recipientMail,
                "Tony",
                "Stark"
            )
        );

        var detail = new CompanySsiDetail(_validCredentialId, _identity.CompanyId, typeId, CompanySsiDetailStatusId.PENDING, Guid.NewGuid(), requesterId, DateTimeOffset.Now);
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(_validCredentialId))
            .Returns(new ValueTuple<bool, SsiApprovalData>(true, data));
        A.CallTo(() => _notificationRepository.CreateNotification(requesterId, NotificationTypeId.CREDENTIAL_APPROVAL, false, A<Action<Notification>?>._))
            .Invokes((Guid receiverUserId, NotificationTypeId notificationTypeId, bool isRead, Action<Notification>? setOptionalParameters) =>
            {
                var notification = new Notification(Guid.NewGuid(), receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameters?.Invoke(notification);
                notifications.Add(notification);
            });
        A.CallTo(() => _companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(_validCredentialId, A<Action<CompanySsiDetail>?>._, A<Action<CompanySsiDetail>>._!))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> updateFields) =>
            {
                initialize?.Invoke(detail);
                updateFields.Invoke(detail);
            });

        // Act
        await _sut.ApproveCredential(_validCredentialId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _mailingService.SendMails(recipientMail, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "CredentialApproval" })))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        if (kindId == VerifiedCredentialTypeKindId.USE_CASE)
        {
            A.CallTo(() => _custodianService.TriggerFrameworkAsync(bpn, A<UseCaseDetailData>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _custodianService.TriggerDismantlerAsync(bpn, typeId, A<CancellationToken>._))
                .MustNotHaveHappened();
        }
        else
        {
            A.CallTo(() => _custodianService.TriggerFrameworkAsync(bpn, A<UseCaseDetailData>._, A<CancellationToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _custodianService.TriggerDismantlerAsync(bpn, typeId, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        notifications.Should().ContainSingle();
        var notification = notifications.Single();
        notification.NotificationTypeId.Should().Be(NotificationTypeId.CREDENTIAL_APPROVAL);
        notification.CreatorUserId.Should().Be(_identity.IdentityId);

        detail.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.ACTIVE);
        detail.DateLastChanged.Should().Be(now);
    }

    [Fact]
    public async Task ApproveCredential_WithInvalidCredentialType_ThrowsException()
    {
        // Arrange
        const string recipientMail = "test@mail.com";
        var now = DateTimeOffset.UtcNow;
        var requesterId = Guid.NewGuid();
        var useCaseData = new UseCaseDetailData(
            VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL,
            "test",
            "1.0.0"
        );

        var bpn = "BPNL00000001TEST";
        var data = new SsiApprovalData(
            CompanySsiDetailStatusId.PENDING,
            default,
            VerifiedCredentialTypeKindId.USE_CASE,
            "Stark Industries",
            bpn,
            null,
            useCaseData,
            new SsiRequesterData(
                requesterId,
                recipientMail,
                "Tony",
                "Stark"
            )
        );

        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(_validCredentialId))
            .Returns(new ValueTuple<bool, SsiApprovalData>(true, data));

        // Act
        async Task Act() => await _sut.ApproveCredential(_validCredentialId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be("VerifiedCredentialType 0 does not exists");
    }

    [Theory]
    [InlineData(VerifiedCredentialTypeKindId.USE_CASE, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK)]
    [InlineData(VerifiedCredentialTypeKindId.CERTIFICATE, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE)]
    public async Task ApproveCredential_WithoutUserMail_ReturnsExpected(VerifiedCredentialTypeKindId kindId, VerifiedCredentialTypeId typeId)
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var requesterId = Guid.NewGuid();
        var notifications = new List<Notification>();
        UseCaseDetailData? useCaseData = null;
        if (kindId == VerifiedCredentialTypeKindId.USE_CASE)
        {
            useCaseData = new UseCaseDetailData(
                VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL,
                "test",
                "1.0.0"
            );
        }

        var bpn = "BPNL00000001TEST";
        var data = new SsiApprovalData(
            CompanySsiDetailStatusId.PENDING,
            typeId,
            kindId,
            "Stark Industries",
            bpn,
            null,
            useCaseData,
            new SsiRequesterData(
                requesterId,
                null,
                null,
                null
            )
        );

        var detail = new CompanySsiDetail(_validCredentialId, _identity.CompanyId, typeId, CompanySsiDetailStatusId.PENDING, Guid.NewGuid(), requesterId, DateTimeOffset.Now);
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(_validCredentialId))
            .Returns(new ValueTuple<bool, SsiApprovalData>(true, data));
        A.CallTo(() => _notificationRepository.CreateNotification(requesterId, NotificationTypeId.CREDENTIAL_APPROVAL, false, A<Action<Notification>?>._))
            .Invokes((Guid receiverUserId, NotificationTypeId notificationTypeId, bool isRead, Action<Notification>? setOptionalParameters) =>
            {
                var notification = new Notification(Guid.NewGuid(), receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameters?.Invoke(notification);
                notifications.Add(notification);
            });
        A.CallTo(() => _companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(_validCredentialId, A<Action<CompanySsiDetail>?>._, A<Action<CompanySsiDetail>>._!))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> updateFields) =>
            {
                initialize?.Invoke(detail);
                updateFields.Invoke(detail);
            });

        // Act
        await _sut.ApproveCredential(_validCredentialId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "CredentialRejected" })))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        if (kindId == VerifiedCredentialTypeKindId.USE_CASE)
        {
            A.CallTo(() => _custodianService.TriggerFrameworkAsync(bpn, A<UseCaseDetailData>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _custodianService.TriggerDismantlerAsync(bpn, typeId, A<CancellationToken>._))
                .MustNotHaveHappened();
        }
        else
        {
            A.CallTo(() => _custodianService.TriggerFrameworkAsync(bpn, A<UseCaseDetailData>._, A<CancellationToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _custodianService.TriggerDismantlerAsync(bpn, typeId, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        notifications.Should().ContainSingle();
        var notification = notifications.Single();
        notification.NotificationTypeId.Should().Be(NotificationTypeId.CREDENTIAL_APPROVAL);
        notification.CreatorUserId.Should().Be(_identity.IdentityId);

        detail.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.ACTIVE);
        detail.DateLastChanged.Should().Be(now);
    }

    #endregion

    #region RejectCredential

    [Fact]
    public async Task RejectCredential_WithoutExistingSsiDetail_ThrowsNotFoundException()
    {
        // Arrange
        var notExistingId = Guid.NewGuid();
        A.CallTo(() => _companySsiDetailsRepository.GetSsiRejectionData(notExistingId))
            .Returns(new ValueTuple<bool, CompanySsiDetailStatusId, VerifiedCredentialTypeId, Guid, string?, string?, string?>());
        async Task Act() => await _sut.RejectCredential(notExistingId).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be($"CompanySsiDetail {notExistingId} does not exists");
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "CredentialRejected" })))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(CompanySsiDetailStatusId.ACTIVE)]
    [InlineData(CompanySsiDetailStatusId.INACTIVE)]
    public async Task RejectCredential_WithNotPendingSsiDetail_ThrowsNotFoundException(CompanySsiDetailStatusId status)
    {
        // Arrange
        var alreadyInactiveId = Guid.NewGuid();
        A.CallTo(() => _companySsiDetailsRepository.GetSsiRejectionData(alreadyInactiveId))
            .Returns(new ValueTuple<bool, CompanySsiDetailStatusId, VerifiedCredentialTypeId, Guid, string?, string?, string?>(true, status, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, Guid.NewGuid(), null, null, null));
        async Task Act() => await _sut.RejectCredential(alreadyInactiveId).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be($"Credential {alreadyInactiveId} must be {CompanySsiDetailStatusId.PENDING}");
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "CredentialRejected" })))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task RejectCredential_WithValidRequest_ReturnsExpected()
    {
        // Arrange
        const string recipientMail = "test@mail.com";
        var now = DateTimeOffset.UtcNow;
        var requesterId = Guid.NewGuid();
        var notifications = new List<Notification>();
        var detail = new CompanySsiDetail(_validCredentialId, _identity.CompanyId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, CompanySsiDetailStatusId.PENDING, Guid.NewGuid(), requesterId, DateTimeOffset.Now);
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiRejectionData(_validCredentialId))
            .Returns(new ValueTuple<bool, CompanySsiDetailStatusId, VerifiedCredentialTypeId, Guid, string?, string?, string?>(true, CompanySsiDetailStatusId.PENDING, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, requesterId, recipientMail, "Tony", "Stark"));
        A.CallTo(() => _notificationRepository.CreateNotification(requesterId, NotificationTypeId.CREDENTIAL_REJECTED, false, A<Action<Notification>?>._))
            .Invokes((Guid receiverUserId, NotificationTypeId notificationTypeId, bool isRead, Action<Notification>? setOptionalParameters) =>
            {
                var notification = new Notification(Guid.NewGuid(), receiverUserId, DateTimeOffset.UtcNow,
                    notificationTypeId, isRead);
                setOptionalParameters?.Invoke(notification);
                notifications.Add(notification);
            });
        A.CallTo(() => _companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(_validCredentialId, A<Action<CompanySsiDetail>?>._, A<Action<CompanySsiDetail>>._!))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> updateFields) =>
            {
                initialize?.Invoke(detail);
                updateFields.Invoke(detail);
            });

        // Act
        await _sut.RejectCredential(_validCredentialId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _mailingService.SendMails(recipientMail, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "CredentialRejected" })))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();

        notifications.Should().ContainSingle();
        var notification = notifications.Single();
        notification.NotificationTypeId.Should().Be(NotificationTypeId.CREDENTIAL_REJECTED);
        notification.CreatorUserId.Should().Be(_identity.IdentityId);

        detail.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.INACTIVE);
        detail.DateLastChanged.Should().Be(now);
    }

    [Fact]
    public async Task RejectCredential_WithoutUserMail_ReturnsExpected()
    {
        // Arrange
        const string recipientMail = "test@mail.com";
        var now = DateTimeOffset.UtcNow;
        var requesterId = Guid.NewGuid();
        var notifications = new List<Notification>();
        var detail = new CompanySsiDetail(_validCredentialId, _identity.CompanyId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, CompanySsiDetailStatusId.PENDING, Guid.NewGuid(), requesterId, DateTimeOffset.UtcNow);
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiRejectionData(_validCredentialId))
            .Returns(new ValueTuple<bool, CompanySsiDetailStatusId, VerifiedCredentialTypeId, Guid, string?, string?, string?>(true, CompanySsiDetailStatusId.PENDING, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, requesterId, null, null, null));
        A.CallTo(() => _notificationRepository.CreateNotification(requesterId, NotificationTypeId.CREDENTIAL_REJECTED, false, A<Action<Notification>>._))
            .Invokes((Guid receiverUserId, NotificationTypeId notificationTypeId, bool isRead, Action<Notification>? setOptionalParameters) =>
            {
                var notification = new Notification(Guid.NewGuid(), receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameters?.Invoke(notification);
                notifications.Add(notification);
            });
        A.CallTo(() => _companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(_validCredentialId, A<Action<CompanySsiDetail>?>._, A<Action<CompanySsiDetail>>._!))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> updateFields) =>
            {
                initialize?.Invoke(detail);
                updateFields.Invoke(detail);
            });

        // Act
        await _sut.RejectCredential(_validCredentialId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _mailingService.SendMails(recipientMail, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "CredentialRejected" })))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();

        notifications.Should().ContainSingle();
        var notification = notifications.Single();
        notification.NotificationTypeId.Should().Be(NotificationTypeId.CREDENTIAL_REJECTED);
        notification.CreatorUserId.Should().Be(_identity.IdentityId);

        detail.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.INACTIVE);
        detail.DateLastChanged.Should().Be(now);
    }

    #endregion

    #region GetCertificateTypes

    [Fact]
    public async Task GetCertificateTypes_WithFilter_ReturnsList()
    {
        // Arrange
        A.CallTo(() => _companySsiDetailsRepository.GetCertificateTypes(_identity.CompanyId))
            .Returns(new[] { VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE }.ToAsyncEnumerable());

        // Act
        var result = await _sut.GetCertificateTypes().ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Should().Be(VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE);
    }

    #endregion

    #region GetAllCompanyCertificates

    [Fact]
    public async Task GetAllCompanyCertificatesAsync_WithDefaultRequest_GetsExpectedEntries()
    {
        // Arrange
        SetupPagination();
        var sut = _fixture.Create<CompanyDataBusinessLogic>();

        // Act
        var result = await sut.GetAllCompanyCertificatesAsync(0, 5, null, null, null);

        // Assert
        result.Content.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllCompanyCertificatesAsync_WithSmallSize_GetsExpectedEntries()
    {
        // Arrange
        const int expectedCount = 3;
        SetupPagination(expectedCount);
        var sut = _fixture.Create<CompanyDataBusinessLogic>();

        // Act
        var result = await sut.GetAllCompanyCertificatesAsync(0, expectedCount, null, null, null);

        // Assert
        result.Content.Should().HaveCount(expectedCount);
    }

    #endregion

    #region GetCompanyCertificateDocuments

    [Fact]
    public async Task GetCompanyCertificateDocumentAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        SetupFakesForGetDocument();

        // Act
        var result = await _sut.GetCompanyCertificateDocumentAsync(ValidDocumentId).ConfigureAwait(false);

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
        async Task Act() => await _sut.GetCompanyCertificateDocumentAsync(documentId).ConfigureAwait(false);

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
        async Task Act() => await _sut.GetCompanyCertificateDocumentAsync(documentId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"Document {documentId} status is not Locked");
    }

    #endregion

    #region DeleteCompanyCertificates

    [Fact]
    public async Task DeleteCompanyCertificateAsync_WithDocumentNotExisting_ThrowsNotFoundException()
    {
        // Arrange
        //var sut = _fixture.Create<CompanyDataBusinessLogic>();
        A.CallTo(() => _companyCertificateRepository.GetCompanyCertificateDocumentDetailsForIdUntrackedAsync(Guid.NewGuid(), _identity.CompanyId))
            .Returns((Guid.NewGuid(), DocumentStatusId.LOCKED, new[] { Guid.NewGuid() }.AsEnumerable(), false));

        // Act
        async Task Act() => await _sut.DeleteCompanyCertificateAsync(Guid.NewGuid()).ConfigureAwait(false);

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
        async Task Act() => await _sut.DeleteCompanyCertificateAsync(documentId).ConfigureAwait(false);

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
        async Task Act() => await _sut.DeleteCompanyCertificateAsync(documentId).ConfigureAwait(false);

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
        await _sut.DeleteCompanyCertificateAsync(documentId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _companyCertificateRepository.AttachAndModifyCompanyCertificateDetails(A<Guid>._, null, A<Action<CompanyCertificate>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyCertificateRepository.AttachAndModifyCompanyCertificateDocumentDetails(documentId, null, A<Action<Document>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened(1, Times.OrMore);

    }

    #endregion

    #region Setup

    private void SetupCreateUseCaseParticipation()
    {
        A.CallTo(() => _companySsiDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(_traceabilityExternalTypeDetailId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK))
            .Returns(true);
        A.CallTo(() => _companySsiDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(A<Guid>.That.Not.Matches(x => x == _traceabilityExternalTypeDetailId), A<VerifiedCredentialTypeId>._))
            .Returns(false);
    }

    private void SetupCreateSsiCertificate()
    {
        A.CallTo(() => _companySsiDetailsRepository.CheckSsiCertificateType(VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE))
            .Returns(true);
        A.CallTo(() => _companySsiDetailsRepository.CheckSsiCertificateType(A<VerifiedCredentialTypeId>.That.Matches(x => x != VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE)))
            .Returns(false);
    }

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
        A.CallTo(() => _companyCertificateRepository.GetCompanyCertificateDocumentDataAsync(ValidDocumentId, DocumentTypeId.COMPANY_CERTIFICATE))
            .ReturnsLazily(() => new ValueTuple<byte[], string, MediaTypeId, bool, bool>(content, "test.pdf", MediaTypeId.PDF, true, true));
        A.CallTo(() => _companyCertificateRepository.GetCompanyCertificateDocumentDataAsync(new Guid("aaf53459-c36b-408e-a805-0b406ce9751d"), DocumentTypeId.COMPANY_CERTIFICATE))
            .ReturnsLazily(() => new ValueTuple<byte[], string, MediaTypeId, bool, bool>(content, "test1.pdf", MediaTypeId.PDF, true, false));
    }
    #endregion
}
