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
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class CompanyDataControllerTests
{
    private const string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private readonly ICompanyDataBusinessLogic _logic;
    private readonly CompanyDataController _controller;
    private readonly Fixture _fixture;

    public CompanyDataControllerTests()
    {
        _fixture = new Fixture();
        _logic = A.Fake<ICompanyDataBusinessLogic>();
        this._controller = new CompanyDataController(_logic);
        _controller.AddControllerContextWithClaim(IamUserId);
    }

    [Fact]
    public async Task GetOwnCompanyDetailsAsync_ReturnsExpected()
    {
        // Arrange
        var companyAddressDetailData = _fixture.Create<CompanyAddressDetailData>();
        A.CallTo(()=> _logic.GetOwnCompanyDetailsAsync(IamUserId))
            .ReturnsLazily(() => companyAddressDetailData);
        
        // Act
         var result = await this._controller.GetOwnCompanyDetailsAsync().ConfigureAwait(false);

        // Assert
        result.Should().BeOfType<CompanyAddressDetailData>();
    }

    [Fact]
    public async Task GetCompanyAssigendUseCaseDetailsAsync_ReturnsExpectedResult()
    {
        // Arrange
        var companyRoleConsentDatas = _fixture.CreateMany<CompanyAssignedUseCaseData>(2).ToAsyncEnumerable();
        A.CallTo(() => _logic.GetCompanyAssigendUseCaseDetailsAsync(IamUserId))
            .ReturnsLazily(() => companyRoleConsentDatas);
        
        // Act
        await this._controller.GetCompanyAssigendUseCaseDetailsAsync().ToListAsync();

        // Assert
        A.CallTo(() => _logic.GetCompanyAssigendUseCaseDetailsAsync(IamUserId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateCompanyAssignedUseCaseDetailsAsync_NoContent_ReturnsExpectedResult()
    {
        // Arrange
        var useCaseData = _fixture.Create<UseCaseIdDetails>();
        A.CallTo(() => _logic.CreateCompanyAssignedUseCaseDetailsAsync(IamUserId, useCaseData.useCaseId))
            .ReturnsLazily(() => System.Net.HttpStatusCode.NoContent);
        
        // Act
        var result = await this._controller.CreateCompanyAssignedUseCaseDetailsAsync(useCaseData).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.CreateCompanyAssignedUseCaseDetailsAsync(IamUserId, useCaseData.useCaseId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<StatusCodeResult>();
        result.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task CreateCompanyAssignedUseCaseDetailsAsync_AlreadyReported_ReturnsExpectedResult()
    {
        // Arrange
        var useCaseData = _fixture.Create<UseCaseIdDetails>();
        A.CallTo(() => _logic.CreateCompanyAssignedUseCaseDetailsAsync(IamUserId, useCaseData.useCaseId))
            .ReturnsLazily(() => System.Net.HttpStatusCode.AlreadyReported);
        
        // Act
        var result = await this._controller.CreateCompanyAssignedUseCaseDetailsAsync(useCaseData).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.CreateCompanyAssignedUseCaseDetailsAsync(IamUserId, useCaseData.useCaseId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<StatusCodeResult>();
        result.StatusCode.Should().Be(208);
    }

    [Fact]
    public async Task RemoveCompanyAssignedUseCaseDetailsAsync_ReturnsExpectedResult()
    {
        // Arrange
        var useCaseData = _fixture.Create<UseCaseIdDetails>();
        A.CallTo(() => _logic.RemoveCompanyAssignedUseCaseDetailsAsync(IamUserId, useCaseData.useCaseId))
            .ReturnsLazily(() => Task.CompletedTask);
        
        // Act
        var result = await this._controller.RemoveCompanyAssignedUseCaseDetailsAsync(useCaseData).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.RemoveCompanyAssignedUseCaseDetailsAsync(IamUserId, useCaseData.useCaseId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }
}
