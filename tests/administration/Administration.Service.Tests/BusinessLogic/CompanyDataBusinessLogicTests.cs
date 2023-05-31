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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic.Tests;

public class CompanyDataBusinessLogicTests
{
    private readonly IdentityData _identity = new(Guid.NewGuid().ToString(), Guid.NewGuid(), IdentityTypeId.COMPANY_USER, Guid.NewGuid());
    private readonly IFixture _fixture;
    private readonly ICompanyRepository _companyRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IConsentRepository _consentRepository;
    private readonly ICompanyRolesRepository _companyRolesRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly CompanyDataBusinessLogic _sut;

    public CompanyDataBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _companyRepository = A.Fake<ICompanyRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _consentRepository = A.Fake<IConsentRepository>();
        _companyRolesRepository = A.Fake<ICompanyRolesRepository>();
        _languageRepository = A.Fake<ILanguageRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentRepository>()).Returns(_consentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRolesRepository>()).Returns(_companyRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ILanguageRepository>()).Returns(_languageRepository);
        _sut = new CompanyDataBusinessLogic(_portalRepositories);
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
        var result = await _sut.GetCompanyDetailsAsync(_identity.CompanyId);

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
        async Task Act() => await _sut.GetCompanyDetailsAsync(_identity.CompanyId);

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
        var companyRoleConsentDatas = new[] {
            _fixture.Create<CompanyRoleConsentData>(),
            new CompanyRoleConsentData(
                _fixture.Create<CompanyRoleId>(),
                _fixture.Create<string>(),
                true,
                new ConsentAgreementData [] {
                    new (Guid.NewGuid(), _fixture.Create<string>(), Guid.NewGuid(), 0),
                    new (Guid.NewGuid(), _fixture.Create<string>(), Guid.NewGuid(), ConsentStatusId.ACTIVE),
                    new (Guid.NewGuid(), _fixture.Create<string>(), Guid.NewGuid(), ConsentStatusId.INACTIVE),
                }),
            _fixture.Create<CompanyRoleConsentData>(),
        };
        var companyId = _fixture.Create<Guid>();
        var languageShortName = "en";

        A.CallTo(() => _companyRepository.GetCompanyStatusDataAsync(_identity.CompanyId))
            .Returns((true, companyId));

        A.CallTo(() => _languageRepository.IsValidLanguageCode(languageShortName))
            .Returns(true);

        A.CallTo(() => _companyRepository.GetCompanyRoleAndConsentAgreementDataAsync(companyId, languageShortName))
            .Returns(companyRoleConsentDatas.ToAsyncEnumerable());

        // Act
        var result = await _sut.GetCompanyRoleAndConsentAgreementDetailsAsync(_identity, languageShortName).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull()
            .And.HaveSameCount(companyRoleConsentDatas);

        result.Zip(companyRoleConsentDatas).Should()
            .AllSatisfy(x => x.Should().Match<(CompanyRoleConsentViewData Result, CompanyRoleConsentData Mock)>(
                z =>
                z.Result.CompanyRoleId == z.Mock.CompanyRoleId &&
                z.Result.RoleDescription == z.Mock.RoleDescription &&
                z.Result.CompanyRolesActive == z.Mock.CompanyRolesActive &&
                z.Result.Agreements.SequenceEqual(z.Mock.Agreements.Select(a => new ConsentAgreementViewData(a.AgreementId, a.AgreementName, a.DocumentId, a.ConsentStatus == 0 ? null : a.ConsentStatus)))));

        A.CallTo(() => _companyRepository.GetCompanyRoleAndConsentAgreementDataAsync(companyId, languageShortName)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompanyRoleAndConsentAgreementDetails_ThrowsNotFoundException()
    {
        // Arrange
        var languageShortName = "en";

        A.CallTo(() => _companyRepository.GetCompanyStatusDataAsync(_identity.CompanyId))
            .Returns(((bool, Guid))default);

        A.CallTo(() => _languageRepository.IsValidLanguageCode(languageShortName))
            .Returns(true);

        // Act
        async Task Act() => await _sut.GetCompanyRoleAndConsentAgreementDetailsAsync(_identity, languageShortName).ToListAsync().ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"User {_identity.UserEntityId} is not associated with any company");
    }

    [Fact]
    public async Task GetCompanyRoleAndConsentAgreementDetails_ThrowsConflictException()
    {
        // Arrange
        var companyId = _fixture.Create<Guid>();
        var languageShortName = "en";

        A.CallTo(() => _companyRepository.GetCompanyStatusDataAsync(_identity.CompanyId))
            .Returns((false, companyId));

        A.CallTo(() => _languageRepository.IsValidLanguageCode(languageShortName))
            .Returns(true);

        // Act
        async Task Act() => await _sut.GetCompanyRoleAndConsentAgreementDetailsAsync(_identity, languageShortName).ToListAsync().ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Company Status is Incorrect");
    }

    [Fact]
    public async Task GetCompanyRoleAndConsentAgreementDetails_Throws()
    {
        // Arrange
        var languageShortName = "eng";

        A.CallTo(() => _languageRepository.IsValidLanguageCode(languageShortName))
            .Returns(false);

        // Act
        async Task Act() => await _sut.GetCompanyRoleAndConsentAgreementDetailsAsync(_identity, languageShortName).ToListAsync().ConfigureAwait(false);

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
        var companyId = _fixture.Create<Guid>();
        var companyUserId = _fixture.Create<Guid>();
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
                }),
            new (CompanyRoleId.APP_PROVIDER,
                new ConsentDetails[] {
                    new (agreementId3, ConsentStatusId.ACTIVE),
                    new (agreementId4, ConsentStatusId.ACTIVE),
                    new (agreementId5, ConsentStatusId.ACTIVE),
                }),
        };

        var agreementData = new (Guid, CompanyRoleId)[]{
            new (agreementId1, CompanyRoleId.ACTIVE_PARTICIPANT),
            new (agreementId2, CompanyRoleId.ACTIVE_PARTICIPANT),
            new (agreementId3, CompanyRoleId.ACTIVE_PARTICIPANT),
            new (agreementId3, CompanyRoleId.APP_PROVIDER),
            new (agreementId4, CompanyRoleId.APP_PROVIDER),
            new (agreementId5, CompanyRoleId.APP_PROVIDER),
        }.ToAsyncEnumerable();

        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(_identity.CompanyUserId, A<IEnumerable<CompanyRoleId>>._))
            .Returns((true, companyId, Enumerable.Empty<CompanyRoleId>(), companyUserId, consentStatusDetails));

        A.CallTo(() => _companyRepository.GetAgreementAssignedRolesDataAsync(A<IEnumerable<CompanyRoleId>>._))
            .Returns(agreementData);

        A.CallTo(() => _consentRepository.AddAttachAndModifyConsents(A<IEnumerable<ConsentStatusDetails>>._, A<IEnumerable<(Guid, ConsentStatusId)>>._, A<Guid>._, A<Guid>._, A<DateTimeOffset>._))
            .Returns(new Consent[] {
                new(_fixture.Create<Guid>(), agreementId1, companyId, companyUserId, ConsentStatusId.ACTIVE, utcNow),
                new(_fixture.Create<Guid>(), agreementId2, companyId, companyUserId, ConsentStatusId.ACTIVE, utcNow),
                new(_fixture.Create<Guid>(), agreementId3, companyId, companyUserId, ConsentStatusId.ACTIVE, utcNow),
                new(_fixture.Create<Guid>(), agreementId4, companyId, companyUserId, ConsentStatusId.ACTIVE, utcNow),
                new(_fixture.Create<Guid>(), agreementId5, companyId, companyUserId, ConsentStatusId.ACTIVE, utcNow)
            });

        // Act
        await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(_identity, companyRoleConsentDetails).ConfigureAwait(false);

        // Assert
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
                    x.Count() == 5 &&
                    x.Contains(new(agreementId1, ConsentStatusId.ACTIVE)) &&
                    x.Contains(new(agreementId2, ConsentStatusId.ACTIVE)) &&
                    x.Contains(new(agreementId3, ConsentStatusId.ACTIVE)) &&
                    x.Contains(new(agreementId4, ConsentStatusId.ACTIVE)) &&
                    x.Contains(new(agreementId5, ConsentStatusId.ACTIVE))),
                companyId,
                companyUserId,
                A<DateTimeOffset>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_CompanyStatus_ThrowsConflictException()
    {
        // Arrange
        var companyRoleConsentDetails = _fixture.CreateMany<CompanyRoleConsentDetails>(2);
        var companyId = _fixture.Create<Guid>();
        var companyUserId = _fixture.Create<Guid>();

        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(_identity.CompanyUserId, A<IEnumerable<CompanyRoleId>>._))
            .Returns((false, companyId, null, companyUserId, null));

        // Act
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(_identity, companyRoleConsentDetails).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Company Status is Incorrect");
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_MissingAgreementAssignedRole_ThrowsControllerArgumentException()
    {
        // Arrange
        // Arrange
        var companyId = _fixture.Create<Guid>();
        var companyUserId = _fixture.Create<Guid>();
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

        var agreementData = new (Guid, CompanyRoleId)[]{
            new (agreementId1, CompanyRoleId.ACTIVE_PARTICIPANT),
            new (agreementId2, CompanyRoleId.ACTIVE_PARTICIPANT),
            new (agreementId3, CompanyRoleId.ACTIVE_PARTICIPANT),
            new (agreementId3, CompanyRoleId.APP_PROVIDER),
            new (agreementId4, CompanyRoleId.APP_PROVIDER),
            new (agreementId5, CompanyRoleId.APP_PROVIDER),
        }.ToAsyncEnumerable();

        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(_identity.CompanyUserId, A<IEnumerable<CompanyRoleId>>._))
            .Returns((true, companyId, Enumerable.Empty<CompanyRoleId>(), companyUserId, consentStatusDetails));

        A.CallTo(() => _companyRepository.GetAgreementAssignedRolesDataAsync(A<IEnumerable<CompanyRoleId>>._))
            .Returns(agreementData);

        // Act
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(_identity, companyRoleConsentDetails).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"All agreements need to get signed. Missing active consents: [ACTIVE_PARTICIPANT: [{agreementId2}, {agreementId3}], APP_PROVIDER: [{agreementId3}]]");
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_ExtraAgreementAssignedRole_ThrowsControllerArgumentException()
    {
        // Arrange
        // Arrange
        var companyId = _fixture.Create<Guid>();
        var companyUserId = _fixture.Create<Guid>();
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

        var agreementData = new (Guid, CompanyRoleId)[]{
            new (agreementId1, CompanyRoleId.ACTIVE_PARTICIPANT),
            new (agreementId2, CompanyRoleId.ACTIVE_PARTICIPANT),
            new (agreementId3, CompanyRoleId.ACTIVE_PARTICIPANT),
            new (agreementId3, CompanyRoleId.APP_PROVIDER),
            new (agreementId4, CompanyRoleId.APP_PROVIDER),
            new (agreementId5, CompanyRoleId.APP_PROVIDER),
        }.ToAsyncEnumerable();

        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(_identity.CompanyUserId, A<IEnumerable<CompanyRoleId>>._))
            .Returns((true, companyId, Enumerable.Empty<CompanyRoleId>(), companyUserId, consentStatusDetails));

        A.CallTo(() => _companyRepository.GetAgreementAssignedRolesDataAsync(A<IEnumerable<CompanyRoleId>>._))
            .Returns(agreementData);

        // Act
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(_identity, companyRoleConsentDetails).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"Agreements not associated with requested companyRoles: [ACTIVE_PARTICIPANT: [{agreementId4}], APP_PROVIDER: [{agreementId1}, {agreementId2}]]");
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_CompanyRoleId_ThrowsConflictException()
    {
        // Arrange
        // Arrange
        var companyId = _fixture.Create<Guid>();
        var companyUserId = _fixture.Create<Guid>();

        var companyRoleConsentDetails = _fixture.CreateMany<CompanyRoleConsentDetails>();
        var consentStatusDetails = _fixture.CreateMany<ConsentStatusDetails>();
        var companyRoles = new[] { CompanyRoleId.APP_PROVIDER, CompanyRoleId.ACTIVE_PARTICIPANT };

        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(_identity.CompanyUserId, A<IEnumerable<CompanyRoleId>>._))
            .Returns((true, companyId, companyRoles, companyUserId, consentStatusDetails));

        // Act
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(_identity, companyRoleConsentDetails).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"companyRoles [APP_PROVIDER, ACTIVE_PARTICIPANT] are already assigned to company {companyId}");
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_ThrowsForbiddenException()
    {
        // Arrange
        var companyRoleConsentDetails = _fixture.CreateMany<CompanyRoleConsentDetails>(2);
        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(_identity.CompanyUserId, A<IEnumerable<CompanyRoleId>>._))
            .Returns(((bool, Guid, IEnumerable<CompanyRoleId>?, Guid, IEnumerable<ConsentStatusDetails>?))default);

        // Act
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(_identity, companyRoleConsentDetails).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"user {_identity.UserEntityId} is not associated with any company");
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var companyRoleConsentDetails = _fixture.CreateMany<CompanyRoleConsentDetails>(2);
        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(_identity.CompanyUserId, A<IEnumerable<CompanyRoleId>>._))
            .Returns((true, Guid.NewGuid(), null!, Guid.NewGuid(), null!));

        // Act
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(_identity, companyRoleConsentDetails).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be($"neither CompanyRoleIds nor ConsentStatusDetails should ever be null here");
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_AgreementAssignedrole_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var companyRoleConsentDetails = _fixture.CreateMany<CompanyRoleConsentDetails>(2);
        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(_identity.CompanyUserId, A<IEnumerable<CompanyRoleId>>._))
            .Returns((true, Guid.NewGuid(), Enumerable.Empty<CompanyRoleId>(), Guid.NewGuid(), null!));

        // Act
        async Task Act() => await _sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(_identity, companyRoleConsentDetails).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be($"neither CompanyRoleIds nor ConsentStatusDetails should ever be null here");
    }

    #endregion

    #region CompanyAssigendUseCaseDetails

    [Fact]
    public async Task GetCompanyAssigendUseCaseDetailsAsync_ResturnsExpected()
    {
        // Arrange
        var companyAssignedUseCaseData = _fixture.CreateMany<CompanyAssignedUseCaseData>(2).ToAsyncEnumerable();
        A.CallTo(() => _companyRepository.GetCompanyAssigendUseCaseDetailsAsync(_identity.CompanyId))
            .ReturnsLazily(() => companyAssignedUseCaseData);

        // Act
        var result = await _sut.GetCompanyAssigendUseCaseDetailsAsync(_identity).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();

        A.CallTo(() => _companyRepository.GetCompanyAssigendUseCaseDetailsAsync(_identity.CompanyId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyAssignedUseCaseDetailsAsync_NoContent_ReturnsExpected()
    {
        // Arrange
        var useCaseId = _fixture.Create<Guid>();
        var companyId = _fixture.Create<Guid>();
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(_identity.CompanyId, useCaseId))
            .Returns((false, true, companyId));

        // Act
        var result = await _sut.CreateCompanyAssignedUseCaseDetailsAsync(_identity, useCaseId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companyRepository.CreateCompanyAssignedUseCase(companyId, useCaseId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCompanyAssignedUseCaseDetailsAsync_AlreadyReported_ReturnsExpected()
    {
        // Arrange
        var useCaseId = _fixture.Create<Guid>();
        var companyId = _fixture.Create<Guid>();

        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(_identity.CompanyId, useCaseId))
            .Returns((true, true, companyId));

        // Act
        var result = await _sut.CreateCompanyAssignedUseCaseDetailsAsync(_identity, useCaseId).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
        A.CallTo(() => _companyRepository.CreateCompanyAssignedUseCase(companyId, useCaseId)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();

    }

    [Fact]
    public async Task CreateCompanyAssignedUseCaseDetailsAsync_ThrowsConflictException()
    {
        // Arrange
        var useCaseId = _fixture.Create<Guid>();
        var companyId = _fixture.Create<Guid>();

        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(_identity.CompanyId, useCaseId))
            .Returns((false, false, companyId));

        // Act
        async Task Act() => await _sut.CreateCompanyAssignedUseCaseDetailsAsync(_identity, useCaseId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Company Status is Incorrect");
    }

    [Fact]
    public async Task RemoveCompanyAssignedUseCaseDetailsAsync_ReturnsExpected()
    {
        // Arrange
        var useCaseId = _fixture.Create<Guid>();
        var companyId = _fixture.Create<Guid>();

        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(_identity.CompanyId, useCaseId))
            .Returns((true, true, companyId));

        // Act
        await _sut.RemoveCompanyAssignedUseCaseDetailsAsync(_identity, useCaseId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companyRepository.RemoveCompanyAssignedUseCase(companyId, useCaseId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RemoveCompanyAssignedUseCaseDetailsAsync_companyStatus_ThrowsConflictException()
    {
        // Arrange
        var useCaseId = _fixture.Create<Guid>();
        var companyId = _fixture.Create<Guid>();

        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(_identity.CompanyId, useCaseId))
            .Returns((true, false, companyId));

        // Act
        async Task Act() => await _sut.RemoveCompanyAssignedUseCaseDetailsAsync(_identity, useCaseId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Company Status is Incorrect");
    }

    [Fact]
    public async Task RemoveCompanyAssignedUseCaseDetailsAsync_useCaseId_ThrowsConflictException()
    {
        // Arrange
        var useCaseId = _fixture.Create<Guid>();
        var companyId = _fixture.Create<Guid>();

        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(_identity.CompanyId, useCaseId))
            .Returns((false, true, companyId));

        // Act
        async Task Act() => await _sut.RemoveCompanyAssignedUseCaseDetailsAsync(_identity, useCaseId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"UseCaseId {useCaseId} is not available");
    }

    #endregion
}
