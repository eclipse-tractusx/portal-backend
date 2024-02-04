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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
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
        this._controller = new CompanyDataController(_logic);
    }

    [Fact]
    public async Task GetOwnCompanyDetailsAsync_ReturnsExpected()
    {
        // Arrange
        var companyAddressDetailData = _fixture.Create<CompanyAddressDetailData>();
        A.CallTo(() => _logic.GetCompanyDetailsAsync())
            .Returns(companyAddressDetailData);

        // Act
        var result = await this._controller.GetOwnCompanyDetailsAsync().ConfigureAwait(false);

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
        await this._controller.GetCompanyAssigendUseCaseDetailsAsync().ToListAsync();

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
        var result = await this._controller.CreateCompanyAssignedUseCaseDetailsAsync(useCaseData).ConfigureAwait(false);

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
        var result = await this._controller.CreateCompanyAssignedUseCaseDetailsAsync(useCaseData).ConfigureAwait(false);

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
        var result = await this._controller.RemoveCompanyAssignedUseCaseDetailsAsync(useCaseData).ConfigureAwait(false);

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
        await this._controller.GetCompanyRoleAndConsentAgreementDetailsAsync(languageShortName).ToListAsync().ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.GetCompanyRoleAndConsentAgreementDetailsAsync(languageShortName)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync()
    {
        // Arrange
        var companyRoleConsentDetails = _fixture.CreateMany<CompanyRoleConsentDetails>(2);

        // Act
        var result = await this._controller.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetUseCaseParticipation_WithValidRequest_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _logic.GetUseCaseParticipationAsync(null))
            .Returns(_fixture.CreateMany<UseCaseParticipationData>(5));

        // Act
        var result = await _controller.GetUseCaseParticipation(null).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.GetUseCaseParticipationAsync(null)).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetUseCaseParticipation_WithLanguageExplicitlySet_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _logic.GetUseCaseParticipationAsync("de"))
            .Returns(_fixture.CreateMany<UseCaseParticipationData>(5));

        // Act
        var result = await _controller.GetUseCaseParticipation("de").ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.GetUseCaseParticipationAsync("de")).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetSsiCertificationData_WithValidData_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _logic.GetSsiCertificatesAsync())
            .Returns(_fixture.CreateMany<CertificateParticipationData>(5));

        // Act
        var result = await _controller.GetSsiCertificationData().ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.GetSsiCertificatesAsync()).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task CreateUseCaseParticipation_WithValidRequest_ReturnsExpected()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var data = new UseCaseParticipationCreationData(Guid.NewGuid(), VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, file);

        // Act
        await _controller.CreateUseCaseParticipation(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.CreateUseCaseParticipation(data, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateSsiCertificate_WithValidRequest_ReturnsExpected()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("test content", "test.pdf", MediaTypeId.PDF.MapToMediaType());
        var data = new SsiCertificateCreationData(null, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, file);

        // Act
        await _controller.CreateSsiCertificate(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.CreateSsiCertificate(data, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ApproveCredential_WithValidData_CallsExpected()
    {
        // Arrange
        var credentialId = Guid.NewGuid();

        // Act
        await _controller.ApproveCredential(credentialId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.ApproveCredential(credentialId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RejectCredentialWithValidData_CallsExpected()
    {
        // Arrange
        var credentialId = Guid.NewGuid();

        // Act
        await _controller.RejectCredential(credentialId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.RejectCredential(credentialId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCredentials_CallsExpected()
    {
        // Arrange
        var companySsiDetailStatusId = _fixture.Create<CompanySsiDetailStatusId>();
        var credentialTypeId = _fixture.Create<VerifiedCredentialTypeId>();
        var companyName = _fixture.Create<string>();
        var sorting = _fixture.Create<CompanySsiDetailSorting>();
        var paginationResponse = new Pagination.Response<CredentialDetailData>(new Pagination.Metadata(15, 1, 1, 15), _fixture.CreateMany<CredentialDetailData>(5));
        A.CallTo(() => _logic.GetCredentials(A<int>._, A<int>._, A<CompanySsiDetailStatusId?>._, A<VerifiedCredentialTypeId?>._, A<string?>._, A<CompanySsiDetailSorting?>._))
            .Returns(paginationResponse);

        //Act
        var result = await this._controller.GetCredentials(companySsiDetailStatusId: companySsiDetailStatusId, credentialTypeId: credentialTypeId, companyName: companyName, sorting: sorting).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetCredentials(0, 15, companySsiDetailStatusId, credentialTypeId, companyName, sorting)).MustHaveHappenedOnceExactly();
        Assert.IsType<Pagination.Response<CredentialDetailData>>(result);
        result.Content.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetCredentialTypes()
    {
        // Arrange
        A.CallTo(() => _logic.GetCertificateTypes())
            .Returns(new[] { VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE }.ToAsyncEnumerable());

        //Act
        var result = await this._controller.GetCertificateTypes().ToListAsync().ConfigureAwait(false);

        //Assert
        result.Should().ContainSingle().Which.Should().Be(VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE);
    }
}
