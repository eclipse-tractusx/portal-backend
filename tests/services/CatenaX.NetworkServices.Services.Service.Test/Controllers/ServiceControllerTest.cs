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

using AutoFixture;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.Services.Service.BusinessLogic;
using CatenaX.NetworkServices.Services.Service.Controllers;
using CatenaX.NetworkServices.Tests.Shared.Extensions;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CatenaX.NetworkServices.Services.Service.Test.Controllers
{
    public class ServiceControllerTest
    {
        private static readonly string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
        private readonly IFixture _fixture;
        private readonly IServiceBusinessLogic _logic;
        private readonly ServicesController _controller;

        public ServiceControllerTest()
        {
            _fixture = new Fixture();
            _logic = A.Fake<IServiceBusinessLogic>();
            this._controller = new ServicesController(_logic);
            _controller.AddControllerContextWithClaim(IamUserId);
        }

        [Fact]
        public async Task CreateServiceOffering_ReturnsExpectedId()
        {
            //Arrange
            var id = new Guid("d90995fe-1241-4b8d-9f5c-f3909acc6383");
            var serviceOfferingData = _fixture.Create<ServiceOfferingData>();
            A.CallTo(() => _logic.CreateServiceOffering(A<ServiceOfferingData>._, IamUserId))
                      .Returns(id);

            //Act
            var result = await this._controller.CreateServiceOffering(serviceOfferingData).ConfigureAwait(false);

            //Assert
            A.CallTo(() => _logic.CreateServiceOffering(serviceOfferingData, IamUserId)).MustHaveHappenedOnceExactly();
            Assert.IsType<CreatedAtRouteResult>(result);
            result.Value.Should().Be(id);
        }

        [Fact]
        public async Task GetAllActiveServicesAsync_ReturnsExpectedId()
        {
            //Arrange
            var paginationResponse = new Pagination.Response<ServiceOverviewData>(new Pagination.Metadata(15, 1, 1, 15), _fixture.CreateMany<ServiceOverviewData>(5));
            A.CallTo(() => _logic.GetAllActiveServicesAsync(0, 15))
                .Returns(paginationResponse);

            //Act
            var result = await this._controller.GetAllActiveServicesAsync().ConfigureAwait(false);

            //Assert
            A.CallTo(() => _logic.GetAllActiveServicesAsync(0, 15)).MustHaveHappenedOnceExactly();
            Assert.IsType<Pagination.Response<ServiceOverviewData>>(result);
            result.Content.Should().HaveCount(5);
        }
        
        [Fact]
        public async Task AddServiceSubscription_ReturnsExpectedId()
        {
            //Arrange
            var offerSubscriptionId = Guid.NewGuid();
            A.CallTo(() => _logic.AddServiceSubscription(A<Guid>._, IamUserId))
                .Returns(offerSubscriptionId);

            //Act
            var serviceId = Guid.NewGuid();
            var result = await this._controller.AddServiceSubscription(serviceId).ConfigureAwait(false);

            //Assert
            A.CallTo(() => _logic.AddServiceSubscription(serviceId, IamUserId)).MustHaveHappenedOnceExactly();
            Assert.IsType<CreatedAtRouteResult>(result);
            result.Value.Should().Be(offerSubscriptionId);
        }
        
        [Fact]
        public async Task GetServiceDetails_ReturnsExpectedId()
        {
            //Arrange
            var serviceId = Guid.NewGuid();
            var serviceDetailData = _fixture.Create<ServiceDetailData>();
            A.CallTo(() => _logic.GetServiceDetailsAsync(serviceId, A<string>._, IamUserId))
                .Returns(serviceDetailData);

            //Act
            var result = await this._controller.GetServiceDetails(serviceId).ConfigureAwait(false);

            //Assert
            A.CallTo(() => _logic.GetServiceDetailsAsync(serviceId, "en", IamUserId)).MustHaveHappenedOnceExactly();
            Assert.IsType<ServiceDetailData>(result);
            result.Should().Be(serviceDetailData);
        }

    }
}