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

    #region CompanyAssigendUseCaseDetails

    [Fact]
    public async Task GetCompanyAssigendUseCaseDetailsAsync_ResturnsExpected()
    {
        // Arrange
        var companyAssignedUseCaseData = _fixture.CreateMany<CompanyAssignedUseCaseData>(2).ToAsyncEnumerable();
        A.CallTo(() => _companyRepository.GetCompanyAssigendUseCaseDetailsAsync(IamUserId))
            .ReturnsLazily(() => companyAssignedUseCaseData);

        var sut = new CompanyDataBusinessLogic(_portalRepositories);

        // Act
        var result = await sut.GetCompanyAssigendUseCaseDetailsAsync(IamUserId).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        A.CallTo(() => _companyRepository.GetCompanyAssigendUseCaseDetailsAsync(A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyAssignedUseCaseDetailsAsync_NoConent_ReturnsExpected()
    {
        // Arrange
        var useCaseId = _fixture.Create<Guid>();
        var companyId = _fixture.Create<Guid>();
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(IamUserId,useCaseId))
            .ReturnsLazily(() => (false, true, companyId));
        
        var sut = new CompanyDataBusinessLogic(_portalRepositories);
        
        // Act
        var result = await sut.CreateCompanyAssignedUseCaseDetailsAsync(IamUserId, useCaseId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companyRepository.CreateCompanyAssignedUseCase(companyId, useCaseId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        result.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CreateCompanyAssignedUseCaseDetailsAsync_AlreadyReported_ReturnsExpected()
    {
        // Arrange
        var useCaseId = _fixture.Create<Guid>();
        var companyId = _fixture.Create<Guid>();
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(IamUserId,useCaseId))
            .ReturnsLazily(() => (true, true, companyId));
        
        var sut = new CompanyDataBusinessLogic(_portalRepositories);
        
        // Act
        var result = await sut.CreateCompanyAssignedUseCaseDetailsAsync(IamUserId, useCaseId).ConfigureAwait(false);

        // Assert
        result.Should().Be(System.Net.HttpStatusCode.AlreadyReported);
        A.CallTo(() => _companyRepository.CreateCompanyAssignedUseCase(companyId, useCaseId)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();

    }

    [Fact]
    public async Task CreateCompanyAssignedUseCaseDetailsAsync_ThrowsConflictException()
    {
        // Arrange
        var useCaseId = _fixture.Create<Guid>();
        var companyId = _fixture.Create<Guid>();
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(IamUserId,useCaseId))
            .ReturnsLazily(() => (false, false, companyId));
        
        var sut = new CompanyDataBusinessLogic(_portalRepositories);
        
        // Act
        async Task Act() => await sut.CreateCompanyAssignedUseCaseDetailsAsync(IamUserId, useCaseId).ConfigureAwait(false);

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
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(IamUserId,useCaseId))
            .ReturnsLazily(() => (true, true, companyId));
        
        var sut = new CompanyDataBusinessLogic(_portalRepositories);
        
        // Act
        await sut.RemoveCompanyAssignedUseCaseDetailsAsync(IamUserId, useCaseId).ConfigureAwait(false);

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
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(IamUserId,useCaseId))
            .ReturnsLazily(() => (true, false, companyId));
        
        var sut = new CompanyDataBusinessLogic(_portalRepositories);
        
        // Act
        async Task Act() => await sut.RemoveCompanyAssignedUseCaseDetailsAsync(IamUserId, useCaseId).ConfigureAwait(false);

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
        A.CallTo(() => _companyRepository.GetCompanyStatusAndUseCaseIdAsync(IamUserId,useCaseId))
            .ReturnsLazily(() => (false, true, companyId));
        
        var sut = new CompanyDataBusinessLogic(_portalRepositories);
        
        // Act
        async Task Act() => await sut.RemoveCompanyAssignedUseCaseDetailsAsync(IamUserId, useCaseId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"UseCaseId {useCaseId} is not available");
    }

    #endregion
}
