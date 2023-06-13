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

using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class UserControllerTest
{
    private const string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private static readonly Guid CompanyUserId = new("05455d3a-fc86-4f5a-a89a-ba964ead163d");
    private readonly IdentityData _identity = new(IamUserId, CompanyUserId, IdentityTypeId.COMPANY_USER, Guid.NewGuid());
    private readonly IUserBusinessLogic _logic;
    private readonly IUserRolesBusinessLogic _rolesLogic;
    private readonly UserController _controller;
    private readonly Fixture _fixture;

    public UserControllerTest()
    {
        _fixture = new Fixture();
        _logic = A.Fake<IUserBusinessLogic>();
        _rolesLogic = A.Fake<IUserRolesBusinessLogic>();
        var logger = A.Fake<ILogger<UserController>>();
        var uploadBusinessLogic = A.Fake<IUserUploadBusinessLogic>();
        this._controller = new UserController(logger, _logic, uploadBusinessLogic, _rolesLogic);
        _controller.AddControllerContextWithClaim(IamUserId, _identity);
    }

    [Fact]
    public async Task ModifyUserRolesAsync_WithValidData_ReturnsExpectedResult()
    {
        //Arrange
        var appId = new Guid("8d4bfde6-978f-4d82-86ce-8d90d52fbf3f");
        var userRoleInfo = new UserRoleInfo(CompanyUserId, new[] { "Company Admin" });
        A.CallTo(() => _rolesLogic.ModifyUserRoleAsync(A<Guid>._, A<UserRoleInfo>._, A<Guid>._))
                  .Returns(Enumerable.Empty<UserRoleWithId>());

        //Act
        var result = await this._controller.ModifyUserRolesAsync(appId, userRoleInfo).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _rolesLogic.ModifyUserRoleAsync(A<Guid>.That.Matches(x => x == appId), A<UserRoleInfo>.That.Matches(x => x.CompanyUserId == CompanyUserId && x.Roles.Count() == 1), _identity.CompanyId)).MustHaveHappenedOnceExactly();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOwnUserDetails_ReturnsExpectedResult()
    {
        // Arrange
        var data = _fixture.Create<CompanyOwnUserDetails>();
        A.CallTo(() => _logic.GetOwnUserDetails(_identity.UserId))
            .Returns(data);

        // Act
        var result = await this._controller.GetOwnUserDetails().ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.GetOwnUserDetails(_identity.UserId)).MustHaveHappenedOnceExactly();
        result.Should().Be(data);
    }
}
