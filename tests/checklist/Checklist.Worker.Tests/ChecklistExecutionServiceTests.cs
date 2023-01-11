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

    private static readonly Guid IdWithoutBpn = new("0a9bd7b1-e692-483e-8128-dbf52759c7a5");
    private static readonly Guid IdWithBpn = new("c244f79a-7faf-4c59-bb85-fbfdf72ce46f");
    private static readonly Guid IdWithFailingCustodian = new("bda6d1b5-042e-493a-894c-11f3a89c12b1");
    private static readonly Guid NotExistingApplicationId = new("1942e8d3-b545-4fbc-842c-01a694f84390");
    private static readonly Guid ActiveApplicationCompanyId = new("66c765dd-872d-46e0-aac1-f79330b55406");
    private static readonly string IamUserId = new Guid("4C1A6851-D4E7-4E10-A011-3732CD045E8A").ToString();
    private static readonly Guid CompanyId = new("95c4339e-e087-4cd2-a5b8-44d385e64630");
    private const string ValidBpn = "BPNL123698762345";
    private const string ValidCompanyName = "valid company";
    private const string AlreadyTakenBpn = "BPNL123698762666";

    private readonly IApplicationChecklistRepository _applicationChecklistRepository;

    private readonly IChecklistService _checklistService;
    private ChecklistExecutionService _service;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public ChecklistExecutionServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var portalRepositories = A.Fake<IPortalRepositories>();
        _applicationChecklistRepository = A.Fake<IApplicationChecklistRepository>();
        _checklistService = A.Fake<IChecklistService>();

        A.CallTo(() => portalRepositories.GetInstance<IApplicationChecklistRepository>())
            .Returns(_applicationChecklistRepository);

        _hostApplicationLifetime = fixture.Create<IHostApplicationLifetime>();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"WorkerBatchSize", "5"}
            }.ToImmutableDictionary())
            .Build();
        var serviceProvider = fixture.Create<IServiceProvider>();
        A.CallTo(() => serviceProvider.GetService(typeof(IPortalRepositories))).Returns(portalRepositories);
        A.CallTo(() => serviceProvider.GetService(typeof(IChecklistService))).Returns(_checklistService);
        var serviceScope = fixture.Create<IServiceScope>();
        A.CallTo(() => serviceScope.ServiceProvider).Returns(serviceProvider);
        var serviceScopeFactory = fixture.Create<IServiceScopeFactory>();
        A.CallTo(() => serviceScopeFactory.CreateScope()).Returns(serviceScope);

        _service = new ChecklistExecutionService(_hostApplicationLifetime, serviceScopeFactory,
            fixture.Create<ILogger<ChecklistExecutionService>>(), config);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoPendingItems_NoServiceCall()
    {
        // Arrange
        A.CallTo(() => _applicationChecklistRepository.GetChecklistDataGroupedByApplicationId(5))
            .Returns(new List<ValueTuple<Guid, IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>>().ToAsyncEnumerable());

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _checklistService.ProcessChecklist(A<Guid>._,
            A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._,
            CancellationToken.None)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_With5PendingApplications_CallsServiceExactly5Times()
    {
        // Arrange
        var checklist = new[]
        {
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.IN_PROGRESS),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO)
        };
        var list = new List<ValueTuple<Guid, IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>>
            {
                new(Guid.NewGuid(), checklist),
                new(Guid.NewGuid(), checklist),
                new(Guid.NewGuid(), checklist),
                new(Guid.NewGuid(), checklist),
                new(Guid.NewGuid(), checklist),
            };
        A.CallTo(() => _applicationChecklistRepository.GetChecklistDataGroupedByApplicationId(5))
            .Returns(list.ToAsyncEnumerable());

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _checklistService.ProcessChecklist(A<Guid>._,
            A<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>._,
            A<CancellationToken>._)).MustHaveHappened(5, Times.Exactly);
    }
    
    [Fact]
    public async Task ExecuteAsync_WithException_LogsError()
    {
        // Arrange
        var checklist = new[]
        {
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.IN_PROGRESS),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(
                ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO)
        };
        var list = new List<ValueTuple<Guid, IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>>>
        {
            new(Guid.NewGuid(), checklist),
        };
        A.CallTo(() => _applicationChecklistRepository.GetChecklistDataGroupedByApplicationId(5))
            .Returns(list.ToAsyncEnumerable());
        A.CallTo(() => _checklistService.ProcessChecklist(A<Guid>._, A<IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)>>._, A<CancellationToken>._))
            .Throws(() => new Exception("Only a test"));

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        Environment.ExitCode.Should().Be(1);
        A.CallTo(() => _hostApplicationLifetime.StopApplication()).MustHaveHappenedOnceExactly();
    }
}