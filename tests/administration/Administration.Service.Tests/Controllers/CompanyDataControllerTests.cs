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
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class CompanyDataControllerTests
{
    private readonly ICompanyDataBusinessLogic _logic;
    private readonly CompanyDataController _controller;
    private readonly Fixture _fixture;

    public CompanyDataControllerTests()
    {
        _fixture = new Fixture();
        _logic = A.Fake<ICompanyDataBusinessLogic>();
        _controller = new CompanyDataController(_logic);
        var identity = A.Fake<IIdentityData>();
        A.CallTo(() => identity.IdentityId).Returns(Guid.NewGuid());
        _controller.AddControllerContextWithClaimAndBearer("ac-token", identity);
    }

    [Fact]
    public async Task GetOwnCompanyDetailsAsync_ReturnsExpected()
    {
        // Arrange
        var companyAddressDetailData = _fixture.Create<CompanyAddressDetailData>();
        A.CallTo(() => _logic.GetCompanyDetailsAsync())
            .Returns(companyAddressDetailData);

        // Act
        var result = await _controller.GetOwnCompanyDetailsAsync();

        // Assert
        result.Should().BeOfType<CompanyAddressDetailData>();
        A.CallTo(() => _logic.GetCompanyDetailsAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompanyAssigendUseCaseDetailsAsync_ReturnsExpectedResult()
    {
        // Arrange
        var companyRoleConsentDatas = _fixture.CreateMany<CompanyAssignedUseCaseData>(2).ToAsyncEnumerable();
        A.CallTo(() => _logic.GetCompanyAssigendUseCaseDetailsAsync())
            .Returns(companyRoleConsentDatas);

        // Act
        await _controller.GetCompanyAssigendUseCaseDetailsAsync().ToListAsync();

        // Assert
        A.CallTo(() => _logic.GetCompanyAssigendUseCaseDetailsAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyAssignedUseCaseDetailsAsync_NoContent_ReturnsExpectedResult()
    {
        // Arrange
        var useCaseData = _fixture.Create<UseCaseIdDetails>();
        A.CallTo(() => _logic.CreateCompanyAssignedUseCaseDetailsAsync(A<Guid>._))
            .Returns(true);

        // Act
        var result = await _controller.CreateCompanyAssignedUseCaseDetailsAsync(useCaseData);

        // Assert
        A.CallTo(() => _logic.CreateCompanyAssignedUseCaseDetailsAsync(useCaseData.useCaseId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<StatusCodeResult>();
        result.StatusCode.Should().Be((int)HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateCompanyAssignedUseCaseDetailsAsync_AlreadyReported_ReturnsExpectedResult()
    {
        // Arrange
        var useCaseData = _fixture.Create<UseCaseIdDetails>();
        A.CallTo(() => _logic.CreateCompanyAssignedUseCaseDetailsAsync(A<Guid>._))
            .Returns(false);

        // Act
        var result = await _controller.CreateCompanyAssignedUseCaseDetailsAsync(useCaseData);

        // Assert
        A.CallTo(() => _logic.CreateCompanyAssignedUseCaseDetailsAsync(useCaseData.useCaseId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
        result.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveCompanyAssignedUseCaseDetailsAsync_ReturnsExpectedResult()
    {
        // Arrange
        var useCaseData = _fixture.Create<UseCaseIdDetails>();

        // Act
        var result = await _controller.RemoveCompanyAssignedUseCaseDetailsAsync(useCaseData);

        // Assert
        A.CallTo(() => _logic.RemoveCompanyAssignedUseCaseDetailsAsync(useCaseData.useCaseId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetCompanyRoleAndConsentAgreementDetailsAsync_ReturnsExpectedResult()
    {
        // Arrange
        var languageShortName = "en";
        var companyRoleConsentDatas = _fixture.CreateMany<CompanyRoleConsentViewData>(2).ToAsyncEnumerable();
        A.CallTo(() => _logic.GetCompanyRoleAndConsentAgreementDetailsAsync(A<string>._))
            .Returns(companyRoleConsentDatas);

        // Act
        await _controller.GetCompanyRoleAndConsentAgreementDetailsAsync(languageShortName).ToListAsync();

        // Assert
        A.CallTo(() => _logic.GetCompanyRoleAndConsentAgreementDetailsAsync(languageShortName)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync()
    {
        // Arrange
        var companyRoleConsentDetails = _fixture.CreateMany<CompanyRoleConsentDetails>(2);

        // Act
        var result = await _controller.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails);

        // Assert
        A.CallTo(() => _logic.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task CreateUseCaseParticipation_WithValidRequest_ReturnsExpected()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var data = new UseCaseParticipationCreationData(Guid.NewGuid(), "TRACEABILITY_FRAMEWORK", file);

        // Act
        await _controller.CreateUseCaseParticipation(data, CancellationToken.None);

        // Assert
        A.CallTo(() => _logic.CreateUseCaseParticipation(data, "ac-token", A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetDimServiceUrls()
    {
        // Arrange
        var decentralIdentityManagementAuthUrl = "https://example.com/auth";
        var decentralIdentityManagementServiceUrl = "https://example.com/service";
        A.CallTo(() => _logic.GetDimServiceUrls())
            .Returns(new DimUrlsResponse("did:web:issuer:test:12345", "BPNL12345678", "did:web:holder:test:123234", "https://example.org/bdrs", decentralIdentityManagementAuthUrl, decentralIdentityManagementServiceUrl));

        //Act
        var result = await _controller.GetDimServiceUrls();

        //Assert
        result.DecentralIdentityManagementAuthUrl.Should().Be(decentralIdentityManagementAuthUrl);
        result.DecentralIdentityManagementServiceUrl.Should().Be(decentralIdentityManagementServiceUrl);
    }

    [Fact]
    public async Task GetCompaniesWithMissingSdDocument()
    {
        // Arrange
        var paginationResponse = new Pagination.Response<CompanyMissingSdDocumentData>(new Pagination.Metadata(15, 1, 1, 15), _fixture.CreateMany<CompanyMissingSdDocumentData>(5));
        A.CallTo(() => _logic.GetCompaniesWithMissingSdDocument(A<int>._, A<int>._))
            .Returns(paginationResponse);

        //Act
        var result = await _controller.GetCompaniesWithMissingSdDocument();

        //Assert
        result.Content.Should().HaveCount(5);
    }

    [Fact]
    public async Task TriggerSelfDescriptionProcess_CallsExpected()
    {
        // Act
        var result = await _controller.TriggerSelfDescriptionProcess();

        // Assert
        A.CallTo(() => _logic.TriggerSelfDescriptionCreation()).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }
}
