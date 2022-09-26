/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using System;
using System.Threading.Tasks;
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Controllers;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared.Extensions;
using FakeItEasy;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.Tests.Controllers
{
    public class RegistrationControllerTest
    {
        private static readonly string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
        private readonly IRegistrationBusinessLogic _logic;
        private readonly RegistrationController _controller;

        public RegistrationControllerTest()
        {
            _logic = A.Fake<IRegistrationBusinessLogic>();
            this._controller = new RegistrationController(_logic);
            _controller.AddControllerContextWithClaim(IamUserId);
        }

        [Fact]
        public async Task Test1()
        {
            //Arrange
            var id = new Guid("d90995fe-1241-4b8d-9f5c-f3909acc6383");
            A.CallTo(() => _logic.ApprovePartnerRequest(IamUserId, id))
                      .Returns(true);

            //Act
            var result = await this._controller.ApprovePartnerRequest(id).ConfigureAwait(false);

            //Assert
            A.CallTo(() => _logic.ApprovePartnerRequest(IamUserId, id)).MustHaveHappenedOnceExactly();
            Assert.IsType<bool>(result);
            Assert.True(result);
        }

        [Fact]
        public async Task Test2()
        {
            //Arrange
            A.CallTo(() => _logic.ApprovePartnerRequest(IamUserId, Guid.Empty))
                      .Returns(false);

            //Act
            var result = await this._controller.ApprovePartnerRequest(Guid.Empty).ConfigureAwait(false);

            //Assert
            A.CallTo(() => _logic.ApprovePartnerRequest(IamUserId, Guid.Empty)).MustHaveHappenedOnceExactly();
            Assert.IsType<bool>(result);
            Assert.False(result);
        }
    }
}