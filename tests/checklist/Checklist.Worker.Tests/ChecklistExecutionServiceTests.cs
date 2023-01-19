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

using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Worker.Tests;

public class ChecklistExecutionServiceTests
{
    private readonly IApplicationChecklistRepository _applicationChecklistRepository;

    private readonly IChecklistService _checklistService;
    private readonly IChecklistCreationService _checklistCreationService;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ChecklistExecutionService _service;

    public ChecklistExecutionServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var portalRepositories = A.Fake<IPortalRepositories>();
        _applicationChecklistRepository = A.Fake<IApplicationChecklistRepository>();
        _checklistService = A.Fake<IChecklistService>();
        _checklistCreationService = A.Fake<IChecklistCreationService>();

        A.CallTo(() => portalRepositories.GetInstance<IApplicationChecklistRepository>())
            .Returns(_applicationChecklistRepository);

        _hostApplicationLifetime = fixture.Create<IHostApplicationLifetime>();
        var serviceProvider = fixture.Create<IServiceProvider>();
        A.CallTo(() => serviceProvider.GetService(typeof(IPortalRepositories))).Returns(portalRepositories);
        A.CallTo(() => serviceProvider.GetService(typeof(IChecklistService))).Returns(_checklistService);
        A.CallTo(() => serviceProvider.GetService(typeof(IChecklistCreationService))).Returns(_checklistCreationService);
        var serviceScope = fixture.Create<IServiceScope>();
        A.CallTo(() => serviceScope.ServiceProvider).Returns(serviceProvider);
        var serviceScopeFactory = fixture.Create<IServiceScopeFactory>();
        A.CallTo(() => serviceScopeFactory.CreateScope()).Returns(serviceScope);
        A.CallTo(() => serviceProvider.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactory);

        _service = new ChecklistExecutionService(_hostApplicationLifetime, serviceScopeFactory, fixture.Create<ILogger<ChecklistExecutionService>>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNoPendingItems_NoServiceCall()
    {
        // Arrange
        A.CallTo(() => _applicationChecklistRepository.GetChecklistDataOrderedByApplicationId())
            .Returns(new List<ValueTuple<Guid, ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>>().ToAsyncEnumerable());

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _checklistService.ProcessChecklist(A<Guid>._,
            A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._,
            CancellationToken.None)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WithException_LogsError()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var list = new List<ValueTuple<Guid, ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>>
        {
            new(applicationId, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
            new(applicationId, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.IN_PROGRESS),
            new(applicationId, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO),
            new(applicationId, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
            new(applicationId, ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO),
        };
        A.CallTo(() => _applicationChecklistRepository.GetChecklistDataOrderedByApplicationId())
            .Returns(list.ToAsyncEnumerable());
        A.CallTo(() => _checklistService.ProcessChecklist(A<Guid>._, A<IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)>>._, A<CancellationToken>._))
            .Throws(() => new Exception("Only a test"));

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        Environment.ExitCode.Should().Be(1);
        A.CallTo(() => _hostApplicationLifetime.StopApplication()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_With5PendingApplications_CallsServiceExactly5Times()
    {
        // Arrange
        var application1 = Guid.NewGuid();
        var application2 = Guid.NewGuid();
        var application3 = Guid.NewGuid();
        var application4 = Guid.NewGuid();
        var application5 = Guid.NewGuid();
        var list = new List<ValueTuple<Guid, ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>>
            {
                new(application1, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
                new(application1, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.IN_PROGRESS),
                new(application1, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO),
                new(application1, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
                new(application1, ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO),
                new(application2, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
                new(application2, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.IN_PROGRESS),
                new(application2, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO),
                new(application2, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
                new(application2, ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO),
                new(application3, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
                new(application3, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.IN_PROGRESS),
                new(application3, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO),
                new(application3, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
                new(application3, ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO),
                new(application4, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
                new(application4, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.IN_PROGRESS),
                new(application4, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO),
                new(application4, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
                new(application4, ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO),
                new(application5, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
                new(application5, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.IN_PROGRESS),
                new(application5, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO),
                new(application5, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
                new(application5, ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO),
            };
        A.CallTo(() => _applicationChecklistRepository.GetChecklistDataOrderedByApplicationId())
            .Returns(list.ToAsyncEnumerable());

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _checklistService.ProcessChecklist(A<Guid>._,
            A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._,
            A<CancellationToken>._)).MustHaveHappened(5, Times.Exactly);
    }
    
    [Fact]
    public async Task ExecuteAsync_WithMissingType_CreatesTypeAndExecutes()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var list = new List<ValueTuple<Guid, ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>>
        {
            new(applicationId, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
            new(applicationId, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE),
            new(applicationId, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
            new(applicationId, ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO),
        };
        A.CallTo(() => _applicationChecklistRepository.GetChecklistDataOrderedByApplicationId())
            .Returns(list.ToAsyncEnumerable());

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _checklistCreationService.CreateMissingChecklistItems(applicationId, A<IEnumerable<ApplicationChecklistEntryTypeId>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _checklistService.ProcessChecklist(A<Guid>._, A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}