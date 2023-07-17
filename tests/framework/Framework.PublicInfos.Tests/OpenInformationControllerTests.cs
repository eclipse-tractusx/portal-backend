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

using Org.Eclipse.TractusX.Portal.Backend.Framework.PublicInfos;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class OpenInformationControllerTests
{
    private const string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private readonly IdentityData _identity = new(IamUserId, Guid.NewGuid(), IdentityTypeId.COMPANY_USER, Guid.NewGuid());
    private readonly IPublicInformationBusinessLogic _logic;
    private readonly OpenInformationController _controller;
    private readonly Fixture _fixture;

    public OpenInformationControllerTests()
    {
        _fixture = new Fixture();
        _logic = A.Fake<IPublicInformationBusinessLogic>();
        this._controller = new OpenInformationController(_logic);
        _controller.AddControllerContextWithClaim(IamUserId, _identity);
    }

    [Fact]
    public async Task GetOpenUrls_ReturnsExpected()
    {
        // Arrange
        var urlInformation = _fixture.CreateMany<UrlInformation>(5);
        A.CallTo(() => _logic.GetPublicUrls(_identity.CompanyId))
            .Returns(urlInformation);

        // Act
        var result = await this._controller.GetOpenUrls().ConfigureAwait(false);

        // Assert
        result.Should().BeOfType<IEnumerable<UrlInformation>>().And.HaveCount(urlInformation.Count());
        A.CallTo(() => _logic.GetPublicUrls(_identity.CompanyId)).MustHaveHappenedOnceExactly();
    }
}
