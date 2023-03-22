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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic.Tests;

public class CompanyDataBusinessLogicTests
{
    private static readonly string IamUserId = Guid.NewGuid().ToString();
    private readonly IFixture _fixture;
    private readonly ICompanyRepository _companyRepository;
    private IPortalRepositories _portalRepositories;
    private IConsentRepository _consentRepository;
    private ICompanyRolesRepository _companyRolesRepository;
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

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentRepository>()).Returns(_consentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRolesRepository>()).Returns(_companyRolesRepository);
        _sut = new CompanyDataBusinessLogic(_portalRepositories);
    }

    #region GetOwnCompanyDetails

    [Fact]
    public async Task GetOwnCompanyDetailsAsync_ExpectedResults()
    {
        // Arrange
        var companyAddressDetailData = _fixture.Create<CompanyAddressDetailData>();
        A.CallTo(() => _companyRepository.GetOwnCompanyDetailsAsync(IamUserId))
            .ReturnsLazily(() => companyAddressDetailData);
        
        var sut = new CompanyDataBusinessLogic(_portalRepositories);
        
        // Act
        var result = await sut.GetOwnCompanyDetailsAsync(IamUserId);

        // Assert
        result.Should().NotBeNull();
        result.CompanyId.Should().Be(companyAddressDetailData.CompanyId);
    }

    [Fact]
    public async Task GetOwnCompanyDetailsAsync_ThrowsConflictException()
    {
        // Arrange
        A.CallTo(() => _companyRepository.GetOwnCompanyDetailsAsync(IamUserId))
            .ReturnsLazily(() => (CompanyAddressDetailData?)null);
        
        var sut = new CompanyDataBusinessLogic(_portalRepositories);
        
        // Act
        async Task Act() => await sut.GetOwnCompanyDetailsAsync(IamUserId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"user {IamUserId} is not associated with any company");
    }

    #endregion

    #region GetCompanyRoleAndConsentAgreementDetails

    [Fact]
    public async Task GetCompanyRoleAndConsentAgreementDetails_CallsExpected()
    {
        // Arrange
        var companyRoleConsentDatas = _fixture.CreateMany<CompanyRoleConsentData>(2).ToAsyncEnumerable();
        A.CallTo(() => _companyRepository.GetCompanyRoleAndConsentAgreementDetailsAsync(IamUserId))
            .ReturnsLazily(() => companyRoleConsentDatas);

        var sut = new CompanyDataBusinessLogic(_portalRepositories);

        // Act
        var result = await sut.GetCompanyRoleAndConsentAgreementDetailsAsync(IamUserId).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        A.CallTo(() => _companyRepository.GetCompanyRoleAndConsentAgreementDetailsAsync(A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompanyRoleAndConsentAgreementDetails_ThrowsConflictException()
    {
        // Arrange
        var companyRoleConsentDatas = _fixture.CreateMany<CompanyRoleConsentData>(0).ToAsyncEnumerable();
        A.CallTo(() => _companyRepository.GetCompanyRoleAndConsentAgreementDetailsAsync(IamUserId))
            .Returns(companyRoleConsentDatas);

        var sut = new CompanyDataBusinessLogic(_portalRepositories);

        // Act
        async Task Act() => await sut.GetCompanyRoleAndConsentAgreementDetailsAsync(IamUserId).ToListAsync().ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"user {IamUserId} is not associated with any company or Incorrect Status");
    }

    #endregion

    #region  CreateCompanyRoleAndConsentAgreementDetails

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_ReturnsExpected()
    {
        // Arrange
        var companyId = _fixture.Create<Guid>();
        var companyUserId = _fixture.Create<Guid>();
        var companyRole = new [] { CompanyRoleId.OPERATOR, CompanyRoleId.SERVICE_PROVIDER};
        var agreementAssignedRole = new []{
            CompanyRoleId.ACTIVE_PARTICIPANT,
            CompanyRoleId.APP_PROVIDER,
            CompanyRoleId.SERVICE_PROVIDER,
            CompanyRoleId.OPERATOR 
        };
        var companyRoleConsentDetails = new []{
            new CompanyRoleConsentDetails( CompanyRoleId.ACTIVE_PARTICIPANT,
                new []{new ConsentDetails(_fixture.Create<Guid>(), ConsentStatusId.ACTIVE)}),
            
            new CompanyRoleConsentDetails( CompanyRoleId.APP_PROVIDER,
                new []{new ConsentDetails(_fixture.Create<Guid>(), ConsentStatusId.ACTIVE)})
        };

        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(IamUserId))
            .ReturnsLazily(() => (true, companyId, companyRole, companyUserId, agreementAssignedRole));
        
        var sut = new CompanyDataBusinessLogic(_portalRepositories);
        
        // Act
        await sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(IamUserId,companyRoleConsentDetails).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _consentRepository.CreateConsent(A<Guid>._,companyId,companyUserId,ConsentStatusId.ACTIVE,A<Action<Consent>?>._)).MustHaveHappenedTwiceExactly();
        A.CallTo(() => _companyRolesRepository.CreateCompanyAssignedRole(companyId, A<CompanyRoleId>._)).MustHaveHappenedTwiceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_CompanyStatus_ThrowsConflictException()
    {
        // Arrange
        var companyRoleConsentDetails = _fixture.CreateMany<CompanyRoleConsentDetails>(2);
        var companyId = _fixture.Create<Guid>();
        var companyUserId = _fixture.Create<Guid>();
        var companyRole = new [] { CompanyRoleId.OPERATOR, CompanyRoleId.SERVICE_PROVIDER};
        var agreementAssignedRole = new []{
            CompanyRoleId.ACTIVE_PARTICIPANT,
            CompanyRoleId.APP_PROVIDER,
            CompanyRoleId.SERVICE_PROVIDER,
            CompanyRoleId.OPERATOR 
        };
        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(IamUserId))
            .ReturnsLazily(() => (false,companyId,companyRole,companyUserId,agreementAssignedRole));
        
        var sut = new CompanyDataBusinessLogic(_portalRepositories);

        // Act
        async Task Act() => await sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(IamUserId,companyRoleConsentDetails).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Company Status is Incorrect");
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_AgreementAssignedRole_ThrowsConflictException()
    {
        // Arrange
        var companyId = _fixture.Create<Guid>();
        var companyUserId = _fixture.Create<Guid>();
        var companyRole = new [] { CompanyRoleId.OPERATOR, CompanyRoleId.SERVICE_PROVIDER};
        var agreementAssignedRole = new []{ CompanyRoleId.APP_PROVIDER };
        var companyRoleConsentDetails = new []{
            new CompanyRoleConsentDetails( CompanyRoleId.ACTIVE_PARTICIPANT,
                new []{ new ConsentDetails(_fixture.Create<Guid>(), ConsentStatusId.ACTIVE) })};

        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(IamUserId))
            .ReturnsLazily(() => (true,companyId,companyRole,companyUserId,agreementAssignedRole));
        
        var sut = new CompanyDataBusinessLogic(_portalRepositories);

        // Act
        async Task Act() => await sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(IamUserId,companyRoleConsentDetails).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("All agreement need to get signed");
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_ConsentStatus_ThrowsConflictException()
    {
        // Arrange
        var companyId = _fixture.Create<Guid>();
        var companyUserId = _fixture.Create<Guid>();
        var companyRole = new [] { CompanyRoleId.OPERATOR, CompanyRoleId.SERVICE_PROVIDER};
        var agreementAssignedRole = new []{ CompanyRoleId.APP_PROVIDER };
        var companyRoleConsentDetails = new []{
            new CompanyRoleConsentDetails( CompanyRoleId.APP_PROVIDER,
                new []{ new ConsentDetails(_fixture.Create<Guid>(), ConsentStatusId.INACTIVE) })};

        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(IamUserId))
            .ReturnsLazily(() => (true,companyId,companyRole,companyUserId,agreementAssignedRole));
        
        var sut = new CompanyDataBusinessLogic(_portalRepositories);

        // Act
        async Task Act() => await sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(IamUserId,companyRoleConsentDetails).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("All agreement need to get signed");
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_CompanyRoleId_ThrowsConflictException()
    {
        // Arrange
        var companyId = _fixture.Create<Guid>();
        var companyUserId = _fixture.Create<Guid>();
        var companyRole = new [] { CompanyRoleId.APP_PROVIDER, CompanyRoleId.ACTIVE_PARTICIPANT};
        var agreementAssignedRole = new []{ CompanyRoleId.APP_PROVIDER };
        var companyRoleConsentDetails = new []{
            new CompanyRoleConsentDetails( CompanyRoleId.APP_PROVIDER,
                new []{ new ConsentDetails(_fixture.Create<Guid>(), ConsentStatusId.ACTIVE) })};

        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(IamUserId))
            .ReturnsLazily(() => (true,companyId,companyRole,companyUserId,agreementAssignedRole));
        
        var sut = new CompanyDataBusinessLogic(_portalRepositories);

        // Act
        async Task Act() => await sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(IamUserId,companyRoleConsentDetails).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("companyRole already exists");
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync_ThrowsNotFoundException()
    {
        // Arrange
        var companyRoleConsentDetails = _fixture.CreateMany<CompanyRoleConsentDetails>(2);

        A.CallTo(() => _companyRepository.GetCompanyRolesDataAsync(IamUserId))
            .ReturnsLazily(() => new ValueTuple<bool,Guid,IEnumerable<CompanyRoleId>,Guid,IEnumerable<CompanyRoleId>>());
        
        var sut = new CompanyDataBusinessLogic(_portalRepositories);

        // Act
        async Task Act() => await sut.CreateCompanyRoleAndConsentAgreementDetailsAsync(IamUserId,companyRoleConsentDetails).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"user {IamUserId} is not associated with any company");
    }
    #endregion
}