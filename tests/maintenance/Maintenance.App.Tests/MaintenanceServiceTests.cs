/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.ProcessIdentity;
using Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.Services;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Maintenance.App.Tests;

public class MaintenanceServiceTests
{
    private readonly IBatchDeleteService _batchDeleteService;
    private readonly IClearinghouseBusinessLogic _clearinghouseBusinessLogic;
    private readonly MaintenanceService _service;
    private readonly IProcessIdentityDataDetermination _processIdentityDataDetermination;

    public MaintenanceServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _batchDeleteService = A.Fake<IBatchDeleteService>();
        _clearinghouseBusinessLogic = A.Fake<IClearinghouseBusinessLogic>();
        _processIdentityDataDetermination = A.Fake<IProcessIdentityDataDetermination>();

        var serviceProvider = A.Fake<IServiceProvider>();
        A.CallTo(() => serviceProvider.GetService(typeof(IBatchDeleteService))).Returns(_batchDeleteService);
        A.CallTo(() => serviceProvider.GetService(typeof(IClearinghouseBusinessLogic))).Returns(_clearinghouseBusinessLogic);
        A.CallTo(() => serviceProvider.GetService(typeof(IProcessIdentityDataDetermination))).Returns(_processIdentityDataDetermination);
        var serviceScope = A.Fake<IServiceScope>();
        A.CallTo(() => serviceScope.ServiceProvider).Returns(serviceProvider);
        var serviceScopeFactory = A.Fake<IServiceScopeFactory>();
        A.CallTo(() => serviceScopeFactory.CreateScope()).Returns(serviceScope);
        A.CallTo(() => serviceProvider.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactory);

        _service = new MaintenanceService(serviceScopeFactory);
    }

    [Fact]
    public async Task ExecuteAsync_CallsExpectedServices()
    {
        // Act
        await _service.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _processIdentityDataDetermination.GetIdentityData())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _batchDeleteService.CleanupDocuments(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _clearinghouseBusinessLogic.CheckEndClearinghouseProcesses(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
