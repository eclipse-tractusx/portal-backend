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

using AutoFixture;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.Tests.Controllers;

public class ServiceChangeControllerTest
{
    private readonly IdentityData _identity = new("4C1A6851-D4E7-4E10-A011-3732CD045E8A", Guid.NewGuid(), IdentityTypeId.COMPANY_USER, Guid.NewGuid());
    private readonly IFixture _fixture;
    private readonly ServiceChangeController _controller;
    private readonly IServiceChangeBusinessLogic _logic;

    public ServiceChangeControllerTest()
    {
        _fixture = new Fixture();
        _logic = A.Fake<IServiceChangeBusinessLogic>();
        this._controller = new ServiceChangeController(_logic);
        _controller.AddControllerContextWithClaim(_identity.UserEntityId, _identity);
    }

    [Fact]
    public async Task DeactivateApp_ReturnsNoContent()
    {
        //Arrange
        var serviceId = _fixture.Create<Guid>();

        //Act
        var result = await this._controller.DeactivateService(serviceId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.DeactivateOfferByServiceIdAsync(serviceId, _identity.CompanyId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }
}
