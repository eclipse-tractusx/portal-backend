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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
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
    private readonly Guid _detailVersionId = Guid.NewGuid();
    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IConsentRepository _consentRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ICompanyRolesRepository _companyRolesRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly ICompanySsiDetailsRepository _companySsiDetailsRepository;
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
        _documentRepository = A.Fake<IDocumentRepository>();
        _languageRepository = A.Fake<ILanguageRepository>();
        _notificationRepository = A.Fake<INotificationRepository>();
        _companySsiDetailsRepository = A.Fake<ICompanySsiDetailsRepository>();

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

        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

        var options = Options.Create(new CompanyDataSettings { MaxPageSize = 20, UseCaseParticipationMediaTypes = new[] { MediaTypeId.PDF }, SsiCertificateMediaTypes = new[] { MediaTypeId.PDF } });
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
        ex.Message.Should().Be(CompanyDataErrors.INVALID_COMPANY.ToString());
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
        ex.Message.Should().Be(CompanyDataErrors.COMPANY_NOT_FOUND.ToString());
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
        ex.Message.Should().Be(CompanyDataErrors.INVALID_COMPANY_STATUS.ToString());
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
        ex.Message.Should().Be(CompanyDataErrors.INVALID_LANGUAGECODE.ToString());

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
        ex.Message.Should().Be(CompanyDataErrors.INVALID_COMPANY_STATUS.ToString());
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
        ex.Message.Should().Be(CompanyDataErrors.MISSING_AGREEMENTS.ToString());
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
        ex.Message.Should().Be(CompanyDataErrors.UNASSIGN_ALL_ROLES.ToString());
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
        ex.Message.Should().Be(CompanyDataErrors.UNASSIGN_ALL_ROLES.ToString());
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
        ex.Message.Should().Be(CompanyDataErrors.COMPANY_NOT_FOUND.ToString());
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
        ex.Message.Should().Be(CompanyDataErrors.COMPANY_ROLE_IDS_CONSENT_STATUS_NULL.ToString());
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
        ex.Message.Should().Be(CompanyDataErrors.COMPANY_ROLE_IDS_CONSENT_STATUS_NULL.ToString());
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
        ex.Message.Should().Be(CompanyDataErrors.INVALID_COMPANY_STATUS.ToString());
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
        ex.Message.Should().Be(CompanyDataErrors.INVALID_COMPANY_STATUS.ToString());
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
        ex.Message.Should().Be(CompanyDataErrors.USE_CASE_NOT_FOUND.ToString());
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
        A.CallTo(() => _companySsiDetailsRepository.GetUseCaseParticipationForCompany(_identity.CompanyId, "en", A<DateTimeOffset>._))
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
        A.CallTo(() => _companySsiDetailsRepository.GetUseCaseParticipationForCompany(_identity.CompanyId, "en", A<DateTimeOffset>._))
            .Returns(_fixture.Build<UseCaseParticipationTransferData>().With(x => x.VerifiedCredentials, verifiedCredentials).CreateMany(5).ToAsyncEnumerable());

        // Act
        var Act = () => _sut.GetUseCaseParticipationAsync("en");

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be(CompanyDataErrors.MULTIPLE_SSI_DETAIL.ToString());
    }

    #endregion

    #region GetSsiCertificates

    [Fact]
    public async Task GetSsiCertificates_WithValidRequest_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _companySsiDetailsRepository.GetSsiCertificates(_identity.CompanyId, A<DateTimeOffset>._))
            .Returns(_fixture.Build<SsiCertificateTransferData>().With(x => x.Credentials, Enumerable.Repeat(new SsiCertificateExternalTypeDetailTransferData(_fixture.Create<ExternalTypeDetailData>(), _fixture.CreateMany<CompanySsiDetailTransferData>(1)), 1)).CreateMany(5).ToAsyncEnumerable());

        // Act
        var result = await _sut.GetSsiCertificatesAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetSsiCertificates_WithMultipleSsiDetailData_ThrowsException()
    {
        // Arrange
        var verifiedCredentials = _fixture.Build<SsiCertificateExternalTypeDetailTransferData>()
            .With(x => x.SsiDetailData, _fixture.CreateMany<CompanySsiDetailTransferData>(2))
            .CreateMany(4);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiCertificates(_identity.CompanyId, A<DateTimeOffset>._))
            .Returns(_fixture.Build<SsiCertificateTransferData>().With(x => x.Credentials, verifiedCredentials).CreateMany(5).ToAsyncEnumerable());

        // Act
        var Act = () => _sut.GetSsiCertificatesAsync();

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be(CompanyDataErrors.MULTIPLE_SSI_DETAIL.ToString());
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
        ex.Message.Should().Be(CompanyDataErrors.EXTERNAL_TYPE_DETAIL_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task CreateUseCaseParticipation_WithExpiryInPast_ThrowsControllerArgumentException()
    {
        // Arrange
        var verifiedCredentialExternalTypeDetailId = Guid.NewGuid();
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(DateTimeOffset.UtcNow);
        A.CallTo(() => _companySsiDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(verifiedCredentialExternalTypeDetailId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK))
            .Returns(DateTimeOffset.UtcNow.AddDays(-1));
        var data = new UseCaseParticipationCreationData(verifiedCredentialExternalTypeDetailId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, file);

        // Act
        async Task Act() => await _sut.CreateUseCaseParticipation(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(CompanyDataErrors.EXPIRY_DATE_IN_PAST.ToString());
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
        ex.Message.Should().Be(CompanyDataErrors.CREDENTIAL_ALREADY_EXISTING.ToString());
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
        detail.VerifiedCredentialExternalTypeDetailVersionId.Should().Be(_traceabilityExternalTypeDetailId);
    }

    #endregion

    #region CreateSsiCertificate

    [Fact]
    public async Task CreateSsiCertificate_WithInvalidDocumentContentType_ThrowsUnsupportedMediaTypeException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PNG.MapToMediaType());
        var data = new SsiCertificateCreationData(null, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, file);

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
        var data = new SsiCertificateCreationData(null, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, file);

        // Act
        async Task Act() => await _sut.CreateSsiCertificate(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(CompanyDataErrors.CREDENTIAL_NO_CERTIFICATE.ToString());
    }

    [Fact]
    public async Task CheckSsiDetailsExistsForCompany_WithRequestAlreadyExisting_ThrowsControllerArgumentException()
    {
        // Arrange
        SetupCreateSsiCertificate();
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var data = new SsiCertificateCreationData(null, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, file);

        A.CallTo(() => _companySsiDetailsRepository.CheckSsiDetailsExistsForCompany(_identity.CompanyId, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, VerifiedCredentialTypeKindId.CERTIFICATE, A<Guid>._))
            .Returns(true);

        // Act
        async Task Act() => await _sut.CreateSsiCertificate(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(CompanyDataErrors.CREDENTIAL_ALREADY_EXISTING.ToString());
    }

    [Fact]
    public async Task CreateSsiCertificate_WithMultipleDetailsButDetailIdNotSet_ThrowsControllerArgumentException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var data = new SsiCertificateCreationData(null, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, file);
        A.CallTo(() => _companySsiDetailsRepository.CheckSsiCertificateType(VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE))
            .Returns((true, _fixture.CreateMany<Guid>(5)));

        // Act
        async Task Act() => await _sut.CreateSsiCertificate(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(CompanyDataErrors.EXTERNAL_TYPE_DETAIL_ID_NOT_SET.ToString());
    }

    [Fact]
    public async Task CreateSsiCertificate_WithDetailsIdNotExisting_ThrowsControllerArgumentException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var data = new SsiCertificateCreationData(Guid.NewGuid(), VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, file);

        A.CallTo(() => _companySsiDetailsRepository.CheckSsiCertificateType(VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE))
            .Returns((true, Enumerable.Repeat(_detailVersionId, 1)));

        // Act
        async Task Act() => await _sut.CreateSsiCertificate(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(CompanyDataErrors.EXTERNAL_TYPE_DETAIL_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task CreateSsiCertificate_WithValidCall_CreatesExpected()
    {
        // Arrange
        SetupCreateSsiCertificate();
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var data = new SsiCertificateCreationData(null, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, file);
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
        detail.VerifiedCredentialExternalTypeDetailVersionId.Should().Be(_detailVersionId);
    }

    #endregion

    #region GetCredentials

    [Fact]
    public async Task GetCredentials_WithFilter_ReturnsList()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var companySsiDetailStatusId = _fixture.Create<CompanySsiDetailStatusId>();
        var credentialTypeId = _fixture.Create<VerifiedCredentialTypeId>();
        var companyName = _fixture.Create<string>();
        var verificationCredentialType = _fixture.Build<VerifiedCredentialType>()
            .With(x => x.VerifiedCredentialTypeAssignedUseCase, new VerifiedCredentialTypeAssignedUseCase(VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, useCaseId)
            {
                UseCase = new UseCase(useCaseId, "test", "name"),
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
        ex.Message.Should().Be(CompanyDataErrors.SSI_DETAILS_NOT_FOUND.ToString());
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
        ex.Message.Should().Be(CompanyDataErrors.CREDENTIAL_NOT_PENDING.ToString());
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
        ex.Message.Should().Be(CompanyDataErrors.BPN_NOT_SET.ToString());
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "CredentialApproval" })))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task ApproveCredential_WithExpiryInThePast_ReturnsExpected()
    {
        // Arrange
        const string recipientMail = "test@mail.com";
        const string bpn = "BPNL00000001TEST";
        const VerifiedCredentialTypeId typeId = VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK;
        var now = DateTimeOffset.Now;
        var requesterId = Guid.NewGuid();
        var detailData = new DetailData(
            VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL,
            "test",
            "1.0.0",
            DateTimeOffset.Now.AddDays(-5)
        );

        var data = new SsiApprovalData(
            CompanySsiDetailStatusId.PENDING,
            typeId,
            VerifiedCredentialTypeKindId.USE_CASE,
            "Stark Industries",
            bpn,
            detailData,
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
        async Task Act() => await _sut.ApproveCredential(_validCredentialId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be(CompanyDataErrors.EXPIRY_DATE_IN_PAST.ToString());
        A.CallTo(() => _mailingService.SendMails(recipientMail, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "CredentialApproval" })))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _custodianService.TriggerDismantlerAsync(bpn, typeId, A<DateTimeOffset>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _custodianService.TriggerFrameworkAsync(A<CustodianFrameworkRequest>.That.Matches(x => x.HolderIdentifier == bpn), A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(VerifiedCredentialTypeKindId.USE_CASE, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, "2024-01-05 +0", "2024-01-05 +0")]
    [InlineData(VerifiedCredentialTypeKindId.CERTIFICATE, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, "2027-01-30 +0", "2025-01-01 +0")]
    public async Task ApproveCredential_WithValidRequest_ReturnsExpected(VerifiedCredentialTypeKindId kindId, VerifiedCredentialTypeId typeId, DateTimeOffset expiry, DateTimeOffset expectedExpiry)
    {
        // Arrange
        const string recipientMail = "test@mail.com";
        const string bpn = "BPNL00000001TEST";
        var now = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var requesterId = Guid.NewGuid();
        var notifications = new List<Notification>();
        var detailData = new DetailData(
            kindId == VerifiedCredentialTypeKindId.USE_CASE ? VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL : VerifiedCredentialExternalTypeId.PCF_CREDENTIAL,
            "test",
            "1.0.0",
            expiry
        );

        var data = new SsiApprovalData(
            CompanySsiDetailStatusId.PENDING,
            typeId,
            kindId,
            "Stark Industries",
            bpn,
            detailData,
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
            A.CallTo(() => _custodianService.TriggerFrameworkAsync(A<CustodianFrameworkRequest>.That.Matches(x => x.HolderIdentifier == bpn && x.Expiry == expectedExpiry), A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _custodianService.TriggerDismantlerAsync(bpn, typeId, A<DateTimeOffset>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }
        else
        {
            A.CallTo(() => _custodianService.TriggerFrameworkAsync(A<CustodianFrameworkRequest>.That.Matches(x => x.HolderIdentifier == bpn && x.Expiry == expectedExpiry), A<CancellationToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _custodianService.TriggerDismantlerAsync(bpn, typeId, A<DateTimeOffset>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        notifications.Should().ContainSingle();
        var notification = notifications.Single();
        notification.NotificationTypeId.Should().Be(NotificationTypeId.CREDENTIAL_APPROVAL);
        notification.CreatorUserId.Should().Be(_identity.IdentityId);

        detail.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.ACTIVE);
        detail.DateLastChanged.Should().Be(now);
        detail.ExpiryDate.Should().Be(expectedExpiry);
    }

    [Fact]
    public async Task ApproveCredential_WithInvalidCredentialType_ThrowsException()
    {
        // Arrange
        const string recipientMail = "test@mail.com";
        var now = DateTimeOffset.UtcNow;
        var requesterId = Guid.NewGuid();
        var useCaseData = new DetailData(
            VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL,
            "test",
            "1.0.0",
            DateTimeOffset.UtcNow
        );

        var bpn = "BPNL00000001TEST";
        var data = new SsiApprovalData(
            CompanySsiDetailStatusId.PENDING,
            default,
            VerifiedCredentialTypeKindId.USE_CASE,
            "Stark Industries",
            bpn,
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
        ex.Message.Should().Be(CompanyDataErrors.CREDENTIAL_TYPE_NOT_FOUND.ToString());
    }

    [Theory]
    [InlineData(VerifiedCredentialTypeKindId.USE_CASE, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK)]
    [InlineData(VerifiedCredentialTypeKindId.CERTIFICATE, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE)]
    public async Task ApproveCredential_WithDetailVersionNotSet_ThrowsConflictException(VerifiedCredentialTypeKindId kindId, VerifiedCredentialTypeId typeId)
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var requesterId = Guid.NewGuid();

        var bpn = "BPNL00000001TEST";
        var data = new SsiApprovalData(
            CompanySsiDetailStatusId.PENDING,
            typeId,
            kindId,
            "Stark Industries",
            bpn,
            null,
            new SsiRequesterData(
                requesterId,
                null,
                null,
                null
            )
        );

        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(_validCredentialId))
            .Returns(new ValueTuple<bool, SsiApprovalData>(true, data));
        async Task Act() => await _sut.ApproveCredential(_validCredentialId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { "CredentialRejected" })))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();

        A.CallTo(() => _custodianService.TriggerDismantlerAsync(bpn, typeId, A<DateTimeOffset>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _custodianService.TriggerFrameworkAsync(A<CustodianFrameworkRequest>.That.Matches(x => x.HolderIdentifier == bpn), A<CancellationToken>._))
            .MustNotHaveHappened();
        ex.Message.Should().Be(CompanyDataErrors.EXTERNAL_TYPE_DETAIL_ID_NOT_SET.ToString());
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
        var detailData = new DetailData(
            kindId == VerifiedCredentialTypeKindId.USE_CASE ? VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL : VerifiedCredentialExternalTypeId.PCF_CREDENTIAL,
            "test",
            "1.0.0",
            DateTimeOffset.UtcNow
        );

        const string bpn = "BPNL00000001TEST";
        var data = new SsiApprovalData(
            CompanySsiDetailStatusId.PENDING,
            typeId,
            kindId,
            "Stark Industries",
            bpn,
            detailData,
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
            A.CallTo(() => _custodianService.TriggerFrameworkAsync(A<CustodianFrameworkRequest>.That.Matches(x => x.HolderIdentifier == bpn), A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _custodianService.TriggerDismantlerAsync(bpn, typeId, A<DateTimeOffset>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }
        else
        {
            A.CallTo(() => _custodianService.TriggerFrameworkAsync(A<CustodianFrameworkRequest>.That.Matches(x => x.HolderIdentifier == bpn), A<CancellationToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _custodianService.TriggerDismantlerAsync(bpn, typeId, A<DateTimeOffset>._, A<CancellationToken>._))
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
        ex.Message.Should().Be(CompanyDataErrors.SSI_DETAILS_NOT_FOUND.ToString());
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
        ex.Message.Should().Be(CompanyDataErrors.CREDENTIAL_NOT_PENDING.ToString());
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

    #region Setup

    private void SetupCreateUseCaseParticipation()
    {
        A.CallTo(() => _companySsiDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(_traceabilityExternalTypeDetailId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK))
            .Returns(DateTimeOffset.UtcNow);
        A.CallTo(() => _companySsiDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(A<Guid>.That.Not.Matches(x => x == _traceabilityExternalTypeDetailId), A<VerifiedCredentialTypeId>._))
            .Returns((DateTimeOffset)default);
    }

    private void SetupCreateSsiCertificate()
    {
        A.CallTo(() => _companySsiDetailsRepository.CheckSsiCertificateType(VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE))
            .Returns((true, Enumerable.Repeat(_detailVersionId, 1)));
        A.CallTo(() => _companySsiDetailsRepository.CheckSsiCertificateType(A<VerifiedCredentialTypeId>.That.Matches(x => x != VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE)))
            .Returns((false, Enumerable.Empty<Guid>()));
    }

    #endregion
}
