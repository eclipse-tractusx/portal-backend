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
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class CompanyDataControllerTests
{
    private const string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private readonly IdentityData _identity = new(IamUserId, Guid.NewGuid(), IdentityTypeId.COMPANY_USER, Guid.NewGuid());
    private readonly ICompanyDataBusinessLogic _logic;
    private readonly CompanyDataController _controller;
    private readonly Fixture _fixture;

    public CompanyDataControllerTests()
    {
        _fixture = new Fixture();
        _logic = A.Fake<ICompanyDataBusinessLogic>();
        this._controller = new CompanyDataController(_logic);
        _controller.AddControllerContextWithClaim(IamUserId, _identity);
    }

    [Fact]
    public async Task GetOwnCompanyDetailsAsync_ReturnsExpected()
    {
        // Arrange
        var companyAddressDetailData = _fixture.Create<CompanyAddressDetailData>();
        A.CallTo(() => _logic.GetCompanyDetailsAsync(A<Guid>._))
            .Returns(companyAddressDetailData);

        // Act
        var result = await this._controller.GetOwnCompanyDetailsAsync().ConfigureAwait(false);

        // Assert
        result.Should().BeOfType<CompanyAddressDetailData>();
        A.CallTo(() => _logic.GetCompanyDetailsAsync(_identity.CompanyId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompanyAssigendUseCaseDetailsAsync_ReturnsExpectedResult()
    {
        // Arrange
        var companyRoleConsentDatas = _fixture.CreateMany<CompanyAssignedUseCaseData>(2).ToAsyncEnumerable();
        A.CallTo(() => _logic.GetCompanyAssigendUseCaseDetailsAsync(A<Guid>._))
            .Returns(companyRoleConsentDatas);

        // Act
        await this._controller.GetCompanyAssigendUseCaseDetailsAsync().ToListAsync();

        // Assert
        A.CallTo(() => _logic.GetCompanyAssigendUseCaseDetailsAsync(_identity.CompanyId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyAssignedUseCaseDetailsAsync_NoContent_ReturnsExpectedResult()
    {
        // Arrange
        var useCaseData = _fixture.Create<UseCaseIdDetails>();
        A.CallTo(() => _logic.CreateCompanyAssignedUseCaseDetailsAsync(A<Guid>._, A<Guid>._))
            .Returns(true);

        // Act
        var result = await this._controller.CreateCompanyAssignedUseCaseDetailsAsync(useCaseData).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.CreateCompanyAssignedUseCaseDetailsAsync(_identity.CompanyId, useCaseData.useCaseId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<StatusCodeResult>();
        result.StatusCode.Should().Be((int)HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateCompanyAssignedUseCaseDetailsAsync_AlreadyReported_ReturnsExpectedResult()
    {
        // Arrange
        var useCaseData = _fixture.Create<UseCaseIdDetails>();
        A.CallTo(() => _logic.CreateCompanyAssignedUseCaseDetailsAsync(A<Guid>._, A<Guid>._))
            .Returns(false);

        // Act
        var result = await this._controller.CreateCompanyAssignedUseCaseDetailsAsync(useCaseData).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.CreateCompanyAssignedUseCaseDetailsAsync(_identity.CompanyId, useCaseData.useCaseId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
        result.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveCompanyAssignedUseCaseDetailsAsync_ReturnsExpectedResult()
    {
        // Arrange
        var useCaseData = _fixture.Create<UseCaseIdDetails>();

        // Act
        var result = await this._controller.RemoveCompanyAssignedUseCaseDetailsAsync(useCaseData).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.RemoveCompanyAssignedUseCaseDetailsAsync(_identity.CompanyId, useCaseData.useCaseId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetCompanyRoleAndConsentAgreementDetailsAsync_ReturnsExpectedResult()
    {
        // Arrange
        var languageShortName = "en";
        var companyRoleConsentDatas = _fixture.CreateMany<CompanyRoleConsentViewData>(2).ToAsyncEnumerable();
        A.CallTo(() => _logic.GetCompanyRoleAndConsentAgreementDetailsAsync(A<Guid>._, A<string>._))
            .Returns(companyRoleConsentDatas);

        // Act
        await this._controller.GetCompanyRoleAndConsentAgreementDetailsAsync(languageShortName).ToListAsync().ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.GetCompanyRoleAndConsentAgreementDetailsAsync(_identity.CompanyId, languageShortName)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync()
    {
        // Arrange
        var companyRoleConsentDetails = _fixture.CreateMany<CompanyRoleConsentDetails>(2);

        // Act
        var result = await this._controller.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.CreateCompanyRoleAndConsentAgreementDetailsAsync(new(_identity.UserId, _identity.CompanyId), companyRoleConsentDetails)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetUseCaseParticipation_WithValidRequest_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _logic.GetUseCaseParticipationAsync(_identity.CompanyId, null))
            .Returns(_fixture.CreateMany<UseCaseParticipationData>(5));

        // Act
        var result = await _controller.GetUseCaseParticipation(null).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.GetUseCaseParticipationAsync(_identity.CompanyId, null)).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetUseCaseParticipation_WithLanguageExplicitlySet_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _logic.GetUseCaseParticipationAsync(_identity.CompanyId, "de"))
            .Returns(_fixture.CreateMany<UseCaseParticipationData>(5));

        // Act
        var result = await _controller.GetUseCaseParticipation("de").ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.GetUseCaseParticipationAsync(_identity.CompanyId, "de")).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetSsiCertificationData_WithValidData_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _logic.GetSsiCertificatesAsync(_identity.CompanyId))
            .Returns(_fixture.CreateMany<SsiCertificateData>(5));

        // Act
        var result = await _controller.GetSsiCertificationData().ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.GetSsiCertificatesAsync(_identity.CompanyId)).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }
}
