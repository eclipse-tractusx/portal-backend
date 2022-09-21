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
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Controllers;
using CatenaX.NetworkServices.Tests.Shared;
using FakeItEasy;
using Xunit;

namespace CatenaX.NetworkServices.Administration.Service.Tests.Controllers
{
    public class PartnerNetworkControllerTest
    {
   
        private readonly IPartnerNetworkBusinessLogic _logic;
        private readonly PartnerNetworkController _controller;

       // private readonly IAsyncEnumerable<string> companyBpns;

        public PartnerNetworkControllerTest()
        {
            _logic = A.Fake<IPartnerNetworkBusinessLogic>();
            this._controller = new PartnerNetworkController(_logic);
        }

        [Fact]
        public async Task GetAllMemberCompaniesBPN_Test()
        {
            //Arrange
          
            A.CallTo(() => _logic.GetAllMemberCompaniesBPNAsync());
    
            //Act
            var result = this._controller.GetAllMemberCompaniesBPNAsync();

            //Assert
            await foreach (var item in result)
            {
                A.CallTo(() => _logic.GetAllMemberCompaniesBPNAsync()).MustHaveHappenedOnceExactly();
                Assert.IsType<string>(result);
            }
        }
    }
}