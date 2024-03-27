/********************************************************************************
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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class RegistrationStatusControllerTest
{
    private readonly IRegistrationStatusBusinessLogic _logic;
    private readonly RegistrationStatusController _controller;

    public RegistrationStatusControllerTest()
    {
        _logic = A.Fake<IRegistrationStatusBusinessLogic>();
        _controller = new RegistrationStatusController(_logic);
        _controller.AddControllerContextWithClaim();
    }

    [Fact]
    public async Task GetCallbackAddress_ReturnsExpectedCallbackUrl()
    {
        //Arrange
        A.CallTo(() => _logic.GetCallbackAddress())
                  .Returns(new OnboardingServiceProviderCallbackResponseData("https://callback-url.com", "https//auth.url", "test"));

        //Act
        var result = await _controller.GetCallbackAddress();

        //Assert
        result.CallbackUrl.Should().Be("https://callback-url.com");
    }

    [Fact]
    public async Task GetCompanyWithAddressAsync_ReturnsExpectedResult()
    {
        //Act
        var result = await _controller.SetCallbackAddress(new OnboardingServiceProviderCallbackRequestData("https://callback-url.com", "https//auth.url", "test", "test123"));

        //Assert
        A.CallTo(() => _logic.SetCallbackAddress(A<OnboardingServiceProviderCallbackRequestData>._)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }
}
