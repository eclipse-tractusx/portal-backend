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
    private readonly CompanyDataBusinessLogic _sut;

    public CompanyDataBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _companyRepository = A.Fake<ICompanyRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
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
        A.CallTo(() => _companyRepository.GetCompanyRoleAndConsentAgreementDetailsAsync(IamUserId))
            .Returns(null!);

        var sut = new CompanyDataBusinessLogic(_portalRepositories);

        // Act
        async Task Act() => await sut.GetCompanyRoleAndConsentAgreementDetailsAsync(IamUserId).ToListAsync().ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"user {IamUserId} is not associated with any company or Incorrect Status");
    }

    #endregion
}