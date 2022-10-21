/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
using Microsoft.Extensions.Logging;
using Xunit;
using System;
using System.Threading.Tasks;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.Tests;

public class AppReleaseProcessControllerTest
{
    private static readonly string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private readonly IFixture _fixture;
    private readonly AppReleaseProcessController controller;
    private readonly IAppReleaseBusinessLogic appReleaseBusineesLogicFake;
    private readonly ILogger<AppReleaseProcessController> appReleaseLoggerFake;
    public AppReleaseProcessControllerTest()
    {
        _fixture = new Fixture();
        appReleaseBusineesLogicFake = A.Fake<IAppReleaseBusinessLogic>();
        appReleaseLoggerFake = A.Fake<ILogger<AppReleaseProcessController>>();
        this.controller = new AppReleaseProcessController(appReleaseBusineesLogicFake);
        controller.AddControllerContextWithClaim(IamUserId);
    }

    [Fact]
    public async Task AddAppUserRole_AndUserRoleDescriptionWith201StatusCode()
    {
        //Arrange
        Guid appId = new Guid("5cf74ef8-e0b7-4984-a872-474828beb5d2");
        Guid userId = new Guid("7eab8e16-8298-4b41-953b-515745423658");
        var appUserRoles = _fixture.CreateMany<AppUserRole>(3);
        var appRoleData = _fixture.CreateMany<AppRoleData>(3);
        A.CallTo(() => appReleaseBusineesLogicFake.AddAppUserRoleAsync(appId, appUserRoles,userId.ToString()))
            .Returns(appRoleData);

        //Act
        var result =await this.controller.AddAppUserRole(appId, appUserRoles).ConfigureAwait(false);
        foreach (var item in result)
        {
            //Assert
            A.CallTo(() => appReleaseBusineesLogicFake.AddAppUserRoleAsync(appId, appUserRoles,userId.ToString())).MustHaveHappenedOnceExactly();
            Assert.NotNull(item);
            Assert.IsType<AppRoleData>(item);
        }
    }

}
